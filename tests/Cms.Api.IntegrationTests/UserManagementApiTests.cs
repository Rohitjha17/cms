using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Cms.Domain.Constants;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// Account administration is the path by which every real customer gets access, so the
/// privilege boundaries around it are asserted end to end.
/// </summary>
public sealed class UserManagementApiTests : IClassFixture<CmsApiFactory>
{
    private const string TenantAdminEmail = "admin@demo.local";
    private const string TenantAdminPassword = "Admin@12345";

    private static readonly SemaphoreSlim TokenGate = new(1, 1);
    private static readonly Dictionary<string, string> TokenCache = [];

    private readonly HttpClient _client;

    public UserManagementApiTests(CmsApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Anonymous_CannotListAccounts()
    {
        using var response = await _client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TenantAdmin_InvitesEditor_AndThatEditorCanSignIn()
    {
        var token = await LoginAsync(TenantAdminEmail, TenantAdminPassword);
        var email = UniqueEmail("editor");
        const string password = "Editor@98765";

        using var create = Authorized(HttpMethod.Post, "/api/users", token);
        create.Content = JsonContent.Create(new
        {
            email,
            fullName = "Invited Editor",
            role = AppRoles.Editor,
            isActive = true,
            password
        });

        using var created = await _client.SendAsync(create);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        using var payload = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var user = payload.RootElement.GetProperty("data").GetProperty("user");
        Assert.Equal(email, user.GetProperty("email").GetString());
        Assert.Equal(AppRoles.Editor, user.GetProperty("role").GetString());
        Assert.True(user.GetProperty("isActive").GetBoolean());

        // A reset token is always issued so the invitee can set their own password.
        Assert.False(string.IsNullOrWhiteSpace(
            payload.RootElement.GetProperty("data").GetProperty("passwordResetToken").GetString()));

        var editorToken = await LoginAsync(email, password);
        Assert.False(string.IsNullOrWhiteSpace(editorToken));
    }

    [Fact]
    public async Task TenantAdmin_CannotGrantSuperAdmin()
    {
        var token = await LoginAsync(TenantAdminEmail, TenantAdminPassword);

        using var request = Authorized(HttpMethod.Post, "/api/users", token);
        request.Content = JsonContent.Create(new
        {
            email = UniqueEmail("escalate"),
            role = AppRoles.SuperAdmin,
            isActive = true,
            password = "Escalate@12345"
        });

        using var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TenantAdmin_CannotCreateAccountsInAnotherTenant()
    {
        var token = await LoginAsync(TenantAdminEmail, TenantAdminPassword);
        var email = UniqueEmail("crosstenant");

        using var request = Authorized(HttpMethod.Post, "/api/users", token);
        request.Content = JsonContent.Create(new
        {
            email,
            role = AppRoles.Editor,
            tenantId = Guid.NewGuid(), // a tenant the caller has nothing to do with
            isActive = true,
            password = "Cross@12345"
        });

        using var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // The posted tenant is ignored rather than honoured: the account lands in the
        // caller's own tenant, so it must be visible in their own listing.
        var listed = await ListEmailsAsync(token);
        Assert.Contains(email, listed);
    }

    [Fact]
    public async Task TenantAdmin_CannotSeePlatformSuperAdministrators()
    {
        var token = await LoginAsync(TenantAdminEmail, TenantAdminPassword);
        var listed = await ListEmailsAsync(token);

        Assert.Contains(TenantAdminEmail, listed);
        Assert.DoesNotContain("superadmin@demo.local", listed);
    }

    [Fact]
    public async Task Editor_CannotReachAccountAdministration()
    {
        var adminToken = await LoginAsync(TenantAdminEmail, TenantAdminPassword);
        var email = UniqueEmail("plain-editor");
        const string password = "Editor@24680";

        using var create = Authorized(HttpMethod.Post, "/api/users", adminToken);
        create.Content = JsonContent.Create(new
        {
            email,
            role = AppRoles.Editor,
            isActive = true,
            password
        });
        using var created = await _client.SendAsync(create);
        created.EnsureSuccessStatusCode();

        var editorToken = await LoginAsync(email, password);

        using var request = Authorized(HttpMethod.Get, "/api/users", editorToken);
        using var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TenantAdmin_CannotDeactivateTheirOwnAccount()
    {
        var token = await LoginAsync(TenantAdminEmail, TenantAdminPassword);
        var users = await ListUsersAsync(token);
        var self = users.Single(x => x.Email == TenantAdminEmail);

        using var request = Authorized(HttpMethod.Patch, $"/api/users/{self.Id}/status", token);
        request.Content = JsonContent.Create(new { isActive = false });

        using var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_AnswersIdenticallyForKnownAndUnknownAddresses()
    {
        var known = await ForgotPasswordMessageAsync(TenantAdminEmail);
        var unknown = await ForgotPasswordMessageAsync("nobody@nowhere.test");

        Assert.Equal(known, unknown);
    }

    [Fact]
    public async Task ResetPassword_WithTamperedToken_IsRejected()
    {
        using var response = await _client.PostAsJsonAsync("/api/users/reset-password", new
        {
            email = TenantAdminEmail,
            token = "not-a-real-token",
            newPassword = "Brand@New12345",
            confirmPassword = "Brand@New12345"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static string UniqueEmail(string prefix) => $"{prefix}-{Guid.NewGuid():N}@demo.local";

    private static HttpRequestMessage Authorized(HttpMethod method, string uri, string token)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Site-Key", "school");
        return request;
    }

    /// <summary>
    /// Tokens are cached for the lifetime of the shared fixture. The sign-in endpoint is
    /// rate limited to 10 requests a minute, so re-authenticating in every test would make
    /// the suite fail for reasons that have nothing to do with the behaviour under test.
    /// </summary>
    private async Task<string> LoginAsync(string email, string password)
    {
        await TokenGate.WaitAsync();
        try
        {
            if (TokenCache.TryGetValue(email, out var cached))
            {
                return cached;
            }

            using var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
            response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var token = json.RootElement.GetProperty("data").GetProperty("token").GetString()!;
            TokenCache[email] = token;
            return token;
        }
        finally
        {
            TokenGate.Release();
        }
    }

    private async Task<string> ForgotPasswordMessageAsync(string email)
    {
        using var response = await _client.PostAsJsonAsync("/api/users/forgot-password", new { email });
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("message").GetString()!;
    }

    private async Task<List<(string Id, string Email)>> ListUsersAsync(string token)
    {
        using var request = Authorized(HttpMethod.Get, "/api/users", token);
        using var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("data").EnumerateArray()
            .Select(x => (
                Id: x.GetProperty("id").GetString()!,
                Email: x.GetProperty("email").GetString()!))
            .ToList();
    }

    private async Task<List<string>> ListEmailsAsync(string token) =>
        (await ListUsersAsync(token)).Select(x => x.Email).ToList();
}
