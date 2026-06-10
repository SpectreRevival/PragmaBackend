using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class OutfitData : IEquatable<OutfitData>
{
    [SetsRequiredMembers]
    public OutfitData(Guid itemInstanceId, ActiveAlterationData[] alterationData, string itemCatalogId)
    {
        ItemInstanceId = itemInstanceId;
        AlterationData = alterationData ?? throw new ArgumentNullException(nameof(alterationData));
        ItemCatalogId = itemCatalogId;
    }

    [PgName("item_instance_id")]
    public required Guid ItemInstanceId { get; set; }
    [PgName("alteration_data")]
    public required ActiveAlterationData[] AlterationData { get; set; }
    [PgName("item_catalog_id")]
    public required string ItemCatalogId { get; set; }

    public virtual bool Equals(OutfitData? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return ItemInstanceId == other.ItemInstanceId
            && ItemCatalogId == other.ItemCatalogId
            && AlterationData.SequenceEqual(other.AlterationData);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ItemCatalogId);
        hash.Add(ItemInstanceId);
        hash.Add(AlterationData);
        return hash.ToHashCode();
    }
}