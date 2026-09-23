using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PersonalFinance.Domain.Enums;

public class IsolationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public IsolationTests(ApiFactory factory)
    {
        _factory = factory;
        factory.ResetDatabaseAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task UserB_cannot_get_UserA_account()
    {
        var clientA = _factory.CreateClient();
        var clientB = _factory.CreateClient();

        var tokenA = await _factory.RegisterAsync(clientA);
        ApiFactory.UseToken(clientA, tokenA);

        var create = await clientA.PostAsJsonAsync("/api/Account", new
        {
            name = "Cash",
            initialBalance = 100000,
            type = (int)AccountType.Cash, // 0
            currency = "LKR"
        });
        create.EnsureSuccessStatusCode();

        using var created = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var accountId = created.RootElement.GetProperty("id").GetGuid();

        var tokenB = await _factory.RegisterAsync(clientB);
        ApiFactory.UseToken(clientB, tokenB);

        var asB = await clientB.GetAsync($"/api/Account/{accountId}");
        Assert.Equal(HttpStatusCode.NotFound, asB.StatusCode);
    }
}