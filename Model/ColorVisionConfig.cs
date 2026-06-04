using Npgsql;
using Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class ColorVisionConfig : VersionedData, IDatabaseSyncable<ColorVisionConfig, Guid>, IEquatable<ColorVisionConfig>
{
    [SetsRequiredMembers]
    public ColorVisionConfig(Guid playerId, string colorVisionType, int severity, bool correctDeficiency, bool showCorrectDeficiency, bool comfortSwapEffect, bool customOutlineColor, RGBAColor outlineColor, RGBAColor outlineColorLower, double outlineThicknessScale, double outlineBrightnessScale, Int64 version)
    {
        PlayerId = playerId;
        ColorVisionType = colorVisionType ?? throw new ArgumentNullException(nameof(colorVisionType));
        Severity = severity;
        CorrectDeficiency = correctDeficiency;
        ShowCorrectDeficiency = showCorrectDeficiency;
        UseComfortSwapEffect = comfortSwapEffect;
        UseCustomOutlineColor = customOutlineColor;
        OutlineColor = outlineColor;
        OutlineColorLower = outlineColorLower;
        OutlineThicknessScale = outlineThicknessScale;
        OutlineBrightnessScale = outlineBrightnessScale;
        Version = version;
    }

    public required Guid PlayerId { get; set; }
    public required string ColorVisionType { get; set; }
    public required Int32 Severity { get; set; }
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
        await using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        if (!await reader.ReadAsync())
        {
            return null;
        }
        return new ColorVisionConfig(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<string>(1),
            await reader.GetFieldValueAsync<Int32>(2),
            await reader.GetFieldValueAsync<bool>(3),
            await reader.GetFieldValueAsync<bool>(4),
            await reader.GetFieldValueAsync<bool>(5),
            await reader.GetFieldValueAsync<bool>(6),
            await reader.GetFieldValueAsync<RGBAColor>(7),
            await reader.GetFieldValueAsync<RGBAColor>(8),
            await reader.GetFieldValueAsync<double>(9),
            await reader.GetFieldValueAsync<double>(10),
            await reader.GetFieldValueAsync<Int64>(11)
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
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return PlayerId == other.PlayerId
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
            && Version == other.Version;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
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
}