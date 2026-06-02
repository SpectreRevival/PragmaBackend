DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'ActiveAlterationData') THEN
		CREATE TYPE ActiveAlterationData AS (
			channel_id TEXT,
			alteration_id TEXT
		);
	END IF;
END
$$;