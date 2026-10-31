using BiblioWebApi.Application.Administration;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioWebApi.Controllers;

[ApiController, Route("api")]
public sealed class SystemController(ISender sender) : ControllerBase
{
    [AllowAnonymous, HttpGet("health")] public IActionResult Health() => Ok(new { status = "ok", timestamp = DateTime.UtcNow });
    [AllowAnonymous, HttpGet("metadata")] public IActionResult Metadata() => Ok(new { roles = Enum.GetNames<UserRole>(), userStatuses = Enum.GetNames<UserStatus>(), copyStatuses = Enum.GetNames<CopyStatus>(), loanStatuses = Enum.GetNames<LoanStatus>(), reservationStatuses = Enum.GetNames<ReservationStatus>(), fineStatuses = Enum.GetNames<FineStatus>() });
    [Authorize(Roles = "Administrator,Librarian"), HttpGet("dashboard/summary")] public async Task<IActionResult> Summary(CancellationToken ct) => Ok(await sender.Send(new DashboardSummaryQuery(), ct));
    [Authorize(Roles = "Administrator,Librarian"), HttpGet("dashboard/recent-activity")] public async Task<IActionResult> Activity(CancellationToken ct) => Ok(await sender.Send(new RecentActivityQuery(), ct));
    [Authorize(Roles = "Administrator,Librarian"), HttpGet("dashboard/alerts")] public async Task<IActionResult> Alerts(CancellationToken ct) => Ok(await sender.Send(new DashboardAlertsQuery(), ct));
    [Authorize(Roles = "Administrator,Librarian"), HttpGet("dashboard/loans-by-status")] public async Task<IActionResult> LoansByStatus(CancellationToken ct) => Ok(await sender.Send(new LoansByStatusQuery(), ct));
    [Authorize(Roles = "Administrator,Librarian"), HttpGet("dashboard/books-availability")] public async Task<IActionResult> BooksAvailability(CancellationToken ct) => Ok(await sender.Send(new BooksAvailabilityQuery(), ct));
    [Authorize(Roles = "Administrator,Librarian"), HttpGet("dashboard/overdue")] public async Task<IActionResult> Overdue(CancellationToken ct) => Ok(await sender.Send(new DashboardOverdueQuery(), ct));
}


