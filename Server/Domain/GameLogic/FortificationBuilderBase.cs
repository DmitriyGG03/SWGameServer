using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain.GameLogic;

public abstract class FortificationBuilderBase : IPlanetAction
{
    protected readonly HeroPlanetRelation Relation;
    
    protected FortificationBuilderBase(HeroPlanetRelation relation)
    {
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

        return Task.FromResult(new ServiceResult<PlanetActionResult>(new PlanetActionResult(Relation.Status, Relation.FortificationLevel,
            Relation.IterationsLeftToTheNextStatus)));
    }

    public abstract Fortification GetNextFortificationLevel();
}