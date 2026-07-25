using System.Diagnostics.CodeAnalysis;

namespace Model;

public class GameEndWeaponStat : IEquatable<GameEndWeaponStat?>
{
    [SetsRequiredMembers]
    public GameEndWeaponStat(string weaponId, long weaponKillCount)
    {
        WeaponId = weaponId ?? throw new ArgumentNullException(nameof(weaponId));
        WeaponKillCount = weaponKillCount;
    }

    public required string WeaponId { get; set; }
    public required Int64 WeaponKillCount { get; set; }

    public override bool Equals(object? obj)
    {
        return Equals(obj as GameEndWeaponStat);
    }

    public bool Equals(GameEndWeaponStat? other)
    {
        return other is not null &&
               WeaponId == other.WeaponId &&
               WeaponKillCount == other.WeaponKillCount;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(WeaponId, WeaponKillCount);
    }

    public static bool operator ==(GameEndWeaponStat? left, GameEndWeaponStat? right)
    {
        return EqualityComparer<GameEndWeaponStat>.Default.Equals(left, right);
    }

    public static bool operator !=(GameEndWeaponStat? left, GameEndWeaponStat? right)
    {
        return !(left == right);
    }
}