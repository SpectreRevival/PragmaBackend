using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class GameEndPlayer
{
    [SetsRequiredMembers]
    public GameEndPlayer(Guid playerId, string teamId, bool finishedMatch, GameEndPlayerStats stats, string sponsor, GameEndWeaponStat[] weaponStats)
    {
        PlayerId = playerId;
        TeamId = teamId ?? throw new ArgumentNullException(nameof(teamId));
        FinishedMatch = finishedMatch;
        Stats = stats ?? throw new ArgumentNullException(nameof(stats));
        Sponsor = sponsor ?? throw new ArgumentNullException(nameof(sponsor));
        WeaponStats = weaponStats ?? throw new ArgumentNullException(nameof(weaponStats));
    }

    public required Guid PlayerId { get; set; }
    public required string TeamId { get; set; }
    public required bool FinishedMatch { get; set; }
    public required GameEndPlayerStats Stats { get; set; }
    public required string Sponsor { get; set; }
    public required GameEndWeaponStat[] WeaponStats { get; set; }

    public virtual bool Equals(GameEndPlayer? player)
    {
        return player is not null &&
               EqualityComparer<Type>.Default.Equals(EqualityContract, player.EqualityContract) &&
               PlayerId.Equals(player.PlayerId) &&
               TeamId == player.TeamId &&
               FinishedMatch == player.FinishedMatch &&
               EqualityComparer<GameEndPlayerStats>.Default.Equals(Stats, player.Stats) &&
               Sponsor == player.Sponsor &&
               EqualityComparer<GameEndWeaponStat[]>.Default.Equals(WeaponStats, player.WeaponStats);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(EqualityContract, PlayerId, TeamId, FinishedMatch, Stats, Sponsor, WeaponStats);
    }
}