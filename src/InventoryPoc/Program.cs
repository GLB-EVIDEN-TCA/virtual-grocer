using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using System.Reflection;

// Set up configuration
var assemblyDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
var builder = new ConfigurationBuilder().InitializeCommonConfiguration(assemblyDirectory);

IConfiguration configuration = builder.Build();

var apiKey = configuration["Azure:OpenAI:ApiKey"];
var azureEndpoint = configuration["Azure:OpenAI:Endpoint"];
var model = configuration["Azure:OpenAI:Model"];

var kernel = KernelBuilder.Create();
kernel.Config.AddAzureTextCompletionService(model!, azureEndpoint!, apiKey!);

var skillsDirectory = Path.Combine(assemblyDirectory!, "skills");
var invSkill = kernel.ImportSemanticSkillFromDirectory(skillsDirectory, "Inventory");
var querySkill = kernel.ImportSkill(new QueryBuilderSkill());

Console.WriteLine("Hey there shopper! How can I help you today?");

string userInput;
do
{
    Console.Write("Prompt (type 'quit' to exit): ");
    userInput = Console.ReadLine()!;

    if (string.IsNullOrWhiteSpace(userInput))
    {
        userInput = "I want to make chocolate cake.";
    }

    if (userInput.ToLower() != "quit")
    {
        var r0 = await kernel.RunAsync(userInput, invSkill["PersonalShopper2"], querySkill["BuildQuery"]);
        //var result = await kernel.RunAsync(userInput, skill["ShopQueryBuilder"]);

        Console.WriteLine("From az openai:");
        Console.WriteLine(r0);
        //Console.WriteLine(result);

        ////var cog = new CogSearch(configuration);
        ////Console.WriteLine("From az cog search:");
        ////Console.WriteLine();
        ////cog.SearchAndPrint(r0.Result);
        ////cog.SearchAndPrint(result.Result.Trim());
        Console.WriteLine();
        Console.WriteLine("Anything else?");
    }
} while (userInput.ToLower() != "quit");