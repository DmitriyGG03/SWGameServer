using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Common.Constants;
using Server.Domain;
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
        await HandlePostResearchOrColonizeAsync(result, request);
    }

    private async Task HandlePostResearchOrColonizeAsync(ServiceResult<MessageContainer> result, ResearchColonizePlanetRequest request)
    {
        if (result.Success == false)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.Session.PostResearchOrColonizeErrorHandler,
                result.ErrorMessage);
        }
        
        _logger.LogInformation($"Successfully done {nameof(PostResearchOrColonizePlanet)} method, result message: {result.Value.Message}");
        await HandleStatusesAsync(result, request);
    }

    private async Task HandleStatusesAsync(ServiceResult<MessageContainer> result, ResearchColonizePlanetRequest request)
    {
        if (result.Value.Message.StartsWith(SuccessMessages.Session.StartedResearching))
        {
            await NotifyStartResearchingAsync(result);
        }
        else if (result.Value.Message == SuccessMessages.Session.Researched)
        {
            await NotifyResearchedPlanetAsync(request);
        }
        else if (result.Value.Message.StartsWith(SuccessMessages.Session.StartedColonization))
        {
            await NotifyStartColonizationAsync(result);
        }
        else if (result.Value.Message == SuccessMessages.Session.Colonized)
        {
            await NotifyColonizedPlanetAsync(request);
        }
        else if (result.Value.Message.StartsWith(SuccessMessages.Session.IterationDone))
        {
            await NotifyIterationDoneAsync(result);
        }
        else
        {
            throw new InvalidOperationException("Can not handle given status");
        }
    }

    private async Task NotifyStartResearchingAsync(ServiceResult<MessageContainer> result)
    {
        await this.Clients.Caller.SendAsync(ClientHandlers.Session.StartedResearching,
            result.Value.Message);
    }

    private async Task NotifyStartColonizationAsync(ServiceResult<MessageContainer> result)
    {
        await this.Clients.Caller.SendAsync(ClientHandlers.Session.StartedColonizingPlanet,
            result.Value.Message);
    }
    
    private async Task NotifyResearchedPlanetAsync(ResearchColonizePlanetRequest request)
    {
        var heroMap = await _sessionService.GetHeroMapAsync(request.HeroId, CancellationToken.None);
        await this.Clients.Caller.SendAsync(ClientHandlers.Session.ResearchedPlanet, heroMap);
    }

    private async Task NotifyColonizedPlanetAsync(ResearchColonizePlanetRequest request)
    {
        var result = await _sessionService.GetUserIdWithHeroIdBySessionId(request.SessionId, CancellationToken.None);
        if (result.Success == false)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler, result.ErrorMessage);
        }            
        else
        {
            await SendHeroMapsToHeroes(result.Value);
        }
    }

    private async Task SendHeroMapsToHeroes(Dictionary<Guid, Guid> userIdWithHeroId)
    {
        foreach (var item in userIdWithHeroId)
        {
            var heroMap = await _sessionService.GetHeroMapAsync(item.Value, CancellationToken.None);
            await this.Clients.User(item.Key.ToString()).SendAsync(ClientHandlers.Session.ReceiveHeroMap, heroMap);
        }
    }

    private async Task NotifyIterationDoneAsync(ServiceResult<MessageContainer> result)
    {
        await this.Clients.Caller.SendAsync(ClientHandlers.Session.IterationDone,
            result.Value.Message);
    }
}