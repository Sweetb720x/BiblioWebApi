using BiblioWebApi.Application.Reservations;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioWebApi.Controllers;

[ApiController, Authorize, Route("api/reservations")]
public sealed class ReservationsController(ISender sender) : ControllerBase
{
    [Authorize(Roles = "Administrator,Librarian"), HttpGet]
    public Task<IReadOnlyCollection<Reservation>> GetAll(CancellationToken ct) => sender.Send(new ListReservationsQuery(), ct);
    [Authorize(Roles = "Administrator,Librarian"), HttpGet("pending")]
    public Task<IReadOnlyCollection<Reservation>> Pending(CancellationToken ct) => sender.Send(new ListReservationsQuery(true), ct);
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Reservation>> Get(int id, CancellationToken ct) => Ok(await sender.Send(new GetReservationQuery(id), ct));
    [HttpPost]
    public async Task<ActionResult<Reservation>> Create(CreateReservationRequest request, CancellationToken ct) { var reservation = await sender.Send(new CreateReservationCommand(request), ct); return CreatedAtAction(nameof(Get), new { id = reservation.Id }, reservation); }
    [Authorize(Roles = "Administrator,Librarian"), HttpPatch("{id:int}/approve")]
    public async Task<ActionResult<Reservation>> Approve(int id, CancellationToken ct) => Ok(await sender.Send(new ApproveReservationCommand(id), ct));
    [Authorize(Roles = "Administrator,Librarian"), HttpPatch("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, CancellationToken ct) { await sender.Send(new ChangeReservationStatusCommand(id, ReservationStatus.Rejected), ct); return NoContent(); }
    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct) { await sender.Send(new ChangeReservationStatusCommand(id, ReservationStatus.Cancelled), ct); return NoContent(); }
    [Authorize(Roles = "Administrator,Librarian"), HttpPatch("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, CancellationToken ct) { await sender.Send(new CompleteReservationCommand(id), ct); return NoContent(); }
}

[ApiController, Authorize, Route("api/users/{userId:int}/reservations")]
public sealed class UserReservationsController(ISender sender) : ControllerBase
{
    [HttpGet] public Task<IReadOnlyCollection<Reservation>> Get(int userId, CancellationToken ct) => sender.Send(new ListUserReservationsQuery(userId), ct);
}


