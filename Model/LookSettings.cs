using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class LookSettings : IEquatable<LookSettings>, IInterchangeable<LookSettings, Packets.LookSettings>
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

    public static LookSettings FromPacket(Packets.LookSettings inst)
    {
        return new LookSettings(inst.YawRate, inst.PitchScale, inst.TurnAccelYawBonus, inst.TurnAccelPitchBonus, inst.TurnAccelDelaySeconds, inst.TurnAccelTimeToMax);
    }

    public virtual bool Equals(LookSettings? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (YawRate == other.YawRate
            && PitchScale == other.PitchScale
            && TurnAccelYawBonus == other.TurnAccelYawBonus
            && TurnAccelPitchBonus == other.TurnAccelPitchBonus
            && TurnAccelDelaySeconds == other.TurnAccelDelaySeconds
            && TurnAccelTimeToMax == other.TurnAccelTimeToMax));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(YawRate);
        hash.Add(PitchScale);
        hash.Add(TurnAccelYawBonus);
        hash.Add(TurnAccelYawBonus);
        hash.Add(TurnAccelDelaySeconds);
        hash.Add(TurnAccelTimeToMax);
        return hash.ToHashCode();
    }

    public Packets.LookSettings ToPacket()
    {
        Packets.LookSettings packet = new()
        {
            YawRate = YawRate,
            PitchScale = PitchScale,
            TurnAccelYawBonus = TurnAccelYawBonus,
            TurnAccelPitchBonus = TurnAccelPitchBonus,
            TurnAccelDelaySeconds = TurnAccelDelaySeconds,
            TurnAccelTimeToMax = TurnAccelTimeToMax
        };
        return packet;
    }
}