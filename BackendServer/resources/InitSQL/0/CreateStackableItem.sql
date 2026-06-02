DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'StackableItem') THEN
		CREATE TYPE StackableItem AS (
			catalog_id UUID,
			instance_id UUID,
			amount BIGINT
		);
	END IF;
END
$$;
