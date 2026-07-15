namespace TeejoshSystem.Domain.Ports.Outbound;

public interface IApplicationTransaction
{
    Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        Func<T, bool> shouldCommit,
        CancellationToken cancellationToken = default);
}
