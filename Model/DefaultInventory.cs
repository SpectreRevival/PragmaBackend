using System.Text.Json;
using System.Text.Json.Nodes;

namespace Model;

public record class DefaultInventory
{
    private static readonly DefaultInventory defaultData = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "defaults", "DefaultInventory.json")))
        .Deserialize<DefaultInventory>(new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });
    public required StackableItem[] StackableItems { get; set; }
    public required CustomizedInstancedItem[] CustomizedInstancedItems { get; set; }
    public required ProgressionTrackingItem[] ProgresionTrackingItems { get; set; }
    public required SponsorUnlockTrackerItem[] SponsorUnlockItems { get; set; }

    public static DefaultInventory Get()
    {
        return defaultData;
    }
}