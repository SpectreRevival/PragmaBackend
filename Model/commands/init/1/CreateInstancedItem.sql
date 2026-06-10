CREATE TABLE IF NOT EXISTS instanced_items (
    instance_id UUID PRIMARY KEY,
    catalog_id TEXT NOT NULL,
    owning_player_id UUID NOT NULL,
    viewed BOOL NOT NULL,
    alteration_channels ALTERATIONCHANNEL [],
    sponsor_name TEXT,
    progression_by_stats HSTORE,
    are_objectives_completed BOOL,
    current_objective_id UUID,
    current_objective_index INT,
    is_premium_unlocked BOOL,
    team_id UUID,
    last_contribution OBJECTIVECONTRIBUTION,
    is_bundle_purchased BOOL,
    num_levels_purchased INT,

    CONSTRAINT progression_check CHECK (
        (
            progression_by_stats IS NULL
            AND are_objectives_completed IS NULL
            AND current_objective_id IS NULL
            AND current_objective_index IS NULL
            AND is_premium_unlocked IS NULL
            AND is_bundle_purchased IS NULL
            AND num_levels_purchased IS NULL
            AND team_id IS NULL
            AND last_contribution IS NULL
        )
        OR
        (
            progression_by_stats IS NOT NULL
            AND are_objectives_completed IS NOT NULL
            AND current_objective_id IS NOT NULL
            AND current_objective_index IS NOT NULL
            AND is_premium_unlocked IS NOT NULL
            AND is_bundle_purchased IS NOT NULL
            AND num_levels_purchased IS NOT NULL
        )
    ),
    CONSTRAINT team_id_plus_obj_contrib CHECK (
        (last_contribution IS NULL AND team_id IS NULL)
        OR (last_contribution IS NOT NULL AND team_id IS NOT NULL)
    ),
    CONSTRAINT ext_exclusivity CHECK (
        (
            alteration_channels IS NOT NULL
            AND are_objectives_completed IS NULL
            AND sponsor_name IS NULL
        )
        OR (
            alteration_channels IS NULL
            AND are_objectives_completed IS NOT NULL
            AND sponsor_name IS NULL
        )
        OR (
            alteration_channels IS NULL
            AND are_objectives_completed IS NULL
            AND sponsor_name IS NOT NULL
        )
    )
);
