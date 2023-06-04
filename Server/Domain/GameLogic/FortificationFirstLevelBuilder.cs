using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain.GameLogic;

public class FortificationFirstLevelBuilder : FortificationBuilderBase
{
    public FortificationFirstLevelBuilder(HeroPlanetRelation relation, Hero hero) : base(relation, hero)
    {
    }

    protected override Fortification GetNextFortificationLevel()
    {
        return Fortification.Weak;
    }
}