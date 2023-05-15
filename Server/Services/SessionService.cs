using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Server.Common.Constants;
using Server.Domain;
using Server.Services.Abstract;
using SharedLibrary.Models;
using System.Drawing;

namespace Server.Services
{
    public class SessionService : ISessionService
    {
        private readonly GameDbContext _context;
        private readonly IMapGenerator _mapGenerator;
        private readonly IHeroService _heroService;
        private readonly ILogger<SessionService> _logger;
        public SessionService(GameDbContext context, IMapGenerator mapGenerator,  IHeroService heroService, ILogger<SessionService> logger)
        {
            _context = context;
            _mapGenerator = mapGenerator;
            _heroService = heroService;
            _logger = logger;
        }
        
        /// <summary>
        /// Create a new session based on existed lobby
        /// </summary>
        /// <param name="lobbyId">Lobby id based on which the session will be created</param>
        /// <param name="cancellationToken">Token to cancel operation</param>
        /// <returns>Service result with new created session</returns>
        public async Task<ServiceResult<Session>> CreateAsync(Guid lobbyId, CancellationToken cancellationToken)
        {
            // TODO: session can be created only by owner of lobby 
            var lobby = await _context.Lobbies.Include(x => x.LobbyInfos)
                 .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(l => l.Id == lobbyId);
            if (lobby == null)
                return new ServiceResult<Session>(ErrorMessages.Lobby.NotFound);

            var lobbyInfos = lobby.LobbyInfos;
            if (lobbyInfos.Any(x => x.Ready == false))
                return new ServiceResult<Session>(ErrorMessages.Lobby.UsersNotReady);

            var defaultOptions = new MapGenerationOptions(800, 600, 50, 25, 60);
            var sessionMap = _mapGenerator.GenerateMap(defaultOptions);
            
            var session = new Session
            {
                Id = Guid.NewGuid(),
                Name = lobby.LobbyName,
                Heroes = new List<Hero>(),
                SessionMapId = sessionMap.Id,
                SessionMap = sessionMap,
                TurnNumber = 0,
                HeroTurnId = Guid.Empty
            };
            session.TurnTimeLimit = session.CalculateTurnTimeLimit(sessionMap.Planets.Count);
            
            await _context.Sessions.AddAsync(session, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            // add heroes
            var heroes = new List<Hero>();
            foreach (var item in lobbyInfos)
            {
                var hero = new Hero
                {
                    HeroId = Guid.NewGuid(),
                    Name = item.User.Username,
                    Argb = item.Argb,
                    ColonizationShipLimit = 10,
                    ResearchShipLimit = 10,
                    Resourses = 10,
                    SessionId = session.Id,
                    Session = session,
                    UserId = item.UserId
                };
                
                var homePlanet = sessionMap.Planets[Random.Shared.Next(0, sessionMap.Planets.Count)];
                hero.HomePlanetId = homePlanet.Id;
                homePlanet.OwnerId = hero.HeroId;

                var addingResult = await _heroService.Create(item.UserId, hero, cancellationToken);

                if (addingResult.Success == false)
                {
                    _logger.LogError(addingResult.ErrorMessage);
                    return new ServiceResult<Session>(addingResult.ErrorMessage);
                }
                else
                {
                    heroes.Add(hero);
                }
            }
            
            var heroPlanetRelationsResult = await GenerateHeroPlanetRelations(heroes, sessionMap.Planets);
            if (heroPlanetRelationsResult.Success == false)
                return new ServiceResult<Session>(heroPlanetRelationsResult.ErrorMessage);

            if (session.HeroTurnId == Guid.Empty)
            {
                session.HeroTurnId = heroes.First().HeroId;
                await UpdateSessionAsync(session, cancellationToken);
            }
            
            return new ServiceResult<Session>(session);
        }
        public async Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .Include(x => x.Heroes)
                 .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);
            
            return session;
        }

        public async Task<ServiceResult> ResearchOrColonizePlanetAsync(Guid sessionId, Guid planetId,
            CancellationToken cancellationToken)
        {
            var planet = await _context.Planets.FirstOrDefaultAsync(x => x.Id == planetId, cancellationToken);
            if (planet is null)
                return new ServiceResult(ErrorMessages.Planet.NotFound);

            throw new NotImplementedException();
            /*
             * if (planet.Status == (int)PlanetStatus.Researched)
            {
                planet.Status = (int)PlanetStatus.Colonized;
            }

            planet.Status = (int)PlanetStatus.Researched;
             */
            await _context.SaveChangesAsync(cancellationToken);
            return new ServiceResult();
        }

        public async Task<HeroMapView?> GetHeroMapAsync(Guid heroId, CancellationToken cancellationToken)
        {
            var heroPlanets = await _context.HeroPlanetRelations
                .Include(x => x.Planet)
                .Where(x => x.HeroId == heroId && x.Status >= (int)PlanetStatus.Known)
                .ToListAsync(cancellationToken);

            if (heroPlanets.Any() == false)
                return null;

            var planets = heroPlanets.Select(x =>
            {
                if (x.Status == (int)PlanetStatus.Enemy)
                {
                    x.Planet.IsEnemy = true;
                }
                else
                {
                    x.Planet.IsEnemy = false;
                }
                
                return x.Planet;
            }).ToList();
            var connections = await GetConnections(planets);

            var heroMap = new HeroMapView
            {
                HeroId = heroId,
                Planets = planets,
                Connections = connections
            };
            return heroMap;
        }

        private async Task<ServiceResult<int>> UpdateSessionAsync(Session designation, CancellationToken cancellationToken)
        {
            var session = await _context.Sessions.FirstOrDefaultAsync(x => x.Id == designation.Id, cancellationToken);
            if (session is null)
            {
                throw new InvalidOperationException("Given session does not exist, you can not update it");
            }

            session.Name = designation.Name;
            session.TurnNumber = designation.TurnNumber;
            session.HeroTurnId = designation.HeroTurnId;
            session.TurnTimeLimit = designation.TurnTimeLimit;

            var result = await _context.SaveChangesAsync(cancellationToken);
            return new ServiceResult<int>(result);
        }

        private async Task<ServiceResult> GenerateHeroPlanetRelations(List<Hero> heroes, List<Planet> planets)
        {
            var relations = new List<HeroPlanetRelation>();
            foreach (var hero in heroes)
            {
                foreach (var planet in planets)
                {
                    var relation = new HeroPlanetRelation
                    {
                        HeroId = hero.HeroId,
                        PlanetId = planet.Id,
                        IterationsLeftToTheNextStatus = 1
                    };
                    
                    if (planet.OwnerId == null)
                    {
                        relation.Status = (int)PlanetStatus.Known;
                        relations.Add(relation);
                    }
                    else if (planet.OwnerId == hero.HomePlanetId)
                    {
                        relation.Status = (int)PlanetStatus.Colonized;
                        relations.Add(relation);
                    }
                    else if (planet.OwnerId != hero.HomePlanetId)
                    {
                        relation.Status = (int)PlanetStatus.Enemy;
                        relations.Add(relation);
                    }
                }
            }

            await _context.HeroPlanetRelations.AddRangeAsync(relations);
            var updated = await _context.SaveChangesAsync();
            if(updated == 0)
            {
                var exception = new InvalidDataException("Database records has not been updated");
                _logger.LogError(exception, "Can not Create planet relations");
                throw exception;
            }

            return new ServiceResult();
        }

        private async Task<List<Edge>> GetConnections(List<Planet> planets)
        {
            var connections = new List<Edge>();
            foreach (var planet in planets)
            {
                var subResult =await _context.Connections
                    .Where(x => planet.Id == x.FromPlanetId || planet.Id == x.ToPlanetId)
                    .ToListAsync();
                connections.AddRange(subResult);
            }
            return connections;
        }
    }
}