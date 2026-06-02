namespace Model;

public record class OutfitLoadout
{
    public required string PlayerId { get; set; }
    public required string LoadoutId { get; set; }
    public required OutfitData Head { get; set; }
    public required OutfitData Hair { get; set; }
    public required OutfitData FaceStyle { get; set; }
    public required OutfitData FaceAccessory { get; set; }
    public required OutfitData Outfit { get; set; }
}