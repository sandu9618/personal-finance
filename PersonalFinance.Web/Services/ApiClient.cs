using System.Net.Http.Json;
using System.Text.Json;
using PersonalFinance.Web.Models;

namespace PersonalFinance.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken) =>
        PostAsync<AuthResponse>("api/auth/register", request, cancellationToken);

    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken) =>
        PostAsync<AuthResponse>("api/auth/login", request, cancellationToken);

    public Task<DashboardResponse> GetDashboardAsync(CancellationToken cancellationToken) =>
        GetAsync<DashboardResponse>("api/dashboard", cancellationToken);

    public Task<AccountListResponse> GetAccountsAsync(CancellationToken cancellationToken) =>
        GetAsync<AccountListResponse>("api/account", cancellationToken);

    public Task CreateAccountAsync(AccountRequest request, CancellationToken cancellationToken) =>
        SendAsync("api/account", request, HttpMethod.Post, cancellationToken);

    public Task UpdateAccountAsync(Guid id, AccountRequest request, CancellationToken cancellationToken) =>
        SendAsync($"api/account/{id}", request, HttpMethod.Put, cancellationToken);

    public Task DeleteAccountAsync(Guid id, CancellationToken cancellationToken) =>
        DeleteAsync($"api/account/{id}", cancellationToken);

    public Task<CategoryListResponse> GetCategoriesAsync(CancellationToken cancellationToken) =>
        GetAsync<CategoryListResponse>("api/category", cancellationToken);

    public Task CreateCategoryAsync(CategoryRequest request, CancellationToken cancellationToken) =>
        SendAsync("api/category", request, HttpMethod.Post, cancellationToken);

    public Task UpdateCategoryAsync(Guid id, CategoryRequest request, CancellationToken cancellationToken) =>
        SendAsync($"api/category/{id}", request, HttpMethod.Put, cancellationToken);

    public Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken) =>
        DeleteAsync($"api/category/{id}", cancellationToken);

    public Task<TransactionListResponse> GetTransactionsAsync(CancellationToken cancellationToken) =>
        GetAsync<TransactionListResponse>("api/transaction", cancellationToken);

    public Task CreateTransactionAsync(TransactionRequest request, CancellationToken cancellationToken) =>
        SendAsync("api/transaction", request, HttpMethod.Post, cancellationToken);

    public Task UpdateTransactionAsync(Guid id, TransactionRequest request, CancellationToken cancellationToken) =>
        SendAsync($"api/transaction/{id}", request, HttpMethod.Put, cancellationToken);

    public Task DeleteTransactionAsync(Guid id, CancellationToken cancellationToken) =>
        DeleteAsync($"api/transaction/{id}", cancellationToken);

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync(path, cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    private async Task<T> PostAsync<T>(string path, object body, CancellationToken cancellationToken)
    {
        using var response = await _http.PostAsJsonAsync(path, body, cancellationToken);
        return await ReadAsync<T>(response, cancellationToken);
    }

    private async Task SendAsync(string path, object body, HttpMethod method, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(body)
        };
        using var response = await _http.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private async Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await _http.DeleteAsync(path, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response, cancellationToken);
        var body = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
        if (body is null)
        {
            throw new ApiException((int)response.StatusCode, "The API returned an empty response.");
        }

        return body;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new ApiException((int)response.StatusCode, await ReadErrorMessageAsync(response, cancellationToken));
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(body))
        {
            return $"The API returned {(int)response.StatusCode}.";
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.String
                && message.GetString() is string text
                && !string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var parts = new List<string>();
                foreach (var property in errors.EnumerateObject())
                {
                    if (property.Value.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var item in property.Value.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String && item.GetString() is string error && error.Length > 0)
                        {
                            parts.Add(error);
                        }
                    }
                }

                if (parts.Count > 0)
                {
                    return string.Join(" ", parts);
                }
            }
        }
        catch (JsonException)
        {
        }

        return $"The API returned {(int)response.StatusCode}.";
    }
}
