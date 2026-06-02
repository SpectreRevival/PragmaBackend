namespace Model;

public record class ResponseCurve
{
    public required string DisplayName { get; set; }
    public required double Exponent { get; set; }
    public required double ResponseCurveArcDegree { get; set; }
    public required double ResponseCurveSlope { get; set; }
    public required double ResponseCurveLinearBlendPower { get; set; }
}