COPY color_vision_config (
player_id,
color_vision_type,
severity,
correct_deficiency,
show_correct_deficiency,
comfort_swap_effect,
custom_outline_color,
outline_color,
outline_color_lower,
outline_thickness_scale,
outline_brightness_scale,
color_vision_config_version
) FROM STDIN (FORMAT BINARY);