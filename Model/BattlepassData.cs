using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Model;

public record class BattlepassData : IDatabaseSyncableDefault<BattlepassData, Guid>
{
    private static readonly BattlepassData defaultData = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "defaults", "BattlepassData.json"))).Deserialize<BattlepassData>();
    [SetsRequiredMembers]
    public BattlepassData(Guid playerId, Guid[] activeBattlePasses, Guid[] battlepassQuests, Guid[] activeBattlepassQuests, int battlepassLevel)
    {
        PlayerId = playerId;
        ActiveBattlePasses = activeBattlePasses ?? throw new ArgumentNullException(nameof(activeBattlePasses));
        BattlepassQuests = battlepassQuests ?? throw new ArgumentNullException(nameof(battlepassQuests));
        ActiveBattlepassQuests = activeBattlepassQuests ?? throw new ArgumentNullException(nameof(activeBattlepassQuests));
        BattlepassLevel = battlepassLevel;
    }

    public required Guid PlayerId { get; set; }
    public required Guid[] ActiveBattlePasses { get; set; }
    public required Guid[] BattlepassQuests { get; set; }
    public required Guid[] ActiveBattlepassQuests { get; set; }
    public required int BattlepassLevel { get; set; }

    public static async Task<BattlepassData?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_battlepass_data.sql");
        cmd.Parameters.AddWithValue("playerid", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new BattlepassData(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<Guid[]>(1),
            await reader.GetFieldValueAsync<Guid[]>(2),
            await reader.GetFieldValueAsync<Guid[]>(3),
            await reader.GetFieldValueAsync<int>(4)
        );
    }

    public Guid GetKey()
    {
        return PlayerId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_battlepass_data.sql");
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("active_battle_passes", ActiveBattlePasses);
        cmd.Parameters.AddWithValue("battlepass_quests", BattlepassQuests);
        cmd.Parameters.AddWithValue("active_battlepass_quests", ActiveBattlepassQuests);
        cmd.Parameters.AddWithValue("battlepass_level", BattlepassLevel);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(BattlepassData? other)
    {
        return other is not null && PlayerId == other.PlayerId && BattlepassLevel == other.BattlepassLevel
            && ActiveBattlePasses.SequenceEqual(other.ActiveBattlePasses)
            && BattlepassQuests.SequenceEqual(other.BattlepassQuests)
            && ActiveBattlepassQuests.SequenceEqual(other.ActiveBattlepassQuests);
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PlayerId);
        hash.Add(BattlepassLevel);
        hash.Add(ActiveBattlePasses);
        hash.Add(BattlepassQuests);
        hash.Add(ActiveBattlePasses);
        return hash.ToHashCode();
    }

    public static BattlepassData CreateDefault(Guid playerId)
    {
        return defaultData with { PlayerId = playerId };
    }

    public Packets.BattlepassData ToPacket()
    {
        var packet = new Packets.BattlepassData();
        packet.ActiveBattlePasses.AddRange(ActiveBattlePasses.Select(b => b.ToString()));
        packet.BpQuests.AddRange(BattlepassQuests.Select(q => q.ToString()));
        packet.ActiveBpQuests.AddRange(ActiveBattlepassQuests.Select(q => q.ToString()));
        packet.DebugSeasonOffsetMillis = "0";
        packet.SeasonEntry = SeasonEntry.GetActive().ToPacket();
        return packet;
    }
}