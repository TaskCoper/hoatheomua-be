using FluentValidation;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HoaTheoMua.Filters;

/// <summary>
/// Runs FluentValidation for every action argument that has a registered
/// <see cref="IValidator{T}"/>. Replaces per-service validator injection:
/// on failure it throws <see cref="ValidationExceptionError"/> which the
/// <c>GlobalExceptionHandlerMiddleware</c> renders as a 422 error envelope.
/// </summary>
public class ValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;
        var errors = new List<ValidationError>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validator = ResolveValidator(services, argument.GetType());
            if (validator is null)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            if (!result.IsValid)
            {
                errors.AddRange(result.Errors.Select(e => new ValidationError
                {
                    Field = ToCamelCasePath(e.PropertyName),
                    Message = e.ErrorMessage
                }));
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationExceptionErrorV1(errors);
        }

        await next();
    }

    // Camel-cases each segment of a property path so nested/collection paths
    // stay correct: "Items[0].Quantity" -> "items[0].quantity".
    private static string ToCamelCasePath(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return propertyName;
        }

        var segments = propertyName.Split('.');
        for (var i = 0; i < segments.Length; i++)
        {
            segments[i] = CamelCaseSegment(segments[i]);
        }

        return string.Join('.', segments);
    }

    private static string CamelCaseSegment(string segment)
    {
        if (string.IsNullOrEmpty(segment) || !char.IsUpper(segment[0]))
        {
            return segment;
        }

        return char.ToLowerInvariant(segment[0]) + segment[1..];
    }

    // Walk the type hierarchy so a request that inherits from a validated base
    // (e.g. UpdateRequest : CreateRequest) is still validated by the base validator.
    private static IValidator? ResolveValidator(IServiceProvider services, Type argumentType)
    {
        for (var type = argumentType; type is not null && type != typeof(object); type = type.BaseType)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(type);
            if (services.GetService(validatorType) is IValidator validator)
            {
                return validator;
            }
        }

        return null;
    }
}
