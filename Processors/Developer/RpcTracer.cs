#if DO_RPC_TRACER
using Serilog;
using System.Collections.Concurrent;
using System.Text;

namespace Processors;

public sealed class RpcTracer
{
    private const int MaxBodyChars = 1200;
    private static readonly string TraceDir = Path.Combine(AppContext.BaseDirectory, "rpc_logs");
    private static readonly ConcurrentDictionary<Guid, object> PlayerFileLocks = new();

    private readonly Guid connectionId;
    private readonly Guid playerId;
    private readonly object playerFileLock;
    private readonly string filePath;
    private bool enabled;

    public RpcTracer(Guid connectionId, Guid playerId)
    {
        this.connectionId = connectionId;
        this.playerId = playerId;
        playerFileLock = PlayerFileLocks.GetOrAdd(playerId, _ => new object());
        filePath = Path.Combine(TraceDir, $"player_{playerId}.log");

        try
        {
            Directory.CreateDirectory(TraceDir);
            enabled = true;
        }
        catch (Exception ex)
        {
            enabled = false;
            Log.Warning("RpcTracer disabled, could not create trace dir: {Message}", ex.Message);
        }
    }

    public void Lifecycle(string eventName)
    {
        Write($"event={eventName} conn={connectionId} player={playerId}");
    }

    public void Rpc(string rpcType, int requestId, string requestPayload, string responsePayload,
        double processorMs, double responseSendMs, double totalMs, int notifications)
    {
        string line = $"event=RPC conn={connectionId} player={playerId} rpc={rpcType} reqId={requestId}"
                      + $" procMs={processorMs:F2} responseSendMs={responseSendMs:F2} totalMs={totalMs:F2} notifs={notifications}"
                      + $" reqLen={requestPayload.Length} respLen={responsePayload.Length}"
                      + $" req={Summarize(requestPayload)} resp={Summarize(responsePayload)}";
        Write(line);
    }

    public void ProcessorError(string rpcType, int requestId, double processorMs, string message)
    {
        Write(
            $"event=RPC_ERROR conn={connectionId} player={playerId} rpc={rpcType} reqId={requestId} procMs={processorMs:F2} error={Quote(message)}");
    }

    public void Notification(string rpcType, int sequence, string payload)
    {
        Write(
            $"event=NOTIFY conn={connectionId} player={playerId} rpc={rpcType} seq={sequence} len={payload.Length} body={Summarize(payload)}");
    }

    private static string Summarize(string body)
    {
        if (body.Length > MaxBodyChars)
        {
            body = body[..MaxBodyChars] + $"...<+{body.Length - MaxBodyChars}>";
        }

        return Quote(body);
    }

    private static string Quote(string value)
    {
        return "\"" + value.Replace("\r", " ").Replace("\n", " ") + "\"";
    }

    private void Write(string line)
    {
        Log.Information("RPC_TRACE {Line}", line);
        if (!enabled)
        {
            return;
        }

        string stamped = $"{DateTimeOffset.UtcNow:O} {line}{Environment.NewLine}";
        try
        {
            lock (playerFileLock)
            {
                File.AppendAllText(filePath, stamped, Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            enabled = false;
            Log.Warning("RpcTracer disabled after write failure: {Message}", ex.Message);
        }
    }
}
#endif