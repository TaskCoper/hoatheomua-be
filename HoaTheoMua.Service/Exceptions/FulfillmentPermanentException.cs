namespace HoaTheoMua.Service.Exceptions;

/// <summary>
/// A fulfilment step was rejected on its merits - Ahamove/NhanhVn returned 4xx, NhanhVn replied
/// code != 1, or required local data is missing. Retrying would produce the same answer, so the
/// retry policy ignores this type and lets the message fault immediately, which starts compensation
/// sooner rather than after five pointless attempts.
/// </summary>
public class FulfillmentPermanentException : AppException
{
    public FulfillmentPermanentException(string detail)
        : base(title: "Bad Request", statusCode: 400, messageCode: "FULFILLMENT_PERMANENT", detail: detail) { }
}
