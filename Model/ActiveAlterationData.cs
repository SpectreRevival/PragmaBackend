namespace Model;

public record class ActiveAlterationData
{
    public required string ChannelId { get; set; }
    public required string AlterationId { get; set; }
}