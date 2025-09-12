namespace WIB.Domain;

public class ExpenseAggregate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Year { get; set; }
    public int Month { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal Amount { get; set; }
}
