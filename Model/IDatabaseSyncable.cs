using Npgsql;

namespace Model;

public interface IDatabaseSyncable<T, KeyType> : IKeyed<KeyType>
{
    Task SyncToDatabase();
    IEnumerable<NpgsqlBatchCommand> CreateBatchSyncCommand();
    static abstract Task<T?> RetrieveFromDatabase(KeyType key);
}

public interface IDatabaseSyncableDefault<T, KeyType> : IDatabaseSyncable<T, KeyType>
{
    static abstract T CreateDefault(KeyType key);
}

public static class DatabaseSyncableExtensions
{
    public static void AddSyncToBatch<T, KeyType>(this IDatabaseSyncable<T, KeyType> syncable, NpgsqlBatch batch)
    {
        foreach(var cmd in syncable.CreateBatchSyncCommand())
        {
            batch.BatchCommands.Add(cmd);
        }
    }
}