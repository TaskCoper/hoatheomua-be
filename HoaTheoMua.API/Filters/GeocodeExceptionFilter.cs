using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Geocode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HoaTheoMua.Filters;

public class GeocodeExceptionFilter : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        var isAutocomplete = context.ActionDescriptor.DisplayName?.Contains("Autocomplete", StringComparison.OrdinalIgnoreCase) ?? false;
        var exception = context.Exception;
        var statusCode = 502;
        var reason = "upstream-unreachable";

        if (exception is VietMapException vmEx)
        {
            statusCode = vmEx.StatusCode;
            reason = vmEx.ReasonCode;
        }

        object errorResult = isAutocomplete
            ? new { results = new List<object>(), reason }
            : new { result = (object?)null, reason };

        context.Result = new ObjectResult(errorResult)
        {
            StatusCode = statusCode
        };
        context.ExceptionHandled = true;
    }
}
