using HoaTheoMua.Service.Common;
using Xunit;

namespace HoaTheoMua.Service.Test.CommonTest;

public class DeliverySlotTimeHelperTests
{
    [Fact]
    public void IsWithinSlot_CreatedAtInsideNormalSlot_ReturnsTrue()
    {
        var createdAt = new DateTimeOffset(2026, 10, 8, 8, 30, 0, TimeSpan.FromHours(7));

        var result = DeliverySlotTimeHelper.IsWithinSlot("8h - 10h", createdAt);

        Assert.True(result);
    }

    [Fact]
    public void IsWithinSlot_CreatedAtOutsideNormalSlot_ReturnsFalse()
    {
        var createdAt = new DateTimeOffset(2026, 10, 8, 11, 0, 0, TimeSpan.FromHours(7));

        var result = DeliverySlotTimeHelper.IsWithinSlot("8h - 10h", createdAt);

        Assert.False(result);
    }
}
