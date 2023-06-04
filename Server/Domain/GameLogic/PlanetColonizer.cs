using Server.Common.Constants;
using Server.Repositories;
using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain.GameLogic;

public class PlanetColonizer : IPlanetAction
{
    private readonly HeroPlanetRelation _relation;
    private readonly Hero _hero;
    private readonly Planet _planet;
    public PlanetColonizer(HeroPlanetRelation relation)
    {
        if (relation.Planet is null)
            throw new ArgumentException("Relation must be with planet");
        if (relation.Hero is null)
            throw new ArgumentException("Relation must be with not null hero");

        _relation = relation;
        _planet = relation.Planet;
        _hero = relation.Hero;
    }

    public async Task<ServiceResult<PlanetActionResult>> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (_relation.Status == PlanetStatus.Researched)
        {
            return StartPlanetColonization(_relation, _hero);
        }
        else if(_relation.Status == PlanetStatus.Colonizing)
        {
            var result = ContinuePlanetColonization(_relation, _hero, cancellationToken);
            return new ServiceResult<PlanetActionResult>(result);
        }
        else
        {
            throw new InvalidOperationException($"This is planet colonizer class. It is can not handle other statuses, such as: {_relation.Status}");
        }
    }
    
    private ServiceResult<PlanetActionResult> StartPlanetColonization(HeroPlanetRelation relation, Hero hero)
    {
        if (hero.AvailableColonizationShips == 0)
        {
            return new ServiceResult<PlanetActionResult>(ErrorMessages.Session.NotEnoughColonizationShips);
        }
        
        int resourcesToColonize = relation.Planet.ResourceCount;
        if (hero.Resourses < resourcesToColonize)
            return new ServiceResult<PlanetActionResult>(ErrorMessages.Session.NotEnoughResourcesToColonize);
            
        relation.Status = PlanetStatus.Colonizing;
        relation.IterationsLeftToTheNextStatus = CalculateIterationsToNextStatus();
        hero.AvailableColonizationShips -= 1;
        hero.Resourses -= resourcesToColonize;
        
        var result = new PlanetActionResult(relation.Status, relation.FortificationLevel, _relation.PlanetId, 
            _hero.AvailableResearchShips, _hero.AvailableColonizationShips, _hero.Resourses, 
            relation.IterationsLeftToTheNextStatus);
        return new ServiceResult<PlanetActionResult>(result);
    }
    
    private int CalculateIterationsToNextStatus()
    {
        return Random.Shared.Next(2, 5);
    }
    
    private PlanetActionResult ContinuePlanetColonization(HeroPlanetRelation relation, Hero hero, CancellationToken cancellationToken)
    {
        if (relation.IterationsLeftToTheNextStatus == 1)
        {
            var result = ColonizePlanet(relation, hero);
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
    
    private PlanetActionResult ColonizePlanet(HeroPlanetRelation relation, Hero hero)
    {
        relation.Status = PlanetStatus.Colonized;
        relation.IterationsLeftToTheNextStatus = 1;

        hero.AvailableColonizationShips += 1;
        var planetSize = _planet.Size;
        hero.UpdateAvailableSoldiersAndSoldiersLimitByColonizedPlanetSize(planetSize);

        _planet.OwnerId = hero.HeroId;
        _planet.ColorStatus = hero.ColorStatus;
        
        var result = new PlanetActionResult(relation.Status, relation.FortificationLevel, _relation.PlanetId, 
            _hero.AvailableResearchShips, _hero.AvailableColonizationShips, _hero.Resourses, 
            relation.IterationsLeftToTheNextStatus);
        return result;
    }
}
