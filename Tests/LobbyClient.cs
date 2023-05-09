using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;
using SharedLibrary.Requests;
using SharedLibrary.Responses;
using System.Diagnostics;
using System.Net.Http.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Tests
{
	public class LobbyClient
	{
		private WebApplicationFactory<Program> _appFactory;
		private HttpClient _client;
		private HubConnection? _connection;

		public Lobby? ConnectedLobby { get; set; }
		public IList<Lobby>? Lobbies { get; set; }
		public Hero? Hero { get; set; }

		public LobbyClient(WebApplicationFactory<Program> appFactory)
		{
			_appFactory = appFactory;
			_client = appFactory.CreateClient();
		}

		public bool RegisterClient(string username, string email, string password)
		{
			var response = _client.PostAsJsonAsync($"authentication/register", 
				new RegistrationRequest { Username = username, Email = email, Password = password }).Result;

			if (response.IsSuccessStatusCode)
			{
				_client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response.Content.ReadFromJsonAsync<AuthenticationResponse>().Result.Token);
				return true;
			}
			return false;
		}

		public bool LoginClient(string email, string password)
		{
			var response = _client.PostAsJsonAsync($"authentication/login",
				new LoginRequest {Email = email, Password = password }).Result;

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

			_connection = new HubConnectionBuilder().WithUrl(
				$"https://localhost:{7148}/hubs/lobby",
				options =>
				{
					options.HttpMessageHandlerFactory = _ => _appFactory.Server.CreateHandler();
					options.AccessTokenProvider = () => Task.FromResult(_client.DefaultRequestHeaders.Authorization.Parameter);
				}).WithAutomaticReconnect().Build();

			_connection.On<string>(ClientHandlers.Lobby.Error, (errorMessage) =>
			{
				var message = $"Server error: {errorMessage}";
			});
			_connection.On<Lobby>(ClientHandlers.Lobby.ConnectToLobbyHandler, lobby =>
			{
				ConnectedLobby = lobby;
			});

			_connection.On<Lobby>(ClientHandlers.Lobby.ExitFromLobbyHandler, (lobby) =>
			{
				ConnectedLobby = lobby;
			});

			_connection.On<Lobby>(ClientHandlers.Lobby.ChangeReadyStatus, (lobby) =>
			{
				ConnectedLobby = lobby;
			});

			_connection.On<Lobby>(ClientHandlers.Lobby.ChangeLobbyDataHandler, (lobby) =>
			{
				ConnectedLobby = lobby;
			});

			_connection.On<Hero>(ClientHandlers.Lobby.CreatedSessionHandler, (hero) =>
			{
				Hero = hero;
			});
			_connection.StartAsync();

			return true;
		}

		public bool CreateLobby(string lobbyName, byte maxUserCount)
		{
			if (_client.DefaultRequestHeaders.Authorization == null)
				return false;

			var result = _client.PostAsJsonAsync("Lobby", new CreateLobbyRequest(lobbyName, maxUserCount));

			if(result.Result.IsSuccessStatusCode)
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
				Lobbies = result.Result.Content.ReadFromJsonAsync<GetAllLobbiesResponse>().Result.Lobbies;
				return true;
			}
			return false;
		}

		public bool ConnectToLobby(Guid id)
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connection == null)
				return false;

			_connection.InvokeAsync(ServerHandlers.Lobby.ConnectToLobby, id);

			return true;
		}

		public bool ExitFromLobby(Guid id)
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connection == null)
				return false;

			_connection.InvokeAsync(ServerHandlers.Lobby.ExitFromLobby, id);
			return true;
		}

		public bool CreateSession()
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connection == null)
				return false;

			_connection.InvokeAsync(ServerHandlers.Lobby.CreateSession, ConnectedLobby);
			return true;
		}

		public bool ChangeReadyStatus()
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connection == null)
				return false;

			_connection.InvokeAsync(ServerHandlers.Lobby.ChangeReadyStatus, ConnectedLobby.Id);
			return true;
		}

		public bool ChangeLobbyData(Lobby lobby)
		{
			if (_client.DefaultRequestHeaders.Authorization == null || _connection == null)
				return false;

			_connection.InvokeAsync(ServerHandlers.Lobby.ChangeLobbyData, lobby);
			return true;
		}
	}
}
