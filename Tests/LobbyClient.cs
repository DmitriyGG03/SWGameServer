using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;
using SharedLibrary.Requests;
using SharedLibrary.Responses;
using System.Net.Http.Json;


namespace Tests
{
	public class LobbyClient
	{
		private WebApplicationFactory<Program> _appFactory;
		private HttpClient _client;
		private HubConnection? _connectionLobby;
		private HubConnection? _connectionSession;

		public IList<Lobby>? LobbiesList { get; set; }
		public Lobby? ConnectedLobby { get; set; }
		public Session? Session { get; set; }
		public Hero? Hero { get; set; }
		public HeroMapView? HeroMapView { get; set; }

		public LobbyClient(WebApplicationFactory<Program> appFactory)
		{
			_appFactory = appFactory;
			_client = appFactory.CreateClient();
		}

		public bool RegisterClient(string username, string email, string password)
		{
			var response = _client.PostAsJsonAsync($"authentication/register",
				new RegistrationRequest { Username = username, Email = email, Password = password }).Result;

			if (response.IsSuccessStatusCode && response != null)
			{
				_client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response.Content.ReadFromJsonAsync<AuthenticationResponse>().Result.Token);
				return true;
			}

			return false;
		}

		public bool LoginClient(string email, string password)
		{
			var response = _client.PostAsJsonAsync($"authentication/login",
				new LoginRequest { Email = email, Password = password }).Result;

			if (response.IsSuccessStatusCode)
			{
				_client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response.Content.ReadFromJsonAsync<AuthenticationResponse>().Result.Token);
				return true;
			}

			return false;
		}

		public void LogOut()
		{
			_client.DefaultRequestHeaders.Authorization = null;
		}

		public bool CreateConnection()
		{
			if (_client.DefaultRequestHeaders.Authorization == null)
				return false;

			_connectionLobby = new HubConnectionBuilder().WithUrl(
				$"https://localhost:{7148}/hubs/lobby",
				options =>
				{
					options.HttpMessageHandlerFactory = _ => _appFactory.Server.CreateHandler();
					options.AccessTokenProvider = () => Task.FromResult(_client.DefaultRequestHeaders.Authorization.Parameter);
				}).WithAutomaticReconnect().Build();

			_connectionLobby.On<string>(ClientHandlers.Lobby.Error, (errorMessage) =>
			{
				var message = $"Server error: {errorMessage}";
			});

			_connectionLobby.On<Lobby>(ClientHandlers.Lobby.ConnectToLobbyHandler, (lobby) =>
			{
				ConnectedLobby = lobby;
			});

			_connectionLobby.On<Lobby>(ClientHandlers.Lobby.ExitFromLobbyHandler, (lobby) =>
			{
				ConnectedLobby = lobby;
			});

			_connectionLobby.On<LobbyInfo>(ClientHandlers.Lobby.ChangeReadyStatus, (lobby) =>
			{
				ConnectedLobby.LobbyInfos.FirstOrDefault((l) => l.Id == lobby.Id).Ready = lobby.Ready;
			});

			_connectionLobby.On<Lobby>(ClientHandlers.Lobby.ChangeLobbyDataHandler, (lobby) =>
			{
				ConnectedLobby = lobby;
			});

			_connectionLobby.On<Guid>(ClientHandlers.Lobby.CreatedSessionHandler, (id) =>
			{
				var result = _client.GetAsync($"/Session/{id}");
				if (result.Result.IsSuccessStatusCode)
				{
					Session = result.Result.Content.ReadFromJsonAsync<GetSessionResponse>().Result.Session;
				}
			});

			_connectionSession = new HubConnectionBuilder().WithUrl(
				$"https://localhost:{7148}/hubs/Session",
				options =>
				{
					options.HttpMessageHandlerFactory = _ => _appFactory.Server.CreateHandler();
					options.AccessTokenProvider = () => Task.FromResult(_client.DefaultRequestHeaders.Authorization.Parameter);
				}).WithAutomaticReconnect().Build();

			_connectionSession.On<HeroMapView>(ClientHandlers.Session.ResearchedPlanet, (heroMap) =>
			{
				HeroMapView = heroMap;
			});
			_connectionSession.On<string>(ClientHandlers.Session.ColonizedPlanet, (response) =>
			{
				 var message = $"Server response: {response}";

			});
			_connectionSession.On<string>(ClientHandlers.Session.IterationDone, (response) =>
			{
				var message = $"Server response: {response}";
			});
			_connectionSession.On<string>(ClientHandlers.Session.PostResearchOrColonizeErrorHandler, (response) =>
			{
				var message = $"Server response: {response}";
			});
			_connectionSession.On<string>(ClientHandlers.Session.HealthCheckHandler, (response) =>
			{
				var message = $"Server response: {response}";

			});

			_connectionLobby.StartAsync();
			_connectionSession.StartAsync();

			return true;
		}

		public bool CreateLobby(string lobbyName, byte maxUserCount)
		{
			if (_client.DefaultRequestHeaders.Authorization == null)
				return false;

			var result = _client.PostAsJsonAsync("Lobby", new CreateLobbyRequest(lobbyName, maxUserCount));
			if (result.Result.IsSuccessStatusCode)
			{
				ConnectedLobby = result.Result.Content.ReadFromJsonAsync<CreateLobbyResponse>().Result.Lobby;
				return true;
			}
			return false;
		}

		public bool GetLobbies()
		{
			if (_client.DefaultRequestHeaders.Authorization == null)
				return false;

			var result = _client.GetAsync($"/Lobby");
			if (result.Result.IsSuccessStatusCode)
			{
				LobbiesList = result.Result.Content.ReadFromJsonAsync<GetAllLobbiesResponse>().Result.Lobbies;
				return true;
			}
			return false;
		}

		public bool ConnectToLobby(Guid id)
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connectionLobby == null)
				return false;

			_connectionLobby.InvokeAsync(ServerHandlers.Lobby.ConnectToLobby, id);

			return true;
		}

		public bool ExitFromLobby(Guid id)
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connectionLobby == null)
				return false;

			_connectionLobby.InvokeAsync(ServerHandlers.Lobby.ExitFromLobby, id);
			return true;
		}

		public bool CreateSession()
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connectionLobby == null)
				return false;

			_connectionLobby.InvokeAsync(ServerHandlers.Lobby.CreateSession, ConnectedLobby);
			return true;
		}

		public bool ChangeReadyStatus()
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connectionLobby == null)
				return false;

			_connectionLobby.InvokeAsync(ServerHandlers.Lobby.ChangeReadyStatus, ConnectedLobby.Id);
			return true;
		}

		public bool ChangeLobbyData(Lobby lobby)
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connectionLobby == null)
				return false;

			_connectionLobby.InvokeAsync(ServerHandlers.Lobby.ChangeLobbyData, lobby);
			return true;
		}

		public bool GetHero(Guid id)
		{
			if (_client.DefaultRequestHeaders.Authorization == null)
				return false;

			var result = _client.GetAsync($"/Hero/{id}");
			if (result.Result.IsSuccessStatusCode)
			{
				Hero = result.Result.Content.ReadFromJsonAsync<GetHeroResponse>().Result.Hero;
				return true;
			}

			return false;
		}

		public bool GetHeroMapView(Guid id)
		{
			if (_client.DefaultRequestHeaders.Authorization == null)
				return false;

			var result = _client.GetAsync($"session/heromap/{id}");
			if (result.Result.IsSuccessStatusCode)
			{
				HeroMapView = result.Result.Content.ReadFromJsonAsync<HeroMapView>().Result;
				return true;
			}

			return false;
		}

		public bool ReasearchPlanet(Guid id)
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connectionSession == null)
				return false;

			var request = new ResearchColonizePlanetRequest
			{
				HeroId = Hero.HeroId,
				SessionId = Session.Id,
				PlanetId = id
			};
			_connectionSession.InvokeAsync(ServerHandlers.Session.PostResearchOrColonizePlanet, request);
			return true;
		}
	}
}
