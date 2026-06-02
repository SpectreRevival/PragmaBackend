namespace Model;

public record class PipConfig
{
    public required double Thickness { get; set; }
    public required double Length { get; set; }
    public required double Opacity { get; set; }
    public required double Offset { get; set; }
    public required bool MoveAccuracyOffset { get; set; }
    public required bool FireAccuracyOffset { get; set; }
}