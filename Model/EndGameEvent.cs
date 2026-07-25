using System.Diagnostics.CodeAnalysis;

namespace Model;

// Note: This is a point of game-server (private) and public interchange. This class will be created by the private
// game server and forwarded here for us to use.
public record class EndGameEvent
{
    [SetsRequiredMembers]
    public EndGameEvent(Guid matchId, DateTimeOffset matchCreationTime, DateTimeOffset matchEndTime, string map, string gameMode, GameEndPlayer[] players)
    {
        MatchId = matchId;
        MatchCreationTime = matchCreationTime;
        MatchEndTime = matchEndTime;
        Map = map ?? throw new ArgumentNullException(nameof(map));
        GameMode = gameMode ?? throw new ArgumentNullException(nameof(gameMode));
        Players = players ?? throw new ArgumentNullException(nameof(players));
    }

    public required Guid MatchId { get; set; }
    public required DateTimeOffset MatchCreationTime { get; set; }
    public required DateTimeOffset MatchEndTime { get; set; }
    public required string Map { get; set; }
    public required string GameMode { get; set; }
    public required GameEndPlayer[] Players { get; set; }

    public virtual bool Equals(EndGameEvent? @event)
    {
        return @event is not null &&
               EqualityComparer<Type>.Default.Equals(EqualityContract, @event.EqualityContract) &&
               MatchId.Equals(@event.MatchId) &&
               MatchCreationTime.Equals(@event.MatchCreationTime) &&
               MatchEndTime.Equals(@event.MatchEndTime) &&
               Map == @event.Map &&
               GameMode == @event.GameMode &&
               EqualityComparer<GameEndPlayer[]>.Default.Equals(Players, @event.Players);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(EqualityContract, MatchId, MatchCreationTime, MatchEndTime, Map, GameMode, Players);
    }
}