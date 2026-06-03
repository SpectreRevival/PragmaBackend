using Serilog;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;

namespace Tests;

[TestClass]
public class HTTPTests
{
    public static IEnumerable<object[]> GetHTTPTestInstances()
    {
        string httpReqDir = Path.Combine(Path.Combine(AppContext.BaseDirectory, "testrequests"), "http");
        foreach (string filePath in Directory.EnumerateFiles(httpReqDir))
        {
            HTTPTestData? data = JsonDocument.Parse(File.ReadAllText(filePath)).Deserialize<HTTPTestData>();
            if (data == null)
            {
                Log.Warning($"Failed to deserialize file at {filePath} to HTTPTestData, skipping test");
                continue;
            }
            yield return new object[]
            {
                data
            };
        }
    }

    [TestMethod]
    [DynamicData(nameof(GetHTTPTestInstances))]
    public async Task RunHTTPTest(HTTPTestData testData)
    {
        using var cancelToken = new CancellationTokenSource();
        cancelToken.CancelAfter(TimeSpan.FromSeconds(10));
        HttpClient client = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        try
        {
            using var req = new HttpRequestMessage(new HttpMethod(testData.method), $"http://localhost:8080{testData.path}");
            req.Content = new StringContent(testData.request, Encoding.UTF8, "application/json");
            HttpResponseMessage res = await client.SendAsync(req, cancelToken.Token);
            res.EnsureSuccessStatusCode();
            string content = await res.Content.ReadAsStringAsync(cancelToken.Token);
            Assert.AreEqual(content, testData.response);
        }
        catch (OperationCanceledException ex) when (cancelToken.IsCancellationRequested)
        {
            if (ex.InnerException is TimeoutException)
            {
                Log.Error("Request timed out");
                Assert.Fail("Timed out");
                throw;
            }
        }
        catch (HttpRequestException ex)
        {
            Log.Error($"HTTP request failure: {ex.Message}");
            Assert.Fail();
            throw;
        }
    }
}