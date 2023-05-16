using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server;
using Server.Models;
using Server.Services;
using Server.Services.Abstract;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Server.Common.Constants;
using Server.Hubs;
using Server.Hubs.Providers;

var builder = WebApplication.CreateBuilder(args);

var settings = new Settings();
builder.Configuration.Bind("Settings", settings);
builder.Services.AddSingleton(settings);

var productionDatabase = builder.Configuration.GetConnectionString(ConnectionKeys.Production);
var localDatabase = builder.Configuration.GetConnectionString(ConnectionKeys.Local);

builder.Services.AddDbContext<GameDbContext>(o => o.UseSqlite(localDatabase));

builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddSignalR();

builder.Services.AddControllers().AddNewtonsoftJson(i =>
{
    
});

builder.Services.AddScoped<IHeroService, HeroService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IHashProvider, HashProvider>();
builder.Services.AddScoped<IMapGenerator, DefaultMapGeneratorStrategy>();
builder.Services.AddScoped<ILobbyService, LobbyService>();
builder.Services.AddScoped<ISessionService, SessionService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters()
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.BearerKey)),
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        ValidateIssuer = false,
    };
    
    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/hubs")))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
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
app.MapHub<LobbyHub>("/hubs/lobby");
app.MapHub<SessionHub>("/hubs/session");
app.Run();

//For test
public partial class Program { }
