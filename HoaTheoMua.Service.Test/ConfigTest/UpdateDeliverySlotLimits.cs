using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;
using ConfigRequest = HoaTheoMua.Service.Config.Request;
using ConfigResponse = HoaTheoMua.Service.Config.Response;
using ConfigService = HoaTheoMua.Service.Config.Service;

namespace HoaTheoMua.Service.Test.ConfigTest;

public class UpdateDeliverySlotLimitsTests
{
    private const string ValidConfigJson = """
        {
          "group": "nhanhvn",
          "label": "Giờ giao hàng",
          "value": [229, 234],
          "slotLimits": { "229": 5, "234": null },
          "options": [
            { "label": "8h - 10h", "value": 229 },
            { "label": "10h - 12h", "value": 234 },
            { "label": "VAT", "value": 253 }
          ],
          "required": true,
          "inputType": "multiSelect",
          "validations": []
        }
        """;

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static ConfigService CreateService(AppDbContext dbContext)
    {
        return new ConfigService(dbContext, new Mock<ICurrentUserService>().Object);
    }

    private static ConfigEntity CreateConfig(string value = ValidConfigJson)
    {
        return new ConfigEntity
        {
            Id = Guid.NewGuid(),
            Key = "nhanhvn.order_tags",
            Group = "nhanhvn",
            Kind = "setting",
            Value = value,
            IsDeleted = false,
            UserId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    [Fact]
    public async Task UpdateDeliverySlotLimits_ValidRequest_UpdatesOnlyRequestedLimits()
    {
        await using var dbContext = CreateDbContext();
        var entity = CreateConfig();
        dbContext.Configs.Add(entity);
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var result = await service.UpdateDeliverySlotLimits(new ConfigRequest.UpdateDeliverySlotLimitsRequest
        {
            Slots =
            [
                new ConfigRequest.DeliverySlotLimitInput { TagId = 229, Limit = 20 }
            ]
        });

        var savedConfig = JsonSerializer.Deserialize<ConfigResponse.ConfigItem>(entity.Value)!;
        Assert.Equal(20, savedConfig.SlotLimits[229]);
        Assert.Null(savedConfig.SlotLimits[234]);
        Assert.Single(result.Slots);
        Assert.Equal("8h - 10h", result.Slots[0].Label);
        Assert.Equal(entity.UpdatedAt, result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateDeliverySlotLimits_EmptyRequest_ThrowsBadRequestException()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UpdateDeliverySlotLimits(new ConfigRequest.UpdateDeliverySlotLimitsRequest
            {
                Slots = []
            }));

        Assert.Equal("Slot không được để trống", exception.Message);
    }

    [Fact]
    public async Task UpdateDeliverySlotLimits_NegativeLimit_ThrowsBadRequestException()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UpdateDeliverySlotLimits(new ConfigRequest.UpdateDeliverySlotLimitsRequest
            {
                Slots = [new ConfigRequest.DeliverySlotLimitInput { TagId = 229, Limit = -1 }]
            }));

        Assert.Equal("Giới hạn phải lớn hơn hoặc bằng 0", exception.Message);
    }

    [Fact]
    public async Task UpdateDeliverySlotLimits_DuplicateTagId_ThrowsBadRequestException()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UpdateDeliverySlotLimits(new ConfigRequest.UpdateDeliverySlotLimitsRequest
            {
                Slots =
                [
                    new ConfigRequest.DeliverySlotLimitInput { TagId = 229, Limit = 10 },
                    new ConfigRequest.DeliverySlotLimitInput { TagId = 229, Limit = 20 }
                ]
            }));

        Assert.Equal("Nhãn không được trùng", exception.Message);
    }

    [Fact]
    public async Task UpdateDeliverySlotLimits_TagIdNotInValue_ThrowsBadRequestException()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Configs.Add(CreateConfig());
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UpdateDeliverySlotLimits(new ConfigRequest.UpdateDeliverySlotLimitsRequest
            {
                Slots = [new ConfigRequest.DeliverySlotLimitInput { TagId = 253, Limit = 10 }]
            }));

        Assert.Equal("TagId 253 không phải là tag giao hàng theo khung giờ", exception.Message);
    }

    [Fact]
    public async Task UpdateDeliverySlotLimits_TagIdMissingFromOptions_ThrowsBadRequestException()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Configs.Add(CreateConfig("""
            {
              "value": [229],
              "slotLimits": { "229": null },
              "options": []
            }
            """));
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UpdateDeliverySlotLimits(new ConfigRequest.UpdateDeliverySlotLimitsRequest
            {
                Slots = [new ConfigRequest.DeliverySlotLimitInput { TagId = 229, Limit = 10 }]
            }));

        Assert.Equal("TagId 229 không tồn tại trong mục options", exception.Message);
    }

    [Fact]
    public async Task UpdateDeliverySlotLimits_ConfigNotFound_ThrowsBadRequestException()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UpdateDeliverySlotLimits(new ConfigRequest.UpdateDeliverySlotLimitsRequest
            {
                Slots = [new ConfigRequest.DeliverySlotLimitInput { TagId = 229, Limit = 10 }]
            }));

        Assert.Equal("Không tìm thấy cấu hình của nhãn", exception.Message);
    }
}
