using System.Text.Json.Serialization;

namespace AemenersolSync.Dtos;

public class WellDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("platformId")]
    public int PlatformId { get; set; }

    [JsonPropertyName("uniqueName")]
    public string? UniqueName { get; set; }

    [JsonPropertyName("latitude")]
    public decimal Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public decimal Longitude { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}
