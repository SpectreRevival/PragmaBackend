namespace Model;

public interface IItem;

public record class StackableItem : IItem
{
    public required string CatalogId { get; set; }
    public required string InstanceId { get; set; }
    public required Int64 Amount { get; set; }
}

public record 