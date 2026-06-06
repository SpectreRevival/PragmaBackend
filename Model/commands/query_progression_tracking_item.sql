SELECT instance_id,
	catalog_id,
	owning_player_id,
	viewed,
	progression_by_stats,
	are_objectives_completed,
	current_objective_id,
	current_objective_index,
	is_premium_unlocked,
	team_id,
	last_contribution,
	is_bundle_purchased,
	num_levels_purchased
	FROM instanced_items WHERE instance_id = @instance_id LIMIT 1;