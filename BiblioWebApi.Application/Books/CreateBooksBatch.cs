using BiblioWebApi.Application.Abstractions;
using BiblioWebApi.Application.Common;
using BiblioWebApi.Application.Contracts;
using MediatR;

namespace BiblioWebApi.Application.Books;

public sealed record CreateBooksCommand(CreateBookRequest Request) : IRequest<CreateBooksBatchResponse>, IValidatableRequest
{
    public IReadOnlyCollection<string> Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Request.Isbn)) errors.Add("El ISBN es obligatorio.");
        if (string.IsNullOrWhiteSpace(Request.Title)) errors.Add("El título es obligatorio.");
        if (string.IsNullOrWhiteSpace(Request.Author)) errors.Add("El autor es obligatorio.");
        if (Request.Quantity < 1) errors.Add("La cantidad debe ser mayor que cero.");

        return errors;
    }
}

public sealed class CreateBooksHandler(IBookBatchCreator batchCreator, IBookBatchLimitStore batchLimitStore)
    : IRequestHandler<CreateBooksCommand, CreateBooksBatchResponse>
{
    public async Task<CreateBooksBatchResponse> Handle(CreateBooksCommand request, CancellationToken cancellationToken)
    {
        var maximumQuantity = await batchLimitStore.GetAsync(cancellationToken);
        if (maximumQuantity < 1)
            throw new InvalidOperationException("El límite máximo configurado debe ser mayor que cero.");
        if (request.Request.Quantity < 1 || request.Request.Quantity > maximumQuantity)
            throw new RequestValidationException([$"La cantidad debe estar entre 1 y {maximumQuantity}."]);

        var book = request.Request with { Quantity = 1 };
        var createdBooks = await batchCreator.CreateAsync(book, request.Request.Quantity, cancellationToken);
        return new CreateBooksBatchResponse(createdBooks.Count, createdBooks);
    }
}
