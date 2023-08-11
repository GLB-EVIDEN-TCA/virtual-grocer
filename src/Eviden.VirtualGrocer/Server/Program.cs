using Eviden.VirtualGrocer.Web.Server;
using Eviden.VirtualGrocer.Web.Server.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Initialize the configuration
var config = builder.Configuration;
config.InitializeCommonConfiguration(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)!);

var azureAiKey = config["Azure:OpenAI:ApiKey"];
var azureAiEndpoint = config["Azure:OpenAI:Endpoint"];
var azureAiModel = config["Azure:OpenAI:Model"];
var azureSearchEndpoint = config["Azure:CognitiveSearch:Endpoint"];
var azureSearchKey = config["Azure:CognitiveSearch:QueryKey"];
var azureSearchIndex = config["Azure:CognitiveSearch:Index"];

// Add services to the container.

// Sign-in users with the Microsoft identity platform
builder.Services.AddMicrosoftIdentityWebAppAuthentication(config);

/*builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();*/

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register objects in the DI container
builder.Services.AddSingleton<IConfiguration>(config);
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