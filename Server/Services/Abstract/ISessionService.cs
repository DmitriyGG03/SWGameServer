using Microsoft.AspNetCore.Mvc;
using Server.Domain;
using SharedLibrary.Models;

namespace Server.Services.Abstract
{

    public interface ISessionService
    {
        Task<ServiceResult<Session>> CreateAsync(Guid lobbyId, CancellationToken cancellationToken);
        
    }
}
