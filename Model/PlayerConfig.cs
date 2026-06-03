namespace Model;

public record class PlayerConfig : VersionedData, IDatabaseSyncable<PlayerConfig>
{
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

    public static Task<PlayerConfig?> RetrieveFromDatabase(string key)
    {
        throw new NotImplementedException();
    }

    public object GetKey()
    {
        throw new NotImplementedException();
    }

    public Task SyncToDatabase()
    {
        throw new NotImplementedException();
    }
}