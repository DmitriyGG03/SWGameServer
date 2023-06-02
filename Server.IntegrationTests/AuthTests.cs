using Microsoft.Extensions.DependencyInjection;
using Server;
using Server.Helpers;
using Server.Services;
using SharedLibrary.Models;
using SharedLibrary.Requests;
using System.Net.Http.Json;


namespace Server.IntegrationTests
{
	public class AuthTests
	{
		CustomWebApplicationFactory<Program> _factory;
		HttpClient _client;
		HashProvider _hashProvider;


		public AuthTests()
		{
			_factory = new CustomWebApplicationFactory<Program>();
			_hashProvider = new HashProvider();
			using (var scope = _factory.Services.CreateScope())
			{
				var scopedServices = scope.ServiceProvider;
				var db = scopedServices.GetRequiredService<GameDbContext>();

				var user = new ApplicationUser
				{
					Username = "User1_Test",
					Email = "Test1@gmail.com",
					PasswordHash = "123456789"
				};
				user.ProvideSaltAndHash(_hashProvider);
				user.Heroes = null;

				db.Add(user);
				db.SaveChangesAsync();
			}
			_client = _factory.CreateClient();
		}

		[Fact]
		public async Task Login_WithValidCredentials_ReturnsSuccess()
		{
			//Arrange
			var requsets = new LoginRequest()
			{
				Email = "Test1@gmail.com",
				Password = "123456789"
			};
			//Act
			var response = await _client.PostAsJsonAsync("/Authentication/login", requsets);

			//Assert
			Assert.True(response.IsSuccessStatusCode);
		}

		[Fact]
		public async Task Login_WithInvalidPassword_ReturnsError()
		{
			//Arrange
			var requsets = new LoginRequest()
			{
				Email = "Test1@gmail.com",
				Password = "1423234"
			};
			//Act
			var response = await _client.PostAsJsonAsync("/Authentication/login", requsets);

			//Assert
			Assert.False(response.IsSuccessStatusCode);
		}

		[Fact]
		public async Task Register_WithValidCredentials_ReturnsSuccess()
		{
			// Arrange
			var requsets = new RegistrationRequest()
			{
				Username = "User2_Test",
				Email = "Test2@gmail.com",
				Password = "123456789"
			};
			//Act
			var response = await _client.PostAsJsonAsync("/Authentication/Register", requsets);

			//Assert
			Assert.True(response.IsSuccessStatusCode);
		}

		[Fact]
		public async Task Register_WithAlreadyExistingEmail_ReturnsError()
		{
			// Arrange
			var requsets = new RegistrationRequest()
			{
				Username = "Test3_User",
				Email = "Test1@gmail.com",
				Password = "123456789"
			};
			//Act
			var response = await _client.PostAsJsonAsync("/Authentication/Register", requsets);

			//Assert
			Assert.False(response.IsSuccessStatusCode);
		}
	}
}
