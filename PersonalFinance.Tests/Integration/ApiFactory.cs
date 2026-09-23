using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
  private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:16")
    .Build();
  public async Task InitializeAsync()
  {
    await _db.StartAsync();
  }

  async Task IAsyncLifetime.DisposeAsync()
  {
    await _db.DisposeAsync();
    await base.DisposeAsync();
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseEnvironment("Development");

    builder.ConfigureAppConfiguration((_, config) =>
    {
      config.AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["ConnectionStrings:DefaultConnection"] = _db.GetConnectionString(),
        ["Jwt:Issuer"] = "PersonalFinance",
        ["Jwt:Audience"] = "PersonalFinance",
        ["Jwt:Key"] = "s5uDrlA5yUcwKjTbeImjJjnoADLlUAffMjTJJY0M4KM=",
        ["Jwt:ExpiryMinutes"] = "60"
      });
    });
  }
  public async Task ResetDatabaseAsync()
  {
      using var scope = Services.CreateScope();
      var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      await db.Database.MigrateAsync();
  }

  public async Task<string> RegisterAsync(HttpClient client, string? email = null, string password = "Password1!")
  {
      email ??= $"user-{Guid.NewGuid()}@test.com";

      var response = await client.PostAsJsonAsync("/api/Auth/register", new
      {
          email,
          password
      });

      response.EnsureSuccessStatusCode();

      using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
      return doc.RootElement.GetProperty("token").GetString()!;
  }

  public static void UseToken(HttpClient client, string token)
  {
      client.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue("Bearer", token);
  }
}
