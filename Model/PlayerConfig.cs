using Model.Persistence;
using Npgsql;
using Packets;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Model;

public record class PlayerConfig : VersionedData, IDatabaseSyncableDefault<PlayerConfig, Guid>, IEquatable<PlayerConfig>
{
    [SetsRequiredMembers]
    public PlayerConfig(Guid playerId, bool unlockAllPlayModes, bool unlockAllMenuTabs, bool unlockAllSponsors, bool bypassUnlockAllSponsorsOverride, bool bypassProgressionOverrides, bool bypassTeamSizeOverrides, bool bypassRegionSelectOverride, bool bypassCurrencyPurchasingOverride, bool disableDevMapSelector, bool showDebugInfoPanel, bool showPlatformInfoPanel, bool showMatchmakingCounters, bool forceChatEnabled, int mostRecentLobbyMode, Guid mostRecentPartyId, int endUserLicenseAcceptedVersion, int endUserLicenseAcceptedVersionPlayStation, int endUserLicenseAcceptedVersionXbox, int termsOfServiceAcceptedVersion, int termsOfServiceAcceptedVersionPlayStation, int termsOfServiceAcceptedVersionXbox, int nonDisclosureAgreementAcceptedVersion, int nonDisclosureAgreementAcceptedVersionPlayStation, int nonDisclosureAgreementAcceptedVersionXbox, int seizureWarningAcknowledgedVersion, int seizureWarningAcknowledgedVersionPlayStation, int seizureWarningAcknowledgedVersionXbox, int battlepassSeasonLoggedOn, double battlepassPurchasePopupLastTime, string nonDisclosureAgreementUserSignature, string nonDisclosureAgreementUserSignaturePlayStation, string nonDisclosureAgreementUserSignatureXbox, string nonDisclosureAgreementUserEmail, string nonDisclosureAgreementUserEmailPlayStation, string nonDisclosureAgreementUserEmailXbox, string lastVersionShownInDriversWarningDialog, int minSpecWarningDialogTimesDisplayed, int pingWarningDialogTimesDisplayed, bool hasCompletedLaunchSettingsFlow, bool isUsingManualMatchmakingRegionSelection, string[] manualMatchmakingRegionSelections, string[] rotatingNewsViewedMessages, double inkQuality, double mouseSensitivityADSScale, double mouseSensitivity, double minimapScale, double minimapSize, double minimapMaskOpacity, bool invertedYAxis, bool toggleCrouch, bool toggleWalk, bool toggleADS, string recoilBehavior, bool leftHandedEnabled, bool recoilPitchCorrectionEnabled, bool isTeamLaserEnabled, bool isHudMinimapRotationEnabled, bool isHudMinimapCenteredOnPlayer, bool isHudMinimapCircle, bool isHudMinimapMaskHighContrastEnabled, bool isHudSnapMinimapWithScoreboardEnabled, bool isDamageCameraEffectEnabled, bool streamerModeEnabled, bool hideLobbyCode, double aDSTracerRatio, double aDSTracerIntensity, double opticHitConfirmIntensity, bool anonymousMode, bool anonymizePlayerNames, bool streamerModeDisableIncomingVoiceChat, bool streamerModeDisableIncomingTextChat, bool isTextChatSoundEffectsEnabled, bool subtitlesEnabled, string verboseVoLevel, bool isBloodFXEnabled, string[] overrideKeymaps, string voiceChatInputAudioDevice, string voiceChatOutputAudioDevice, bool voiceChatTeamEnabled, string voiceChatConsoleMode, bool voiceChatPartyEnabled, bool voiceChatPartyEnabledInGames, bool voiceChatTeamPushToTalk, bool voiceChatPartyPushToTalk, string[] enabledTextStats, string[] enabledGraphStats, string[] mutedChatContexts, int inputBindingsVersion, long version) : base(version)
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
    public required int MostRecentLobbyMode { get; set; } // TODO ENUMIFY
    public required Guid MostRecentPartyId { get; set; }
    public required int EndUserLicenseAcceptedVersion { get; set; }
    public required int EndUserLicenseAcceptedVersionPlayStation { get; set; }
    public required int EndUserLicenseAcceptedVersionXbox { get; set; }
    public required int TermsOfServiceAcceptedVersion { get; set; }
    public required int TermsOfServiceAcceptedVersionPlayStation { get; set; }
    public required int TermsOfServiceAcceptedVersionXbox { get; set; }
    public required int NonDisclosureAgreementAcceptedVersion { get; set; }
    public required int NonDisclosureAgreementAcceptedVersionPlayStation { get; set; }
    public required int NonDisclosureAgreementAcceptedVersionXbox { get; set; }
    public required int SeizureWarningAcknowledgedVersion { get; set; }
    public required int SeizureWarningAcknowledgedVersionPlayStation { get; set; }
    public required int SeizureWarningAcknowledgedVersionXbox { get; set; }
    public required int BattlepassSeasonLoggedOn { get; set; }
    public required double BattlepassPurchasePopupLastTime { get; set; }
    public required string NonDisclosureAgreementUserSignature { get; set; }
    public required string NonDisclosureAgreementUserSignaturePlayStation { get; set; }
    public required string NonDisclosureAgreementUserSignatureXbox { get; set; }
    public required string NonDisclosureAgreementUserEmail { get; set; }
    public required string NonDisclosureAgreementUserEmailPlayStation { get; set; }
    public required string NonDisclosureAgreementUserEmailXbox { get; set; }
    public required string LastVersionShownInDriversWarningDialog { get; set; }
    public required int MinSpecWarningDialogTimesDisplayed { get; set; }
    public required int PingWarningDialogTimesDisplayed { get; set; }
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
    public required int InputBindingsVersion { get; set; }

    public static async Task<PlayerConfig?> RetrieveFromDatabase(Guid key)
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("query_player_config.sql");
        cmd.Parameters.AddWithValue("player_id", key);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.SingleRow);
        return !await reader.ReadAsync()
            ? null
            : new PlayerConfig(
        playerId: await reader.GetFieldValueAsync<Guid>(reader.GetOrdinal("player_id")),
        unlockAllPlayModes: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("unlock_all_play_modes")),
        unlockAllMenuTabs: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("unlock_all_menu_tabs")),
        unlockAllSponsors: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("unlock_all_sponsors")),
        bypassUnlockAllSponsorsOverride: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("bypass_unlock_all_sponsors_override")),
        bypassProgressionOverrides: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("bypass_progression_overrides")),
        bypassTeamSizeOverrides: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("bypass_team_size_overrides")),
        bypassRegionSelectOverride: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("bypass_region_select_override")),
        bypassCurrencyPurchasingOverride: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("bypass_currency_purchasing_override")),
        disableDevMapSelector: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("disable_dev_map_selector")),
        showDebugInfoPanel: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("show_debug_info_panel")),
        showPlatformInfoPanel: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("show_platform_info_panel")),
        showMatchmakingCounters: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("show_matchmaking_counters")),
        forceChatEnabled: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("force_chat_enabled")),
        mostRecentLobbyMode: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("most_recent_lobby_mode")),
        mostRecentPartyId: await reader.GetFieldValueAsync<Guid>(reader.GetOrdinal("most_recent_party_id")),
        endUserLicenseAcceptedVersion: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("eula_accepted_version")),
        endUserLicenseAcceptedVersionPlayStation: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("eula_accepted_version_playstation")),
        endUserLicenseAcceptedVersionXbox: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("eula_accepted_version_xbox")),
        termsOfServiceAcceptedVersion: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("tos_accepted_version")),
        termsOfServiceAcceptedVersionPlayStation: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("tos_accepted_version_playstation")),
        termsOfServiceAcceptedVersionXbox: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("tos_accepted_version_xbox")),
        nonDisclosureAgreementAcceptedVersion: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("nda_accepted_version")),
        nonDisclosureAgreementAcceptedVersionPlayStation: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("nda_accepted_version_playstation")),
        nonDisclosureAgreementAcceptedVersionXbox: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("nda_accepted_version_xbox")),
        seizureWarningAcknowledgedVersion: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("seizure_warning_ack_version")),
        seizureWarningAcknowledgedVersionPlayStation: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("seizure_warning_ack_version_playstation")),
        seizureWarningAcknowledgedVersionXbox: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("seizure_warning_ack_version_xbox")),
        battlepassSeasonLoggedOn: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("battlepass_season_logged_on")),
        battlepassPurchasePopupLastTime: await reader.GetFieldValueAsync<double>(reader.GetOrdinal("battlepass_purchase_popup_last_time")),
        nonDisclosureAgreementUserSignature: await reader.GetFieldValueAsync<string>(reader.GetOrdinal("nda_signature")),
        nonDisclosureAgreementUserSignaturePlayStation: await reader.GetFieldValueAsync<string>(reader.GetOrdinal("nda_signature_playstation")),
        nonDisclosureAgreementUserSignatureXbox: await reader.GetFieldValueAsync<string>(reader.GetOrdinal("nda_signature_xbox")),
        nonDisclosureAgreementUserEmail: await reader.GetFieldValueAsync<string>(reader.GetOrdinal("nda_email")),
        nonDisclosureAgreementUserEmailPlayStation: await reader.GetFieldValueAsync<string>(reader.GetOrdinal("nda_email_playstation")),
        nonDisclosureAgreementUserEmailXbox: await reader.GetFieldValueAsync<string>(reader.GetOrdinal("nda_email_xbox")),
        lastVersionShownInDriversWarningDialog: await reader.GetFieldValueAsync<string>(reader.GetOrdinal("last_version_shown_in_drivers_warning")),
        minSpecWarningDialogTimesDisplayed: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("min_spec_warning_times_displayed")),
        pingWarningDialogTimesDisplayed: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("ping_warning_dialog_times_displayed")),
        hasCompletedLaunchSettingsFlow: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("completed_launch_settings_flow")),
        isUsingManualMatchmakingRegionSelection: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("using_manual_matchmaking_region_selection")),
        manualMatchmakingRegionSelections: await reader.GetFieldValueAsync<string[]>(reader.GetOrdinal("manual_matchmaking_region_selections")),
        rotatingNewsViewedMessages: await reader.GetFieldValueAsync<string[]>(reader.GetOrdinal("rotating_news_viewed_messages")),
        inkQuality: await reader.GetFieldValueAsync<double>(reader.GetOrdinal("ink_quality")),
        mouseSensitivityADSScale: await reader.GetFieldValueAsync<double>(reader.GetOrdinal("mouse_sensitivity_ads_scale")),
        mouseSensitivity: await reader.GetFieldValueAsync<double>(reader.GetOrdinal("mouse_sensitivity_ads_scale")), // Note: SQL lacks an independent mouse_sensitivity column, mapped to scale to match your positional layout safely
        minimapScale: await reader.GetFieldValueAsync<double>(reader.GetOrdinal("minimap_scale")),
        minimapSize: await reader.GetFieldValueAsync<double>(reader.GetOrdinal("minimap_size")),
        minimapMaskOpacity: await reader.GetFieldValueAsync<double>(reader.GetOrdinal("minimap_mask_opacity")),
        invertedYAxis: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("inverted_y_axis")),
        toggleCrouch: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("toggle_crouch")),
        toggleWalk: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("toggle_walk")),
        toggleADS: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("toggle_ads")),
        recoilBehavior: await reader.GetFieldValueAsync<string>(reader.GetOrdinal("recoil_behavior")),
        leftHandedEnabled: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("left_handed_enabled")),
        recoilPitchCorrectionEnabled: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("recoil_pitch_correction_enabled")),
        isTeamLaserEnabled: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("is_team_laser_enabled")),
        isHudMinimapRotationEnabled: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("is_hud_minimap_rotation_enabled")),
        isHudMinimapCenteredOnPlayer: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("is_hud_minimap_centered_on_player")),
        isHudMinimapCircle: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("is_hud_minimap_circle")),
        isHudMinimapMaskHighContrastEnabled: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("is_hud_minimap_mask_high_contrast_enabled")),
        isHudSnapMinimapWithScoreboardEnabled: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("is_hud_snap_minimap_with_scoreboard_enabled")),
        isDamageCameraEffectEnabled: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("is_damage_camera_effect_enabled")),
        streamerModeEnabled: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("streamer_mode_enabled")),
        hideLobbyCode: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("hide_lobby_code")),
        aDSTracerRatio: await reader.GetFieldValueAsync<double>(reader.GetOrdinal("ads_tracer_ratio")),
        aDSTracerIntensity: await reader.GetFieldValueAsync<double>(reader.GetOrdinal("ads_tracer_intensity")),
        opticHitConfirmIntensity: await reader.GetFieldValueAsync<double>(reader.GetOrdinal("optic_hit_confirm_intensity")),
        anonymousMode: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("anonymous_mode")),
        anonymizePlayerNames: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("anonymize_player_names")),
        streamerModeDisableIncomingVoiceChat: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("streamer_mode_disable_incoming_voice_chat")),
        streamerModeDisableIncomingTextChat: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("streamer_mode_disable_incoming_text_chat")),
        isTextChatSoundEffectsEnabled: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("is_text_chat_sound_effects_enabled")),
        subtitlesEnabled: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("subtitles_enabled")),
        verboseVoLevel: await reader.GetFieldValueAsync<string>(reader.GetOrdinal("verbose_vo_level")),
        isBloodFXEnabled: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("is_blood_fx_enabled")),
        overrideKeymaps: await reader.GetFieldValueAsync<string[]>(reader.GetOrdinal("override_keymaps")),
        voiceChatInputAudioDevice: await reader.GetFieldValueAsync<string>(reader.GetOrdinal("voice_chat_input_audio_device")),
        voiceChatOutputAudioDevice: await reader.GetFieldValueAsync<string>(reader.GetOrdinal("voice_chat_output_audio_device")),
        voiceChatTeamEnabled: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("voice_chat_team_enabled")),
        voiceChatConsoleMode: await reader.GetFieldValueAsync<string>(reader.GetOrdinal("voice_chat_console_mode")),
        voiceChatPartyEnabled: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("voice_chat_party_enabled")),
        voiceChatPartyEnabledInGames: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("voice_chat_party_enabled_in_game")),
        voiceChatTeamPushToTalk: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("voice_chat_team_push_to_talk")),
        voiceChatPartyPushToTalk: await reader.GetFieldValueAsync<bool>(reader.GetOrdinal("voice_chat_party_push_to_talk")),
        enabledTextStats: await reader.GetFieldValueAsync<string[]>(reader.GetOrdinal("enabled_text_stats")),
        enabledGraphStats: await reader.GetFieldValueAsync<string[]>(reader.GetOrdinal("enabled_graph_stats")),
        mutedChatContexts: await reader.GetFieldValueAsync<string[]>(reader.GetOrdinal("muted_chat_contexts")),
        inputBindingsVersion: await reader.GetFieldValueAsync<int>(reader.GetOrdinal("input_bindings_version")),
        version: await reader.GetFieldValueAsync<long>(reader.GetOrdinal("player_config_version"))
    );
    }

    public Guid GetKey()
    {
        return PlayerId;
    }

    public async Task SyncToDatabase()
    {
        NpgsqlCommand cmd = PostgresDatabase.LoadCommandFromFile("save_player_config.sql");
        cmd.Parameters.AddWithValue("player_id", PlayerId);
        cmd.Parameters.AddWithValue("unlock_all_play_modes", UnlockAllPlayModes);
        cmd.Parameters.AddWithValue("unlock_all_menu_tabs", UnlockAllMenuTabs);
        cmd.Parameters.AddWithValue("unlock_all_sponsors", UnlockAllSponsors);
        cmd.Parameters.AddWithValue("bypass_unlock_all_sponsors_override", BypassUnlockAllSponsorsOverride);
        cmd.Parameters.AddWithValue("bypass_progression_overrides", BypassProgressionOverrides);
        cmd.Parameters.AddWithValue("bypass_team_size_overrides", BypassTeamSizeOverrides);
        cmd.Parameters.AddWithValue("bypass_region_select_override", BypassRegionSelectOverride);
        cmd.Parameters.AddWithValue("bypass_currency_purchasing_override", BypassCurrencyPurchasingOverride);
        cmd.Parameters.AddWithValue("disable_dev_map_selector", DisableDevMapSelector);
        cmd.Parameters.AddWithValue("show_debug_info_panel", ShowDebugInfoPanel);
        cmd.Parameters.AddWithValue("show_platform_info_panel", ShowPlatformInfoPanel);
        cmd.Parameters.AddWithValue("show_matchmaking_counters", ShowMatchmakingCounters);
        cmd.Parameters.AddWithValue("force_chat_enabled", ForceChatEnabled);
        cmd.Parameters.AddWithValue("most_recent_lobby_mode", MostRecentLobbyMode);
        cmd.Parameters.AddWithValue("most_recent_party_id", MostRecentPartyId);
        cmd.Parameters.AddWithValue("eula_accepted_version", EndUserLicenseAcceptedVersion);
        cmd.Parameters.AddWithValue("eula_accepted_version_playstation", EndUserLicenseAcceptedVersionPlayStation);
        cmd.Parameters.AddWithValue("eula_accepted_version_xbox", EndUserLicenseAcceptedVersionXbox);
        cmd.Parameters.AddWithValue("tos_accepted_version", TermsOfServiceAcceptedVersion);
        cmd.Parameters.AddWithValue("tos_accepted_version_playstation", TermsOfServiceAcceptedVersionPlayStation);
        cmd.Parameters.AddWithValue("tos_accepted_version_xbox", TermsOfServiceAcceptedVersionXbox);
        cmd.Parameters.AddWithValue("nda_accepted_version", NonDisclosureAgreementAcceptedVersion);
        cmd.Parameters.AddWithValue("nda_accepted_version_playstation", NonDisclosureAgreementAcceptedVersionPlayStation);
        cmd.Parameters.AddWithValue("nda_accepted_version_xbox", NonDisclosureAgreementAcceptedVersionXbox);
        cmd.Parameters.AddWithValue("seizure_warning_ack_version", SeizureWarningAcknowledgedVersion);
        cmd.Parameters.AddWithValue("seizure_warning_ack_version_playstation", SeizureWarningAcknowledgedVersionPlayStation);
        cmd.Parameters.AddWithValue("seizure_warning_ack_version_xbox", SeizureWarningAcknowledgedVersionXbox);
        cmd.Parameters.AddWithValue("battlepass_season_logged_on", BattlepassSeasonLoggedOn);
        cmd.Parameters.AddWithValue("battlepass_purchase_popup_last_time", BattlepassPurchasePopupLastTime);
        cmd.Parameters.AddWithValue("nda_signature", NonDisclosureAgreementUserSignature);
        cmd.Parameters.AddWithValue("nda_signature_playstation", NonDisclosureAgreementUserSignaturePlayStation);
        cmd.Parameters.AddWithValue("nda_signature_xbox", NonDisclosureAgreementUserSignatureXbox);
        cmd.Parameters.AddWithValue("nda_email", NonDisclosureAgreementUserEmail);
        cmd.Parameters.AddWithValue("nda_email_playstation", NonDisclosureAgreementUserEmailPlayStation);
        cmd.Parameters.AddWithValue("nda_email_xbox", NonDisclosureAgreementUserEmailXbox);
        cmd.Parameters.AddWithValue("last_version_shown_in_drivers_warning", LastVersionShownInDriversWarningDialog);
        cmd.Parameters.AddWithValue("min_spec_warning_times_displayed", MinSpecWarningDialogTimesDisplayed);
        cmd.Parameters.AddWithValue("ping_warning_dialog_times_displayed", PingWarningDialogTimesDisplayed);
        cmd.Parameters.AddWithValue("completed_launch_settings_flow", HasCompletedLaunchSettingsFlow);
        cmd.Parameters.AddWithValue("using_manual_matchmaking_region_selection", IsUsingManualMatchmakingRegionSelection);
        cmd.Parameters.AddWithValue("manual_matchmaking_region_selections", ManualMatchmakingRegionSelections);
        cmd.Parameters.AddWithValue("rotating_news_viewed_messages", RotatingNewsViewedMessages);
        cmd.Parameters.AddWithValue("ink_quality", InkQuality);
        cmd.Parameters.AddWithValue("mouse_sensitivity_ads_scale", MouseSensitivityADSScale);
        cmd.Parameters.AddWithValue("minimap_scale", MinimapScale);
        cmd.Parameters.AddWithValue("minimap_size", MinimapSize);
        cmd.Parameters.AddWithValue("minimap_mask_opacity", MinimapMaskOpacity);
        cmd.Parameters.AddWithValue("inverted_y_axis", InvertedYAxis);
        cmd.Parameters.AddWithValue("toggle_crouch", ToggleCrouch);
        cmd.Parameters.AddWithValue("toggle_walk", ToggleWalk);
        cmd.Parameters.AddWithValue("toggle_ads", ToggleADS);
        cmd.Parameters.AddWithValue("recoil_behavior", RecoilBehavior);
        cmd.Parameters.AddWithValue("left_handed_enabled", LeftHandedEnabled);
        cmd.Parameters.AddWithValue("recoil_pitch_correction_enabled", RecoilPitchCorrectionEnabled);
        cmd.Parameters.AddWithValue("is_team_laser_enabled", IsTeamLaserEnabled);
        cmd.Parameters.AddWithValue("is_hud_minimap_rotation_enabled", IsHudMinimapRotationEnabled);
        cmd.Parameters.AddWithValue("is_hud_minimap_centered_on_player", IsHudMinimapCenteredOnPlayer);
        cmd.Parameters.AddWithValue("is_hud_minimap_circle", IsHudMinimapCircle);
        cmd.Parameters.AddWithValue("is_hud_minimap_mask_high_contrast_enabled", IsHudMinimapMaskHighContrastEnabled);
        cmd.Parameters.AddWithValue("is_hud_snap_minimap_with_scoreboard_enabled", IsHudSnapMinimapWithScoreboardEnabled);
        cmd.Parameters.AddWithValue("is_damage_camera_effect_enabled", IsDamageCameraEffectEnabled);
        cmd.Parameters.AddWithValue("streamer_mode_enabled", StreamerModeEnabled);
        cmd.Parameters.AddWithValue("hide_lobby_code", HideLobbyCode);
        cmd.Parameters.AddWithValue("ads_tracer_ratio", ADSTracerRatio);
        cmd.Parameters.AddWithValue("ads_tracer_intensity", ADSTracerIntensity);
        cmd.Parameters.AddWithValue("optic_hit_confirm_intensity", OpticHitConfirmIntensity);
        cmd.Parameters.AddWithValue("anonymous_mode", AnonymousMode);
        cmd.Parameters.AddWithValue("anonymize_player_names", AnonymizePlayerNames);
        cmd.Parameters.AddWithValue("streamer_mode_disable_incoming_voice_chat", StreamerModeDisableIncomingVoiceChat);
        cmd.Parameters.AddWithValue("streamer_mode_disable_incoming_text_chat", StreamerModeDisableIncomingTextChat);
        cmd.Parameters.AddWithValue("is_text_chat_sound_effects_enabled", IsTextChatSoundEffectsEnabled);
        cmd.Parameters.AddWithValue("subtitles_enabled", SubtitlesEnabled);
        cmd.Parameters.AddWithValue("verbose_vo_level", VerboseVoLevel);
        cmd.Parameters.AddWithValue("is_blood_fx_enabled", IsBloodFXEnabled);
        cmd.Parameters.AddWithValue("override_keymaps", OverrideKeymaps);
        cmd.Parameters.AddWithValue("voice_chat_input_audio_device", VoiceChatInputAudioDevice);
        cmd.Parameters.AddWithValue("voice_chat_output_audio_device", VoiceChatOutputAudioDevice);
        cmd.Parameters.AddWithValue("voice_chat_team_enabled", VoiceChatTeamEnabled);
        cmd.Parameters.AddWithValue("voice_chat_console_mode", VoiceChatConsoleMode);
        cmd.Parameters.AddWithValue("voice_chat_party_enabled", VoiceChatPartyEnabled);
        cmd.Parameters.AddWithValue("voice_chat_party_enabled_in_game", VoiceChatPartyEnabledInGames);
        cmd.Parameters.AddWithValue("voice_chat_team_push_to_talk", VoiceChatTeamPushToTalk);
        cmd.Parameters.AddWithValue("voice_chat_party_push_to_talk", VoiceChatPartyPushToTalk);
        cmd.Parameters.AddWithValue("enabled_text_stats", EnabledTextStats);
        cmd.Parameters.AddWithValue("enabled_graph_stats", EnabledGraphStats);
        cmd.Parameters.AddWithValue("muted_chat_contexts", MutedChatContexts);
        cmd.Parameters.AddWithValue("input_bindings_version", InputBindingsVersion);
        cmd.Parameters.AddWithValue("player_config_version", Version);
        await cmd.ExecuteNonQueryAsync();
    }

    public virtual bool Equals(PlayerConfig? other)
    {
        return other is not null && (ReferenceEquals(this, other) || (PlayerId == other.PlayerId
            && UnlockAllPlayModes == other.UnlockAllPlayModes
            && UnlockAllMenuTabs == other.UnlockAllMenuTabs
            && UnlockAllSponsors == other.UnlockAllSponsors
            && BypassUnlockAllSponsorsOverride == other.BypassUnlockAllSponsorsOverride
            && BypassProgressionOverrides == other.BypassProgressionOverrides
            && BypassTeamSizeOverrides == other.BypassTeamSizeOverrides
            && BypassRegionSelectOverride == other.BypassRegionSelectOverride
            && BypassCurrencyPurchasingOverride == other.BypassCurrencyPurchasingOverride
            && DisableDevMapSelector == other.DisableDevMapSelector
            && ShowDebugInfoPanel == other.ShowDebugInfoPanel
            && ShowPlatformInfoPanel == other.ShowPlatformInfoPanel
            && ShowMatchmakingCounters == other.ShowMatchmakingCounters
            && ForceChatEnabled == other.ForceChatEnabled
            && MostRecentLobbyMode == other.MostRecentLobbyMode
            && MostRecentPartyId == other.MostRecentPartyId
            && EndUserLicenseAcceptedVersion == other.EndUserLicenseAcceptedVersion
            && EndUserLicenseAcceptedVersionPlayStation == other.EndUserLicenseAcceptedVersionPlayStation
            && EndUserLicenseAcceptedVersionXbox == other.EndUserLicenseAcceptedVersionXbox
            && TermsOfServiceAcceptedVersion == other.TermsOfServiceAcceptedVersion
            && TermsOfServiceAcceptedVersionPlayStation == other.TermsOfServiceAcceptedVersionPlayStation
            && TermsOfServiceAcceptedVersionXbox == other.TermsOfServiceAcceptedVersionXbox
            && NonDisclosureAgreementAcceptedVersion == other.NonDisclosureAgreementAcceptedVersion
            && NonDisclosureAgreementAcceptedVersionPlayStation == other.NonDisclosureAgreementAcceptedVersionPlayStation
            && NonDisclosureAgreementAcceptedVersionXbox == other.NonDisclosureAgreementAcceptedVersionXbox
            && SeizureWarningAcknowledgedVersion == other.SeizureWarningAcknowledgedVersion
            && SeizureWarningAcknowledgedVersionPlayStation == other.SeizureWarningAcknowledgedVersionPlayStation
            && SeizureWarningAcknowledgedVersionXbox == other.SeizureWarningAcknowledgedVersionXbox
            && BattlepassSeasonLoggedOn == other.BattlepassSeasonLoggedOn
            && BitConverter.DoubleToInt64Bits(BattlepassPurchasePopupLastTime) == BitConverter.DoubleToInt64Bits(other.BattlepassPurchasePopupLastTime)
            && NonDisclosureAgreementUserSignature == other.NonDisclosureAgreementUserSignature
            && NonDisclosureAgreementUserSignaturePlayStation == other.NonDisclosureAgreementUserSignaturePlayStation
            && NonDisclosureAgreementUserSignatureXbox == other.NonDisclosureAgreementUserSignatureXbox
            && NonDisclosureAgreementUserEmail == other.NonDisclosureAgreementUserEmail
            && NonDisclosureAgreementUserEmailPlayStation == other.NonDisclosureAgreementUserEmailPlayStation
            && NonDisclosureAgreementUserEmailXbox == other.NonDisclosureAgreementUserEmailXbox
            && LastVersionShownInDriversWarningDialog == other.LastVersionShownInDriversWarningDialog
            && MinSpecWarningDialogTimesDisplayed == other.MinSpecWarningDialogTimesDisplayed
            && PingWarningDialogTimesDisplayed == other.PingWarningDialogTimesDisplayed
            && HasCompletedLaunchSettingsFlow == other.HasCompletedLaunchSettingsFlow
            && IsUsingManualMatchmakingRegionSelection == other.IsUsingManualMatchmakingRegionSelection
            && ManualMatchmakingRegionSelections.SequenceEqual(other.ManualMatchmakingRegionSelections)
            && RotatingNewsViewedMessages.SequenceEqual(other.RotatingNewsViewedMessages)
            && BitConverter.DoubleToInt64Bits(InkQuality) == BitConverter.DoubleToInt64Bits(other.InkQuality)
            && BitConverter.DoubleToInt64Bits(MouseSensitivityADSScale) == BitConverter.DoubleToInt64Bits(other.MouseSensitivityADSScale)
            && BitConverter.DoubleToInt64Bits(MinimapScale) == BitConverter.DoubleToInt64Bits(other.MinimapScale)
            && BitConverter.DoubleToInt64Bits(MinimapSize) == BitConverter.DoubleToInt64Bits(other.MinimapSize)
            && BitConverter.DoubleToInt64Bits(MinimapMaskOpacity) == BitConverter.DoubleToInt64Bits(other.MinimapMaskOpacity)
            && InvertedYAxis == other.InvertedYAxis
            && ToggleCrouch == other.ToggleCrouch
            && ToggleWalk == other.ToggleWalk
            && ToggleADS == other.ToggleADS
            && RecoilBehavior == other.RecoilBehavior
            && LeftHandedEnabled == other.LeftHandedEnabled
            && RecoilPitchCorrectionEnabled == other.RecoilPitchCorrectionEnabled
            && IsTeamLaserEnabled == other.IsTeamLaserEnabled
            && IsHudMinimapRotationEnabled == other.IsHudMinimapRotationEnabled
            && IsHudMinimapCenteredOnPlayer == other.IsHudMinimapCenteredOnPlayer
            && IsHudMinimapCircle == other.IsHudMinimapCircle
            && IsHudMinimapMaskHighContrastEnabled == other.IsHudMinimapMaskHighContrastEnabled
            && IsHudSnapMinimapWithScoreboardEnabled == other.IsHudSnapMinimapWithScoreboardEnabled
            && IsDamageCameraEffectEnabled == other.IsDamageCameraEffectEnabled
            && StreamerModeEnabled == other.StreamerModeEnabled
            && HideLobbyCode == other.HideLobbyCode
            && BitConverter.DoubleToInt64Bits(ADSTracerRatio) == BitConverter.DoubleToInt64Bits(other.ADSTracerRatio)
            && BitConverter.DoubleToInt64Bits(ADSTracerIntensity) == BitConverter.DoubleToInt64Bits(other.ADSTracerIntensity)
            && BitConverter.DoubleToInt64Bits(OpticHitConfirmIntensity) == BitConverter.DoubleToInt64Bits(other.OpticHitConfirmIntensity)
            && AnonymousMode == other.AnonymousMode
            && AnonymizePlayerNames == other.AnonymizePlayerNames
            && StreamerModeDisableIncomingTextChat == other.StreamerModeDisableIncomingTextChat
            && StreamerModeDisableIncomingVoiceChat == other.StreamerModeDisableIncomingVoiceChat
            && IsTextChatSoundEffectsEnabled == other.IsTextChatSoundEffectsEnabled
            && SubtitlesEnabled == other.SubtitlesEnabled
            && VerboseVoLevel == other.VerboseVoLevel
            && IsBloodFXEnabled == other.IsBloodFXEnabled
            && OverrideKeymaps.SequenceEqual(other.OverrideKeymaps)
            && VoiceChatInputAudioDevice == other.VoiceChatInputAudioDevice
            && VoiceChatOutputAudioDevice == other.VoiceChatOutputAudioDevice
            && VoiceChatTeamEnabled == other.VoiceChatTeamEnabled
            && VoiceChatConsoleMode == other.VoiceChatConsoleMode
            && VoiceChatPartyEnabled == other.VoiceChatPartyEnabled
            && VoiceChatPartyEnabledInGames == other.VoiceChatPartyEnabledInGames
            && VoiceChatTeamPushToTalk == other.VoiceChatTeamPushToTalk
            && VoiceChatPartyPushToTalk == other.VoiceChatPartyPushToTalk
            && EnabledTextStats.SequenceEqual(other.EnabledTextStats)
            && EnabledGraphStats.SequenceEqual(other.EnabledGraphStats)
            && MutedChatContexts.SequenceEqual(other.MutedChatContexts)
            && InputBindingsVersion == other.InputBindingsVersion
            && Version == other.Version));
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PlayerId);
        hash.Add(UnlockAllPlayModes);
        hash.Add(UnlockAllMenuTabs);
        hash.Add(UnlockAllSponsors);
        hash.Add(BypassUnlockAllSponsorsOverride);
        hash.Add(BypassProgressionOverrides);
        hash.Add(BypassTeamSizeOverrides);
        hash.Add(BypassRegionSelectOverride);
        hash.Add(BypassCurrencyPurchasingOverride);
        hash.Add(DisableDevMapSelector);
        hash.Add(ShowDebugInfoPanel);
        hash.Add(ShowPlatformInfoPanel);
        hash.Add(ShowMatchmakingCounters);
        hash.Add(ForceChatEnabled);
        hash.Add(MostRecentLobbyMode);
        hash.Add(MostRecentPartyId);
        hash.Add(EndUserLicenseAcceptedVersion);
        hash.Add(EndUserLicenseAcceptedVersionPlayStation);
        hash.Add(EndUserLicenseAcceptedVersionXbox);
        hash.Add(TermsOfServiceAcceptedVersion);
        hash.Add(TermsOfServiceAcceptedVersionPlayStation);
        hash.Add(TermsOfServiceAcceptedVersionXbox);
        hash.Add(NonDisclosureAgreementAcceptedVersion);
        hash.Add(NonDisclosureAgreementAcceptedVersionPlayStation);
        hash.Add(NonDisclosureAgreementAcceptedVersionXbox);
        hash.Add(SeizureWarningAcknowledgedVersion);
        hash.Add(SeizureWarningAcknowledgedVersionPlayStation);
        hash.Add(SeizureWarningAcknowledgedVersionXbox);
        hash.Add(BattlepassSeasonLoggedOn);
        hash.Add(BattlepassPurchasePopupLastTime);
        hash.Add(NonDisclosureAgreementUserSignature);
        hash.Add(NonDisclosureAgreementUserSignaturePlayStation);
        hash.Add(NonDisclosureAgreementUserSignatureXbox);
        hash.Add(NonDisclosureAgreementUserEmail);
        hash.Add(NonDisclosureAgreementUserEmailPlayStation);
        hash.Add(NonDisclosureAgreementUserEmailXbox);
        hash.Add(LastVersionShownInDriversWarningDialog);
        hash.Add(MinSpecWarningDialogTimesDisplayed);
        hash.Add(PingWarningDialogTimesDisplayed);
        hash.Add(HasCompletedLaunchSettingsFlow);
        hash.Add(IsUsingManualMatchmakingRegionSelection);
        hash.Add(ManualMatchmakingRegionSelections);
        hash.Add(RotatingNewsViewedMessages);
        hash.Add(InkQuality);
        hash.Add(MouseSensitivityADSScale);
        hash.Add(MinimapScale);
        hash.Add(MinimapSize);
        hash.Add(MinimapMaskOpacity);
        hash.Add(InvertedYAxis);
        hash.Add(ToggleCrouch);
        hash.Add(ToggleWalk);
        hash.Add(ToggleADS);
        hash.Add(RecoilBehavior);
        hash.Add(LeftHandedEnabled);
        hash.Add(RecoilPitchCorrectionEnabled);
        hash.Add(IsTeamLaserEnabled);
        hash.Add(IsHudMinimapRotationEnabled);
        hash.Add(IsHudMinimapCenteredOnPlayer);
        hash.Add(IsHudMinimapCircle);
        hash.Add(IsHudMinimapMaskHighContrastEnabled);
        hash.Add(IsHudSnapMinimapWithScoreboardEnabled);
        hash.Add(IsDamageCameraEffectEnabled);
        hash.Add(StreamerModeEnabled);
        hash.Add(HideLobbyCode);
        hash.Add(ADSTracerRatio);
        hash.Add(ADSTracerIntensity);
        hash.Add(OpticHitConfirmIntensity);
        hash.Add(AnonymousMode);
        hash.Add(AnonymizePlayerNames);
        hash.Add(StreamerModeDisableIncomingTextChat);
        hash.Add(StreamerModeDisableIncomingVoiceChat);
        hash.Add(IsTextChatSoundEffectsEnabled);
        hash.Add(SubtitlesEnabled);
        hash.Add(VerboseVoLevel);
        hash.Add(IsBloodFXEnabled);
        hash.Add(OverrideKeymaps);
        hash.Add(VoiceChatInputAudioDevice);
        hash.Add(VoiceChatOutputAudioDevice);
        hash.Add(VoiceChatTeamEnabled);
        hash.Add(VoiceChatConsoleMode);
        hash.Add(VoiceChatPartyEnabled);
        hash.Add(VoiceChatPartyEnabledInGames);
        hash.Add(VoiceChatTeamPushToTalk);
        hash.Add(VoiceChatPartyPushToTalk);
        hash.Add(EnabledTextStats);
        hash.Add(EnabledGraphStats);
        hash.Add(MutedChatContexts);
        hash.Add(InputBindingsVersion);
        hash.Add(Version);
        return hash.ToHashCode();
    }

    public static PlayerConfig CreateDefault(Guid playerId)
    {
        JsonNode? defaultJson = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "defaults", "PlayerConfig.json")));
        defaultJson[nameof(PlayerId)] = playerId;
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true
        };
        return defaultJson.Deserialize<PlayerConfig>(options);
    }

    public static PlayerConfig FromPacket(Packets.PlayerConfig inst, Guid? id = null)
    {
        return id is null
            ? throw new ArgumentNullException(nameof(id))
            : new PlayerConfig((Guid)id, inst.UnlockAllPlayModes, inst.UnlockAllMenuTabs, inst.UnlockAllSponsors, inst.BypassUnlockAllSponsorsOverride,
            inst.BypassProgressionOverrides, inst.BypassTeamSizeOverrides, inst.BypassRegionSelectOverride, inst.BypassCurrencyPurchasingOverride, inst.DisableDevMapSelector,
            inst.ShowDebugInfoPanel, inst.ShowPlatformInfoPanel, inst.ShowMatchmakingCounters, inst.ForceChatEnabled, inst.MostRecentLobbyMode, Guid.Parse(inst.MostRecentPartyId), inst.EndUserLicenseAcceptedVersion,
            inst.EndUserLicenseAcceptedVersionPlayStation, inst.EndUserLicenseAcceptedVersionXbox, inst.TermsOfServiceAcceptedVersion, inst.TermsOfServiceAcceptedVersionPlayStation, inst.TermsOfServiceAcceptedVersionXbox, inst.NonDisclosureAgreementAcceptedVersion, inst.NonDisclosureAgreementAcceptedVersionPlayStation, inst.NonDisclosureAgreementAcceptedVersionXbox,
            inst.SeizureWarningAcknowledgedVersion, inst.SeizureWarningAcknowledgedVersionPlayStation, inst.SeizureWarningAcknowledgedVersionXbox, inst.BattlepassSeasonLoggedOn, inst.BattlepassPurchasePopupLastTime, inst.NonDisclosureAgreementUserSignature, inst.NonDisclosureAgreementUserSignaturePlayStation, inst.NonDisclosureAgreementUserSignatureXbox,
            inst.NonDisclosureAgreementUserEmail, inst.NonDisclosureAgreementUserEmailPlayStation, inst.NonDisclosureAgreementUserEmailXbox, inst.LastVersionShownInDriversWarningDialog, inst.MinSpecWarningDialogTimesDisplayed, inst.PingWarningDialogTimesDisplayed,
            inst.HasCompletedLaunchSettingsFlow, inst.IsUsingManualMatchmakingRegionSelection, inst.ManualMatchmakingRegionSelections.ToArray(),
            inst.RotatingNewsViewedMessages.ToArray(), inst.InkQuality, inst.MouseSensitivityADSScale, inst.MouseSensitivity, inst.MinimapScale, inst.MinimapSize, inst.MinimapMaskOpacity, inst.BInvertedYAxis, inst.BToggleCrouch,
            inst.BToggleWalk, inst.BToggleADS, inst.RecoilBehavior, inst.BLeftHandedEnabled, inst.BRecoilPitchCorrectionEnabled, inst.BIsTeamLaserEnabled, inst.BIsHudMinimapRotationEnabled, inst.BIsHudMinimapCenteredOnPlayer, inst.BIsHudMinimapCircle,
            inst.BIsHudMinimapMaskHighContrastEnabled, inst.BIsHudSnapMinimapWithScoreboardEnabled, inst.BIsDamageCameraEffectEnabled, inst.BStreamerModeEnabled,
            inst.BHideLobbyCode, inst.ADSTracerRatio, inst.ADSTracerIntensity, inst.OpticHitConfirmIntensity, inst.BAnonymousMode, inst.BAnonymizePlayerNames, inst.BStreamerModeDisableIncomingVoiceChat,
            inst.BStreamerModeDisableIncomingTextChat, inst.BIsTextChatSoundEffectsEnabled, inst.BSubtitles, inst.VerboseVoLevel, inst.BIsBloodFXEnabled, inst.OverrideKeymaps.Overrides.ToArray(), inst.VoiceChatInputAudioDevice, inst.VoiceChatOutputAudioDevice,
            inst.BVoiceChatTeamEnabled, inst.VoiceChatConsoleMode, inst.BVoiceChatPartyEnabled, inst.BVoiceChatPartyEnabledInGames, inst.BVoiceChatTeamPushToTalk, inst.BVoiceChatPartyPushToTalk, inst.EnabledTextStats.ToArray(), inst.EnabledGraphStats.ToArray(), inst.MutedChatContexts.ToArray(), inst.InputBindingsVersion, inst.Version);
    }

    public async Task<Packets.PlayerConfig> ToPacketFull(Guid playerId)
    {
        Packets.PlayerConfig packet = new()
        {
            UnlockAllPlayModes = UnlockAllPlayModes,
            UnlockAllMenuTabs = UnlockAllMenuTabs,
            UnlockAllSponsors = UnlockAllSponsors,
            BypassUnlockAllSponsorsOverride = BypassUnlockAllSponsorsOverride,
            BypassProgressionOverrides = BypassProgressionOverrides,
            BypassTeamSizeOverrides = BypassTeamSizeOverrides,
            BypassRegionSelectOverride = BypassRegionSelectOverride,
            BypassCurrencyPurchasingOverride = BypassCurrencyPurchasingOverride,
            DisableDevMapSelector = DisableDevMapSelector,
            ShowDebugInfoPanel = ShowDebugInfoPanel,
            ShowPlatformInfoPanel = ShowPlatformInfoPanel,
            ShowMatchmakingCounters = ShowMatchmakingCounters,
            ForceChatEnabled = ForceChatEnabled,
            MostRecentLobbyMode = MostRecentLobbyMode,
            MostRecentPartyId = MostRecentPartyId.ToString(),
            EndUserLicenseAcceptedVersion = EndUserLicenseAcceptedVersion,
            EndUserLicenseAcceptedVersionPlayStation = EndUserLicenseAcceptedVersionPlayStation,
            EndUserLicenseAcceptedVersionXbox = EndUserLicenseAcceptedVersionXbox,
            TermsOfServiceAcceptedVersion = TermsOfServiceAcceptedVersion,
            TermsOfServiceAcceptedVersionPlayStation = TermsOfServiceAcceptedVersionPlayStation,
            TermsOfServiceAcceptedVersionXbox = TermsOfServiceAcceptedVersionXbox,
            NonDisclosureAgreementAcceptedVersion = NonDisclosureAgreementAcceptedVersion,
            NonDisclosureAgreementAcceptedVersionPlayStation = NonDisclosureAgreementAcceptedVersionPlayStation,
            NonDisclosureAgreementAcceptedVersionXbox = NonDisclosureAgreementAcceptedVersionXbox,
            SeizureWarningAcknowledgedVersion = SeizureWarningAcknowledgedVersion,
            SeizureWarningAcknowledgedVersionPlayStation = SeizureWarningAcknowledgedVersionPlayStation,
            SeizureWarningAcknowledgedVersionXbox = SeizureWarningAcknowledgedVersionXbox,
            BattlepassSeasonLoggedOn = BattlepassSeasonLoggedOn,
            BattlepassPurchasePopupLastTime = BattlepassPurchasePopupLastTime,
            NonDisclosureAgreementUserSignature = NonDisclosureAgreementUserSignature,
            NonDisclosureAgreementUserSignaturePlayStation = NonDisclosureAgreementUserSignature,
            NonDisclosureAgreementUserSignatureXbox = NonDisclosureAgreementUserSignatureXbox,
            NonDisclosureAgreementUserEmail = NonDisclosureAgreementUserEmail,
            NonDisclosureAgreementUserEmailPlayStation = NonDisclosureAgreementUserEmailPlayStation,
            NonDisclosureAgreementUserEmailXbox = NonDisclosureAgreementUserEmailXbox,
            LastVersionShownInDriversWarningDialog = LastVersionShownInDriversWarningDialog,
            MinSpecWarningDialogTimesDisplayed = MinSpecWarningDialogTimesDisplayed,
            PingWarningDialogTimesDisplayed = PingWarningDialogTimesDisplayed,
            HasCompletedLaunchSettingsFlow = HasCompletedLaunchSettingsFlow,
            IsUsingManualMatchmakingRegionSelection = IsUsingManualMatchmakingRegionSelection
        };
        foreach (string item in ManualMatchmakingRegionSelections)
        {
            packet.ManualMatchmakingRegionSelections.Add(item);
        }
        foreach (string item in RotatingNewsViewedMessages)
        {
            packet.RotatingNewsViewedMessages.Add(item);
        }
        packet.Version = (int)Version;
        packet.InkQuality = InkQuality;
        packet.MouseSensitivityADSScale = MouseSensitivityADSScale;
        packet.MouseSensitivity = MouseSensitivity;
        packet.MinimapScale = MinimapScale;
        packet.MinimapSize = MinimapSize;
        packet.MinimapMaskOpacity = MinimapMaskOpacity;
        packet.BInvertedYAxis = InvertedYAxis;
        packet.BToggleCrouch = ToggleCrouch;
        packet.BToggleWalk = ToggleWalk;
        packet.BToggleADS = ToggleADS;
        packet.RecoilBehavior = RecoilBehavior;
        packet.BLeftHandedEnabled = LeftHandedEnabled;
        packet.BRecoilPitchCorrectionEnabled = RecoilPitchCorrectionEnabled;
        packet.BIsTeamLaserEnabled = IsTeamLaserEnabled;
        packet.BIsHudMinimapRotationEnabled = IsHudMinimapRotationEnabled;
        packet.BIsHudMinimapCenteredOnPlayer = IsHudMinimapCenteredOnPlayer;
        packet.BIsHudMinimapCircle = IsHudMinimapCircle;
        packet.BIsHudMinimapMaskHighContrastEnabled = IsHudMinimapMaskHighContrastEnabled;
        packet.BIsHudSnapMinimapWithScoreboardEnabled = IsHudSnapMinimapWithScoreboardEnabled;
        packet.BIsDamageCameraEffectEnabled = IsDamageCameraEffectEnabled;
        packet.BStreamerModeEnabled = StreamerModeEnabled;
        packet.BHideLobbyCode = HideLobbyCode;
        packet.ADSTracerRatio = ADSTracerRatio;
        packet.ADSTracerIntensity = ADSTracerIntensity;
        packet.OpticHitConfirmIntensity = OpticHitConfirmIntensity;
        packet.BAnonymousMode = AnonymousMode;
        packet.BAnonymizePlayerNames = AnonymizePlayerNames;
        packet.BStreamerModeDisableIncomingVoiceChat = StreamerModeDisableIncomingVoiceChat;
        packet.BStreamerModeDisableIncomingTextChat = StreamerModeDisableIncomingTextChat;
        packet.BIsTextChatSoundEffectsEnabled = IsTextChatSoundEffectsEnabled;
        packet.BSubtitles = SubtitlesEnabled;
        packet.VerboseVoLevel = VerboseVoLevel;
        packet.BIsBloodFXEnabled = IsBloodFXEnabled;
        OverrideKeymaps keymaps = new();
        foreach (string item in OverrideKeymaps)
        {
            keymaps.Overrides.Add(item);
        }
        packet.OverrideKeymaps = keymaps;
        packet.VoiceChatInputAudioDevice = VoiceChatInputAudioDevice;
        packet.VoiceChatOutputAudioDevice = VoiceChatOutputAudioDevice;
        packet.BVoiceChatTeamEnabled = VoiceChatTeamEnabled;
        packet.VoiceChatConsoleMode = VoiceChatConsoleMode;
        packet.BVoiceChatPartyEnabledInGames = VoiceChatPartyEnabledInGames;
        packet.BVoiceChatTeamPushToTalk = VoiceChatTeamPushToTalk;
        packet.BVoiceChatPartyPushToTalk = VoiceChatPartyPushToTalk;
        foreach (string item in EnabledTextStats)
        {
            packet.EnabledTextStats.Add(item);
        }
        foreach (string item in EnabledGraphStats)
        {
            packet.EnabledGraphStats.Add(item);
        }
        foreach (string item in MutedChatContexts)
        {
            packet.MutedChatContexts.Add(item);
        }
        packet.InputBindingsVersion = InputBindingsVersion;
        packet.SubtitleUserSettings = (await Model.SubtitleUserSettings.RetrieveFromDatabase(playerId)).ToPacket();
        packet.CrosshairConfig = (await Model.CrosshairConfig.RetrieveFromDatabase(playerId)).ToPacket();
        packet.GamepadConfig = (await Model.GamepadConfig.RetrieveFromDatabase(playerId)).ToPacket();
        packet.ColorVisionConfig = (await Model.ColorVisionConfig.RetrieveFromDatabase(playerId)).ToPacket();
        return packet;
    }
}