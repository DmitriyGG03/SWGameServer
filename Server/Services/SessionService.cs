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

        public SessionService(GameDbContext context, IMapGenerator mapGenerator, IHeroService heroService,
            ILogger<SessionService> logger)
        {
            _context = context;
            _mapGenerator = mapGenerator;
            _heroService = heroService;
            _logger = logger;
        }

        public async Task<ServiceResult<Session>> CreateAsync(Guid lobbyId, CancellationToken cancellationToken)
        {
            var lobby = await _context.Lobbies
                .Include(x => x.LobbyInfos)
                 .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(l => l.Id == lobbyId, cancellationToken);
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

            List<Hero> heroes = await CreateHeroesBasedOnLobbyInfosAndAddThemToDbAsync(lobbyInfos, session, sessionMap, cancellationToken);

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

        public async Task<ServiceResult<MessageContainer>> ResearchOrColonizePlanetAsync(Guid sessionId, Guid planetId,
            Guid heroId,
            CancellationToken cancellationToken)
        {
            var relation = await _context.HeroPlanetRelations.FirstOrDefaultAsync(x => x.HeroId == heroId &&
                x.PlanetId == planetId &&
                x.Status >= (int)PlanetStatus.Known, cancellationToken);

            if (relation is null)
            {
                return new ServiceResult<MessageContainer>(ErrorMessages.Relation.NotFound);
            }
            
            var hero = await _context.Heroes.FirstOrDefaultAsync(x => x.HeroId == heroId, cancellationToken);
            if (hero is null)
            {
                return new ServiceResult<MessageContainer>(ErrorMessages.Hero.NotFound);
            }
            
            string message = String.Empty;
            switch (relation.Status)
            {
                case (int)PlanetStatus.Known:
                    message = StartResearchPlanet(relation, hero, cancellationToken);
                    break;
                case (int)PlanetStatus.Researching:
                    message = await ContinuePlanetResearchingAsync(relation, hero, cancellationToken);
                    break;
                case (int)PlanetStatus.Researched:
                    message = StartPlanetColonization(relation, hero);
                    break;
                case (int)PlanetStatus.Colonization:
                    message = ContinuePlanetColonization(relation, hero);
                    break;
                default:
                    return new ServiceResult<MessageContainer>(new MessageContainer { Message = SuccessMessages.Session.CanNotOperateWithGivenPlanet });
            }

            await _context.SaveChangesAsync(cancellationToken);
            return new ServiceResult<MessageContainer>(new MessageContainer { Message = message });
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

                x.Planet.Status = x.Status;
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

        private async Task<ServiceResult<int>> UpdateSessionAsync(Session designation,
            CancellationToken cancellationToken)
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
                    else if (planet.OwnerId == hero.HeroId)
                    {
                        relation.Status = (int)PlanetStatus.Colonized;
                        relations.Add(relation);
                    }
                    else if (planet.OwnerId != hero.HeroId)
                    {
                        relation.Status = (int)PlanetStatus.Enemy;
                        relations.Add(relation);
                    }
                }
            }

            await _context.HeroPlanetRelations.AddRangeAsync(relations);
            var updated = await _context.SaveChangesAsync();
            if (updated == 0)
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
                var subResult = await _context.Connections
                    .Where(x => planet.Id == x.FromPlanetId)
                    .ToListAsync();
                connections.AddRange(subResult);
            }

            return connections;
        }

        private string ContinuePlanetColonization(HeroPlanetRelation relation, Hero hero)
        {
            var message = String.Empty;
            if (relation.IterationsLeftToTheNextStatus == 1)
            {
                relation.Status = (int)PlanetStatus.Colonized;
                relation.IterationsLeftToTheNextStatus = 1;
                message = SuccessMessages.Session.Colonized;
                hero.AvailableColonizationShips += 1;
            }
            else
            {
                relation.IterationsLeftToTheNextStatus -= 1;
                message = SuccessMessages.Session.IterationDone + relation.IterationsLeftToTheNextStatus;
            }

            return message;
        }
        
        private string StartPlanetColonization(HeroPlanetRelation relation, Hero hero)
        {
            if (hero.AvailableColonizationShips == 0)
            {
                return ErrorMessages.Session.NotEnoughColonizationShips;
            }
            
            relation.Status = (int)PlanetStatus.Colonization;
            relation.IterationsLeftToTheNextStatus = CalculateIterationsToNextStatus();
            hero.AvailableColonizationShips -= 1;
            return SuccessMessages.Session.StartedColonization + relation.IterationsLeftToTheNextStatus;
        }

        private string StartResearchPlanet(HeroPlanetRelation relation, Hero hero,
            CancellationToken cancellationToken)
        {
            if (hero.AvailableResearchShips == 0)
            {
                return ErrorMessages.Session.NotEnoughResearchShips;
            }
            
            relation.Status = (int)PlanetStatus.Researching;
            relation.IterationsLeftToTheNextStatus = CalculateIterationsToNextStatus();
            hero.AvailableResearchShips -= 1;
            
            return SuccessMessages.Session.StartedResearching + relation.IterationsLeftToTheNextStatus;
        }

        private async Task<string> ContinuePlanetResearchingAsync(HeroPlanetRelation relation, Hero hero,
            CancellationToken cancellationToken)
        {
            var message = String.Empty;
            if (relation.IterationsLeftToTheNextStatus == 1)
            {
                relation.Status = (int)PlanetStatus.Researched;
                relation.IterationsLeftToTheNextStatus = CalculateIterationsToNextStatus();

                await UpdateNeighborsRelationStatusAndNotSaveChangesAsync(relation.PlanetId, hero.HeroId, cancellationToken);
                hero.AvailableResearchShips += 1;
                message = SuccessMessages.Session.Researched;
            }
            else
            {
                relation.IterationsLeftToTheNextStatus -= 1;
                message = SuccessMessages.Session.IterationDone + relation.IterationsLeftToTheNextStatus;
            }

            return message;
        }

        private async Task UpdateNeighborsRelationStatusAndNotSaveChangesAsync(Guid planetId, Guid heroId,
            CancellationToken cancellationToken)
        {
            List<Guid> neighborPlanetsId = await _context.Connections
                .Where(x => x.ToPlanetId == planetId)
                .Select(x => x.ToPlanetId)
                .ToListAsync(cancellationToken);

            var relationsToKnow = new List<HeroPlanetRelation>();
            foreach (var id in neighborPlanetsId)
            {
                var relationToKnow = await _context.HeroPlanetRelations.FirstOrDefaultAsync(x =>
                    x.HeroId == heroId &&
                    x.PlanetId == id &&
                    x.Status == (int)PlanetStatus.Unknown, cancellationToken);

                if (relationToKnow is not null)
                {
                    relationToKnow.Status = (int)PlanetStatus.Known;
                    relationsToKnow.Add(relationToKnow);
                }
            }

            _context.HeroPlanetRelations.UpdateRange(relationsToKnow);
        }
        private int CalculateIterationsToNextStatus()
        {
            return Random.Shared.Next(1, 5);
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
                    Argb = item.Argb,
                    ColonizationShipLimit = 10,
                    AvailableColonizationShips = 10,
                    ResearchShipLimit = 10,
                    AvailableResearchShips = 10,
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
    }
}