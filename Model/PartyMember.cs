namespace Model;

public record class PartyMember : VersionedData
{
    public required string PlayerId { get; set; }
    public required bool IsReady { get; set; }
    public required bool IsLeader { get; set; }
    public required string PreferredTeam { get; set; }
    public required bool RankedModeUnlocked { get; set; }
}