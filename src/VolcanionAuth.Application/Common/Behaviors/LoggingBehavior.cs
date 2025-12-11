using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace VolcanionAuth.Application.Common.Behaviors;

/// <summary>
/// Provides pipeline behavior that logs the handling and execution time of requests and their responses within a
/// MediatR pipeline.
/// </summary>
/// <remarks>This behavior logs the start and completion of each request, including the elapsed time and any
/// exceptions that occur. It can be used to monitor and diagnose request processing within the pipeline. Logging is
/// performed at the information level for successful requests and at the error level for exceptions.</remarks>
/// <typeparam name="TRequest">The type of the request being handled. Must implement <see cref="IRequest{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the request handler.</typeparam>
/// <param name="logger">The logger used to record information and error messages during request handling.</param>
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request by invoking the next handler in the pipeline and logging execution details.
    /// </summary>
    /// <remarks>This method logs the start and completion of request handling, including the elapsed time. If
    /// an exception occurs during processing, the error is logged and the exception is rethrown.</remarks>
    /// <param name="request">The request message to be processed. Cannot be null.</param>
    /// <param name="next">A delegate that invokes the next handler in the request processing pipeline and returns the response.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response returned by the next
    /// handler.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Get the name of the request type for logging purposes.
        var requestName = typeof(TRequest).Name;
        // Start a stopwatch to measure the execution time.
        var stopwatch = Stopwatch.StartNew();
        // Log the start of request handling.
        logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            // Invoke the next handler in the pipeline and await the response.
            var response = await next();
            // Stop the stopwatch as the request handling is complete.
            stopwatch.Stop();
            // Log the completion of request handling along with the elapsed time.
            logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
            // Return the response from the next handler.
            return response;
        }
        catch (Exception ex)
        {
            // Stop the stopwatch in case of an exception.
            stopwatch.Stop();
            // Log the exception details along with the elapsed time.
            logger.LogError(ex, "Error handling {RequestName} after {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
