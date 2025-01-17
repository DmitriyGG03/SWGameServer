using Server.Common.Constants;
using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain.GameLogic;

public abstract class FortificationBuilderBase : IPlanetAction
{
    protected readonly HeroPlanetRelation Relation;
    protected readonly Hero Hero;
    protected FortificationBuilderBase(HeroPlanetRelation relation)
    {
        if (relation.Planet is null)
            throw new ArgumentException("Relation must be with planet");
        if (relation.Hero is null)
            throw new ArgumentException("Relation must be with not null hero");
        
        Relation = relation;
        Hero = relation.Hero;
    }
    
    public virtual async Task<ServiceResult<PlanetActionResult>> ExecuteAsync(CancellationToken cancellationToken)
    {
        int resourcesToBuildFortification = CalculatePrice();
        if (Hero.Resourses < resourcesToBuildFortification)
            return new ServiceResult<PlanetActionResult>(ErrorMessages.Session.NotEnoughResourcesToBuildFortification);
            
        Relation.FortificationLevel = GetNextFortificationLevel();
        Relation.IterationsLeftToTheNextStatus = 1;
        Hero.Resourses -= resourcesToBuildFortification;
        
        var result = new PlanetActionResult(Relation.Status, Relation.FortificationLevel, Relation.PlanetId, 
            Relation.Hero.AvailableResearchShips, Relation.Hero.AvailableColonizationShips, Relation.Hero.Resourses, 
            Relation.IterationsLeftToTheNextStatus);
        return new ServiceResult<PlanetActionResult>(result);
    }

    protected abstract Fortification GetNextFortificationLevel();
    protected abstract int CalculatePrice();
}