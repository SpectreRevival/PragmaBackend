DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typename = 'ActiveAlterationData') THEN
		CREATE TYPE ActiveAlterationData AS (
			TEXT channel_id,
			TEXT alteration_id
		);
	END IF;
END
$$;