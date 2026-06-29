using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class WeaponData : IEquatable<WeaponData>, IInterchangeable<WeaponData, Packets.WeaponData>
{
    [SetsRequiredMembers]
    public WeaponData(Guid itemInstanceId, ActiveAlterationData[] alterationData, WeaponAttachment? attachment, string itemCatalogId)
    {
        ItemInstanceId = itemInstanceId;
        AlterationData = alterationData ?? throw new ArgumentNullException(nameof(alterationData));
        Attachment = attachment;
        ItemCatalogId = itemCatalogId;
    }

    [PgName("item_instance_id")]
    public required Guid ItemInstanceId { get; set; }
    [PgName("alteration_data")]
    public required ActiveAlterationData[] AlterationData { get; set; }
    [PgName("attachment")]
    public WeaponAttachment? Attachment { get; set; }
    [PgName("item_catalog_id")]
    public required string ItemCatalogId { get; set; }

    public static WeaponData FromPacket(Packets.WeaponData inst)
    {
        WeaponAttachment? attachment = null;
        if (inst.AttachmentItemInstanceId != "" && inst.AttachmentItemCatalogId != "")
        {
            attachment = new WeaponAttachment(Guid.Parse(inst.AttachmentItemInstanceId), inst.AttachmentItemCatalogId);
        }
        return new WeaponData(Guid.Parse(inst.ItemInstanceId), inst.AlterationData.Select(altData => ActiveAlterationData.FromPacket(altData)).ToArray(), attachment, inst.ItemCatalogId);
    }

    public virtual bool Equals(WeaponData? other)
    {
        return other is not null && (ReferenceEquals(this, other) || ((Attachment != null || other.Attachment == null) && ItemInstanceId == other.ItemInstanceId
            && AlterationData.SequenceEqual(other.AlterationData)
            && (Attachment == null || Attachment.Equals(other.Attachment))
            && ItemCatalogId == other.ItemCatalogId));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(ItemInstanceId);
        hash.Add(ItemCatalogId);
        hash.Add(AlterationData);
        if (Attachment != null)
        {
            hash.Add(Attachment);
        }
        return hash.ToHashCode();
    }

    public Packets.WeaponData ToPacket()
    {
        Packets.WeaponData packet = new()
        {
            ItemInstanceId = ItemInstanceId.ToString(),
            ItemCatalogId = ItemCatalogId,
        };
        if (Attachment != null)
        {
            packet.AttachmentItemInstanceId = Attachment.AttachmentItemInstanceId.ToString();
            packet.AttachmentItemCatalogId = Attachment.AttachmentItemCatalogId;
        }
        foreach (ActiveAlterationData alt in AlterationData)
        {
            packet.AlterationData.Add(alt.ToPacket());
        }
        return packet;
    }
}