using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class PlayerConfig : VersionedData, IDatabaseSyncable<PlayerConfig, Guid>
{
    [SetsRequiredMembers]
    public PlayerConfig(Guid playerId, bool unlockAllPlayModes, bool unlockAllMenuTabs, bool unlockAllSponsors, bool bypassUnlockAllSponsorsOverride, bool bypassProgressionOverrides, bool bypassTeamSizeOverrides, bool bypassRegionSelectOverride, bool bypassCurrencyPurchasingOverride, bool disableDevMapSelector, bool showDebugInfoPanel, bool showPlatformInfoPanel, bool showMatchmakingCounters, bool forceChatEnabled, int mostRecentLobbyMode, Guid mostRecentPartyId, int endUserLicenseAcceptedVersion, int endUserLicenseAcceptedVersionPlayStation, int endUserLicenseAcceptedVersionXbox, int termsOfServiceAcceptedVersion, int termsOfServiceAcceptedVersionPlayStation, int termsOfServiceAcceptedVersionXbox, int nonDisclosureAgreementAcceptedVersion, int nonDisclosureAgreementAcceptedVersionPlayStation, int nonDisclosureAgreementAcceptedVersionXbox, int seizureWarningAcknowledgedVersion, int seizureWarningAcknowledgedVersionPlayStation, int seizureWarningAcknowledgedVersionXbox, int battlepassSeasonLoggedOn, double battlepassPurchasePopupLastTime, string nonDisclosureAgreementUserSignature, string nonDisclosureAgreementUserSignaturePlayStation, string nonDisclosureAgreementUserSignatureXbox, string nonDisclosureAgreementUserEmail, string nonDisclosureAgreementUserEmailPlayStation, string nonDisclosureAgreementUserEmailXbox, string lastVersionShownInDriversWarningDialog, int minSpecWarningDialogTimesDisplayed, int pingWarningDialogTimesDisplayed, bool hasCompletedLaunchSettingsFlow, bool isUsingManualMatchmakingRegionSelection, string[] manualMatchmakingRegionSelections, string[] rotatingNewsViewedMessages, double inkQuality, double mouseSensitivityADSScale, double mouseSensitivity, double minimapScale, double minimapSize, double minimapMaskOpacity, bool invertedYAxis, bool toggleCrouch, bool toggleWalk, bool toggleADS, string recoilBehavior, bool leftHandedEnabled, bool recoilPitchCorrectionEnabled, bool isTeamLaserEnabled, bool isHudMinimapRotationEnabled, bool isHudMinimapCenteredOnPlayer, bool isHudMinimapCircle, bool isHudMinimapMaskHighContrastEnabled, bool isHudSnapMinimapWithScoreboardEnabled, bool isDamageCameraEffectEnabled, bool streamerModeEnabled, bool hideLobbyCode, double aDSTracerRatio, double aDSTracerIntensity, double opticHitConfirmIntensity, bool anonymousMode, bool anonymizePlayerNames, bool streamerModeDisableIncomingVoiceChat, bool streamerModeDisableIncomingTextChat, bool isTextChatSoundEffectsEnabled, bool subtitlesEnabled, string verboseVoLevel, bool isBloodFXEnabled, ColorVisionConfig colorVisionConfig, CrosshairConfig crosshairConfig, SubtitleUserSettings subtitleUserSettings, GamepadConfig gamepadConfig, string[] overrideKeymaps, string voiceChatInputAudioDevice, string voiceChatOutputAudioDevice, bool voiceChatTeamEnabled, string voiceChatConsoleMode, bool voiceChatPartyEnabled, bool voiceChatPartyEnabledInGames, bool voiceChatTeamPushToTalk, bool voiceChatPartyPushToTalk, string[] enabledTextStats, string[] enabledGraphStats, string[] mutedChatContexts, int inputBindingsVersion, Int64 version) : base(version)
    {
        PlayerId = playerId;
        UnlockAllPlayModes = unlockAllPlayModes;
        UnlockAllMenuTabs = unlockAllMenuTabs;
        UnlockAllSponsors = unlockAllSponsors;
        BypassUnlockAllSponsorsOverride = bypassUnlockAllSponsorsOverride;
        BypassProgressionOverrides = bypassProgressionOverrides;
        BypassTeamSizeOverrides = bypassTeamSizeOverrides;
        BypassRegionSelectOverride = bypassRegionSelectOverride;
        BypassCurrencyPurchasingOverride = bypassCurrencyPurchasingOverride;
        DisableDevMapSelector = disableDevMapSelector;
        ShowDebugInfoPanel = showDebugInfoPanel;
        ShowPlatformInfoPanel = showPlatformInfoPanel;
        ShowMatchmakingCounters = showMatchmakingCounters;
        ForceChatEnabled = forceChatEnabled;
        MostRecentLobbyMode = mostRecentLobbyMode;
        MostRecentPartyId = mostRecentPartyId;
        EndUserLicenseAcceptedVersion = endUserLicenseAcceptedVersion;
        EndUserLicenseAcceptedVersionPlayStation = endUserLicenseAcceptedVersionPlayStation;
        EndUserLicenseAcceptedVersionXbox = endUserLicenseAcceptedVersionXbox;
        TermsOfServiceAcceptedVersion = termsOfServiceAcceptedVersion;
        TermsOfServiceAcceptedVersionPlayStation = termsOfServiceAcceptedVersionPlayStation;
        TermsOfServiceAcceptedVersionXbox = termsOfServiceAcceptedVersionXbox;
        NonDisclosureAgreementAcceptedVersion = nonDisclosureAgreementAcceptedVersion;
        NonDisclosureAgreementAcceptedVersionPlayStation = nonDisclosureAgreementAcceptedVersionPlayStation;
        NonDisclosureAgreementAcceptedVersionXbox = nonDisclosureAgreementAcceptedVersionXbox;
        SeizureWarningAcknowledgedVersion = seizureWarningAcknowledgedVersion;
        SeizureWarningAcknowledgedVersionPlayStation = seizureWarningAcknowledgedVersionPlayStation;
        SeizureWarningAcknowledgedVersionXbox = seizureWarningAcknowledgedVersionXbox;
        BattlepassSeasonLoggedOn = battlepassSeasonLoggedOn;
        BattlepassPurchasePopupLastTime = battlepassPurchasePopupLastTime;
        NonDisclosureAgreementUserSignature = nonDisclosureAgreementUserSignature ?? throw new ArgumentNullException(nameof(nonDisclosureAgreementUserSignature));
        NonDisclosureAgreementUserSignaturePlayStation = nonDisclosureAgreementUserSignaturePlayStation ?? throw new ArgumentNullException(nameof(nonDisclosureAgreementUserSignaturePlayStation));
        NonDisclosureAgreementUserSignatureXbox = nonDisclosureAgreementUserSignatureXbox ?? throw new ArgumentNullException(nameof(nonDisclosureAgreementUserSignatureXbox));
        NonDisclosureAgreementUserEmail = nonDisclosureAgreementUserEmail ?? throw new ArgumentNullException(nameof(nonDisclosureAgreementUserEmail));
        NonDisclosureAgreementUserEmailPlayStation = nonDisclosureAgreementUserEmailPlayStation ?? throw new ArgumentNullException(nameof(nonDisclosureAgreementUserEmailPlayStation));
        NonDisclosureAgreementUserEmailXbox = nonDisclosureAgreementUserEmailXbox ?? throw new ArgumentNullException(nameof(nonDisclosureAgreementUserEmailXbox));
        LastVersionShownInDriversWarningDialog = lastVersionShownInDriversWarningDialog ?? throw new ArgumentNullException(nameof(lastVersionShownInDriversWarningDialog));
        MinSpecWarningDialogTimesDisplayed = minSpecWarningDialogTimesDisplayed;
        PingWarningDialogTimesDisplayed = pingWarningDialogTimesDisplayed;
        HasCompletedLaunchSettingsFlow = hasCompletedLaunchSettingsFlow;
        IsUsingManualMatchmakingRegionSelection = isUsingManualMatchmakingRegionSelection;
        ManualMatchmakingRegionSelections = manualMatchmakingRegionSelections ?? throw new ArgumentNullException(nameof(manualMatchmakingRegionSelections));
        RotatingNewsViewedMessages = rotatingNewsViewedMessages ?? throw new ArgumentNullException(nameof(rotatingNewsViewedMessages));
        InkQuality = inkQuality;
        MouseSensitivityADSScale = mouseSensitivityADSScale;
        MouseSensitivity = mouseSensitivity;
        MinimapScale = minimapScale;
        MinimapSize = minimapSize;
        MinimapMaskOpacity = minimapMaskOpacity;
        InvertedYAxis = invertedYAxis;
        ToggleCrouch = toggleCrouch;
        ToggleWalk = toggleWalk;
        ToggleADS = toggleADS;
        RecoilBehavior = recoilBehavior ?? throw new ArgumentNullException(nameof(recoilBehavior));
        LeftHandedEnabled = leftHandedEnabled;
        RecoilPitchCorrectionEnabled = recoilPitchCorrectionEnabled;
        IsTeamLaserEnabled = isTeamLaserEnabled;
        IsHudMinimapRotationEnabled = isHudMinimapRotationEnabled;
        IsHudMinimapCenteredOnPlayer = isHudMinimapCenteredOnPlayer;
        IsHudMinimapCircle = isHudMinimapCircle;
        IsHudMinimapMaskHighContrastEnabled = isHudMinimapMaskHighContrastEnabled;
        IsHudSnapMinimapWithScoreboardEnabled = isHudSnapMinimapWithScoreboardEnabled;
        IsDamageCameraEffectEnabled = isDamageCameraEffectEnabled;
        StreamerModeEnabled = streamerModeEnabled;
        HideLobbyCode = hideLobbyCode;
        ADSTracerRatio = aDSTracerRatio;
        ADSTracerIntensity = aDSTracerIntensity;
        OpticHitConfirmIntensity = opticHitConfirmIntensity;
        AnonymousMode = anonymousMode;
        AnonymizePlayerNames = anonymizePlayerNames;
        StreamerModeDisableIncomingVoiceChat = streamerModeDisableIncomingVoiceChat;
        StreamerModeDisableIncomingTextChat = streamerModeDisableIncomingTextChat;
        IsTextChatSoundEffectsEnabled = isTextChatSoundEffectsEnabled;
        SubtitlesEnabled = subtitlesEnabled;
        VerboseVoLevel = verboseVoLevel ?? throw new ArgumentNullException(nameof(verboseVoLevel));
        IsBloodFXEnabled = isBloodFXEnabled;
        ColorVisionConfig = colorVisionConfig ?? throw new ArgumentNullException(nameof(colorVisionConfig));
        CrosshairConfig = crosshairConfig ?? throw new ArgumentNullException(nameof(crosshairConfig));
        SubtitleUserSettings = subtitleUserSettings ?? throw new ArgumentNullException(nameof(subtitleUserSettings));
        GamepadConfig = gamepadConfig ?? throw new ArgumentNullException(nameof(gamepadConfig));
        OverrideKeymaps = overrideKeymaps ?? throw new ArgumentNullException(nameof(overrideKeymaps));
        VoiceChatInputAudioDevice = voiceChatInputAudioDevice ?? throw new ArgumentNullException(nameof(voiceChatInputAudioDevice));
        VoiceChatOutputAudioDevice = voiceChatOutputAudioDevice ?? throw new ArgumentNullException(nameof(voiceChatOutputAudioDevice));
        VoiceChatTeamEnabled = voiceChatTeamEnabled;
        VoiceChatConsoleMode = voiceChatConsoleMode ?? throw new ArgumentNullException(nameof(voiceChatConsoleMode));
        VoiceChatPartyEnabled = voiceChatPartyEnabled;
        VoiceChatPartyEnabledInGames = voiceChatPartyEnabledInGames;
        VoiceChatTeamPushToTalk = voiceChatTeamPushToTalk;
        VoiceChatPartyPushToTalk = voiceChatPartyPushToTalk;
        EnabledTextStats = enabledTextStats ?? throw new ArgumentNullException(nameof(enabledTextStats));
        EnabledGraphStats = enabledGraphStats ?? throw new ArgumentNullException(nameof(enabledGraphStats));
        MutedChatContexts = mutedChatContexts ?? throw new ArgumentNullException(nameof(mutedChatContexts));
        InputBindingsVersion = inputBindingsVersion;
    }

    public required Guid PlayerId { get; set; }
    public required bool UnlockAllPlayModes { get; set; }
    public required bool UnlockAllMenuTabs { get; set; }
    public required bool UnlockAllSponsors { get; set; }
    public required bool BypassUnlockAllSponsorsOverride { get; set; }
    public required bool BypassProgressionOverrides { get; set; }
    public required bool BypassTeamSizeOverrides { get; set; }
    public required bool BypassRegionSelectOverride { get; set; }
    public required bool BypassCurrencyPurchasingOverride { get; set; }
    public required bool DisableDevMapSelector { get; set; }
    public required bool ShowDebugInfoPanel { get; set; }
    public required bool ShowPlatformInfoPanel { get; set; }
    public required bool ShowMatchmakingCounters { get; set; }
    public required bool ForceChatEnabled { get; set; }
    public required Int32 MostRecentLobbyMode { get; set; } // TODO ENUMIFY
    public required Guid MostRecentPartyId { get; set; }
    public required Int32 EndUserLicenseAcceptedVersion { get; set; }
    public required Int32 EndUserLicenseAcceptedVersionPlayStation { get; set; }
    public required Int32 EndUserLicenseAcceptedVersionXbox { get; set; }
    public required Int32 TermsOfServiceAcceptedVersion { get; set; }
    public required Int32 TermsOfServiceAcceptedVersionPlayStation { get; set; }
    public required Int32 TermsOfServiceAcceptedVersionXbox { get; set; }
    public required Int32 NonDisclosureAgreementAcceptedVersion { get; set; }
    public required Int32 NonDisclosureAgreementAcceptedVersionPlayStation { get; set; }
    public required Int32 NonDisclosureAgreementAcceptedVersionXbox { get; set; }
    public required Int32 SeizureWarningAcknowledgedVersion { get; set; }
    public required Int32 SeizureWarningAcknowledgedVersionPlayStation { get; set; }
    public required Int32 SeizureWarningAcknowledgedVersionXbox { get; set; }
    public required Int32 BattlepassSeasonLoggedOn { get; set; }
    public required double BattlepassPurchasePopupLastTime { get; set; }
    public required string NonDisclosureAgreementUserSignature { get; set; }
    public required string NonDisclosureAgreementUserSignaturePlayStation { get; set; }
    public required string NonDisclosureAgreementUserSignatureXbox { get; set; }
    public required string NonDisclosureAgreementUserEmail { get; set; }
    public required string NonDisclosureAgreementUserEmailPlayStation { get; set; }
    public required string NonDisclosureAgreementUserEmailXbox { get; set; }
    public required string LastVersionShownInDriversWarningDialog { get; set; }
    public required Int32 MinSpecWarningDialogTimesDisplayed { get; set; }
    public required Int32 PingWarningDialogTimesDisplayed { get; set; }
    public required bool HasCompletedLaunchSettingsFlow { get; set; }
    public required bool IsUsingManualMatchmakingRegionSelection { get; set; }
    public required string[] ManualMatchmakingRegionSelections { get; set; }
    public required string[] RotatingNewsViewedMessages { get; set; }
    public required double InkQuality { get; set; }
    public required double MouseSensitivityADSScale { get; set; }
    public required double MouseSensitivity { get; set; }
    public required double MinimapScale { get; set; }
    public required double MinimapSize { get; set; }
    public required double MinimapMaskOpacity { get; set; }
    public required bool InvertedYAxis { get; set; }
    public required bool ToggleCrouch { get; set; }
    public required bool ToggleWalk { get; set; }
    public required bool ToggleADS { get; set; }
    public required string RecoilBehavior { get; set; } // todo probably enum
    public required bool LeftHandedEnabled { get; set; }
    public required bool RecoilPitchCorrectionEnabled { get; set; }
    public required bool IsTeamLaserEnabled { get; set; }
    public required bool IsHudMinimapRotationEnabled { get; set; }
    public required bool IsHudMinimapCenteredOnPlayer { get; set; }
    public required bool IsHudMinimapCircle { get; set; }
    public required bool IsHudMinimapMaskHighContrastEnabled { get; set; }
    public required bool IsHudSnapMinimapWithScoreboardEnabled { get; set; }
    public required bool IsDamageCameraEffectEnabled { get; set; }
    public required bool StreamerModeEnabled { get; set; }
    public required bool HideLobbyCode { get; set; }
    public required double ADSTracerRatio { get; set; }
    public required double ADSTracerIntensity { get; set; }
    public required double OpticHitConfirmIntensity { get; set; }
    public required bool AnonymousMode { get; set; }
    public required bool AnonymizePlayerNames { get; set; }
    public required bool StreamerModeDisableIncomingVoiceChat { get; set; }
    public required bool StreamerModeDisableIncomingTextChat { get; set; }
    public required bool IsTextChatSoundEffectsEnabled { get; set; }
    public required bool SubtitlesEnabled { get; set; }
    public required string VerboseVoLevel { get; set; }
    public required bool IsBloodFXEnabled { get; set; }
    public required ColorVisionConfig ColorVisionConfig { get; set; }
    public required CrosshairConfig CrosshairConfig { get; set; }
    public required SubtitleUserSettings SubtitleUserSettings { get; set; }
    public required GamepadConfig GamepadConfig { get; set; }
    public required string[] OverrideKeymaps { get; set; }
    public required string VoiceChatInputAudioDevice { get; set; }
    public required string VoiceChatOutputAudioDevice { get; set; }
    public required bool VoiceChatTeamEnabled { get; set; }
    public required string VoiceChatConsoleMode { get; set; } // prob enum
    public required bool VoiceChatPartyEnabled { get; set; }
    public required bool VoiceChatPartyEnabledInGames { get; set; }
    public required bool VoiceChatTeamPushToTalk { get; set; }
    public required bool VoiceChatPartyPushToTalk { get; set; }
    public required string[] EnabledTextStats { get; set; }
    public required string[] EnabledGraphStats { get; set; }
    public required string[] MutedChatContexts { get; set; }
    public required Int32 InputBindingsVersion { get; set; }

    public static Task<PlayerConfig?> RetrieveFromDatabase(Guid key)
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