namespace Model;

public record class WeaponAttachment
{
    public required string AttachmentItemInstanceId { get; set; }
    public required string AttachmentItemCatalogId { get; set; }
}