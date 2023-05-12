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
            var map = _mapGenerator.GenerateMap(defaultOptions);
            
            var session = new Session
            {
                Id = Guid.NewGuid(),
                Name = lobby.LobbyName,
                Heroes = new List<Hero>(),
                SessionMapId = map.Id,
                SessionMap = map,
                TurnNumber = 0,
                ActiveHeroId = 0
            };
            
            await _context.Sessions.AddAsync(session, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            // add heroes
            foreach (var item in lobbyInfos)
            {
                var hero = new Hero
                {
                    Name = item.User.Username,
                    Argb = item.Argb,
                    ColonizationShipLimit = 10,
                    ResearchShipLimit = 10,
                    Resourses = 10,
                    SessionId = session.Id,
                    Session = session,
                    UserId = item.UserId
                };
                
                var homePlanet = map.Planets[Random.Shared.Next(0, map.Planets.Count)];
                hero.HeroMapView = new HeroMapView
                {
                    Planets = map.Planets, Connections = map.Connections, HeroId = hero.HeroId,
                    HomePlanetId = homePlanet.Id, HomePlanet = homePlanet
                };
                
                var addingResult = await _heroService.Create(item.UserId, hero, cancellationToken);

                if (addingResult.Success == false)
                {
                    _logger.LogError(addingResult.ErrorMessage);
                    return new ServiceResult<Session>(addingResult.ErrorMessage);
                }
            }

            return new ServiceResult<Session>(session);
        }
        public async Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var session = await _context.Sessions
                .Include(x => x.Heroes)
                 .ThenInclude(x => x.User)
                .Include(x => x.SessionMap)
                 .ThenInclude(x => x.Connections)
                .Include(x => x.SessionMap)
                 .ThenInclude(x => x.Planets)
                  .ThenInclude(x => x.Position)
                .AsSplitQuery()
                .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);
            
            return session;
        }

        public async Task<ServiceResult> ResearchOrColonizePlanetAsync(Guid sessionId, Guid planetId,
            CancellationToken cancellationToken)
        {
            var planet = await _context.Planets.FirstOrDefaultAsync(x => x.Id == planetId, cancellationToken);
            if (planet is null)
                return new ServiceResult(ErrorMessages.Planet.NotFound);

            if (planet.Status == (int)PlanetStatus.Researched)
            {
                planet.Status = (int)PlanetStatus.Colonized;
            }

            planet.Status = (int)PlanetStatus.Researched;
            await _context.SaveChangesAsync(cancellationToken);
            return new ServiceResult();
        }
    }
}