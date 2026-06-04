INSERT INTO subtitle_settings (
	player_id,
	font_size,
	background_opacity,
	speaker_qualifier_display,
	post_player_subtitles,
	post_player_subtitles_to_chat,
	names_to_show_mask,
	subtitle_settings_version
) VALUES (
	@player_id,
	@font_size,
	@background_opacity,
	@speaker_qualifier_display,
	@post_player_subtitles,
	@post_player_subtitles_to_chat,
	@names_to_show_mask,
	@version
) ON CONFLICT (player_id)
DO UPDATE SET
	font_size = EXCLUDED.font_size,
	background_opacity = EXCLUDED.background_opacity,
	speaker_qualifier_display = EXCLUDED.speaker_qualifier_display,
	post_player_subtitles = EXCLUDED.post_player_subtitles,
	post_player_subtitles_to_chat = EXCLUDED.post_player_subtitles_to_chat,
	names_to_show_mask = EXCLUDED.names_to_show_mask,
	subtitle_settings_version = EXCLUDED.subtitle_settings_version;