using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain.GameLogic;

public class FortificationThirdLevelBuilder : FortificationBuilderBase
{
    public FortificationThirdLevelBuilder(HeroPlanetRelation relation) : base(relation)
    {
    }

    public override Fortification GetNextFortificationLevel()
    {
        return Fortification.Strong;
    }
}