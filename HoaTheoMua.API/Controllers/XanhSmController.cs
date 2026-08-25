using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.XanhSmService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HoaTheoMua.Controllers;

/// <summary>Nhóm API FE dùng để lấy dịch vụ, báo giá, tạo và hủy đơn giao Xanh SM.</summary>
[ApiController]
// [Authorize(Policy = JwtExtensions.CustomerPolicy)]
public sealed class XanhSmController : ControllerBase
{
    private readonly IService _service;
    public XanhSmController(IService service) => _service = service;

    /// <summary>
    /// Endpoint cho FE lấy danh sách dịch vụ Xanh SM khả dụng tại khu vực giao nhận hiện tại.
    /// FluentValidation kiểm tra tọa độ và pickup trước khi service gọi upstream.
    /// </summary>
    [HttpPost("/api/v1/xanh-sm/available-services")]
    public async Task<IActionResult> GetAvailableServices([FromBody] Request.GetAvailableServicesRequest request,
        CancellationToken cancellationToken)
    {
        // Controller chỉ điều phối; validation do FluentValidation và nghiệp vụ gọi upstream do Service xử lý.
        var result = await _service.GetAvailableServices(request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, null, HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Endpoint báo giá. FE cần lưu fee_id và order_time response để gửi nguyên vẹn ở endpoint tạo đơn.
    /// </summary>
    [HttpPost("/api/v1/xanh-sm/estimates")]
    public async Task<IActionResult> EstimateOrder([FromBody] Request.EstimateOrderRequest request,
        CancellationToken cancellationToken)
    {
        // Response được bọc theo ApiResponseFactory chung của toàn hệ thống.
        var result = await _service.EstimateOrder(request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, null, HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Endpoint tạo đơn giao Xanh SM từ fee_id đã estimate. Addon tại request root được validator kiểm tra trước.
    /// </summary>
    [HttpPost("/api/v1/xanh-sm/orders")]
    public async Task<IActionResult> CreateOrder([FromBody] Request.CreateXanhSmOrderRequest request,
        CancellationToken cancellationToken)
    {
        // fee_id và order_time đã được FE lấy từ API estimate trước đó.
        var result = await _service.CreateOrder(request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, null, HttpContext.TraceIdentifier));
    }

    /// <summary>Endpoint hủy đơn Xanh SM bằng order_id mà Xanh SM trả về khi tạo đơn thành công.</summary>
    [HttpPost("/api/v1/xanh-sm/orders/cancel")]
    public async Task<IActionResult> CancelOrder([FromBody] Request.CancelOrderRequest request,
        CancellationToken cancellationToken)
    {
        // Xanh SM chỉ cho phép hủy bằng order_id do chính họ trả sau khi tạo đơn.
        var result = await _service.CancelOrder(request, cancellationToken);
        return Ok(ApiResponseFactory.Base(result, true, null, HttpContext.TraceIdentifier));
    }
}
