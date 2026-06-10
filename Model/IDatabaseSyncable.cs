namespace Model;

public interface IDatabaseSyncable<T, KeyType>
{
    public Task SyncToDatabase();
    public abstract static Task<T?> RetrieveFromDatabase(KeyType key);

    public abstract KeyType GetKey();

}

public interface IDatabaseSyncableDefault<T, KeyType> : IDatabaseSyncable<T, KeyType>
{
    public abstract static T CreateDefault(KeyType key);
}