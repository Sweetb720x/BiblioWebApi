using BiblioWebApi.Application.Contracts;

namespace BiblioWebApi.Application.Abstractions;

public interface IBookBatchCreator
{
    Task<IReadOnlyCollection<BookResponse>> CreateAsync(CreateBookRequest book, int quantity, CancellationToken cancellationToken);
}
