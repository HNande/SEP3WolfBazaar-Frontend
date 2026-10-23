using System.ComponentModel.DataAnnotations;

namespace BlazorApp.DTO;

public class ItemDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, MinimumLength = 8, ErrorMessage = "Name must be between 8 and 200 characters.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(500, MinimumLength = 20, ErrorMessage = "Description must be between 20 and 500 characters.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "Price must be a positive number.")]
    public int Price { get; set; }

    public List<string> Photos { get; set; } = new();

    [Required(ErrorMessage = "Keyword is required.")]
    public string Keyword { get; set; }
}