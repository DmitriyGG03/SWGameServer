using SharedLibrary.Models;
using SharedLibrary.Models.Enums;

namespace Server.Domain.GameLogic;

public class PlanetDoNothingAction : IPlanetAction
{
    public async Task<ServiceResult<PlanetActionResult>> ExecuteAsync(CancellationToken cancellationToken)
    {
        // do nothing
        await Task.Delay(1, cancellationToken);
        return new ServiceResult<PlanetActionResult>(new PlanetActionResult(PlanetStatus.Colonized, -1));
    }
}