DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = lower('uint8')) THEN
        CREATE DOMAIN uint8 AS SMALLINT
            CHECK (VALUE >= 0 AND VALUE <= 255);
    END IF;
END
$$;

DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = lower('RGBAColor')) THEN
		CREATE TYPE RGBAColor AS (
			r uint8,
			g uint8,
			b uint8,
			a uint8
		);
	END IF;
END
$$;
