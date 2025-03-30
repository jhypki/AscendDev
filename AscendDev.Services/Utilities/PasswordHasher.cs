using AscendDev.Core.Interfaces.Utils;
using Microsoft.Extensions.Logging;

namespace AscendDev.Services.Utilities;

public class PasswordHasher(ILogger<PasswordHasher> logger) : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        try
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

            return hashedPassword;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error hashing password");
            throw;
        }
    }

    public bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error verifying password");
            throw;
        }
    }
}