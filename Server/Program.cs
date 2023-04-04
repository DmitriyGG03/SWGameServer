using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Server;
using Server.Models;
using Server.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var settings = new Settings();
builder.Configuration.Bind("Settings", settings);
builder.Services.AddSingleton(settings);


builder.Services.AddDbContext<GameDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Db")));

builder.Services.AddControllers().AddNewtonsoftJson(i =>
{
    
});

builder.Services.AddScoped<IHeroService, HeroService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

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

app.Run();

