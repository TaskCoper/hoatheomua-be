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
}
