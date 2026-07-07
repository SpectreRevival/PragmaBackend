using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Processors;

public partial class SpectreWebsocket
{
    private static readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, SpectreWebsocket>> ConnectionsByPlayerId = new();
    public required Guid PlayerId { get; init; }
    public Guid ConnectionId { get; } = Guid.NewGuid();
    private WebSocket Socket { get; init; }
    private int sequenceId = 0;
    private readonly SemaphoreSlim sendLock = new(1, 1);
    private static readonly int MAX_BUFFER_SIZE = 32 * 1024 * 1024;

    partial void InitTracing();
    partial void TraceConnect();
    partial void TraceRequestReceived(string rpcType, int requestId);
    partial void TraceAfterProcess();
    partial void TraceAfterResponse();
    partial void TraceRpcComplete(string rpcType, int requestId, string requestPayload, string responsePayload, int notifications);
    partial void TraceProcessorError(string rpcType, int requestId, string message);
    partial void TraceNotification(string rpcType, int sequence, string payload);
    partial void TraceDisconnect();

    [SetsRequiredMembers]
    public SpectreWebsocket(HttpContext upgradeRequest, WebSocket webSocket)
    {
        Socket = webSocket;
        StringValues token = upgradeRequest.Request.Headers.Authorization;
        if (StringValues.IsNullOrEmpty(token))
        {
            throw new InvalidDataException("tried to start websocket connection without authorization header");
        }
        string bearer = token.ToString();
        if (!bearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("auth header was not a bearer token");
        }
        string rawJwt = bearer[7..].Trim();
        JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(rawJwt);
        Guid? possiblePID = Guid.Parse(jwt.Claims.FirstOrDefault(c => c.Type == "pragmaPlayerId")?.Value ?? throw new InvalidDataException("failed to get value for pragmaPlayerId claim"));
        if (possiblePID == null)
        {
            throw new InvalidDataException("No valid GUID on pragmaPlayerId claim");
        }
        PlayerId = (Guid)possiblePID;
        ConnectionsByPlayerId.GetOrAdd(PlayerId, _ => new ConcurrentDictionary<Guid, SpectreWebsocket>())[ConnectionId] = this;
        InitTracing();
    }

    public async Task SendNotificationAsync(SpectreRpcType rpcType, string payload, CancellationToken cancellationToken = default)
    {
        await sendLock.WaitAsync(cancellationToken);
        int seq = 0;
        try
        {
            seq = sequenceId;
            string fullMessage = "{\"sequenceNumber\":" + sequenceId.ToString() + ",\"notification\":{\"type\":\"" + rpcType.ToString() + "\",\"payload\":" + payload + "}}";
            await Socket.SendAsync(Encoding.UTF8.GetBytes(fullMessage), WebSocketMessageType.Text, true, cancellationToken);
            sequenceId++;
        }
        finally
        {
            sendLock.Release();
        }
        TraceNotification(rpcType.ToString(), seq, payload);
    }

    public async Task SendNotificationAsync(WebsocketNotification notif, CancellationToken cancellationToken = default)
    {
        await SendNotificationAsync(notif.GetRpcType(), notif.GetData(), cancellationToken);
    }

    public static async Task<bool> SendNotificationToPlayerAsync(Guid playerId, SpectreRpcType rpcType, string payload, CancellationToken cancellationToken = default)
    {
        if (!ConnectionsByPlayerId.TryGetValue(playerId, out ConcurrentDictionary<Guid, SpectreWebsocket>? connections))
        {
            return false;
        }

        bool delivered = false;
        foreach (KeyValuePair<Guid, SpectreWebsocket> pair in connections)
        {
            try
            {
                await pair.Value.SendNotificationAsync(rpcType, payload, cancellationToken);
                delivered = true;
            }
            catch (Exception ex) when (ex is WebSocketException or ObjectDisposedException or OperationCanceledException)
            {
                Log.Warning("Dropping stale websocket notification target {ConnectionId} for player {PlayerId}: {Message}", pair.Key, playerId, ex.Message);
                UnregisterConnection(playerId, pair.Key, pair.Value);
            }
        }
        return delivered;
    }

    public static async Task<bool> SendNotificationToPlayerAsync(Guid playerId, WebsocketNotification notif, CancellationToken cancellationToken = default)
    {
        return await SendNotificationToPlayerAsync(playerId, notif.GetRpcType(), notif.GetData(), cancellationToken);
    }

    private async Task SendResponseAsync(SpectreWebsocketRequest request, SpectreWebsocketMessage payload, CancellationToken cancellationToken)
    {
        await sendLock.WaitAsync(cancellationToken);
        try
        {
            string fullMessage = "{\"sequenceNumber\":" + sequenceId.ToString() + ",\"response\":{\"requestId\":" + request.RequestId.ToString() + ",\"type\":\"" + request.RpcType.GetResponseType().ToString() + "\",\"payload\":" + payload.GetData() + "}}";
            await Socket.SendAsync(Encoding.UTF8.GetBytes(fullMessage), WebSocketMessageType.Text, true, cancellationToken);
            sequenceId++;
        }
        finally
        {
            sendLock.Release();
        }
    }

    public async Task HandleAsync(CancellationToken cancellationToken = default)
    {
        byte[] recv = new byte[MAX_BUFFER_SIZE];

        TraceConnect();
        try
        {
            while (Socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                using MemoryStream memStream = new();
                ValueWebSocketReceiveResult result;
                do
                {
                    result = await Socket.ReceiveAsync(new Memory<byte>(recv), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", cancellationToken);
                        return;
                    }
                    memStream.Write(recv, 0, result.Count);
                } while (!result.EndOfMessage);
                memStream.Seek(0, SeekOrigin.Begin);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string rawText = Encoding.UTF8.GetString(memStream.ToArray());
                    SpectreWebsocketRequest wsReq;
                    try
                    {
                        wsReq = new(JsonDocument.Parse(rawText));
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("Failed to parse websocket message from player {PlayerId}: {Message}", PlayerId, ex.Message);
                        continue;
                    }

                    TraceRequestReceived(wsReq.RpcType.ToString(), wsReq.RequestId);
                    WebsocketPacketProcessor? processor = WebsocketPacketProcessor.GetProcessorForRequestType(wsReq.RpcType);
                    if (processor is null)
                    {
                        string error = $"No websocket processor recognized for rpc type {wsReq.RpcType}";
                        Log.Warning("{Message}", error);
                        TraceProcessorError(wsReq.RpcType.ToString(), wsReq.RequestId, error);
                        continue;
                    }

                    SpectreWebsocketMessage messageOut;
                    try
                    {
                        messageOut = await processor.ProcessPacket(wsReq, this);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Processor for {RpcType} request {RequestId} failed for player {PlayerId}", wsReq.RpcType, wsReq.RequestId, PlayerId);
                        TraceProcessorError(wsReq.RpcType.ToString(), wsReq.RequestId, ex.Message);
                        continue;
                    }
                    TraceAfterProcess();

                    await SendResponseAsync(wsReq, messageOut, cancellationToken);
                    TraceAfterResponse();

                    WebsocketNotification[] postSendNotifications = messageOut.GetNotifications();
                    foreach (WebsocketNotification notif in postSendNotifications)
                    {
                        try
                        {
                            await SendNotificationAsync(notif.GetRpcType(), notif.GetData());
                        }
                        catch (Exception ex) when (ex is WebSocketException or ObjectDisposedException or OperationCanceledException)
                        {
                            Log.Warning("Post-response notification {RpcType} failed for player {PlayerId}: {Message}", notif.GetRpcType(), PlayerId, ex.Message);
                        }
                    }

                    try
                    {
                        await messageOut.RunPostResponseActionsAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Post-response action for {RpcType} request {RequestId} failed for player {PlayerId}", wsReq.RpcType, wsReq.RequestId, PlayerId);
                    }

                    TraceRpcComplete(wsReq.RpcType.ToString(), wsReq.RequestId, wsReq.RequestPayload.ToJsonString(), messageOut.GetData(), postSendNotifications.Length);
                }
                else
                {
                    Log.Warning($"Unrecognized websocket message type received {result.MessageType}");
                }
            }
        }
        catch (Exception ex) when (ex is WebSocketException or OperationCanceledException or ObjectDisposedException)
        {
            Log.Error($"Websocket connection closed due to exception: {ex.Message}");
        }
        finally
        {
            bool wasLastConnection = UnregisterConnection(PlayerId, ConnectionId, this);
            if (wasLastConnection)
            {
                await RegisterOfflineAsync();
            }
            TraceDisconnect();
            sendLock.Dispose();
        }
    }

    private static bool UnregisterConnection(Guid playerId, Guid connectionId, SpectreWebsocket connection)
    {
        if (!ConnectionsByPlayerId.TryGetValue(playerId, out ConcurrentDictionary<Guid, SpectreWebsocket>? connections))
        {
            return false;
        }

        connections.TryRemove(new KeyValuePair<Guid, SpectreWebsocket>(connectionId, connection));
        if (!connections.IsEmpty)
        {
            return false;
        }

        return ConnectionsByPlayerId.TryRemove(new KeyValuePair<Guid, ConcurrentDictionary<Guid, SpectreWebsocket>>(playerId, connections));
    }

    private async Task RegisterOfflineAsync()
    {
        try
        {
            Model.PlayerPresence presence = await Model.PlayerPresence.RetrieveFromDatabase(PlayerId)
                ?? Model.PlayerPresence.CreateDefault(PlayerId);
            presence.BasicStatus = Model.PlayerBasicPresence.Offline;
            presence.LastUpdatedTime = DateTimeOffset.UtcNow;
            await presence.SyncToDatabase();
            await FriendPresenceNotifications.SendPresenceUpdateToFriends(PlayerId, presence);
        }
        catch (Exception ex)
        {
            Log.Warning($"Failed to register player {PlayerId} offline: {ex.Message}");
        }
    }
}