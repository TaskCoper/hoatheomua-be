using System.Text.Json;
using HoaTheoMua.Service.Order;
using Xunit;

namespace HoaTheoMua.Service.Test.OrderTest
{
    // Direct tests for the custom Order.Converter applied to OrderListRootResponse.Data.
    // It normalizes NhanhVn's "data" field: a real array becomes a populated list, while
    // the API's "{}"-instead-of-array quirk (and an empty array) must degrade to an empty
    // list rather than throwing. Deserialization uses Web defaults to match the service,
    // which reads the payload via HttpContent.ReadFromJsonAsync.
    public class OrderDataConverterTests
    {
        private static readonly JsonSerializerOptions Options =
            new JsonSerializerOptions(JsonSerializerDefaults.Web);

        [Fact]
        public void Converter_DataIsArray_ParsesAllItems()
        {
            var json = "{\"code\":1,\"data\":[{\"info\":{\"id\":101}},{\"info\":{\"id\":102}}]}";

            var result = JsonSerializer.Deserialize<OrderListRootResponse>(json, Options);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Data.Count);
            Assert.Equal(101, result.Data[0].Info.Id);
            Assert.Equal(102, result.Data[1].Info.Id);
        }

        [Fact]
        public void Converter_DataIsEmptyObject_ReturnsEmptyList()
        {
            // NhanhVn returns {} instead of [] when there are no orders; the converter
            // exists specifically so this does not blow up deserialization.
            var json = "{\"code\":1,\"data\":{}}";

            var result = JsonSerializer.Deserialize<OrderListRootResponse>(json, Options);

            Assert.NotNull(result);
            Assert.Empty(result!.Data);
        }

        [Fact]
        public void Converter_DataIsEmptyArray_ReturnsEmptyList()
        {
            var json = "{\"code\":1,\"data\":[]}";

            var result = JsonSerializer.Deserialize<OrderListRootResponse>(json, Options);

            Assert.NotNull(result);
            Assert.Empty(result!.Data);
        }
    }
}
