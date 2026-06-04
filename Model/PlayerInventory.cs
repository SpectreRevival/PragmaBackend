namespace Model;

public record class PlayerInventory : IDatabaseSyncable<PlayerInventory, Guid>
{
    public required Guid PlayerId { get; set; }
    public required StackableItem[] StackableItems { get; set; }
    public required InstancedItem[] InstancedItems { get; set; }
    public required Int64 InventoryVersion { get; set; }

    public static Task<PlayerInventory?> RetrieveFromDatabase(Guid key)
    {
        throw new NotImplementedException();
    }

    public Guid GetKey()
    {
        throw new NotImplementedException();
    }

    public Task SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}