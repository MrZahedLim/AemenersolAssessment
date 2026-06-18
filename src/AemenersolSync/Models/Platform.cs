namespace AemenersolSync.Models;

public class Platform
{
    // Id ikut dari API (bukan auto-generate) supaya boleh upsert by id
    public int Id { get; set; }
    public string UniqueName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<Well> Wells { get; set; } = new();
}
