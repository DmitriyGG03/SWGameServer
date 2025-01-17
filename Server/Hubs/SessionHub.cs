using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Common.Constants;
using Server.Domain;
using Server.Domain.Exceptions;
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
    private readonly IHeroService _heroService;
    
    public SessionHub(ISessionService sessionService, ILogger<SessionHub> logger, CyclicDependencySolver cyclicDependencySolver, IHeroMapService heroMapService, IGameService gameService, IHeroService heroService)
    {
        _sessionService = sessionService;
        _logger = logger;
        _cyclicDependencySolver = cyclicDependencySolver;
        _heroMapService = heroMapService;
        _gameService = gameService;
        _heroService = heroService;
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
            ServiceResult<(Session session, bool nextTurn)> result =
                await _gameService.MakeNextTurnAsync(request.SessionId, request.HeroId, CancellationToken.None);

            if (result.Success == false)
            {
                await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler, result.ErrorMessage);
            }
            else
            {
                (Session session, bool nextTurn) = result.Value;

                if (nextTurn == true)
                {
                    await HandleNextTurnAndNotifyClients(session);
                }
                else
                {
                    _cyclicDependencySolver.Solve(session);
                    await this.Clients.All.SendAsync(ClientHandlers.Session.ReceiveSession, session);
                }
            }
        }
        catch (GameEndedException e)
        {
            if (e.Winner is null)
                throw new ArgumentException("GameEnded exception should contain winner");

            await NotifyGameEndAsync(e.Winner);
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

    [Authorize]
    public async Task GetHeroData(Guid heroId)
    {
        try
        {
            var hero = await _heroService.GetByIdAsync(heroId, CancellationToken.None);
            if (hero is null)
            {
                await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler, "There is no hero with given user id");
                return;
            }
            if (hero.Session is null)
            {
                throw new InvalidOperationException("You probably changed GetHeroByUserIdAsync method. Hero must be with session");
            }
            
            var session = hero.Session;
            
            List<Battle> sessionBattles = await _gameService.GetBattlesBySessionAsync(session, CancellationToken.None);
            var heroMap = await _heroMapService.GetHeroMapAsync(hero.HeroId, CancellationToken.None);
            _cyclicDependencySolver.Solve(session);
            var response = new NextTurnResponse
            {
                Hero = hero,
                Session = session,
                Battles = sessionBattles,
                HeroMapView = heroMap
            };

            await this.Clients.Caller.SendAsync(ClientHandlers.Session.GetHeroDataHandler, response);
        }
        catch (Exception e)
        {
            await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler, e.Message);
            _logger.LogError(e, e.Message);
        }
    }

    [Authorize]
    public async Task ExitFromSession(ExitFromSessionRequest request)
    {
        try
        {
            var exitFromSessionResult =
                await _sessionService.ExitFromSessionAsync(request.SessionId, request.HeroId, CancellationToken.None);
            if (exitFromSessionResult.Success == false)
            {
                await this.Clients.Caller.SendAsync(ClientHandlers.ErrorHandler, exitFromSessionResult.ErrorMessage);
                return;
            }
            else
            {
                var response = new ExitFromSessionResponse
                {
                    Hero = exitFromSessionResult.Value
                };

                _cyclicDependencySolver.Solve(response.Hero);
                await this.Clients.All.SendAsync(ClientHandlers.Session.ExitFromSessionHandler, response);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
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
        _cyclicDependencySolver.Solve(session);
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
            Fortification = result.FortificationLevel,
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
    
    private Guid GetUserIdFromContext()
    {
        var result = Guid.Empty;
        string? userId = this.Context.User?.FindFirst("id")?.Value;
        if (Guid.TryParse(userId, out result) == true)
        {
            return result;
        }
        else
        {
            _logger.LogError($"Can not resolve user id ({userId}), it's not guid type");
            throw new ArgumentException("Invalid Guid format");
        }
    }

    private async Task NotifyGameEndAsync(Hero winner)
    {
        if (winner.SessionId is null)
        {
            throw new ArgumentException("Session id in hero can not be null");
        }
        
        var session = await _sessionService.GetByIdAsync(winner.SessionId.Value, CancellationToken.None);
        if (session is null)
            throw new InvalidOperationException("Session can not be null. Something unexpected has occured");
        
        _cyclicDependencySolver.Solve(winner);
        var response = new GameEndedResponse
        {
            GameWinner = winner,
            CountOfTurns = session.TurnNumber
        };

        await this.Clients.All.SendAsync(ClientHandlers.Session.GameEnded, response);
    }
}