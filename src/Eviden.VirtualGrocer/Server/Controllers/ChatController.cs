using System.Text.Json;
using Azure.Search.Documents;
using Eviden.VirtualGrocer.Web.Server.Skills;
using Eviden.VirtualGrocer.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;
using Eviden.VirtualGrocer.Web.Server.Skills.History;

namespace Eviden.VirtualGrocer.Web.Server.Controllers
{
    [ApiController]
    [Authorize]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IKernel _semanticKernel;
        private readonly ILogger<ChatController> _logger;

        private readonly ChatRepository _chatRepo;
        private readonly ITokenCounter _tokenCounter;
        private readonly ResultRepository _resultRepo;

        public ChatController(
            IKernel semanticKernel,
            SearchClient searchClient,
            IConfiguration config,
            ILogger<ChatController> logger,
            ChatRepository chatRepo,
            ResultRepository resultRepo,
            ITokenCounter tokenCounter)
        {
            _resultRepo = resultRepo;
            _chatRepo = chatRepo;
            _tokenCounter = tokenCounter;
            _semanticKernel = semanticKernel;
            _logger = logger;

            //semanticKernel.ImportSkill(new QueryBuilderSkill(), "Inventory");
            //semanticKernel.ImportSkill(new InventoryLookupSkill(searchClient), "Inventory");
            //semanticKernel.ImportSkill(new RememberShoppingList(), "Inventory");
            //semanticKernel.ImportSkill(new RenderOutput($"{config["Azure:Storage:ProductImagePath"]}"), "Inventory");
        }

        [HttpPost]
        public async Task<ChatMessage> Post([FromBody] ChatPrompt prompt)
        {
            _logger.LogDebug($"Calling {nameof(ChatController)}.{nameof(Post)} with {nameof(prompt)} = \"{prompt.Prompt}\"");

            // save user prompt to chat history (prompt)
            var history = await _chatRepo.GetAsync(prompt.ChatId);

            // TODO: add chat history section to prompt, identify budget, concat history and submit with prompt!
            // don't think adding chat history will work unless we add context inferrence, where 'I want to buy X' will set the context to 'buy'; further asking 'and Y' would reuse the previous context.

            SKContext? skContext = null;
            try
            {
                ContextVariables variables = new ContextVariables(prompt.Prompt!);
                //variables.Set("chatHistory", ExtractChatHistory(history));
                variables.Set("chatId", prompt.ChatId);
                variables.Set("chatHistory", ExtractUserChatHistory(history));
                variables.Set("originalPrompt", prompt.Prompt!);

                //Call Semantic Kernel
                skContext = await _semanticKernel.RunAsync(
                    variables,
                    _semanticKernel.Skills.GetFunction("Inventory", "ItemIntent"),
                    _semanticKernel.Skills.GetFunction("Inventory", SkillNames.RenderItemIntentResponse),
                    _semanticKernel.Skills.GetFunction("Inventory", "PersonalShopper"),
                    _semanticKernel.Skills.GetFunction("Inventory", SkillNames.RememberShoppingListResult),
                    _semanticKernel.Skills.GetFunction("Inventory", SkillNames.BuildInventoryQuery),
                    _semanticKernel.Skills.GetFunction("Inventory", SkillNames.FindInventory),
                    _semanticKernel.Skills.GetFunction("Inventory", SkillNames.RenderShoppingListResponse));

                if (skContext.ErrorOccurred)
                {
                    string? errorMessage;
                    if (skContext.LastException is Microsoft.SemanticKernel.AI.AIException)
                    {
                        errorMessage = ((Microsoft.SemanticKernel.AI.AIException)skContext.LastException).Detail;
                    }
                    else
                    {
						errorMessage = skContext.LastException?.Message;
					}

					return new ChatMessage(prompt.ChatId) { PreContent = skContext.Result, IsError = true, ErrorMessage = errorMessage };
                }

                history.Add("User", prompt.Prompt!);

                // save response to chat history (skContext.Result)
                history.Add("Bot", skContext.Result!);
                await _chatRepo.StashAsync(history);

                return JsonSerializer.Deserialize<ChatMessage>(skContext.Result) ?? new ChatMessage(prompt.ChatId);
            }
            catch (Exception ex)
            {
                if (skContext != null)
                {
                    return new ChatMessage(prompt.ChatId) { PreContent = skContext.Result, IsError = true, ErrorMessage = ex.ToString() };
                }

                return new ChatMessage(prompt.ChatId)
                {
                    PreContent = "Woah, I did not expect you to say that. Try asking something else!",
                    IsError = true
                };
            }
        }

        private string ExtractUserChatHistory(Skills.History.ChatHistory history)
        {
            int budget = 1000;
            string log = history.ConcatMessageHistory(_tokenCounter, budget, x => x.StartsWith("User"));
            return log;
        }
    }
}