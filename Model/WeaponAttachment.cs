namespace Model;

public record class WeaponAttachment
{
    public required Guid AttachmentItemInstanceId { get; set; }
    public required Guid AttachmentItemCatalogId { get; set; }
}