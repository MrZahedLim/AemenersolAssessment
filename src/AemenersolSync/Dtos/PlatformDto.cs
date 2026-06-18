using System.Text.Json.Serialization;

namespace AemenersolSync.Dtos;

// Map JSON dari API. Guna nullable supaya kalau key hilang (cth Dummy takde
// createdAt/updatedAt) tak crash, jadi null je.
public class PlatformDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

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

    [JsonPropertyName("well")]
    public List<WellDto> Well { get; set; } = new();
}
