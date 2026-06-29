CREATE TABLE IF NOT EXISTS sponsor_tracker_items (
	instance_id UUID PRIMARY KEY,
	catalog_id TEXT NOT NULL,
	owning_player_id UUID NOT NULL,
	viewed BOOL NOT NULL,
	sponsor_name TEXT NOT NULL
);