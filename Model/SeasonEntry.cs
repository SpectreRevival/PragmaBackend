using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Model;

public record class SeasonEntry : IEquatable<SeasonEntry>, IInterchangeable<SeasonEntry, Packets.SeasonEntry>
{
    private static readonly SeasonEntry active = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "staticdata", "ActiveSeason.json"))).Deserialize<SeasonEntry>();

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

    public static SeasonEntry GetActive()
    {
        return active;
    }
}