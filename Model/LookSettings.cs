namespace Model;

public record class LookSettings
{
    public required double YawRate { get; set; }
    public required double PitchScale { get; set; }
    public required double TurnAccelYawBonus { get; set; }
    public required double TurnAccelPitchBonus { get; set; }
    public required double TurnAccelDelaySeconds { get; set; }
    public required double TurnAccelTimeToMax { get; set; }
}