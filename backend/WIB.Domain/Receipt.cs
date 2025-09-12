namespace WIB.Domain;

public class Receipt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StoreId { get; set; }
    public Store? Store { get; set; }
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public decimal? TaxTotal { get; set; }
    public string Currency { get; set; } = "EUR";
    public string? RawText { get; set; }
    public ICollection<ReceiptLine> Lines { get; set; } = new List<ReceiptLine>();
}
