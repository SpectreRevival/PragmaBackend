namespace Model;

public record class LegacySeasonData
{
    public required Int64 SoloRankedPoints { get; set; }
    public required Int64 CurrentSoloRank { get; set; } // TODO make enum
}