using DatabaseORM.Constants;
using Microsoft.Extensions.Configuration;

namespace DatabaseORM.Tools.Extensions;

public static class IConfigurationExtensions
{
    /// <summary>
    /// Checks the "Environment" value of the appsettings file.
    /// A missing value counts as Production, so a misconfigured deployment fails safe
    /// (no Swagger, strict cookies, no sensitive data logging) instead of open.
    /// </summary>
    public static bool IsProduction(this IConfiguration configuration)
    {
        string? environment = configuration[ConfigurationKeys.Environment];

        return string.IsNullOrWhiteSpace(environment) || environment == EnvironmentConstants.Production;
    }
}
