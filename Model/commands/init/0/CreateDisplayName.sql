DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = lower('DisplayName')) THEN
		CREATE TYPE DisplayName AS (
			player_name TEXT,
			discriminator TEXT
		);
	END IF;
END
$$;
