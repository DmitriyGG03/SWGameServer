using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Services.Abstract;
using SharedLibrary.Requests;

namespace Server.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class LobbyController
{
    private readonly ILobbyService _lobbyService;
    public LobbyController(ILobbyService lobbyService)
    {
        _lobbyService = lobbyService;
    }

    public async Task<IActionResult> GetAllLobbiesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> GetLobbyByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> CreateLobbyAsync(CreateLobbyRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> DeleteLobbyIfThereAreNoUsers()
    {
        throw new NotImplementedException();
    }
}