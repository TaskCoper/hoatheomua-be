namespace HoaTheoMua.Service.Exceptions;

/// <summary>
/// A fulfilment step failed in a way that retrying can plausibly fix: Ahamove/NhanhVn returned 5xx,
/// the socket dropped, or the call timed out. The endpoint retry policy retries these.
/// </summary>
public class FulfillmentTransientException : AppException
{
    public FulfillmentTransientException(string detail)
        : base(title: "Bad Gateway", statusCode: 502, messageCode: "FULFILLMENT_TRANSIENT", detail: detail) { }
}
