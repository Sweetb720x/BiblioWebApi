using BiblioWebApi.Application.Loans;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioWebApi.Controllers;

[ApiController, Authorize, Route("api/loans")]
public sealed class LoansController(ISender sender) : ControllerBase
{
    [HttpGet] public Task<IReadOnlyCollection<LoanResponse>> GetAll(CancellationToken ct) => sender.Send(new ListLoansQuery(), ct);
    [HttpGet("overdue")] public Task<IReadOnlyCollection<LoanResponse>> Overdue(CancellationToken ct) => sender.Send(new ListOverdueLoansQuery(), ct);
    [HttpGet("{id:int}")] public async Task<ActionResult<LoanResponse>> Get(int id, CancellationToken ct) => Ok(await sender.Send(new GetLoanQuery(id), ct));
    [HttpPost] public async Task<ActionResult<LoanResponse>> Create(CreateLoanRequest request, CancellationToken ct) { var loan = await sender.Send(new CreateLoanCommand(request), ct); return CreatedAtAction(nameof(Get), new { id = loan.Id }, loan); }
    [HttpPatch("{id:int}/return")] public async Task<IActionResult> Return(int id, CancellationToken ct) { await sender.Send(new ReturnLoanCommand(id), ct); return NoContent(); }
    [HttpPatch("{id:int}/renew")] public async Task<ActionResult<LoanResponse>> Renew(int id, CancellationToken ct) => Ok(await sender.Send(new RenewLoanCommand(id), ct));
    [HttpPatch("{id:int}/cancel")] public async Task<IActionResult> Cancel(int id, CancellationToken ct) { await sender.Send(new CancelLoanCommand(id), ct); return NoContent(); }
}

[ApiController, Authorize, Route("api/users/{userId:int}/loans")]
public sealed class UserLoansController(ISender sender) : ControllerBase
{
    [HttpGet] public Task<IReadOnlyCollection<LoanResponse>> Get(int userId, CancellationToken ct) => sender.Send(new ListUserLoansQuery(userId), ct);
}


