using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain.GameLogic;

public class FortificationThirdLevelBuilder : FortificationBuilderBase
{
    public FortificationThirdLevelBuilder(HeroPlanetRelation relation) : base(relation)
    {
    }

    protected override Fortification GetNextFortificationLevel()
    {
        return Fortification.Strong;
    }

    protected override int CalculatePrice()
    {
        return 300;
    }
}