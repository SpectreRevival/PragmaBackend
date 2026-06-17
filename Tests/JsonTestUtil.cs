using Json.Patch;
using JsonDiffPatchDotNet;
using JsonDiffPatchDotNet.Formatters;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tests;

public class JsonTestUtil
{
    public static JsonNode? GetByPath(JsonNode root, string path)
    {
        var parts = path.Split('/');

        JsonNode? current = root;

        foreach (var part in parts)
        {
            if (current is JsonObject obj)
            {
                if (!obj.TryGetPropertyValue(part, out current))
                    return null;
            }
            else if (current is JsonArray arr)
            {
                if (int.TryParse(part, out int index))
                {
                    if (index < 0 || index >= arr.Count)
                        return null;

                    current = arr[index];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        return current;
    }

    public static bool JsonMatchesSchema(string response, string expectedResponse, bool ignoreReplace, bool ignoreAdd)
    {
        var resJson = JsonNode.Parse(response);
        var expectedJson = JsonNode.Parse(expectedResponse);
        JsonPatch patchDiff = expectedJson.CreatePatch(resJson);
        Console.WriteLine($"Actual response: {resJson.ToJsonString(new JsonSerializerOptions()
        {
            WriteIndented = true
        })}");
        Console.WriteLine($"Expected response: {expectedJson.ToJsonString(new JsonSerializerOptions()
        {
            WriteIndented = true
        })}");
        Console.WriteLine("Diff: ");
        foreach (PatchOperation op in patchDiff.Operations)
        {
            Console.WriteLine($"- Path: {op.Path}\nOp: {op.Op.ToString()}\nValue: {op.Value}");
            if (op.Op == OperationType.Remove || op.Op == OperationType.Move)
            {
                return false;
            }
            if ((op.Op == OperationType.Add || op.Op == OperationType.Copy) && !ignoreAdd)
            {
                return false;
            }
            if (op.Op == OperationType.Replace)
            {
                JsonNode? node = GetByPath(expectedJson, op.Path.ToString().Substring(1));
                if (node.ToString() == "*")
                {
                    continue;
                }
                if (ignoreReplace)
                {
                    continue;
                }
                return false;
            }
        }
        return true;
    }
}