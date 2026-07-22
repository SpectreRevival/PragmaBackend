INSERT INTO stackable_items (
	instance_id,
	catalog_id,
	amount,
	owning_player_id
) VALUES (
	@instance_id,
	@catalog_id,
	@amount,
	@owning_player_id
) ON CONFLICT (instance_id)
DO UPDATE SET
	catalog_id = EXCLUDED.catalog_id,
	amount = EXCLUDED.amount,
	owning_player_id = EXCLUDED.owning_player_id;