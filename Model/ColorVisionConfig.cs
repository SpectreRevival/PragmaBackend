using Npgsql;
using Persistence;
using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class ColorVisionConfig : VersionedData, IDatabaseSyncable<ColorVisionConfig>
{
    [SetsRequiredMembers]
    public ColorVisionConfig(Guid playerId, string colorVisionType, int severity, bool correctDeficiency, bool showCorrectDeficiency, bool comfortSwapEffect, bool customOutlineColor, RGBAColor outlineColor, RGBAColor outlineColorLower, double outlineThicknessScale, double outlineBrightnessScale)
    {
        PlayerId = playerId;
        ColorVisionType = colorVisionType ?? throw new ArgumentNullException(nameof(colorVisionType));
        Severity = severity;
        CorrectDeficiency = correctDeficiency;
        ShowCorrectDeficiency = showCorrectDeficiency;
        ComfortSwapEffect = comfortSwapEffect;
        CustomOutlineColor = customOutlineColor;
        OutlineColor = outlineColor ?? throw new ArgumentNullException(nameof(outlineColor));
        OutlineColorLower = outlineColorLower ?? throw new ArgumentNullException(nameof(outlineColorLower));
        OutlineThicknessScale = outlineThicknessScale;
        OutlineBrightnessScale = outlineBrightnessScale;
    }

    public required Guid PlayerId { get; set; }
    public required string ColorVisionType { get; set; }
    public required Int32 Severity { get; set; }
    public required bool CorrectDeficiency { get; set; }
    public required bool ShowCorrectDeficiency { get; set; }
    public required bool ComfortSwapEffect { get; set; }
    public required bool CustomOutlineColor { get; set; }
    public required RGBAColor OutlineColor { get; set; }
    public required RGBAColor OutlineColorLower { get; set; }
    public required double OutlineThicknessScale { get; set; }
    public required double OutlineBrightnessScale { get; set; }

    public static async Task<ColorVisionConfig?> RetrieveFromDatabase(string key)
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
            await reader.GetFieldValueAsync<double>(10)
        );
    }

    public object GetKey()
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
        cmd.Parameters.AddWithValue("comfortswapeffect", ComfortSwapEffect);
        cmd.Parameters.AddWithValue("outlinecolor", OutlineColor);
        cmd.Parameters.AddWithValue("outlinecolorlower", OutlineColorLower);
        cmd.Parameters.AddWithValue("outlineThicknessScale", OutlineThicknessScale);
        cmd.Parameters.AddWithValue("outlineBrightnessScale", OutlineBrightnessScale);
        cmd.Parameters.AddWithValue("version", Version);
        await cmd.ExecuteNonQueryAsync();
    }
}