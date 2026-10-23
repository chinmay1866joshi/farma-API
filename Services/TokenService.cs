using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FarmaApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace FarmaApi.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration config)
    {
        _config = config;
        var secret = _config["Jwt:Key"] ?? "KafhOrganicFarmSecretKeyMustBe32CharsLong123!";
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }

    public int GetTokenExpiryMinutes(bool rememberMe = false)
    {
        // 30 days if rememberMe, otherwise 24 hours (1440 mins)
        return rememberMe ? 60 * 24 * 30 : 60 * 24;
    }

    public string CreateToken(User user, bool rememberMe = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("City", user.City),
            new("State", user.State),
            new("MobileNumber", user.MobileNumber)
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

        var expiry = DateTime.UtcNow.AddMinutes(GetTokenExpiryMinutes(rememberMe));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiry,
            SigningCredentials = creds,
            Issuer = _config["Jwt:Issuer"] ?? "FarmaApi",
            Audience = _config["Jwt:Audience"] ?? "FarmaApiUsers"
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
