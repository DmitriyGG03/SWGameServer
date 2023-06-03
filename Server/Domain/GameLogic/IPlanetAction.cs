namespace Server.Domain.GameLogic;

public interface IPlanetAction
{
    Task<ServiceResult<PlanetActionResult>> ExecuteAsync(CancellationToken cancellationToken);
}