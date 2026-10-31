using BiblioWebApi.Application.Abstractions;
using BiblioWebApi.Application.Common;
using BiblioWebApi.Domain;
using BiblioWebApi.Application.Contracts;
using BiblioWebApi.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BiblioWebApi.Application.Catalog;

public sealed record ListCategoriesQuery : IRequest<IReadOnlyCollection<Category>>;
public sealed record CreateCategoryCommand(CreateCategoryRequest Request) : IRequest<Category>, IValidatableRequest
{
    public IReadOnlyCollection<string> Validate() => string.IsNullOrWhiteSpace(Request.Name) ? ["Name is required."] : [];
}
public sealed record ListCopiesQuery(int BookId) : IRequest<IReadOnlyCollection<CopyResponse>>;
public sealed record GetCopyQuery(int Id) : IRequest<CopyResponse>;
public sealed record CreateCopyCommand(int BookId, CreateCopyRequest Request) : IRequest<CopyResponse>, IValidatableRequest
{
    public IReadOnlyCollection<string> Validate() => string.IsNullOrWhiteSpace(Request.InventoryCode) ? ["InventoryCode is required."] : [];
}
public sealed record ChangeCopyStatusCommand(int Id, CopyStatusRequest Request) : IRequest;
public sealed record RetireCopyCommand(int Id) : IRequest;

public sealed class ListCategoriesHandler(IRepository<Category> categories) : IRequestHandler<ListCategoriesQuery, IReadOnlyCollection<Category>>
{
    public async Task<IReadOnlyCollection<Category>> Handle(ListCategoriesQuery request, CancellationToken ct) => (await categories.Query().OrderBy(x => x.Name).ToListAsync(ct)).ToArray();
}
public sealed class CreateCategoryHandler(IRepository<Category> categories, IUnitOfWork unitOfWork) : IRequestHandler<CreateCategoryCommand, Category>
{
    public async Task<Category> Handle(CreateCategoryCommand command, CancellationToken ct) { var category = new Category { Name = command.Request.Name.Trim() }; await categories.AddAsync(category, ct); await unitOfWork.SaveChangesAsync(ct); return category; }
}
public sealed class ListCopiesHandler(IRepository<BookCopy> copies) : IRequestHandler<ListCopiesQuery, IReadOnlyCollection<CopyResponse>>
{
    public async Task<IReadOnlyCollection<CopyResponse>> Handle(ListCopiesQuery request, CancellationToken ct) => (await copies.Query().Where(x => x.BookId == request.BookId).ToListAsync(ct)).Select(ApplicationMappings.ToCopyResponse).ToArray();
}
public sealed class GetCopyHandler(IRepository<BookCopy> copies) : IRequestHandler<GetCopyQuery, CopyResponse>
{
    public async Task<CopyResponse> Handle(GetCopyQuery request, CancellationToken ct) => ApplicationMappings.ToCopyResponse(await copies.GetByIdAsync(request.Id, ct) ?? throw new KeyNotFoundException("COPY_NOT_FOUND"));
}
public sealed class CreateCopyHandler(IRepository<Book> books, IRepository<BookCopy> copies, IUnitOfWork unitOfWork) : IRequestHandler<CreateCopyCommand, CopyResponse>
{
    public async Task<CopyResponse> Handle(CreateCopyCommand command, CancellationToken ct)
    {
        if (await books.GetByIdAsync(command.BookId, ct) is null) throw new KeyNotFoundException("BOOK_NOT_FOUND");
        var copy = new BookCopy { BookId = command.BookId, InventoryCode = command.Request.InventoryCode.Trim(), Notes = command.Request.Notes };
        await copies.AddAsync(copy, ct); await unitOfWork.SaveChangesAsync(ct); return ApplicationMappings.ToCopyResponse(copy);
    }
}
public sealed class ChangeCopyStatusHandler(IRepository<BookCopy> copies, IUnitOfWork unitOfWork) : IRequestHandler<ChangeCopyStatusCommand>
{
    public async Task Handle(ChangeCopyStatusCommand command, CancellationToken ct) { var copy = await copies.GetByIdAsync(command.Id, ct) ?? throw new KeyNotFoundException("COPY_NOT_FOUND"); copy.Status = command.Request.Status; await unitOfWork.SaveChangesAsync(ct); }
}
public sealed class RetireCopyHandler(IRepository<BookCopy> copies, IUnitOfWork unitOfWork) : IRequestHandler<RetireCopyCommand>
{
    public async Task Handle(RetireCopyCommand command, CancellationToken ct) { var copy = await copies.GetByIdAsync(command.Id, ct) ?? throw new KeyNotFoundException("COPY_NOT_FOUND"); copy.Status = CopyStatus.Retired; await unitOfWork.SaveChangesAsync(ct); }
}




