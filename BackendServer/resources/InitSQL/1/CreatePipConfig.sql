DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'PipConfig') THEN
		CREATE TYPE PipConfig AS (
			thickness DOUBLE PRECISION,
			piplen DOUBLE PRECISION,
			opacity DOUBLE PRECISION,
			pip_offset DOUBLE PRECISION,
			move_accuracy_offset BOOL,
			fire_accuracy_offset BOOL
		);
	END IF;
END
$$;
