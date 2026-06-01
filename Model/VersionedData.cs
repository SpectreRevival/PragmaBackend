namespace Model;

public class VersionedData
{
    public Int64 Version { get; set; }
    public void IncrementVersion()
    {
        Version++;
    }
}