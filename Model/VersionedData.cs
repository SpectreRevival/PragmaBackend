using System.Diagnostics.CodeAnalysis;

namespace Model;

public record class VersionedData
{
    [SetsRequiredMembers]
    public VersionedData(long version)
    {
        Version = version;
    }

    public required long Version { get; set; }
    public void IncrementVersion()
    {
        Version++;
    }
}