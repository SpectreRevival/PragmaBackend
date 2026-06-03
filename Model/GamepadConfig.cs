namespace Model;

public record class GamepadConfig : VersionedData, IDatabaseSyncable<GamepadConfig>
{
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

    public static Task<GamepadConfig?> RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public Task SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}