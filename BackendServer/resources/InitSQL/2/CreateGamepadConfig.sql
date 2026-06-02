CREATE TABLE IF NOT EXISTS gamepad_config(
	player_id UUID PRIMARY KEY,
	input_scheme_index INT NOT NULL,
	gamepad_glyph_index INT NOT NULL,
	look_preset_index INT NOT NULL,
	custom_look_config LookConfig NOT NULL,
	custom_response_curve ResponseCurve NOT NULL,
	invert_look BOOL NOT NULL,
	controller_feedback_value INT NOT NULL,
	turn_accel BOOL NOT NULL,
	aim_assist BOOL NOT NULL,
	response_curve_index INT NOT NULL,
	response_curve_arc_deg DOUBLE PRECISION NOT NULL,
	response_curve_slope DOUBLE PRECISION NOT NULL,
	response_curve_linear_blend_pow DOUBLE PRECISION NOT NULL,
	custom_scale_ads DOUBLE PRECISION NOT NULL,
	toggle_crouch BOOL NOT NULL,
	toggle_walk BOOL NOT NULL,
	toggle_plant_defuse BOOL NOT NULL,
	toggle_ads BOOL NOT NULL,
	end_walk_when_firing_behavior TEXT NOT NULL,
	ads_trigger_threshold DOUBLE PRECISION NOT NULL,
	dead_zone_move_amount TEXT NOT NULL,
	custom_dead_zone_move_amount DOUBLE PRECISION NOT NULL,
	dead_zone_look_amount TEXT NOT NULL,
	custom_dead_zone_look_amount DOUBLE PRECISION NOT NULL,
	walk_run_deflection_threshold DOUBLE PRECISION NOT NULL,
	gamepad_config_version INT NOT NULL,

	CONSTRAINT verify_custom_look_config CHECK (
		(custom_look_config).display_name IS NOT NULL AND
		(custom_look_config).look_settings.yaw_rate IS NOT NULL AND
		(custom_look_config).look_settings.pitch_scale IS NOT NULL AND
		(custom_look_config).look_settings.turn_accel_yaw_bonus IS NOT NULL AND
		(custom_look_config).look_settings.turn_accel_pitch_bonus IS NOT NULL AND
		(custom_look_config).look_settings.turn_accel_delay_seconds IS NOT NULL AND
		(custom_look_config).look_settings.turn_accel_time_to_max IS NOT NULL AND
		(custom_look_config).look_settings_ads.yaw_rate IS NOT NULL AND
		(custom_look_config).look_settings_ads.pitch_scale IS NOT NULL AND
		(custom_look_config).look_settings_ads.turn_accel_yaw_bonus IS NOT NULL AND
		(custom_look_config).look_settings_ads.turn_accel_pitch_bonus IS NOT NULL AND
		(custom_look_config).look_settings_ads.turn_accel_delay_seconds IS NOT NULL AND
		(custom_look_config).look_settings_ads.turn_accel_time_to_max IS NOT NULL AND
	),
	CONSTRAINT verify_custom_response_curve CHECK (
		(custom_response_curve).display_name IS NOT NULL AND
		(custom_response_curve).exponent IS NOT NULL AND
		(custom_response_curve).response_curve_arc_degree IS NOT NULL AND
		(custom_response_curve).response_curve_slope IS NOT NULL AND
		(custom_response_curve).response_curve_linear_blend_power IS NOT NULL
	)
);