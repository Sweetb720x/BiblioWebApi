using BiblioWebApi.Application.Books;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BiblioWebApi.Controllers;

[ApiController, Route("api/books")]
public sealed class BooksController(ISender sender) : ControllerBase
{
    [AllowAnonymous, HttpGet]
    public Task<IReadOnlyCollection<BookResponse>> GetAll(string? title, string? author, string? category, string? isbn, CancellationToken ct) => sender.Send(new ListBooksQuery(title, author, category, isbn), ct);
    [AllowAnonymous, HttpGet("search")]
    public Task<IReadOnlyCollection<BookResponse>> Search(string? q, CancellationToken ct) => sender.Send(new ListBooksQuery(q, q, q, q), ct);
    [AllowAnonymous, HttpGet("{id:int}")]
    public async Task<ActionResult<BookResponse>> Get(int id, CancellationToken ct) => Ok(await sender.Send(new GetBookQuery(id), ct));
    [AllowAnonymous, HttpGet("{id:int}/availability")]
    public async Task<IActionResult> Availability(int id, CancellationToken ct) => Ok(await sender.Send(new GetBookAvailabilityQuery(id), ct));
    [Authorize(Roles = "Administrator,Librarian"), HttpPost]
    public async Task<ActionResult<BookResponse>> Create(CreateBookRequest request, CancellationToken ct)
    {
        var book = await sender.Send(new CreateBookCommand(request), ct);
        return CreatedAtAction(nameof(Get), new { id = book.Id }, book);
    }
    [Authorize(Roles = "Administrator,Librarian"), HttpPut("{id:int}"), HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateBookRequest request, CancellationToken ct) { await sender.Send(new UpdateBookCommand(id, request), ct); return NoContent(); }
    [Authorize(Roles = "Administrator,Librarian"), HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) { await sender.Send(new RemoveBookCommand(id), ct); return NoContent(); }
}


