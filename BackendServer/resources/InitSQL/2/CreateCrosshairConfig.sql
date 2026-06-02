CREATE TABLE IF NOT EXISTS crosshair_config (
    player_id UUID PRIMARY KEY,
    color_index INT NOT NULL,
    custom_color RGBACOLOR NOT NULL,
    fire_accuracy_fade BOOL NOT NULL,
    follow_recoil BOOL NOT NULL,
    show_outlines BOOL NOT NULL,
    outline_thickness DOUBLE PRECISION NOT NULL,
    outline_opacity DOUBLE PRECISION NOT NULL,
    show_center_dot BOOL NOT NULL,
    use_ads_settings BOOL NOT NULL,
    center_dot CROSSHAIRDOT NOT NULL,
    center_dot_ads CROSSHAIRDOT NOT NULL,
    sniper_dot CROSSHAIRDOT NOT NULL,
    inner_pip PIPCONFIG NOT NULL,
    outer_pip PIPCONFIG NOT NULL,
    crosshair_config_version INT NOT NULL,

    CONSTRAINT verify_custom_color CHECK (
        (custom_color).r IS NOT NULL
        AND (custom_color).g IS NOT NULL
        AND (custom_color).b IS NOT NULL
        AND (custom_color).a IS NOT NULL
    ),
    CONSTRAINT verify_center_dot CHECK (
        (center_dot).thickness IS NOT NULL
        AND (center_dot).opacity IS NOT NULL
        AND (center_dot).color_index IS NOT NULL
        AND (center_dot).custom_color IS NOT NULL
        AND (center_dot).custom_color.r IS NOT NULL
        AND (center_dot).custom_color.g IS NOT NULL
        AND (center_dot).custom_color.b IS NOT NULL
        AND (center_dot).custom_color.a IS NOT NULL
        AND (center_dot).outline_enabled IS NOT NULL
        AND (center_dot).custom_outline_color IS NOT NULL
        AND (center_dot).custom_outline_color.r IS NOT NULL
        AND (center_dot).custom_outline_color.g IS NOT NULL
        AND (center_dot).custom_outline_color.b IS NOT NULL
        AND (center_dot).custom_outline_color.a IS NOT NULL
        AND (center_dot).outline_opacity IS NOT NULL
        AND (center_dot).outline_thickness IS NOT NULL
    ),
    CONSTRAINT verify_center_dot_ads CHECK (
        (center_dot_ads).thickness IS NOT NULL
        AND (center_dot_ads).opacity IS NOT NULL
        AND (center_dot_ads).color_index IS NOT NULL
        AND (center_dot_ads).custom_color IS NOT NULL
        AND (center_dot_ads).custom_color.r IS NOT NULL
        AND (center_dot_ads).custom_color.g IS NOT NULL
        AND (center_dot_ads).custom_color.b IS NOT NULL
        AND (center_dot_ads).custom_color.a IS NOT NULL
        AND (center_dot_ads).outline_enabled IS NOT NULL
        AND (center_dot_ads).custom_outline_color IS NOT NULL
        AND (center_dot_ads).custom_outline_color.r IS NOT NULL
        AND (center_dot_ads).custom_outline_color.g IS NOT NULL
        AND (center_dot_ads).custom_outline_color.b IS NOT NULL
        AND (center_dot_ads).custom_outline_color.a IS NOT NULL
        AND (center_dot_ads).outline_opacity IS NOT NULL
        AND (center_dot_ads).outline_thickness IS NOT NULL
    ),
    CONSTRAINT verify_sniper_dot CHECK (
        (sniper_dot).thickness IS NOT NULL
        AND (sniper_dot).opacity IS NOT NULL
        AND (sniper_dot).color_index IS NOT NULL
        AND (sniper_dot).custom_color IS NOT NULL
        AND (sniper_dot).custom_color.r IS NOT NULL
        AND (sniper_dot).custom_color.g IS NOT NULL
        AND (sniper_dot).custom_color.b IS NOT NULL
        AND (sniper_dot).custom_color.a IS NOT NULL
        AND (sniper_dot).outline_enabled IS NOT NULL
        AND (sniper_dot).custom_outline_color IS NOT NULL
        AND (sniper_dot).custom_outline_color.r IS NOT NULL
        AND (sniper_dot).custom_outline_color.g IS NOT NULL
        AND (sniper_dot).custom_outline_color.b IS NOT NULL
        AND (sniper_dot).custom_outline_color.a IS NOT NULL
        AND (sniper_dot).outline_opacity IS NOT NULL
        AND (sniper_dot).outline_thickness IS NOT NULL
    ),
    CONSTRAINT verify_inner_pip CHECK (
        (inner_pip).thickness IS NOT NULL
        AND (inner_pip).piplen IS NOT NULL
        AND (inner_pip).opacity IS NOT NULL
        AND (inner_pip).pip_offset IS NOT NULL
        AND (inner_pip).move_accuracy_offset IS NOT NULL
        AND (inner_pip).fire_accuracy_offset IS NOT NULL
    ),
    CONSTRAINT verify_outer_pip CHECK (
        (outer_pip).thickness IS NOT NULL
        AND (outer_pip).piplen IS NOT NULL
        AND (outer_pip).opacity IS NOT NULL
        AND (outer_pip).pip_offset IS NOT NULL
        AND (outer_pip).move_accuracy_offset IS NOT NULL
        AND (outer_pip).fire_accuracy_offset IS NOT NULL
    )
);
