namespace WIB.Domain;

public class ProductAlias
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public string Alias { get; set; } = string.Empty;
}
