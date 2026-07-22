using Npgsql;

namespace Model;

public interface IDatabaseSyncable<T, KeyType> : IKeyed<KeyType>
{
    Task SyncToDatabase();
    NpgsqlBatchCommand CreateBatchSyncCommand();
    Task WriteToBulkWriter(NpgsqlBinaryImporter importer);
    static abstract Task<T?> RetrieveFromDatabase(KeyType key);
    static abstract NpgsqlBinaryImporter CreateBulkWriter();
}

public interface IDatabaseSyncableDefault<T, KeyType> : IDatabaseSyncable<T, KeyType>
{
    static abstract T CreateDefault(KeyType key);
}