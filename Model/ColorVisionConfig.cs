using Model.Persistence;
using Npgsql;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Model;

public record class ColorVisionConfig : VersionedData, IDatabaseSyncableDefault<ColorVisionConfig, Guid>, IEquatable<ColorVisionConfig>, IInterchangeableKeyed<ColorVisionConfig, Packets.ColorVisionConfig, Guid>
{
    [SetsRequiredMembers]
    public ColorVisionConfig(Guid playerId, string colorVisionType, int severity, bool correctDeficiency, bool showCorrectDeficiency, bool useComfortSwapEffect, bool useCustomOutlineColor, RGBAColor outlineColor, RGBAColor outlineColorLower, double outlineThicknessScale, double outlineBrightnessScale, long version) : base(version)
    {
        PlayerId = playerId;
        ColorVisionType = colorVisionType ?? throw new ArgumentNullException(nameof(colorVisionType));
        Severity = severity;
        CorrectDeficiency = correctDeficiency;
        ShowCorrectDeficiency = showCorrectDeficiency;
        UseComfortSwapEffect = useComfortSwapEffect;
        UseCustomOutlineColor = useCustomOutlineColor;
        OutlineColor = outlineColor;
        OutlineColorLower = outlineColorLower;
        OutlineThicknessScale = outlineThicknessScale;
        OutlineBrightnessScale = outlineBrightnessScale;
    }

    public static ColorVisionConfig CreateDefault(Guid playerId)
    {
        JsonNode? defaultJson = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "defaults", "ColorVisionConfig.json")));
        defaultJson[nameof(PlayerId)] = playerId;
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true
        };
        return defaultJson.Deserialize<ColorVisionConfig>(options);
    }

    public required Guid PlayerId { get; set; }
    public required string ColorVisionType { get; set; }
    public required int Severity { get; set; }
    public required bool CorrectDeficiency { get; set; }
    public required bool ShowCorrectDeficiency { get; set; }
    public required bool UseComfortSwapEffect { get; set; }
    public required bool UseCustomOutlineColor { get; set; }
    public required RGBAColor OutlineColor { get; set; }
    public required RGBAColor OutlineColorLower { get; set; }
    public required double OutlineThicknessScale { get; set; }
    public required double OutlineBrightnessScale { get; set; }

    public static async Task<ColorVisionConfig?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_color_vision_config.sql");
        cmd.Parameters.AddWithValue("playerid", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new ColorVisionConfig(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<string>(1),
            await reader.GetFieldValueAsync<int>(2),
            await reader.GetFieldValueAsync<bool>(3),
            await reader.GetFieldValueAsync<bool>(4),
            await reader.GetFieldValueAsync<bool>(5),
            await reader.GetFieldValueAsync<bool>(6),
            await reader.GetFieldValueAsync<RGBAColor>(7),
            await reader.GetFieldValueAsync<RGBAColor>(8),
            await reader.GetFieldValueAsync<double>(9),
            await reader.GetFieldValueAsync<double>(10),
            await reader.GetFieldValueAsync<long>(11)
        );
    }

    public Guid GetKey()
    {
        return PlayerId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_color_vision_config.sql");
        cmd.Parameters.AddWithValue("playerid", PlayerId);
        cmd.Parameters.AddWithValue("colorvisiontype", ColorVisionType);
        cmd.Parameters.AddWithValue("severity", Severity);
        cmd.Parameters.AddWithValue("correctdeficiency", CorrectDeficiency);
        cmd.Parameters.AddWithValue("showcorrectdeficiency", ShowCorrectDeficiency);
        cmd.Parameters.AddWithValue("comfortswapeffect", UseComfortSwapEffect);
        cmd.Parameters.AddWithValue("customoutlinecolor", UseCustomOutlineColor);
        cmd.Parameters.AddWithValue("outlinecolor", OutlineColor);
        cmd.Parameters.AddWithValue("outlinecolorlower", OutlineColorLower);
        cmd.Parameters.AddWithValue("outlineThicknessScale", OutlineThicknessScale);
        cmd.Parameters.AddWithValue("outlineBrightnessScale", OutlineBrightnessScale);
        cmd.Parameters.AddWithValue("version", Version);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(ColorVisionConfig? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (PlayerId == other.PlayerId
            && ColorVisionType == other.ColorVisionType
            && Severity == other.Severity
            && CorrectDeficiency == other.CorrectDeficiency
            && ShowCorrectDeficiency == other.ShowCorrectDeficiency
            && UseComfortSwapEffect == other.UseComfortSwapEffect
            && UseCustomOutlineColor == other.UseCustomOutlineColor
            && OutlineColor.Equals(other.OutlineColor)
            && OutlineColorLower.Equals(other.OutlineColorLower)
            && OutlineThicknessScale == other.OutlineThicknessScale
            && OutlineBrightnessScale == other.OutlineBrightnessScale
            && Version == other.Version));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PlayerId);
        hash.Add(ColorVisionType);
        hash.Add(Severity);
        hash.Add(CorrectDeficiency);
        hash.Add(ShowCorrectDeficiency);
        hash.Add(UseComfortSwapEffect);
        hash.Add(UseCustomOutlineColor);
        hash.Add(OutlineColor);
        hash.Add(OutlineColorLower);
        hash.Add(OutlineThicknessScale);
        hash.Add(OutlineBrightnessScale);
        hash.Add(Version);
        return hash.ToHashCode();
    }

    public static ColorVisionConfig FromPacket(Packets.ColorVisionConfig inst, Guid id)
    {
        return new ColorVisionConfig(id, inst.ColorVisionType, inst.Severity, inst.BCorrectDeficiency, inst.BShowCorrectDeficiency, inst.BComfortSwapEffect, inst.BCustomOutlineColor, RGBAColor.FromPacket(inst.OutlineColor), RGBAColor.FromPacket(inst.OutlineColorLower), inst.OutlineThicknessScale, inst.OutlineBrightnessScale, inst.Version);
    }

    public Packets.ColorVisionConfig ToPacket()
    {
        Packets.ColorVisionConfig packet = new()
        {
            ColorVisionType = ColorVisionType,
            Severity = Severity,
            BCorrectDeficiency = CorrectDeficiency,
            BShowCorrectDeficiency = ShowCorrectDeficiency,
            BComfortSwapEffect = UseComfortSwapEffect,
            BCustomOutlineColor = UseCustomOutlineColor
        };
        Packets.RGBAColor outlineColor = new()
        {
            R = OutlineColor.R,
            G = OutlineColor.G,
            B = OutlineColor.B,
            A = OutlineColor.A
        };
        packet.OutlineColor = outlineColor;
        Packets.RGBAColor lowerColor = new()
        {
            R = OutlineColorLower.R,
            G = OutlineColorLower.G,
            B = OutlineColorLower.B,
            A = OutlineColorLower.A
        };
        packet.OutlineColorLower = lowerColor;
        packet.OutlineThicknessScale = OutlineThicknessScale;
        packet.OutlineBrightnessScale = OutlineBrightnessScale;
        packet.Version = (int)Version;
        return packet;
    }
}