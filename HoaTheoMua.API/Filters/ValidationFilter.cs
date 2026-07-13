using FluentValidation;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace HoaTheoMua.API.Filters;

// Action Filter chạy tự động trước khi Controller Action được thực thi
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Duyệt qua tất cả các tham số truyền vào Action (từ Body, Query, Route, v.v...)
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;

            var argumentType = argument.GetType();
            object validatorInstance = null;
            var currentType = argumentType;

            // Duyệt ngược cây kế thừa để tìm Validator phù hợp (hỗ trợ các class kế thừa như UpdateRequest từ CreateRequest)
            while (currentType != null && currentType != typeof(object))
            {
                // Tạo kiểu dynamic IValidator<currentType> để tìm kiếm trong DI
                var validatorType = typeof(IValidator<>).MakeGenericType(currentType);

                // Lấy Validator từ DI container
                validatorInstance = context.HttpContext.RequestServices.GetService(validatorType);
                if (validatorInstance != null)
                {
                    break; // Nếu tìm thấy validator ở lớp này hoặc lớp cha thì dừng tìm kiếm
                }
                currentType = currentType.BaseType; // Chuyển lên lớp cha tiếp theo
            }

            // Nếu tìm thấy Validator hợp lệ
            if (validatorInstance is IValidator validator)
            {
                // Khởi tạo ValidationContext cho object hiện tại
                var validationContext = new ValidationContext<object>(argument);

                // Thực hiện validate bất đồng bộ
                var validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                // Nếu dữ liệu không hợp lệ
                if (!validationResult.IsValid)
                {
                    // Gom tất cả các thông báo lỗi lại, cách nhau bởi ký tự " | "
                    var errorMessage = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));

                    // Ném lỗi ValidationExceptionError để GlobalExceptionHandlerMiddleware bắt và trả về HTTP 422
                    throw new ValidationExceptionError(errorMessage);
                }
            }
        }

        // Chuyển tiếp Request sang bước tiếp theo trong pipeline (chạy Controller Action)
        await next();
    }
}
