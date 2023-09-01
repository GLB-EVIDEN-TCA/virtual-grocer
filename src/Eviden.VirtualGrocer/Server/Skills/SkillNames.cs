namespace Eviden.VirtualGrocer.Web.Server.Skills
{
    public static class SkillNames
    {
        public const string FindInventory = nameof(InventorySearchSkill.FindInventory);
        public const string BuildInventoryQuery = nameof(InventorySearchSkill.BuildInventoryQuery);
        public const string RememberShoppingListResult = nameof(RememberShoppingListSkill.RememberShoppingListResult);
        public const string RenderShoppingListResponse = nameof(RenderOutputSkill.RenderShoppingListResponse);
    }
}
