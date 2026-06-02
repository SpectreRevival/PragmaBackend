DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'LookSettings') THEN
		CREATE TYPE LookSettings AS (
			yaw_rate DOUBLE PRECISION,
			pitch_scale DOUBLE PRECISION,
			turn_accel_yaw_bonus DOUBLE PRECISION,
			turn_acceL_pitch_bonus DOUBLE PRECISION,
			turn_accel_delay_seconds DOUBLE PRECISION,
			turn_accel_time_to_max DOUBLE PRECISION
		);
	END IF;
END
$$;