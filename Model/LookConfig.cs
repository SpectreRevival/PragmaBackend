using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class LookConfig : IEquatable<LookConfig>, IInterchangeable<LookConfig, Packets.LookConfig>
{
    [SetsRequiredMembers]
    public LookConfig(string displayName, LookSettings lookSettings, LookSettings lookSettingsADS)
    {
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        LookSettings = lookSettings ?? throw new ArgumentNullException(nameof(lookSettings));
        LookSettingsADS = lookSettingsADS ?? throw new ArgumentNullException(nameof(lookSettingsADS));
    }

    [PgName("display_name")]
    public required string DisplayName { get; set; }
    [PgName("look_settings")]
    public required LookSettings LookSettings { get; set; }
    [PgName("look_settings_ads")]
    public required LookSettings LookSettingsADS { get; set; }

    public static LookConfig FromPacket(Packets.LookConfig inst)
    {
        return new LookConfig(inst.DisplayName, LookSettings.FromPacket(inst.LookSettings), LookSettings.FromPacket(inst.LookSettingsADS));
    }

    public virtual bool Equals(LookConfig? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (DisplayName == other.DisplayName
            && LookSettings.Equals(other.LookSettings)
            && LookSettingsADS.Equals(other.LookSettingsADS)));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(DisplayName);
        hash.Add(LookSettingsADS);
        hash.Add(LookSettings);
        return hash.ToHashCode();
    }

    public Packets.LookConfig ToPacket()
    {
        Packets.LookConfig packet = new()
        {
            DisplayName = DisplayName,
            LookSettings = LookSettings.ToPacket(),
            LookSettingsADS = LookSettingsADS.ToPacket()
        };
        return packet;
    }
}