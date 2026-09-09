using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    var connecctionString = configuration.GetConnectionString("DefaultConnection")
      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    services.AddDbContext<AppDbContext>(options => 
      options.UseNpgsql(connecctionString));
    
    services
      .AddIdentityCore<ApplicationUser>()
      .AddRoles<IdentityRole<Guid>>()
      .AddEntityFrameworkStores<AppDbContext>();
    
    services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    services.AddScoped<IAccountRepository, AccountRepository>();

    return services;
  }

  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IAccountService, AccountService>();
    return services;
  }
}
