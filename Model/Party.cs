namespace Model;

public record class Party : VersionedData
{
    public required Guid PartyId { get; set; }
    public required PartyMember[] Members { get; set; }
    public required string InviteCode { get; set; }
    public required string QueuePool { get; set; }
    public required string LobbyMode { get; set; }
    public required string ChatId { get; set; }
    public required bool UseTeamMMR { get; set; }
}