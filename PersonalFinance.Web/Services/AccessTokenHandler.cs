using System.Net.Http.Headers;

namespace PersonalFinance.Web.Services;

public class AccessTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccessTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath ?? "";
        var isAuthCall = path.Contains("/api/auth/", StringComparison.OrdinalIgnoreCase);
        if (!isAuthCall)
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst(AuthCookie.AccessTokenClaim)?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}
