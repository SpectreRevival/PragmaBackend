DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = lower('CrosshairDot')) THEN
		CREATE TYPE CrosshairDot AS (
			thickness DOUBLE PRECISION,
			opacity DOUBLE PRECISION,
			color_index INT,
			custom_color RGBAColor,
			outline_enabled BOOL,
			custom_outline_color RGBAColor,
			outline_opacity DOUBLE PRECISION,
			outline_thickness DOUBLE PRECISION
		);
	END IF;
END
$$;
