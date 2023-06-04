using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain.GameLogic;

public class PlanetDoNothingAction : IPlanetAction
{
    private readonly HeroPlanetRelation _relation;
    private readonly Hero _hero;
    private readonly Planet _planet;
    public PlanetDoNothingAction(HeroPlanetRelation relation)
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
        // do nothing
        await Task.Delay(1, cancellationToken);
        var result = new PlanetActionResult(_relation.Status, _relation.FortificationLevel, _relation.PlanetId, 
            _hero.AvailableResearchShips, _hero.AvailableColonizationShips, _hero.Resourses, 
            _relation.IterationsLeftToTheNextStatus);
        return new ServiceResult<PlanetActionResult>(result);
    }
}