using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.Webhook;

// BaoKim decides whether to redeliver by reading `code` out of the response body, not off the HTTP
// status: 0 means we accepted the callback, anything else queues a retry (10 attempts, 5 minutes
// apart). That is why this exists instead of ApiResponseFactory.Base — that envelope carries no
// `code` field at all, so BaoKim would read it as a failure and retry every delivery.
public class BaoKimCallbackAck
{
    [JsonPropertyName("code")]
    public int Code { get; set; } = BaoKim.BaoKimConstant.CallbackAckCode;

    [JsonPropertyName("message")]
    public string Message { get; set; } = "Thành công";

    [JsonPropertyName("data")]
    public object Data { get; set; } = new();
}
