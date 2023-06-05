using Microsoft.EntityFrameworkCore;
using Server.Common.Constants;
using Server.Domain;
using Server.Domain.Exceptions;
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

    Task<ServiceResult<PlanetActionResult>> StartPlanetColonizationOrResearching(Guid planetId, Guid heroId,
        CancellationToken cancellationToken);
    
    Task<ServiceResult<(Session session, bool nextTurn)>> MakeNextTurnAsync(Guid sessionId, Guid heroId, CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    Task<ServiceResult<Battle>> StartBattleAsync(Guid attackerId, Guid attackedPlanetId, Guid fromPlanetId,
        int attackerSoldiersCount, CancellationToken cancellationToken);

    Task<ServiceResult<Battle>> DefendPlanetAsync(Guid heroId, Guid planetId, int countOfSoldiers,
        CancellationToken cancellationToken);

    Task<List<Battle>> GetBattlesBySessionAsync(Session session, CancellationToken cancellationToken);
}

public class GameService : IGameService
{
    private readonly ISessionService _sessionService;
    private readonly GameDbContext _context;
    private readonly IGameObjectsRepository _gameObjectsRepository;
    private readonly ILogger<GameService> _logger;
    public GameService(GameDbContext context, ISessionService sessionService, IGameObjectsRepository gameObjectsRepository, ILogger<GameService> logger)
    {
        _context = context;
        _sessionService = sessionService;
        _gameObjectsRepository = gameObjectsRepository;
        _logger = logger;
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
    
    public async Task<ServiceResult<PlanetActionResult>> StartPlanetColonizationOrResearching(Guid planetId, Guid heroId,
        CancellationToken cancellationToken)
    {
        var relation = await _gameObjectsRepository.GetRelationByHeroAndPlanetIdsAsync(heroId, planetId, cancellationToken);

        if (relation is null)
            return new ServiceResult<PlanetActionResult>(ErrorMessages.Relation.NotFound);

        IPlanetAction? planetAction = GetPlanetStartActionBasedOnRelationStatus(relation);
        if (planetAction is null)
            return new ServiceResult<PlanetActionResult>(ErrorMessages.Session.CanNotStartResearchingOrColonization);
        
        var result = await planetAction.ExecuteAsync(cancellationToken);
        if (result.Success == false)
            return new ServiceResult<PlanetActionResult>(result.ErrorMessage);

        await SaveChangesAsync(cancellationToken);
        return new ServiceResult<PlanetActionResult>(result.Value);
    }

    public async Task<ServiceResult<(Session session, bool nextTurn)>> MakeNextTurnAsync(Guid sessionId, Guid heroId, CancellationToken cancellationToken)
    {
        var session = await _sessionService.GetByIdAsync(sessionId, cancellationToken);
        if (session is null)
            return new ServiceResult<(Session session, bool nextTurn)>(ErrorMessages.Session.NotFound);
        if (session.HeroTurnId != heroId)
            return new ServiceResult<(Session session, bool nextTurn)>(ErrorMessages.Session.NotHeroTurn);
            
        if (session.Heroes is null)
            throw new NullReferenceException("You probably changed GetByIdAsync method in session service. Heroes can not be null there");
    
        bool newTurn = ChooseNextHeroAndDetermineIfItsTheStartOfNextTurn(session);
        
        if (newTurn)
        {
            session.TurnNumber += 1;
            
            UpdateHeroesSoldiers(session.Heroes);
            await HandleBattlesAsync(cancellationToken);
            
            await HandlePlanetActions(cancellationToken);

            await UpdateHeroesResourcesAsync(session.Heroes);
            await UpdatePlanetsHealthAsync(sessionId, cancellationToken);
        }

        await SaveChangesAsync(cancellationToken);
        return new ServiceResult<(Session session, bool nextTurn)>((session, newTurn));
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
        else
        {
            attacker.AvailableSoldiers -= attackerSoldiersCount;
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
            BattleTurnNumber = 0
        };

        await _context.Battles.AddAsync(battle, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return new ServiceResult<Battle>(battle);
    }

    public async Task<ServiceResult<Battle>> DefendPlanetAsync(Guid heroId, Guid planetId, int countOfSoldiers, CancellationToken cancellationToken)
    {
        var hero = await _context.Heroes.FirstOrDefaultAsync(x => x.HeroId == heroId, cancellationToken);
        if (hero is null)
            return new ServiceResult<Battle>(ErrorMessages.Hero.NotFound);
        if (hero.AvailableSoldiers < countOfSoldiers)
            return new ServiceResult<Battle>(ErrorMessages.Session.NotEnoughSoldiers);
        else
            hero.AvailableSoldiers -= countOfSoldiers;

        var battle = await _context.Battles
            .Include(x => x.AttackedPlanet)
            .FirstOrDefaultAsync(x =>
                x.AttackedPlanetId == planetId &&
                x.DefendingHeroId == heroId &&
                x.Status == BattleStatus.InProcess, cancellationToken);
        if (battle is null)
            return new ServiceResult<Battle>(ErrorMessages.Battle.NotFound);

        if (battle.AttackedPlanet is null)
            throw new InvalidOperationException("Battle must contain attacked planet");

        var attackedPlanet = battle.AttackedPlanet;
        if (attackedPlanet.Health + countOfSoldiers > attackedPlanet.HealthLimit)
        {
            attackedPlanet.Health = attackedPlanet.HealthLimit;
        }
        else
        {
            battle.AttackedPlanet.Health += countOfSoldiers;
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        return new ServiceResult<Battle>(battle);
    }

    public async Task<List<Battle>> GetBattlesBySessionAsync(Session session, CancellationToken cancellationToken)
    {
        if (session.Heroes is null)
            throw new ArgumentException("Session must contain heroes to get their battles");

        var battles = await _context.Battles.ToListAsync(cancellationToken);
        var sessionBattles = battles
            .Where(x => 
                session.Heroes.Any(h => 
                    h.HeroId == x.AttackerHeroId || 
                    h.HeroId == x.DefendingHeroId))
            .ToList();
        return sessionBattles;
    }

    private IPlanetAction? GetPlanetStartActionBasedOnRelationStatus(HeroPlanetRelation relation)
    {
        if (relation.Status is PlanetStatus.Known)
        {
            return new PlanetResearcher(relation, _gameObjectsRepository);
        }
        else if (relation.Status is PlanetStatus.Researched)
        {
            return new PlanetColonizer(relation);
        }

        return null;
    }
    
    private IPlanetAction GetPlanetActionBasedOnRelationStatus(HeroPlanetRelation relation)
    {
        if (relation.Status is PlanetStatus.Known or PlanetStatus.Researching)
        {
            return new PlanetResearcher(relation, _gameObjectsRepository);
        }
        else if (relation.Status is PlanetStatus.Researched or PlanetStatus.Colonizing)
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
        
        return new PlanetDoNothingAction(relation);
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
    
    private async Task UpdateHeroesResourcesAsync(ICollection<Hero> heroes)
    {
        var relations = await _context.HeroPlanetRelations
            .Include(x => x.Planet)
            .Where(x => x.Status == PlanetStatus.Colonized).ToListAsync();
        
        foreach (var hero in heroes)
        {
            foreach (var relation in relations.Where(x => x.HeroId == hero.HeroId))
            {
                hero.Resourses += (int)(relation.Planet.ResourceCount * 0.5);
            }
        }
    }
    
    private async Task UpdatePlanetsHealthAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var sessionMap = await _context.SessionMaps
            .Include(x => x.Planets)
            .FirstOrDefaultAsync(x => x.Session.Id == sessionId, cancellationToken);

        var planets = sessionMap.Planets;
        var battles = await _context.Battles.Where(x => x.Status == BattleStatus.InProcess).ToListAsync(cancellationToken);
        foreach (var planet in planets.Where(x => x.Health < x.HealthLimit))
        {
            if (battles.Any(x => x.AttackedPlanetId == planet.Id))
                 continue;
             
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

    private bool ChooseNextHeroAndDetermineIfItsTheStartOfNextTurn(Session session)
    {
        bool startOfTurn = false;
        session.HeroNumber += 1;
        var heroes = session.Heroes.OrderBy(x => x.Name).ToList();
        
        int nextHeroIndex = session.HeroNumber % heroes.Count;
        if (nextHeroIndex == 0)
        {
            startOfTurn = true;
        }
        
        var hero = heroes[nextHeroIndex];
        session.HeroTurnId = hero.HeroId;

        return startOfTurn;
    }

    private async Task HandleBattlesAsync(CancellationToken cancellationToken)
    {
        var battles = await _context.Battles
            .Include(x => x.AttackerHero)
            .Include(x => x.DefendingHero)
            .Include(x => x.AttackedPlanet)
            .Where(x => x.Status == BattleStatus.InProcess)
            .ToListAsync(cancellationToken);

        if (battles.Any())
        {
            foreach (var battle in battles)
            {
                await HandleBattle(battle, cancellationToken);
            }
        }   
    }

    private async Task HandleBattle(Battle battle, CancellationToken cancellationToken)
    {
        var attackedPlanet = battle.AttackedPlanet;

        if (attackedPlanet is null)
            throw new InvalidOperationException("Attacked planet can not be null. You probably changed the HandleBattlesAsync method");

        var attacker = battle.AttackerHero;
        if (attacker is null)
            throw new InvalidOperationException("You probably changed the battle getting request. Attacker can not be null");
        
        attacker.AvailableSoldiers -= (int)(attacker.AvailableSoldiers * 0.2);
        var damage = attacker.AvailableSoldiers - attackedPlanet.Size * 2;
        if (damage <= 0)
        {
            battle.Status = BattleStatus.DefenderWon;
        }
        else if (attackedPlanet.Health - damage <= 0)
        {
            await ConquerPlanetAsync(battle, attackedPlanet, cancellationToken);
        }
        else
        {
            attackedPlanet.Health -= damage;
            battle.BattleTurnNumber += 1;
        }
    }

    private async Task ConquerPlanetAsync(Battle battle, Planet attackedPlanet, CancellationToken cancellationToken)
    {
        battle.Status = BattleStatus.AttackerWon;
        attackedPlanet.OwnerId = battle.AttackerHeroId;
        attackedPlanet.Health = (int)(attackedPlanet.HealthLimit * 0.5);

        // update planet relation
        var defenderRelation = await _gameObjectsRepository.GetRelationByHeroAndPlanetIdsAsync(
            battle.DefendingHeroId,
            attackedPlanet.Id, cancellationToken);
        if (defenderRelation is null)
            throw new InvalidOperationException("Somehow we do not have defender relation");

        defenderRelation.Status = PlanetStatus.Known;

        var attackerRelation = await _gameObjectsRepository.GetRelationByHeroAndPlanetIdsAsync(
            battle.AttackerHeroId,
            attackedPlanet.Id, cancellationToken);
        if (attackerRelation is null)
            throw new InvalidOperationException("Somehow we do not have attacker relation");

        attackerRelation.Status = PlanetStatus.Colonized;

        if (attackedPlanet.IsCapital)
        {
            HandleWinStatement(battle);
        }
    }

    private void HandleWinStatement(Battle battle)
    {
        _logger.LogWarning($"The capital of hero: {battle.DefendingHeroId} has been colonized");
        _logger.LogWarning($"User: {battle.AttackerHeroId} has been defeated!");

        var gameEndedException = new GameEndedException(battle.AttackerHero);
        throw gameEndedException;
    }

    private async Task HandlePlanetActions(CancellationToken cancellationToken)
    {
        // TODO: update fortifications
        var relations = await _context.HeroPlanetRelations
            .Include(x => x.Hero)
            .Include(x => x.Planet)
            .Where(x => 
                x.Status == PlanetStatus.Researching ||
                x.Status == PlanetStatus.Colonizing)
            .ToListAsync(cancellationToken);

        foreach (var relation in relations)
        {
            IPlanetAction relationHandler = GetPlanetActionBasedOnRelationStatus(relation);
            var result = await relationHandler.ExecuteAsync(cancellationToken);
            if (result.Success == false)
            {
                _logger.LogWarning("We can not handle planet status updating");
            }
        }
    }
}