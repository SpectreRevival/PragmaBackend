using Model;
using System.Text.Json.Nodes;

namespace Processors;

internal static class FriendPresenceNotifications
{
    public static async Task SendPresenceUpdateToFriends(Guid playerId, PlayerPresence presence)
    {
        FriendsList? friends = await FriendsList.RetrieveFromDatabase(playerId);
        if (friends == null)
        {
            return;
        }

        WebsocketNotification notification = new(BuildPresenceUpdateNotification(playerId, presence),
            new SpectreRpcType("FriendRpc.PresenceUpdateV1Notification"));
        foreach (Guid friendId in friends.Friends.Distinct())
        {
            if (friendId == playerId)
            {
                continue;
            }

            await SpectreWebsocket.SendNotificationToPlayerAsync(friendId, notification);
        }
    }

    private static JsonObject BuildPresenceUpdateNotification(Guid playerId, PlayerPresence presence)
    {
        return new JsonObject
        {
            ["newPresence"] = new JsonObject
            {
                ["playerId"] = playerId.ToString(),
                ["gameShardId"] = "00000000-0000-0000-0000-000000000001",
                ["gameTitleId"] = "00000000-0000-0000-0000-000000000001",
                ["basicPresence"] = presence.BasicStatus.ToString(),
                ["version"] = presence.LastUpdatedTime.ToUnixTimeMilliseconds().ToString(),
                ["ext"] = new JsonObject
                {
                    ["lastUpdated"] = presence.LastUpdatedTime.ToUnixTimeMilliseconds().ToString(),
                    ["mainPresenceId"] = presence.AdvancedPresenceType,
                    ["presenceContext"] = presence.AdvancedPresenceContext
                }
            }
        };
    }
}