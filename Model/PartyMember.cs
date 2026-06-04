using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class PartyMember : VersionedData
{
    [SetsRequiredMembers]
    public PartyMember(Guid playerId, bool isReady, bool isLeader, string preferredTeam, bool rankedModeUnlocked, Int64 version) : base(version) 
    {
        PlayerId = playerId;
        IsReady = isReady;
        IsLeader = isLeader;
        PreferredTeam = preferredTeam ?? throw new ArgumentNullException(nameof(preferredTeam));
        RankedModeUnlocked = rankedModeUnlocked;
    }

    public required Guid PlayerId { get; set; }
    public required bool IsReady { get; set; }
    public required bool IsLeader { get; set; }
    public required string PreferredTeam { get; set; }
    public required bool RankedModeUnlocked { get; set; }
}