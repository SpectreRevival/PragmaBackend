namespace Model;

public record class ObjectiveContribution
{
    public required ObjectiveContributionSourceType SourceType { get; set; }
    public required Guid SourceId { get; set; }
}

public enum ObjectiveContributionSourceType
{
    MATCH = 0
}