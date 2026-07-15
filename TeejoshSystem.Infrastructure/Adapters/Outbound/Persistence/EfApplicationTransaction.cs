using TeejoshSystem.Domain.Ports.Outbound;

namespace TeejoshSystem.Infrastructure.Adapters.Outbound.Persistence;

public sealed class EfApplicationTransaction : IApplicationTransaction
{
    private readonly InventarioDbContext _db;

    public EfApplicationTransaction(InventarioDbContext db) => _db = db;

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, Func<T, bool> shouldCommit, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation();
            if (shouldCommit(result)) await transaction.CommitAsync(cancellationToken);
            else await transaction.RollbackAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
