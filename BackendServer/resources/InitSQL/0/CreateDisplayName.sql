DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'DisplayName') THEN
		CREATE TYPE DisplayName AS (
			player_name TEXT,
			discriminator TEXT
		);
	END IF;
END
$$;