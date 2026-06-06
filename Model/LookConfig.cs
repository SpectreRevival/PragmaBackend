using NpgsqlTypes;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class LookConfig : IEquatable<LookConfig>
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

    public virtual bool Equals(LookConfig? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return DisplayName == other.DisplayName
            && LookSettings.Equals(other.LookSettings)
            && LookSettingsADS.Equals(other.LookSettingsADS);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(DisplayName);
        hash.Add(LookSettingsADS);
        hash.Add(LookSettings);
        return hash.ToHashCode();
    }
}