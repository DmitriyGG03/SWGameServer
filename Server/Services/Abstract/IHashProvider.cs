namespace Server.Services.Abstract;

public interface IHashProvider
{
    string ComputeHash(string password, string saltString);
    byte[] GenerateSalt();
}
