namespace Model;

public interface IDatabaseSyncable<T>
{
    public Task SyncToDatabase();
    public abstract static Task<T?> RetrieveFromDatabase(string key);
}