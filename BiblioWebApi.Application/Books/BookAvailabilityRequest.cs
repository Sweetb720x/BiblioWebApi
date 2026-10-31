using BiblioWebApi.Application.Abstractions;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BiblioWebApi.Application.Books;

public sealed record GetBookAvailabilityQuery(int Id) : IRequest<object>;

public sealed class GetBookAvailabilityHandler(IRepository<Book> books) : IRequestHandler<GetBookAvailabilityQuery, object>
{
    public async Task<object> Handle(GetBookAvailabilityQuery request, CancellationToken ct)
    {
        var book = await books.Query().Include(x => x.Copies).SingleOrDefaultAsync(x => x.Id == request.Id && x.IsActive, ct) ?? throw new KeyNotFoundException("BOOK_NOT_FOUND");
        return new
        {
            book.Id,
            book.Title,
            totalCopies = book.Copies.Count,
            availableCopies = book.Copies.Count(x => x.Status == CopyStatus.Available),
            reservedCopies = book.Copies.Count(x => x.Status == CopyStatus.Reserved),
            loanedCopies = book.Copies.Count(x => x.Status == CopyStatus.OnLoan),
            maintenanceCopies = book.Copies.Count(x => x.Status == CopyStatus.Maintenance)
        };
    }
}




