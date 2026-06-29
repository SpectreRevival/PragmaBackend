CREATE TABLE IF NOT EXISTS customized_instanced_items (
	instance_id UUID PRIMARY KEY,
	catalog_id TEXT NOT NULL,
	owning_player_id UUID NOT NULL,
	viewed BOOL NOT NULL,
	alteration_channels ALTERATIONCHANNEL []
);