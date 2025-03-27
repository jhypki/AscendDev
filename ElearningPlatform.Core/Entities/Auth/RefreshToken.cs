namespace ElearningPlatform.Core.Entities.Auth;

public class RefreshToken
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public string CreatedByIp { get; set; } = null!;
    public bool IsActive => Revoked == null && DateTime.UtcNow < Expires;
    public DateTime? Revoked { get; set; }
    public string? RevokedByIp { get; set; }
}