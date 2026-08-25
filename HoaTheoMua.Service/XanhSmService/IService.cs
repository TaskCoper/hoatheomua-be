namespace HoaTheoMua.Service.XanhSmService;

public interface IService
{
    // Bốn chức năng Xanh SM mà controller được phép gọi.
    /// <summary>
    /// Tìm các dịch vụ Xanh SM đang phục vụ tại điểm lấy hàng/điểm giao do FE gửi lên.
    /// Dùng kết quả để lấy <c>service.id</c> trước khi gọi estimate hoặc tạo đơn.
    /// </summary>
    /// <param name="request">Danh sách pickup, dropoff tùy chọn, loại dịch vụ và thời điểm đặt dự kiến.</param>
    /// <param name="cancellationToken">Token hủy của HTTP request từ FE.</param>
    /// <returns>Danh sách service đã được Xanh SM lọc theo khu vực và giờ hoạt động.</returns>
    Task<List<Response.AvailableService>> GetAvailableServices(Request.GetAvailableServicesRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Báo giá cho một hoặc nhiều service tại cùng tuyến đường. Response chứa <c>fee_id</c> dùng để tạo đơn.
    /// </summary>
    /// <param name="request">Service, route point, addon và phương thức thanh toán cần báo giá.</param>
    /// <param name="cancellationToken">Token hủy của HTTP request từ FE.</param>
    /// <returns>Thông tin phí, thời hạn fee_id, quãng đường và thời gian dự kiến.</returns>
    Task<Response.EstimateData> EstimateOrder(Request.EstimateOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo đơn giao hàng dựa trên <c>fee_id</c> vừa estimate. Xanh SM sẽ tự đối chiếu fee_id,
    /// order_time, points và addons với request estimate trước đó.
    /// </summary>
    /// <param name="request">Thông tin đơn, tuyến giao, phương thức thanh toán và addons đã chọn.</param>
    /// <param name="cancellationToken">Token hủy của HTTP request từ FE.</param>
    /// <returns>Đơn mới tạo ở Xanh SM, bao gồm order id và trạng thái ban đầu.</returns>
    Task<Response.CreateOrderData> CreateOrder(Request.CreateXanhSmOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hủy một đơn Xanh SM chưa hoàn tất lấy hàng bằng order id phía Xanh SM và lý do hủy.
    /// </summary>
    /// <param name="request">Order id, cancel code 9000 và nội dung lý do hủy.</param>
    /// <param name="cancellationToken">Token hủy của HTTP request từ FE.</param>
    /// <returns>Thông báo xác nhận hủy từ Xanh SM.</returns>
    Task<Response.CancelOrderData> CancelOrder(Request.CancelOrderRequest request, CancellationToken cancellationToken = default);
}
