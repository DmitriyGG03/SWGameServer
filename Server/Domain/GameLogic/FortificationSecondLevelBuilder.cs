using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain.GameLogic;

public class FortificationSecondLevelBuilder : FortificationBuilderBase
{
    public FortificationSecondLevelBuilder(HeroPlanetRelation relation) : base(relation)
    {
    }

    protected override Fortification GetNextFortificationLevel()
    {
        return Fortification.Reliable;
    }

    protected override int CalculatePrice()
    {
        return 150;
    }
}