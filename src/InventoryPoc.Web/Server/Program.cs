using Eviden.VirtualGrocer.Web.Server;
using Eviden.VirtualGrocer.Web.Server.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Initialize the configuration
var config = builder.Configuration;
config.InitializeCommonConfiguration(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)!);

var azureAiKey = config["Azure:OpenAI:ApiKey"];
var azureAiEndpoint = config["Azure:OpenAI:Endpoint"];
var azureAiModel = config["Azure:OpenAI:Model"];
var azureSearchEndpoint = config["Azure:CognitiveSearch:Endpoint"];
var azureSearchKey = config["Azure:CognitiveSearch:QueryKey"];
var azureSearchIndex = config["Azure:CognitiveSearch:Index"];

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

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();