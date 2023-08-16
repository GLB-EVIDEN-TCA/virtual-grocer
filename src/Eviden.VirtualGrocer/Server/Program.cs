using Eviden.VirtualGrocer.Web.Server;
using Eviden.VirtualGrocer.Web.Server.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using System.Reflection;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using System.Diagnostics;
using Azure.Search.Documents.Indexes.Models;
using System.Linq.Expressions;
using Azure.Security.KeyVault.Certificates;


var builder = WebApplication.CreateBuilder(args);

// Initialize the configuration
var config = builder.Configuration;
config.InitializeCommonConfiguration(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)!);

// Register objects and services in the DI container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


// Sign-in users with the Microsoft identity platform
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(config.GetSection("AzureAd"));

builder.Configuration.AddAzureKeyVault(
    new Uri($"https://evidenvirtualgrocer.vault.azure.net/"),
    new DefaultAzureCredential(new DefaultAzureCredentialOptions
    {
        ManagedIdentityClientId = "{System Assigned ObjectID goes here}"
    }));


var client = new SecretClient(new Uri($"https://evidenvirtualgrocer.vault.azure.net/"), new DefaultAzureCredential());
var azureAiKeyResponse = await client.GetSecretAsync("invpoc-gpt-key");
var azureAiEndpointResponse = await client.GetSecretAsync("invpoc-gpt-endpoint");
var azureSearchKeyResponse = await client.GetSecretAsync("cognitive-search-key");

string azureAiKey = azureAiKeyResponse.ToString();
string azureAiEndpoint = azureAiEndpointResponse.ToString();
string azureSearchKey = azureSearchKeyResponse.ToString();


Trace.WriteLine(azureAiKey);

// Register Azure Cognitive and Search services
//var azureAiKey = config["Azure:OpenAI:ApiKey"];
//var azureAiEndpoint = config["Azure:OpenAI:Endpoint"];
var azureAiModel = config["Azure:OpenAI:Model"];
var azureSearchEndpoint = config["Azure:CognitiveSearch:Endpoint"];
//var azureSearchKey = config["Azure:CognitiveSearch:QueryKey"];
var azureSearchIndex = config["Azure:CognitiveSearch:Index"];

builder.Services.AddAzureSearch(azureSearchEndpoint!, azureSearchIndex!, azureSearchKey!);
builder.Services.AddAzureChatCompletion(azureAiEndpoint!, azureAiModel!, azureAiKey!);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

