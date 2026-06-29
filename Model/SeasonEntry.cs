using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Model;

public record class SeasonEntry : IDatabaseSyncableDefault<SeasonEntry, int>, IEquatable<SeasonEntry>, IInterchangeable<SeasonEntry, Packets.SeasonEntry>
{
    [SetsRequiredMembers]
    public SeasonEntry(int seasonNumber, DateTimeOffset startTime, DateTimeOffset endTime, DateTimeOffset firstWeekStart, DateTimeOffset lastWeekEnd, int numberOfWeeksInSeason)
    {
        SeasonNumber = seasonNumber;
        StartTime = startTime;
        EndTime = endTime;
        FirstWeekStart = firstWeekStart;
        LastWeekEnd = lastWeekEnd;
        NumberOfWeeksInSeason = numberOfWeeksInSeason;
    }

    public required int SeasonNumber { get; set; }
    public required DateTimeOffset StartTime { get; set; }
    public required DateTimeOffset EndTime { get; set; }
    public required DateTimeOffset FirstWeekStart { get; set; }
    public required DateTimeOffset LastWeekEnd { get; set; }
    public required int NumberOfWeeksInSeason { get; set; }

    public static async Task<SeasonEntry?> RetrieveFromDatabase(int key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_season_entry.sql");
        cmd.Parameters.AddWithValue("season_number", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new SeasonEntry(
            await reader.GetFieldValueAsync<int>(0),
            await reader.GetFieldValueAsync<DateTimeOffset>(1),
            await reader.GetFieldValueAsync<DateTimeOffset>(2),
            await reader.GetFieldValueAsync<DateTimeOffset>(3),
            await reader.GetFieldValueAsync<DateTimeOffset>(4),
            await reader.GetFieldValueAsync<int>(5)
        );
    }

    public int GetKey()
    {
        return SeasonNumber;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_season_entry.sql");
        cmd.Parameters.AddWithValue("season_number", SeasonNumber);
        cmd.Parameters.AddWithValue("start_ts", StartTime);
        cmd.Parameters.AddWithValue("end_ts", EndTime);
        cmd.Parameters.AddWithValue("first_week_start_ts", FirstWeekStart);
        cmd.Parameters.AddWithValue("last_week_end_ts", LastWeekEnd);
        cmd.Parameters.AddWithValue("num_weeks", NumberOfWeeksInSeason);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(SeasonEntry? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (SeasonNumber == other.SeasonNumber
            && AreTimestampsEquivalent(StartTime, other.StartTime)
            && AreTimestampsEquivalent(EndTime, other.EndTime)
            && AreTimestampsEquivalent(LastWeekEnd, other.LastWeekEnd)
            && AreTimestampsEquivalent(FirstWeekStart, other.FirstWeekStart)
            && NumberOfWeeksInSeason == other.NumberOfWeeksInSeason));
    }

    private static bool AreTimestampsEquivalent(DateTimeOffset a, DateTimeOffset b)
    {
        return Math.Abs((a - b).TotalMilliseconds) < 1;
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(SeasonNumber);
        hash.Add(StartTime);
        hash.Add(EndTime);
        hash.Add(FirstWeekStart);
        hash.Add(LastWeekEnd);
        hash.Add(NumberOfWeeksInSeason);
        return hash.ToHashCode();
    }

    public static SeasonEntry CreateDefault(int key)
    {
        JsonNode? defaultJson = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "defaults", "SeasonEntry.json")));
        defaultJson[nameof(SeasonNumber)] = key;
        return defaultJson.Deserialize<SeasonEntry>();
    }

    public static SeasonEntry FromPacket(Packets.SeasonEntry inst)
    {
        return new SeasonEntry((int)inst.SeasonNumber, DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(inst.StartTimestampMillis)), DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(inst.EndTimestampMillis)), DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(inst.FirstWeekStartTimestampMillis)), DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(inst.LastWeekEndTimestampMillis)), (int)inst.NumberOfWeeksInSeason); ;
    }

    public Packets.SeasonEntry ToPacket()
    {
        return new Packets.SeasonEntry()
        {
            SeasonNumber = SeasonNumber,
            StartTimestampMillis = StartTime.ToUnixTimeMilliseconds().ToString(),
            EndTimestampMillis = EndTime.ToUnixTimeMilliseconds().ToString(),
            FirstWeekStartTimestampMillis = FirstWeekStart.ToUnixTimeMilliseconds().ToString(),
            LastWeekEndTimestampMillis = LastWeekEnd.ToUnixTimeMilliseconds().ToString(),
            NumberOfWeeksInSeason = NumberOfWeeksInSeason
        };
    }
}