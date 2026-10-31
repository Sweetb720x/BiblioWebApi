using BiblioWebApi.Application.Abstractions;
using BiblioWebApi.Application.Common;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BiblioWebApi.Application.Administration;

public sealed record ListFinesQuery : IRequest<IReadOnlyCollection<Fine>>;
public sealed record GetFineQuery(int Id) : IRequest<Fine>;
public sealed record ListUserFinesQuery(int UserId) : IRequest<IReadOnlyCollection<Fine>>;
public sealed record CreateFineCommand(FineRequest Request) : IRequest<Fine>, IValidatableRequest
{
    public IReadOnlyCollection<string> Validate() => Request.UserId <= 0 || Request.Amount < 0 || string.IsNullOrWhiteSpace(Request.Reason) ? ["UserId, non-negative Amount and Reason are required."] : [];
}
public sealed record SettleFineCommand(int Id, FineStatus Status) : IRequest;
public sealed record GetAccountStatusQuery(int UserId) : IRequest<object>;
public sealed record DashboardSummaryQuery : IRequest<object>;
public sealed record RecentActivityQuery : IRequest<object>;
public sealed record DashboardAlertsQuery : IRequest<object>;
public sealed record LoansByStatusQuery : IRequest<object>;
public sealed record BooksAvailabilityQuery : IRequest<object>;
public sealed record DashboardOverdueQuery : IRequest<object>;

public sealed class ListFinesHandler(IRepository<Fine> fines) : IRequestHandler<ListFinesQuery, IReadOnlyCollection<Fine>>
{
    public async Task<IReadOnlyCollection<Fine>> Handle(ListFinesQuery request, CancellationToken ct) => (await fines.Query().Include(x => x.User).ToListAsync(ct)).ToArray();
}
public sealed class GetFineHandler(IRepository<Fine> fines) : IRequestHandler<GetFineQuery, Fine>
{
    public async Task<Fine> Handle(GetFineQuery request, CancellationToken ct) => await fines.Query().Include(x => x.User).SingleOrDefaultAsync(x => x.Id == request.Id, ct) ?? throw new KeyNotFoundException("FINE_NOT_FOUND");
}
public sealed class ListUserFinesHandler(IRepository<Fine> fines) : IRequestHandler<ListUserFinesQuery, IReadOnlyCollection<Fine>>
{
    public async Task<IReadOnlyCollection<Fine>> Handle(ListUserFinesQuery request, CancellationToken ct) => (await fines.Query().Where(x => x.UserId == request.UserId).ToListAsync(ct)).ToArray();
}
public sealed class CreateFineHandler(IRepository<User> users, IRepository<Fine> fines, IUnitOfWork unitOfWork) : IRequestHandler<CreateFineCommand, Fine>
{
    public async Task<Fine> Handle(CreateFineCommand command, CancellationToken ct) { if (await users.GetByIdAsync(command.Request.UserId, ct) is null) throw new KeyNotFoundException("USER_NOT_FOUND"); var fine = new Fine { UserId = command.Request.UserId, Amount = command.Request.Amount, Reason = command.Request.Reason, LoanId = command.Request.LoanId }; await fines.AddAsync(fine, ct); await unitOfWork.SaveChangesAsync(ct); return fine; }
}
public sealed class SettleFineHandler(IRepository<Fine> fines, IUnitOfWork unitOfWork) : IRequestHandler<SettleFineCommand>
{
    public async Task Handle(SettleFineCommand command, CancellationToken ct) { var fine = await fines.GetByIdAsync(command.Id, ct) ?? throw new KeyNotFoundException("FINE_NOT_FOUND"); fine.Status = command.Status; fine.SettledAt = DateTime.UtcNow; await unitOfWork.SaveChangesAsync(ct); }
}
public sealed class GetAccountStatusHandler(IRepository<Fine> fines, IRepository<Loan> loans) : IRequestHandler<GetAccountStatusQuery, object>
{
    public async Task<object> Handle(GetAccountStatusQuery request, CancellationToken ct) { var pending = await fines.Query().CountAsync(x => x.UserId == request.UserId && x.Status == FineStatus.Pending, ct); var overdue = await loans.Query().CountAsync(x => x.UserId == request.UserId && (x.Status == LoanStatus.Overdue || x.Status == LoanStatus.Active && x.DueDate < DateTime.UtcNow), ct); return new { userId = request.UserId, blocked = pending > 0 || overdue > 0, pendingFines = pending, overdueLoans = overdue }; }
}
public sealed class DashboardSummaryHandler(IRepository<Loan> loans, IRepository<BookCopy> copies, IRepository<User> users) : IRequestHandler<DashboardSummaryQuery, object>
{
    public async Task<object> Handle(DashboardSummaryQuery request, CancellationToken ct) => new { activeLoans = await loans.Query().CountAsync(x => x.Status == LoanStatus.Active || x.Status == LoanStatus.Overdue, ct), availableBooks = await copies.Query().CountAsync(x => x.Status == CopyStatus.Available, ct), activeUsers = await users.Query().CountAsync(x => x.Status == UserStatus.Active, ct), overdueLoans = await loans.Query().CountAsync(x => x.Status == LoanStatus.Overdue || x.Status == LoanStatus.Active && x.DueDate < DateTime.UtcNow, ct), recentActivity = Array.Empty<object>(), alerts = Array.Empty<object>() };
}
public sealed class RecentActivityHandler(IRepository<Loan> loans) : IRequestHandler<RecentActivityQuery, object>
{
    public async Task<object> Handle(RecentActivityQuery request, CancellationToken ct) => await loans.Query().OrderByDescending(x => x.BorrowedAt).Take(20).Select(x => new { x.Id, x.UserId, x.CopyId, x.BorrowedAt, x.Status }).ToListAsync(ct);
}
public sealed class DashboardAlertsHandler(IRepository<Loan> loans, IRepository<Reservation> reservations, IRepository<Fine> fines) : IRequestHandler<DashboardAlertsQuery, object>
{
    public async Task<object> Handle(DashboardAlertsQuery request, CancellationToken ct) => new { overdueLoans = await loans.Query().CountAsync(x => x.Status == LoanStatus.Overdue || x.Status == LoanStatus.Active && x.DueDate < DateTime.UtcNow, ct), pendingReservations = await reservations.Query().CountAsync(x => x.Status == ReservationStatus.Pending, ct), pendingFines = await fines.Query().CountAsync(x => x.Status == FineStatus.Pending, ct) };
}
public sealed class LoansByStatusHandler(IRepository<Loan> loans) : IRequestHandler<LoansByStatusQuery, object>
{
    public async Task<object> Handle(LoansByStatusQuery request, CancellationToken ct) => await loans.Query().GroupBy(x => x.Status).Select(x => new { status = x.Key.ToString(), count = x.Count() }).ToListAsync(ct);
}
public sealed class BooksAvailabilityHandler(IRepository<Book> books) : IRequestHandler<BooksAvailabilityQuery, object>
{
    public async Task<object> Handle(BooksAvailabilityQuery request, CancellationToken ct) => await books.Query().Include(x => x.Copies).Where(x => x.IsActive).Select(x => new { x.Id, x.Title, total = x.Copies.Count, available = x.Copies.Count(c => c.Status == CopyStatus.Available) }).ToListAsync(ct);
}
public sealed class DashboardOverdueHandler(IRepository<Loan> loans) : IRequestHandler<DashboardOverdueQuery, object>
{
    public async Task<object> Handle(DashboardOverdueQuery request, CancellationToken ct) => new { count = await loans.Query().CountAsync(x => x.Status == LoanStatus.Overdue || x.Status == LoanStatus.Active && x.DueDate < DateTime.UtcNow, ct) };
}




