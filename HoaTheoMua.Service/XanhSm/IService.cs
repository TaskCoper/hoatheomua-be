namespace HoaTheoMua.Service.XanhSm;

public interface IService
{
    Task<XanhSmTokenResponse> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    Task<XanhSmApiResponse<List<XanhSmAvailableServiceItem>>> GetAvailableServicesAsync(
        XanhSmAvailableServicesRequest request,
        CancellationToken cancellationToken = default);

    Task<XanhSmApiResponse<XanhSmEstimateData>> EstimateOrderAsync(
        XanhSmEstimateOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<XanhSmApiResponse<XanhSmDelivery>> BookDeliveryAsync(
        XanhSmBookDeliveryRequest request,
        CancellationToken cancellationToken = default);

    Task<XanhSmApiResponse<XanhSmDelivery>> GetDeliveryDetailsAsync(
        string orderId,
        bool includeDriver = true,
        bool includePath = true,
        CancellationToken cancellationToken = default);

    Task<XanhSmApiResponse<List<XanhSmDelivery>>> GetDeliveryListAsync(
        CancellationToken cancellationToken = default);

    Task<XanhSmApiResponse<XanhSmCancelDeliveryData>> CancelDeliveryAsync(
        XanhSmCancelDeliveryRequest request,
        CancellationToken cancellationToken = default);

    Task<XanhSmDoorToDoorQuote> EstimateDoorToDoorAsync(
        IReadOnlyCollection<XanhSmDeliveryPointRequest> points,
        long orderTime,
        CancellationToken cancellationToken = default);

    Task<XanhSmDelivery> BookDoorToDoorAsync(
        IReadOnlyCollection<XanhSmDeliveryPointRequest> points,
        long orderTime,
        long? goodsValue,
        string partnerTripId,
        CancellationToken cancellationToken = default);
}
