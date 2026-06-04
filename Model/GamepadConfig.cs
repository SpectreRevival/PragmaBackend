using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class GamepadConfig : VersionedData, IDatabaseSyncable<GamepadConfig, Guid>
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

    public static Task<GamepadConfig?> RetrieveFromDatabase(Guid key)
    {
        throw new NotImplementedException();
    }

    public Guid GetKey()
    {
        throw new NotImplementedException();
    }

    public Task SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}