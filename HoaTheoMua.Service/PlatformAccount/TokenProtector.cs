using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace HoaTheoMua.Service.PlatformAccount;

public sealed class TokenProtector(IOptions<PlatformAccountOptions> options)
{
    public string Protect(string value, string purpose)
    {
        var id = options.Value.ActiveEncryptionKeyId;
        var key = Key(id);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plain = Encoding.UTF8.GetBytes(value);
        var cipher = new byte[plain.Length];
        var tag = new byte[16];
        try
        {
            using var aes = new AesGcm(key, 16);
            aes.Encrypt(nonce, plain, cipher, tag, Encoding.UTF8.GetBytes(purpose));
            return $"{id}.{Convert.ToBase64String(nonce)}.{Convert.ToBase64String(tag)}.{Convert.ToBase64String(cipher)}";
        }
        finally { CryptographicOperations.ZeroMemory(key); CryptographicOperations.ZeroMemory(plain); }
    }
    public string Unprotect(string value, string purpose)
    {
        try
        {
            var parts = value.Split('.');
            if (parts.Length != 4) throw new CryptographicException();
            var key = Key(parts[0]);
            var cipher = Convert.FromBase64String(parts[3]);
            var plain = new byte[cipher.Length];
            try
            {
                using var aes = new AesGcm(key, 16);
                aes.Decrypt(Convert.FromBase64String(parts[1]), cipher, Convert.FromBase64String(parts[2]), plain, Encoding.UTF8.GetBytes(purpose));
                return Encoding.UTF8.GetString(plain);
            }
            finally { CryptographicOperations.ZeroMemory(key); CryptographicOperations.ZeroMemory(plain); }
        }
        catch (Exception ex) when (ex is FormatException or CryptographicException or System.ArgumentException)
        { throw new AccountException(503, "PLATFORM_TOKEN_UNAVAILABLE", "Không thể đọc thông tin kết nối; kiểm tra khóa mã hóa."); }
    }
    private byte[] Key(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id) || id.Contains('.') || !options.Value.EncryptionKeys.TryGetValue(id, out var value))
                throw new FormatException();
            var key = Convert.FromBase64String(value);
            if (key.Length != 32) throw new FormatException();
            return key;
        }
        catch (FormatException)
        { throw new AccountException(503, "PLATFORM_ENCRYPTION_NOT_CONFIGURED", "Cần cấu hình khóa mã hóa token 256-bit."); }
    }
}
