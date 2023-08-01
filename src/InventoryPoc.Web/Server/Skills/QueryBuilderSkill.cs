using System.Text.Json;
using InventoryPoc.Web.Server.Models;
using InventoryPoc.Web.Shared.Models;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace InventoryPoc.Web.Server.Skills;

public class QueryBuilderSkill
{
    [SKFunction("Build a search query from the input.")]
    [SKFunctionContextParameter(Name = "shoppingList")]
    [SKFunctionName("BuildQuery")]
    public string BuildQuery(SKContext context)
    {
        var itemJson = context.Variables["shoppingList"];
        var shoppingList = JsonSerializer.Deserialize<PersonalShopperCompletionResult>(itemJson);

        IEnumerable<string> terms =
            shoppingList!.Recipes
            .SelectMany(x => (x.Ingredients ?? Enumerable.Empty<Ingredient>()).Select(y => y.Name))
            .Concat(shoppingList.ShoppingListItems)
            .Distinct(StringComparer.InvariantCultureIgnoreCase)
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => BuildQueryForItem(x!));

        var query = string.Join(" or ", terms);
        return query;
    }

    private static string BuildQueryForItem(string item)
    {
        return $"\"{item}\"~";
        //return $"(\"{item}\" or {item})";
    }
}