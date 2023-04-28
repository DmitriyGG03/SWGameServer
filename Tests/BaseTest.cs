using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server;
using SharedLibrary.Requests;
using SharedLibrary.Responses;
using System.Net.Http.Json;


namespace Tests
{
    public class BaseTest
    {
        protected readonly HttpClient _httpClient;

        protected const string _baseUrl = "https://localhost:7148";

        public BaseTest()
        {
            var appFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(host => {
                host.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<GameDbContext>));
                    services.Remove(descriptor);

                    services.AddDbContext<GameDbContext>(options => {
                        options.UseInMemoryDatabase("InMemoryDB");
                    });
                });
            });

            _httpClient = appFactory.CreateClient();
        }

        public bool Register(string username, string email, string password)
        {
            var response = _httpClient.PostAsJsonAsync($"{_baseUrl}/authentication/register", new RegistrationRequest { Username = username, Email = email, Password = password }).Result;
            if (response.IsSuccessStatusCode)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response.Content.ReadFromJsonAsync<AuthenticationResponse>().Result.Token);
            }
            return response.IsSuccessStatusCode;
        }

        public bool Login(string email, string password)
        {
            var response = _httpClient.PostAsJsonAsync($"{_baseUrl}/authentication/login", new LoginRequest { Email = email, Password = password }).Result;
            if (response.IsSuccessStatusCode)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", response.Content.ReadFromJsonAsync<AuthenticationResponse>().Result.Token);
            }
            return response.IsSuccessStatusCode;
        }
    }
}