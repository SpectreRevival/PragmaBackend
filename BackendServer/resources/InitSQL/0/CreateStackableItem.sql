DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'StackableItem') THEN
		CREATE TYPE StackableItem AS (
			catalog_id TEXT,
			instance_id TEXT,
			amount BIGINT
		);
	END IF;
END
$$;
