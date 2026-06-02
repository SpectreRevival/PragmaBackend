DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'ObjectiveContributionSourceType') THEN
		CREATE TYPE ObjectiveContributionSourceType AS ENUM (
			'MATCH'
		);
	END IF;
END
$$;

DO $$
BEGIN
	IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'ObjectiveContribution') THEN
		CREATE TYPE ObjectiveContribution AS (
			source_type ObjectiveContributionSourceType,
			source_id UUID
		);
	END IF;
END
$$;
