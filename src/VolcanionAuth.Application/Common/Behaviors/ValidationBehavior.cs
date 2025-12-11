using FluentValidation;

namespace VolcanionAuth.Application.Common.Behaviors;

/// <summary>
/// Defines a pipeline behavior that performs validation on incoming requests using the specified validators before
/// passing the request to the next handler.
/// </summary>
/// <remarks>If any validation failures are detected, a ValidationException is thrown and the request is not
/// processed further. If no validators are provided, the request is passed to the next handler without validation. This
/// behavior is typically used in MediatR pipelines to enforce validation rules consistently across requests.</remarks>
/// <typeparam name="TRequest">The type of the request message to be validated.</typeparam>
/// <typeparam name="TResponse">The type of the response message returned by the handler.</typeparam>
/// <param name="validators">A collection of validators to apply to the request. Each validator is invoked to validate the request before it is
/// handled.</param>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Validates the specified request using all configured validators and invokes the next handler in the pipeline if
    /// validation succeeds.
    /// </summary>
    /// <remarks>If no validators are configured, the request is passed directly to the next handler without
    /// validation. All validators are executed asynchronously, and any validation errors will prevent further
    /// processing of the request.</remarks>
    /// <param name="request">The request object to be validated and processed. Cannot be null.</param>
    /// <param name="next">A delegate representing the next handler to invoke in the pipeline. This is called after successful validation.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the validation or request handling operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response produced by the next
    /// handler in the pipeline.</returns>
    /// <exception cref="ValidationException">Thrown when one or more validation failures are detected in the request.</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // If there are no validators, proceed to the next handler
        if (!validators.Any())
        {
            return await next();
        }

        // Create a validation context for the request
        var context = new ValidationContext<TRequest>(request);
        // Execute all validators asynchronously
        var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        // Collect all validation failures
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // If there are any validation failures, throw a ValidationException
        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        // If validation succeeds, proceed to the next handler
        return await next();
    }
}
