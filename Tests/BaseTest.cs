using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server;

namespace Tests
{
	public class BaseTest
	{
		protected readonly WebApplicationFactory<Program> _appFactory;

		public BaseTest()
		{
			_appFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(host => {
				host.ConfigureServices(services => {
					var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<GameDbContext>));
					services.Remove(descriptor);
					services.AddDbContext<GameDbContext>(options => {
						options.UseInMemoryDatabase("InMemoryDB");
					});
				});
			});
		}
	}
}
