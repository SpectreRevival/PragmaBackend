CREATE TABLE IF NOT EXISTS player_presence (
    player_id UUID PRIMARY KEY,
    basic_status PLAYERBASICPRESENCE NOT NULL,
    advanced_presence_type INT NOT NULL,
    advanced_presence_context TEXT NOT NULL,
    last_updated_time TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE OR REPLACE FUNCTION SET_UPDATED_TIME_PLAYER_PRESENCE()
RETURNS TRIGGER AS $$
BEGIN
	NEW.last_updated_time = NOW();
	return NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_update_time ON player_presence;
CREATE TRIGGER trg_update_time BEFORE UPDATE ON player_presence FOR EACH ROW EXECUTE FUNCTION SET_UPDATED_TIME_PLAYER_PRESENCE();
