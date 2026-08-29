using System.Security.Cryptography;
using System.Text;

namespace HoaTheoMua.Service.BaoKim;

// RSA SHA256 (PKCS#1 v1.5) signing/verification for BaoKim B2B requests.
// BaoKim verifies the signature over the exact request-body bytes, so callers must
// sign the same string they send on the wire.
public static class BaoKimSigner
{
    // Signs the raw request body with our private key and returns the base64 signature.
    public static string Sign(string rawData, string privateKeyPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);
        var signature = rsa.SignData(
            Encoding.UTF8.GetBytes(rawData),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(signature);
    }

    // Verifies the Signature header on an inbound BaoKim callback against BaoKim's public key.
    //
    // CAUTION: BaoKim's webhook docs only describe this header as "Chữ ký số để xác thực tính toàn
    // vẹn dữ liệu" and never state the algorithm or the signed payload. This mirrors Sign above
    // (RSA SHA256 PKCS#1 v1.5 over the raw body bytes) on the assumption the scheme is symmetric —
    // that is an inference, so confirm it against a real callback before trusting it in production.
    // Returns false when the signature simply does not match. Throws only when publicKeyPem itself
    // is unusable, which is a configuration fault on our side rather than a bad caller — keeping
    // those apart is what lets the caller distinguish a 500 from a 401.
    public static bool Verify(string rawData, string signatureBase64, string publicKeyPem)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);

        byte[] signatureBytes;
        try
        {
            signatureBytes = Convert.FromBase64String(signatureBase64);
        }
        catch (FormatException)
        {
            // A signature that isn't even base64 is not a valid signature; it is not our fault.
            return false;
        }

        return rsa.VerifyData(
            Encoding.UTF8.GetBytes(rawData),
            signatureBytes,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
    }
}
