namespace Model;

public record class BattlepassData
{
    public required Guid[] ActiveBattlePasses { get; set; }
    public required Guid[] BattlepassQuests { get; set; }
    public required Guid[] ActiveBattlepassQuests { get; set; }
    public required Int32 BattlepassLevel { get; set; }
}