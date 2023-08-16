namespace Eviden.VirtualGrocer.Shared.Models
{
    public record ChatMessage(string ChatId)
    {
		public string? PreContent { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; } = string.Empty;

		public string? InventoryContent { get; set; } = string.Empty;
        public string? RecipeContent { get; set; } = string.Empty;
        public bool IsUser { get; set; }
        public bool IsLoading { get; set; }
        public bool IsError { get; set; }
        public bool IsNonInteractive { get; set; }

		public Product[]? Products { get; set; } = Array.Empty<Product>();
        public Recipe[]? Recipes { get; set; } = Array.Empty<Recipe>();
    }
}