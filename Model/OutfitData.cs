using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class OutfitData : IEquatable<OutfitData>, IInterchangeable<OutfitData, Packets.OutfitData>
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

    public static OutfitData FromPacket(Packets.OutfitData inst)
    {
        return new OutfitData(Guid.Parse(inst.ItemInstanceId), inst.AlterationData.Select(altData => ActiveAlterationData.FromPacket(altData)).ToArray(), inst.ItemCatalogId);
    }

    public virtual bool Equals(OutfitData? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (ItemInstanceId == other.ItemInstanceId
            && ItemCatalogId == other.ItemCatalogId
            && AlterationData.SequenceEqual(other.AlterationData)));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(ItemCatalogId);
        hash.Add(ItemInstanceId);
        hash.Add(AlterationData);
        return hash.ToHashCode();
    }

    public Packets.OutfitData ToPacket()
    {
        Packets.OutfitData packet = new()
        {
            ItemInstanceId = ItemInstanceId.ToString(),
            ItemCatalogId = ItemCatalogId.ToString()
        };
        foreach (ActiveAlterationData alt in AlterationData)
        {
            packet.AlterationData.Add(alt.ToPacket());
        }
        return packet;
    }
}