DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = lower('OverrideInput')) THEN
		CREATE TYPE OverrideInput AS (
			is_axis BOOLEAN,
			index_map INTEGER,
			input_name TEXT,
			axis_scale DOUBLE PRECISION,
			key_name TEXT,
			shift BOOLEAN,
			ctrl BOOLEAN,
			alt BOOLEAN,
			cmd BOOLEAN
		);
	END IF;
END
$$;
