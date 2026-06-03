namespace Model;

public record class ProfileData : IDatabaseSyncable<ProfileData>
{
    public required Guid PlayerId { get; set; }
    public required DisplayName DisplayName { get; set; }
    public required Guid BannerItemId { get; set; }
    public required Guid PreSprayItemId { get; set; }
    public required Guid MatchSprayItemId { get; set; }
    public required Guid PostSprayItemId { get; set; }
    public required Guid AttackerOutfitLoadoutId { get; set; }
    public required Guid DefenderOutfitLoadoutId { get; set; }
    public required Guid AttackerWeaponLoadoutId { get; set; }
    public required Guid DefenderWeaponLoadoutId { get; set; }
    public required DateTimeOffset LastUpdated { get; set; }
    public required DateTimeOffset NextNewDayRollover { get; set; }
    public required DateTimeOffset LastLogin { get; set; }
    public required Int32 PlayerFlags { get; set; }
    public required Int64 CrewScore { get; set; }
    public required Int32 CurrentSoloRank { get; set; } // todo enum
    public required Int32 HighestTeamRank { get; set; } // todo enum
    public required string DivisionType { get; set; } // todo enum
    public required Int64 InventoryVersion { get; set; }
    public required Guid CrewId { get; set; }
    public required string AccountIdProvider { get; set; } // todo enum
    public required string PlatformName { get; set; }
    public required string ProviderAccountId { get; set; }
    public required string CrossplayPlatformKind { get; set; }
    public required Int32 GamesRemainingUntilCrewJoin { get; set; }

    public static Task<ProfileData?> RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public Task SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}