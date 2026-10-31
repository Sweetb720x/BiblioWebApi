using BiblioWebApi.Application.Administration;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioWebApi.Controllers;

[ApiController, Authorize, Route("api/fines")]
public sealed class FinesController(ISender sender) : ControllerBase
{
    [Authorize(Roles = "Administrator,Librarian"), HttpGet] public Task<IReadOnlyCollection<Fine>> GetAll(CancellationToken ct) => sender.Send(new ListFinesQuery(), ct);
    [HttpGet("{id:int}")] public async Task<ActionResult<Fine>> Get(int id, CancellationToken ct) => Ok(await sender.Send(new GetFineQuery(id), ct));
    [Authorize(Roles = "Administrator,Librarian"), HttpPost] public async Task<ActionResult<Fine>> Create(FineRequest request, CancellationToken ct) { var fine = await sender.Send(new CreateFineCommand(request), ct); return CreatedAtAction(nameof(Get), new { id = fine.Id }, fine); }
    [Authorize(Roles = "Administrator,Librarian"), HttpPatch("{id:int}/pay")] public async Task<IActionResult> Pay(int id, CancellationToken ct) { await sender.Send(new SettleFineCommand(id, FineStatus.Paid), ct); return NoContent(); }
    [Authorize(Roles = "Administrator,Librarian"), HttpPatch("{id:int}/waive")] public async Task<IActionResult> Waive(int id, CancellationToken ct) { await sender.Send(new SettleFineCommand(id, FineStatus.Waived), ct); return NoContent(); }
}

[ApiController, Authorize, Route("api/users/{userId:int}")]
public sealed class UserAccountController(ISender sender) : ControllerBase
{
    [HttpGet("fines")] public Task<IReadOnlyCollection<Fine>> Fines(int userId, CancellationToken ct) => sender.Send(new ListUserFinesQuery(userId), ct);
    [HttpGet("account-status")] public async Task<IActionResult> AccountStatus(int userId, CancellationToken ct) => Ok(await sender.Send(new GetAccountStatusQuery(userId), ct));
}


