namespace Model;

public record class LegacyPlayerData : IAttributeData<LegacyPlayerData, Packets.LegacyPlayerData>
{
    public required LegacySeasonData SeasonData { get; set; }
    public required LegacyStatsData RankedStats { get; set; }
    public required LegacyStatsData CasualStats { get; set; }
    public required LegacyStatsData TeamStats { get; set; }

    public static LegacyPlayerData GetFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public void SyncToDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public Packets.LegacyPlayerData ToPacketType()
    {
        throw new NotImplementedException();
    }
}