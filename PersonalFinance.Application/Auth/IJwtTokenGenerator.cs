using PersonalFinance.Domain.Entities;

public interface IJwtTokenGenerator
{
  string Generate(ApplicationUser user, out DateTime expireAt);
}