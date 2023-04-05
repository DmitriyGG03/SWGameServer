using Server.Services.Abstract;
using SharedLibrary.Models;

namespace Server.Helpers;

public static class AuthenticationHelper
{
    public static void ProvideSaltAndHash(this User user, IHashProvider hashProvider)
    {
        var salt = hashProvider.GenerateSalt();
        user.Salt = Convert.ToBase64String(salt);
        user.PasswordHash = hashProvider.ComputeHash(user.PasswordHash, user.Salt);
    }
}
