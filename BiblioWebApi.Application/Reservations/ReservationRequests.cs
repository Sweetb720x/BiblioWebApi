using BiblioWebApi.Application.Abstractions;
using BiblioWebApi.Application.Common;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using BiblioWebApi.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BiblioWebApi.Application.Reservations;

public sealed record ListReservationsQuery(bool PendingOnly = false) : IRequest<IReadOnlyCollection<Reservation>>;
public sealed record GetReservationQuery(int Id) : IRequest<Reservation>;
public sealed record ListUserReservationsQuery(int UserId) : IRequest<IReadOnlyCollection<Reservation>>;
public sealed record CreateReservationCommand(CreateReservationRequest Request) : IRequest<Reservation>, IValidatableRequest
{
    public IReadOnlyCollection<string> Validate() => Request.UserId <= 0 || Request.BookId <= 0 ? ["UserId and BookId must be positive."] : [];
}
public sealed record ApproveReservationCommand(int Id) : IRequest<Reservation>;
public sealed record ChangeReservationStatusCommand(int Id, ReservationStatus Status) : IRequest;
public sealed record CompleteReservationCommand(int Id) : IRequest;

internal static class ReservationQueries
{
    public static IQueryable<Reservation> WithDetails(IRepository<Reservation> reservations, bool tracking = false) => reservations.Query(!tracking).Include(x => x.Book).Include(x => x.User).Include(x => x.Copy);
}
public sealed class ListReservationsHandler(IRepository<Reservation> reservations) : IRequestHandler<ListReservationsQuery, IReadOnlyCollection<Reservation>>
{
    public async Task<IReadOnlyCollection<Reservation>> Handle(ListReservationsQuery request, CancellationToken ct) => (await ReservationQueries.WithDetails(reservations).Where(x => !request.PendingOnly || x.Status == ReservationStatus.Pending).ToListAsync(ct)).ToArray();
}
public sealed class GetReservationHandler(IRepository<Reservation> reservations) : IRequestHandler<GetReservationQuery, Reservation>
{
    public async Task<Reservation> Handle(GetReservationQuery request, CancellationToken ct) => await ReservationQueries.WithDetails(reservations).SingleOrDefaultAsync(x => x.Id == request.Id, ct) ?? throw new KeyNotFoundException("RESERVATION_NOT_FOUND");
}
public sealed class ListUserReservationsHandler(IRepository<Reservation> reservations) : IRequestHandler<ListUserReservationsQuery, IReadOnlyCollection<Reservation>>
{
    public async Task<IReadOnlyCollection<Reservation>> Handle(ListUserReservationsQuery request, CancellationToken ct) => (await ReservationQueries.WithDetails(reservations).Where(x => x.UserId == request.UserId).ToListAsync(ct)).ToArray();
}
public sealed class CreateReservationHandler(IRepository<User> users, IRepository<Book> books, IRepository<BookCopy> copies, IRepository<Reservation> reservations, IUnitOfWork unitOfWork, ILibraryRules library) : IRequestHandler<CreateReservationCommand, Reservation>
{
    public async Task<Reservation> Handle(CreateReservationCommand command, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(command.Request.UserId, ct) ?? throw new KeyNotFoundException("USER_NOT_FOUND");
        var book = await books.GetByIdAsync(command.Request.BookId, ct) ?? throw new KeyNotFoundException("BOOK_NOT_FOUND");
        var allowed = await library.CanReserveAsync(user, book.Id, ct); if (!allowed.Allowed) throw new InvalidOperationException(allowed.Code);
        if (!await copies.Query().AnyAsync(x => x.BookId == book.Id && x.Status == CopyStatus.Available, ct)) throw new InvalidOperationException("BOOK_NOT_AVAILABLE");
        var reservation = new Reservation { UserId = user.Id, BookId = book.Id, User = user, Book = book }; await reservations.AddAsync(reservation, ct); await unitOfWork.SaveChangesAsync(ct); return reservation;
    }
}
public sealed class ApproveReservationHandler(IRepository<Reservation> reservations, IRepository<BookCopy> copies, IUnitOfWork unitOfWork, ILibraryRules library) : IRequestHandler<ApproveReservationCommand, Reservation>
{
    public async Task<Reservation> Handle(ApproveReservationCommand command, CancellationToken ct)
    {
        var reservation = await ReservationQueries.WithDetails(reservations, true).SingleOrDefaultAsync(x => x.Id == command.Id, ct) ?? throw new KeyNotFoundException("RESERVATION_NOT_FOUND");
        var copy = await copies.Query(false).FirstOrDefaultAsync(x => x.BookId == reservation.BookId && x.Status == CopyStatus.Available, ct) ?? throw new InvalidOperationException("BOOK_NOT_AVAILABLE");
        reservation.Status = ReservationStatus.Approved; reservation.CopyId = copy.Id; reservation.Copy = copy; reservation.ApprovedAt = DateTime.UtcNow; reservation.PickupExpiresAt = DateTime.UtcNow.AddHours(library.PickupHours); copy.Status = CopyStatus.Reserved; await unitOfWork.SaveChangesAsync(ct); return reservation;
    }
}
public sealed class ChangeReservationStatusHandler(IRepository<Reservation> reservations, IUnitOfWork unitOfWork) : IRequestHandler<ChangeReservationStatusCommand>
{
    public async Task Handle(ChangeReservationStatusCommand command, CancellationToken ct) { var reservation = await ReservationQueries.WithDetails(reservations, true).SingleOrDefaultAsync(x => x.Id == command.Id, ct) ?? throw new KeyNotFoundException("RESERVATION_NOT_FOUND"); reservation.Status = command.Status; if (command.Status is ReservationStatus.Cancelled or ReservationStatus.Rejected or ReservationStatus.Expired) if (reservation.Copy?.Status == CopyStatus.Reserved) reservation.Copy.Status = CopyStatus.Available; await unitOfWork.SaveChangesAsync(ct); }
}
public sealed class CompleteReservationHandler(IRepository<Reservation> reservations, IUnitOfWork unitOfWork) : IRequestHandler<CompleteReservationCommand>
{
    public async Task Handle(CompleteReservationCommand command, CancellationToken ct) { var reservation = await ReservationQueries.WithDetails(reservations, true).SingleOrDefaultAsync(x => x.Id == command.Id, ct) ?? throw new KeyNotFoundException("RESERVATION_NOT_FOUND"); reservation.Status = ReservationStatus.PickedUp; if (reservation.Copy is not null) reservation.Copy.Status = CopyStatus.OnLoan; await unitOfWork.SaveChangesAsync(ct); }
}




