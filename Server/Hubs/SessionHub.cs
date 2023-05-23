using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Common.Constants;
using Server.Services.Abstract;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Requests;

namespace Server.Hubs;

public class SessionHub : Hub
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<SessionHub> _logger;
    public SessionHub(ISessionService sessionService, ILogger<SessionHub> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }
    
    [Authorize]
    public async Task HealthCheck()
    {
        await this.Clients.Caller.SendAsync(ClientHandlers.Session.HealthCheckHandler, "session hub is online");
    }
    [Authorize]
    public async Task PostResearchOrColonizePlanet(ResearchColonizePlanetRequest request)
    {
        var result = await _sessionService.ResearchOrColonizePlanetAsync(request.SessionId, request.PlanetId, request.HeroId, 
            CancellationToken.None);
        if (result.Success == false)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.Session.PostResearchOrColonizeErrorHandler,
                result.Value.Message);
        }
        
        _logger.LogInformation($"Successfully done {nameof(PostResearchOrColonizePlanet)} method, result message: {result.Value.Message}");

        if (result.Value.Message == SuccessMessages.Session.StartedResearching)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.Session.ColonizedPlanet,
                result.Value.Message);
        }
        else if (result.Value.Message == SuccessMessages.Session.Researched)
        {
            // update hero map
            var heroMap = await _sessionService.GetHeroMapAsync(request.HeroId, CancellationToken.None);
            await this.Clients.Caller.SendAsync(ClientHandlers.Session.ResearchedPlanet, heroMap);
        }
        else if (result.Value.Message == SuccessMessages.Session.StartedColonization)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.Session.ColonizedPlanet,
                result.Value.Message);
        }
        else if (result.Value.Message == SuccessMessages.Session.Colonized)
        {
            // TODO: update hero map for every hero
            await this.Clients.Caller.SendAsync(ClientHandlers.Session.ColonizedPlanet,
                result.Value.Message);
        }
        else if (result.Value.Message.StartsWith(SuccessMessages.Session.IterationDone))
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.Session.IterationDone,
                result.Value.Message);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}