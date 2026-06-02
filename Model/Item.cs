namespace Model;

public abstract record class Item
{
    public required string CatalogId { get; set; }
    public required string InstanceId { get; set; }
}

public record class StackableItem : Item
{
    public required Int64 Amount { get; set; }
}

public abstract record class InstancedItem : Item
{
    public required bool Viewed { get; set; }
}

public record class CustomizedInstancedItem : InstancedItem
{
    public required AlterationChannel[] AlterationChannels { get; set; }
}

public record class ProgressionTrackingItem : InstancedItem
{
    public required Dictionary<string,string> ProgressionByStats { get; set; }
    public required bool AreObjectivesCompleted { get; set; }
    public required string CurrentObjectiveId { get; set; }
    public required Int32 CurrentObjectiveIndex { get; set; }
    public required bool IsPremiumUnlocked { get; set; }
    public string? TeamId { get; set; }
    public ObjectiveContribution? LastContribution { get; set; }
    public required bool IsBundlePurchased { get; set; }
    public required Int32 NumLevelsPurchased { get; set; }
}

public record class SponsorUnlockTrackerItem : InstancedItem
{
   public required string SponsorName { get; set; }
}
// Todo just separate out the progression into its own table and make instanced item just CustomizedInstancedItem, we'll track progression data separately