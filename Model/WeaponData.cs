namespace Model;

public record class WeaponData
{
    public required string ItemInstanceId { get; set; }
    public required ActiveAlterationData[] AlterationData { get; set; }
    public WeaponAttachment? Attachment { get; set; }
    public required string ItemCatalogId { get; set; }
}