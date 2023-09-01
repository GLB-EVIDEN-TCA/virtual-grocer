using System.Reflection;

namespace Eviden.VirtualGrocer.Web.Server.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder InitializeCommonConfiguration(this IConfigurationBuilder config) =>
            config.InitializeCommonConfiguration(string.Empty);

        public static IConfigurationBuilder InitializeCommonConfiguration(
            this IConfigurationBuilder config,
            string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = Directory.GetCurrentDirectory();
            }

            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

            return config
                .SetBasePath(path)
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true); ;
        }
    }
}
