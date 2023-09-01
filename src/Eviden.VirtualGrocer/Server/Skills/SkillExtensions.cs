using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SemanticFunctions;
using System.Reflection;

namespace Eviden.VirtualGrocer.Web.Server.Skills
{
    /// <summary>
    /// Extension methods for the <see cref="IKernel"/> interface.
    /// </summary>
    public static class SkillExtensions
    {
        /// <summary>
        /// Searches for and adds all embedded skills to the kernel.
        /// </summary>
        /// <param name="kernel"></param>
        /// <returns></returns>
        public static IKernel AddEmbeddedSkills(this IKernel kernel)
        {
            // Extract all resource names from the current assembly
            string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            // Filter and group relevant resources for skills
            var skillResources = ExtractSkillResources(resourceNames);

            // Register each skill resource to the kernel
            RegisterSkills(kernel, skillResources);

            return kernel;
        }

        private static IEnumerable<SkillResources> ExtractSkillResources(string[] resourceNames)
        {
            // Identify resources related to skills (based on their extensions)
            var relevantResources =
                from resourceName in resourceNames
                where IsSkillResource(resourceName)
                select new
                {
                    ResourceName = resourceName,
                    SkillBits = ExtractSkillBits(resourceName)
                };

            // Group the resources by their skill and function names
            return from resource in relevantResources
                   let skillName = resource.SkillBits[0]
                   let functionName = resource.SkillBits[1]
                   let functionResource = new FunctionResource(skillName, functionName, resource.ResourceName)
                   orderby functionResource.ResourceName
                   group functionResource by $"{skillName}.{functionName}" into groupedResources
                   where groupedResources.Count() == 2
                   select new SkillResources(groupedResources.First(), groupedResources.Last());
        }

        private static bool IsSkillResource(string resourceName)
        {
            return resourceName.EndsWith("skprompt.txt", StringComparison.OrdinalIgnoreCase) ||
                   resourceName.EndsWith("config.json", StringComparison.OrdinalIgnoreCase);
        }

        private static string[] ExtractSkillBits(string resourceName)
        {
            var skillBitsFull = resourceName.Split('.');
            if (skillBitsFull.Length < 4)
                throw new InvalidOperationException("Unexpected resource name format.");

            return skillBitsFull[^4..];
        }

        private static void RegisterSkills(IKernel kernel, IEnumerable<SkillResources> skillResources)
        {
            foreach (var resource in skillResources)
            {
                var config = PromptTemplateConfig.FromJson(resource.config.ReadText());
                var template = new PromptTemplate(resource.prompt.ReadText(), config, kernel.PromptTemplateEngine);
                var functionConfig = new SemanticFunctionConfig(config, template);
                kernel.RegisterSemanticFunction(resource.SkillName, resource.FunctionName, functionConfig);
            }
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

    internal record SkillResources(FunctionResource config, FunctionResource prompt)
    {
        public string SkillName => config.SkillName;

        public string FunctionName => config.FunctionName;
    }
}
