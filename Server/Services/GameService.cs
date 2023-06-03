using Microsoft.EntityFrameworkCore;
using Server.Common.Constants;
using Server.Domain;
using Server.Domain.GameLogic;
using Server.Repositories;
using Server.Services.Abstract;
using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Services;

public interface IGameService
{
    Task<ServiceResult<IPlanetAction>> GetPlanetActionHandlerAsync(Guid planetId, Guid heroId,
        CancellationToken cancellationToken);

    Task<ServiceResult<Session>> MakeNextTurnAsync(Guid sessionId, Guid heroId, CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public class GameService : IGameService
{
    private readonly ISessionService _sessionService;
    private readonly GameDbContext _context;
    private readonly IGameObjectsRepository _gameObjectsRepository;

    public GameService(GameDbContext context, ISessionService sessionService, IGameObjectsRepository gameObjectsRepository)
    {
        _context = context;
        _sessionService = sessionService;
        _gameObjectsRepository = gameObjectsRepository;
    }

    public async Task<ServiceResult<IPlanetAction>> GetPlanetActionHandlerAsync(Guid planetId, Guid heroId,
        CancellationToken cancellationToken)
    {
        var relation = await _gameObjectsRepository.GetRelationByHeroAndPlanetIdsAsync(heroId, planetId, cancellationToken);

        if (relation is null)
            return new ServiceResult<IPlanetAction>(ErrorMessages.Relation.NotFound);

        IPlanetAction planetAction = GetPlanetActionBasedOnRelationStatus(relation);
        return new ServiceResult<IPlanetAction>(planetAction);
    }

    public async Task<ServiceResult<Session>> MakeNextTurnAsync(Guid sessionId, Guid heroId, CancellationToken cancellationToken)
    {
        var session = await _sessionService.GetByIdAsync(sessionId, cancellationToken);
        if (session is null)
            return new ServiceResult<Session>(ErrorMessages.Session.NotFound);
        if (session.HeroTurnId != heroId)
            return new ServiceResult<Session>(ErrorMessages.Session.NotHeroTurn);
            
        if (session.Heroes is null)
            throw new NullReferenceException("You probably changed GetByIdAsync method in session service. Heroes can not be null there");

        session.TurnNumber += 1;
        var heroes = session.Heroes.OrderBy(x => x.Name).ToList();
        int nextHeroIndex = session.TurnNumber % heroes.Count;
        var hero = heroes[nextHeroIndex];
            
        session.HeroTurnId = hero.HeroId;
        await _sessionService.UpdateSessionAsync(session, cancellationToken);

        return new ServiceResult<Session>(session);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    private IPlanetAction GetPlanetActionBasedOnRelationStatus(HeroPlanetRelation relation)
    {
        if (relation.Status == PlanetStatus.Known || relation.Status == PlanetStatus.Researching)
        {
            return new PlanetResearcher(relation, _gameObjectsRepository);
        }
        else if (relation.Status == PlanetStatus.Researched || relation.Status == PlanetStatus.Colonizing)
        {
            return new PlanetColonizer(relation, _gameObjectsRepository);
        }
        else
        {
            return new PlanetDoNothingAction();
        }
    }
}