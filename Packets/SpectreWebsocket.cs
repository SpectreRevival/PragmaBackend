using Google.Protobuf;
using Google.Protobuf.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Packets;

public class SpectreWebsocket
{
    public required Guid PlayerId { get; init; }
    private WebSocket Socket { get; init; }
    private Int32 sequenceId = 0;
    private readonly SemaphoreSlim sendLock = new(1, 1);
    private static readonly Int32 MAX_BUFFER_SIZE = 32 * 1024 * 1024;
    private static readonly JsonFormatter outFormatter = new(
        new JsonFormatter.Settings(true)
        .WithFormatDefaultValues(true)
        .WithFormatEnumsAsIntegers(true)
        .WithIndentation("")
        .WithPreserveProtoFieldNames(true)
    );

    [SetsRequiredMembers]
    public SpectreWebsocket(HttpContext upgradeRequest, WebSocket webSocket)
    {
        Socket = webSocket;
        var token = upgradeRequest.Request.Headers.Authorization;
        if (StringValues.IsNullOrEmpty(token))
        {
            throw new InvalidDataException("tried to start websocket connection without authorization header");
        }
        string bearer = token.ToString();
        if(!bearer.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("auth header was not a bearer token");
        }
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer);
        Guid? possiblePID = Guid.Parse(jwt.Claims.FirstOrDefault(c => c.Type == "pragmaPlayerId")?.Value ?? throw new InvalidDataException("failed to get value for pragmaPlayerId claim"));
        if (possiblePID == null)
        {
            throw new InvalidDataException("No valid GUID on pragmaPlayerId claim");
        }
        PlayerId = (Guid)possiblePID;
    }

    public async Task HandleAsync(CancellationToken cancellationToken = default)
    {
        byte[] recv = new byte[MAX_BUFFER_SIZE];
        
        try
        {
            while(Socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                using var memStream = new MemoryStream();
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
                if(result.MessageType == WebSocketMessageType.Text)
                {
                    string rawText = Encoding.UTF8.GetString(memStream.ToArray());
                    SpectreWebsocketRequest wsReq = new(JsonDocument.Parse(rawText));
                    WebsocketPacketProcessor? processor = WebsocketPacketProcessor.GetProcessorForRequestType(wsReq.RpcType);
                    if (processor is null)
                    {
                        Log.Warning($"No websocket processor recognized for rpc type {wsReq.RpcType}, skipping message");
                        continue;
                    } else
                    {
                        IMessage messageOut = await processor.ProcessPacket(wsReq, this);
                        await sendLock.WaitAsync(cancellationToken);
                        string payloadString = outFormatter.Format(messageOut);
                        string fullMessage = "{\"sequenceNumber\":" + sequenceId.ToString() + ",\"response\":{\"requestId\":" + wsReq.RequestId.ToString() + ",\"type\":\"" + wsReq.RpcType.GetResponseType().ToString() + "\",\"payload\":" + payloadString + "}}";
                        Socket.SendAsync(Encoding.UTF8.GetBytes(fullMessage), WebSocketMessageType.Text, true, cancellationToken);
                        sequenceId++;
                        sendLock.Release();
                    }
                } else
                {
                    Log.Warning($"Unrecognized websocket message type received {result.MessageType}");
                }
            }
        } catch (WebSocketException ex)
        {
            Log.Error($"Websocket connection closed due to exception: {ex.Message}");
        } finally
        {
            sendLock.Dispose();
        }
    }
}