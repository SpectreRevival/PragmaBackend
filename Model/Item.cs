namespace Model;

public abstract record class Item
{
    public required Guid OwningPlayerId { get; set; }
    public required Guid CatalogId { get; set; }
    public required Guid InstanceId { get; set; }
}

public record class StackableItem : Item, IDatabaseSyncable<StackableItem>
{
    public required Int64 Amount { get; set; }

    public static StackableItem RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public void SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}

public abstract record class InstancedItem : Item
{
    public required bool Viewed { get; set; }
}

public record class CustomizedInstancedItem : InstancedItem, IDatabaseSyncable<CustomizedInstancedItem>
{
    public required AlterationChannel[] AlterationChannels { get; set; }

    public static CustomizedInstancedItem RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public void SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}

public record class ProgressionTrackingItem : InstancedItem, IDatabaseSyncable<ProgressionTrackingItem>
{
    public required Dictionary<string,string> ProgressionByStats { get; set; }
    public required bool AreObjectivesCompleted { get; set; }
    public required Guid CurrentObjectiveId { get; set; }
    public required Int32 CurrentObjectiveIndex { get; set; }
    public required bool IsPremiumUnlocked { get; set; }
    public Guid? TeamId { get; set; }
    public ObjectiveContribution? LastContribution { get; set; }
    public required bool IsBundlePurchased { get; set; }
    public required Int32 NumLevelsPurchased { get; set; }

    public static ProgressionTrackingItem RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public void SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}

public record class SponsorUnlockTrackerItem : InstancedItem, IDatabaseSyncable<SponsorUnlockTrackerItem>
{
   public required string SponsorName { get; set; }

    public static SponsorUnlockTrackerItem RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public void SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}