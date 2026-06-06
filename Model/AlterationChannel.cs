using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class AlterationChannel : IEquatable<AlterationChannel>
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

    public virtual bool Equals(AlterationChannel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ChannelId == other.ChannelId
             && Alterations.SequenceEqual(other.Alterations);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ChannelId);
        hash.Add(Alterations);
        return hash.ToHashCode();
    }
}