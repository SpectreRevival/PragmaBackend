namespace Model;

public record class PlayerPresence
{
    public required PlayerBasicPresence BasicStatus { get; set; }
    public required DateTimeOffset LastUpdatedTime { get; set; }
    public required Int32 AdvancedPresenceType { get; set; } // Todo make enum
    public required string AdvancedPresenceContext { get; set; }
}

public enum PlayerBasicPresence
{
    Online = 0,
    Offline = 1
}