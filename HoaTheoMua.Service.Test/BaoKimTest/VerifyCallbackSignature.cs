using System.Security.Cryptography;
using HoaTheoMua.Service.BaoKim;
using HoaTheoMua.Service.Exceptions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using BaoKimServiceImpl = HoaTheoMua.Service.BaoKim.Service;

namespace HoaTheoMua.Service.Test.BaoKimTest
{
    // Callback endpoint /api/v1/baokim phải anonymous (BaoKim không gửi được JWT), nên chữ ký là
    // thứ duy nhất chặn giữa một POST bất kỳ và việc điều tài xế đi giao hàng miễn phí.
    public class VerifyCallbackSignatureTest
    {
        private const string SampleBody =
            """{"request_id":"REQ-1","operation":"PAYMENT_TRANS","payment_result":{"result_code":200}}""";

        // Helpers

        private static (string PrivatePem, string PublicPem) CreateKeyPair()
        {
            using var rsa = RSA.Create(2048);
            return (rsa.ExportPkcs8PrivateKeyPem(), rsa.ExportSubjectPublicKeyInfoPem());
        }

        private static BaoKimServiceImpl CreateService(string publicKeyPem)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["BaoKimOption:BaseUrl"] = "https://devtest.baokim.vn",
                    ["BaoKimOption:MerchantCode"] = "hoatheomua",
                    ["BaoKimOption:ClientId"] = "hoatheomua",
                    ["BaoKimOption:ClientSecret"] = "secret",
                    ["BaoKimOption:PrivateKey"] = "",
                    ["BaoKimOption:PublicKey"] = publicKeyPem
                })
                .Build();

            return new BaoKimServiceImpl(
                new HttpClient(new Mock<HttpMessageHandler>().Object), configuration);
        }

        // TEST CASE 01: Chữ ký hợp lệ do đúng khoá của BaoKim ký -> chấp nhận.
        [Fact]
        public void VerifyCallbackSignature_ValidSignature_DoesNotThrow()
        {
            // Arrange
            var (privatePem, publicPem) = CreateKeyPair();
            var signature = BaoKimSigner.Sign(SampleBody, privatePem);
            var service = CreateService(publicPem);

            // Act
            var exception = Record.Exception(() => service.VerifyCallbackSignature(SampleBody, signature));

            // Assert
            Assert.Null(exception);
        }

        // TEST CASE 02: Body bị sửa sau khi ký -> từ chối.
        // Đây chính là kịch bản kẻ tấn công đổi total_amount hoặc mrc_order_id.
        [Fact]
        public void VerifyCallbackSignature_TamperedBody_ThrowsUnauthorizedException()
        {
            // Arrange
            var (privatePem, publicPem) = CreateKeyPair();
            var signature = BaoKimSigner.Sign(SampleBody, privatePem);
            var service = CreateService(publicPem);

            var tamperedBody = SampleBody.Replace("\"result_code\":200", "\"result_code\":999");

            // Act
            var act = () => service.VerifyCallbackSignature(tamperedBody, signature);

            // Assert
            var exception = Assert.Throws<UnauthorizedException>(act);
            Assert.Contains("Invalid BaoKim callback signature", exception.Message);
        }

        // TEST CASE 03: Chữ ký ký bằng khoá KHÁC (kẻ tấn công tự tạo keypair) -> từ chối.
        [Fact]
        public void VerifyCallbackSignature_SignedByDifferentKey_ThrowsUnauthorizedException()
        {
            // Arrange: attacker tự ký bằng khoá riêng của họ.
            var (attackerPrivatePem, _) = CreateKeyPair();
            var (_, baoKimPublicPem) = CreateKeyPair();

            var forgedSignature = BaoKimSigner.Sign(SampleBody, attackerPrivatePem);
            var service = CreateService(baoKimPublicPem);

            // Act
            var act = () => service.VerifyCallbackSignature(SampleBody, forgedSignature);

            // Assert
            var exception = Assert.Throws<UnauthorizedException>(act);
            Assert.Contains("Invalid BaoKim callback signature", exception.Message);
        }

        // TEST CASE 04: Không có header Signature -> từ chối (không mặc định cho qua).
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void VerifyCallbackSignature_MissingSignature_ThrowsUnauthorizedException(string? signature)
        {
            // Arrange
            var (_, publicPem) = CreateKeyPair();
            var service = CreateService(publicPem);

            // Act
            var act = () => service.VerifyCallbackSignature(SampleBody, signature);

            // Assert
            var exception = Assert.Throws<UnauthorizedException>(act);
            Assert.Contains("Missing BaoKim callback signature", exception.Message);
        }

        // TEST CASE 05: Signature không phải base64 -> 401, KHÔNG được để FormatException lọt ra 500.
        [Fact]
        public void VerifyCallbackSignature_NonBase64Signature_ThrowsUnauthorizedNotServerError()
        {
            // Arrange
            var (_, publicPem) = CreateKeyPair();
            var service = CreateService(publicPem);

            // Act
            var act = () => service.VerifyCallbackSignature(SampleBody, "khong-phai-base64!!!");

            // Assert
            var exception = Assert.Throws<UnauthorizedException>(act);
            Assert.Contains("Invalid BaoKim callback signature", exception.Message);
        }

        // TEST CASE 06: Chưa cấu hình PublicKey -> fail closed (ServerException), tuyệt đối không
        // được im lặng cho qua, vì khi đó callback giả cũng được fulfil.
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void VerifyCallbackSignature_PublicKeyNotConfigured_FailsClosed(string publicKey)
        {
            // Arrange
            var (privatePem, _) = CreateKeyPair();
            var signature = BaoKimSigner.Sign(SampleBody, privatePem);
            var service = CreateService(publicKey);

            // Act
            var act = () => service.VerifyCallbackSignature(SampleBody, signature);

            // Assert
            var exception = Assert.Throws<ServerException>(act);
            Assert.Contains("PublicKey is not configured", exception.Message);
        }

        // TEST CASE 07: PublicKey cấu hình sai định dạng PEM -> ServerException (500).
        // Đây là lỗi cấu hình phía mình, không được báo thành "chữ ký sai" (401) của người gọi:
        // 401 sẽ khiến ta đi truy sai hướng khi thực ra chỉ là dán nhầm key.
        [Fact]
        public void VerifyCallbackSignature_MalformedPublicKey_FailsAsServerErrorNotUnauthorized()
        {
            // Arrange
            var (privatePem, _) = CreateKeyPair();
            var signature = BaoKimSigner.Sign(SampleBody, privatePem);
            var service = CreateService("-----BEGIN PUBLIC KEY-----\nkhong-phai-key\n-----END PUBLIC KEY-----");

            // Act
            var act = () => service.VerifyCallbackSignature(SampleBody, signature);

            // Assert
            var exception = Assert.Throws<ServerException>(act);
            Assert.Contains("not a usable PEM public key", exception.Message);
        }
    }
}
