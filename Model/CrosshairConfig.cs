using Model.Persistence;
using Npgsql;
using Packets;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Model;

public record class CrosshairConfig : VersionedData, IDatabaseSyncableDefault<CrosshairConfig, Guid>, IEquatable<CrosshairConfig>, IInterchangeableKeyed<CrosshairConfig, Packets.CrosshairConfig, Guid>
{
    [SetsRequiredMembers]
    public CrosshairConfig(Guid playerId, int colorIndex, bool advancedCrosshairSettings, RGBAColor customColor, bool fireAccuracyFade, bool followRecoil, bool showOutlines, double outlineThickness, double outlineOpacity, bool showCenterDot, bool useADSSettings, CrosshairDot centerDot, CrosshairDot centerDotADS, CrosshairDot sniperDot, PipConfig innerPip, PipConfig outerPip, long version) : base(version)
    {
        PlayerId = playerId;
        ColorIndex = colorIndex;
        AdvancedCrosshairSettings = advancedCrosshairSettings;
        CustomColor = customColor;
        FireAccuracyFade = fireAccuracyFade;
        FollowRecoil = followRecoil;
        ShowOutlines = showOutlines;
        OutlineThickness = outlineThickness;
        OutlineOpacity = outlineOpacity;
        ShowCenterDot = showCenterDot;
        UseADSSettings = useADSSettings;
        CenterDot = centerDot ?? throw new ArgumentNullException(nameof(centerDot));
        CenterDotADS = centerDotADS ?? throw new ArgumentNullException(nameof(centerDotADS));
        SniperDot = sniperDot ?? throw new ArgumentNullException(nameof(sniperDot));
        InnerPip = innerPip ?? throw new ArgumentNullException(nameof(innerPip));
        OuterPip = outerPip ?? throw new ArgumentNullException(nameof(outerPip));
    }

    public required Guid PlayerId { get; set; }
    public required int ColorIndex { get; set; }
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

    public static async Task<CrosshairConfig?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_crosshair_config.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new CrosshairConfig(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<int>(1),
            await reader.GetFieldValueAsync<bool>(2),
            await reader.GetFieldValueAsync<RGBAColor>(3),
            await reader.GetFieldValueAsync<bool>(4),
            await reader.GetFieldValueAsync<bool>(5),
            await reader.GetFieldValueAsync<bool>(6),
            await reader.GetFieldValueAsync<double>(7),
            await reader.GetFieldValueAsync<double>(8),
            await reader.GetFieldValueAsync<bool>(9),
            await reader.GetFieldValueAsync<bool>(10),
            await reader.GetFieldValueAsync<CrosshairDot>(11),
            await reader.GetFieldValueAsync<CrosshairDot>(12),
            await reader.GetFieldValueAsync<CrosshairDot>(13),
            await reader.GetFieldValueAsync<PipConfig>(14),
            await reader.GetFieldValueAsync<PipConfig>(15),
            await reader.GetFieldValueAsync<long>(16)
        );
    }

    public Guid GetKey()
    {
        return PlayerId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_crosshair_config.sql");
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("color_index", ColorIndex);
        cmd.Parameters.AddWithValue("advanced_crosshair_settings", AdvancedCrosshairSettings);
        cmd.Parameters.AddWithValue("custom_color", CustomColor);
        cmd.Parameters.AddWithValue("fire_accuracy_fade", FireAccuracyFade);
        cmd.Parameters.AddWithValue("follow_recoil", FollowRecoil);
        cmd.Parameters.AddWithValue("show_outlines", ShowOutlines);
        cmd.Parameters.AddWithValue("outline_thickness", OutlineThickness);
        cmd.Parameters.AddWithValue("outline_opacity", OutlineOpacity);
        cmd.Parameters.AddWithValue("show_center_dot", ShowCenterDot);
        cmd.Parameters.AddWithValue("use_ads_settings", UseADSSettings);
        cmd.Parameters.AddWithValue("center_dot", CenterDot);
        cmd.Parameters.AddWithValue("center_dot_ads", CenterDotADS);
        cmd.Parameters.AddWithValue("sniper_dot", SniperDot);
        cmd.Parameters.AddWithValue("inner_pip", InnerPip);
        cmd.Parameters.AddWithValue("outer_pip", OuterPip);
        cmd.Parameters.AddWithValue("version", Version);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(CrosshairConfig? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (PlayerId == other.PlayerId
            && ColorIndex == other.ColorIndex
            && AdvancedCrosshairSettings == other.AdvancedCrosshairSettings
            && CustomColor.Equals(other.CustomColor)
            && FireAccuracyFade.Equals(other.FireAccuracyFade)
            && FollowRecoil == other.FollowRecoil
            && ShowOutlines == other.ShowOutlines
            && OutlineThickness == other.OutlineThickness
            && OutlineOpacity == other.OutlineOpacity
            && UseADSSettings == other.UseADSSettings
            && CenterDot.Equals(other.CenterDot)
            && CenterDotADS.Equals(other.CenterDotADS)
            && SniperDot.Equals(other.SniperDot)
            && InnerPip.Equals(other.InnerPip)
            && OuterPip.Equals(other.OuterPip)
            && Version == other.Version));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PlayerId);
        hash.Add(ColorIndex);
        hash.Add(AdvancedCrosshairSettings);
        hash.Add(CustomColor);
        hash.Add(FireAccuracyFade);
        hash.Add(FollowRecoil);
        hash.Add(ShowOutlines);
        hash.Add(OutlineThickness);
        hash.Add(OutlineOpacity);
        hash.Add(UseADSSettings);
        hash.Add(CenterDot);
        hash.Add(CenterDotADS);
        hash.Add(SniperDot);
        hash.Add(InnerPip);
        hash.Add(OuterPip);
        hash.Add(Version);
        return hash.ToHashCode();
    }

    public static CrosshairConfig CreateDefault(Guid playerId)
    {
        JsonNode? defaultJson = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "defaults", "CrosshairConfig.json")));
        defaultJson[nameof(PlayerId)] = playerId;
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true
        };
        return defaultJson.Deserialize<CrosshairConfig>(options);
    }

    public static CrosshairConfig FromPacket(Packets.CrosshairConfig inst, Guid id)
    {
        return new CrosshairConfig(id, inst.ColorIndex, inst.BAdvancedCrosshairSettings, RGBAColor.FromPacket(inst.CustomColor), inst.BFireAccuracyFade, inst.BFollowRecoil, inst.BShowOutlines, inst.OutlineThickness, inst.OutlineOpacity, inst.BShowCenterDot, inst.BUseADSSettings, CrosshairDot.FromPacket(inst.CenterDotConfig), CrosshairDot.FromPacket(inst.CenterDotConfigADS), CrosshairDot.FromPacket(inst.SniperDotConfig), PipConfig.FromPacket(inst.PipConfigs.Inner), PipConfig.FromPacket(inst.PipConfigs.Outer), inst.Version);
    }

    public Packets.CrosshairConfig ToPacket()
    {
        Packets.CrosshairConfig packet = new()
        {
            Version = (int)Version,
            ColorIndex = ColorIndex,
            BAdvancedCrosshairSettings = AdvancedCrosshairSettings
        };
        Packets.RGBAColor customColor = new()
        {
            R = CustomColor.R,
            G = CustomColor.G,
            B = CustomColor.B,
            A = CustomColor.A
        };
        packet.CustomColor = customColor;
        packet.BFireAccuracyFade = FireAccuracyFade;
        packet.BFollowRecoil = FollowRecoil;
        packet.BShowOutlines = ShowOutlines;
        packet.OutlineThickness = OutlineThickness;
        packet.OutlineOpacity = OutlineOpacity;
        packet.BShowCenterDot = ShowCenterDot;
        packet.BUseADSSettings = UseADSSettings;
        packet.CenterDotConfig = CenterDot.ToPacket();
        packet.CenterDotConfigADS = CenterDotADS.ToPacket();
        packet.SniperDotConfig = SniperDot.ToPacket();
        packet.PipConfigs = new PipConfigs()
        {
            Outer = OuterPip.ToPacket(),
            Inner = InnerPip.ToPacket()
        };
        return packet;
    }
}