using Eviden.VirtualGrocer.Web.Server.Models;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Eviden.VirtualGrocer.Web.Server.Skills
{
    public class RememberShoppingListSkill
    {
        [SKFunction("Remember the shopping list from the Personal Shopper skill.")]
        [SKFunctionName(SkillNames.RememberShoppingListResult)]
        public string RememberShoppingListResult(string shoppingList, SKContext context)
        {
            PersonalShopperCompletionResult result =
				System.Text.Json.JsonSerializer.Deserialize<PersonalShopperCompletionResult>(shoppingList)!;
            string json = System.Text.Json.JsonSerializer.Serialize(result);
            context.Variables["shoppingList"] = json;
            return json;
        }
    }
}
