SELECT instance_id, catalog_id, owning_player_id, viewed, alteration_channels FROM customized_instanced_items WHERE owning_player_id = @player_id AND alteration_channels IS NOT NULL;
