using Microsoft.CodeCoverage.Core.Reports.Coverage;
using Packets;
using Serilog;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tests;

[TestClass]
public class WebsocketTests
{
    public static IEnumerable<object[]> GetWebsocketTestInstances()
    {
        string wsReqDir = Path.Combine(Path.Combine(AppContext.BaseDirectory, "testrequests"), "ws");
        foreach (string filePath in Directory.EnumerateFiles(wsReqDir))
        {
            yield return new object[]
            {
                filePath,
                Path.GetFileNameWithoutExtension(filePath)
            };
        }
    }

    public static string GetCustomTestName(MethodInfo methodInfo, object[] data)
    {
        if (data != null && data.Length > 1 && data[1] is string testName)
        {
            return $"{methodInfo.Name}_{testName}";
        }
        return methodInfo.Name;
    }

    [TestMethod]
    [DynamicData(nameof(GetWebsocketTestInstances), DynamicDataDisplayName = nameof(GetCustomTestName))]
    public async Task RunWebsocketTest(string testDataFile, string testName)
    {
        WebsocketTestData? testData = JsonDocument.Parse(File.ReadAllText(testDataFile)).Deserialize<WebsocketTestData>();
        Assert.IsNotNull(testData);
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        using var ws = new ClientWebSocket();
        await ws.ConnectAsync(new Uri("ws://localhost:8081/"), cts.Token);

        JsonObject fullSendMessage = new();
        fullSendMessage["requestId"] = 0;
        fullSendMessage["type"] = testData.rpcType;
        fullSendMessage["payload"] = testData.requestBody;

        byte[] sendBuf = Encoding.UTF8.GetBytes(fullSendMessage.ToJsonString());
        var sendSegment = new ArraySegment<byte>(sendBuf);

        await ws.SendAsync(sendSegment, WebSocketMessageType.Text, endOfMessage: true, cts.Token);

        while (!cts.IsCancellationRequested)
        {
            byte[] recvBuf = new byte[16 * 1024 * 1024];
            using var memStreamer = new MemoryStream();
            WebSocketReceiveResult result;

            do
            {
                var receiveSegment = new ArraySegment<byte>(recvBuf);
                result = await ws.ReceiveAsync(receiveSegment, cts.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Log.Error("Remote closed the websocket");
                    Assert.Fail();
                    throw new WebSocketException("Websocket channel closed");
                }
                memStreamer.Write(recvBuf, 0, result.Count);
            } while (!result.EndOfMessage);

            memStreamer.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(memStreamer, Encoding.UTF8);
            string resText = await reader.ReadToEndAsync(cts.Token);
            if (resText.Contains("notification")) continue; // skip notifications
            SpectreWebsocketResponse? res = JsonDocument.Parse(resText).Deserialize<SpectreWebsocketResponse>();
            if(res == null)
            {
                Assert.Fail();
                throw new InvalidDataException("Failed to convert res to SpectreWebsocketResponse json");
            }
            Assert.AreEqual(res.response.payload, testData.responsePayload);
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Completed request", cts.Token);
            break;
        }
    }
}