using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoaTheoMua.Repository;
using CategoryProductEntity = HoaTheoMua.Repository.Entity.CategoryProduct;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.CategoryProduct;
using HoaTheoMua.Service.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;
using CategoryProductService = HoaTheoMua.Service.CategoryProduct.Service;

namespace HoaTheoMua.Service.Test.CategoryProductTest;

public class GetCategoryWithChildTest
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }

    private static CategoryProductService CreateService(AppDbContext dbContext)
    {
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(x => x.GetRequiredUserId()).Returns(UserId);
        return new CategoryProductService(dbContext, currentUserMock.Object);
    }

    [Fact]
    public async Task GetCategoryWithChild_PageIsString_ReturnsMatch()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Products",
            Name_En = "Products",
            Type = CategoryProductType.Fix,
            IsActive = true,
            Page = new List<string> { "Products", "1" }
        };

        var child = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Flowers",
            Name_En = "Flowers",
            Type = CategoryProductType.Fix,
            ParentCategoryProductId = parent.Id,
            IsActive = true,
            Page = new List<string>()
        };

        await dbContext.CategoryProducts.AddRangeAsync(parent, child);
        await dbContext.SaveChangesAsync();

        var request = new Request.GetCategoryProductWithChild
        {
            page = new List<string> { "Products" }
        };

        var result = await service.GetCategoryWithChild(request);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Products", result[0].Name);
        Assert.NotNull(result[0].Children);
        Assert.Single(result[0].Children!);
        Assert.Equal("Flowers", result[0].Children![0].Name);
    }

    [Fact]
    public async Task GetCategoryWithChild_ChildPageMatches_ReturnsAncestors()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = CreateService(dbContext);

        var parent = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Parent",
            Name_En = "Parent",
            Type = CategoryProductType.Fix,
            IsActive = true,
            Page = new List<string>()
        };

        var child = new CategoryProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Products",
            Name_En = "Products",
            Type = CategoryProductType.Fix,
            ParentCategoryProductId = parent.Id,
            IsActive = true,
            Page = new List<string> { "Products" }
        };

        await dbContext.CategoryProducts.AddRangeAsync(parent, child);
        await dbContext.SaveChangesAsync();

        var request = new Request.GetCategoryProductWithChild
        {
            page = new List<string> { "Products" }
        };

        var result = await service.GetCategoryWithChild(request);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Parent", result[0].Name);
        Assert.NotNull(result[0].Children);
        Assert.Single(result[0].Children!);
        Assert.Equal("Products", result[0].Children![0].Name);
    }
}
