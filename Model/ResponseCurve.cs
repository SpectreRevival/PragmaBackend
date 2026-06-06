using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class ResponseCurve : IEquatable<ResponseCurve>
{
    [SetsRequiredMembers]
    public ResponseCurve(string displayName, double exponent, double responseCurveArcDegree, double responseCurveSlope, double responseCurveLinearBlendPower)
    {
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        Exponent = exponent;
        ResponseCurveArcDegree = responseCurveArcDegree;
        ResponseCurveSlope = responseCurveSlope;
        ResponseCurveLinearBlendPower = responseCurveLinearBlendPower;
    }

    [PgName("display_name")]
    public required string DisplayName { get; set; }
    [PgName("exponent")]
    public required double Exponent { get; set; }
    [PgName("response_curve_arc_degree")]
    public required double ResponseCurveArcDegree { get; set; }
    [PgName("response_curve_slope")]
    public required double ResponseCurveSlope { get; set; }
    [PgName("response_curve_linear_blend_power")]
    public required double ResponseCurveLinearBlendPower { get; set; }

    public virtual bool Equals(ResponseCurve? other)
    {
        if (other is null) return false;
        if(ReferenceEquals(this, other)) return false;

        return DisplayName == other.DisplayName
            && Exponent == other.Exponent
            && ResponseCurveArcDegree == other.ResponseCurveArcDegree
            && ResponseCurveSlope == other.ResponseCurveSlope
            && ResponseCurveLinearBlendPower == other.ResponseCurveLinearBlendPower;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(DisplayName);
        hash.Add(Exponent);
        hash.Add(ResponseCurveArcDegree);
        hash.Add(ResponseCurveSlope);
        hash.Add(ResponseCurveLinearBlendPower);
        return hash.ToHashCode();
    }
}