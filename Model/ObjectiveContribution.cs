namespace Model;

public record class ObjectiveContribution
{
    public required ObjectiveContributionSourceType SourceType { get; set; }
    public required string SourceId { get; set; }
}

public enum ObjectiveContributionSourceType
{
    MATCH = 0
}