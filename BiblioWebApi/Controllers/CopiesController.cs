using BiblioWebApi.Application.Catalog;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioWebApi.Controllers;

[ApiController, Authorize, Route("api")]
public sealed class CopiesController(ISender sender) : ControllerBase
{
    [HttpGet("books/{bookId:int}/copies")]
    public Task<IReadOnlyCollection<CopyResponse>> GetByBook(int bookId, CancellationToken ct) => sender.Send(new ListCopiesQuery(bookId), ct);
    [Authorize(Roles = "Administrator,Librarian"), HttpPost("books/{bookId:int}/copies")]
    public async Task<ActionResult<CopyResponse>> Create(int bookId, CreateCopyRequest request, CancellationToken ct) { var copy = await sender.Send(new CreateCopyCommand(bookId, request), ct); return Created($"/api/copies/{copy.Id}", copy); }
    [HttpGet("copies/{id:int}")]
    public async Task<ActionResult<CopyResponse>> Get(int id, CancellationToken ct) => Ok(await sender.Send(new GetCopyQuery(id), ct));
    [Authorize(Roles = "Administrator,Librarian"), HttpPatch("copies/{id:int}/status")]
    public async Task<IActionResult> Status(int id, CopyStatusRequest request, CancellationToken ct) { await sender.Send(new ChangeCopyStatusCommand(id, request), ct); return NoContent(); }
    [Authorize(Roles = "Administrator,Librarian"), HttpDelete("copies/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) { await sender.Send(new RetireCopyCommand(id), ct); return NoContent(); }
}


