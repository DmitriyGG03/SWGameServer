using SharedLibrary.Models;
using Xunit;

namespace Tests
{
	public class LobbyControllerTest : BaseTest
	{
		LobbyClient _lobbyClient1;
		LobbyClient _lobbyClient2;

		public LobbyControllerTest() : base()
		{
			_lobbyClient1 = new LobbyClient(_appFactory);
			_lobbyClient2 = new LobbyClient(_appFactory);
		}

		[Fact]
		public void RegisterTest()
		{
			Assert.True(_lobbyClient1.RegisterClient("user1", "test1@gmail.com", "123456789"));
		}

		[Fact]
		public void LoginTest()
		{
			Assert.True(_lobbyClient1.RegisterClient("user2", "test2@gmail.com", "123456789"));
			_lobbyClient1.LogOut();
			Assert.True(_lobbyClient1.LoginClient("test2@gmail.com", "123456789"));
		}

		[Fact]
		public void CreateConnectionTest()
		{
			_lobbyClient1.RegisterClient("user12", "test12@gmail.com", "123456789");
			Assert.True(_lobbyClient1.CreateConnection());
		}

		[Fact]
		public void CreateLobbyTest()
		{
			Assert.True(_lobbyClient1.RegisterClient("user3", "test3@gmail.com", "123456789"));
			Assert.True(_lobbyClient1.CreateLobby("Lobby1", 2));
		}

		[Fact]
		public void GetLobbiesTest()
		{
			Assert.True(_lobbyClient1.RegisterClient("user4", "test4@gmail.com", "123456789"));
			Assert.True(_lobbyClient2.RegisterClient("user5", "test5@gmail.com", "123456789"));
			Assert.True(_lobbyClient1.CreateLobby("Lobby2", 2));
			Assert.True(_lobbyClient2.GetLobbies());
			Assert.NotEmpty(_lobbyClient2.Lobbies);
		}

		[Fact]
		public void ConnectToLobbyTest()
		{
			Assert.True(_lobbyClient1.RegisterClient("user6", "test6@gmail.com", "123456789"));
			Assert.True(_lobbyClient1.CreateLobby("Lobb3", 2));

			Assert.True(_lobbyClient2.RegisterClient("user7", "test7@gmail.com", "123456789"));
			Assert.True(_lobbyClient2.GetLobbies());
			Assert.True(_lobbyClient2.CreateConnection());
			Assert.True(_lobbyClient2.ConnectToLobby(_lobbyClient2.Lobbies.Last().Id));

			Thread.Sleep(5000);
			Assert.Equal(_lobbyClient1._ConnectedLobby.Id, _lobbyClient2._ConnectedLobby.Id);
		}

		[Fact]
		public void ExitFromLobbyTest()
		{
			Assert.True(_lobbyClient1.RegisterClient("user8", "test8@gmail.com", "123456789"));
			Assert.True(_lobbyClient1.CreateConnection());
			Assert.True(_lobbyClient1.CreateLobby("Lobb4", 2));

			Assert.True(_lobbyClient2.RegisterClient("user9", "test9@gmail.com", "123456789"));
			Assert.True(_lobbyClient2.CreateConnection());
			Assert.True(_lobbyClient2.GetLobbies());
			Assert.True(_lobbyClient2.ConnectToLobby(_lobbyClient2.Lobbies.Last().Id));
			Assert.True(_lobbyClient2.ExitFromLobby(_lobbyClient2.Lobbies.Last().Id));

			Thread.Sleep(5000);
			Assert.Equal(1, _lobbyClient1._ConnectedLobby.LobbyInfos.Count);
		}

		[Fact]
		public void AutoDeleteLobbyTest()
		{
			Assert.True(_lobbyClient1.RegisterClient("user10", "test10@gmail.com", "123456789"));
			Assert.True(_lobbyClient1.CreateConnection());
			Assert.True(_lobbyClient1.CreateLobby("Lobb5", 2));

			Assert.True(_lobbyClient2.RegisterClient("user11", "test11@gmail.com", "123456789"));
			Assert.True(_lobbyClient2.CreateConnection());

			Assert.True(_lobbyClient2.GetLobbies());
			int n = _lobbyClient2.Lobbies.Count;

			Assert.True(_lobbyClient1.ExitFromLobby(_lobbyClient1._ConnectedLobby.Id));
			Thread.Sleep(5000);

			Assert.True(_lobbyClient2.GetLobbies());
			Assert.Equal(n - 1, _lobbyClient2.Lobbies.Count);
		}
	}
}