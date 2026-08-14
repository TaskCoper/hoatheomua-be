using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Common;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Xunit;
using OrderService = HoaTheoMua.Service.Order.Service;
using ProductService = HoaTheoMua.Service.Product.IService;

namespace HoaTheoMua.Service.Test.OrderTest;

public class GetLimitCoreProductTest
{
    private static Repository.Entity.Product CreatMaterial(string name, decimal availableForSale)
    {
        return new Repository.Entity.Product()
        {
            Id = Guid.NewGuid(),
            ProductName = name,
            AvailableForSale = availableForSale,
            IsDeleted = false,
        };
    }

    private ComboSpecification CreateComboSpecification(
        Guid comboId, Repository.Entity.Product material, int quantity, bool isCore, bool isActive)
    {
        return new ComboSpecification()
        {
            Id = Guid.NewGuid(),
            SubId = comboId,
            MatId = material.Id,
            Quantity = quantity,
            IsCore = isCore,
            IsActive = isActive,
            IsDeleted = false,
            Mat = material,
        };
    }

    private OrderService CreateOrderService(List<ComboSpecification>  comboSpecifications)
    {
        var mockSpecifications = comboSpecifications.BuildMockDbSet();
        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var mockDbContext = new Mock<AppDbContext>(options);
        mockDbContext
            .Setup(x => x.ComboSpecifications)
            .Returns(mockSpecifications.Object);

        var currentUser = new Mock<ICurrentUserService>();
        var productService = new Mock<ProductService>();
        var baoKimService = new Mock<HoaTheoMua.Service.BaoKim.IService>();
        var configuration = new ConfigurationBuilder().Build();
        
        return new OrderService(
            mockDbContext.Object,
            currentUser.Object,
            new HttpClient(),
            productService.Object,
            configuration,
            baoKimService.Object);
    }
    
    [Fact]
    public async Task GetLimitCoreProduct_ComboA_ReturnsTwo()
    {
        var comboAId = Guid.NewGuid();
        
        var hoaMai = CreatMaterial("Hoa Mai", 37m);
        var hoaDao = CreatMaterial("Hoa Đào", 4m);
        var hoaXanh = CreatMaterial("Hoa Xanh", 4m);
        var hoaCuc = CreatMaterial("Hoa Cúc", 0m);

        var specifications = new List<ComboSpecification>
        {
            CreateComboSpecification(comboAId, hoaMai, quantity: 6, isCore: true, isActive: true),
            CreateComboSpecification(comboAId, hoaDao, quantity: 2, isCore: true, isActive: true),
            CreateComboSpecification(comboAId, hoaXanh, quantity: 4, isCore: true, isActive: false),
            CreateComboSpecification(comboAId, hoaCuc, quantity: 3, isCore: false, isActive: true)
        };

        var service = CreateOrderService(specifications);
        
        var result = await service.GetLimitCoreProduct(comboAId);
        
        Assert.Equal(2m, result.SellableQuantityCombo);
        Assert.Equal(3, result.Materials.Count);
        Assert.Contains(result.Materials, x => x.MatId == hoaCuc.Id);
        Assert.DoesNotContain(result.Materials, x => x.MatId == hoaXanh.Id);
    }

    [Fact]
    public async Task GetLimitCoreProduct_ComboB_HasNoCore_ThrowsBadRequestException()
    {
        var comboBId = Guid.NewGuid();

        var hoaCuc = CreatMaterial("Hoa Cúc", 3m);
        var hoaVang = CreatMaterial("Hoa Vàng", 9m);

        var specifications = new List<ComboSpecification>
        {
            CreateComboSpecification(comboBId, hoaCuc, quantity: 1, isCore: false, isActive: true),
            CreateComboSpecification(comboBId, hoaVang, quantity: 3, isCore: false, isActive: true)
        };

        var service = CreateOrderService(specifications);
        
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.GetLimitCoreProduct(comboBId));

        Assert.Equal("Tồn tại vật liệu core với số lượng <= 0", exception.Message);
    }
    
    [Fact]
    public async Task GetLimitCoreProduct_ComboB_HasCore_EQUAL_ZERO()
    {
        var comboBId = Guid.NewGuid();

        var hoaCuc = CreatMaterial("Hoa Cúc", 3m);
        var hoaVang = CreatMaterial("Hoa Vàng", 9m);

        var specifications = new List<ComboSpecification>
        {
            CreateComboSpecification(comboBId, hoaCuc, quantity: 10, isCore: true, isActive: true),
            CreateComboSpecification(comboBId, hoaVang, quantity: 3, isCore: false, isActive: true)
        };

        var service = CreateOrderService(specifications);
        
        var result = await service.GetLimitCoreProduct(comboBId);
        
        Assert.Equal(0m, result.SellableQuantityCombo);
        Assert.Equal(2, result.Materials.Count);
    }
    
    
}
