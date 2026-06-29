DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = lower('ObjectiveContributionSourceType')) THEN
		CREATE TYPE ObjectiveContributionSourceType AS ENUM (
			'MATCH'
		);
	END IF;
END
$$;

DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = lower('ObjectiveContribution')) THEN
		CREATE TYPE ObjectiveContribution AS (
			source_type ObjectiveContributionSourceType,
			source_id UUID
		);
	END IF;
END
$$;
