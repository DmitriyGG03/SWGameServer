using SharedLibrary.Models;
using System.Security.Cryptography;

namespace Server.Helpers;

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
