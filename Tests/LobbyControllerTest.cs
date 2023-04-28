using Microsoft.AspNetCore.SignalR.Client;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;
using SharedLibrary.Requests;
using System.Data.Common;
using System.Net.Http.Json;
using Xunit;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Tests
{
	public class LobbyControllerTest : BaseTest
	{
		public LobbyControllerTest()
		{
			
		}

		[Fact]
		public async Task CreateLobbyTest()
		{
			var client = RegisterClient("user1", "test1@gmail.com", "123456789");
			var result =  await client.PostAsJsonAsync($"{_baseUrl}/Lobby", new CreateLobbyRequest { LobbyName = "Lobby1", MaxUsersCount = 4 });
			Assert.True(result.IsSuccessStatusCode);
		}

		[Fact]
		public async Task GetLobbiesTest()
		{
			var client1 = RegisterClient("user2", "test2@gmail.com", "123456789");
			var client2 = RegisterClient("user3", "test3@gmail.com", "123456789");

			var result1 = await client1.PostAsJsonAsync($"{_baseUrl}/Lobby", new CreateLobbyRequest { LobbyName = "Lobby2", MaxUsersCount = 4 });
			var result2 = await client2.GetAsync($"{_baseUrl}/Lobby");

			Assert.True(result2.IsSuccessStatusCode);
		}

		[Fact]
		public async Task GetLobbyByIdTest()
		{
			var client1 = RegisterClient("user4", "test4@gmail.com", "123456789");
			var client2 = RegisterClient("user5", "test5@gmail.com", "123456789");

			var postLobbyResult = await client1.PostAsJsonAsync($"{_baseUrl}/Lobby", new CreateLobbyRequest { LobbyName = "Lobby3", MaxUsersCount = 4 });
			var response = await client2.GetAsync(postLobbyResult.Headers.Location.ToString().Replace("Hero", "Lobby"));
			
			Assert.True(response.IsSuccessStatusCode, response.StatusCode.ToString());
		}

		[Fact]
		public async Task ConnectToLobbyTest()
		{
			var client1 = RegisterClient("user6", "test6@gmail.com", "123456789");
			var client2 = RegisterClient("user7", "test7@gmail.com", "123456789");

			var lobbyClient1 = new LobbyClient(_appFactory, client1);
			var lobbyClient2 = new LobbyClient(_appFactory, client2);
			
			var postLobbyResult = await client1.PostAsJsonAsync($"{_baseUrl}/Lobby", new CreateLobbyRequest { LobbyName = "Lobby4", MaxUsersCount = 4 });

			string path = postLobbyResult.Headers.Location.ToString();
			int pos = path.LastIndexOf("/") + 1;
			lobbyClient2.ConnectToLobby(new Guid(path.Substring(pos, path.Length - pos)));

			Thread.Sleep(5000);
			Assert.NotNull(lobbyClient2.Lobby);
		}

		[Fact]
		public async Task ExitFromLobbyTest()
		{
			var client1 = RegisterClient("user8", "test8@gmail.com", "123456789");
			var client2 = RegisterClient("user9", "test9@gmail.com", "123456789");

			var lobbyClient1 = new LobbyClient(_appFactory, client1);
			var lobbyClient2 = new LobbyClient(_appFactory, client2);

			var postLobbyResult =  await client1.PostAsJsonAsync($"{_baseUrl}/Lobby", new CreateLobbyRequest { LobbyName = "Lobby5", MaxUsersCount = 4 });

			string path = postLobbyResult.Headers.Location.ToString();
			int pos = path.LastIndexOf("/") + 1;
			lobbyClient2.ConnectToLobby(new Guid(path.Substring(pos, path.Length - pos)));
			Thread.Sleep(5000);
			lobbyClient2.ExitFromLobby(new Guid(path.Substring(pos, path.Length - pos)));
			Thread.Sleep(10000);
			Assert.Null(lobbyClient2.Lobby);
		}

		[Fact]
		public async Task CreateSessionTest()
		{
			var client1 = RegisterClient("user10", "test10@gmail.com", "123456789");
			var lobbyClient1 = new LobbyClient(_appFactory, client1);

			var postLobbyResult = await client1.PostAsJsonAsync($"{_baseUrl}/Lobby", new CreateLobbyRequest { LobbyName = "Lobby6", MaxUsersCount = 4 });
			Thread.Sleep(5000);

			string path = postLobbyResult.Headers.Location.ToString();
			int pos = path.LastIndexOf("/") + 1;
			
			lobbyClient1.ConnectToLobby(new Guid(path.Substring(pos, path.Length - pos)));
			Thread.Sleep(5000);

			Assert.Null(lobbyClient1.Hero);
		}
	}
}

