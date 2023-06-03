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

    Task<ServiceResult<Battle>> StartBattleAsync(Guid attackerId, Guid attackedPlanetId, Guid fromPlanetId,
        int attackerSoldiersCount, CancellationToken cancellationToken);
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
    
        // update statuses
        UpdateHeroesSoldiers(session.Heroes);
        await UpdatePlanetsHealthAsync(sessionId, cancellationToken);
        
        ChooseNextHero(session);
        
        await _sessionService.UpdateSessionAsync(session, cancellationToken);

        return new ServiceResult<Session>(session);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ServiceResult<Battle>> StartBattleAsync(Guid attackerId, Guid attackedPlanetId, Guid fromPlanetId, 
        int attackerSoldiersCount, CancellationToken cancellationToken)
    {
        var attacker = await _context.Heroes.FirstOrDefaultAsync(x => x.HeroId == attackerId, cancellationToken);
        if (attacker is null)
            return new ServiceResult<Battle>(ErrorMessages.Hero.NotFound);

        if (attackerSoldiersCount > attacker.AvailableSoldiers)
        {
            return new ServiceResult<Battle>(ErrorMessages.Session.NotEnoughSoldiers);
        }

        var heroPlanetRelation = await _context.HeroPlanetRelations
            .Include(x => x.Hero)
            .FirstOrDefaultAsync(x => 
                x.PlanetId == attackedPlanetId && 
                x.Status == PlanetStatus.Colonized, 
                cancellationToken);

        if (heroPlanetRelation is null)
            return new ServiceResult<Battle>(ErrorMessages.Relation.NotFound);

        var defendingHero = heroPlanetRelation.Hero;

        var battle = new Battle
        {
            Id = Guid.NewGuid(),
            AttackerHeroId = attackerId,
            DefendingHeroId = defendingHero.HeroId,
            AttackedPlanetId = attackedPlanetId,
            AttackedFromId = fromPlanetId,
            Status = BattleStatus.InProcess,
            AttackerSoldiers = attackerSoldiersCount,
            DefenderSoldiers = 0
        };

        _context.Battles.Add(battle);
        await _context.SaveChangesAsync(cancellationToken);
        return new ServiceResult<Battle>(battle);
    }

    private IPlanetAction GetPlanetActionBasedOnRelationStatus(HeroPlanetRelation relation)
    {
        if (relation.Status == PlanetStatus.Known || relation.Status == PlanetStatus.Researching)
        {
            return new PlanetResearcher(relation, _gameObjectsRepository);
        }
        else if (relation.Status == PlanetStatus.Researched || relation.Status == PlanetStatus.Colonizing)
        {
            return new PlanetColonizer(relation);
        }
        else if (relation.Status == PlanetStatus.Colonized && relation.FortificationLevel == Fortification.None)
        {
            return new FortificationFirstLevelBuilder(relation);
        }
        else if (relation.FortificationLevel == Fortification.Weak)
        {
            return new FortificationSecondLevelBuilder(relation);
        }
        else if (relation.FortificationLevel == Fortification.Reliable)
        {
            return new FortificationThirdLevelBuilder(relation);
        }
        else
        {
            return new PlanetDoNothingAction();
        }
    }

    private void UpdateHeroesSoldiers(ICollection<Hero> heroes)
    {
        foreach (var hero in heroes)
        {
            var soldiersCount = hero.CalculateNextSoldiersCount();

            if (soldiersCount + hero.AvailableSoldiers > hero.SoldiersLimit)
            {
                hero.AvailableSoldiers = hero.SoldiersLimit;
            }
            else
            {
                hero.AvailableSoldiers += soldiersCount;
            }
        }
    }

    private async Task UpdatePlanetsHealthAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var sessionMap = await _context.SessionMaps
            .Include(x => x.Planets)
            .FirstOrDefaultAsync(x => x.Session.Id == sessionId, cancellationToken);

        var planets = sessionMap.Planets;

        foreach (var planet in planets.Where(x => x.Health < x.HealthLimit))
        {
            var heal = planet.CalculateHealOnTheNextTurn();

            if (heal + planet.Health > planet.HealthLimit)
            {
                planet.Health = planet.HealthLimit;
            }
            else
            {
                planet.Health += heal;
            }
        }
    }

    private void ChooseNextHero(Session session)
    {
        session.TurnNumber += 1;
        var heroes = session.Heroes.OrderBy(x => x.Name).ToList();
        
        int nextHeroIndex = session.TurnNumber % heroes.Count;
        var hero = heroes[nextHeroIndex];
        session.HeroTurnId = hero.HeroId;
    }
}