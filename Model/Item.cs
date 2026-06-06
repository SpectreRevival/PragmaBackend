using Npgsql;
using Packets;
using Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public abstract record class Item
{
    [SetsRequiredMembers]
    protected Item(Guid owningPlayerId, Guid catalogId, Guid instanceId)
    {
        OwningPlayerId = owningPlayerId;
        CatalogId = catalogId;
        InstanceId = instanceId;
    }

    public required Guid OwningPlayerId { get; set; }
    public required Guid CatalogId { get; set; }
    public required Guid InstanceId { get; set; }
}

public record class StackableItem : Item, IDatabaseSyncable<StackableItem, Guid>, IEquatable<StackableItem>
{
    [SetsRequiredMembers]
    public StackableItem(Guid owningPlayerId, Guid catalogId, Guid instanceId, Int64 amount) : base(owningPlayerId, catalogId, instanceId)
    {
        Amount = amount;
    }

    public required Int64 Amount { get; set; }

    public static async Task<StackableItem?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_stackable_item.sql");
        cmd.Parameters.AddWithValue("instance_id", key);
        await using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        if (!await reader.ReadAsync())
        {
            return null;
        }
        return new StackableItem(
            await reader.GetFieldValueAsync<Guid>(3),
            await reader.GetFieldValueAsync<Guid>(1),
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<Int64>(2)
        );
    }

    public Guid GetKey()
    {
        return InstanceId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_stackable_item.sql");
        cmd.Parameters.AddWithValue("instance_id", InstanceId);
        cmd.Parameters.AddWithValue("catalog_id", CatalogId);
        cmd.Parameters.AddWithValue("amount", Amount);
        cmd.Parameters.AddWithValue("owning_player_id", OwningPlayerId);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(StackableItem? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return OwningPlayerId == other.OwningPlayerId
            && CatalogId == other.CatalogId
            && InstanceId == other.InstanceId
            && Amount == other.Amount;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(OwningPlayerId);
        hash.Add(CatalogId);
        hash.Add(InstanceId);
        hash.Add(Amount);
        return hash.ToHashCode();
    }
}

public abstract record class InstancedItem : Item
{
    [SetsRequiredMembers]
    protected InstancedItem(Guid owningPlayerId, Guid catalogId, Guid instanceId, bool viewed) : base(owningPlayerId, catalogId, instanceId)
    {
        Viewed = viewed;
    }

    public required bool Viewed { get; set; }
}

public record class CustomizedInstancedItem : InstancedItem, IDatabaseSyncable<CustomizedInstancedItem, Guid>, IEquatable<CustomizedInstancedItem>
{
    [SetsRequiredMembers]
    public CustomizedInstancedItem(Guid owningPlayerId, Guid catalogId, Guid instanceId, bool viewed, AlterationChannel[] alterationChannels) : base(owningPlayerId, catalogId, instanceId, viewed)
    {
        AlterationChannels = alterationChannels;
    }

    public required AlterationChannel[] AlterationChannels { get; set; }

    public static async Task<CustomizedInstancedItem?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_customized_instanced_item.sql");
        cmd.Parameters.AddWithValue("instance_id", key);
        await using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        if (!await reader.ReadAsync())
        {
            return null;
        }
        return new CustomizedInstancedItem(
            await reader.GetFieldValueAsync<Guid>(2),
            await reader.GetFieldValueAsync<Guid>(1),
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<bool>(3),
            await reader.GetFieldValueAsync<AlterationChannel[]>(4)
        );
    }

    public Guid GetKey()
    {
        return InstanceId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_customized_instanced_item.sql");
        cmd.Parameters.AddWithValue("instance_id", InstanceId);
        cmd.Parameters.AddWithValue("catalog_id", CatalogId);
        cmd.Parameters.AddWithValue("owning_player_id", OwningPlayerId);
        cmd.Parameters.AddWithValue("alteration_channels", AlterationChannels);
        cmd.Parameters.AddWithValue("viewed", Viewed);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(CustomizedInstancedItem? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return InstanceId == other.InstanceId
            && CatalogId == other.CatalogId
            && OwningPlayerId == other.OwningPlayerId
            && AlterationChannels.SequenceEqual(other.AlterationChannels)
            && Viewed == other.Viewed;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(InstanceId);
        hash.Add(CatalogId);
        hash.Add(OwningPlayerId);
        hash.Add(AlterationChannels);
        hash.Add(Viewed);
        return hash.ToHashCode();
    }
}

public record class ProgressionTrackingItem : InstancedItem, IDatabaseSyncable<ProgressionTrackingItem, Guid>
{
    [SetsRequiredMembers]
    public ProgressionTrackingItem(Guid owningPlayerId, Guid catalogId, Guid instanceId, bool viewed, Dictionary<string, string> progressionByStats, bool areObjectivesCompleted, Guid currentObjectiveId, int currentObjectiveIndex, bool isPremiumUnlocked, Guid? teamId, ObjectiveContribution? lastContribution, bool isBundlePurchased, int numLevelsPurchased) : base(owningPlayerId, catalogId, instanceId, viewed)
    {
        ProgressionByStats = progressionByStats ?? throw new ArgumentNullException(nameof(progressionByStats));
        AreObjectivesCompleted = areObjectivesCompleted;
        CurrentObjectiveId = currentObjectiveId;
        CurrentObjectiveIndex = currentObjectiveIndex;
        IsPremiumUnlocked = isPremiumUnlocked;
        TeamId = teamId;
        LastContribution = lastContribution;
        IsBundlePurchased = isBundlePurchased;
        NumLevelsPurchased = numLevelsPurchased;
    }

    public required Dictionary<string,string> ProgressionByStats { get; set; }
    public required bool AreObjectivesCompleted { get; set; }
    public required Guid CurrentObjectiveId { get; set; }
    public required Int32 CurrentObjectiveIndex { get; set; }
    public required bool IsPremiumUnlocked { get; set; }
    public Guid? TeamId { get; set; }
    public ObjectiveContribution? LastContribution { get; set; }
    public required bool IsBundlePurchased { get; set; }
    public required Int32 NumLevelsPurchased { get; set; }

    public static Task<ProgressionTrackingItem?> RetrieveFromDatabase(Guid key)
    {
        throw new NotImplementedException();
    }

    public Guid GetKey()
    {
        return InstanceId;
    }

    public Task SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}

public record class SponsorUnlockTrackerItem : InstancedItem, IDatabaseSyncable<SponsorUnlockTrackerItem, Guid>, IEquatable<SponsorUnlockTrackerItem>
{
    [SetsRequiredMembers]
    public SponsorUnlockTrackerItem(Guid owningPlayerId, Guid catalogId, Guid instanceId, bool viewed, string sponsorName) : base(owningPlayerId, catalogId, instanceId, viewed)
    {
        SponsorName = sponsorName;
    }

    public required string SponsorName { get; set; }

    public static async Task<SponsorUnlockTrackerItem?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_sponsor_unlock_tracker_item.sql");
        cmd.Parameters.AddWithValue("instance_id", key);
        await using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        if (!await reader.ReadAsync())
        {
            return null;
        }
        return new SponsorUnlockTrackerItem(
            await reader.GetFieldValueAsync<Guid>(2),
            await reader.GetFieldValueAsync<Guid>(1),
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<bool>(3),
            await reader.GetFieldValueAsync<string>(4)
        );
    }

    public Guid GetKey()
    {
        return InstanceId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_sponsor_unlock_tracker_item.sql");
        cmd.Parameters.AddWithValue("instance_id", InstanceId);
        cmd.Parameters.AddWithValue("catalog_id", CatalogId);
        cmd.Parameters.AddWithValue("owning_player_id", OwningPlayerId);
        cmd.Parameters.AddWithValue("viewed", Viewed);
        cmd.Parameters.AddWithValue("sponsor_name", SponsorName);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(SponsorUnlockTrackerItem? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return InstanceId == other.InstanceId
            && CatalogId == other.CatalogId
            && OwningPlayerId == other.OwningPlayerId
            && Viewed == other.Viewed
            && SponsorName == other.SponsorName;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(InstanceId);
        hash.Add(CatalogId);
        hash.Add(OwningPlayerId);
        hash.Add(Viewed);
        hash.Add(SponsorName);
        return hash.ToHashCode();
    }
}