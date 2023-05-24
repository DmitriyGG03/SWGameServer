using Microsoft.AspNetCore.Mvc;
using Server.Domain;
using SharedLibrary.Models;

namespace Server.Services.Abstract
{

    public interface ISessionService
    {
        /// <summary>
        /// Create a new session based on existed lobby
        /// </summary>
        /// <param name="lobbyId">Lobby id based on which the session will be created</param>
        /// <param name="cancellationToken">Token to cancel operation</param>
        /// <returns>Service result with new created session</returns>
        Task<ServiceResult<Session>> CreateAsync(Guid lobbyId, CancellationToken cancellationToken);
        /// <summary>
        /// Get session from persistence storage by it's id
        /// </summary>
        /// <param name="sessionId">Session id based on which the session will be returned</param>
        /// <param name="cancellationToken">Token to cancel operation</param>
        /// <returns>Session that was found by given id or null</returns>
        Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken);

        Task<ServiceResult<MessageContainer>> ResearchOrColonizePlanetAsync(Guid sessionId, Guid planetId, Guid heroId,
            CancellationToken cancellationToken);

        Task<HeroMapView?> GetHeroMapAsync(Guid heroId, CancellationToken cancellationToken);

        Task<ServiceResult<Dictionary<Guid, Guid>>> GetUserIdWithHeroIdBySessionId(Guid sessionId,
            CancellationToken cancellationToken);

        Task<ServiceResult<Session>> MakeNextTurnAsync(Guid sessionId, Guid heroId, CancellationToken cancellationToken);
    }
}
