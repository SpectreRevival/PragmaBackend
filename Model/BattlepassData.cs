namespace Model;

public record class BattlepassData
{
    public required string[] ActiveBattlePasses { get; set; }
    public required string[] BattlepassQuests { get; set; }
    public required string[] ActiveBattlepassQuests { get; set; }
}