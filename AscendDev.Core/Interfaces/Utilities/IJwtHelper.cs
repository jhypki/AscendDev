namespace AscendDev.Core.Interfaces.Utils;

public interface IJwtHelper
{
    string GenerateToken(Guid id, string email);
    string GenerateToken(Guid id, string email, IEnumerable<string> roles);
}