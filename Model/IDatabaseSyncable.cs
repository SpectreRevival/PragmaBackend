namespace Model;

public interface IDatabaseSyncable<T, KeyType> : IKeyed<KeyType>
{
    Task SyncToDatabase();
    static abstract Task<T?> RetrieveFromDatabase(KeyType key);
}

public interface IDatabaseSyncableDefault<T, KeyType> : IDatabaseSyncable<T, KeyType>
{
    static abstract T CreateDefault(KeyType key);
}