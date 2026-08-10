WITH MatchingMatchIds AS (
	SELECT TeamData.match_id FROM match_history_team_data TeamData
	INNER JOIN match_history_player_data PlayerData ON TeamData.team_number = PlayerData.team_number
	WHERE PlayerData.player_id ANY(@player_ids)
	GROUP BY TeamData.match_id
	HAVING COUNT(DISTINCT PlayerData.player_id) = array_length(@player_ids, 1)
)
SELECT TeamData.team_id FROM match_history MatchHistory INNER JOIN MatchingMatchIds matchids ON MatchHistory.match_id = m.match_id
INNER JOIN match_history_team_data TeamData ON MatchHistory.match_id = TeamData.match_id
ORDER BY MatchHistory.match_date DESC
LIMIT 1;