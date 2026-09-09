public interface IAuthService
{
    Task<AuthResponse> Register(string email, string password);
    Task<AuthResponse> Login(string email, string password);
}