namespace ElearningPlatform.Core.Interfaces.Utils;

public interface IJwtHelper
{
    string GenerateToken(Guid id, string email, string role);
}