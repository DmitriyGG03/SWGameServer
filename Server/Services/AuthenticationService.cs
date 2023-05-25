using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Server.Domain;
using Server.Helpers;
using Server.Models;
using Server.Services.Abstract;
using SharedLibrary.Models;

namespace Server.Services;

public class AuthenticationService : IAuthenticationService
{
	public Settings Settings { get; init; }
	public GameDbContext Context { get; init; }
	private readonly IHashProvider _hashProvider;

    public AuthenticationService(Settings settings, GameDbContext context, IHashProvider hashProvider)
    {
        Settings = settings;
        Context = context;
        _hashProvider = hashProvider;
    }

    public AuthenticationResult Register(string username, string email, string password)
	{
		if (Context.Users.Any(u => u.Username.Equals(username))) 
			return new AuthenticationResult(new string[] { "The user with given username already exists" });
		if(Context.Users.Any(u => u.Email.Equals(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$", RegexOptions.IgnoreCase)))
			return new AuthenticationResult(new string[] { "Incorrect email format" });

		var user = new ApplicationUser
		{
			Username = username,
			PasswordHash = password,
			Email = email
		};
		user.ProvideSaltAndHash(_hashProvider);
		user.Heroes = null;

		Context.Add(user);
		Context.SaveChanges();

		string accessToken = GenerateJwtToken(AssembleClaimsIdentity(user));
		string[] operationResult = new[] { "New user has been successfully created" };

        return new AuthenticationResult(operationResult, user, accessToken);
	}
	public AuthenticationResult Login(string email, string password)
	{
		var user = Context.Users.FirstOrDefault(u => u.Email == email);
		
		if (user == null ||
		    user.PasswordHash != _hashProvider.ComputeHash(password, user.Salt)) 
			return new AuthenticationResult(new[] { "Email or password is incorrect" });

		return new AuthenticationResult(new[] { "Login is successful" }, user, GenerateJwtToken(AssembleClaimsIdentity(user)));
	}

	private ClaimsIdentity AssembleClaimsIdentity(ApplicationUser user)
	{
		var subject = new ClaimsIdentity(new[]
		{
			new Claim("id", user.Id.ToString()),
			new Claim("hero", JsonConvert.SerializeObject(user.Heroes))
		});

		return subject;
	}
	private string GenerateJwtToken(ClaimsIdentity subject)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(Settings.BearerKey);
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = subject,
			Expires = DateTime.Now.AddYears(10),
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};
		var token = tokenHandler.CreateToken(tokenDescriptor);
		return tokenHandler.WriteToken(token);
	}
}