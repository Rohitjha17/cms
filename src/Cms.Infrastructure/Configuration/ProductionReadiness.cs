using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cms.Infrastructure.Configuration;

/// <summary>
/// Refuses to start a production deployment that is not actually configured for production.
///
/// The container image carries demo settings so the sample workspace runs out of the box: demo
/// content, a published sign-in password and a file database. Deployed as-is those become a live
/// school's website — running on someone else's demo data, reachable with a password printed in
/// this repository, storing everything in a file that a restart discards.
///
/// None of that announces itself. The site comes up and looks fine. So rather than let it, the
/// application stops on the first run and says exactly what is missing.
/// </summary>
public static class ProductionReadiness
{
    /// <summary>Settings a real deployment must supply, checked before the first request.</summary>
    public static void ThrowIfMisconfigured(
        IConfiguration configuration, IHostEnvironment environment, ILogger logger)
    {
        if (!environment.IsProduction())
        {
            return;
        }

        if (configuration.GetValue<bool>("DemoMode:Enabled"))
        {
            logger.LogWarning(
                "Running in demo mode: sample content is seeded, an unmapped domain falls back to "
                + "the demo workspace, and the demo sign-in password applies. Set "
                + "DemoMode__Enabled=false and Seed__EnableDemoData=false for a real deployment.");
            return;
        }

        var problems = new List<string>();
        var provider = configuration.GetValue<string>("Database:Provider") ?? "SqlServer";

        if (string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            problems.Add(
                "Database__Provider is Sqlite. That is a single file, meant for one person on one "
                + "machine; a restart on most hosting discards it. Use SqlServer.");
        }

        var connectionName = string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase)
            ? "Sqlite"
            : "DefaultConnection";
        var connection = configuration.GetConnectionString(connectionName);

        if (string.IsNullOrWhiteSpace(connection))
        {
            problems.Add($"ConnectionStrings__{connectionName} is empty.");
        }
        else if (connection.Contains("SET_VIA_ENVIRONMENT", StringComparison.OrdinalIgnoreCase))
        {
            problems.Add(
                $"ConnectionStrings__{connectionName} still holds the placeholder from the repository.");
        }

        if (configuration.GetValue<bool>("Seed:EnableDemoData"))
        {
            problems.Add(
                "Seed__EnableDemoData is true. A real school's console would be filled with the "
                + "demo academy's pages, staff and notices.");
        }

        foreach (var (key, whatItIsFor) in new[]
                 {
                     ("Platform:Domain", "the address the console is reached at"),
                     ("Platform:SuperAdminEmail", "the first account that can sign in"),
                     ("Platform:SuperAdminPassword", "that account's password")
                 })
        {
            if (string.IsNullOrWhiteSpace(configuration[key]))
            {
                problems.Add($"{key.Replace(":", "__")} is empty — {whatItIsFor}.");
            }
        }

        // Only the API signs tokens; the other applications have no Jwt section at all.
        if (configuration.GetSection("Jwt").Exists())
        {
            var key = configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(key) || key.Length < 32)
            {
                problems.Add("Jwt__Key must be set and at least 32 characters.");
            }
            else if (key.Contains("Development", StringComparison.OrdinalIgnoreCase))
            {
                problems.Add("Jwt__Key is still the development key from the repository.");
            }
        }

        if (string.Equals(configuration.GetValue<string>("Storage:Provider") ?? "Local", "Local",
                StringComparison.OrdinalIgnoreCase))
        {
            // Survivable with a persistent disk, so this is a warning rather than a refusal.
            logger.LogWarning(
                "Storage__Provider is Local: uploaded photographs and documents are written to this "
                + "machine's disk. Unless a persistent disk is mounted they are lost on the next "
                + "deployment. Storage__Provider=S3 avoids the question.");
        }

        if (problems.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "This deployment is not configured for production, and starting anyway would put a "
            + "school's website on demo settings. Set the following in the hosting environment:"
            + Environment.NewLine
            + string.Join(Environment.NewLine, problems.Select(problem => "  - " + problem)));
    }
}
