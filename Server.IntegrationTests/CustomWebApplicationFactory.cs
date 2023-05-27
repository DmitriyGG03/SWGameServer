using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Server.IntegrationTests
{
	public class CustomWebApplicationFactory<TProgram>: WebApplicationFactory<TProgram> where TProgram : class
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureServices(services =>
			{
				var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<GameDbContext>));
				services.Remove(dbContextDescriptor);

				var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(GameDbContext));
				services.Remove(dbConnectionDescriptor);

				var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<GameDbContext>));
				services.Remove(descriptor);

				services.AddDbContext<GameDbContext>(options => {
					options.UseInMemoryDatabase("InMemoryDB");
				});
			});

			builder.UseEnvironment("Development");
		}
	}
}
