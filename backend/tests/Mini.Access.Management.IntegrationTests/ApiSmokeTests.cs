using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mini.Access.Management.Application.Contracts;
using Mini.Access.Management.EntityFrameworkCore;
using Testcontainers.MySql;

namespace Mini.Access.Management.IntegrationTests;

public sealed class ApiSmokeTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private readonly string password = $"T!{Guid.NewGuid():N}aA1";
    private readonly MySqlContainer mysql;
    private WebApplicationFactory<Program>? factory;
    private HttpClient? client;
    private HttpClient Client => client ??= factory!.CreateClient();

    public ApiSmokeTests()
    {
        mysql = new MySqlBuilder("mysql:8.4")
            .WithDatabase("access_tests").WithUsername("test")
            .WithPassword(password).Build();
    }

    [Fact]
    public async Task AccessRequestCompletesTwoApprovalsAndProvisioning()
    {
        await LoginAsync("employee@access.local", password);
        var systems = await Client.GetFromJsonAsync<TargetSystemDto[]>("/api/target-systems");
        var roles = await Client.GetFromJsonAsync<RoleDto[]>("/api/roles");
        var maker = roles!.Single(x => x.Name == "Maker");
        var created = await (await Client.PostAsJsonAsync("/api/access-requests",
            new CreateAccessRequest
            {
                TargetSystemId = systems![0].Id,
                RequestedRoleId = maker.Id,
                BusinessJustification = "Required for daily transaction work."
            })).Content.ReadFromJsonAsync<AccessRequestDto>(JsonOptions);
        var submitted = await (await Client.PostAsync(
            $"/api/access-requests/{created!.Id}/submit", null))
            .Content.ReadFromJsonAsync<AccessRequestDto>(JsonOptions);
        Assert.Equal(Mini.Access.Management.Domain.Shared.AccessRequestStatus.Pending, submitted!.Status);

        await LoginAsync("manager@access.local", password);
        await Client.PostAsJsonAsync($"/api/access-requests/{created.Id}/approve",
            new ApprovalActionRequest { Remarks = "Manager approved." });
        await LoginAsync("security@access.local", password);
        var approved = await (await Client.PostAsJsonAsync(
            $"/api/access-requests/{created.Id}/approve",
            new ApprovalActionRequest { Remarks = "Security approved." }))
            .Content.ReadFromJsonAsync<AccessRequestDto>(JsonOptions);
        Assert.Equal(Mini.Access.Management.Domain.Shared.AccessRequestStatus.Approved, approved!.Status);

        await LoginAsync("admin@access.local", password);
        var provisioned = await (await Client.PostAsync(
            $"/api/access-requests/{created.Id}/provision", null))
            .Content.ReadFromJsonAsync<AccessRequestDto>(JsonOptions);
        Assert.Equal(Mini.Access.Management.Domain.Shared.AccessRequestStatus.Provisioned, provisioned!.Status);
    }

    [Fact]
    public async Task RetriedUserCreationReturnsTheSameUser()
    {
        await LoginAsync("admin@access.local", password);
        var key = Guid.NewGuid().ToString();
        var payload = new CreateUserRequest
        {
            FullName = "Retry Safe User",
            Email = $"retry-{Guid.NewGuid():N}@access.local",
            Password = password
        };

        var first = await CreateUserAsync(payload, key);
        var retry = await CreateUserAsync(payload, key);

        Assert.Equal(first.Id, retry.Id);
    }

    public async Task InitializeAsync()
    {
        await mysql.StartAsync();
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Default", mysql.GetConnectionString());
            builder.UseSetting("DemoPassword", password);
            builder.UseSetting("Jwt:SigningKey", password);
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = mysql.GetConnectionString(),
                    ["DemoPassword"] = password,
                    ["Jwt:SigningKey"] = password
                }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AccessManagementDbContext>>();
                services.AddDbContext<AccessManagementDbContext>(
                    options => options.UseMySQL(mysql.GetConnectionString()));
            });
        });
    }

    private async Task LoginAsync(string email, string password)
    {
        var response = await Client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest { Email = email, Password = password });
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login!.AccessToken);
    }

    private async Task<UserDto> CreateUserAsync(CreateUserRequest payload, string key)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/users")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("Idempotency-Key", key);
        using var response = await Client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions))!;
    }

    public async Task DisposeAsync()
    {
        client?.Dispose();
        if (factory is not null) await factory.DisposeAsync();
        await mysql.DisposeAsync();
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
