using BiblioWebApi.Domain;

namespace BiblioWebApi.Application.Services;

public interface ILibraryRules
{
    int LoanDays { get; }
    int PickupHours { get; }
    Task<(bool Allowed, string Code, string Message)> CanBorrowAsync(User user, CancellationToken cancellationToken);
    Task<(bool Allowed, string Code, string Message)> CanReserveAsync(User user, int bookId, CancellationToken cancellationToken);
}


