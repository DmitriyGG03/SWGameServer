using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server;
using SharedLibrary.Contracts.Hubs;
using SharedLibrary.Models;
using SharedLibrary.Requests;
using SharedLibrary.Responses;
using System.Net.Http.Json;


namespace Tests
{
	public class BaseTest
	{
		protected const string _baseUrl = "https://localhost:7148";

		protected readonly WebApplicationFactory<Program> _appFactory;
		protected List<HttpClient> _httpClients;

		public BaseTest()
		{
			_appFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(host => {
				host.ConfigureServices(services =>
				{
					var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<GameDbContext>));
					services.Remove(descriptor);

					services.AddDbContext<GameDbContext>(options => {
						options.UseInMemoryDatabase("InMemoryDB");
					});
				});
			});

			_httpClients = new List<HttpClient>();
		}

		public HttpClient RegisterClient(string username, string email, string password)
		{
			HttpClient client = _appFactory.CreateClient();
			var response = client.PostAsJsonAsync($"{_baseUrl}/authentication/register", new RegistrationRequest { Username = username, Email = email, Password = password }).Result;
			
			if (response.IsSuccessStatusCode)
			{
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response.Content.ReadFromJsonAsync<AuthenticationResponse>().Result.Token);
				return client;
			}
			return client;
		}

		public HttpClient LoginUser(string email, string password)
		{
			HttpClient client = _appFactory.CreateClient();

			var response = client.PostAsJsonAsync($"{_baseUrl}/authentication/login", new LoginRequest { Email = email, Password = password }).Result;
			if (response.IsSuccessStatusCode)
			{
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response.Content.ReadFromJsonAsync<AuthenticationResponse>().Result.Token);
				return client;
			}
			return client;
		}
	}
}
