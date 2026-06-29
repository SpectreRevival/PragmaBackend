using System.Diagnostics.CodeAnalysis;

namespace Processors;

public record class SpectreRpcType
{
    private static readonly BilateralMap<int, string> idsToNames = new(Path.Combine(AppContext.BaseDirectory, "resources", "rpctypes.json"), false);

    public required int TypeId { get; init; }
    [SetsRequiredMembers]
    public SpectreRpcType(int typeId)
    {
        TypeId = typeId;
    }
    [SetsRequiredMembers]
    public SpectreRpcType(string name)
    {
        TypeId = idsToNames.Reverse[name];
    }

    public SpectreRpcType GetResponseType()
    {
        string resName = GetName();
        if (resName.Length >= 7 && resName.EndsWith("Request"))
        {
            resName = resName[..^7];
        }
        resName += "Response";
        return new SpectreRpcType(resName);
    }

    public string GetName()
    {
        return idsToNames.Forward[TypeId];
    }

    public override string ToString()
    {
        return GetName();
    }
}