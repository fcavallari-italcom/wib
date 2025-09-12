using WIB.Application.Interfaces;
using WIB.Domain;

namespace WIB.Infrastructure.Clients;

public class ReceiptStorage : IReceiptStorage
{
    public Task<Receipt> SaveAsync(Receipt receipt, CancellationToken ct)
        => Task.FromResult(receipt);
}
