using System.Text.Json.Serialization;

namespace BlazorApp.DTO;

public class ItemSummaryDto
{
    public int Id { get; set; }

    [JsonPropertyName("itemName")]
    public string Name { get; set; } = string.Empty;

    public string MemberName { get; set; } = string.Empty;

    public List<string> Categories { get; set; } = new();

    public string Description { get; set; } = string.Empty;

    public decimal? Price { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [JsonIgnore]
    public string? ImageUrl { get; set; }
}