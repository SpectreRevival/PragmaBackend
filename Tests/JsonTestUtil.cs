using Json.Patch;
using JsonDiffPatchDotNet;
using JsonDiffPatchDotNet.Formatters;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tests;

public class JsonTestUtil
{
    public static bool JsonMatchesSchema(string response, string expectedResponse, bool ignoreReplace, bool ignoreAdd)
    {
        var resJson = JsonNode.Parse(response);
        var expectedJson = JsonNode.Parse(expectedResponse);
        JsonPatch patchDiff = resJson.CreatePatch(expectedJson);
        foreach (PatchOperation op in patchDiff.Operations)
        {
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
                if (op.Value.GetValue<string>() == "*")
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