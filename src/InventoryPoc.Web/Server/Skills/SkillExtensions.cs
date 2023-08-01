using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SemanticFunctions;
using System.Reflection;

namespace InventoryPoc.Web.Server.Skills
{
    public static class SkillExtensions
    {
        public static IKernel AddEmbeddedSkills(this IKernel kernel)
        {
            string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            // discover all semantic skill functions, grouping by skill/function; ordering is done to ensure the config.json is processed first.
            // surprisingly, this is not a personal record for number of LINQ statements.
            var functions =
                from resourceName in resourceNames
                where resourceName.EndsWith("skprompt.txt", StringComparison.OrdinalIgnoreCase) ||
                    resourceName.EndsWith("config.json", StringComparison.OrdinalIgnoreCase)
                let skillBitsFull = resourceName.Split('.')
                where skillBitsFull.Length >= 4
                let skillBits = skillBitsFull[^4..]
                let skillName = skillBits[0]
                let functionName = skillBits[1]
                let resource = new FunctionResource(skillName, functionName, resourceName)
                orderby resource.ResourceName
                group resource by $"{resource.SkillName}.{functionName}" into g
                where g.Count() == 2
                select new SkillResources(g.First(), g.Last());

            foreach(var function in functions)
            {
                var config = PromptTemplateConfig.FromJson(function.Config.ReadText());
                var template = new PromptTemplate(function.Prompt.ReadText(), config, kernel.PromptTemplateEngine);
                var functionConfig = new SemanticFunctionConfig(config, template);
                kernel.RegisterSemanticFunction(function.SkillName, function.FunctionName, functionConfig);
            }

            return kernel;
        }
    }

    internal record FunctionResource(string SkillName, string FunctionName, string ResourceName)
    {
        public string ReadText()
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)!;
            using StreamReader reader = new(stream);
            return reader.ReadToEnd();
        }
    }

    internal record SkillResources(FunctionResource Config, FunctionResource Prompt)
    {
        public string SkillName => Config.SkillName;

        public string FunctionName => Config.FunctionName;
    }
}
