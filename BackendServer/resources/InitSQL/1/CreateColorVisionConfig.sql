CREATE TABLE IF NOT EXISTS color_vision_config (
    player_id UUID PRIMARY KEY,
    color_vision_type TEXT NOT NULL,
    severity INT NOT NULL,
    correct_deficiency BOOL NOT NULL,
    show_correct_deficiency BOOL NOT NULL,
    comfort_swap_effect BOOL NOT NULL,
    custom_outline_color BOOL NOT NULL,
    outline_color RGBACOLOR NOT NULL,
    outline_color_lower RGBACOLOR NOT NULL,
    outline_thickness_scale DOUBLE PRECISION NOT NULL,
    outline_brightness_scale DOUBLE PRECISION NOT NULL,
    color_vision_config_version INT NOT NULL,

    CONSTRAINT verify_outline_color CHECK (
        (outline_color).r IS NOT NULL
        AND (outline_color).g IS NOT NULL
        AND (outline_color).b IS NOT NULL
        AND (outline_color).a IS NOT NULL
    ),
    CONSTRAINT verify_outline_color_lower CHECK (
        (outline_color_lower).r IS NOT NULL
        AND (outline_color_lower).g IS NOT NULL
        AND (outline_color_lower).b IS NOT NULL
        AND (outline_color_lower).a IS NOT NULL
    )
);
