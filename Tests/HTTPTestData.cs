namespace Tests;

public record class HTTPTestData
{
    public required string method { get; set; }
    public required string path { get; set; }
    public required string request { get; set; }
    public required string response { get; set; }
    public required string testAge { get; set; }
}