namespace Model;

public record class BattlepassData : IDatabaseSyncable<BattlepassData>
{
    public required Guid PlayerId { get; set; }
    public required Guid[] ActiveBattlePasses { get; set; }
    public required Guid[] BattlepassQuests { get; set; }
    public required Guid[] ActiveBattlepassQuests { get; set; }
    public required Int32 BattlepassLevel { get; set; }

    public static BattlepassData RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public void SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}