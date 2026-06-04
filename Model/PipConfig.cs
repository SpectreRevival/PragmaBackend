using NpgsqlTypes;

namespace Model;

public record class PipConfig
{
    [PgName("thickness")]
    public required double Thickness { get; set; }
    [PgName("piplen")]
    public required double Length { get; set; }
    [PgName("opacity")]
    public required double Opacity { get; set; }
    [PgName("pip_offset")]
    public required double Offset { get; set; }
    [PgName("move_accuracy_offset")]
    public required bool MoveAccuracyOffset { get; set; }
    [PgName("fire_accuracy_offset")]
    public required bool FireAccuracyOffset { get; set; }
}