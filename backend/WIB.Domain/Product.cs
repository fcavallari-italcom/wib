namespace WIB.Domain;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Gtin { get; set; }
    public Guid ProductTypeId { get; set; }
    public ProductType? ProductType { get; set; }
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    public ICollection<ProductAlias> Aliases { get; set; } = new List<ProductAlias>();
    public ICollection<PriceHistory> Prices { get; set; } = new List<PriceHistory>();
}
