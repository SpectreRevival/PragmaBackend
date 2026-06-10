using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class WeaponAttachment : IEquatable<WeaponAttachment>
{
    [SetsRequiredMembers]
    public WeaponAttachment(Guid attachmentItemInstanceId, string attachmentItemCatalogId)
    {
        AttachmentItemInstanceId = attachmentItemInstanceId;
        AttachmentItemCatalogId = attachmentItemCatalogId;
    }

    [PgName("attachment_item_instance_id")]
    public required Guid AttachmentItemInstanceId { get; set; }
    [PgName("attachment_item_catalog_id")]
    public required string AttachmentItemCatalogId { get; set; }

    public virtual bool Equals(WeaponAttachment? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return AttachmentItemCatalogId == other.AttachmentItemCatalogId
            && AttachmentItemInstanceId == other.AttachmentItemInstanceId;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(AttachmentItemInstanceId);
        hash.Add(AttachmentItemCatalogId);
        return hash.ToHashCode();
    }
}