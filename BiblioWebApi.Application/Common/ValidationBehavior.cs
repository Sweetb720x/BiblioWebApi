using MediatR;

namespace BiblioWebApi.Application.Common;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IValidatableRequest validatable)
        {
            var errors = validatable.Validate();
            if (errors.Count > 0) throw new RequestValidationException(errors);
        }

        return next(cancellationToken);
    }
}

public sealed class RequestValidationException(IReadOnlyCollection<string> errors) : Exception("La solicitud contiene datos inválidos.")
{
    public IReadOnlyCollection<string> Errors { get; } = errors;
}




