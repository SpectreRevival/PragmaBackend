INSERT INTO color_vision_config 
(player_id, 
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
) VALUES (
 @playerid,
 @colorvisiontype,
 @severity,
 @correctdeficiency,
 @showcorrectdeficiency,
 @comfortswapeffect,
 @customoutlinecolor,
 @outlinecolor,
 @outlinecolorlower,
 @outlineThicknessScale,
 @outlineBrightnessScale,
 @version
) ON CONFLICT (player_id)
DO UPDATE SET
	color_vision_type = EXCLUDED.color_vision_type,
	severity = EXCLUDED.severity,
	correct_deficiency = EXCLUDED.correct_deficiency,
	show_correct_deficiency = EXCLUDED.show_correct_deficiency,
	comfort_swap_effect = EXCLUDED.comfort_swap_effect,
	custom_outline_color = EXCLUDED.custom_outline_color,
	outline_color = EXCLUDED.outline_color,
	outline_color_lower = EXCLUDED.outline_color_lower,
	outline_thickness_scale = EXCLUDED.outline_thickness_scale,
	outline_brightness_scale = EXCLUDED.outline_brightness_scale,
	color_vision_config_version = EXCLUDED.color_vision_config_version;