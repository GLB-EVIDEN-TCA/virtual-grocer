using Eviden.VirtualGrocer.Web.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Initialize the configuration
var config = builder.Configuration;

//Log appSettings
var logger = LoggerFactory.Create(config =>
{
    config.AddConsole();
}).CreateLogger("Program");
logger.LogInformation(config["Azure:KeyVault:Uri"]);
logger.LogInformation(config["Azure:OpenAI:Endpoint"]);
logger.LogInformation(config["Azure:OpenAI:Model"]);
logger.LogInformation(config["Azure:CognitiveSearch:Endpoint"]);

// Register Azure KeyVault
var keyVaultUri = config["Azure:KeyVault:Uri"];

try
{
    config.AddAzureKeyVault(
            new Uri(keyVaultUri!),
            new DefaultAzureCredential()
        );
}
catch(Exception ex)
{
    logger.LogWarning(ex, "Unable to load Azure KeyVault configuration.");
}

// Register Azure Cognitive and Search services
var azureAiKey = config["azure-openai-key"];
var azureAiEndpoint = config["Azure:OpenAI:Endpoint"];
var azureAiModel = config["Azure:OpenAI:Model"];
var azureSearchEndpoint = config["Azure:CognitiveSearch:Endpoint"];
var azureSearchKey = config["cognitive-search-key"];
var azureSearchIndex = config["Azure:CognitiveSearch:Index"];

// Register objects and services in the DI container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Sign-in users with the Microsoft identity platform
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(config.GetSection("AzureAd"));

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