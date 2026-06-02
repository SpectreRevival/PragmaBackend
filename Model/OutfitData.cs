namespace Model;

public record class OutfitData
{
    public required string ItemInstanceId { get; set; }
    public required ActiveAlterationData[] AlterationData { get; set; }
    public required string ItemCatalogId { get; set; }
}