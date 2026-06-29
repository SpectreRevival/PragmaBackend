using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Model;

public record class ObjectiveContribution : IEquatable<ObjectiveContribution>, IInterchangeable<ObjectiveContribution, Packets.ObjectiveContribution>
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

    public static ObjectiveContribution FromPacket(Packets.ObjectiveContribution inst)
    {
        return new ObjectiveContribution((ObjectiveContributionSourceType)(int)inst.SourceType, Guid.Parse(inst.SourceId));
    }

    public virtual bool Equals(ObjectiveContribution? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (SourceType == other.SourceType
            && SourceId == other.SourceId));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(SourceType);
        hash.Add(SourceId);
        return hash.ToHashCode();
    }

    public Packets.ObjectiveContribution ToPacket()
    {
        return new Packets.ObjectiveContribution()
        {
            SourceType = (Packets.ObjectiveContribution.Types.SourceType)SourceType,
            SourceId = SourceId.ToString()
        };
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ObjectiveContributionSourceType
{
    [PgName("MATCH")]
    MATCH = 0
}