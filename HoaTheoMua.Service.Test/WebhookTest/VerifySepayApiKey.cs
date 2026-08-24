using System.Collections.Generic;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using WebhookServiceImpl = HoaTheoMua.Service.Webhook.Service;

namespace HoaTheoMua.Service.Test.WebhookTest
{
    public class VerifySepayApiKeyTests
    {
        private const string ConfiguredApiKey = "sepay_test_api_key";

        private static AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new AppDbContext(options);
        }

        private static WebhookServiceImpl CreateService(string? apiKey = ConfiguredApiKey)
        {
            var settings = new Dictionary<string, string?>();
            if (apiKey != null)
            {
                settings["SepayOption:ApiKey"] = apiKey;
            }

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            // Everything Ahamove/NhanhVn related moved into Fulfillment.Service, so the webhook
            // now needs only the DbContext, its own SepayOption and the fulfilment service.
            var mockFulfillmentService = new Mock<HoaTheoMua.Service.Fulfillment.IService>();
            return new WebhookServiceImpl(
                CreateInMemoryDbContext(),
                configuration,
                mockFulfillmentService.Object);
        }

        [Fact]
        public void VerifySepayApiKey_ValidHeader_DoesNotThrow()
        {
            var service = CreateService();

            var exception = Record.Exception(
                () => service.VerifySepayApiKey($"Apikey {ConfiguredApiKey}"));

            Assert.Null(exception);
        }

        [Fact]
        public void VerifySepayApiKey_ApiKeyNotConfigured_ThrowsServerException()
        {
            var service = CreateService(apiKey: null);

            Assert.Throws<ServerException>(
                () => service.VerifySepayApiKey($"Apikey {ConfiguredApiKey}"));
        }

        [Fact]
        public void VerifySepayApiKey_MissingHeader_ThrowsUnauthorizedException()
        {
            var service = CreateService();

            Assert.Throws<UnauthorizedException>(() => service.VerifySepayApiKey(null));
        }

        [Fact]
        public void VerifySepayApiKey_WrongScheme_ThrowsUnauthorizedException()
        {
            var service = CreateService();

            Assert.Throws<UnauthorizedException>(
                () => service.VerifySepayApiKey($"Bearer {ConfiguredApiKey}"));
        }

        [Fact]
        public void VerifySepayApiKey_WrongKey_ThrowsUnauthorizedException()
        {
            var service = CreateService();

            Assert.Throws<UnauthorizedException>(
                () => service.VerifySepayApiKey("Apikey wrong_key"));
        }
    }
}
