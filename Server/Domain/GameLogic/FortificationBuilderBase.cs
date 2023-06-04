using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain.GameLogic;

public abstract class FortificationBuilderBase : IPlanetAction
{
    protected readonly HeroPlanetRelation Relation;
    
    protected FortificationBuilderBase(HeroPlanetRelation relation)
    {
        if (relation.Planet is null)
            throw new ArgumentException("Relation must be with planet");
        if (relation.Hero is null)
            throw new ArgumentException("Relation must be with not null hero");
        
        Relation = relation;
    }
    
    public virtual Task<ServiceResult<PlanetActionResult>> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (Relation.IterationsLeftToTheNextStatus == 1)
        {
            Relation.FortificationLevel = GetNextFortificationLevel();
            Relation.IterationsLeftToTheNextStatus = 1;
        }
        else
        {
            Relation.IterationsLeftToTheNextStatus -= 1;
        }
        
        var result = new PlanetActionResult(Relation.Status, Relation.FortificationLevel, Relation.PlanetId, 
            Relation.Hero.AvailableResearchShips, Relation.Hero.AvailableColonizationShips, Relation.Hero.Resourses, 
            Relation.IterationsLeftToTheNextStatus);
        return Task.FromResult(new ServiceResult<PlanetActionResult>(result));
    }

    public abstract Fortification GetNextFortificationLevel();
}