namespace InventoryPoc.Web.Shared.Models
{

    public record Product(string? Name, string? ImagePath, decimal Cost, string? Size, string? Id)
    {
        public int Quantity { get; set; } = 0;
    }
}
