INSERT INTO season_entries (season_number, start_ts, end_ts, first_week_start_ts, last_week_end_ts, num_weeks)
VALUES (@season_number, @start_ts, @end_ts, @first_week_start_ts, @last_week_end_ts, @num_weeks)
ON CONFLICT (season_number)
DO UPDATE SET
	start_ts = EXCLUDED.start_ts,
	end_ts = EXCLUDED.end_ts,
	first_week_start_ts = EXCLUDED.first_week_start_ts,
	last_week_end_ts = EXCLUDED.last_week_end_ts,
	num_weeks = EXCLUDED.num_weeks;