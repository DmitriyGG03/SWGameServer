using Microsoft.AspNetCore.Mvc;
using Server.Domain;
using SharedLibrary.Models;

namespace Server.Services.Abstract
{

    public interface ISessionService
    {
        Task<ServiceResult<Session>> Create(Guid lobbyId);
        
    }
}
