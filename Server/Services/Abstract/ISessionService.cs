﻿using Server.Domain;
using SharedLibrary.Models;

namespace Server.Services.Abstract
{

    public interface ISessionService
    {
        Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken);

        Task<bool> IsHeroTurn(Guid sessionId, Guid heroId, CancellationToken cancellationToken);
        
        Task<ServiceResult<Session>> CreateAsync(Guid lobbyId, CancellationToken cancellationToken);

        Task<int> UpdateSessionAsync(Session designation, CancellationToken cancellationToken);
        
        Task<ServiceResult<Dictionary<Guid, Guid>>> GetUserIdWithHeroIdBySessionIdAsync(Guid sessionId,
            CancellationToken cancellationToken);
    }
}
