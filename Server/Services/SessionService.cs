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
        public async Task<ServiceResult<Session>> CreateAsync(Guid lobbyId, CancellationToken cancellationToken)
        {
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
            
            // await _context.SessionMaps.AddAsync(map, cancellationToken);
            var session = new Session
            {
                Id = Guid.NewGuid(),
                Name = lobby.LobbyName,
                Heroes = new List<Hero>(),
                SessionMapId = map.Id,
                SessionMap = map,
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
                hero.HeroMap = new HeroMap
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
    }
}