using FluentValidation;
using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Auth;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IService = HoaTheoMua.Service.Auth.IService;
using Request = HoaTheoMua.Service.Auth.Request;

namespace HoaTheoMua.Controllers;

[ApiController]
public class UserController: ControllerBase
{
    private readonly IService _authService;
    
    private readonly IValidator<Request.RegisterRequest> _registerValidator;
    private readonly IValidator<Request.ReSendMailRequest> _resendValidator;
    private readonly IValidator<Request.VerifyEmailRequest> _verifyEmailValidator;
    private readonly IValidator<Request.LoginRequest> _loginValidator;

    
    public UserController(IService userService,  
        IValidator<Request.RegisterRequest> registerValidator,
        IValidator<Request.ReSendMailRequest> resendValidator,
        IValidator<Request.VerifyEmailRequest> verifyEmailValidator,
        IValidator<Request.LoginRequest> loginValidator)
    {
        _authService = userService;
        _registerValidator = registerValidator;
        _resendValidator = resendValidator;
        _verifyEmailValidator = verifyEmailValidator;
        _loginValidator = loginValidator;
    }

    [HttpGet("/api/v1/auth/me")]
    public async Task<IActionResult> GetMe()
    {
        var result = await _authService.GetMe();
        return Ok(ApiResponseFactory.Base(result,true,"",HttpContext.TraceIdentifier));
    }
    
    [HttpPost("/api/v1/auth/register")]
    public async Task<IActionResult> RegisterTask([FromBody]Request.RegisterRequest request)
    {
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            
            throw new ValidationExceptionError(errorMessage); 
        }
        var result = await _authService.Register(request);
        return Ok(ApiResponseFactory.Base(result, true, "",HttpContext.TraceIdentifier));
    }
    
    [HttpPost("/api/v1/auth/verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody]Request.VerifyEmailRequest request)
    {
        var validationResult = await _verifyEmailValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            
            throw new ValidationExceptionError(errorMessage); 
        }
        var result = await _authService.VerifyEmail(request);
        if (!string.IsNullOrEmpty(result!.AccessToken) && !string.IsNullOrEmpty(result.RefreshToken))
        {
            HttpContext.Response.WriteAuthCookies(result.AccessToken, result.RefreshToken);
        }
        return Ok(ApiResponseFactory.Base(result.Message, true, "",HttpContext.TraceIdentifier));
    }

    [HttpPost("/api/v1/auth/re-send-mail")]
    public async Task<IActionResult> ReSendMail([FromBody]Request.ReSendMailRequest request)
    {
        var validationResult = await _resendValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            
            throw new ValidationExceptionError(errorMessage); 
        }
        var result = await _authService.ReSendMail(request);
        return Ok(ApiResponseFactory.Base(result, true, "", HttpContext.TraceIdentifier));
    }

    [HttpPost("/api/v1/auth/refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var result = await _authService.RefreshToken();
        HttpContext.Response.WriteAuthCookies(result.AccessToken!, result.RefreshToken!);
        return Ok(ApiResponseFactory.Base(result.Message, true, "",HttpContext.TraceIdentifier));
    }

    [HttpPost("/api/v1/auth/sign-out")] 
    public async Task<IActionResult> Logout()
    {
        var result = await _authService.Logout();
        HttpContext.Response.DeleteAuthCookies();
        return Ok(ApiResponseFactory.Base(result.Message, true, "",HttpContext.TraceIdentifier));
    }
    
    [HttpPost("/api/v1/auth/sign-in")]
    public async Task<IActionResult> SignIn([FromBody] Request.LoginRequest request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            
            throw new ValidationExceptionError(errorMessage); 
        }
        var result = await _authService.LoginAsync(request);
        HttpContext.Response.WriteAuthCookies(result.AccessToken!, result.RefreshToken!);
        return Ok(ApiResponseFactory.Base(result.Message, true, "",HttpContext.TraceIdentifier));

    }
}