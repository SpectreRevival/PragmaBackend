#if DO_RPC_TRACER
using System.Diagnostics;

namespace Processors;

public partial class SpectreWebsocket
{
    private RpcTracer? tracer;
    private long requestStartTimestamp;
    private long processorDoneTimestamp;
    private long responseSentTimestamp;

    partial void InitTracing()
    {
        tracer = new RpcTracer(ConnectionId, PlayerId);
    }

    partial void TraceConnect()
    {
        tracer?.Lifecycle("CONNECT");
    }

    partial void TraceDisconnect()
    {
        tracer?.Lifecycle("DISCONNECT");
    }

    partial void TraceRequestReceived(string rpcType, int requestId)
    {
        requestStartTimestamp = Stopwatch.GetTimestamp();
        processorDoneTimestamp = 0;
        responseSentTimestamp = 0;
    }

    partial void TraceAfterProcess()
    {
        processorDoneTimestamp = Stopwatch.GetTimestamp();
    }

    partial void TraceAfterResponse()
    {
        responseSentTimestamp = Stopwatch.GetTimestamp();
    }

    partial void TraceRpcComplete(string rpcType, int requestId, string requestPayload, string responsePayload,
        int notifications)
    {
        long done = responseSentTimestamp == 0 ? Stopwatch.GetTimestamp() : responseSentTimestamp;
        tracer?.Rpc(
            rpcType,
            requestId,
            requestPayload,
            responsePayload,
            ElapsedMs(requestStartTimestamp, processorDoneTimestamp),
            ElapsedMs(processorDoneTimestamp, done),
            ElapsedMs(requestStartTimestamp, done),
            notifications);
    }

    partial void TraceProcessorError(string rpcType, int requestId, string message)
    {
        long now = Stopwatch.GetTimestamp();
        tracer?.ProcessorError(rpcType, requestId,
            requestStartTimestamp == 0 ? 0 : ElapsedMs(requestStartTimestamp, now), message);
    }

    partial void TraceNotification(string rpcType, int sequence, string payload)
    {
        tracer?.Notification(rpcType, sequence, payload);
    }

    private static double ElapsedMs(long from, long to)
    {
        return from == 0 || to == 0 ? 0 : (to - from) * 1000.0 / Stopwatch.Frequency;
    }
}
#endif