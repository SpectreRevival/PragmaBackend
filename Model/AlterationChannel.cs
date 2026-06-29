using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class AlterationChannel : IEquatable<AlterationChannel>, IInterchangeable<AlterationChannel, Packets.AlterationChannel>
{
    [SetsRequiredMembers]
    public AlterationChannel(string channelId, string[] alterations)
    {
        ChannelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
        Alterations = alterations ?? throw new ArgumentNullException(nameof(alterations));
    }

    [PgName("channel_id")]
    public required string ChannelId { get; set; }

    [PgName("alterations")]
    public required string[] Alterations { get; set; }

    public static AlterationChannel FromPacket(Packets.AlterationChannel inst)
    {
        return new AlterationChannel(inst.ChannelId, inst.OwnedAlterations.ToArray());
    }

    public virtual bool Equals(AlterationChannel? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (ChannelId == other.ChannelId
             && Alterations.SequenceEqual(other.Alterations)));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(ChannelId);
        hash.Add(Alterations);
        return hash.ToHashCode();
    }

    public Packets.AlterationChannel ToPacket()
    {
        Packets.AlterationChannel ret = new()
        {
            ChannelId = ChannelId
        };
        ret.OwnedAlterations.AddRange(Alterations);
        return ret;
    }
}