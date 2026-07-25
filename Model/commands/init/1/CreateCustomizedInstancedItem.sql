CREATE TABLE IF NOT EXISTS customized_instanced_items (
	instance_id UUID PRIMARY KEY,
	catalog_id TEXT NOT NULL,
	owning_player_id UUID NOT NULL,
	viewed BOOL NOT NULL,
	alteration_channels ALTERATIONCHANNEL []
);

CREATE INDEX IF NOT EXISTS idx_customized_instanced_by_player ON customized_instanced_items (
	owning_player_id, instance_id
);