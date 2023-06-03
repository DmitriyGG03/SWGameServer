using Microsoft.EntityFrameworkCore;
using Server.Common.Constants;
using Server.Domain;
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

            await _context.Sessions.AddAsync(session, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            List<Hero> heroes = await CreateHeroesBasedOnLobbyInfosAndAddThemToDbAsync(lobbyInfos, session, sessionMap, cancellationToken);

            await GenerateHeroPlanetRelationsAsync(heroes, sessionMap.Planets, sessionMap.Connections);

            if (session.HeroTurnId == Guid.Empty)
            {
                session.HeroTurnId = heroes.First().HeroId;
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
            if (session is null)
            {
                return new ServiceResult<Dictionary<Guid, Guid>>(ErrorMessages.Session.NotFound);
            }

            if (session.Heroes is null)
                throw new InvalidOperationException("Can not get user id's, cause heroes in session is null");

            return new ServiceResult<Dictionary<Guid, Guid>>(session.Heroes
                .Select(x => new {x.UserId, x.HeroId})
                .ToDictionary(t => t.UserId, t => t.HeroId));
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
            foreach (var item in lobbyInfos)
            {
                var hero = new Hero
                {
                    HeroId = Guid.NewGuid(),
                    Name = item.User.Username,
                    ColorStatus = item.ColorStatus,
                    ColonizationShipLimit = 1,
                    AvailableColonizationShips = 1,
                    ResearchShipLimit = 1,
                    AvailableResearchShips = 1,
                    Resourses = 1000,
                    AvailableSoldiers = int.MinValue,
                    SoldiersLimit = int.MinValue,
                    SessionId = session.Id,
                    Session = session,
                    UserId = item.UserId
                };
                
                // TODO: change home planet receiving
                var homePlanet = sessionMap.Planets[Random.Shared.Next(0, sessionMap.Planets.Count)];
                hero.HomePlanetId = homePlanet.Id;
                hero.HomePlanet = homePlanet;
                homePlanet.OwnerId = hero.HeroId;
                homePlanet.IsCapital = true;
                
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
            }

            return heroes;
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
            return connections
                .Where(x => x.FromPlanetId == planetId)
                .Select(x => x.To)
                .ToList();
        }

    }
}