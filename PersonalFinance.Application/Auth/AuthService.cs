using Microsoft.AspNetCore.Identity;
using PersonalFinance.Domain.Entities;

public class AuthService : IAuthService
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IJwtTokenGenerator _jwtTokenGenerator;

  public AuthService(UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwtTokenGenerator)
  {
    _userManager = userManager;
    _jwtTokenGenerator = jwtTokenGenerator;
  }

  public async Task<AuthResponse> Register(string email, string password)
  {
    var user = new ApplicationUser
    {
      UserName = email,
      Email = email,
      CreatedAt = DateTime.UtcNow
    };

    var result = await _userManager.CreateAsync(user, password);

    if (!result.Succeeded)
    {
      var message = string.Join(" ", result.Errors.Select(error => error.Description));
      throw new InvalidOperationException(message);
    }

    var token = _jwtTokenGenerator.Generate(user, out var expiresAt);

    return new AuthResponse(token, expiresAt, user.Id, user.Email);
  }

  public async Task<AuthResponse> Login(string email, string password)
  {
    var user = await _userManager.FindByEmailAsync(email);

    if (user == null || !await _userManager.CheckPasswordAsync(user, password))
    {
      throw new UnauthorizedAccessException("Invalid email or password");
    }

    var token = _jwtTokenGenerator.Generate(user, out var expiresAt);

    return new AuthResponse(token, expiresAt, user.Id, user.Email ?? null!);
  }
}