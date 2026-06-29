using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public abstract record class Item
{
    [SetsRequiredMembers]
    protected Item(Guid owningPlayerId, string catalogId, Guid instanceId)
    {
        OwningPlayerId = owningPlayerId;
        CatalogId = catalogId ?? throw new ArgumentNullException(nameof(catalogId));
        InstanceId = instanceId;
    }

    public required Guid OwningPlayerId { get; set; }
    public required string CatalogId { get; set; }
    public required Guid InstanceId { get; set; }
}

public record class StackableItem : Item, IDatabaseSyncable<StackableItem, Guid>, IEquatable<StackableItem>
{
    [SetsRequiredMembers]
    public StackableItem(Guid owningPlayerId, string catalogId, Guid instanceId, long amount) : base(owningPlayerId, catalogId, instanceId)
    {
        Amount = amount;
    }

    public required long Amount { get; set; }

    public static async Task<StackableItem?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_stackable_item.sql");
        cmd.Parameters.AddWithValue("instance_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new StackableItem(
            await reader.GetFieldValueAsync<Guid>(3),
            await reader.GetFieldValueAsync<string>(1),
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<long>(2)
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
        return other is not null && (ReferenceEquals(this, other) || (OwningPlayerId == other.OwningPlayerId
            && CatalogId == other.CatalogId
            && InstanceId == other.InstanceId
            && Amount == other.Amount));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
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
    protected InstancedItem(Guid owningPlayerId, string catalogId, Guid instanceId, bool viewed) : base(owningPlayerId, catalogId, instanceId)
    {
        Viewed = viewed;
    }

    public required bool Viewed { get; set; }
}

public record class CustomizedInstancedItem : InstancedItem, IDatabaseSyncable<CustomizedInstancedItem, Guid>, IEquatable<CustomizedInstancedItem>
{
    [SetsRequiredMembers]
    public CustomizedInstancedItem(Guid owningPlayerId, string catalogId, Guid instanceId, bool viewed, AlterationChannel[] alterationChannels) : base(owningPlayerId, catalogId, instanceId, viewed)
    {
        AlterationChannels = alterationChannels;
    }

    public required AlterationChannel[] AlterationChannels { get; set; }

    public static async Task<CustomizedInstancedItem?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_customized_instanced_item.sql");
        cmd.Parameters.AddWithValue("instance_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new CustomizedInstancedItem(
            await reader.GetFieldValueAsync<Guid>(2),
            await reader.GetFieldValueAsync<string>(1),
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
        return other is not null && (ReferenceEquals(this, other) || (InstanceId == other.InstanceId
            && CatalogId == other.CatalogId
            && OwningPlayerId == other.OwningPlayerId
            && AlterationChannels.SequenceEqual(other.AlterationChannels)
            && Viewed == other.Viewed));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(InstanceId);
        hash.Add(CatalogId);
        hash.Add(OwningPlayerId);
        hash.Add(AlterationChannels);
        hash.Add(Viewed);
        return hash.ToHashCode();
    }
}

public record class ProgressionTrackingItem : InstancedItem, IDatabaseSyncable<ProgressionTrackingItem, Guid>, IEquatable<ProgressionTrackingItem>
{
    [SetsRequiredMembers]
    public ProgressionTrackingItem(Guid owningPlayerId, string catalogId, Guid instanceId, bool viewed, Dictionary<string, string> progressionByStats, bool areObjectivesCompleted, int currentObjectiveId, int currentObjectiveIndex, bool isPremiumUnlocked, Guid? teamId, ObjectiveContribution? lastContribution, bool isBundlePurchased, int numLevelsPurchased) : base(owningPlayerId, catalogId, instanceId, viewed)
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

    public required Dictionary<string, string> ProgressionByStats { get; set; }
    public required bool AreObjectivesCompleted { get; set; }
    public required int CurrentObjectiveId { get; set; }
    public required int CurrentObjectiveIndex { get; set; }
    public required bool IsPremiumUnlocked { get; set; }
    public Guid? TeamId { get; set; }
    public ObjectiveContribution? LastContribution { get; set; }
    public required bool IsBundlePurchased { get; set; }
    public required int NumLevelsPurchased { get; set; }

    public static async Task<ProgressionTrackingItem?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_progression_tracking_item.sql");
        cmd.Parameters.AddWithValue("instance_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new ProgressionTrackingItem(
            await reader.GetFieldValueAsync<Guid>(2),
            await reader.GetFieldValueAsync<string>(1),
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<bool>(3),
            await reader.GetFieldValueAsync<Dictionary<string, string>>(4),
            await reader.GetFieldValueAsync<bool>(5),
            await reader.GetFieldValueAsync<int>(6),
            await reader.GetFieldValueAsync<int>(7),
            await reader.GetFieldValueAsync<bool>(8),
            await reader.GetFieldValueAsync<Guid?>(9),
            await reader.GetFieldValueAsync<ObjectiveContribution?>(10),
            await reader.GetFieldValueAsync<bool>(11),
            await reader.GetFieldValueAsync<int>(12)
        );
    }

    public Guid GetKey()
    {
        return InstanceId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_progression_tracking_item.sql");
        cmd.Parameters.AddWithValue("instance_id", InstanceId);
        cmd.Parameters.AddWithValue("catalog_id", CatalogId);
        cmd.Parameters.AddWithValue("owning_player_id", OwningPlayerId);
        cmd.Parameters.AddWithValue("viewed", Viewed);
        cmd.Parameters.Add(new NpgsqlParameter("progression_by_stats", NpgsqlTypes.NpgsqlDbType.Hstore) { Value = ProgressionByStats });
        cmd.Parameters.AddWithValue("are_objectives_completed", AreObjectivesCompleted);
        cmd.Parameters.AddWithValue("current_objective_id", CurrentObjectiveId);
        cmd.Parameters.AddWithValue("current_objective_index", CurrentObjectiveIndex);
        cmd.Parameters.AddWithValue("is_premium_unlocked", IsPremiumUnlocked);
        cmd.Parameters.AddWithValue("team_id", (object?)TeamId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("last_contribution", (object?)LastContribution ?? DBNull.Value);
        cmd.Parameters.AddWithValue("is_bundle_purchased", IsBundlePurchased);
        cmd.Parameters.AddWithValue("num_levels_purchased", NumLevelsPurchased);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(ProgressionTrackingItem? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (InstanceId == other.InstanceId
            && CatalogId == other.CatalogId
            && OwningPlayerId == other.OwningPlayerId
            && Viewed == other.Viewed
            && ProgressionByStats.Count == other.ProgressionByStats.Count
            && !ProgressionByStats.Except(other.ProgressionByStats).Any()
            && AreObjectivesCompleted == other.AreObjectivesCompleted
            && CurrentObjectiveId == other.CurrentObjectiveId
            && CurrentObjectiveIndex == other.CurrentObjectiveIndex
            && IsPremiumUnlocked == other.IsPremiumUnlocked
            && TeamId == other.TeamId
            && LastContribution is null && other.LastContribution is null)
|| (LastContribution is not null && LastContribution.Equals(other.LastContribution)
            && IsBundlePurchased == other.IsBundlePurchased
            && NumLevelsPurchased == other.NumLevelsPurchased));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(InstanceId);
        hash.Add(CatalogId);
        hash.Add(OwningPlayerId);
        hash.Add(Viewed);
        hash.Add(ProgressionByStats);
        hash.Add(AreObjectivesCompleted);
        hash.Add(CurrentObjectiveId);
        hash.Add(IsPremiumUnlocked);
        hash.Add(TeamId);
        hash.Add(LastContribution);
        hash.Add(IsBundlePurchased);
        hash.Add(NumLevelsPurchased);
        return hash.ToHashCode();
    }
}

public record class SponsorUnlockTrackerItem : InstancedItem, IDatabaseSyncable<SponsorUnlockTrackerItem, Guid>, IEquatable<SponsorUnlockTrackerItem>
{
    [SetsRequiredMembers]
    public SponsorUnlockTrackerItem(Guid owningPlayerId, string catalogId, Guid instanceId, bool viewed, string sponsorName) : base(owningPlayerId, catalogId, instanceId, viewed)
    {
        SponsorName = sponsorName;
    }

    public required string SponsorName { get; set; }

    public static async Task<SponsorUnlockTrackerItem?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_sponsor_unlock_tracker_item.sql");
        cmd.Parameters.AddWithValue("instance_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new SponsorUnlockTrackerItem(
            await reader.GetFieldValueAsync<Guid>(2),
            await reader.GetFieldValueAsync<string>(1),
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
        return other is not null && (ReferenceEquals(this, other) || (InstanceId == other.InstanceId
            && CatalogId == other.CatalogId
            && OwningPlayerId == other.OwningPlayerId
            && Viewed == other.Viewed
            && SponsorName == other.SponsorName));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(InstanceId);
        hash.Add(CatalogId);
        hash.Add(OwningPlayerId);
        hash.Add(Viewed);
        hash.Add(SponsorName);
        return hash.ToHashCode();
    }
}