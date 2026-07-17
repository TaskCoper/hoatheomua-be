namespace HoaTheoMua.Service.BaoKim;

public class BaoKimOption
{
    public string BaseUrl { get; set; } = null!;
    public string MerchantCode { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string PrivateKey { get; set; } = null!;

    // BaoKim's public key (not ours), used to verify the Signature header on inbound callbacks.
    // Obtain it from the BaoKim merchant dashboard. Empty means callbacks cannot be authenticated,
    // and the webhook fails closed rather than fulfilling orders on unverified input.
    public string PublicKey { get; set; } = "";
}