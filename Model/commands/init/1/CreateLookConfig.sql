DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = lower('LookConfig')) THEN
		CREATE TYPE LookConfig AS (
			display_name TEXT,
			look_settings LookSettings,
			look_settings_ads LookSettings
		);
	END IF;
END
$$;
