using Model.Persistence;
using Npgsql;
using Packets;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Model;

public abstract record class TrackedProgression
{
    [SetsRequiredMembers]
    protected TrackedProgression(Guid playerId, Guid[] activeDailyQuests, Guid[] activeWeeklyQuests, Guid[] activeEventQuests, DateTimeOffset lastRolloverTimestamp)
    {
        PlayerId = playerId;
        ActiveDailyQuests = activeDailyQuests ?? throw new ArgumentNullException(nameof(activeDailyQuests));
        ActiveWeeklyQuests = activeWeeklyQuests ?? throw new ArgumentNullException(nameof(activeWeeklyQuests));
        ActiveEventQuests = activeEventQuests ?? throw new ArgumentNullException(nameof(activeEventQuests));
        LastRolloverTimestamp = lastRolloverTimestamp;
    }

    public required Guid PlayerId { get; set; }
    public required Guid[] ActiveDailyQuests { get; set; }
    public required Guid[] ActiveWeeklyQuests { get; set; }
    public required Guid[] ActiveEventQuests { get; set; }
    public required DateTimeOffset LastRolloverTimestamp { get; set; }
}

public record class TeamTrackedProgression : TrackedProgression, IDatabaseSyncableDefault<TeamTrackedProgression, Guid>, IEquatable<TeamTrackedProgression>, IInterchangeableKeyed<TeamTrackedProgression, Packets.TrackedProgression, Guid>
{
    private static readonly TeamTrackedProgression defaultData = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "defaults", "TeamTrackedProgression.json")))
        .Deserialize<TeamTrackedProgression>(new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

    [SetsRequiredMembers]
    public TeamTrackedProgression(Guid playerId, Guid[] activeDailyQuests, Guid[] activeWeeklyQuests, Guid[] activeEventQuests, DateTimeOffset lastRolloverTimestamp, Guid teamId) : base(playerId, activeDailyQuests, activeWeeklyQuests, activeEventQuests, lastRolloverTimestamp)
    {
        TeamId = teamId;
    }

    public required Guid TeamId { get; set; }

    public static async Task<TeamTrackedProgression?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_team_progression.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new TeamTrackedProgression(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<Guid[]>(2),
            await reader.GetFieldValueAsync<Guid[]>(3),
            await reader.GetFieldValueAsync<Guid[]>(4),
            await reader.GetFieldValueAsync<DateTimeOffset>(5),
            await reader.GetFieldValueAsync<Guid>(1)
        );
    }

    public Guid GetKey()
    {
        return PlayerId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_team_progression.sql");
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("team_id", TeamId);
        cmd.Parameters.AddWithValue("active_daily_quests", ActiveDailyQuests);
        cmd.Parameters.AddWithValue("active_weekly_quests", ActiveWeeklyQuests);
        cmd.Parameters.AddWithValue("active_event_quests", ActiveEventQuests);
        cmd.Parameters.AddWithValue("last_rollover", LastRolloverTimestamp);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(TeamTrackedProgression? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return PlayerId == other.PlayerId
            && ActiveDailyQuests.SequenceEqual(other.ActiveDailyQuests)
            && ActiveWeeklyQuests.SequenceEqual(other.ActiveWeeklyQuests)
            && ActiveEventQuests.SequenceEqual(other.ActiveEventQuests)
            && LastRolloverTimestamp.ToUnixTimeMilliseconds() == other.LastRolloverTimestamp.ToUnixTimeMilliseconds()
            && TeamId == other.TeamId;
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PlayerId);
        hash.Add(ActiveDailyQuests);
        hash.Add(ActiveWeeklyQuests);
        hash.Add(ActiveEventQuests);
        hash.Add(LastRolloverTimestamp.ToUnixTimeMilliseconds());
        hash.Add(TeamId);
        return hash.ToHashCode();
    }

    public static TeamTrackedProgression CreateDefault(Guid playerId)
    {
        return defaultData with { PlayerId = playerId };
    }

    public static TeamTrackedProgression FromPacket(Packets.TrackedProgression inst, Guid playerId)
    {
        return new TeamTrackedProgression(playerId, inst.ActiveDailyQuests.Select(q => Guid.Parse(q)).ToArray(), inst.ActiveWeeklyQuests.Select(q => Guid.Parse(q)).ToArray(), inst.ActiveEventQuests.Select(q => Guid.Parse(q)).ToArray(), DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(inst.LastRolloverTimestamp)), Guid.Parse(inst.TeamId));
    }

    public Packets.TrackedProgression ToPacket()
    {
        var packet = new Packets.TrackedProgression();
        packet.ActiveDailyQuests.AddRange(ActiveDailyQuests.Select(q => q.ToString()));
        packet.ActiveWeeklyQuests.AddRange(ActiveWeeklyQuests.Select(q => q.ToString()));
        packet.ActiveEventQuests.AddRange(ActiveEventQuests.Select(q => q.ToString()));
        packet.LastRolloverTimestamp = LastRolloverTimestamp.ToUnixTimeMilliseconds().ToString();
        packet.TeamId = TeamId.ToString();
        return packet;
    }

    public Packets.InstancedItem ToProgressionTrackerItem()
    {
        var packet = new Packets.InstancedItem()
        {
            CatalogId = "MtnManualItem:TeamProgressionTracker",
            InstanceId = Guid.NewGuid().ToString()
        };
        InstanceExtData ext = new();
        Packets.TrackedProgression prog = ToPacket();
        ext.TrackedProgression = prog;
        packet.Ext = ext;
        return packet;
    }
}

public record class IndividualTrackedProgression : TrackedProgression, IDatabaseSyncableDefault<IndividualTrackedProgression, Guid>, IEquatable<IndividualTrackedProgression>
{
    private static readonly IndividualTrackedProgression defaultData = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "defaults", "IndividualTrackedProgression.json")))
        .Deserialize<IndividualTrackedProgression>(new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });

    [SetsRequiredMembers]
    public IndividualTrackedProgression(Guid playerId, Guid[] activeDailyQuests, Guid[] activeWeeklyQuests, Guid[] activeEventQuests, DateTimeOffset lastRolloverTimestamp, Guid activeEndorsement) : base(playerId, activeDailyQuests, activeWeeklyQuests, activeEventQuests, lastRolloverTimestamp)
    {
        ActiveEndorsement = activeEndorsement;
    }

    public required Guid ActiveEndorsement { get; set; }

    public static async Task<IndividualTrackedProgression?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_individual_progression.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new IndividualTrackedProgression(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<Guid[]>(1),
            await reader.GetFieldValueAsync<Guid[]>(2),
            await reader.GetFieldValueAsync<Guid[]>(3),
            await reader.GetFieldValueAsync<DateTimeOffset>(5),
            await reader.GetFieldValueAsync<Guid>(4)
        );
    }

    public Guid GetKey()
    {
        return PlayerId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_individual_progression.sql");
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("active_daily_quests", ActiveDailyQuests);
        cmd.Parameters.AddWithValue("active_weekly_quests", ActiveWeeklyQuests);
        cmd.Parameters.AddWithValue("active_event_quests", ActiveEventQuests);
        cmd.Parameters.AddWithValue("active_endorsement", ActiveEndorsement);
        cmd.Parameters.AddWithValue("last_rollover", LastRolloverTimestamp);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(IndividualTrackedProgression? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (PlayerId == other.PlayerId
            && ActiveDailyQuests.SequenceEqual(other.ActiveDailyQuests)
            && ActiveWeeklyQuests.SequenceEqual(other.ActiveWeeklyQuests)
            && ActiveEventQuests.SequenceEqual(other.ActiveEventQuests)
            && LastRolloverTimestamp.ToUnixTimeMilliseconds() == other.LastRolloverTimestamp.ToUnixTimeMilliseconds()
            && ActiveEndorsement == other.ActiveEndorsement));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PlayerId);
        hash.Add(ActiveDailyQuests);
        hash.Add(ActiveWeeklyQuests);
        hash.Add(ActiveEventQuests);
        hash.Add(LastRolloverTimestamp.ToUnixTimeMilliseconds());
        hash.Add(ActiveEndorsement);
        return hash.ToHashCode();
    }

    public static IndividualTrackedProgression CreateDefault(Guid playerId)
    {
        return defaultData with { PlayerId = playerId };
    }

    public async Task<Packets.TrackedProgression> AssemblePacketIndividualProgression()
    {
        var packet = new Packets.TrackedProgression();
        packet.ActiveEndorsement = ActiveEndorsement.ToString();
        packet.ActiveDailyQuests.AddRange(ActiveDailyQuests.Select(q => q.ToString()));
        packet.ActiveWeeklyQuests.AddRange(ActiveWeeklyQuests.Select(q => q.ToString()));
        packet.ActiveEventQuests.AddRange(ActiveEventQuests.Select(q => q.ToString()));
        packet.LastRolloverTimestamp = LastRolloverTimestamp.ToUnixTimeMilliseconds().ToString();
        BattlepassData bpData = await BattlepassData.RetrieveFromDatabase(PlayerId);
        packet.BpTrackerData = bpData.ToPacket();
        return packet;
    }

    public async Task<Packets.InstancedItem> ToProgressionTrackerItem()
    {
        var packet = new Packets.InstancedItem()
        {
            CatalogId = "MtnManualItem:ProgressionTracker",
            InstanceId = Guid.NewGuid().ToString()
        };
        InstanceExtData ext = new();
        ext.TrackedProgression = await AssemblePacketIndividualProgression();
        packet.Ext = ext;
        return packet;
    }
}