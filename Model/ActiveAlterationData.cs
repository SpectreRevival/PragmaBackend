using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class ActiveAlterationData : IEquatable<ActiveAlterationData>
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

    public virtual bool Equals(ActiveAlterationData? other)
    {
        if (other is null) return false;
        if(ReferenceEquals(this, other)) return true;

        return ChannelId == other.ChannelId &&
            AlterationId == other.AlterationId;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ChannelId);
        hash.Add(AlterationId);
        return hash.ToHashCode();
    }
}