using Cms.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// A deployment that is not configured must stop and say so, rather than come up looking healthy
/// on the demo academy's content, a password published in this repository and a database the next
/// restart discards. Nothing about that failure is visible from the outside until it is too late.
/// </summary>
public sealed class ProductionReadinessTests
{
    private static readonly Dictionary<string, string?> ReadyForProduction = new()
    {
        ["Database:Provider"] = "SqlServer",
        ["ConnectionStrings:DefaultConnection"] = "Server=db;Database=CmsDb;User Id=u;Password=p",
        ["Platform:Domain"] = "console.school.example",
        ["Platform:SuperAdminEmail"] = "ops@school.example",
        ["Platform:SuperAdminPassword"] = "a-real-password",
        ["Storage:Provider"] = "S3"
    };

    [Fact]
    public void AProperlyConfiguredDeployment_Starts()
    {
        var exception = Record.Exception(() => Check(ReadyForProduction));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("Database:Provider", "Sqlite", "Sqlite")]
    [InlineData("ConnectionStrings:DefaultConnection", "", "is empty")]
    [InlineData("ConnectionStrings:DefaultConnection", "Server=db;Password=SET_VIA_ENVIRONMENT", "placeholder")]
    [InlineData("Seed:EnableDemoData", "true", "demo academy")]
    [InlineData("Platform:Domain", "", "Platform__Domain")]
    [InlineData("Platform:SuperAdminEmail", "", "Platform__SuperAdminEmail")]
    [InlineData("Platform:SuperAdminPassword", "", "Platform__SuperAdminPassword")]
    public void AMisconfiguredDeployment_RefusesToStart(string key, string value, string expectedInMessage)
    {
        var settings = new Dictionary<string, string?>(ReadyForProduction) { [key] = value };

        var exception = Assert.Throws<InvalidOperationException>(() => Check(settings));

        Assert.Contains(expectedInMessage, exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TheApisSigningKey_MustNotBeTheOneFromTheRepository()
    {
        var settings = new Dictionary<string, string?>(ReadyForProduction)
        {
            ["Jwt:Key"] = "CmsDevelopmentOnlyKey_Replace_InProduction_32!"
        };

        var exception = Assert.Throws<InvalidOperationException>(() => Check(settings));

        Assert.Contains("Jwt__Key", exception.Message);
    }

    /// <summary>The demo container is allowed to be a demo; everything else is not.</summary>
    [Fact]
    public void TheDemoContainer_IsStillAllowedToRun()
    {
        var settings = new Dictionary<string, string?>
        {
            ["DemoMode:Enabled"] = "true",
            ["Database:Provider"] = "Sqlite",
            ["Seed:EnableDemoData"] = "true"
        };

        var exception = Record.Exception(() => Check(settings));

        Assert.Null(exception);
    }

    [Fact]
    public void DevelopmentIsNeverChecked()
    {
        var settings = new Dictionary<string, string?> { ["Database:Provider"] = "Sqlite" };

        var exception = Record.Exception(() => Check(settings, "Development"));

        Assert.Null(exception);
    }

    private static void Check(Dictionary<string, string?> settings, string environmentName = "Production")
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var environment = new HostingEnvironment { EnvironmentName = environmentName };

        ProductionReadiness.ThrowIfMisconfigured(configuration, environment, NullLogger.Instance);
    }
}
