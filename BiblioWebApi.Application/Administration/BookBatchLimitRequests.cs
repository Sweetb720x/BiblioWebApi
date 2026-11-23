using BiblioWebApi.Application.Abstractions;
using BiblioWebApi.Application.Common;
using BiblioWebApi.Application.Contracts;
using MediatR;

namespace BiblioWebApi.Application.Administration;

public sealed record GetBookBatchLimitQuery : IRequest<BookBatchLimitResponse>;

public sealed record SetBookBatchLimitCommand(UpdateBookBatchLimitRequest Request) : IRequest<BookBatchLimitResponse>, IValidatableRequest
{
    public IReadOnlyCollection<string> Validate() => Request.Maximum < 1
        ? ["El límite máximo debe ser mayor que cero."]
        : [];
}

public sealed class GetBookBatchLimitHandler(IBookBatchLimitStore store)
    : IRequestHandler<GetBookBatchLimitQuery, BookBatchLimitResponse>
{
    public async Task<BookBatchLimitResponse> Handle(GetBookBatchLimitQuery query, CancellationToken cancellationToken)
        => new(await store.GetAsync(cancellationToken));
}

public sealed class SetBookBatchLimitHandler(IBookBatchLimitStore store)
    : IRequestHandler<SetBookBatchLimitCommand, BookBatchLimitResponse>
{
    public async Task<BookBatchLimitResponse> Handle(SetBookBatchLimitCommand command, CancellationToken cancellationToken)
        => new(await store.SetAsync(command.Request.Maximum, cancellationToken));
}
