namespace Eviden.VirtualGrocer.Shared.Models
{
	public record Recipe(
		string? Name,
		string? Url,
		string? Description,
		string[]? Directions,
		Ingredient[]? Ingredients);
}
