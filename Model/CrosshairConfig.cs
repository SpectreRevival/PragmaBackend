namespace Model;

public record class CrosshairConfig : VersionedData, IDatabaseSyncable<CrosshairConfig>
{
    public required Guid PlayerId { get; set; }
    public required Int32 ColorIndex { get; set; }
    public required bool AdvancedCrosshairSettings { get; set; }
    public required RGBAColor CustomColor { get; set; }
    public required bool FireAccuracyFade { get; set; }
    public required bool FollowRecoil { get; set; }
    public required bool ShowOutlines { get; set; }
    public required double OutlineThickness { get; set; }
    public required double OutlineOpacity { get; set; }
    public required bool ShowCenterDot { get; set; }
    public required bool UseADSSettings { get; set; }
    public required CrosshairDot CenterDot { get; set; }
    public required CrosshairDot CenterDotADS { get; set; }
    public required CrosshairDot SniperDot { get; set; }
    public required PipConfig InnerPip { get; set; }
    public required PipConfig OuterPip { get; set; }

    public static Task<CrosshairConfig?> RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public object GetKey()
    {
        throw new NotImplementedException();
    }

    public Task SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}