using Eviden.VirtualGrocer.Shared.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Newtonsoft.Json;

namespace Eviden.VirtualGrocer.Web.Client
{
    /// <summary>
    /// Extension method container class for loading configuration from the server.
    /// </summary>
    public static class ConfigurationLoader
    {
        /// <summary>
        /// Loads the client configuration from the server and sets up authentication.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public static async Task<WebAssemblyHostBuilder> LoadConfigurationAndSetupAuthenticationAsync(this WebAssemblyHostBuilder builder)
        {
            var configuration = builder.Configuration.Build();
            var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

            var response = await httpClient.GetAsync("/clientSettings");

            if (response.IsSuccessStatusCode)
            {
                var settingsJson = await response.Content.ReadAsStringAsync();
                var settings = JsonConvert.DeserializeObject<ClientSettings>(settingsJson);

                if (settings == null)
                {
                    throw new ApplicationException("Unable to deserialize client settings from server.");
                }

                builder.Services.AddMsalAuthentication(options =>
                {
                    options.ProviderOptions.Authentication.ClientId = settings.AzureAdClientId;
                    options.ProviderOptions.Authentication.Authority = settings.AzureAdAuthority;
                    options.ProviderOptions.DefaultAccessTokenScopes.Add("api://virtual-grocer/chat");
                });
            }
            else
            {
                throw new ApplicationException($"Unable to load client configuration from server - Http Status Code: {response.StatusCode} | Reason: {response.ReasonPhrase}");
            }

            return builder;
        }
    }
}
