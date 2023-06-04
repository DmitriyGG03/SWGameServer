using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain.GameLogic;

public class FortificationSecondLevelBuilder : FortificationBuilderBase
{
    public FortificationSecondLevelBuilder(HeroPlanetRelation relation, Hero hero) : base(relation, hero)
    {
    }

    protected override Fortification GetNextFortificationLevel()
    {
        return Fortification.Reliable;
    }
}