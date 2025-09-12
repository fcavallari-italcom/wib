namespace WIB.Domain;

public class ProductType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? AliasesJson { get; set; }
}
