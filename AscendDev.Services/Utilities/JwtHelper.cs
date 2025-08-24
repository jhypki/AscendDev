using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AscendDev.Core.Interfaces.Utils;
using AscendDev.Core.Models.Auth;
using Microsoft.IdentityModel.Tokens;

namespace AscendDev.Services.Utilities;

public class JwtHelper : IJwtHelper
{
    private readonly JwtSettings _jwtSettings;

    public JwtHelper(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings),
            "JWT settings cannot be null.");

        if (string.IsNullOrEmpty(_jwtSettings.Key))
            throw new ArgumentNullException(nameof(_jwtSettings.Key), "JWT Key is not configured.");
        if (string.IsNullOrEmpty(_jwtSettings.Issuer))
            throw new ArgumentNullException(nameof(_jwtSettings.Issuer), "JWT Issuer is not configured.");
        if (string.IsNullOrEmpty(_jwtSettings.Audience))
            throw new ArgumentNullException(nameof(_jwtSettings.Audience), "JWT Audience is not configured.");
    }

    public string GenerateToken(Guid userId, string email)
    {
        return GenerateToken(userId, email, Enumerable.Empty<string>());
    }

    public string GenerateToken(Guid userId, string email, IEnumerable<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email)
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}