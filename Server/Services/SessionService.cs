using System.Drawing;
using Microsoft.EntityFrameworkCore;
using Server.Common.Constants;
using Server.Domain;
using Server.Domain.Exceptions;
using Server.Services.Abstract;
using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Services
{
    public class SessionService : ISessionService
    {
        private readonly GameDbContext _context;
        private readonly IMapGenerator _mapGenerator;
        private readonly IHeroService _heroService;
        private readonly ILogger<SessionService> _logger;

        public SessionService(GameDbContext context, IMapGenerator mapGenerator, IHeroService heroService,
            ILogger<SessionService> logger)
        {
            _context = context;
            _mapGenerator = mapGenerator;
            _heroService = heroService;
            _logger = logger;
        }

        public async Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .Include(x => x.Heroes)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);

            return session;
        }
        
        public async Task<ServiceResult<Session>> CreateAsync(Guid lobbyId, CancellationToken cancellationToken)
        {
            var lobby = await GetLobbyByIdWithLobbyInfosAsync(lobbyId, cancellationToken);
            if (lobby == null)
                return new ServiceResult<Session>(ErrorMessages.Lobby.NotFound);

            var lobbyInfos = lobby.LobbyInfos;
            if (lobbyInfos.Any(x => x.Ready == false))
                return new ServiceResult<Session>(ErrorMessages.Lobby.UsersNotReady);

            var sessionMap = GenerateSessionMap();
            var session = CreateSessionAndCalculateTurnTimeLimit(lobby, sessionMap);

            lobby.Visible = false;
            await _context.Sessions.AddAsync(session, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            List<Hero> heroes = await CreateHeroesBasedOnLobbyInfosAndAddThemToDbAsync(lobbyInfos, session, sessionMap, cancellationToken);

            await GenerateHeroPlanetRelationsAsync(heroes, sessionMap.Planets, sessionMap.Connections);

            if (session.HeroTurnId == Guid.Empty)
            {
                session.HeroTurnId = heroes.OrderBy(x => x.Name).First().HeroId;
                await UpdateSessionAsync(session, cancellationToken);
            }

            return new ServiceResult<Session>(session);
        }
        
        public async Task<int> UpdateSessionAsync(Session designation, CancellationToken cancellationToken)
        {
            var session = await _context.Sessions.FirstOrDefaultAsync(x => x.Id == designation.Id, cancellationToken);
            if (session is null)
            {
                throw new InvalidOperationException("Given session does not exist, you can not update it");
            }

            session.Name = designation.Name;
            session.HeroNumber = designation.HeroNumber;
            session.HeroTurnId = designation.HeroTurnId;
            session.TurnTimeLimit = designation.TurnTimeLimit;

            var result = await _context.SaveChangesAsync(cancellationToken);
            return result;
        }

        public async Task<bool> IsHeroTurn(Guid sessionId, Guid heroId,
            CancellationToken cancellationToken)
        {
            var session = await GetByIdAsync(sessionId, cancellationToken);
            if (session is null)
            {
                _logger.LogWarning($"We can not find session by id! Session id: {sessionId}");
                return false;
            }
            if (session.HeroTurnId != heroId)
                return false;

            return true;
        }

        public async Task<ServiceResult<Dictionary<Guid, Guid>>> GetUserIdWithHeroIdBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .Include(x => x.Heroes)
                .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);
            if (session.Heroes is null)
                throw new InvalidOperationException("Can not get user id's, cause heroes in session is null");

            return new ServiceResult<Dictionary<Guid, Guid>>(session.Heroes
                .Select(x => new {x.UserId, x.HeroId})
                .ToDictionary(t => t.UserId, t => t.HeroId));
        }
        
        public Dictionary<Guid, Guid> GetUserIdWithHeroIdBySession(Session session)
        {
            if (session.Heroes is null)
                throw new InvalidOperationException("Can not get user id's, cause heroes in session is null");

            return new Dictionary<Guid, Guid>(session.Heroes
                .Select(x => new {x.UserId, x.HeroId})
                .ToDictionary(t => t.UserId, t => t.HeroId));
        }

        public async Task<ServiceResult<Hero>> ExitFromSessionAsync(Guid sessionId, Guid heroId, CancellationToken cancellationToken)
        {
            var session = await GetByIdAsync(sessionId, cancellationToken);
            if (session is null)
                return new ServiceResult<Hero>(ErrorMessages.Session.NotFound);

            if (session.Heroes is null)
                throw new InvalidOperationException("Can not exit from session, cause heroes in session is null");
            
            var hero = session.Heroes.FirstOrDefault(x => x.HeroId == heroId);
            if (hero is null)
                return new ServiceResult<Hero>(ErrorMessages.Hero.NotFound);
            
            session.Heroes.Remove(hero);
            await _context.SaveChangesAsync(cancellationToken);

            if (session.Heroes.Count == 1)
            {
                // throw new GameEndedException(session.Heroes.First());
            }
            
            return new ServiceResult<Hero>(hero);
        }

        private static Session CreateSessionAndCalculateTurnTimeLimit(Lobby lobby, SessionMap sessionMap)
        {
            var session = new Session
            {
                Id = Guid.NewGuid(),
                Name = lobby.LobbyName,
                Heroes = new List<Hero>(),
                SessionMapId = sessionMap.Id,
                SessionMap = sessionMap,
                HeroNumber = 0,
                HeroTurnId = Guid.Empty
            };
            session.TurnTimeLimit = session.CalculateTurnTimeLimit(sessionMap.Planets.Count);
            return session;
        }
        
        private async Task<Lobby?> GetLobbyByIdWithLobbyInfosAsync(Guid lobbyId, CancellationToken cancellationToken)
        {
            var lobby = await _context.Lobbies
                .Include(x => x.LobbyInfos)!
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(l => l.Id == lobbyId, cancellationToken);
            return lobby;
        }
        
        private SessionMap GenerateSessionMap()
        {
            var defaultOptions = new MapGenerationOptions(800, 600, 50, 25, 60);
            var sessionMap = _mapGenerator.GenerateMap(defaultOptions);
            return sessionMap;
        }

        private async Task<List<Hero>> CreateHeroesBasedOnLobbyInfosAndAddThemToDbAsync(ICollection<LobbyInfo> lobbyInfos, Session session, SessionMap sessionMap, CancellationToken cancellationToken)
        {
            var heroes = new List<Hero>();
            int counter = 0;
            var homePlanets = new List<Planet>();   
            foreach (var item in lobbyInfos)
            {
                var hero = new Hero
                {
                    HeroId = Guid.NewGuid(),
                    Name = item.Name,
                    ColorStatus = item.ColorStatus,
                    ColonizationShipLimit = 1,
                    AvailableColonizationShips = 1,
                    ResearchShipLimit = 1,
                    AvailableResearchShips = 1,
                    Resourses = 100,
                    AvailableSoldiers = int.MinValue,
                    SoldiersLimit = int.MinValue,
                    SessionId = session.Id,
                    Session = session,
                    UserId = item.UserId
                };
                
                
                var sorted = sessionMap.Planets.OrderBy(x => x.X).ToList();
                sessionMap.Planets = sorted;
                var randomIndex = CalculateRandomIndex(sessionMap, counter);

                var homePlanet = sessionMap.Planets[randomIndex];
                homePlanets.Add(homePlanet);
                if (randomIndex == 1)
                {
                    homePlanet.Y = homePlanets.First().Y;
                    sessionMap.Connections.Add(new Edge(homePlanet, homePlanets.First()));
                }
                
                hero.HomePlanetId = homePlanet.Id;
                hero.HomePlanet = homePlanet;
                homePlanet.OwnerId = hero.HeroId;
                homePlanet.IsCapital = true;
                homePlanet.ResourceCount = 10;
                homePlanet.ResourceType = ResourceType.OnlyResources;
                homePlanet.ColorStatus = hero.ColorStatus;
                
                homePlanets.Add(homePlanet);
                sessionMap.Connections.Add(new Edge(homePlanet, sessionMap.Planets.First()));
                
                hero.SetSoldiersLimitBasedOnPlanetSize(homePlanet.Size);
                hero.InitializeAvailableSoldiers();

                var addingResult = await _heroService.Create(item.UserId, hero, cancellationToken);

                if (addingResult.Success == false)
                {
                    _logger.LogError(addingResult.ErrorMessage);
                    throw new InvalidOperationException("Can not add hero via hero service: " +
                                                        addingResult.ErrorMessage);
                }
                else
                {
                    heroes.Add(hero);
                }

                counter += 1;
            }

            return heroes;
        }

        private static Planet GetPlanet(int counter)
        {
            if(counter == 0)
            {
                return new Planet(new PointF(10, 10),
                    15,
                    "1",
                    PlanetType.Earth,
                    ResourceType.OnlyResources,
                    21,
                    Planet.CalculateHealthLimit(15));
            }
            else
            {
                return new Planet(new PointF(10, 30),
                    15,
                    "1",
                    PlanetType.Earth,
                    ResourceType.OnlyResources,
                    21,
                    Planet.CalculateHealthLimit(15));
            }
        }

        private static int CalculateRandomIndex(SessionMap sessionMap, int counter)
        {
            int randomIndex = 0;
            
            if (counter == 0)
            {
                return 0;
                randomIndex = Random.Shared.Next(0, 10);
            }
            else if (counter == 1)
            {
                return 1;
                randomIndex = Random.Shared.Next(10, 20);
            }
            else if (counter == 2)
            {
                randomIndex = Random.Shared.Next(20, 30);
            }
            else
            {
                randomIndex = Random.Shared.Next(30, sessionMap.Planets.Count);
            }

            return randomIndex;
        }

        private async Task GenerateHeroPlanetRelationsAsync(List<Hero> heroes, List<Planet> planets,
            List<Edge> connections)
        {
            if (heroes.Any(hero => hero.HomePlanet is null))
                throw new ArgumentException("We can not generate hero planets relation if at least one hero does not have home planet");
            if (connections.Any(x => x.From is null || x.To is null))
                throw new ArgumentException("Connections must have planets");

            var relations = new List<HeroPlanetRelation>();
            foreach (var hero in heroes)
            {
                var knownRelationsPerHero = GenerateRelations(hero.HeroId, hero.HomePlanet, planets, connections);
                knownRelationsPerHero = knownRelationsPerHero.DistinctBy(x => x.PlanetId).ToList();
                relations.AddRange(knownRelationsPerHero);
            }
            
            await _context.HeroPlanetRelations.AddRangeAsync(relations);
            var updated = await _context.SaveChangesAsync();
            if (updated == 0)
            {
                var exception = new InvalidDataException("Database records has not been updated");
                _logger.LogError(exception, "Can not Create planet relations");
                throw exception;
            }
        }

        private List<HeroPlanetRelation> GenerateRelations(Guid heroId, Planet homePlanet, List<Planet> planets, List<Edge> connections)
        {
            var relations = new List<HeroPlanetRelation>();
            relations.Add(new HeroPlanetRelation
            {
                HeroId = heroId,
                PlanetId = homePlanet.Id,
                FortificationLevel = Fortification.None,
                Status = PlanetStatus.Colonized,
                IterationsLeftToTheNextStatus = 1
            });

            List<Planet> neighbors = GetNeighborsPlanet(homePlanet.Id, connections);
            foreach (var planet in neighbors)
            {
                relations.Add(new HeroPlanetRelation
                {
                    HeroId = heroId,
                    PlanetId = planet.Id,
                    FortificationLevel = Fortification.None,
                    Status = PlanetStatus.Known,
                    IterationsLeftToTheNextStatus = 1
                });
            }


            var otherPlanets = new List<Planet>();
            foreach (var planet in planets)
            {
                if(neighbors.Any(x => x.Id == planet.Id) || planet.Id == homePlanet.Id)
                    continue;

                otherPlanets.Add(planet);
            }

            foreach (var planet in otherPlanets)
            {
                relations.Add(new HeroPlanetRelation
                {
                    HeroId = heroId,
                    PlanetId = planet.Id,
                    FortificationLevel = Fortification.None,
                    Status = PlanetStatus.Unknown,
                    IterationsLeftToTheNextStatus = 1
                });
            }

            return relations;
        }

        private List<Planet> GetNeighborsPlanet(Guid planetId, List<Edge> connections)
        {
            var fromPlanets = connections
                .Where(x => x.FromPlanetId == planetId)
                .Select(x => x.To)
                .ToList();
            var toPlanets = connections
                .Where(x => x.ToPlanetId == planetId)
                .Select(x => x.From)
                .ToList();
            
            var planets = new List<Planet>(fromPlanets);   
            planets.AddRange(toPlanets);
            return planets;
        }

    }
}