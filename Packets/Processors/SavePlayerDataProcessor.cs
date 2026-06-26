using Google.Protobuf;
using Model;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Packets.Processors;

public class SavePlayerDataProcessor : WebsocketPacketProcessor, IWebsocketPacketProcessorSingleton
{
    [SetsRequiredMembers]
    public SavePlayerDataProcessor(SpectreRpcType rpcType) : base(rpcType)
    {
    }

    public static SpectreRpcType GetRpcType()
    {
        return new SpectreRpcType("MtnPlayerDataServiceRpc.SavePlayerDataForClientV1Request");
    }

    private static async Task ConvertAndSaveItem(FlatInstancedItem item, Guid playerId)
    {
        CustomizedInstancedItem newItem = new(playerId, item.ItemCatalogId, Guid.Parse(item.ItemInstanceId), true, []);
        await newItem.SyncToDatabase();
    }

    private static async Task ConvertAndSavePlayerConfig(PlayerConfig config, Guid playerId)
    {
        Model.PlayerConfig modelcfg = new(
            playerId,
            config.UnlockAllPlayModes,
            config.UnlockAllMenuTabs,
            config.UnlockAllSponsors,
            config.BypassUnlockAllSponsorsOverride,
            config.BypassProgressionOverrides,
            config.BypassTeamSizeOverrides,
            config.BypassRegionSelectOverride,
            config.BypassCurrencyPurchasingOverride,
            config.DisableDevMapSelector,
            config.ShowDebugInfoPanel,
            config.ShowPlatformInfoPanel,
            config.ShowMatchmakingCounters,
            config.ForceChatEnabled,
            config.MostRecentLobbyMode,
            Guid.Parse(config.MostRecentPartyId),
            config.EndUserLicenseAcceptedVersion,
            config.EndUserLicenseAcceptedVersionPlayStation,
            config.EndUserLicenseAcceptedVersionXbox,
            config.TermsOfServiceAcceptedVersion,
            config.TermsOfServiceAcceptedVersionPlayStation,
            config.TermsOfServiceAcceptedVersionXbox,
            config.NonDisclosureAgreementAcceptedVersion,
            config.NonDisclosureAgreementAcceptedVersionPlayStation,
            config.NonDisclosureAgreementAcceptedVersionXbox,
            config.SeizureWarningAcknowledgedVersion,
            config.SeizureWarningAcknowledgedVersionPlayStation,
            config.SeizureWarningAcknowledgedVersionXbox,
            config.BattlepassSeasonLoggedOn,
            config.BattlepassPurchasePopupLastTime,
            config.NonDisclosureAgreementUserSignature,
            config.NonDisclosureAgreementUserSignaturePlayStation,
            config.NonDisclosureAgreementUserSignatureXbox,
            config.NonDisclosureAgreementUserEmail,
            config.NonDisclosureAgreementUserEmailPlayStation,
            config.NonDisclosureAgreementUserEmailXbox,
            config.LastVersionShownInDriversWarningDialog,
            config.MinSpecWarningDialogTimesDisplayed,
            config.PingWarningDialogTimesDisplayed,
            config.HasCompletedLaunchSettingsFlow,
            config.IsUsingManualMatchmakingRegionSelection,
            config.ManualMatchmakingRegionSelections.ToArray(),
            config.RotatingNewsViewedMessages.ToArray(),
            config.InkQuality,
            config.MouseSensitivityADSScale,
            config.MouseSensitivity,
            config.MinimapScale,
            config.MinimapSize,
            config.MinimapMaskOpacity,
            config.BInvertedYAxis,
            config.BToggleCrouch,
            config.BToggleWalk,
            config.BToggleADS,
            config.RecoilBehavior,
            config.BLeftHandedEnabled,
            config.BRecoilPitchCorrectionEnabled,
            config.BIsTeamLaserEnabled,
            config.BIsHudMinimapRotationEnabled,
            config.BIsHudMinimapCenteredOnPlayer,
            config.BIsHudMinimapCircle,
            config.BIsHudMinimapMaskHighContrastEnabled,
            config.BIsHudSnapMinimapWithScoreboardEnabled,
            config.BIsDamageCameraEffectEnabled,
            config.BStreamerModeEnabled,
            config.BHideLobbyCode,
            config.ADSTracerRatio,
            config.ADSTracerIntensity,
            config.OpticHitConfirmIntensity,
            config.BAnonymousMode,
            config.BAnonymizePlayerNames,
            config.BStreamerModeDisableIncomingVoiceChat,
            config.BStreamerModeDisableIncomingTextChat,
            config.BIsTextChatSoundEffectsEnabled,
            config.BSubtitles,
            config.VerboseVoLevel,
            config.BIsBloodFXEnabled,
            config.OverrideKeymaps.Overrides.ToArray(),
            config.VoiceChatInputAudioDevice,
            config.VoiceChatOutputAudioDevice,
            config.BVoiceChatTeamEnabled,
            config.VoiceChatConsoleMode,
            config.BVoiceChatPartyEnabled,
            config.BVoiceChatPartyEnabledInGames,
            config.BVoiceChatTeamPushToTalk,
            config.BVoiceChatPartyPushToTalk,
            config.EnabledTextStats.ToArray(),
            config.EnabledGraphStats.ToArray(),
            config.MutedChatContexts.ToArray(),
            config.InputBindingsVersion,
            config.Version + 1);
        await modelcfg.SyncToDatabase();
    }

    private static async Task ConvertAndSaveSubtitleUserSettings(SubtitleUserSettings settings, Guid playerId)
    {
        Model.SubtitleUserSettings dat = new(playerId, settings.FontSize, settings.BackgroundOpacity, settings.SpeakerQualifierDisplay, settings.BPostPlayerSubtitles, settings.BPostPlayerSubtitlesToChat, settings.NamesToShowMask, settings.Version);
        await dat.SyncToDatabase();
    }

    private static Model.RGBAColor ConvertRGBAColor(RGBAColor color)
    {
        return new Model.RGBAColor((byte)color.R, (byte)color.G, (byte)color.B, (byte)color.A);
    }

    private static Model.PipConfig ConvertPipConfig(PipConfig config)
    {
        return new Model.PipConfig(config.Thickness, config.Length, config.Opacity, config.Offset, config.BMoveAccuracyOffset, config.BFireAccuracyOffset);
    }

    private static Model.CrosshairDot ConvertCrosshairDot(CrosshairDot dot)
    {
        return new Model.CrosshairDot(dot.Thickness, dot.Opacity, dot.ColorIndex, ConvertRGBAColor(dot.CustomColor), dot.BOutlineEnabled, ConvertRGBAColor(dot.CustomOutlineColor), dot.OutlineOpacity, dot.Thickness);
    }

    private static async Task ConvertAndSaveCrosshairConfig(CrosshairConfig cfg, Guid playerId)
    {
        Model.CrosshairConfig dat = new(playerId, cfg.ColorIndex, cfg.BAdvancedCrosshairSettings, ConvertRGBAColor(cfg.CustomColor), cfg.BFireAccuracyFade, cfg.BFollowRecoil, cfg.BShowOutlines, cfg.OutlineThickness, cfg.OutlineOpacity, cfg.BShowCenterDot, cfg.BUseADSSettings,
            ConvertCrosshairDot(cfg.CenterDotConfig), ConvertCrosshairDot(cfg.SniperDotConfig), ConvertCrosshairDot(cfg.CenterDotConfigADS), ConvertPipConfig(cfg.PipConfigs.Inner), ConvertPipConfig(cfg.PipConfigs.Outer), cfg.Version);
        await dat.SyncToDatabase();
    }

    private static async Task ConvertAndSaveColorVisionConfig(ColorVisionConfig cfg, Guid playerId)
    {
        Model.ColorVisionConfig dat = new(playerId, cfg.ColorVisionType, cfg.Severity, cfg.BCorrectDeficiency, cfg.BShowCorrectDeficiency, cfg.BComfortSwapEffect, cfg.BCustomOutlineColor, ConvertRGBAColor(cfg.OutlineColor), ConvertRGBAColor(cfg.OutlineColorLower), cfg.OutlineThicknessScale, cfg.OutlineBrightnessScale, cfg.Version);
        await dat.SyncToDatabase();
    }

    private static Model.LookConfig ConvertLookConfig(LookConfig cfg)
    {
        return new Model.LookConfig(cfg.DisplayName, ConvertLookSettings(cfg.LookSettings), ConvertLookSettings(cfg.LookSettingsADS));
    }

    private static Model.LookSettings ConvertLookSettings(LookSettings cfg)
    {
        return new Model.LookSettings(cfg.YawRate, cfg.PitchScale, cfg.TurnAccelYawBonus, cfg.TurnAccelPitchBonus, cfg.TurnAccelDelaySeconds, cfg.TurnAccelTimeToMax);
    }

    private static Model.ResponseCurve ConvertResponseCurve(ResponseCurve curve)
    {
        return new Model.ResponseCurve(curve.DisplayName, curve.Exponent, curve.ResponseCurveArcDeg, curve.ResponseCurveSlope, curve.ResponseCurveLinearBlendPow);
    }

    private static async Task ConvertAndSaveGamepadConfig(GamepadConfig cfg, Guid playerId)
    {
        Model.GamepadConfig dat = new(playerId, cfg.InputSchemeIndex, cfg.GamepadGlyphIndex, cfg.LookPresetIndex, ConvertLookConfig(cfg.CustomLookConfig), ConvertResponseCurve(cfg.CustomResponseCurve),
            cfg.BInvertLook, cfg.ControllerFeedbackValue, cfg.BTurnAccel, cfg.BAimAssist, cfg.ResponseCurveIndex, cfg.ResponseCurveArcDeg, cfg.ResponseCurveSlope, cfg.ResponseCurveLinearBlendPow, cfg.CustomScaleADS, cfg.BToggleCrouch, cfg.BToggleWalk, cfg.BTogglePlantDefuse, cfg.BToggleADS, cfg.EndWalkWhenFiringBehavior, cfg.ADSTriggerThreshold, cfg.DeadZoneMoveAmount, cfg.CustomDeadZoneMoveAmount, cfg.DeadZoneLookAmount, cfg.CustomDeadZoneLookAmount, cfg.WalkRunDeflectionThreshold, cfg.Version);
        await dat.SyncToDatabase();
    }

    public override async Task<SpectreWebsocketMessage> ProcessPacket(SpectreWebsocketRequest Packet, SpectreWebsocket ConnectionHandler)
    {
        string innerJson = Packet.RequestPayload["data"].ToString();
        innerJson = innerJson.Replace("\\\"", "\"");
        innerJson = innerJson.Replace("\\r", "");
        innerJson = innerJson.Replace("\\n", "");
        innerJson = innerJson.Replace("\\t", "");
        innerJson = innerJson.Replace("\\u0022", "\"");
        innerJson = innerJson.Replace("\\u002B", "+");
        Packet.RequestPayload["data"] = JsonNode.Parse(innerJson);
        var parser = new JsonParser(JsonParser.Settings.Default);
        PlayerData packetData = parser.Parse<PlayerData>(Packet.RequestPayload.ToJsonString());
        Guid playerId = Guid.Parse(packetData.PlayerId);
        Model.ProfileData profile = await Model.ProfileData.RetrieveFromDatabase(playerId);
        if (packetData.Banner.ItemCatalogId != "" && packetData.Banner.ItemInstanceId != "")
        {
            await ConvertAndSaveItem(packetData.Banner, playerId);
            profile.BannerItemId = Guid.Parse(packetData.Banner.ItemInstanceId);
        }
        if(packetData.PreSpray.ItemCatalogId != "" && packetData.PreSpray.ItemInstanceId != "")
        {
            await ConvertAndSaveItem(packetData.PreSpray, playerId);
            profile.PreSprayItemId = Guid.Parse(packetData.PreSpray.ItemInstanceId);
        }
        if (packetData.MatchSpray.ItemCatalogId != "" && packetData.MatchSpray.ItemInstanceId != "")
        {
            await ConvertAndSaveItem(packetData.MatchSpray, playerId);
            profile.MatchSprayItemId = Guid.Parse(packetData.MatchSpray.ItemInstanceId);
        }
        if (packetData.PostSpray.ItemCatalogId != "" && packetData.PostSpray.ItemInstanceId != "")
        {
            await ConvertAndSaveItem(packetData.PostSpray, playerId);
            profile.PostSprayItemId = Guid.Parse(packetData.PostSpray.ItemInstanceId);
        }

        if (packetData.AttackerOutfitLoadoutId != "") profile.AttackerOutfitLoadoutId = Guid.Parse(packetData.AttackerOutfitLoadoutId);
        if (packetData.AttackerWeaponLoadoutId != "") profile.AttackerWeaponLoadoutId = Guid.Parse(packetData.AttackerWeaponLoadoutId);
        if (packetData.DefenderOutfitLoadoutId != "") profile.DefenderOutfitLoadoutId = Guid.Parse(packetData.DefenderOutfitLoadoutId);
        if (packetData.DefenderWeaponLoadoutId != "") profile.DefenderWeaponLoadoutId = Guid.Parse(packetData.DefenderWeaponLoadoutId);
        profile.PlayerFlags = (int)Math.Round(packetData.PlayerFlags);

        await ConvertAndSavePlayerConfig(packetData.Data, playerId);
        await ConvertAndSaveCrosshairConfig(packetData.Data.CrosshairConfig, playerId);
        await ConvertAndSaveSubtitleUserSettings(packetData.Data.SubtitleUserSettings, playerId);
        await ConvertAndSaveColorVisionConfig(packetData.Data.ColorVisionConfig, playerId);
        await ConvertAndSaveGamepadConfig(packetData.Data.GamepadConfig, playerId);

        await profile.SyncToDatabase();

        return SpectreWebsocketMessage.From("{\"success\":true}");
    }
}