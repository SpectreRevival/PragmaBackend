using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class VersionedData
{
    [SetsRequiredMembers]
    public VersionedData(Int64 version)
    {
        Version = version;
    }

    public required Int64 Version { get; set; }
    public void IncrementVersion()
    {
        Version++;
    }
}