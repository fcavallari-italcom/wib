namespace WIB.Domain;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Gtin { get; set; }
    public Guid ProductTypeId { get; set; }
    public Guid? CategoryId { get; set; }
}
