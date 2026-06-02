namespace Model;

public record class ProfileData
{
    public required string PlayerId { get; set; }
    public required DisplayName DisplayName { get; set; }
    public required string BannerItemId { get; set; }
    public required string PreSprayItemId { get; set; }
    public required string MatchSprayItemId { get; set; }
    public required string PostSprayItemId { get; set; }
    public required string AttackerOutfitLoadoutId { get; set; }
    public required string DefenderOutfitLoadoutId { get; set; }
    public required string AttackerWeaponLoadoutId { get; set; }
    public required string DefenderWeaponLoadoutId { get; set; }
    public required DateTimeOffset LastUpdated { get; set; }
    public required DateTimeOffset NextNewDayRollover { get; set; }
    public required DateTimeOffset LastLogin { get; set; }
    public required Int32 PlayerFlags { get; set; }
    public required Int64 CrewScore { get; set; }
    public required Int32 CurrentSoloRank { get; set; } // todo enum
    public required Int32 HighestTeamRank { get; set; } // todo enum
    public required string DivisionType { get; set; } // todo enum
    public required Int64 InventoryVersion { get; set; }
    public required string CrewId { get; set; }
    public required string AccountIdProvider { get; set; } // todo enum
    public required string PlatformName { get; set; }
    public required string ProviderAccountId { get; set; }
    public required string CrossplayPlatformKind { get; set; }
    public required Int32 GamesRemainingUntilCrewJoin { get; set; }
}