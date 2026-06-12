namespace HoaTheoMua.Service.Exceptions;

public class ValidationExceptionError: AppException
{
   public ValidationExceptionError(string detail)
           : base(title: "Unprocessable Entity", statusCode: 422, messageCode: "VALIDATION_ERROR", detail: detail) { }
}