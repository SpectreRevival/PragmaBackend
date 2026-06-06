using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class PartyMember : IEquatable<PartyMember>
{
    [SetsRequiredMembers]
    public PartyMember(Guid playerId, bool isReady, bool isLeader, string preferredTeam, bool rankedModeUnlocked, Int64 partyMemberVersion)
    {
        PlayerId = playerId;
        IsReady = isReady;
        IsLeader = isLeader;
        PreferredTeam = preferredTeam ?? throw new ArgumentNullException(nameof(preferredTeam));
        RankedModeUnlocked = rankedModeUnlocked;
        PartyMemberVersion = partyMemberVersion;
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
    public required Int64 PartyMemberVersion { get; set; }

    public virtual bool Equals(PartyMember? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return PlayerId == other.PlayerId
            && IsReady == other.IsReady
            && IsLeader == other.IsLeader
            && PreferredTeam == other.PreferredTeam
            && RankedModeUnlocked == other.RankedModeUnlocked
            && PartyMemberVersion == other.PartyMemberVersion;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(PlayerId);
        hash.Add(IsReady);
        hash.Add(IsLeader);
        hash.Add(PreferredTeam);
        hash.Add(RankedModeUnlocked);
        hash.Add(PartyMemberVersion);
        return hash.ToHashCode();
    }
}