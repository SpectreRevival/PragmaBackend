namespace Model;

// TODO: Make the methods in this class to actually read items and such. Not bothering right now as it's not a priority
public class InventoryStore
{
    private static readonly InventoryStore inst = new();
    private readonly string data;
    public InventoryStore()
    {
        data = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "staticdata", "login_inventorydata.json"));
    }

    public static InventoryStore Get()
    {
        return inst;
    }

    public string GetPacketString()
    {
        return data;
    }
}