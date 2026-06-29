using System.Text.Json.Nodes;

namespace Tests;

public static class JsonNodeExtensions
{
    public static void ReplaceToken(this JsonNode? node, string tokenToFind, string replacementValue)
    {
        if (node == null)
        {
            return;
        }

        // Case 1: The node is a JSON Object
        if (node is JsonObject jsonObject)
        {
            // Materialize keys to an array to avoid modifying the collection while iterating
            string[] keys = jsonObject.Select(p => p.Key).ToArray();

            foreach (string? key in keys)
            {
                JsonNode? childNode = jsonObject[key];

                if (childNode is JsonValue jsonValue && jsonValue.TryGetValue(out string? stringValue))
                {
                    if (stringValue == tokenToFind)
                    {
                        // Replace the exact matching string value
                        jsonObject[key] = JsonValue.Create(replacementValue);
                    }
                }
                else
                {
                    // Recurse deeper into child objects or arrays
                    childNode?.ReplaceToken(tokenToFind, replacementValue);
                }
            }
        }
        // Case 2: The node is a JSON Array
        else if (node is JsonArray jsonArray)
        {
            for (int i = 0; i < jsonArray.Count; i++)
            {
                JsonNode? childNode = jsonArray[i];

                if (childNode is JsonValue jsonValue && jsonValue.TryGetValue(out string? stringValue))
                {
                    if (stringValue == tokenToFind)
                    {
                        jsonArray[i] = JsonValue.Create(replacementValue);
                    }
                }
                else
                {
                    // Recurse deeper into objects or nested arrays
                    childNode?.ReplaceToken(tokenToFind, replacementValue);
                }
            }
        }
    }
}