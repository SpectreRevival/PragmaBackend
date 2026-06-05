using Npgsql;
using Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class ProfileData : IDatabaseSyncable<ProfileData, Guid>, IEquatable<ProfileData>
{
    [SetsRequiredMembers]
    public ProfileData(Guid playerId, DisplayName displayName, Guid bannerItemId, Guid preSprayItemId, Guid matchSprayItemId, Guid postSprayItemId, Guid attackerOutfitLoadoutId, Guid defenderOutfitLoadoutId, Guid attackerWeaponLoadoutId, Guid defenderWeaponLoadoutId, DateTimeOffset lastUpdated, DateTimeOffset nextNewDayRollover, DateTimeOffset lastLogin, int playerFlags, long crewScore, int currentSoloRank, int highestTeamRank, string divisionType, long inventoryVersion, Guid crewId, string accountIdProvider, string platformName, string providerAccountId, string crossplayPlatformKind, int gamesRemainingUntilCrewJoin)
    {
        PlayerId = playerId;
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        BannerItemId = bannerItemId;
        PreSprayItemId = preSprayItemId;
        MatchSprayItemId = matchSprayItemId;
        PostSprayItemId = postSprayItemId;
        AttackerOutfitLoadoutId = attackerOutfitLoadoutId;
        DefenderOutfitLoadoutId = defenderOutfitLoadoutId;
        AttackerWeaponLoadoutId = attackerWeaponLoadoutId;
        DefenderWeaponLoadoutId = defenderWeaponLoadoutId;
        LastUpdated = lastUpdated;
        NextNewDayRollover = nextNewDayRollover;
        LastLogin = lastLogin;
        PlayerFlags = playerFlags;
        CrewScore = crewScore;
        CurrentSoloRank = currentSoloRank;
        HighestTeamRank = highestTeamRank;
        DivisionType = divisionType ?? throw new ArgumentNullException(nameof(divisionType));
        InventoryVersion = inventoryVersion;
        CrewId = crewId;
        AccountIdProvider = accountIdProvider ?? throw new ArgumentNullException(nameof(accountIdProvider));
        PlatformName = platformName ?? throw new ArgumentNullException(nameof(platformName));
        ProviderAccountId = providerAccountId ?? throw new ArgumentNullException(nameof(providerAccountId));
        CrossplayPlatformKind = crossplayPlatformKind ?? throw new ArgumentNullException(nameof(crossplayPlatformKind));
        GamesRemainingUntilCrewJoin = gamesRemainingUntilCrewJoin;
    }

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

    public static async Task<ProfileData?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_profile_data.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        if (!await reader.ReadAsync())
        {
            return null;
        }
        return new ProfileData(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<DisplayName>(1),
            await reader.GetFieldValueAsync<Guid>(2),
            await reader.GetFieldValueAsync<Guid>(3),
            await reader.GetFieldValueAsync<Guid>(4),
            await reader.GetFieldValueAsync<Guid>(5),
            await reader.GetFieldValueAsync<Guid>(6),
            await reader.GetFieldValueAsync<Guid>(7),
            await reader.GetFieldValueAsync<Guid>(8),
            await reader.GetFieldValueAsync<Guid>(9),
            await reader.GetFieldValueAsync<DateTimeOffset>(10),
            await reader.GetFieldValueAsync<DateTimeOffset>(11),
            await reader.GetFieldValueAsync<DateTimeOffset>(12),
            await reader.GetFieldValueAsync<Int32>(13),
            await reader.GetFieldValueAsync<Int64>(14),
            await reader.GetFieldValueAsync<Int32>(15),
            await reader.GetFieldValueAsync<Int32>(16),
            await reader.GetFieldValueAsync<string>(17),
            await reader.GetFieldValueAsync<Int64>(18),
            await reader.GetFieldValueAsync<Guid>(19),
            await reader.GetFieldValueAsync<string>(20),
            await reader.GetFieldValueAsync<string>(21),
            await reader.GetFieldValueAsync<string>(22),
            await reader.GetFieldValueAsync<string>(23),
            await reader.GetFieldValueAsync<Int32>(24)
        );
    }

    public Guid GetKey()
    {
        return PlayerId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_profile_data.sql");
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("display_name", DisplayName);
        cmd.Parameters.AddWithValue("banner_item_id", BannerItemId);
        cmd.Parameters.AddWithValue("pre_spray_item_id", PreSprayItemId);
        cmd.Parameters.AddWithValue("match_spray_item_id", MatchSprayItemId);
        cmd.Parameters.AddWithValue("post_spray_item_id", PostSprayItemId);
        cmd.Parameters.AddWithValue("attacker_outfit_loadout_id", AttackerOutfitLoadoutId);
        cmd.Parameters.AddWithValue("defender_outfit_loadout_id", DefenderOutfitLoadoutId);
        cmd.Parameters.AddWithValue("attacker_weapon_loadout_id", AttackerWeaponLoadoutId);
        cmd.Parameters.AddWithValue("defender_weapon_loadout_id", DefenderWeaponLoadoutId);
        cmd.Parameters.AddWithValue("last_updated", LastUpdated);
        cmd.Parameters.AddWithValue("next_new_day_rollover", NextNewDayRollover);
        cmd.Parameters.AddWithValue("last_login", LastLogin);
        cmd.Parameters.AddWithValue("player_flags", PlayerFlags);
        cmd.Parameters.AddWithValue("crew_score", CrewScore);
        cmd.Parameters.AddWithValue("current_solo_rank", CurrentSoloRank);
        cmd.Parameters.AddWithValue("highest_team_rank", HighestTeamRank);
        cmd.Parameters.AddWithValue("division_type", DivisionType);
        cmd.Parameters.AddWithValue("inventory_version", InventoryVersion);
        cmd.Parameters.AddWithValue("crew_id", CrewId);
        cmd.Parameters.AddWithValue("account_id_provider", AccountIdProvider);
        cmd.Parameters.AddWithValue("platform_name", PlatformName);
        cmd.Parameters.AddWithValue("provider_account_id", ProviderAccountId);
        cmd.Parameters.AddWithValue("crossplay_platform_kind", CrossplayPlatformKind);
        cmd.Parameters.AddWithValue("games_remaining_until_crew_join", GamesRemainingUntilCrewJoin);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(ProfileData? other)
    {
        if (other is null) return false;
        if(ReferenceEquals(this, other)) return true;

        return PlayerId == other.PlayerId
            && DisplayName.Equals(other.DisplayName)
            && BannerItemId == other.BannerItemId
            && PreSprayItemId == other.PreSprayItemId
            && MatchSprayItemId == other.MatchSprayItemId
            && PostSprayItemId == other.PostSprayItemId
            && AttackerOutfitLoadoutId == other.AttackerOutfitLoadoutId
            && DefenderOutfitLoadoutId == other.DefenderOutfitLoadoutId
            && AttackerWeaponLoadoutId == other.AttackerWeaponLoadoutId
            && DefenderWeaponLoadoutId == other.DefenderWeaponLoadoutId
            && LastUpdated.ToUnixTimeMilliseconds() == other.LastUpdated.ToUnixTimeMilliseconds()
            && NextNewDayRollover.ToUnixTimeMilliseconds() == other.NextNewDayRollover.ToUnixTimeMilliseconds()
            && LastLogin.ToUnixTimeMilliseconds() == other.LastLogin.ToUnixTimeMilliseconds()
            && PlayerFlags == other.PlayerFlags
            && CrewScore == other.CrewScore
            && CurrentSoloRank == other.CurrentSoloRank
            && HighestTeamRank == other.HighestTeamRank
            && DivisionType == other.DivisionType
            && InventoryVersion == other.InventoryVersion
            && CrewId == other.CrewId
            && AccountIdProvider == other.AccountIdProvider
            && PlatformName == other.PlatformName
            && ProviderAccountId == other.ProviderAccountId
            && CrossplayPlatformKind == other.CrossplayPlatformKind
            && GamesRemainingUntilCrewJoin == other.GamesRemainingUntilCrewJoin;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(PlayerId);
        hash.Add(DisplayName);
        hash.Add(BannerItemId);
        hash.Add(PreSprayItemId);
        hash.Add(MatchSprayItemId);
        hash.Add(PostSprayItemId);
        hash.Add(AttackerOutfitLoadoutId);
        hash.Add(DefenderOutfitLoadoutId);
        hash.Add(AttackerWeaponLoadoutId);
        hash.Add(DefenderWeaponLoadoutId);
        hash.Add(LastUpdated.ToUnixTimeMilliseconds());
        hash.Add(NextNewDayRollover.ToUnixTimeMilliseconds());
        hash.Add(LastLogin.ToUnixTimeMilliseconds());
        hash.Add(PlayerFlags);
        hash.Add(CrewScore);
        hash.Add(CurrentSoloRank);
        hash.Add(HighestTeamRank);
        hash.Add(DivisionType);
        hash.Add(InventoryVersion);
        hash.Add(CrewId);
        hash.Add(AccountIdProvider);
        hash.Add(PlatformName);
        hash.Add(ProviderAccountId);
        hash.Add(CrossplayPlatformKind);
        hash.Add(GamesRemainingUntilCrewJoin);
        return hash.ToHashCode();
    }
}