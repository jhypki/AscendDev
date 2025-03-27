namespace ElearningPlatform.Core.Entities.Auth;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}