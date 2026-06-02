namespace Model;

public record class OutfitData
{
    public required Guid ItemInstanceId { get; set; }
    public required ActiveAlterationData[] AlterationData { get; set; }
    public required Guid ItemCatalogId { get; set; }
}