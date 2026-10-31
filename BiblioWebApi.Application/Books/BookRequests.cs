using BiblioWebApi.Application.Abstractions;
using BiblioWebApi.Application.Common;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using BiblioWebApi.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BiblioWebApi.Application.Books;

public sealed record ListBooksQuery(string? Title, string? Author, string? Category, string? Isbn) : IRequest<IReadOnlyCollection<BookResponse>>;
public sealed record GetBookQuery(int Id) : IRequest<BookResponse>;
public sealed record CreateBookCommand(CreateBookRequest Request) : IRequest<BookResponse>, IValidatableRequest
{
    public IReadOnlyCollection<string> Validate() => string.IsNullOrWhiteSpace(Request.Isbn) || string.IsNullOrWhiteSpace(Request.Title) || string.IsNullOrWhiteSpace(Request.Author) ? ["ISBN, Title and Author are required."] : [];
}
public sealed record UpdateBookCommand(int Id, UpdateBookRequest Request) : IRequest;
public sealed record RemoveBookCommand(int Id) : IRequest;

public sealed class ListBooksHandler(IRepository<Book> books) : IRequestHandler<ListBooksQuery, IReadOnlyCollection<BookResponse>>
{
    public async Task<IReadOnlyCollection<BookResponse>> Handle(ListBooksQuery request, CancellationToken ct)
    {
        var query = books.Query().Include(x => x.Category).Include(x => x.Copies).Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(request.Title)) query = query.Where(x => x.Title.Contains(request.Title));
        if (!string.IsNullOrWhiteSpace(request.Author)) query = query.Where(x => x.Author.Contains(request.Author));
        if (!string.IsNullOrWhiteSpace(request.Category)) query = query.Where(x => x.Category.Name.Contains(request.Category));
        if (!string.IsNullOrWhiteSpace(request.Isbn)) query = query.Where(x => x.Isbn.Contains(request.Isbn));
        return (await query.ToListAsync(ct)).Select(ApplicationMappings.ToBookResponse).ToArray();
    }

public sealed class UpdateBookHandler(IRepository<Book> books, IUnitOfWork unitOfWork) : IRequestHandler<UpdateBookCommand>
{
    public async Task Handle(UpdateBookCommand command, CancellationToken ct)
    {
        var book = await books.GetByIdAsync(command.Id, ct) ?? throw new KeyNotFoundException("BOOK_NOT_FOUND");
        var request = command.Request;
        if (request.Isbn is not null) book.Isbn = request.Isbn; if (request.Title is not null) book.Title = request.Title; if (request.Author is not null) book.Author = request.Author; if (request.CategoryId.HasValue) book.CategoryId = request.CategoryId.Value; if (request.Publisher is not null) book.Publisher = request.Publisher; if (request.PublicationYear.HasValue) book.PublicationYear = request.PublicationYear.Value; if (request.Description is not null) book.Description = request.Description; if (request.IsActive.HasValue) book.IsActive = request.IsActive.Value;
        await unitOfWork.SaveChangesAsync(ct);
    }
}

public sealed class RemoveBookHandler(IRepository<Book> books, IUnitOfWork unitOfWork) : IRequestHandler<RemoveBookCommand>
{
    public async Task Handle(RemoveBookCommand command, CancellationToken ct) { var book = await books.GetByIdAsync(command.Id, ct) ?? throw new KeyNotFoundException("BOOK_NOT_FOUND"); book.IsActive = false; await unitOfWork.SaveChangesAsync(ct); }
}
}

public sealed class GetBookHandler(IRepository<Book> books) : IRequestHandler<GetBookQuery, BookResponse>
{
    public async Task<BookResponse> Handle(GetBookQuery request, CancellationToken ct)
    {
        var book = await books.Query().Include(x => x.Category).Include(x => x.Copies).SingleOrDefaultAsync(x => x.Id == request.Id && x.IsActive, ct) ?? throw new KeyNotFoundException("BOOK_NOT_FOUND");
        return ApplicationMappings.ToBookResponse(book);
    }
}

public sealed class CreateBookHandler(IRepository<Book> books, IUnitOfWork unitOfWork) : IRequestHandler<CreateBookCommand, BookResponse>
{
    public async Task<BookResponse> Handle(CreateBookCommand command, CancellationToken ct)
    {
        var book = new Book { Isbn = command.Request.Isbn.Trim(), Title = command.Request.Title.Trim(), Author = command.Request.Author.Trim(), CategoryId = command.Request.CategoryId, Publisher = command.Request.Publisher, PublicationYear = command.Request.PublicationYear, Description = command.Request.Description };
        await books.AddAsync(book, ct); await unitOfWork.SaveChangesAsync(ct);
        return new BookResponse(book.Id, book.Isbn, book.Title, book.Author, book.Publisher, book.PublicationYear, book.Description, book.CategoryId, "", 0, 0);
    }
}




