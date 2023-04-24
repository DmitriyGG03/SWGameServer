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
        private readonly IMapService _mapService;
        private readonly IHeroService _heroService;
        public SessionService(GameDbContext context, IMapGenerator mapGenerator, IMapService mapService, IHeroService heroService)
        {
            _context = context;
            _mapGenerator = mapGenerator;
            _mapService = mapService;
            _heroService = heroService;
        }
        public async Task<ServiceResult<Session>> Create(Guid lobbyId)
        {

            var lobby = await _context.Lobbies.FirstOrDefaultAsync(l => l.Id == lobbyId);

            if (lobby == null)
                return new ServiceResult<Session>(ErrorMessages.Lobby.NotFound);

            /*
            var ready = lobby.LobbyInfos.All(li => li.Ready);
            if (!ready)
                return new ServiceResult<Session>(ErrorMessages.Lobby.UsersNotReady);
            */
            var session = new Session();

            var defaultOptions = new MapGenerationOptions(800, 600, 50, 25, 60);

            //session.SessionMap = _mapGenerator.GenerateMap(defaultOptions);
            session.Id = Guid.NewGuid();
            session.Name = lobby.LobbyName;
           // session.SessionMap.Id = Guid.NewGuid();
            //var users = lobby.LobbyInfos.Select(l => l.User).ToList();
            var users = _context.Users.Take(1).ToList();
            var heroes = new List<Hero>();
          //  await _context.SessionMaps.AddAsync(session.SessionMap);
           // await _context.SaveChangesAsync(new CancellationToken(false));
           // await _context.Sessions.AddAsync(session, new CancellationToken(false));
           // await _context.SaveChangesAsync(new CancellationToken(false));
            for (int i = 0; i < users.Count; i++)
            {
                var hero = new Hero();
                hero.ResearchShipLimit = 1;
                hero.Resourses = 1;
                hero.Argb = 1;
                hero.ColonizationShipLimit = 1;
                hero.Name = users[i].Username;
               // hero.SessionId = session.Id;
                await _context.SessionMaps.AddAsync(session.SessionMap);
                await _context.SaveChangesAsync(new CancellationToken(false));
                var heroResult = await _heroService.Create(users[i].Id, hero, new CancellationToken(false));
                 if (!heroResult.Success) {
                    return new ServiceResult<Session>(heroResult.ErrorMessage);
            }
             heroes.Add(heroResult.Value);
             users[i].LobbyInfos = lobby.LobbyInfos;
             users[i].Heroes.Add(heroResult.Value);
             _context.Users.Update(users[i]);
                await _context.SaveChangesAsync(new CancellationToken(false));
                 }
                var heromaps = _mapService.GetHeroMaps(heroes, session.SessionMap, new CancellationToken(false)).Result.Value;
                  for (int i = 0; i < heroes.Count; i++) {
              //   await _context.HeroMaps.AddAsync(heromaps[i]);
                 await _context.SaveChangesAsync(new CancellationToken(false));
                heroes[i].HeroMapId = heromaps[i].Id;
                heroes[i].SessionId = session.Id;
                  }

                // session.Heroes = heroes;
                //


                return new ServiceResult<Session>(session);
        }
    }
}