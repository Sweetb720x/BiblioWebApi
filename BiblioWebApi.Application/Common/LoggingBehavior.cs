using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BiblioWebApi.Application.Common;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        logger.LogInformation("Handling MediatR request {RequestName}", name);
        var started = Stopwatch.GetTimestamp();
        try
        {
            var response = await next(cancellationToken);
            logger.LogInformation("Handled MediatR request {RequestName} in {ElapsedMs} ms", name, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Request {RequestName} failed", name);
            throw;
        }
    }
}




