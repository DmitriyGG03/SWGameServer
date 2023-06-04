using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain.GameLogic;

public class FortificationThirdLevelBuilder : FortificationBuilderBase
{
    public FortificationThirdLevelBuilder(HeroPlanetRelation relation, Hero hero) : base(relation, hero)
    {
    }

    protected override Fortification GetNextFortificationLevel()
    {
        return Fortification.Strong;
    }
}