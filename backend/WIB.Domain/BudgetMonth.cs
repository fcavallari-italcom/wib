namespace WIB.Domain;

public class BudgetMonth
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal LimitAmount { get; set; }
}
