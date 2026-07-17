namespace HoaTheoMua.Service.BaoKim;

public static class BaoKimConstant
{
    public static string GetTokenUrl = "/b2b/auth-service/api/oauth/get-token";
    public static string CreateOrder = "/b2b/core/api/ext/order/send";

    // BaoKim returns this code in the response envelope on success.
    public const int SuccessCode = 100;

    // Storefront URLs BaoKim redirects the buyer to after payment / on failure.
    public const string PaymentSuccessUrl = "https://hoa-theo-mua.vercel.app/payment/success";
    public const string PaymentFailUrl = "https://hoa-theo-mua.vercel.app/payment/fail";
    public const string ProductUrl = "https://hoa-theo-mua.vercel.app/products";
}