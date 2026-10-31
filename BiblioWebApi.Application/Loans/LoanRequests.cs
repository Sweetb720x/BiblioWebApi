using BiblioWebApi.Application.Abstractions;
using BiblioWebApi.Application.Common;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using BiblioWebApi.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BiblioWebApi.Application.Loans;

public sealed record ListLoansQuery : IRequest<IReadOnlyCollection<LoanResponse>>;
public sealed record ListOverdueLoansQuery : IRequest<IReadOnlyCollection<LoanResponse>>;
public sealed record GetLoanQuery(int Id) : IRequest<LoanResponse>;
public sealed record ListUserLoansQuery(int UserId) : IRequest<IReadOnlyCollection<LoanResponse>>;
public sealed record CreateLoanCommand(CreateLoanRequest Request) : IRequest<LoanResponse>, IValidatableRequest
{
    public IReadOnlyCollection<string> Validate() => Request.DueDate <= DateTime.UtcNow ? ["DueDate must be in the future."] : [];
}
public sealed record ReturnLoanCommand(int Id) : IRequest;
public sealed record RenewLoanCommand(int Id) : IRequest<LoanResponse>;
public sealed record CancelLoanCommand(int Id) : IRequest;

internal static class LoanQueries
{
    public static IQueryable<Loan> WithDetails(IRepository<Loan> loans, bool tracking = false) => loans.Query(!tracking).Include(x => x.Copy).ThenInclude(x => x.Book);
}

public sealed class ListLoansHandler(IRepository<Loan> loans) : IRequestHandler<ListLoansQuery, IReadOnlyCollection<LoanResponse>>
{
    public async Task<IReadOnlyCollection<LoanResponse>> Handle(ListLoansQuery request, CancellationToken ct) => (await LoanQueries.WithDetails(loans).ToListAsync(ct)).Select(ApplicationMappings.ToLoanResponse).ToArray();
}
public sealed class ListOverdueLoansHandler(IRepository<Loan> loans) : IRequestHandler<ListOverdueLoansQuery, IReadOnlyCollection<LoanResponse>>
{
    public async Task<IReadOnlyCollection<LoanResponse>> Handle(ListOverdueLoansQuery request, CancellationToken ct) => (await LoanQueries.WithDetails(loans).Where(x => x.Status == LoanStatus.Overdue || x.Status == LoanStatus.Active && x.DueDate < DateTime.UtcNow).ToListAsync(ct)).Select(ApplicationMappings.ToLoanResponse).ToArray();
}
public sealed class GetLoanHandler(IRepository<Loan> loans) : IRequestHandler<GetLoanQuery, LoanResponse>
{
    public async Task<LoanResponse> Handle(GetLoanQuery request, CancellationToken ct) => ApplicationMappings.ToLoanResponse(await LoanQueries.WithDetails(loans).SingleOrDefaultAsync(x => x.Id == request.Id, ct) ?? throw new KeyNotFoundException("LOAN_NOT_FOUND"));
}
public sealed class ListUserLoansHandler(IRepository<Loan> loans) : IRequestHandler<ListUserLoansQuery, IReadOnlyCollection<LoanResponse>>
{
    public async Task<IReadOnlyCollection<LoanResponse>> Handle(ListUserLoansQuery request, CancellationToken ct) => (await LoanQueries.WithDetails(loans).Where(x => x.UserId == request.UserId).ToListAsync(ct)).Select(ApplicationMappings.ToLoanResponse).ToArray();
}
public sealed class CreateLoanHandler(IRepository<User> users, IRepository<BookCopy> copies, IRepository<Loan> loans, IUnitOfWork unitOfWork, ILibraryRules library) : IRequestHandler<CreateLoanCommand, LoanResponse>
{
    public async Task<LoanResponse> Handle(CreateLoanCommand command, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(command.Request.UserId, ct) ?? throw new KeyNotFoundException("USER_NOT_FOUND");
        var copy = await copies.Query(false).Include(x => x.Book).SingleOrDefaultAsync(x => x.Id == command.Request.CopyId, ct) ?? throw new KeyNotFoundException("COPY_NOT_FOUND");
        var allowed = await library.CanBorrowAsync(user, ct); if (!allowed.Allowed) throw new InvalidOperationException(allowed.Code);
        if (copy.Status != CopyStatus.Available) throw new InvalidOperationException("BOOK_NOT_AVAILABLE");
        copy.Status = CopyStatus.OnLoan;
        var loan = new Loan { UserId = user.Id, CopyId = copy.Id, DueDate = command.Request.DueDate, Status = LoanStatus.Active };
        await loans.AddAsync(loan, ct); await unitOfWork.SaveChangesAsync(ct); loan.Copy = copy; return ApplicationMappings.ToLoanResponse(loan);
    }
}
public sealed class ReturnLoanHandler(IRepository<Loan> loans, IUnitOfWork unitOfWork) : IRequestHandler<ReturnLoanCommand>
{
    public async Task Handle(ReturnLoanCommand command, CancellationToken ct) { var loan = await loans.Query(false).Include(x => x.Copy).SingleOrDefaultAsync(x => x.Id == command.Id, ct) ?? throw new KeyNotFoundException("LOAN_NOT_FOUND"); if (loan.Status == LoanStatus.Returned) throw new InvalidOperationException("LOAN_ALREADY_RETURNED"); loan.Status = LoanStatus.Returned; loan.ReturnedAt = DateTime.UtcNow; loan.Copy.Status = CopyStatus.Available; await unitOfWork.SaveChangesAsync(ct); }
}
public sealed class RenewLoanHandler(IRepository<Loan> loans, IUnitOfWork unitOfWork, ILibraryRules library) : IRequestHandler<RenewLoanCommand, LoanResponse>
{
    public async Task<LoanResponse> Handle(RenewLoanCommand command, CancellationToken ct) { var loan = await LoanQueries.WithDetails(loans, true).SingleOrDefaultAsync(x => x.Id == command.Id, ct) ?? throw new KeyNotFoundException("LOAN_NOT_FOUND"); if (loan.Status != LoanStatus.Active || loan.Renewals >= 2) throw new InvalidOperationException("RENEWAL_NOT_ALLOWED"); loan.DueDate = loan.DueDate.AddDays(library.LoanDays); loan.Renewals++; await unitOfWork.SaveChangesAsync(ct); return ApplicationMappings.ToLoanResponse(loan); }
}
public sealed class CancelLoanHandler(IRepository<Loan> loans, IUnitOfWork unitOfWork) : IRequestHandler<CancelLoanCommand>
{
    public async Task Handle(CancelLoanCommand command, CancellationToken ct) { var loan = await loans.Query(false).Include(x => x.Copy).SingleOrDefaultAsync(x => x.Id == command.Id, ct) ?? throw new KeyNotFoundException("LOAN_NOT_FOUND"); loan.Status = LoanStatus.Cancelled; loan.Copy.Status = CopyStatus.Available; await unitOfWork.SaveChangesAsync(ct); }
}




