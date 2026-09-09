
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PersonalFinance.Domain.Entities;

public class JwtTokenGenerator : IJwtTokenGenerator
{
  private readonly IConfiguration _configuration;

  public JwtTokenGenerator(IConfiguration configuration)
  {
    _configuration = configuration;
  }
  
  public string Generate(ApplicationUser user, out DateTime expireAt)
  {
    var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration.");
    var issuer = _configuration["Jwt:Issuer"];
    var audience = _configuration["Jwt:Audience"];
    var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

    expireAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
      new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
    };

    var credentials = new SigningCredentials(
      new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
      SecurityAlgorithms.HmacSha256
    );

    var token = new JwtSecurityToken(
      issuer: issuer,
      audience: audience,
      claims: claims,
      expires: expireAt,
      signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}