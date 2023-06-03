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

		[Fact]
		public void RegisterTest()
		{
			Assert.True(_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
		}

		[Fact]
		public void LoginTest()
		{
			string email = GenerateEmail();
			Assert.True(_lobbyClient1.RegisterClient(GenerateUserName(), email, "123456789"));
			_lobbyClient1.LogOut();
			Assert.True(_lobbyClient1.LoginClient(email, "123456789"));
		}

		[Fact]
		public void CreateConnectionTest()
		{
			_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789");
			Assert.True(_lobbyClient1.CreateConnection());
		}

		[Fact]
		public void CreateLobbyTest()
		{
			Assert.True(_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient1.CreateLobby(GenerateLobbyName(), 2));
		}

		[Fact]
		public void GetLobbiesTest()
		{
			Assert.True(_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient1.CreateLobby(GenerateLobbyName(), 2));
			Assert.True(_lobbyClient2.GetLobbies());
			Assert.NotEmpty(_lobbyClient2.LobbiesList);
		}

		[Fact]
		public void ConnectToLobbyTest()
		{
			Assert.True(_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient1.CreateLobby(GenerateLobbyName(), 2));

			Assert.True(_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient2.GetLobbies());
			Assert.True(_lobbyClient2.CreateConnection());
			Assert.True(_lobbyClient2.ConnectToLobby(_lobbyClient2.LobbiesList.Last().Id));

			Thread.Sleep(5000);
			Assert.Equal(_lobbyClient1.ConnectedLobby.Id, _lobbyClient2.ConnectedLobby.Id);
		}

		[Fact]
		public void ExitFromLobbyTest()
		{
			Assert.True(_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient1.CreateConnection());
			Assert.True(_lobbyClient1.CreateLobby(GenerateLobbyName(), 2));

			Assert.True(_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient2.CreateConnection());
			Assert.True(_lobbyClient2.GetLobbies());
			Assert.True(_lobbyClient2.ConnectToLobby(_lobbyClient2.LobbiesList.Last().Id));
			Assert.True(_lobbyClient2.ExitFromLobby(_lobbyClient2.LobbiesList.Last().Id));

			Thread.Sleep(5000);
			Assert.Equal(1, _lobbyClient1.ConnectedLobby.LobbyInfos.Count);
		}

		[Fact]
		public void AutoDeleteLobbyTest()
		{
			Assert.True(_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient1.CreateConnection());
			Assert.True(_lobbyClient1.CreateLobby(GenerateLobbyName(), 2));

			Assert.True(_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient2.CreateConnection());

			Assert.True(_lobbyClient2.GetLobbies());
			int n = _lobbyClient2.LobbiesList.Count;

			Assert.True(_lobbyClient1.ExitFromLobby(_lobbyClient1.ConnectedLobby.Id));
			Thread.Sleep(5000);

			Assert.True(_lobbyClient2.GetLobbies());
			Assert.Equal(n - 1, _lobbyClient2.LobbiesList.Count);
		}

		[Fact]
		public void CreateSessionTest()
		{
			Assert.True(_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient1.CreateConnection());
			Assert.True(_lobbyClient1.CreateLobby("Lobb5", 2));

			Assert.True(_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient2.CreateConnection());
			Assert.True(_lobbyClient2.GetLobbies());
			Assert.True(_lobbyClient2.ConnectToLobby(_lobbyClient2.LobbiesList.Last().Id));

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.ChangeReadyStatus());
			Assert.True(_lobbyClient2.ChangeReadyStatus());

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.CreateSession());

			Thread.Sleep(5000);

			Assert.NotNull(_lobbyClient1.Session);
			Assert.NotNull(_lobbyClient2.Session);
		}

		[Fact]
		public void ChangeReadyStatusTest()
		{
			Assert.True(_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient1.CreateConnection());
			Assert.True(_lobbyClient1.CreateLobby(GenerateLobbyName(), 2));

			Assert.True(_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient2.CreateConnection());
			Assert.True(_lobbyClient2.GetLobbies());
			Assert.True(_lobbyClient2.ConnectToLobby(_lobbyClient2.LobbiesList.Last().Id));

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.ChangeReadyStatus());
			Assert.True(_lobbyClient2.ChangeReadyStatus());

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.ConnectedLobby.LobbyInfos.ToArray()[0].Ready);
			Assert.True(_lobbyClient1.ConnectedLobby.LobbyInfos.ToArray()[1].Ready);

			Assert.True(_lobbyClient2.ConnectedLobby.LobbyInfos.ToArray()[0].Ready);
			Assert.True(_lobbyClient2.ConnectedLobby.LobbyInfos.ToArray()[1].Ready);
		}

		[Fact]
		public void ChangeLobbyDataTest()
		{
			Assert.True(_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient1.CreateConnection());
			Assert.True(_lobbyClient1.CreateLobby(GenerateLobbyName(), 2));

			Assert.True(_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient2.CreateConnection());
			Assert.True(_lobbyClient2.GetLobbies());
			Assert.True(_lobbyClient2.ConnectToLobby(_lobbyClient2.LobbiesList.Last().Id));

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.ChangeLobbyData(new Lobby()
			{
				LobbyName = "New Name",
				MaxHeroNumbers = 4
			}));
			Thread.Sleep(5000);

			Assert.Equal("New Name", _lobbyClient2.ConnectedLobby.LobbyName);
			Assert.Equal(4, _lobbyClient2.ConnectedLobby.MaxHeroNumbers);
		}

		[Fact]
		public void GetHeroTest()
		{
			Assert.True(_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient1.CreateConnection());
			Assert.True(_lobbyClient1.CreateLobby(GenerateLobbyName(), 2));

			Assert.True(_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient2.CreateConnection());
			Assert.True(_lobbyClient2.GetLobbies());
			Assert.True(_lobbyClient2.ConnectToLobby(_lobbyClient2.LobbiesList.Last().Id));

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.ChangeReadyStatus());
			Assert.True(_lobbyClient2.ChangeReadyStatus());

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.CreateSession());

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.GetHero(_lobbyClient1.Session.Heroes.ToList()[0].HeroId));

		}

		[Fact]
		public void GetHeroMapView()
		{
			Assert.True(_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient1.CreateConnection());
			Assert.True(_lobbyClient1.CreateLobby(GenerateLobbyName(), 2));

			Assert.True(_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient2.CreateConnection());
			Assert.True(_lobbyClient2.GetLobbies());
			Assert.True(_lobbyClient2.ConnectToLobby(_lobbyClient2.LobbiesList.Last().Id));

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.ChangeReadyStatus());
			Assert.True(_lobbyClient2.ChangeReadyStatus());

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.CreateSession());

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.GetHero(_lobbyClient1.Session.Heroes.ToList()[0].HeroId));

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.GetHeroMapView(_lobbyClient1.Hero.HeroId));

			Assert.NotNull(_lobbyClient1.HeroMapView);
		}

		[Fact]
		public void ResearchPlanetTest()
		{
			Assert.True(_lobbyClient1.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient1.CreateConnection());
			Assert.True(_lobbyClient1.CreateLobby(GenerateLobbyName(), 2));

			Assert.True(_lobbyClient2.RegisterClient(GenerateUserName(), GenerateEmail(), "123456789"));
			Assert.True(_lobbyClient2.CreateConnection());
			Assert.True(_lobbyClient2.GetLobbies());
			Assert.True(_lobbyClient2.ConnectToLobby(_lobbyClient2.LobbiesList.Last().Id));

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.ChangeReadyStatus());
			Assert.True(_lobbyClient2.ChangeReadyStatus());

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.CreateSession());

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.GetHero(_lobbyClient1.Session.Heroes.ToList()[0].HeroId));

			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.GetHeroMapView(_lobbyClient1.Hero.HeroId));
			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.ReasearchPlanet(_lobbyClient1.HeroMapView.Planets.First().Id));
			Thread.Sleep(5000);

			Assert.True(_lobbyClient1.HeroMapView.Planets.First().Status >= PlanetStatus.Researching);
		}
	}
}