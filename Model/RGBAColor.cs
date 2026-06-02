namespace Model;

public record class RGBAColor
{
    public required byte R { get; set; }
    public required byte G { get; set; }
    public required byte B { get; set; }
    public required byte A { get; set; }
}