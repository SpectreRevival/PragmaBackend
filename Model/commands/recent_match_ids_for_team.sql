SELECT MatchHistory.match_id FROM match_history MatchHistory
INNER JOIN match_history_team_data TeamData ON MatchHistory.match_id = TeamData.match_id
WHERE MatchHistory.match_date BETWEEN @start_date AND @end_date
AND TeamData.team_id = @team_id
ORDER BY MatchHistory.match_date DESC
LIMIT @limit;