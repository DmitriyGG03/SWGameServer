using Server.Services;
using SharedLibrary.Models;

namespace Server.IntegrationTests
{
	public class LobbyTests
	{
		CustomWebApplicationFactory<Program> _factory;

		Client _lobbyClient1;
		Client _lobbyClient2;

		private static int _userNameCount = 0;
		private static int _emailCount = 0;
		private static int _lobbyNameCount = 0;

		private string GenerateUserName()
		{
			return $"user{_userNameCount++}";
		}
		private string GenerateEmail()
		{
			return $"user{_emailCount++}test@gmail.com";
		}
		private string GenerateLobbyName()
		{
			return $"Lobby{_lobbyNameCount++}";
		}

		public LobbyTests()
		{
			_factory = new CustomWebApplicationFactory<Program>();
		}

		[Fact]
		public void CreateLobbyWithValidCredentials_ReturnsLobby()
		{
			_lobbyClient1 = new Client(_factory);

			//Arrange
			_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");

			//Act
			_lobbyClient1.CreateLobby(GenerateLobbyName(), 2);

			//Assert
			Assert.NotNull(_lobbyClient1.ConnectedLobby);
		}

		[Fact]
		public void GetLobbiesTest_ReturnListOfLobbies()
		{
			_lobbyClient1 = new Client(_factory);
			_lobbyClient2 = new Client(_factory);

			//Arrange
			_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			_lobbyClient1.CreateLobby(GenerateLobbyName(), 2);

			//Act
			_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			_lobbyClient2.GetLobbies();

			//Assert
			Assert.NotNull(_lobbyClient2.LobbiesList);
		}

		[Fact]
		public async void ConnectToLobby_WithExistingId_ReturnsLobby()
		{
			_lobbyClient1 = new Client(_factory);
			_lobbyClient2 = new Client(_factory);

			//Arrange
			_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			_lobbyClient1.CreateLobby(GenerateLobbyName(), 2);

			_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			_lobbyClient2.GetLobbies();

			_lobbyClient2.CreateConnection();
			//Act
			await _lobbyClient2.ConnectToLobbyAsync(_lobbyClient2.LobbiesList.Last().Id);

			//Assert
			Assert.NotNull(_lobbyClient2.ConnectedLobby);
		}

		[Fact]
		public async void ConnectToLobby_WithNonExistentId_ReturnsError()
		{
			_lobbyClient1 = new Client(_factory);

			//Arrange
			_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			_lobbyClient1.CreateConnection();

			//Act
			await _lobbyClient1.ConnectToLobbyAsync(Guid.NewGuid());

			//Assert
			Assert.Null(_lobbyClient1.ConnectedLobby);
		}

		[Fact]
		public async void ExitFromLobby_ReturnsLobbyWithoutUser()
		{
			_lobbyClient1 = new Client(_factory);
			_lobbyClient2 = new Client(_factory);

			//Arrange
			_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			_lobbyClient1.CreateConnection();
			_lobbyClient1.CreateLobby(GenerateLobbyName(), 2);

			_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			_lobbyClient2.CreateConnection();
			_lobbyClient2.GetLobbies();
			_lobbyClient2.ConnectToLobbyAsync(_lobbyClient2.LobbiesList.Last().Id);

			//Act
			await _lobbyClient2.ExitFromLobbyAsync(_lobbyClient2.LobbiesList.Last().Id);

			//Assert
			Assert.Equal(1, _lobbyClient1.ConnectedLobby.LobbyInfos.Count);
		}

		[Fact]
		public async void ChangeUserReady_ReturnsLobbyWithNewUserData()
		{
			//Arrange
			_lobbyClient1 = new Client(_factory);
			_lobbyClient2 = new Client(_factory);

			_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			_lobbyClient1.CreateConnection();
			_lobbyClient1.CreateLobby(GenerateLobbyName(), 2);

			_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			_lobbyClient2.CreateConnection();
			_lobbyClient2.GetLobbies();
			await _lobbyClient2.ConnectToLobbyAsync(_lobbyClient2.LobbiesList.Last().Id);

			//Act
			await _lobbyClient1.ChangeReadyStatusAsync();
			await _lobbyClient2.ChangeReadyStatusAsync();

			//Assert
			Assert.True(_lobbyClient1.ConnectedLobby.LobbyInfos.ToArray()[0].Ready);
			Assert.True(_lobbyClient1.ConnectedLobby.LobbyInfos.ToArray()[1].Ready);

			Assert.True(_lobbyClient2.ConnectedLobby.LobbyInfos.ToArray()[0].Ready);
			Assert.True(_lobbyClient2.ConnectedLobby.LobbyInfos.ToArray()[1].Ready);
		}

		[Fact] 
		public async void CreateSession_ReturnSessionId()
		{
			//Arrange
			_lobbyClient1 = new Client(_factory);
			_lobbyClient2 = new Client(_factory);

			_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			_lobbyClient1.CreateConnection();
			_lobbyClient1.CreateLobby(GenerateLobbyName(), 2);

			_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			_lobbyClient2.CreateConnection();
			_lobbyClient2.GetLobbies();
			await _lobbyClient2.ConnectToLobbyAsync(_lobbyClient2.LobbiesList.Last().Id);
			await _lobbyClient1.ChangeReadyStatusAsync();
			await _lobbyClient2.ChangeReadyStatusAsync();

			//Act
			await _lobbyClient1.CreateSessionAsync();

			//Assert
			Assert.NotNull(_lobbyClient1.Session);
			Assert.NotNull(_lobbyClient2.Session);
		
		}

		[Fact]
		public async void ChangeLobbyData_ReturnLobbyWithNewData()
		{
			//Arrange
			_lobbyClient1 = new Client(_factory);
			_lobbyClient2 = new Client(_factory);

			_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			_lobbyClient1.CreateConnection();
			_lobbyClient1.CreateLobby(GenerateLobbyName(), 2);

			_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			_lobbyClient2.CreateConnection();
			_lobbyClient2.GetLobbies();
			await _lobbyClient2.ConnectToLobbyAsync(_lobbyClient2.LobbiesList.Last().Id);

			//Act
			await (_lobbyClient1.ChangeLobbyDataAsync(
				new Lobby()
				{
					LobbyName = "New Name",
					MaxHeroNumbers = 4
				}));

			//Assert
			Assert.Equal("New Name", _lobbyClient2.ConnectedLobby.LobbyName);
			Assert.Equal(4, _lobbyClient2.ConnectedLobby.MaxHeroNumbers);
		}

		[Fact]
		public async void MakeNewTurn_ReturnNewSessionWithNextHeroTurnId()
		{
			//Arrange
			_lobbyClient1 = new Client(_factory);
			_lobbyClient2 = new Client(_factory);

			_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			_lobbyClient1.CreateConnection();
			_lobbyClient1.CreateLobby(GenerateLobbyName(), 2);

			_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			_lobbyClient2.CreateConnection();
			_lobbyClient2.GetLobbies();
			await _lobbyClient2.ConnectToLobbyAsync(_lobbyClient2.LobbiesList.Last().Id);
			await _lobbyClient1.ChangeReadyStatusAsync();
			await _lobbyClient2.ChangeReadyStatusAsync();
			await _lobbyClient1.CreateSessionAsync();

			Assert.True(await _lobbyClient1.MakeNextTurn());
			Assert.True(await _lobbyClient2.MakeNextTurn());
			Assert.True(await _lobbyClient1.MakeNextTurn());
			Assert.True(await _lobbyClient2.MakeNextTurn());
			Assert.True(await _lobbyClient1.MakeNextTurn());
		}
	}
}
