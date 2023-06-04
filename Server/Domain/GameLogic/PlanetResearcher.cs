using Server.Common.Constants;
using Server.Repositories;
using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain.GameLogic;

public class PlanetResearcher : IPlanetAction
{
    private readonly HeroPlanetRelation _relation;
    private readonly Hero _hero;
    private readonly IGameObjectsRepository _gameObjectsRepository;
    
    public PlanetResearcher(HeroPlanetRelation relation, IGameObjectsRepository gameObjectsRepository)
    {
        if (relation.Planet is null)
            throw new ArgumentException("Relation must be with planet");
        if (relation.Hero is null)
            throw new ArgumentException("Relation must be with not null hero");

        _relation = relation;
        _gameObjectsRepository = gameObjectsRepository;
        _hero = relation.Hero;
    }

    public async Task<ServiceResult<PlanetActionResult>> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (_relation.Status == PlanetStatus.Known)
        {
            return StartPlanetResearching(_relation, _hero);
        }
        else if(_relation.Status == PlanetStatus.Researching)
        {
            var result = await ContinuePlanetResearchingAsync(_relation, _hero, cancellationToken);
            return new ServiceResult<PlanetActionResult>(result);
        }
        else
        {
            throw new InvalidOperationException($"This is planet researcher class. It is can not handle other statuses, such as: {_relation.Status}");
        }
    }
    
    private ServiceResult<PlanetActionResult> StartPlanetResearching(HeroPlanetRelation relation, Hero hero)
    {
        if (hero.AvailableResearchShips == 0)
        {
            return new ServiceResult<PlanetActionResult>(ErrorMessages.Session.NotEnoughResearchShips);
        }
            
        relation.Status = PlanetStatus.Researching;
        relation.IterationsLeftToTheNextStatus = CalculateIterationsToNextStatus();
        hero.AvailableResearchShips -= 1;

        var result = new PlanetActionResult(relation.Status, relation.FortificationLevel, _relation.PlanetId, 
            _hero.AvailableResearchShips, _hero.AvailableColonizationShips, _hero.Resourses, 
            relation.IterationsLeftToTheNextStatus);
        return new ServiceResult<PlanetActionResult>(result);
    }
    
    private int CalculateIterationsToNextStatus()
    {
        return Random.Shared.Next(2, 5);
    }
    
    private async Task<PlanetActionResult> ContinuePlanetResearchingAsync(HeroPlanetRelation relation, Hero hero, CancellationToken cancellationToken)
    {
        if (relation.IterationsLeftToTheNextStatus == 1)
        {
            // researched
            relation.Status = PlanetStatus.Researched;
            relation.IterationsLeftToTheNextStatus = 1;

            await UpdateNeighborsRelationStatusesAsync(relation.PlanetId, hero.HeroId, cancellationToken);
            hero.AvailableResearchShips += 1;
            var result = new PlanetActionResult(relation.Status, relation.FortificationLevel, _relation.PlanetId, 
                _hero.AvailableResearchShips, _hero.AvailableColonizationShips, _hero.Resourses, 
                relation.IterationsLeftToTheNextStatus);
            return result;
        }
        else
        {
            relation.IterationsLeftToTheNextStatus -= 1;
            var result = new PlanetActionResult(relation.Status, relation.FortificationLevel, _relation.PlanetId, 
                _hero.AvailableResearchShips, _hero.AvailableColonizationShips, _hero.Resourses, 
                relation.IterationsLeftToTheNextStatus);
            return result;
        }
    }
    
    private async Task UpdateNeighborsRelationStatusesAsync(Guid planetId, Guid heroId, CancellationToken cancellationToken)
    {
        List<Planet> neighborPlanets = await _gameObjectsRepository.GetNeighborPlanetsAsync(planetId, cancellationToken);

        var relationsToKnow = new List<HeroPlanetRelation>();
        foreach (var planet in neighborPlanets)
        {
            var relationToKnow =
                await _gameObjectsRepository.GetUnknownRelationByHeroAndPlanetIdsAsync(heroId, planet.Id, cancellationToken);

            if (relationToKnow is not null)
            {
                relationToKnow.Status = PlanetStatus.Known;
                relationsToKnow.Add(relationToKnow);
            }
        }

        _gameObjectsRepository.UpdateHeroPlanetRelations(relationsToKnow);
    }
}