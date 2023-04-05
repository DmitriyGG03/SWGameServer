namespace Server.Services.Abstract;

public interface IAuthenticationService
{
    (bool success, string content) Register(string username, string email, string password);
    (bool success, string token) Login(string email, string password);
}
