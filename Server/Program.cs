using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Server;
using Server.Models;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);

var settings = new Settings();

builder.Configuration.Bind("Settings", settings);


builder.Services.AddDbContext<GameDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Db")));

builder.Services.AddControllers().AddNewtonsoftJson(i =>
{
    
});

builder.Services.AddScoped<IPlayerService, PlayerService>();

var app = builder.Build(); // Создает обьект WebApplication

if (app.Environment.IsDevelopment()) 
{
    
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

