DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'ResponseCurve') THEN
		CREATE TYPE ResponseCurve AS (
			display_name TEXT,
			exponent DOUBLE PRECISION,
			response_curve_arc_degree DOUBLE PRECISION,
			response_curve_slope DOUBLE PRECISION,
			response_curve_linear_blend_power DOUBLE PRECISION
		);
	END IF;
END
$$;
