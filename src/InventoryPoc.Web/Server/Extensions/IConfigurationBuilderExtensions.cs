namespace InventoryPoc.Web.Server.Extensions
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

            return config.SetBasePath(path)
                .AddJsonFile("common.appsettings.json", false, true)
                .AddJsonFile($"common.appsettings.{environment}.json", true, true);
        }
    }
}
