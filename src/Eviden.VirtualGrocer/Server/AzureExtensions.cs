using Azure;
using Azure.Search.Documents;
using Eviden.VirtualGrocer.Web.Server.Skills;
using Microsoft.SemanticKernel;

namespace Eviden.VirtualGrocer.Web.Server
{
    public static class AzureExtensions
    {
        /// <summary>
        /// Delegate to register skills with a Semantic Kernel
        /// </summary>
        public delegate Task RegisterSkillsWithKernel(IServiceProvider sp, IKernel kernel);

        public static IServiceCollection AddAzureSearch(
            this IServiceCollection services,
            string endpoint,
            string index,
            string key)
        {
            Uri uri = new Uri(endpoint);
            AzureKeyCredential credential = new AzureKeyCredential(key);
            return services.AddSingleton(new SearchClient(uri, index, credential));
        }

        public static IServiceCollection AddAzureChatCompletion(
            this IServiceCollection services,
            string endpoint,
            string model,
            string key)
        {
            services.AddScoped(
                sp =>
                {
                    IKernel kernel = Kernel.Builder.WithAzureChatCompletionService(model, endpoint, key).Build();
                    sp.GetRequiredService<RegisterSkillsWithKernel>()(sp, kernel);

                    return kernel;
                });

            services.AddScoped<RegisterSkillsWithKernel>(sp => RegisterSkillsAsync);

            return services;
        }

        private static Task RegisterSkillsAsync(IServiceProvider sp, IKernel kernel)
        {
            kernel.AddEmbeddedSkills();

            //kernel.ImportSkill(new QueryBuilderSkill(), "Inventory");
            kernel.ImportSkill(new InventorySearchSkill(sp.GetRequiredService<SearchClient>()), "Inventory");
            kernel.ImportSkill(new RememberShoppingListSkill(), "Inventory");
            kernel.ImportSkill(
                new RenderOutputSkill(
                    $"{sp.GetRequiredService<IConfiguration>()["Azure:Storage:ProductImagePath"]}"), "Inventory");

            return Task.CompletedTask;
        }
    }
}
