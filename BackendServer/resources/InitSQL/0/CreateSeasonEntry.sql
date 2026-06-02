CREATE TABLE IF NOT EXISTS season_entries (
    season_number INT PRIMARY KEY,
    start_ts TIMESTAMPTZ NOT NULL,
    end_ts TIMESTAMPTZ NOT NULL,
    last_week_end_ts TIMESTAMPTZ NOT NULL,
    num_weeks INT NOT NULL
);
