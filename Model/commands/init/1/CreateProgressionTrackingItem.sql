CREATE TABLE IF NOT EXISTS progression_tracking_items (
	instance_id UUID PRIMARY KEY,
	catalog_id TEXT NOT NULL,
	owning_player_id UUID NOT NULL,
	viewed BOOL NOT NULL,
	progression_by_stats HSTORE NOT NULL,
	are_objectives_completed BOOL NOT NULL,
	current_objective_id INT NOT NULL,
	current_objective_index INT NOT NULL,
	is_premium_unlocked BOOL NOT NULL,
	team_id UUID,
	last_contribution OBJECTIVECONTRIBUTION,
	is_bundle_purchased BOOL NOT NULL,
	num_levels_purchased INT NOT NULL
);