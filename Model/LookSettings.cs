using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class LookSettings : IEquatable<LookSettings>
{
    [SetsRequiredMembers]
    public LookSettings(double yawRate, double pitchScale, double turnAccelYawBonus, double turnAccelPitchBonus, double turnAccelDelaySeconds, double turnAccelTimeToMax)
    {
        YawRate = yawRate;
        PitchScale = pitchScale;
        TurnAccelYawBonus = turnAccelYawBonus;
        TurnAccelPitchBonus = turnAccelPitchBonus;
        TurnAccelDelaySeconds = turnAccelDelaySeconds;
        TurnAccelTimeToMax = turnAccelTimeToMax;
    }

    [PgName("yaw_rate")]
    public required double YawRate { get; set; }
    [PgName("pitch_scale")]
    public required double PitchScale { get; set; }
    [PgName("turn_accel_yaw_bonus")]
    public required double TurnAccelYawBonus { get; set; }
    [PgName("turn_accel_pitch_bonus")]
    public required double TurnAccelPitchBonus { get; set; }
    [PgName("turn_accel_delay_seconds")]
    public required double TurnAccelDelaySeconds { get; set; }
    [PgName("turn_accel_time_to_max")]
    public required double TurnAccelTimeToMax { get; set; }

    public virtual bool Equals(LookSettings? other)
    {
        if (other is null) return false;
        if(ReferenceEquals(this, other)) return true;

        return YawRate == other.YawRate
            && PitchScale == other.PitchScale
            && TurnAccelYawBonus == other.TurnAccelYawBonus
            && TurnAccelPitchBonus == other.TurnAccelPitchBonus
            && TurnAccelDelaySeconds == other.TurnAccelDelaySeconds
            && TurnAccelTimeToMax == other.TurnAccelTimeToMax;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(YawRate);
        hash.Add(PitchScale);
        hash.Add(TurnAccelYawBonus);
        hash.Add(TurnAccelYawBonus);
        hash.Add(TurnAccelDelaySeconds);
        hash.Add(TurnAccelTimeToMax);
        return hash.ToHashCode();
    }
}