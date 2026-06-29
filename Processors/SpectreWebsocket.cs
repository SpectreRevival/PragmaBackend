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

public record class SpectreWebsocketNotification(SpectreRpcType RpcType, SpectreWebsocketMessage Payload);

public class SpectreWebsocket
{
    private static readonly ConcurrentDictionary<Guid, SpectreWebsocket> ConnectionsByPlayerId = new();
    public required Guid PlayerId { get; init; }
    private WebSocket Socket { get; init; }
    private int sequenceId = 0;
    private readonly SemaphoreSlim sendLock = new(1, 1);
    private readonly List<SpectreWebsocketNotification> postResponseNotifications = [];
    private readonly object postResponseNotificationsLock = new();
    private static readonly int MAX_BUFFER_SIZE = 32 * 1024 * 1024;

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
        ConnectionsByPlayerId[PlayerId] = this;
    }

    public void QueuePostResponseNotification(SpectreRpcType rpcType, SpectreWebsocketMessage payload)
    {
        lock (postResponseNotificationsLock)
        {
            postResponseNotifications.Add(new SpectreWebsocketNotification(rpcType, payload));
        }
    }

    private List<SpectreWebsocketNotification> DrainPostResponseNotifications()
    {
        lock (postResponseNotificationsLock)
        {
            List<SpectreWebsocketNotification> notifications = [.. postResponseNotifications];
            postResponseNotifications.Clear();
            return notifications;
        }
    }

    public async Task SendNotificationAsync(SpectreRpcType rpcType, SpectreWebsocketMessage payload, CancellationToken cancellationToken = default)
    {
        await sendLock.WaitAsync(cancellationToken);
        try
        {
            string fullMessage = "{\"sequenceNumber\":" + sequenceId.ToString() + ",\"notification\":{\"type\":\"" + rpcType.ToString() + "\",\"payload\":" + payload.GetData() + "}}";
            await Socket.SendAsync(Encoding.UTF8.GetBytes(fullMessage), WebSocketMessageType.Text, true, cancellationToken);
            sequenceId++;
        }
        finally
        {
            sendLock.Release();
        }
    }

    public static async Task<bool> SendNotificationToPlayerAsync(Guid playerId, SpectreRpcType rpcType, SpectreWebsocketMessage payload, CancellationToken cancellationToken = default)
    {
        if (!ConnectionsByPlayerId.TryGetValue(playerId, out SpectreWebsocket? connection))
        {
            return false;
        }

        await connection.SendNotificationAsync(rpcType, payload, cancellationToken);
        return true;
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
                    SpectreWebsocketRequest wsReq = new(JsonDocument.Parse(rawText));
                    WebsocketPacketProcessor? processor = WebsocketPacketProcessor.GetProcessorForRequestType(wsReq.RpcType);
                    if (processor is null)
                    {
                        Log.Warning($"No websocket processor recognized for rpc type {wsReq.RpcType}, skipping message");
                        continue;
                    }
                    else
                    {
                        SpectreWebsocketMessage messageOut = await processor.ProcessPacket(wsReq, this);
                        await SendResponseAsync(wsReq, messageOut, cancellationToken);
                        foreach (SpectreWebsocketNotification notification in DrainPostResponseNotifications())
                        {
                            await SendNotificationAsync(notification.RpcType, notification.Payload, cancellationToken);
                        }
                    }
                }
                else
                {
                    Log.Warning($"Unrecognized websocket message type received {result.MessageType}");
                }
            }
        }
        catch (WebSocketException ex)
        {
            Log.Error($"Websocket connection closed due to exception: {ex.Message}");
        }
        finally
        {
            ConnectionsByPlayerId.TryRemove(PlayerId, out _);
            sendLock.Dispose();
        }
    }
}