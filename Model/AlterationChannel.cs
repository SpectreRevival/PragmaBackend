namespace Model;

public record class AlterationChannel
{
    public required string ChannelId { get; set; }
    public required string[] Alterations { get; set; }
}