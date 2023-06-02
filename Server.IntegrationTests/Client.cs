using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;
using SharedLibrary.Requests;
using SharedLibrary.Responses;
using System.Net.Http.Json;


namespace Server.IntegrationTests
{
	public class Client
	{
		private CustomWebApplicationFactory<Program> _appFactory;
		private HttpClient _client;
		private HubConnection? _connectionLobby;
		private HubConnection? _connectionSession;

		public Guid UserId { get; set; }
		public IList<Lobby>? LobbiesList { get; set; }
		public Lobby? ConnectedLobby { get; set; }
		public Session? Session { get; set; }
		public Hero? Hero { get; set; }
		public HeroMapView? HeroMapView { get; set; }


		public Client(CustomWebApplicationFactory<Program> appFactory)
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
				var authenticationResponse = response.Content.ReadFromJsonAsync<AuthenticationResponse>().Result;
				_client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationResponse.Token);
				UserId = authenticationResponse.UserId;
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
				var authenticationResponse = response.Content.ReadFromJsonAsync<AuthenticationResponse>().Result;
				_client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationResponse.Token);
				UserId = authenticationResponse.UserId;
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

			_connectionLobby.ServerTimeout = TimeSpan.FromSeconds(600);

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

			_connectionLobby.On<Lobby>(ClientHandlers.Lobby.ChangeLobbyDataHandler, (lobby) =>
			{
				ConnectedLobby = lobby;
			});

			_connectionLobby.On<LobbyInfo>(ClientHandlers.Lobby.ChangeReadyStatus, (LobbyInfo) =>
			{
				ConnectedLobby.LobbyInfos.FirstOrDefault((l) => l.Id == LobbyInfo.Id).Ready = LobbyInfo.Ready;
			});

			_connectionLobby.On<Guid>(ClientHandlers.Lobby.CreatedSessionHandler, (sessionId) =>
			{
				var result = _client.GetAsync($"/Session/{sessionId}");
				if (result.Result.IsSuccessStatusCode)
				{
					Session = result.Result.Content.ReadFromJsonAsync<GetSessionResponse>().Result.Session;
					Hero = Session.Heroes.First();
				}
			});

			_connectionSession = new HubConnectionBuilder().WithUrl(
			$"https://localhost:{7148}/hubs/Session",
			options =>
			{
				options.HttpMessageHandlerFactory = _ => _appFactory.Server.CreateHandler();
				options.AccessTokenProvider = () => Task.FromResult(_client.DefaultRequestHeaders.Authorization.Parameter);
			}).WithAutomaticReconnect().Build();

			_connectionSession.ServerTimeout = TimeSpan.FromSeconds(600);

			_connectionSession.On<Session>(ClientHandlers.Session.ReceiveSession, (session) =>
			{
				Session = session;
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


		public async Task<bool> ConnectToLobbyAsync(Guid id)
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connectionLobby == null)
				return false;

			var tcs = new TaskCompletionSource<bool>();

			_connectionLobby.On<Lobby>(ClientHandlers.Lobby.ConnectToLobbyHandler, (lobby) =>
			{
				ConnectedLobby = lobby;
				tcs.SetResult(true);
			});

			await _connectionLobby.InvokeAsync(ServerHandlers.Lobby.ConnectToLobby, id);

			var result = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(10)));

			if (result == tcs.Task)
				return true;

			return false;
		}

		public async Task<bool> ChangeLobbyDataAsync(Lobby lobby)
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connectionLobby == null)
				return false;

			var tcs = new TaskCompletionSource<bool>();

			_connectionLobby.On<Lobby>(ClientHandlers.Lobby.ChangeLobbyDataHandler, (lobby) =>
			{
				ConnectedLobby = lobby;
				tcs.SetResult(true);
			});

			await _connectionLobby.InvokeAsync(ServerHandlers.Lobby.ChangeLobbyData, lobby);

			var result = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(10)));

			if (result == tcs.Task)
				return true;

			return false;
		}

		public async Task<bool> ChangeReadyStatusAsync()
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connectionLobby == null)
				return false;

			var tcs = new TaskCompletionSource<bool>();

			_connectionLobby.On<LobbyInfo>(ClientHandlers.Lobby.ChangeReadyStatus, (LobbyInfo) =>
			{
				ConnectedLobby.LobbyInfos.FirstOrDefault((l) => l.Id == LobbyInfo.Id).Ready = LobbyInfo.Ready;
				tcs.TrySetResult(true);
			});

			await _connectionLobby.InvokeAsync(ServerHandlers.Lobby.ChangeReadyStatus, ConnectedLobby.Id);

			var result = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(10)));

			if (result == tcs.Task)
				return true;

			return false;
		}

		public async Task<bool> ExitFromLobbyAsync(Guid id)
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connectionLobby == null)
				return false;

			var tcs = new TaskCompletionSource<bool>();

			_connectionLobby.On<Lobby>(ClientHandlers.Lobby.ExitFromLobbyHandler, (lobby) =>
			{
				ConnectedLobby = lobby;
				tcs.SetResult(true);
			});

			await _connectionLobby.InvokeAsync(ServerHandlers.Lobby.ExitFromLobby, id);

			var result = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(10)));

			if (result == tcs.Task)
				return true;

			return false;
		}

		public async Task<bool> CreateSessionAsync()
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connectionLobby == null)
				return false;

			var tcs = new TaskCompletionSource<bool>();

			_connectionLobby.On<Guid>(ClientHandlers.Lobby.CreatedSessionHandler, (sessionId) =>
			{
				var result = _client.GetAsync($"/Session/{sessionId}");
				if (result.Result.IsSuccessStatusCode)
				{
					Session = result.Result.Content.ReadFromJsonAsync<GetSessionResponse>().Result.Session;
				}
				tcs.SetResult(true);
			});

			await _connectionLobby.InvokeAsync(ServerHandlers.Lobby.CreateSession, ConnectedLobby);

			var result = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(10)));

			if (result == tcs.Task)
				return true;

			return false;
		}

		public async Task<bool> MakeNextTurn()
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connectionSession == null)
				return false;

			var tcs = new TaskCompletionSource<bool>();

			_connectionSession.On<Session>(ClientHandlers.Session.ReceiveSession, (session) =>
			{
				Session = session;
				tcs.TrySetResult(true);
			});

			var userHero = Session.Heroes.First(h => h.UserId == UserId);
			await _connectionSession.InvokeAsync(ServerHandlers.Session.NextTurn, new NextTurnRequest() 
			{ 
				SessionId = Session.Id, 
				HeroId = userHero.HeroId
			});

			var result = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(10)));

			if (result == tcs.Task)
				return true;

			return false;
		}
	}
}