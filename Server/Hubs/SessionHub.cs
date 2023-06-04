using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Common.Constants;
using Server.Domain;
using Server.Domain.GameLogic;
using Server.Services;
using Server.Services.Abstract;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;
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
        try
        {
            ServiceResult<(Session session, bool nextTurn)> result = await _gameService.MakeNextTurnAsync(request.SessionId, request.HeroId, CancellationToken.None);

            if (result.Success == false)
            {
                await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler, result.ErrorMessage);
            }
            else
            {
                (Session session, bool nextTurn) = result.Value;
                _cyclicDependencySolver.Solve(session);
            
                if(nextTurn == true)
                {
                    await HandleNextTurnAndNotifyClients(session);
                }            
                else
                {
                    await this.Clients.All.SendAsync(ClientHandlers.Session.ReceiveSession, session);
                }
            }
        }
        catch (Exception e)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.ServerInternalError, e.Message);
            _logger.LogError(e, e.Message);
        }
    }

    [Authorize]
    public async Task PostResearchOrColonizePlanet(UpdatePlanetStatusRequest request)
    {
        try
        {
            var planetActionResult = await _gameService
                .StartPlanetColonizationOrResearching(request.PlanetId, request.HeroId, CancellationToken.None);
            if (planetActionResult.Success == false)
            {
                await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler, planetActionResult.ErrorMessage);
            }
            else
            {
                var result = planetActionResult.Value;
                var response = GetUpdatedPlanetStatusResponse(result);
            
                await this.Clients.Caller.SendAsync(ClientHandlers.Session.StartPlanetResearchingOrColonization, response);
            }
        }
        catch (Exception e)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.ServerInternalError, e.Message);
            _logger.LogError(e, e.Message);
        }
    }

    [Authorize]
    public async Task BuildFortification(UpdatePlanetStatusRequest request)
    {
        try
        {
            var planetActionResult = await _gameService
                .GetPlanetActionHandlerAsync(request.PlanetId, request.HeroId, CancellationToken.None);
            await HandlePlanetActionResultAndNotifyClients(planetActionResult, request);
        }
        catch (Exception e)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.ServerInternalError, e.Message);
            _logger.LogError(e, e.Message);
        }
    }

    [Authorize]
    public async Task StartBattle(StartBattleRequest request)
    {
        try
        {
            var result = await _gameService.StartBattleAsync(request.HeroId, request.AttackedPlanetId,
                request.FromPlanetId, request.CountOfSoldiers, CancellationToken.None);

            await HandleBattleResult(result);
        }
        catch (Exception e)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.ServerInternalError, e.Message);
            _logger.LogError(e, e.Message);
        }
    }

    [Authorize]
    public async Task DefendPlanet(DefendPlanetRequest request)
    {
        try
        {
            var result = await _gameService.DefendPlanetAsync(request.HeroId, request.AttackedPlanetId,
                request.CountOfSoldiers, CancellationToken.None);

            await HandleBattleResult(result);
        }
        catch (Exception e)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.ServerInternalError, e.Message);
            _logger.LogError(e, e.Message);
        }
    }
    
    private async Task HandleBattleResult(ServiceResult<Battle> result)
    {
        if (result.Success == false)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler, result.ErrorMessage);
        }
        else
        {
            var battle = result.Value;
            _cyclicDependencySolver.Solve(result.Value);
            await this.Clients.All.SendAsync(ClientHandlers.Session.ReceiveBattle, battle);
        }
    }
    
    private async Task HandleNextTurnAndNotifyClients(Session session)
    {
        List<Battle> sessionBattles = await _gameService.GetBattlesBySessionAsync(session, CancellationToken.None);
        var response = new NextTurnResponse
        {
            Session = session,
            Battles = sessionBattles,
        };

        var userIdsWithHeroIds = _sessionService.GetUserIdWithHeroIdBySession(session);
        var heroes = session.Heroes;
        session.Heroes = null;
        foreach (var item in userIdsWithHeroIds)
        {
            var heroMap = await _heroMapService.GetHeroMapAsync(item.Value, CancellationToken.None);
            response.HeroMapView = heroMap;
            response.Hero = heroes.First(x => x.HeroId == item.Value);
            await this.Clients.User(item.Key.ToString()).SendAsync(ClientHandlers.Session.NextTurnHandler, response);
        }
    }

    private async Task HandlePlanetActionResultAndNotifyClients(ServiceResult<IPlanetAction?> planetAction, UpdatePlanetStatusRequest request)
    {
        if (planetAction.Success == false)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler, planetAction.ErrorMessage);
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
                var planetActionResult = await planetAction.Value.ExecuteAsync(CancellationToken.None);
                await _gameService.SaveChangesAsync(CancellationToken.None);

                if (planetActionResult.Success == false)
                {
                    await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler, ErrorMessages.Session.NotHeroTurn);
                }
                else
                {
                    var result = planetActionResult.Value;
                    var response = GetFortificationResponse(result);
                    
                    await this.Clients.Caller.SendAsync(ClientHandlers.Session.UpdatedFortification, response);
                }
            }
        }
    }

    private UpdatedFortificationResponse GetFortificationResponse(PlanetActionResult result)
    {
        return new UpdatedFortificationResponse
        {
            IterationsToTheNextStatus = result.IterationsToTheNextStatus,
            PlanetId = result.PlanetId,
            AvailableResearchShips = result.AvailableResearchShips,
            AvailableColonizationShips = result.AvailableColonizationShips,
            Resources = result.Resources
        };
    }
    
    private UpdatedPlanetStatusResponse GetUpdatedPlanetStatusResponse(PlanetActionResult result)
    {
        return new UpdatedPlanetStatusResponse
        {
            RelationStatus = result.RelationStatus,
            IterationsToTheNextStatus = result.IterationsToTheNextStatus,
            PlanetId = result.PlanetId,
            AvailableResearchShips = result.AvailableResearchShips,
            AvailableColonizationShips = result.AvailableColonizationShips,
            Resources = result.Resources
        };
    }
}