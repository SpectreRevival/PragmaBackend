using Json.Patch;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tests;

public class JsonTestUtil
{
    public static JsonNode? GetByPath(JsonNode root, string path)
    {
        string[] parts = path.Split('/');

        JsonNode? current = root;

        foreach (string part in parts)
        {
            if (current is JsonObject obj)
            {
                if (!obj.TryGetPropertyValue(part, out current))
                {
                    return null;
                }
            }
            else if (current is JsonArray arr)
            {
                if (int.TryParse(part, out int index))
                {
                    if (index < 0 || index >= arr.Count)
                    {
                        return null;
                    }

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
        JsonNode? resJson = JsonNode.Parse(response);
        JsonNode? expectedJson = JsonNode.Parse(expectedResponse);
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
            Console.WriteLine($"- Path: {op.Path}\nOp: {op.Op}\nValue: {op.Value}");
        }
        foreach (PatchOperation op in patchDiff.Operations)
        {
            if (op.Op is OperationType.Remove or OperationType.Move)
            {
                return false;
            }
            if ((op.Op == OperationType.Add || op.Op == OperationType.Copy) && !ignoreAdd)
            {
                return false;
            }
            if (op.Op == OperationType.Replace)
            {
                JsonNode? node = GetByPath(expectedJson, op.Path.ToString()[1..]);
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