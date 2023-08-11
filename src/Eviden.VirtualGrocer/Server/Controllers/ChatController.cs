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

namespace Eviden.VirtualGrocer.Web.Server.Controllers
{
    [ApiController]
    [Authorize]
    //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IKernel _semanticKernel;
        private readonly ILogger<ChatController> _logger;

        // this is hacky; should stash chat history in a repo so it is unique per user
        private ChatHistory? _chatHistory;

        public ChatController(
            IKernel semanticKernel,
            SearchClient searchClient,
            IConfiguration config,
            ILogger<ChatController> logger)
        {
            _semanticKernel = semanticKernel;
            _logger = logger;

            semanticKernel.ImportSkill(new QueryBuilderSkill(), "Inventory");
            semanticKernel.ImportSkill(new InventoryLookupSkill(searchClient), "Inventory");
            semanticKernel.ImportSkill(new RememberShoppingList(), "Inventory");
            semanticKernel.ImportSkill(new RenderOutput($"{config["Azure:Storage:ProductImagePath"]}"), "Inventory");
        }

        [HttpPost]
        public async Task<ChatMessage> Post([FromBody] ChatPrompt prompt)
        {
            _logger.LogDebug($"Calling {nameof(ChatController)}.{nameof(Post)} with {nameof(prompt)} = \"{prompt.Prompt}\"");

            SKContext? skContext = null;
            try
            {
                //Call Semantic Kernel
                skContext = await _semanticKernel.RunAsync(
                    prompt.Prompt!,
                    _semanticKernel.Skills.GetFunction("Inventory", "PersonalShopper"),
                    _semanticKernel.Skills.GetFunction("Inventory", "Store"),
                    _semanticKernel.Skills.GetFunction("Inventory", "BuildQuery"),
                    _semanticKernel.Skills.GetFunction("Inventory", "Lookup"),
                    _semanticKernel.Skills.GetFunction("Inventory", "Render"));

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

					return new ChatMessage { PreContent = skContext.Result, IsError = true, ErrorMessage = errorMessage };
                }

                return JsonSerializer.Deserialize<ChatMessage>(skContext.Result) ?? new ChatMessage();
            }
            catch (Exception ex)
            {
                if (skContext != null)
                {
                    return new ChatMessage { PreContent = skContext.Result, IsError = true, ErrorMessage = ex.ToString() };
                }

                return new ChatMessage
                {
                    PreContent = "Woah, I did not expect you to say that. Try asking something else!",
                    IsError = true
                };
            }
        }
    }
}