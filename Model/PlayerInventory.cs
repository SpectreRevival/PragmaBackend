namespace Model;

public record class PlayerInventory : IDatabaseSyncable<PlayerInventory>
{
    public required Guid PlayerId { get; set; }
    public required StackableItem[] StackableItems { get; set; }
    public required InstancedItem[] InstancedItems { get; set; }
    public required Int64 InventoryVersion { get; set; }

    public static PlayerInventory RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public void SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}