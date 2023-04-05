using Server.Domain;

namespace Server.Services.Abstract;

public interface IAuthenticationService
{
    AuthenticationResult Register(string username, string email, string password);
    AuthenticationResult Login(string email, string password);
}
