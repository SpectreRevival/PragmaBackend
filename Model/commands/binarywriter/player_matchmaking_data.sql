COPY player_matchmaking_data (
player_id,
casual_mmr,
ranked_mmr,
solo_rank_points,
casual_matches_played,
ranked_matches_played,
casual_matches_played_seasonal,
ranked_matches_played_seasonal,
ranked_placement_matches,
current_solo_rank,
highest_team_rank,
casual_matches_won,
ranked_matches_won,
priority_matchmaking_until,
restrict_matchmaking_until,
map_history
) FROM STDIN (FORMAT BINARY);