using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Xunit;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using ProductService = HoaTheoMua.Service.Product.Service;
using ComboSpecEntity = HoaTheoMua.Repository.Entity.ComboSpecification;
  
namespace HoaTheoMua.Service.Test.ProductTest
{
    public class GetComboSpecificationTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContext;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IMapper> _mockMapper;
        private readonly HttpClient _httpClient;

        public GetComboSpecificationTests()
        {
            _mockHttpContext = new Mock<IHttpContextAccessor>();
            _mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
            _mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);

            _mockMapper = new Mock<IMapper>();
            _httpClient = new HttpClient();
        }

        private AppDbContext CreateMockDbContext(List<ProductEntity> products, List<ComboSpecEntity> comboSpecs)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var mockDbContext = new Mock<AppDbContext>(options);

            var mockProductDbSet = products.BuildMockDbSet();
            mockDbContext.Setup(x => x.Products).Returns(mockProductDbSet.Object);

            var mockComboSpecsDbSet = comboSpecs.BuildMockDbSet();
            mockDbContext.Setup(x => x.ComboSpecification).Returns(mockComboSpecsDbSet.Object);

            var configs = new List<HoaTheoMua.Repository.Entity.Config>().BuildMockDbSet();
            mockDbContext.Setup(x => x.Configs).Returns(configs.Object);

            return mockDbContext.Object;
        }

        private ProductService CreateService(AppDbContext dbContext)
        {
            return new ProductService(
                dbContext,
                _mockHttpContext.Object,
                _httpClient,
                _mockConfig.Object,
                _mockMapper.Object
            );
        }

        [Fact]
        public async Task GetComboSpecification_ProductNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<ProductEntity>(), new List<ComboSpecEntity>());
            var service = CreateService(dbContext);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => service.GetComboSpecificationAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetComboSpecification_NoMaterials_ReturnsEmptyMaterials()
        {
            // Arrange
            var comboId = Guid.NewGuid();
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = comboId, ProductName = "Empty Combo", IsDeleted = false }
            };
            var dbContext = CreateMockDbContext(products, new List<ComboSpecEntity>());
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetComboSpecificationAsync(comboId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(comboId.ToString(), result.SubId);
            Assert.Empty(result.Materials);
        }

        [Fact]
        public async Task GetComboSpecification_WithMaterials_CalculatesMaxComboAndSortsCorrectly()
        {
            // Arrange
            var comboId = Guid.NewGuid();
            var coreMat1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Core 1", AvailableForSale = 10, IsDeleted = false };
            var coreMat2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Core 2", AvailableForSale = 40, IsDeleted = false };
            var supportMat = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Support 1", AvailableForSale = 0, IsDeleted = false };
            
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = comboId, ProductName = "Full Combo", IsDeleted = false },
                coreMat1, coreMat2, supportMat
            };

            var comboSpecs = new List<ComboSpecEntity>
            {
                new ComboSpecEntity { SubId = comboId, MatId = coreMat1.Id, Mat = coreMat1, Quantity = 2, IsCore = true, IsActive = true }, // max = 5
                new ComboSpecEntity { SubId = comboId, MatId = supportMat.Id, Mat = supportMat, Quantity = 1, IsCore = false, IsActive = true }, // max = 0 (support)
                new ComboSpecEntity { SubId = comboId, MatId = coreMat2.Id, Mat = coreMat2, Quantity = 5, IsCore = true, IsActive = true }, // max = 8
            };

            var dbContext = CreateMockDbContext(products, comboSpecs);
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetComboSpecificationAsync(comboId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Materials.Count);
            
            // Note: According to new user's implementation, Role is a boolean (c.IsCore), and OrderBy(x => x.Role) sorts false (Support) before true (Core).
            // false is 0, true is 1 in sorting. So Support comes first!
            Assert.False(result.Materials[0].Role);
            Assert.True(result.Materials[1].Role);
            Assert.True(result.Materials[2].Role);
            
            // Verify individual max combos
            var core1Item = result.Materials.First(m => m.MaterialId == coreMat1.Id.ToString());
            Assert.Equal(5, core1Item.MaxComboPossible);

            var supportItem = result.Materials.First(m => m.MaterialId == supportMat.Id.ToString());
            Assert.Equal(0, supportItem.MaxComboPossible);
            
            var core2Item = result.Materials.First(m => m.MaterialId == coreMat2.Id.ToString());
            Assert.Equal(8, core2Item.MaxComboPossible);
        }
    }
}
