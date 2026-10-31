using BiblioWebApi.Infrastructure.Persistence;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BiblioWebApi.Infrastructure.Services;

public sealed class LibraryRules(BiblioDbContext db, IConfiguration configuration) : ILibraryRules
{
    public int MaxLoans => configuration.GetValue("Library:MaxActiveLoans", 3);
    public int MaxReservations => configuration.GetValue("Library:MaxActiveReservations", 3);
    public int PickupHours => configuration.GetValue("Library:ReservationPickupHours", 48);
    public int LoanDays => configuration.GetValue("Library:LoanDays", 14);

    public async Task<(bool Allowed, string Code, string Message)> CanBorrowAsync(User user, CancellationToken ct)
    {
        if (user.Status != UserStatus.Active) return (false, "USER_NOT_ACTIVE", "El usuario no está activo.");
        var hasDebt = await db.Fines.AnyAsync(x => x.UserId == user.Id && x.Status == FineStatus.Pending, ct);
        var overdue = await db.Loans.AnyAsync(x => x.UserId == user.Id && (x.Status == LoanStatus.Overdue || (x.Status == LoanStatus.Active && x.DueDate < DateTime.UtcNow)), ct);
        if (hasDebt || overdue) return (false, "USER_BLOCKED", "El usuario tiene multas pendientes o préstamos vencidos.");
        var count = await db.Loans.CountAsync(x => x.UserId == user.Id && (x.Status == LoanStatus.Active || x.Status == LoanStatus.Overdue), ct);
        return count >= MaxLoans ? (false, "LOAN_LIMIT_REACHED", "El usuario alcanzó el límite de préstamos activos.") : (true, "", "");
    }

    public async Task<(bool Allowed, string Code, string Message)> CanReserveAsync(User user, int bookId, CancellationToken ct)
    {
        var borrow = await CanBorrowAsync(user, ct);
        if (!borrow.Allowed) return borrow;
        var count = await db.Reservations.CountAsync(x => x.UserId == user.Id && (x.Status == ReservationStatus.Pending || x.Status == ReservationStatus.Approved), ct);
        if (count >= MaxReservations) return (false, "RESERVATION_LIMIT_REACHED", "El usuario alcanzó el límite de reservas activas.");
        var duplicate = await db.Reservations.AnyAsync(x => x.UserId == user.Id && x.BookId == bookId && (x.Status == ReservationStatus.Pending || x.Status == ReservationStatus.Approved), ct);
        return duplicate ? (false, "DUPLICATE_RESERVATION", "El usuario ya tiene una reserva activa para este libro.") : (true, "", "");
    }

}


