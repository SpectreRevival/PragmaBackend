SELECT DISTINCT MatchHistory.match_id FROM match_history MatchHistory 
INNER JOIN match_history_player_data PlayerData ON MatchHistory.match_id = PlayerData.match_id
WHERE MatchHistory.match_date BETWEEN @start_date AND @end_date
AND PlayerData.player_id = @player_id
ORDER BY MatchHistory.match_date DESC
LIMIT @limit;