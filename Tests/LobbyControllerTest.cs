using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;
using SharedLibrary.Requests;
using System.Net.Http.Json;
using Xunit;

namespace Tests
{
	public class LobbyControllerTest : BaseTest
	{
		const int port = 7148;
		const string hubName = "lobby";
		string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjUiLCJoZXJvIjoibnVsbCIsIm5iZiI6MTY4MjQyOTU4NywiZXhwIjoxOTk4MDQ4Nzg3LCJpYXQiOjE2ODI0Mjk1ODd9.LfSP4PpvU8uGIsxV5BqnZRRaByZBvGwFt6rhoRXTvFQ";

		Lobby? currentLobby = null;
		//HubConnection? connection;

		public LobbyControllerTest()
		{
			Register("R1az", "test21@gmail.com", "ZfrwRgfs2");

			//connection = new HubConnectionBuilder()
			//	.WithUrl($"https://localhost:{port}/hubs/{hubName}", options => { options.AccessTokenProvider = () => Task.FromResult(accessToken); })
			//	.Build();
		}

		[Fact]
		public async Task CreateLobbyTest()
		{
			var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/Lobby", new CreateLobbyRequest { LobbyName = "Lobby1", MaxUsersCount = 4 });
			Assert.True(response.IsSuccessStatusCode);
		}

		[Fact]
		public async Task GetLobbiesTest()
		{
			var response = await _httpClient.GetAsync($"{_baseUrl}/Lobby");
			Assert.True(response.IsSuccessStatusCode, response.StatusCode.ToString());
		}

		[Fact]
		public async Task GetLobbyById()
		{
			var postLobbyResponse = await _httpClient.PostAsJsonAsync($"{_baseUrl}/Lobby", new CreateLobbyRequest { LobbyName = "Lobby1", MaxUsersCount = 4 });
			var response = await _httpClient.GetAsync(postLobbyResponse.Headers.Location.ToString().Replace("Hero", "Lobby"));
			Assert.True(response.IsSuccessStatusCode, response.StatusCode.ToString());
		}

		[Fact]
		public async Task SignalRTest()
		{
			var connection = new HubConnectionBuilder()
				.WithUrl($"https://localhost:{port}/hubs/{hubName}", options =>
				{
					options.AccessTokenProvider = () => Task.FromResult(accessToken);
				})
				.WithAutomaticReconnect()
				.Build();

			connection.On<Lobby>(ClientHandlers.Lobby.ConnectToLobbyHandler, (lobby) =>
			{
				currentLobby = lobby;
			});

			await connection.StartAsync();

			var postLobbyResponse = await _httpClient.PostAsJsonAsync($"{_baseUrl}/Lobby", new CreateLobbyRequest { LobbyName = "Lobby1", MaxUsersCount = 4 });

			string path = postLobbyResponse.Headers.Location.ToString();
			int pos = path.LastIndexOf("/") + 1;
			await connection.InvokeAsync(ServerHandlers.Lobby.ConnectToLobby, new Guid(path.Substring(pos, path.Length - pos)));

			while (true)
			{
				if (currentLobby != null)
				{
					Assert.Equal("Lobby1", currentLobby.LobbyName);
					break;
				}
			}
		}
	}
}