using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Model;

public record class ObjectiveContribution : IEquatable<ObjectiveContribution>
{
    [SetsRequiredMembers]
    public ObjectiveContribution(ObjectiveContributionSourceType sourceType, Guid sourceId)
    {
        SourceType = sourceType;
        SourceId = sourceId;
    }

    [PgName("source_type")]
    public required ObjectiveContributionSourceType SourceType { get; set; }
    [PgName("source_id")]
    public required Guid SourceId { get; set; }

    public virtual bool Equals(ObjectiveContribution? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return SourceType == other.SourceType
            && SourceId == other.SourceId;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(SourceType);
        hash.Add(SourceId);
        return hash.ToHashCode();
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ObjectiveContributionSourceType
{
    [PgName("MATCH")]
    MATCH = 0
}