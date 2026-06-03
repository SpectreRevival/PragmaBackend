using Npgsql;
using Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class BattlepassData : IDatabaseSyncable<BattlepassData>
{
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
    public required Int32 BattlepassLevel { get; set; }

    public async static Task<BattlepassData?> RetrieveFromDatabase(string key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_battlepass_data.sql");
        cmd.Parameters.AddWithValue("playerid", key);
        await using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        if(!await reader.ReadAsync())
        {
            return null;
        }
        return new BattlepassData(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<Guid[]>(1),
            await reader.GetFieldValueAsync<Guid[]>(2),
            await reader.GetFieldValueAsync<Guid[]>(3),
            await reader.GetFieldValueAsync<Int32>(4)
        );
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
}