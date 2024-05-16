using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fake.Detection.Auth.Configure;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fake.Detection.Auth.Helpers;

public class JWTHelper
{
    private readonly IOptionsMonitor<JWTOptions> _options;

    public JWTHelper(IOptionsMonitor<JWTOptions> options)
    {
        _options = options;
    }

    public string GenerateJwtToken(string login)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_options.CurrentValue.SecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(nameof(login), login) }),
            Expires = DateTime.UtcNow.AddMinutes(_options.CurrentValue.ExpireMinutes),
            Issuer = _options.CurrentValue.Issuer,
            Audience = _options.CurrentValue.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return tokenHandler.WriteToken(token);
    }
    
    public string? GetLoginFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_options.CurrentValue.SecretKey);

        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = false,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero
        }, out var validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        var userNameClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "login");

        return userNameClaim?.Value;
    }
}