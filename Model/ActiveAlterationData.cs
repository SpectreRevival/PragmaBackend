using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class ActiveAlterationData : IEquatable<ActiveAlterationData>, IInterchangeable<ActiveAlterationData, Packets.ActiveAlterationData>
{
    [SetsRequiredMembers]
    public ActiveAlterationData(string channelId, string alterationId)
    {
        ChannelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
        AlterationId = alterationId ?? throw new ArgumentNullException(nameof(alterationId));
    }

    [PgName("channel_id")]
    public required string ChannelId { get; set; }
    [PgName("alteration_id")]
    public required string AlterationId { get; set; }

    public static ActiveAlterationData FromPacket(Packets.ActiveAlterationData inst)
    {
        return new ActiveAlterationData(inst.ChannelId, inst.AlterationId);
    }

    public virtual bool Equals(ActiveAlterationData? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (ChannelId == other.ChannelId &&
            AlterationId == other.AlterationId));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(ChannelId);
        hash.Add(AlterationId);
        return hash.ToHashCode();
    }

    public Packets.ActiveAlterationData ToPacket()
    {
        Packets.ActiveAlterationData ret = new()
        {
            AlterationId = AlterationId,
            ChannelId = ChannelId
        };
        return ret;
    }
}