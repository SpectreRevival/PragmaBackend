namespace Model;

public record class PlayerInventory
{
    public required StackableItem[] StackableItems { get; set; }
    public required InstancedItem[] InstancedItems { get; set; }
    public required Int64 InventoryVersion { get; set; }
}