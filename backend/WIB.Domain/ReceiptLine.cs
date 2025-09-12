namespace WIB.Domain;

public class ReceiptLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReceiptId { get; set; }
    public Receipt? Receipt { get; set; }
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }
    public string LabelRaw { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal? VatRate { get; set; }
}
