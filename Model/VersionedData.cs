namespace Model;

public record class VersionedData
{
    public Int64 Version { get; set; }
    public void IncrementVersion()
    {
        Version++;
    }
}