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
        /// <summary>
        /// Performs research or colonization actions on a planet asynchronously.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <param name="planetId">The unique identifier of the planet.</param>
        /// <param name="heroId">The unique identifier of the hero.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns></returns>
        Task<ServiceResult<MessageContainer>> ResearchOrColonizePlanetAsync(Guid sessionId, Guid planetId, Guid heroId,
            CancellationToken cancellationToken);
        /// <summary>
        /// Retrieves the map associated with a hero asynchronously.
        /// </summary>
        /// <param name="heroId">The unique identifier of the hero.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns></returns>
        Task<HeroMapView?> GetHeroMapAsync(Guid heroId, CancellationToken cancellationToken);
        /// <summary>
        /// Retrieves the user IDs associated with hero IDs for a given session asynchronously.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns></returns>
        Task<ServiceResult<Dictionary<Guid, Guid>>> GetUserIdWithHeroIdBySessionId(Guid sessionId,
            CancellationToken cancellationToken);
        /// <summary>
        /// Advances the game to the next turn asynchronously.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <param name="heroId">The unique identifier of the hero.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns></returns>
        Task<ServiceResult<Session>> MakeNextTurnAsync(Guid sessionId, Guid heroId, CancellationToken cancellationToken);
    }
}
