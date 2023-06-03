using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Common.Constants;
using Server.Domain;
using Server.Domain.GameLogic;
using Server.Services;
using Server.Services.Abstract;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;
using SharedLibrary.Models.Enums;
using SharedLibrary.Requests;
using SharedLibrary.Responses;

namespace Server.Hubs;

public class SessionHub : Hub
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<SessionHub> _logger;
    private readonly CyclicDependencySolver _cyclicDependencySolver;
    private readonly IHeroMapService _heroMapService;
    private readonly IGameService _gameService;
    public SessionHub(ISessionService sessionService, ILogger<SessionHub> logger, CyclicDependencySolver cyclicDependencySolver, IHeroMapService heroMapService, IGameService gameService)
    {
        _sessionService = sessionService;
        _logger = logger;
        _cyclicDependencySolver = cyclicDependencySolver;
        _heroMapService = heroMapService;
        _gameService = gameService;
    }

    [Authorize]
    public async Task HealthCheck()
    {
        await this.Clients.Caller.SendAsync(ClientHandlers.Session.HealthCheckHandler, "session hub is online");
    }

    [Authorize]
    public async Task MakeNextTurn(NextTurnRequest request)
    {
        ServiceResult<Session> result = await _gameService.MakeNextTurnAsync(request.SessionId, request.HeroId, CancellationToken.None);
        await HandleSessionResultAndNotifyClients(result);
    }

    [Authorize]
    public async Task PostResearchOrColonizePlanet(UpdatePlanetStatusRequest request)
    {
        var planetActionResult = await _gameService
            .GetPlanetActionHandlerAsync(request.PlanetId, request.HeroId, CancellationToken.None);
        await HandlePlanetActionResultAndNotifyClients(planetActionResult, request);
    }

    [Authorize]
    public async Task BuildFortification(UpdatePlanetStatusRequest request)
    {
        var planetActionResult = await _gameService
            .GetPlanetActionHandlerAsync(request.PlanetId, request.HeroId, CancellationToken.None);
        await HandlePlanetActionResultAndNotifyClients(planetActionResult, request);
    }

    private async Task HandleSessionResultAndNotifyClients(ServiceResult<Session> result)
    {
        if (result.Success == false)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler, result.ErrorMessage);
        }
        else
        {
            var session = result.Value;
            _cyclicDependencySolver.Solve(session);
            await this.Clients.All.SendAsync(ClientHandlers.Session.ReceiveSession, session);
        }
    }

    private async Task HandlePlanetActionResultAndNotifyClients(ServiceResult<IPlanetAction?> planetActionResult, UpdatePlanetStatusRequest request)
    {
        if (planetActionResult.Success == false)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler, planetActionResult.ErrorMessage);
        }
        else
        {
            var turnResult = await _sessionService
                .IsHeroTurn(request.SessionId, request.HeroId, CancellationToken.None);
            if (turnResult == false)
            {
                await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler, ErrorMessages.Session.NotHeroTurn);
            }
            else
            {
                var result = await planetActionResult.Value.ExecuteAsync(CancellationToken.None);
                await _gameService.SaveChangesAsync(CancellationToken.None);
                await HandlePlanetActionResultAsync(result, request);
            }
        }
    }
    
    private async Task HandlePlanetActionResultAsync(ServiceResult<PlanetActionResult> result, UpdatePlanetStatusRequest request)
    {
        if (result.Success == false)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler,
                result.ErrorMessage);
        }
        else
        {
            _logger.LogInformation($"Successfully handled request, result message: {result.Value}");
            await HandleSuccessStatusesAsync(result, request);
        }
    }

    private async Task HandleSuccessStatusesAsync(ServiceResult<PlanetActionResult> result, UpdatePlanetStatusRequest request)
    {
        if (result.Value is null)
            throw new NullReferenceException("Somehow value is null. Result is not succeeded");
        
        if (result.Value.RelationStatus == PlanetStatus.Researching)
        {
            await NotifyStartResearchingAsync(result);
        }
        else if (result.Value.RelationStatus == PlanetStatus.Researched)
        {
            await NotifyResearchedPlanetAsync(request);
        }
        else if (result.Value.RelationStatus == PlanetStatus.Colonizing)
        {
            await NotifyColonizingAsync(result);
        }
        else if (result.Value.RelationStatus == PlanetStatus.Colonized)
        {
            await NotifyColonizedPlanetAsync(request);
        }
        else if (result.Value.FortificationLevel > Fortification.None)
        {
            await NotifyUpdatedFortificationStatusAsync(result);
        }
        else
        {
            await NotifyUnhandledStatusAsync();
        }
    }

    private async Task NotifyStartResearchingAsync(ServiceResult<PlanetActionResult> result)
    {
        await this.Clients.Caller.SendAsync(ClientHandlers.Session.ResearchingPlanet, GetUpdatedPlanetStatusResponse(result.Value));
    }
    
    private async Task NotifyResearchedPlanetAsync(UpdatePlanetStatusRequest request)
    {
        var heroMap = await _heroMapService.GetHeroMapAsync(request.HeroId, CancellationToken.None);
        await this.Clients.Caller.SendAsync(ClientHandlers.Session.ResearchedPlanet, heroMap);
    }

    private async Task NotifyColonizingAsync(ServiceResult<PlanetActionResult> result)
    {
        await this.Clients.Caller.SendAsync(ClientHandlers.Session.ColonizingPlanet, GetUpdatedPlanetStatusResponse(result.Value));
    }
    
    private async Task NotifyColonizedPlanetAsync(UpdatePlanetStatusRequest request)
    {
        var result = await _sessionService.GetUserIdWithHeroIdBySessionIdAsync(request.SessionId, CancellationToken.None);
        if (result.Success == false)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler, result.ErrorMessage);
        }            
        else
        {
            await SendHeroMapsToHeroes(result.Value);
        }
    }
    
    private async Task NotifyUpdatedFortificationStatusAsync(ServiceResult<PlanetActionResult> result)
    {
        await this.Clients.Caller.SendAsync(ClientHandlers.Session.UpdatedFortification, GetFortificationResponse(result.Value));
    }

    private async Task SendHeroMapsToHeroes(Dictionary<Guid, Guid> userIdWithHeroId)
    {
        foreach (var item in userIdWithHeroId)
        {
            var heroMap = await _heroMapService.GetHeroMapAsync(item.Value, CancellationToken.None);
            await this.Clients.User(item.Key.ToString()).SendAsync(ClientHandlers.Session.ReceiveHeroMap, heroMap);
        }
    }

    private async Task NotifyUnhandledStatusAsync()
    {
        await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler,
            "We are sorry but now we can not handle given status. Selected planet probably already colonized");
    }

    private UpdatedFortificationResponse GetFortificationResponse(PlanetActionResult result)
    {
        return new UpdatedFortificationResponse
        {
            Fortification = result.FortificationLevel, IterationsToTheNextStatus = result.IterationsToTheNextStatus
        };
    }
    private UpdatedPlanetStatusResponse GetUpdatedPlanetStatusResponse(PlanetActionResult result)
    {
        return new UpdatedPlanetStatusResponse { RelationStatus = result.RelationStatus, IterationsToTheNextStatus = result.IterationsToTheNextStatus};
    }
}