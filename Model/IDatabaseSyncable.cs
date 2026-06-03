namespace Model;

public interface IDatabaseSyncable<T>
{
    public void SyncToDatabase();
    public abstract static T RetrieveFromDatabase(string key);
}