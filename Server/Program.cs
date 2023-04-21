using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server;
using Server.Models;
using Server.Services;
using Server.Services.Abstract;
using System.Text;
using Server.Common.Constants;
using Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

var settings = new Settings();
builder.Configuration.Bind("Settings", settings);
builder.Services.AddSingleton(settings);

var productionDatabase = builder.Configuration.GetConnectionString(ConnectionKeys.Production);
var localDatabase = builder.Configuration.GetConnectionString(ConnectionKeys.Local);
builder.Services.AddDbContext<GameDbContext>(o => o.UseSqlServer(localDatabase));
builder.Services.AddSignalR();

builder.Services.AddControllers().AddNewtonsoftJson(i =>
{
    
});

builder.Services.AddScoped<IHeroService, HeroService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IHashProvider, HashProvider>();
builder.Services.AddScoped<IMapService, MapService>();
builder.Services.AddScoped<IMapGenerator, DefaultMapGeneratorStrategy>();
builder.Services.AddScoped<ILobbyService, LobbyService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters()
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.BearerKey)),
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        ValidateIssuer = false,
    };
});

var app = builder.Build(); // Создает обьект WebApplication

if (app.Environment.IsDevelopment()) 
{
    
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<LobbyHub>("/lobbyHub");
app.Run();

//For test
public partial class Program { }
