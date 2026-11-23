namespace BiblioWebApi.Application.Abstractions;

public interface IBookBatchLimitStore
{
    Task<int> GetAsync(CancellationToken cancellationToken);
    Task<int> SetAsync(int maximum, CancellationToken cancellationToken);
}
