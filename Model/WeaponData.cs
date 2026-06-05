using NpgsqlTypes;

namespace Model;

public record class WeaponData : IEquatable<WeaponData>
{
    [PgName("item_instance_id")]
    public required Guid ItemInstanceId { get; set; }
    [PgName("alteration_data")]
    public required ActiveAlterationData[] AlterationData { get; set; }
    [PgName("attachment")]
    public WeaponAttachment? Attachment { get; set; }
    [PgName("item_catalog_id")]
    public required Guid ItemCatalogId { get; set; }

    public virtual bool Equals(WeaponData? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Attachment == null && other.Attachment != null) return false;

        return ItemInstanceId == other.ItemInstanceId
            && AlterationData.SequenceEqual(other.AlterationData)
            && (Attachment == null || Attachment.Equals(other.Attachment))
            && ItemCatalogId == other.ItemCatalogId;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ItemInstanceId);
        hash.Add(ItemCatalogId);
        hash.Add(AlterationData);
        if(Attachment != null)
        {
            hash.Add(Attachment);
        }
        return hash.ToHashCode();
    }
}