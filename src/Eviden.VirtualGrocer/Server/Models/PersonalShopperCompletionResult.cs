namespace Eviden.VirtualGrocer.Web.Server.Models
{
    public record PersonalShopperCompletionResult(
        IEnumerable<RecipeCompletionResult> Recipes,
        IEnumerable<string> ShoppingListItems,
        IEnumerable<string> OtherContent);
}
