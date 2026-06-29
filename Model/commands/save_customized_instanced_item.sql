INSERT INTO customized_instanced_items (
	instance_id,
	catalog_id,
	owning_player_id,
	viewed,
	alteration_channels
) VALUES (
	@instance_id,
	@catalog_id,
	@owning_player_id,
	@viewed,
	@alteration_channels
) ON CONFLICT (instance_id)
DO UPDATE SET
	catalog_id = EXCLUDED.catalog_id,
	owning_player_id = EXCLUDED.owning_player_id,
	viewed = EXCLUDED.viewed,
	alteration_channels = EXCLUDED.alteration_channels;