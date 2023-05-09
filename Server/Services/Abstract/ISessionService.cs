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
        
    }
}
