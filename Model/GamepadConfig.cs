using Npgsql;
using Persistence;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Model;

public record class GamepadConfig : VersionedData, IDatabaseSyncableDefault<GamepadConfig, Guid>, IEquatable<GamepadConfig>
{
    [SetsRequiredMembers]
    public GamepadConfig(Guid playerId, int inputSchemeIndex, int gamepadGlyphIndex, int lookPresetIndex, LookConfig customLookConfig, ResponseCurve customResponseCurve, bool invertLook, int controllerFeedbackValue, bool turnAccel, bool aimAssist, int responseCurveIndex, double responseCurveArcDeg, double responseCurveSlope, double responseCurveLinearBlendPow, double customScaleADS, bool toggleCrouch, bool toggleWalk, bool togglePlantDefuse, bool toggleADS, string endWalkWhenFiringBehavior, double aDSTriggerThreshold, string deadZoneMoveAmount, double customDeadZoneMoveAmount, string deadZoneLookAmount, double customDeadZoneLookAmount, double walkRunDeflectionThreshold, Int64 version) : base(version)
    {
        PlayerId = playerId;
        InputSchemeIndex = inputSchemeIndex;
        GamepadGlyphIndex = gamepadGlyphIndex;
        LookPresetIndex = lookPresetIndex;
        CustomLookConfig = customLookConfig ?? throw new ArgumentNullException(nameof(customLookConfig));
        CustomResponseCurve = customResponseCurve ?? throw new ArgumentNullException(nameof(customResponseCurve));
        InvertLook = invertLook;
        ControllerFeedbackValue = controllerFeedbackValue;
        TurnAccel = turnAccel;
        AimAssist = aimAssist;
        ResponseCurveIndex = responseCurveIndex;
        ResponseCurveArcDeg = responseCurveArcDeg;
        ResponseCurveSlope = responseCurveSlope;
        ResponseCurveLinearBlendPow = responseCurveLinearBlendPow;
        CustomScaleADS = customScaleADS;
        ToggleCrouch = toggleCrouch;
        ToggleWalk = toggleWalk;
        TogglePlantDefuse = togglePlantDefuse;
        ToggleADS = toggleADS;
        EndWalkWhenFiringBehavior = endWalkWhenFiringBehavior ?? throw new ArgumentNullException(nameof(endWalkWhenFiringBehavior));
        ADSTriggerThreshold = aDSTriggerThreshold;
        DeadZoneMoveAmount = deadZoneMoveAmount ?? throw new ArgumentNullException(nameof(deadZoneMoveAmount));
        CustomDeadZoneMoveAmount = customDeadZoneMoveAmount;
        DeadZoneLookAmount = deadZoneLookAmount ?? throw new ArgumentNullException(nameof(deadZoneLookAmount));
        CustomDeadZoneLookAmount = customDeadZoneLookAmount;
        WalkRunDeflectionThreshold = walkRunDeflectionThreshold;
    }

    public required Guid PlayerId { get; set; }
    public required Int32 InputSchemeIndex { get; set; }
    public required Int32 GamepadGlyphIndex { get; set; }
    public required Int32 LookPresetIndex { get; set; }
    public required LookConfig CustomLookConfig { get; set; }
    public required ResponseCurve CustomResponseCurve { get; set; }
    public required bool InvertLook { get; set; }
    public required Int32 ControllerFeedbackValue { get; set; }
    public required bool TurnAccel { get; set; }
    public required bool AimAssist { get; set; }
    public required Int32 ResponseCurveIndex { get; set; }
    public required double ResponseCurveArcDeg { get; set; }
    public required double ResponseCurveSlope { get; set; }
    public required double ResponseCurveLinearBlendPow { get; set; }
    public required double CustomScaleADS { get; set; }
    public required bool ToggleCrouch { get; set; }
    public required bool ToggleWalk { get; set; }
    public required bool TogglePlantDefuse { get; set; }
    public required bool ToggleADS { get; set; }
    public required string EndWalkWhenFiringBehavior { get; set; }
    public required double ADSTriggerThreshold { get; set; }
    public required string DeadZoneMoveAmount { get; set; }
    public required double CustomDeadZoneMoveAmount { get; set; }
    public required string DeadZoneLookAmount { get; set; }
    public required double CustomDeadZoneLookAmount { get; set; }
    public required double WalkRunDeflectionThreshold { get; set; }

    public static async Task<GamepadConfig?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_gamepad_config.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        if (!await reader.ReadAsync())
        {
            return null;
        }
        return new GamepadConfig(
            await reader.GetFieldValueAsync<Guid>(0),
            await reader.GetFieldValueAsync<Int32>(1),
            await reader.GetFieldValueAsync<Int32>(2),
            await reader.GetFieldValueAsync<Int32>(3),
            await reader.GetFieldValueAsync<LookConfig>(4),
            await reader.GetFieldValueAsync<ResponseCurve>(5),
            await reader.GetFieldValueAsync<bool>(6),
            await reader.GetFieldValueAsync<Int32>(7),
            await reader.GetFieldValueAsync<bool>(8),
            await reader.GetFieldValueAsync<bool>(9),
            await reader.GetFieldValueAsync<Int32>(10),
            await reader.GetFieldValueAsync<double>(11),
            await reader.GetFieldValueAsync<double>(12),
            await reader.GetFieldValueAsync<double>(13),
            await reader.GetFieldValueAsync<double>(14),
            await reader.GetFieldValueAsync<bool>(15),
            await reader.GetFieldValueAsync<bool>(16),
            await reader.GetFieldValueAsync<bool>(17),
            await reader.GetFieldValueAsync<bool>(18),
            await reader.GetFieldValueAsync<string>(19),
            await reader.GetFieldValueAsync<double>(20),
            await reader.GetFieldValueAsync<string>(21),
            await reader.GetFieldValueAsync<double>(22),
            await reader.GetFieldValueAsync<string>(23),
            await reader.GetFieldValueAsync<double>(24),
            await reader.GetFieldValueAsync<double>(25),
            await reader.GetFieldValueAsync<Int64>(26)
        );
    }

    public Guid GetKey()
    {
        return PlayerId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_gamepad_config.sql");
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("input_scheme_index", InputSchemeIndex);
        cmd.Parameters.AddWithValue("gamepad_glyph_index", GamepadGlyphIndex);
        cmd.Parameters.AddWithValue("look_preset_index", LookPresetIndex);
        cmd.Parameters.AddWithValue("custom_look_config", CustomLookConfig);
        cmd.Parameters.AddWithValue("custom_response_curve", CustomResponseCurve);
        cmd.Parameters.AddWithValue("invert_look", InvertLook);
        cmd.Parameters.AddWithValue("controller_feedback_value", ControllerFeedbackValue);
        cmd.Parameters.AddWithValue("turn_accel", TurnAccel);
        cmd.Parameters.AddWithValue("aim_assist", AimAssist);
        cmd.Parameters.AddWithValue("response_curve_index", ResponseCurveIndex);
        cmd.Parameters.AddWithValue("response_curve_arc_deg", ResponseCurveArcDeg);
        cmd.Parameters.AddWithValue("response_curve_slope", ResponseCurveSlope);
        cmd.Parameters.AddWithValue("response_curve_linear_blend_pow", ResponseCurveLinearBlendPow);
        cmd.Parameters.AddWithValue("custom_scale_ads", CustomScaleADS);
        cmd.Parameters.AddWithValue("toggle_crouch", ToggleCrouch);
        cmd.Parameters.AddWithValue("toggle_walk", ToggleWalk);
        cmd.Parameters.AddWithValue("toggle_plant_defuse", TogglePlantDefuse);
        cmd.Parameters.AddWithValue("toggle_ads", ToggleADS);
        cmd.Parameters.AddWithValue("end_walk_when_firing_behavior", EndWalkWhenFiringBehavior);
        cmd.Parameters.AddWithValue("ads_trigger_threshold", ADSTriggerThreshold);
        cmd.Parameters.AddWithValue("dead_zone_move_amount", DeadZoneMoveAmount);
        cmd.Parameters.AddWithValue("custom_dead_zone_move_amount", CustomDeadZoneMoveAmount);
        cmd.Parameters.AddWithValue("dead_zone_look_amount", DeadZoneLookAmount);
        cmd.Parameters.AddWithValue("custom_dead_zone_look_amount", CustomDeadZoneLookAmount);
        cmd.Parameters.AddWithValue("walk_run_deflection_threshold", WalkRunDeflectionThreshold);
        cmd.Parameters.AddWithValue("version", Version);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(GamepadConfig? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return PlayerId == other.PlayerId
            && InputSchemeIndex == other.InputSchemeIndex
            && GamepadGlyphIndex == other.GamepadGlyphIndex
            && LookPresetIndex == other.LookPresetIndex
            && CustomLookConfig.Equals(other.CustomLookConfig)
            && CustomResponseCurve.Equals(other.CustomResponseCurve)
            && InvertLook == other.InvertLook
            && ControllerFeedbackValue == other.ControllerFeedbackValue
            && TurnAccel == other.TurnAccel
            && AimAssist == other.AimAssist
            && ResponseCurveIndex == other.ResponseCurveIndex
            && ResponseCurveArcDeg == other.ResponseCurveArcDeg
            && ResponseCurveSlope == other.ResponseCurveSlope
            && ResponseCurveLinearBlendPow == other.ResponseCurveLinearBlendPow
            && CustomScaleADS == other.CustomScaleADS
            && ToggleCrouch == other.ToggleCrouch
            && ToggleWalk == other.ToggleWalk
            && TogglePlantDefuse == other.TogglePlantDefuse
            && ToggleADS == other.ToggleADS
            && EndWalkWhenFiringBehavior == other.EndWalkWhenFiringBehavior
            && ADSTriggerThreshold == other.ADSTriggerThreshold
            && DeadZoneMoveAmount == other.DeadZoneMoveAmount
            && CustomDeadZoneMoveAmount == other.CustomDeadZoneMoveAmount
            && DeadZoneLookAmount == other.DeadZoneLookAmount
            && CustomDeadZoneLookAmount == other.CustomDeadZoneLookAmount
            && WalkRunDeflectionThreshold == other.WalkRunDeflectionThreshold;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(PlayerId);
        hash.Add(InputSchemeIndex);
        hash.Add(GamepadGlyphIndex);
        hash.Add(LookPresetIndex);
        hash.Add(CustomLookConfig);
        hash.Add(CustomResponseCurve);
        hash.Add(InvertLook);
        hash.Add(ControllerFeedbackValue);
        hash.Add(TurnAccel);
        hash.Add(AimAssist);
        hash.Add(ResponseCurveIndex);
        hash.Add(ResponseCurveArcDeg);
        hash.Add(ResponseCurveSlope);
        hash.Add(ResponseCurveLinearBlendPow);
        hash.Add(CustomScaleADS);
        hash.Add(ToggleCrouch);
        hash.Add(ToggleWalk);
        hash.Add(TogglePlantDefuse);
        hash.Add(ToggleADS);
        hash.Add(EndWalkWhenFiringBehavior);
        hash.Add(ADSTriggerThreshold);
        hash.Add(DeadZoneMoveAmount);
        hash.Add(CustomDeadZoneMoveAmount);
        hash.Add(DeadZoneLookAmount);
        hash.Add(CustomDeadZoneLookAmount);
        hash.Add(WalkRunDeflectionThreshold);
        return hash.ToHashCode();
    }

    public static GamepadConfig CreateDefault(Guid playerId)
    {
        var defaultJson = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "defaults", "GamepadConfig.json")));
        defaultJson[nameof(PlayerId)] = playerId;
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return defaultJson.Deserialize<GamepadConfig>(options);
    }
}