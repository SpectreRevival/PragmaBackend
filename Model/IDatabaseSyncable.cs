namespace Model;

public interface IDatabaseSyncable<T, KeyType>
{
    public Task SyncToDatabase();
    public abstract static Task<T?> RetrieveFromDatabase(KeyType key);

    public abstract KeyType GetKey();
}