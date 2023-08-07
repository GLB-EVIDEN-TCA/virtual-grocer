using Eviden.VirtualGrocer.Web.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddHttpClient("Eviden.VirtualGrocer.Web.Client.ServerAPI", client => 
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("Eviden.VirtualGrocer.Web.Client.ServerAPI"));
await builder.Build().RunAsync();

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("{SCOPE URI}");
});
//add DI bin to client side
//builder.Services.AddSingleton<IConfiguration>(config);
//builder.Services.AddAzureSearch(azureSearchEndpoint!, azureSearchIndex!, azureSearchKey!);
//builder.Services.AddAzureChatCompletion(azureAiEndpoint!, azureAiModel!, azureAiKey!);

