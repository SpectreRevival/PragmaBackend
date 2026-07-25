using System.Diagnostics.CodeAnalysis;

namespace Model;

public class GameEndPlayerStats : IEquatable<GameEndPlayerStats?>
{
    [SetsRequiredMembers]
    public GameEndPlayerStats(long killCount, long deathCount, long aceCount, long dualityKillCount, long firstKillCount, long firstDeathCount, double kAST, double dualityRating, long assistCount, long tradeCount, double impactCount, long winCount, long roundsPlayed, long roundsSurvived, long numHeadshots, long numDamagingShots, long totalDamage)
    {
        KillCount = killCount;
        DeathCount = deathCount;
        AceCount = aceCount;
        DualityKillCount = dualityKillCount;
        FirstKillCount = firstKillCount;
        FirstDeathCount = firstDeathCount;
        KAST = kAST;
        DualityRating = dualityRating;
        AssistCount = assistCount;
        TradeCount = tradeCount;
        ImpactCount = impactCount;
        WinCount = winCount;
        RoundsPlayed = roundsPlayed;
        RoundsSurvived = roundsSurvived;
        NumHeadshots = numHeadshots;
        NumDamagingShots = numDamagingShots;
        TotalDamage = totalDamage;
    }

    public required Int64 KillCount { get; set; }
    public required Int64 DeathCount { get; set; }
    public required Int64 AceCount { get; set; }
    public required Int64 DualityKillCount { get; set; }
    public required Int64 FirstKillCount { get; set; }
    public required Int64 FirstDeathCount { get; set; }
    public required double KAST { get; set; }
    public required double DualityRating { get; set; }
    public required Int64 AssistCount { get; set; }
    public required Int64 TradeCount { get; set; }
    public required double ImpactCount { get; set; }
    public required Int64 WinCount { get; set; }
    public required Int64 RoundsPlayed { get; set; }
    public required Int64 RoundsSurvived { get; set; }
    public required Int64 NumHeadshots { get; set; }
    public required Int64 NumDamagingShots { get; set; }
    public required Int64 TotalDamage { get; set; }

    public override bool Equals(object? obj)
    {
        return Equals(obj as GameEndPlayerStats);
    }

    public bool Equals(GameEndPlayerStats? other)
    {
        return other is not null &&
               KillCount == other.KillCount &&
               DeathCount == other.DeathCount &&
               AceCount == other.AceCount &&
               DualityKillCount == other.DualityKillCount &&
               FirstKillCount == other.FirstKillCount &&
               FirstDeathCount == other.FirstDeathCount &&
               KAST == other.KAST &&
               DualityRating == other.DualityRating &&
               AssistCount == other.AssistCount &&
               TradeCount == other.TradeCount &&
               ImpactCount == other.ImpactCount &&
               WinCount == other.WinCount &&
               RoundsPlayed == other.RoundsPlayed &&
               RoundsSurvived == other.RoundsSurvived &&
               NumHeadshots == other.NumHeadshots &&
               NumDamagingShots == other.NumDamagingShots &&
               TotalDamage == other.TotalDamage;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(KillCount);
        hash.Add(DeathCount);
        hash.Add(AceCount);
        hash.Add(DualityKillCount);
        hash.Add(FirstKillCount);
        hash.Add(FirstDeathCount);
        hash.Add(KAST);
        hash.Add(DualityRating);
        hash.Add(AssistCount);
        hash.Add(TradeCount);
        hash.Add(ImpactCount);
        hash.Add(WinCount);
        hash.Add(RoundsPlayed);
        hash.Add(RoundsSurvived);
        hash.Add(NumHeadshots);
        hash.Add(NumDamagingShots);
        hash.Add(TotalDamage);
        return hash.ToHashCode();
    }

    public static bool operator ==(GameEndPlayerStats? left, GameEndPlayerStats? right)
    {
        return EqualityComparer<GameEndPlayerStats>.Default.Equals(left, right);
    }

    public static bool operator !=(GameEndPlayerStats? left, GameEndPlayerStats? right)
    {
        return !(left == right);
    }
}