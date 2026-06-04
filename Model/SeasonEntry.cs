using Npgsql;
using Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class SeasonEntry : IDatabaseSyncable<SeasonEntry, Int32>
{
    [SetsRequiredMembers]
    public SeasonEntry(int seasonNumber, DateTimeOffset startTimestampMillis, DateTimeOffset endTimestampMillis, DateTimeOffset lastWeekEndTimestampMillis, int numberOfWeeksInSeason)
    {
        SeasonNumber = seasonNumber;
        StartTimestampMillis = startTimestampMillis;
        EndTimestampMillis = endTimestampMillis;
        LastWeekEndTimestampMillis = lastWeekEndTimestampMillis;
        NumberOfWeeksInSeason = numberOfWeeksInSeason;
    }

    public required Int32 SeasonNumber { get; set; }
    public required DateTimeOffset StartTimestampMillis { get; set; }
    public required DateTimeOffset EndTimestampMillis { get; set; }
    public required DateTimeOffset LastWeekEndTimestampMillis { get; set; }
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
            await reader.GetFieldValueAsync<Int32>(4)
        );
    }

    public Int32 GetKey()
    {
        throw new NotImplementedException();
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_season_entry.sql");
        cmd.Parameters.AddWithValue("season_number", SeasonNumber);
        cmd.Parameters.AddWithValue("start_ts", StartTimestampMillis);
        cmd.Parameters.AddWithValue("end_ts", EndTimestampMillis);
        cmd.Parameters.AddWithValue("last_week_end_ts", LastWeekEndTimestampMillis);
        cmd.Parameters.AddWithValue("num_weeks", NumberOfWeeksInSeason);
        await cmd.ExecuteNonQueryAsync();
    }
}