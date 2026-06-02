namespace Model;

public record class ProfileData
{
    public required string PlayerId { get; set; }
    public required DisplayName DisplayName { get; set; }
    public required string BannerItemId { get; set; }
    public required Int64 CrewScore { get; set; }
    public required Int32 CurrentSoloRank { get; set; } // todo enum
    public required Int32 HighestTeamRank { get; set; } // todo enum
    public required string DivisionType { get; set; } // todo enum
}