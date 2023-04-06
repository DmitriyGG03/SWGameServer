using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server;
using SharedLibrary.Requests;
using SharedLibrary.Responses;
using System.Net.Http.Json;

namespace Tests
{
	public class IntegrationTests
	{
		protected readonly HttpClient httpClient;

		public IntegrationTests()
		{
			var appFactory = new WebApplicationFactory<Program>()
					.WithWebHostBuilder(host =>
					{
						host.ConfigureServices(services =>
						{
							var descriptor = services.SingleOrDefault(
								d => d.ServiceType ==
								typeof(DbContextOptions<GameDbContext>));

							services.Remove(descriptor);

							services.AddDbContext<GameDbContext>(options =>
							{
								options.UseInMemoryDatabase("InMemoryDB");
							});
						});
					});
			httpClient = appFactory.CreateClient();
		}

		[Fact]
		public async Task RegistrationTest()
		{
			var response = await httpClient.PostAsJsonAsync("https://localhost:7148/Authentication/register", new RegistrationRequest
			{
				Username = AccountData.Username,
				Email = AccountData.Email,
				Password = AccountData.Password
			});

			var result = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
			Assert.Matches("^([a-zA-Z0-9_=]+)\\.([a-zA-Z0-9_=]+)\\.([a-zA-Z0-9_\\-\\+\\/=]*)", result.Token);
		}

		[Fact]
		public async Task LoginTest()
		{
			var response1 = await httpClient.PostAsJsonAsync("https://localhost:7148/Authentication/register", new RegistrationRequest
			{
				Username = AccountData.Username,
				Email = AccountData.Email,
				Password = AccountData.Password
			});

			var response2 = await httpClient.PostAsJsonAsync("https://localhost:7148/Authentication/login", new LoginRequest
			{
				Email = AccountData.Email,
				Password = AccountData.Password
			});

			var result = await response2.Content.ReadFromJsonAsync<AuthenticationResponse>();
			Assert.Matches("^([a-zA-Z0-9_=]+)\\.([a-zA-Z0-9_=]+)\\.([a-zA-Z0-9_\\-\\+\\/=]*)", result.Token);
		}
	}
}