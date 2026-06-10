using Npgsql;
using Persistence;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Model;

public record class SeasonEntry : IDatabaseSyncableDefault<SeasonEntry, Int32>, IEquatable<SeasonEntry>
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

    public required Int32 SeasonNumber { get; set; }
    public required DateTimeOffset StartTime { get; set; }
    public required DateTimeOffset EndTime { get; set; }
    public required DateTimeOffset FirstWeekStart { get; set; }
    public required DateTimeOffset LastWeekEnd { get; set; }
    public required Int32 NumberOfWeeksInSeason { get; set; }

    public static async Task<SeasonEntry?> RetrieveFromDatabase(Int32 key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_season_entry.sql");
        cmd.Parameters.AddWithValue("season_number", key);
        await using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        if(!await reader.ReadAsync())
        {
            return null;
        }
        return new SeasonEntry(
            await reader.GetFieldValueAsync<Int32>(0),
            await reader.GetFieldValueAsync<DateTimeOffset>(1),
            await reader.GetFieldValueAsync<DateTimeOffset>(2),
            await reader.GetFieldValueAsync<DateTimeOffset>(3),
            await reader.GetFieldValueAsync<DateTimeOffset>(4),
            await reader.GetFieldValueAsync<Int32>(5)
        );
    }

    public Int32 GetKey()
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
        if (other is null) return false;

        if (ReferenceEquals(this, other)) return true;

        return SeasonNumber == other.SeasonNumber
            && AreTimestampsEquivalent(StartTime, other.StartTime)
            && AreTimestampsEquivalent(EndTime, other.EndTime)
            && AreTimestampsEquivalent(LastWeekEnd, other.LastWeekEnd)
            && AreTimestampsEquivalent(FirstWeekStart, other.FirstWeekStart)
            && NumberOfWeeksInSeason == other.NumberOfWeeksInSeason;
    }

    private static bool AreTimestampsEquivalent(DateTimeOffset a, DateTimeOffset b)
    {
        return Math.Abs((a - b).TotalMilliseconds) < 1;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
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
        var defaultJson = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "defaults", "SeasonEntry.json")));
        defaultJson[nameof(SeasonNumber)] = key;
        return defaultJson.Deserialize<SeasonEntry>();
    }
}