namespace Model;

public record class DisplayName
{
    public required string PlayerName { get; set; }
    public required string Discriminator { get; set; }
}