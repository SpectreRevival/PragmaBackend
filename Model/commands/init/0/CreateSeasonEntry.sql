CREATE TABLE IF NOT EXISTS season_entries (
    season_number INT PRIMARY KEY,
    start_ts TIMESTAMPTZ NOT NULL,
    end_ts TIMESTAMPTZ NOT NULL,
    first_week_start_ts TIMESTAMPTZ NOT NULL,
    last_week_end_ts TIMESTAMPTZ NOT NULL,
    num_weeks INT NOT NULL
);

INSERT INTO season_entries (season_number, start_ts, end_ts, first_week_start_ts, last_week_end_ts, num_weeks)
VALUES (1, to_timestamp(1739984400000 / 1000.0), to_timestamp(1748966400000 / 1000.0), to_timestamp(1740477600000 / 1000.0), to_timestamp(1747735200000 / 1000.0), 12)
ON CONFLICT (season_number) DO NOTHING;