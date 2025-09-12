using Microsoft.EntityFrameworkCore;
using WIB.Domain;

namespace WIB.Infrastructure;

public class WibDbContext : DbContext
{
    public WibDbContext(DbContextOptions<WibDbContext> options) : base(options) {}

    public DbSet<Store> Stores => Set<Store>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductAlias> ProductAliases => Set<ProductAlias>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<ReceiptLine> ReceiptLines => Set<ReceiptLine>();
    public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();
    public DbSet<BudgetMonth> BudgetMonths => Set<BudgetMonth>();
    public DbSet<ExpenseAggregate> ExpenseAggregates => Set<ExpenseAggregate>();
    public DbSet<LabelingEvent> LabelingEvents => Set<LabelingEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Category>().HasData(new Category
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "Uncategorized"
        });
        modelBuilder.Entity<ProductType>().HasData(new ProductType
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "Generic"
        });
    }
}
