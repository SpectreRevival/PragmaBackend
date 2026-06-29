INSERT INTO sponsor_tracker_items (
	instance_id,
	catalog_id,
	owning_player_id,
	viewed,
	sponsor_name
) VALUES (
	@instance_id,
	@catalog_Id,
	@owning_player_id,
	@viewed,
	@sponsor_name
) ON CONFLICT (instance_id)
DO UPDATE SET
	catalog_id = EXCLUDED.catalog_id,
	owning_player_id = EXCLUDED.owning_player_id,
	viewed = EXCLUDED.viewed,
	sponsor_name = EXCLUDED.sponsor_name;