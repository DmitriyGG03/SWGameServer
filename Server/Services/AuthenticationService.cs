using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Server.Models;
using SharedLibrary;

namespace Server.Services;

public class AuthenticationService : IAuthenticationService
{
	public Settings Settings { get; init; }
	public GameDbContext Context { get; init; }

	public AuthenticationService(Settings settings, GameDbContext context)
	{
		Settings = settings;
		Context = context;
	}

	public (bool seccess, string content) Register(string username, string password)
	{
		if (Context.Users.Any(u => u.Username.Equals(username))) return (false, "This username already exists");

		var user = new User
		{
			Username = username,
			PasswordHash = password
		};
		user.ProvideSaltAndHash();

		Context.Add(user);
		Context.SaveChanges();

		return (true, "");
	}

	public (bool success, string token) Login(string username, string password)
	{
		var user = Context.Users.SingleOrDefault(u => u.Username.Equals(username));

		if (user == null) return (false, "No user with that name found");

		if (user.PasswordHash != AuthenticationHelper.ComputeHash(password, user.Salt)) return (false, "Password is incorrect");
		return (true, GenerateJwtToken(AssembleClaimsIdentity(user)));
	}

	private ClaimsIdentity AssembleClaimsIdentity(User user)
	{
		var subject = new ClaimsIdentity(new[]
		{
			new Claim("id", user.Id.ToString()),
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

public interface IAuthenticationService
{
	(bool seccess, string content) Register(string username, string password);
	(bool success, string token) Login(string username, string password);
}

public static class AuthenticationHelper
{
	public static void ProvideSaltAndHash(this User user)
	{
		var salt = GenerateSalt();
		user.Salt = Convert.ToBase64String(salt);
		user.PasswordHash = ComputeHash(user.PasswordHash, user.Salt);
	}

	private static byte[] GenerateSalt()
	{
		var rng = RandomNumberGenerator.Create();
		var salt = new byte[24];
		rng.GetBytes(salt);
		return salt;
	}

	public static string ComputeHash(string password, string saltString)
	{
		var salt = Convert.FromBase64String(saltString);
		byte[] bytes;

		using (var hashGenerator = new Rfc2898DeriveBytes(password, salt))
		{
			hashGenerator.IterationCount = 10101;
			bytes = hashGenerator.GetBytes(24);
		}
		return Convert.ToBase64String(bytes);
	}
}