namespace WIB.Domain;

public class PriceHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid StoreId { get; set; }
    public Store? Store { get; set; }
    public DateTime Date { get; set; }
    public decimal UnitPrice { get; set; }
}
