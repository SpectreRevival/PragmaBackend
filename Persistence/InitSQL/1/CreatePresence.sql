CREATE TABLE IF NOT EXISTS player_presence (
	player_id UUID PRIMARY KEY,
	basic_status PlayerBasicPresence NOT NULL,
	advanced_presence_type INT NOT NULL,
	advanced_presence_context TEXT NOT NULL,
	last_updated_time TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE OR REPLACE FUNCTION set_updated_time_player_presence()
RETURNS TRIGGER AS $$
BEGIN
	NEW.last_updated_time = NOW();
	return NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_update_time ON player_presence;
CREATE TRIGGER trg_update_time BEFORE UPDATE ON player_presence FOR EACH ROW EXECUTE FUNCTION set_updated_time_player_presence();