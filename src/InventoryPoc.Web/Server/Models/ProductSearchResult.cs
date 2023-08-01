using InventoryPoc.Web.Shared.Models;
using System.Text.Json.Serialization;

namespace InventoryPoc.Web.Server.Models
{
    public record ProductSearchResult(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("image_name")] string? ImageName,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("cost")] decimal Cost,
        [property: JsonPropertyName("size")] string? Size)
    {
        public Product ToProduct(string imagePath) => new Product(Name, $"{imagePath}{ImageName}", Cost, Size, Id);
    }
}
