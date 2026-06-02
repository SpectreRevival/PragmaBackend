DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'AlterationChannel') THEN
		CREATE TYPE AlterationChannel AS (
			channel_id TEXT,
			alterations TEXT[]
		);
	END IF;
END
$$;