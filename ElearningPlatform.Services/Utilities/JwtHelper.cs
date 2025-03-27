using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElearningPlatform.Core.Interfaces.Utils;
using ElearningPlatform.Core.Models.Auth;
using Microsoft.IdentityModel.Tokens;

namespace ElearningPlatform.Services.Utilities;

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
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email)
            }),
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