namespace HoaTheoMua.Service.BaoKim;

public static class BaoKimConstant
{
    public static string GetTokenUrl = "/b2b/auth-service/api/oauth/get-token";
    public static string CreateOrder = "/b2b/core/api/ext/order/send";

    // BaoKim returns this code in the response envelope on success.
    public const int SuccessCode = 100;

    // Callback (webhook) codes. These are a different scheme from SuccessCode above, which is the
    // envelope code on our own outbound calls — do not unify them.
    // payment_result.result_code: 200 = payment succeeded.
    public const int CallbackSuccessResultCode = 200;

    // order.status and transaction.status: 1 = completed.
    public const int CallbackCompletedStatus = 1;

    // The code BaoKim expects back in our callback response body. Anything other than 0 makes it
    // redeliver the webhook — up to 10 attempts, 5 minutes apart.
    public const int CallbackAckCode = 0;

    // Storefront URLs BaoKim redirects the buyer to after payment / on failure.
    public const string PaymentSuccessUrl = "https://hoa-theo-mua.vercel.app/payment/success";
    public const string PaymentFailUrl = "https://hoa-theo-mua.vercel.app/payment/fail";
    public const string ProductUrl = "https://hoa-theo-mua.vercel.app/products";
}