﻿using System.IdentityModel.Tokens.Jwt;
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

		var user = new User
		{
			Username = username,
			PasswordHash = password,
			Email = email
		};
		user.ProvideSaltAndHash(_hashProvider);
		user.Hero = null;

		Context.Add(user);
		Context.SaveChanges();

		string accessToken = GenerateJwtToken(AssembleClaimsIdentity(user));
        return new AuthenticationResult(accessToken);
	}
	public AuthenticationResult Login(string email, string password)
	{
		var user = Context.Users.FirstOrDefault(u => u.Email == email);

		if (user == null) 
			return new AuthenticationResult(new string[] { "No user with that email found" });
		if (user.PasswordHash != _hashProvider.ComputeHash(password, user.Salt)) 
			return new AuthenticationResult(new string[] { "Password is incorrect" });

		return new AuthenticationResult(GenerateJwtToken(AssembleClaimsIdentity(user)));
	}

	private ClaimsIdentity AssembleClaimsIdentity(User user)
	{
		var subject = new ClaimsIdentity(new[]
		{
			new Claim("id", user.Id.ToString()),
			new Claim("hero", JsonConvert.SerializeObject(user.Hero))
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