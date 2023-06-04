using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain.GameLogic;

public class FortificationFirstLevelBuilder : FortificationBuilderBase
{
    public FortificationFirstLevelBuilder(HeroPlanetRelation relation) : base(relation)
    {
    }

    protected override Fortification GetNextFortificationLevel()
    {
        return Fortification.Weak;
    }
}