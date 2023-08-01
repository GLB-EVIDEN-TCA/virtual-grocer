using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel;
using InventoryPoc.Configuration;

namespace InventoryPoc.Skills
{
    /// <summary>
    /// A Semantic Kernel skill that interacts with ChatGPT
    /// </summary>
    internal class RecipeSkill
    {
        private readonly IChatCompletion _chatCompletion;
        private readonly OpenAIChatHistory _chatHistory;
        private readonly ChatRequestSettings _chatRequestSettings;

        public RecipeSkill(IKernel semanticKernel, IOptions<OpenAiServiceOptions> openAIOptions)
        {
            // Set up the chat request settings
            _chatRequestSettings = new ChatRequestSettings
            {
                MaxTokens = openAIOptions.Value.MaxTokens,
                Temperature = openAIOptions.Value.Temperature,
                FrequencyPenalty = openAIOptions.Value.FrequencyPenalty,
                PresencePenalty = openAIOptions.Value.PresencePenalty,
                TopP = openAIOptions.Value.TopP
            };

            // Configure the semantic kernel
            semanticKernel.Config.AddOpenAIChatCompletionService("chat", openAIOptions.Value.ChatModel,
                openAIOptions.Value.Key);

            // Set up the chat completion and history - the history is used to keep track of the conversation
            // and is part of the prompt sent to ChatGPT to allow a continuous conversation
            _chatCompletion = semanticKernel.GetService<IChatCompletion>();
            _chatHistory = (OpenAIChatHistory)_chatCompletion.CreateNewChat(openAIOptions.Value.SystemPrompt);
        }

        /// <summary>
        /// Send a prompt to the LLM.
        /// </summary>
        [SKFunction("Send a prompt to the LLM.")]
        [SKFunctionName("Prompt")]
        public async Task<string> Prompt(string prompt)
        {
            string reply;
            try
            {
                // Add the question as a user message to the chat history, then send everything to OpenAI.
                // The chat history is used as context for the prompt
                _chatHistory.AddUserMessage(prompt);
                reply = await _chatCompletion.GenerateMessageAsync(_chatHistory, _chatRequestSettings);

                // Add the interaction to the chat history.
                _chatHistory.AddAssistantMessage(reply);
            }
            catch (AIException ex)
            {
                // Reply with the error message if there is one
                reply = $"OpenAI returned an error ({ex.Message}). Please try again.";
            }

            return reply;
        }

        /// <summary>
        /// Log the history of the chat with the LLM.
        /// This will log the system prompt that configures the chat, along with the user and assistant messages.
        /// </summary>
        [SKFunction("Log the history of the chat with the LLM.")]
        [SKFunctionName("LogChatHistory")]
        public Task LogChatHistory()
        {
            Console.WriteLine();
            Console.WriteLine("Chat history:");
            Console.WriteLine();

            // Log the chat history including system, user and assistant (AI) messages
            foreach (var message in _chatHistory.Messages)
            {
                // Depending on the role, use a different color
                var role = message.AuthorRole;
                switch (role)
                {
                    case ChatHistory.AuthorRoles.System:
                        role = ChatHistory.AuthorRoles.System;
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    case ChatHistory.AuthorRoles.User:
                        role = ChatHistory.AuthorRoles.User;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case ChatHistory.AuthorRoles.Assistant:
                        role = ChatHistory.AuthorRoles.Assistant;
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                }

                // Write the role and the message
                Console.WriteLine($"{role}{message.Content}");
            }

            return Task.CompletedTask;
        }
    }
}
