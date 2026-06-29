namespace Model;

public record class DefaultInventory
{
    public required StackableItem[] StackableItems { get; set; }
    public required CustomizedInstancedItem[] CustomizedInstancedItems { get; set; }
    public required ProgressionTrackingItem[] ProgresionTrackingItems { get; set; }
    public required SponsorUnlockTrackerItem[] SponsorUnlockItems { get; set; }
}