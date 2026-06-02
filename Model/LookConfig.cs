namespace Model;

public record class LookConfig
{
    public required string DisplayName { get; set; }
    public required LookSettings LookSettings { get; set; }
    public required LookSettings LookSettingsADS { get; set; }
}