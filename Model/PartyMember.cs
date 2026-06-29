using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class PartyMember : IEquatable<PartyMember>
{
    [SetsRequiredMembers]
    public PartyMember(Guid playerId, bool isReady, bool isLeader, string preferredTeam, bool rankedModeUnlocked, long partyMemberVersion, string region = "")
    {
        PlayerId = playerId;
        IsReady = isReady;
        IsLeader = isLeader;
        PreferredTeam = preferredTeam ?? throw new ArgumentNullException(nameof(preferredTeam));
        RankedModeUnlocked = rankedModeUnlocked;
        PartyMemberVersion = partyMemberVersion;
        Region = region ?? throw new ArgumentNullException(nameof(region));
    }

    [PgName("player_id")]
    public required Guid PlayerId { get; set; }
    [PgName("is_ready")]
    public required bool IsReady { get; set; }
    [PgName("is_leader")]
    public required bool IsLeader { get; set; }
    [PgName("preferred_team")]
    public required string PreferredTeam { get; set; }
    [PgName("ranked_mode_unlocked")]
    public required bool RankedModeUnlocked { get; set; }
    [PgName("party_member_version")]
    public required long PartyMemberVersion { get; set; }
    [PgName("region")]
    public required string Region { get; set; }

    public virtual bool Equals(PartyMember? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (PlayerId == other.PlayerId
            && IsReady == other.IsReady
            && IsLeader == other.IsLeader
            && PreferredTeam == other.PreferredTeam
            && RankedModeUnlocked == other.RankedModeUnlocked
            && PartyMemberVersion == other.PartyMemberVersion
            && Region == other.Region));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PlayerId);
        hash.Add(IsReady);
        hash.Add(IsLeader);
        hash.Add(PreferredTeam);
        hash.Add(RankedModeUnlocked);
        hash.Add(PartyMemberVersion);
        hash.Add(Region);
        return hash.ToHashCode();
    }
}