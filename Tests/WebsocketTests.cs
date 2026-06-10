using Microsoft.CodeCoverage.Core.Reports.Coverage;
using Packets;
using Packets.Processors;
using Serilog;
using System.Net.Http.Json;
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
            WebsocketTestData? testData = JsonDocument.Parse(File.ReadAllText(filePath)).Deserialize<WebsocketTestData>();
            yield return new object[]
            {
                filePath,
                testData.rpcType
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
        HttpClient client = new();
        var authRequest = new AuthenticateHandler.AuthenticateHandlerRequest(
            "STEAM",
            "14000000101B0A7F70C1FBBEE7E69F6C01001001B908BE67180000000100000002000000C1F9C3FB3D7764BEA0A8010001000000B20000003200000004000000E7E69F6C010010013E4E280061342A2D061C6E6400000000AD58BA672D08D66701007A730E00000000008561B7CAE2C32A312B14297D110EC706041E747242EC356EC06CC7CBED9E88B06ACE7C131B109603310CD4EBBD780C0CBD71DDFD43C7FDDB210DD69E69A76D7BE28F51C11FDEF5E04ACA018AB9333065E4340076F77F12BD906C57366CF6FD93931D52A9F0657279759865B5C60E99538904EBD23A3804FE02D594647943FE49",
            "00000000-0000-0000-0000-000000000001",
            "eyJraWQiOiJkM0p0T3E2ankzX0hxdXdUc3J6dDgxd2gzQkxpQS00Zi1xTThtai0wLVlRPSIsImFsZyI6IlJTMjU2IiwidHlwIjoiSldUIn0.eyJpc3MiOiJwcmFnbWEiLCJzdWIiOiIxMTYyOCIsImlhdCI6MTc0MDUwNzQyNywiZXhwIjoxNzQwNTkzODI3LCJqdGkiOiIyYjFiMzc5Yi1jZmQwLTRkMzAtYWE4NS02OWMyZWIyMWE4MWEiLCJ0aWNrZXROdW1iZXIiOiIxMTYyOCIsImlzQWxsb3dlZEluIjoidHJ1ZSJ9.PJp5MNXz2_xvCkq_XjzZeui1MvS9ylDLDgeLkJiv9jp_FVnTI9LISMtajHcef-7JehNs5sQC6P_Gpmb6JuVdD4k7HUX7a9IAgM8HKAagnfgmymn02SSpL7Mfz9wbH8FgOYU2ylKG_ExIW_aSG5HK588_waNeSydygwX2zRoSf8ZYZzbUHmMsZcG2iXpDq_Peejbt6Cgep9lsyNE5L5ZZzil9_KVu3FaEojcrI7tiPpHX7wi2K_J78rxmg2weUreowhv0VJA-YGqtOUlqFl7Ep8VGi-IrJdAf4gLeiVZMQoktc_g5tD9FgXzEAH_aDoBqGgoqnbKLcWLRiT1TAYGgXtCfw15Efh_ta-h4IIOI-DAnhJ1ujapd80Z87Wo6h7SpBaOitaI-bjBPkqDQGe2JooUNCrki848vPrfu0IQW00vawUtLX6LaS_aAEs0L2Vjxyebk1X37E9KwTDoxQGdmurutcnvSmVXOoO4P8F6o4oGx-A9d6HgFJl5rRie2LrWSJHlmcFm5_IKYw7okHwBh63Cx3mhUevji5SkEGj3gbwlBURjeEXpOm0qr-ECeKdmagbi_ipiiQB8m8FNwAbx9Z-Sl3nbJ-kS3QtPZrFHqxf91sgFY16H6sn1ruhna-ZygG5cYKf4JWbEcmLrSmdQ_xIBODjWDcatvNKGrv7Cx_Ng"
        );
        HttpResponseMessage authResponse = await client.PostAsJsonAsync("http://localhost:8081/v1/account/authenticateorcreatev2", authRequest);
        authResponse.EnsureSuccessStatusCode();
        AuthenticateHandler.AuthenticateHandlerResponse authResponseTokens = await authResponse.Content.ReadFromJsonAsync<AuthenticateHandler.AuthenticateHandlerResponse>();
        string[] skippedWsTests = File.ReadAllLines(Path.Combine(Path.Combine(AppContext.BaseDirectory, "testrequests"), "wsSkipTests.txt"));
        WebsocketTestData? testData = JsonDocument.Parse(File.ReadAllText(testDataFile)).Deserialize<WebsocketTestData>();
        Assert.IsNotNull(testData);
        if (skippedWsTests.Any(rpc => rpc == testData.rpcType))
        {
            Assert.Inconclusive($"Skipping because rpc {testData.rpcType} is in the skipped websocket tests list");
        }
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        using var ws = new ClientWebSocket();
        ws.Options.SetRequestHeader("Authorization", $"Bearer {authResponseTokens.pragmaTokens.pragmaGameToken}");
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
            if (res == null)
            {
                Assert.Fail();
                throw new InvalidDataException("Failed to convert res to SpectreWebsocketResponse json");
            }
            if(!JsonTestUtil.JsonMatchesSchema(res.response.payload.ToJsonString(), testData.responsePayload.ToJsonString(), testData.ignoreReplace == true, testData.ignoreAdd == true))
            {
                Assert.Fail("Json schema did not match");
            }
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Completed request", cts.Token);
            break;
        }
    }
}