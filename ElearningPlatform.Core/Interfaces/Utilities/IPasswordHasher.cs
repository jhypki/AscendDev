namespace ElearningPlatform.Core.Interfaces.Utils;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}