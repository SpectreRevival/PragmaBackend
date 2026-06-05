using Microsoft.CodeCoverage.Core.Reports.Coverage;
using Serilog;
using System.Linq.Expressions;
using System.Reflection;
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
    [DynamicData(nameof(GetHTTPTestInstances), DynamicDataDisplayName = nameof(GetCustomTestName))]
    public async Task RunHTTPTest(string testDataFile, string testName)
    {
        HTTPTestData? testData = JsonDocument.Parse(File.ReadAllText(testDataFile)).Deserialize<HTTPTestData>();
        Assert.IsNotNull(testData);
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
    }
}