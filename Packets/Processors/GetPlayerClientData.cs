using Google.Protobuf;
using System.Diagnostics.CodeAnalysis;

namespace Packets.Processors;

public class GetPlayerClientData : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public GetPlayerClientData(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnPlayerDataServiceRpc.GetAllPlayerDataClientV1Request");
    }

    public static PlayerMatchmakingData MatchmakingDataConvert(Model.PlayerMatchmakingData mm)
    {
        PlayerMatchmakingData packet = new();
        packet.PlayerId = mm.PlayerId.ToString();
        packet.CasualMmr = mm.CasualMMR;
        packet.RankedMmr = mm.RankedMMR;
        packet.SoloRankPoints = mm.SoloRankPoints;
        packet.CasualMatchesPlayedCount = mm.CasualMatchesPlayed;
        packet.RankedMatchesPlayedCount = mm.RankedMatchesPlayed;
        packet.CasualMatchesPlayedSeasonCount = mm.CasualMatchesPlayedSeasonal;
        packet.RankedMatchesPlayedSeasonCount = mm.RankedMatchesPlayedSeasonal;
        foreach (string placementMatch in mm.RankedPlacementMatches)
        {
            packet.RankedPlacementMatches.Add(placementMatch);
        }
        packet.CurrentSoloRank = mm.CurrentSoloRank;
        packet.HighestTeamRank = mm.HighestTeamRank;
        packet.CasualMatchesWonCount = mm.CasualMatchesWon;
        packet.RankedMatchesWonCount = mm.RankedMatchesWon;
        packet.PriorityMatchmakingUntil = mm.PriorityMatchmakingUntil.ToUnixTimeMilliseconds().ToString();
        packet.RestrictMatchmakingUntil = mm.RestrictMatchmakingUntil.ToUnixTimeMilliseconds().ToString();
        packet.MatchmakingFlags = 1.0;
        packet.MapHistory = mm.MapHistory;
        packet.Country = "US";
        packet.Address = "0.0.0.0";
        packet.PrimaryGeographicRegion = "uswest-1";
        packet.SecondaryGeographicRegion = "uscentral-2";
        packet.Subdivision = "CO";
        return packet;
    }

    public static async Task<FlatInstancedItem> GetFlatInstancedItem(Guid instanceId)
    {
        Model.CustomizedInstancedItem item = await Model.CustomizedInstancedItem.RetrieveFromDatabase(instanceId);
        FlatInstancedItem packet = new();
        packet.ItemInstanceId = item.InstanceId.ToString();
        packet.ItemCatalogId = item.CatalogId;
        return packet;
    }

    public static async Task<ColorVisionConfig> CreatePacketColorVisionConfig(Guid playerId)
    {
        ColorVisionConfig packet = new();
        Model.ColorVisionConfig cfg = await Model.ColorVisionConfig.RetrieveFromDatabase(playerId);
        packet.ColorVisionType = cfg.ColorVisionType;
        packet.Severity = cfg.Severity;
        packet.BCorrectDeficiency = cfg.CorrectDeficiency;
        packet.BShowCorrectDeficiency = cfg.ShowCorrectDeficiency;
        packet.BComfortSwapEffect = cfg.UseComfortSwapEffect;
        packet.BCustomOutlineColor = cfg.UseCustomOutlineColor;
        RGBAColor outlineColor = new();
        outlineColor.R = cfg.OutlineColor.R;
        outlineColor.G = cfg.OutlineColor.G;
        outlineColor.B = cfg.OutlineColor.B;
        outlineColor.A = cfg.OutlineColor.A;
        packet.OutlineColor = outlineColor;
        RGBAColor lowerColor = new();
        lowerColor.R = cfg.OutlineColorLower.R;
        lowerColor.G = cfg.OutlineColorLower.G;
        lowerColor.B = cfg.OutlineColorLower.B;
        lowerColor.A = cfg.OutlineColorLower.A;
        packet.OutlineColorLower = lowerColor;
        packet.OutlineThicknessScale = cfg.OutlineThicknessScale;
        packet.OutlineBrightnessScale = cfg.OutlineBrightnessScale;
        packet.Version = (int)cfg.Version;
        return packet;
    }

    public static async Task<SubtitleUserSettings> CreatePacketSubtitleSettings(Guid playerId)
    {
        SubtitleUserSettings packet = new();
        Model.SubtitleUserSettings cfg = await Model.SubtitleUserSettings.RetrieveFromDatabase(playerId);
        packet.Version = (int)cfg.Version;
        packet.FontSize = cfg.FontSize;
        packet.BackgroundOpacity = cfg.BackgroundOpacity;
        packet.SpeakerQualifierDisplay = cfg.SpeakerQualifierDisplay;
        packet.BPostPlayerSubtitles = cfg.PostPlayerSubtitles;
        packet.BPostPlayerSubtitlesToChat = cfg.PostPlayerSubtitlesToChat;
        packet.NamesToShowMask = cfg.NamesToShowMask;
        return packet;
    }

    public static CrosshairDot ConvertCrosshairDot(Model.CrosshairDot dot)
    {
        CrosshairDot packet = new();
        packet.Thickness = dot.Thickness;
        packet.Opacity = dot.Opacity;
        packet.ColorIndex = dot.ColorIndex;
        RGBAColor customColor = new();
        customColor.R = dot.CustomColor.R;
        customColor.G = dot.CustomColor.G;
        customColor.B = dot.CustomColor.B;
        customColor.A = dot.CustomColor.A;
        packet.CustomColor = customColor;
        packet.BOutlineEnabled = dot.OutlineEnabled;
        RGBAColor outlineColor = new();
        outlineColor.R = dot.CustomOutlineColor.R;
        outlineColor.G = dot.CustomOutlineColor.G;
        outlineColor.B = dot.CustomOutlineColor.B;
        outlineColor.A = dot.CustomOutlineColor.A;
        packet.CustomOutlineColor = outlineColor;
        packet.OutlineOpacity = dot.OutlineOpacity;
        packet.OutlineThickness = dot.OutlineThickness;
        return packet;
    }

    public static PipConfig ConvertPipConfig(Model.PipConfig pip)
    {
        PipConfig packet = new();
        packet.Thickness = pip.Thickness;
        packet.Length = pip.Length;
        packet.Opacity = pip.Opacity;
        packet.Offset = pip.Offset;
        packet.BMoveAccuracyOffset = pip.MoveAccuracyOffset;
        packet.BFireAccuracyOffset = pip.FireAccuracyOffset;
        return packet;
    }

    public static async Task<CrosshairConfig> CreatePacketCrosshairConfig(Guid playerId)
    {
        CrosshairConfig packet = new();
        Model.CrosshairConfig cfg = await Model.CrosshairConfig.RetrieveFromDatabase(playerId);
        packet.Version = (int)cfg.Version;
        packet.ColorIndex = cfg.ColorIndex;
        packet.BAdvancedCrosshairSettings = cfg.AdvancedCrosshairSettings;
        RGBAColor customColor = new();
        customColor.R = cfg.CustomColor.R;
        customColor.G = cfg.CustomColor.G;
        customColor.B = cfg.CustomColor.B;
        customColor.A = cfg.CustomColor.A;
        packet.CustomColor = customColor;
        packet.BFireAccuracyFade = cfg.FireAccuracyFade;
        packet.BFollowRecoil = cfg.FollowRecoil;
        packet.BShowOutlines = cfg.ShowOutlines;
        packet.OutlineThickness = cfg.OutlineThickness;
        packet.OutlineOpacity = cfg.OutlineOpacity;
        packet.BShowCenterDot = cfg.ShowCenterDot;
        packet.BUseADSSettings = cfg.UseADSSettings;
        packet.CenterDotConfig = ConvertCrosshairDot(cfg.CenterDot);
        packet.CenterDotConfigADS = ConvertCrosshairDot(cfg.CenterDotADS);
        packet.SniperDotConfig = ConvertCrosshairDot(cfg.SniperDot);
        PipConfigs pips = new();
        pips.Outer = ConvertPipConfig(cfg.OuterPip);
        pips.Inner = ConvertPipConfig(cfg.InnerPip);
        return packet;
    }

    public static LookSettings ConvertLookSettings(Model.LookSettings cfg)
    {
        LookSettings packet = new();
        packet.YawRate = cfg.YawRate;
        packet.PitchScale = cfg.PitchScale;
        packet.TurnAccelYawBonus = cfg.TurnAccelYawBonus;
        packet.TurnAccelPitchBonus = cfg.TurnAccelPitchBonus;
        packet.TurnAccelDelaySeconds = cfg.TurnAccelDelaySeconds;
        packet.TurnAccelTimeToMax = cfg.TurnAccelTimeToMax;
        return packet;
    }

    public static async Task<GamepadConfig> CreatePacketGamepadConfig(Guid playerId)
    {
        GamepadConfig packet = new();
        Model.GamepadConfig cfg = await Model.GamepadConfig.RetrieveFromDatabase(playerId);
        packet.Version = (int)cfg.Version;
        packet.InputSchemeIndex = cfg.InputSchemeIndex;
        packet.GamepadGlyphIndex = cfg.GamepadGlyphIndex;
        packet.LookPresetIndex = cfg.LookPresetIndex;
        LookConfig look = new();
        look.DisplayName = cfg.CustomLookConfig.DisplayName;
        look.LookSettings = ConvertLookSettings(cfg.CustomLookConfig.LookSettings);
        look.LookSettingsADS = ConvertLookSettings(cfg.CustomLookConfig.LookSettingsADS);
        ResponseCurve curve = new();
        curve.DisplayName = cfg.CustomResponseCurve.DisplayName;
        curve.Exponent = cfg.CustomResponseCurve.Exponent;
        curve.ResponseCurveArcDeg = cfg.CustomResponseCurve.ResponseCurveArcDegree;
        curve.ResponseCurveSlope = cfg.CustomResponseCurve.ResponseCurveSlope;
        curve.ResponseCurveLinearBlendPow = cfg.CustomResponseCurve.ResponseCurveLinearBlendPower;
        packet.CustomResponseCurve = curve;
        packet.CustomLookConfig = look;
        packet.BInvertLook = cfg.InvertLook;
        packet.ControllerFeedbackValue = cfg.ControllerFeedbackValue;
        packet.BTurnAccel = cfg.TurnAccel;
        packet.BAimAssist = cfg.AimAssist;
        packet.ResponseCurveIndex = cfg.ResponseCurveIndex;
        packet.ResponseCurveArcDeg = cfg.ResponseCurveArcDeg;
        packet.ResponseCurveSlope = cfg.ResponseCurveSlope;
        packet.ResponseCurveLinearBlendPow = cfg.ResponseCurveLinearBlendPow;
        packet.CustomScaleADS = cfg.CustomScaleADS;
        packet.BToggleCrouch = cfg.ToggleCrouch;
        packet.BToggleWalk = cfg.ToggleWalk;
        packet.BTogglePlantDefuse = cfg.TogglePlantDefuse;
        packet.BToggleADS = cfg.ToggleADS;
        packet.EndWalkWhenFiringBehavior = cfg.EndWalkWhenFiringBehavior;
        packet.ADSTriggerThreshold = cfg.ADSTriggerThreshold;
        packet.DeadZoneMoveAmount = cfg.DeadZoneMoveAmount;
        packet.CustomDeadZoneMoveAmount = cfg.CustomDeadZoneMoveAmount;
        packet.DeadZoneLookAmount = cfg.DeadZoneLookAmount;
        packet.CustomDeadZoneLookAmount = cfg.CustomDeadZoneLookAmount;
        packet.WalkRunDeflectionThreshold = cfg.WalkRunDeflectionThreshold;
        return packet;
    }

    public static async Task<PlayerConfig> CreatePacketConfigForPlayer(Guid playerId)
    {
        PlayerConfig packet = new();
        Model.PlayerConfig cfg = await Model.PlayerConfig.RetrieveFromDatabase(playerId);
        packet.UnlockAllPlayModes = cfg.UnlockAllPlayModes;
        packet.UnlockAllMenuTabs = cfg.UnlockAllMenuTabs;
        packet.UnlockAllSponsors = cfg.UnlockAllSponsors;
        packet.BypassUnlockAllSponsorsOverride = cfg.BypassUnlockAllSponsorsOverride;
        packet.BypassProgressionOverrides = cfg.BypassProgressionOverrides;
        packet.BypassTeamSizeOverrides = cfg.BypassTeamSizeOverrides;
        packet.BypassRegionSelectOverride = cfg.BypassRegionSelectOverride;
        packet.BypassCurrencyPurchasingOverride = cfg.BypassCurrencyPurchasingOverride;
        packet.DisableDevMapSelector = cfg.DisableDevMapSelector;
        packet.ShowDebugInfoPanel = cfg.ShowDebugInfoPanel;
        packet.ShowPlatformInfoPanel = cfg.ShowPlatformInfoPanel;
        packet.ShowMatchmakingCounters = cfg.ShowMatchmakingCounters;
        packet.ForceChatEnabled = cfg.ForceChatEnabled;
        packet.MostRecentLobbyMode = cfg.MostRecentLobbyMode;
        packet.MostRecentPartyId = cfg.MostRecentPartyId.ToString();
        packet.EndUserLicenseAcceptedVersion = cfg.EndUserLicenseAcceptedVersion;
        packet.EndUserLicenseAcceptedVersionPlayStation = cfg.EndUserLicenseAcceptedVersionPlayStation;
        packet.EndUserLicenseAcceptedVersionXbox = cfg.EndUserLicenseAcceptedVersionXbox;
        packet.TermsOfServiceAcceptedVersion = cfg.TermsOfServiceAcceptedVersion;
        packet.TermsOfServiceAcceptedVersionPlayStation = cfg.TermsOfServiceAcceptedVersionPlayStation;
        packet.TermsOfServiceAcceptedVersionXbox = cfg.TermsOfServiceAcceptedVersionXbox;
        packet.NonDisclosureAgreementAcceptedVersion = cfg.NonDisclosureAgreementAcceptedVersion;
        packet.NonDisclosureAgreementAcceptedVersionPlayStation = cfg.NonDisclosureAgreementAcceptedVersionPlayStation;
        packet.NonDisclosureAgreementAcceptedVersionXbox = cfg.NonDisclosureAgreementAcceptedVersionXbox;
        packet.SeizureWarningAcknowledgedVersion = cfg.SeizureWarningAcknowledgedVersion;
        packet.SeizureWarningAcknowledgedVersionPlayStation = cfg.SeizureWarningAcknowledgedVersionPlayStation;
        packet.SeizureWarningAcknowledgedVersionXbox = cfg.SeizureWarningAcknowledgedVersionXbox;
        packet.BattlepassSeasonLoggedOn = cfg.BattlepassSeasonLoggedOn;
        packet.BattlepassPurchasePopupLastTime = cfg.BattlepassPurchasePopupLastTime;
        packet.NonDisclosureAgreementUserSignature = cfg.NonDisclosureAgreementUserSignature;
        packet.NonDisclosureAgreementUserSignaturePlayStation = cfg.NonDisclosureAgreementUserSignature;
        packet.NonDisclosureAgreementUserSignatureXbox = cfg.NonDisclosureAgreementUserSignatureXbox;
        packet.NonDisclosureAgreementUserEmail = cfg.NonDisclosureAgreementUserEmail;
        packet.NonDisclosureAgreementUserEmailPlayStation = cfg.NonDisclosureAgreementUserEmailPlayStation;
        packet.NonDisclosureAgreementUserEmailXbox = cfg.NonDisclosureAgreementUserEmailXbox;
        packet.LastVersionShownInDriversWarningDialog = cfg.LastVersionShownInDriversWarningDialog;
        packet.MinSpecWarningDialogTimesDisplayed = cfg.MinSpecWarningDialogTimesDisplayed;
        packet.PingWarningDialogTimesDisplayed = cfg.PingWarningDialogTimesDisplayed;
        packet.HasCompletedLaunchSettingsFlow = cfg.HasCompletedLaunchSettingsFlow;
        packet.IsUsingManualMatchmakingRegionSelection = cfg.IsUsingManualMatchmakingRegionSelection;
        foreach (var item in cfg.ManualMatchmakingRegionSelections)
        {
            packet.ManualMatchmakingRegionSelections.Add(item);
        }
        foreach (var item in cfg.RotatingNewsViewedMessages)
        {
            packet.RotatingNewsViewedMessages.Add(item);
        }
        packet.Version = (int)cfg.Version;
        packet.InkQuality = cfg.InkQuality;
        packet.MouseSensitivityADSScale = cfg.MouseSensitivityADSScale;
        packet.MouseSensitivity = cfg.MouseSensitivity;
        packet.MinimapScale = cfg.MinimapScale;
        packet.MinimapSize = cfg.MinimapSize;
        packet.MinimapMaskOpacity = cfg.MinimapMaskOpacity;
        packet.BInvertedYAxis = cfg.InvertedYAxis;
        packet.BToggleCrouch = cfg.ToggleCrouch;
        packet.BToggleWalk = cfg.ToggleWalk;
        packet.BToggleADS = cfg.ToggleADS;
        packet.RecoilBehavior = cfg.RecoilBehavior;
        packet.BLeftHandedEnabled = cfg.LeftHandedEnabled;
        packet.BRecoilPitchCorrectionEnabled = cfg.RecoilPitchCorrectionEnabled;
        packet.BIsTeamLaserEnabled = cfg.IsTeamLaserEnabled;
        packet.BIsHudMinimapRotationEnabled = cfg.IsHudMinimapRotationEnabled;
        packet.BIsHudMinimapCenteredOnPlayer = cfg.IsHudMinimapCenteredOnPlayer;
        packet.BIsHudMinimapCircle = cfg.IsHudMinimapCircle;
        packet.BIsHudMinimapMaskHighContrastEnabled = cfg.IsHudMinimapMaskHighContrastEnabled;
        packet.BIsHudSnapMinimapWithScoreboardEnabled = cfg.IsHudSnapMinimapWithScoreboardEnabled;
        packet.BIsDamageCameraEffectEnabled = cfg.IsDamageCameraEffectEnabled;
        packet.BStreamerModeEnabled = cfg.StreamerModeEnabled;
        packet.BHideLobbyCode = cfg.HideLobbyCode;
        packet.ADSTracerRatio = cfg.ADSTracerRatio;
        packet.ADSTracerIntensity = cfg.ADSTracerIntensity;
        packet.OpticHitConfirmIntensity = cfg.OpticHitConfirmIntensity;
        packet.BAnonymousMode = cfg.AnonymousMode;
        packet.BAnonymizePlayerNames = cfg.AnonymizePlayerNames;
        packet.BStreamerModeDisableIncomingVoiceChat = cfg.StreamerModeDisableIncomingVoiceChat;
        packet.BStreamerModeDisableIncomingTextChat = cfg.StreamerModeDisableIncomingTextChat;
        packet.BIsTextChatSoundEffectsEnabled = cfg.IsTextChatSoundEffectsEnabled;
        packet.BSubtitles = cfg.SubtitlesEnabled;
        packet.VerboseVoLevel = cfg.VerboseVoLevel;
        packet.BIsBloodFXEnabled = cfg.IsBloodFXEnabled;
        packet.ColorVisionConfig = await CreatePacketColorVisionConfig(playerId);
        packet.CrosshairConfig = await CreatePacketCrosshairConfig(playerId);
        packet.SubtitleUserSettings = await CreatePacketSubtitleSettings(playerId);
        packet.GamepadConfig = await CreatePacketGamepadConfig(playerId);
        OverrideKeymaps keymaps = new();
        foreach (var item in cfg.OverrideKeymaps)
        {
            keymaps.Overrides.Add(item);
        }
        packet.OverrideKeymaps = keymaps;
        packet.VoiceChatInputAudioDevice = cfg.VoiceChatInputAudioDevice;
        packet.VoiceChatOutputAudioDevice = cfg.VoiceChatOutputAudioDevice;
        packet.BVoiceChatTeamEnabled = cfg.VoiceChatTeamEnabled;
        packet.VoiceChatConsoleMode = cfg.VoiceChatConsoleMode;
        packet.BVoiceChatPartyEnabledInGames = cfg.VoiceChatPartyEnabledInGames;
        packet.BVoiceChatTeamPushToTalk = cfg.VoiceChatTeamPushToTalk;
        packet.BVoiceChatPartyPushToTalk = cfg.VoiceChatPartyPushToTalk;
        foreach (var item in cfg.EnabledTextStats)
        {
            packet.EnabledTextStats.Add(item);
        }
        foreach (var item in cfg.EnabledGraphStats)
        {
            packet.EnabledGraphStats.Add(item);
        }
        foreach (var item in cfg.MutedChatContexts)
        {
            packet.MutedChatContexts.Add(item);
        }
        packet.InputBindingsVersion = cfg.InputBindingsVersion;
        return packet;
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        FetchLoadoutsRequest req = Packet.GetPayloadAsMessage<FetchLoadoutsRequest>();
        Guid playerId = Guid.Parse(req.PlayerId);
        Model.ProfileData profile = await Model.ProfileData.RetrieveFromDatabase(playerId);
        Model.PlayerMatchmakingData mmData = await Model.PlayerMatchmakingData.RetrieveFromDatabase(playerId);
        PlayerClientData finalRes = new();
        PlayerData res = new();
        res.PlayerId = req.PlayerId;
        res.AttackerOutfitLoadoutId = profile.AttackerOutfitLoadoutId.ToString();
        res.AttackerWeaponLoadoutId = profile.AttackerWeaponLoadoutId.ToString();
        res.DefenderWeaponLoadoutId = profile.DefenderWeaponLoadoutId.ToString();
        res.DefenderOutfitLoadoutId = profile.DefenderOutfitLoadoutId.ToString();
        res.PlayerFlags = profile.PlayerFlags;
        PlayerServiceData serviceData = new();
        res.ServerData = "{}";
        res.PlayerServiceData = serviceData;
        res.LastUpdated = profile.LastUpdated.ToString("yyyy-MM-ddTHH:mm");
        res.LastLogin = profile.LastLogin.ToUnixTimeMilliseconds().ToString();
        res.PostSpray = await GetFlatInstancedItem(profile.PostSprayItemId);
        res.MatchSpray = await GetFlatInstancedItem(profile.MatchSprayItemId);
        res.PreSpray = await GetFlatInstancedItem(profile.PreSprayItemId);
        res.Banner = await GetFlatInstancedItem(profile.BannerItemId);
        res.MatchmakingData = MatchmakingDataConvert(mmData);
        res.Data = await CreatePacketConfigForPlayer(playerId);
        finalRes.Data = res;
        JsonFormatter outFormatter = new(
            new JsonFormatter.Settings(true)
            .WithFormatDefaultValues(true)
            .WithFormatEnumsAsIntegers(true)
            .WithIndentation("")
            .WithPreserveProtoFieldNames(true)
        );
        string playerConfigJsonString = outFormatter.Format(res.Data);
        string jsonString = outFormatter.Format(finalRes);
        string[] strings1 = jsonString.Split("\"data\": {\r\n\"unlockAll"); // strings1[0] contains everything before the data property
        string[] afterstring = strings1[1].Split("\"matchmakingData\""); //afterstring[1] contains everything after the data property
        playerConfigJsonString = playerConfigJsonString.Replace("\r", "");
        playerConfigJsonString = playerConfigJsonString.Replace("\n", "");
        playerConfigJsonString = playerConfigJsonString.Replace("\"", "\\\"");
        string finalString = strings1[0] + $"\"data\": \"{playerConfigJsonString}\"," + "\"matchmakingData\"" + afterstring[1];
        return SpectreWebsocketMessage.From(finalString);
    }
}