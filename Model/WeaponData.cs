namespace Model;

public record class WeaponData
{
    public required Guid ItemInstanceId { get; set; }
    public required ActiveAlterationData[] AlterationData { get; set; }
    public WeaponAttachment? Attachment { get; set; }
    public required Guid ItemCatalogId { get; set; }
}