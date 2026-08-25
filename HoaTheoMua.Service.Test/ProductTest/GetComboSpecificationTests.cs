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
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;

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
            mockDbContext.Setup(x => x.ComboSpecifications).Returns(mockComboSpecsDbSet.Object);

            var configs = new List<ConfigEntity>().BuildMockDbSet();
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

        /// <summary>
        /// Test: Khi không tìm thấy combo với ID truyền vào, hàm ném ra NotFoundException.
        /// </summary>
        [Fact]
        public async Task GetComboSpecification_ComboNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dbContext = CreateMockDbContext(new List<ProductEntity>(), new List<ComboSpecEntity>());
            var service = CreateService(dbContext);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => service.GetComboSpecificationAsync(Guid.NewGuid()));
        }

        /// <summary>
        /// Test: Khi combo đã bị xóa (IsDeleted = true), hàm ném ra NotFoundException.
        /// </summary>
        [Fact]
        public async Task GetComboSpecification_DeletedCombo_ThrowsNotFoundException()
        {
            // Arrange
            var comboId = Guid.NewGuid();
            var combo = new ProductEntity
            {
                Id = comboId,
                ProductName = "Deleted Combo",
                IsDeleted = true
            };
            var mat = new ProductEntity
            {
                Id = Guid.NewGuid(),
                ProductName = "Material 1",
                IsDeleted = false
            };
            var products = new List<ProductEntity> { combo, mat };
            var comboSpecs = new List<ComboSpecEntity>
            {
                new ComboSpecEntity
                {
                    SubId = comboId,
                    Sub = combo,
                    MatId = mat.Id,
                    Mat = mat,
                    Quantity = 1,
                    IsCore = true,
                    IsDeleted = false
                }
            };

            var dbContext = CreateMockDbContext(products, comboSpecs);
            var service = CreateService(dbContext);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => service.GetComboSpecificationAsync(comboId));
        }

        /// <summary>
        /// Test: Khi combo tồn tại nhưng tất cả các nguyên liệu liên quan đều đã bị xóa, trả về danh sách Materials rỗng.
        /// </summary>
        [Fact]
        public async Task GetComboSpecification_ComboFoundButNoAvailableMaterials_ReturnsEmptyMaterials()
        {
            // Arrange
            var comboId = Guid.NewGuid();
            var combo = new ProductEntity
            {
                Id = comboId,
                ProductCode = "EMPTY-COMBO",
                ProductName = "Empty Combo",
                IsDeleted = false
            };
            var deletedMaterial = new ProductEntity
            {
                Id = Guid.NewGuid(),
                ProductName = "Deleted Material",
                IsDeleted = true
            };
            var products = new List<ProductEntity>
            {
                combo,
                deletedMaterial
            };
            var comboSpecs = new List<ComboSpecEntity>
            {
                new ComboSpecEntity
                {
                    SubId = comboId,
                    Sub = combo,
                    MatId = deletedMaterial.Id,
                    Mat = deletedMaterial,
                    Quantity = 1,
                    IsCore = true,
                    IsActive = true,
                    IsDeleted = false
                }
            };
            var dbContext = CreateMockDbContext(products, comboSpecs);
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetComboSpecificationAsync(comboId);

            // Assert
            Assert.Equal(comboId.ToString(), result.SubId);
            Assert.Equal("EMPTY-COMBO", result.ProductCode);
            Assert.Equal("Empty Combo", result.ProductName);
            Assert.Empty(result.Materials);
        }

        /// <summary>
        /// Test: Kiểm tra sắp xếp theo IsCore giảm dần (IsCore = true đứng trước IsCore = false)
        /// và tính toán MaxComboPossible chính xác cho từng thành phần/phụ liệu.
        /// </summary>
        [Fact]
        public async Task GetComboSpecification_WithMaterials_CalculatesMaxComboAndSortsByIsCoreCorrectly()
        {
            // Arrange
            var comboId = Guid.NewGuid();
            var combo = new ProductEntity
            {
                Id = comboId,
                ProductName = "Full Combo",
                IsDeleted = false
            };
            var coreMat1 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Core 1", AvailableForSale = 10, IsDeleted = false };
            var coreMat2 = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Core 2", AvailableForSale = 40, IsDeleted = false };
            var supportMat = new ProductEntity { Id = Guid.NewGuid(), ProductName = "Support 1", AvailableForSale = 0, IsDeleted = false };
            
            var products = new List<ProductEntity>
            {
                combo,
                coreMat1, coreMat2, supportMat
            };

            var comboSpecs = new List<ComboSpecEntity>
            {
                new ComboSpecEntity { SubId = comboId, Sub = combo, MatId = coreMat1.Id, Mat = coreMat1, Quantity = 2, IsCore = true, IsActive = true, IsDeleted = false }, // max = 10 / 2 = 5
                new ComboSpecEntity { SubId = comboId, Sub = combo, MatId = supportMat.Id, Mat = supportMat, Quantity = 1, IsCore = false, IsActive = true, IsDeleted = false }, // max = 0 / 1 = 0 (support)
                new ComboSpecEntity { SubId = comboId, Sub = combo, MatId = coreMat2.Id, Mat = coreMat2, Quantity = 5, IsCore = true, IsActive = true, IsDeleted = false }, // max = 40 / 5 = 8
            };

            var dbContext = CreateMockDbContext(products, comboSpecs);
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetComboSpecificationAsync(comboId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Materials.Count);
            
            // Core materials (IsCore = true) đứng trước support materials (IsCore = false)
            Assert.True(result.Materials[0].IsCore);
            Assert.True(result.Materials[1].IsCore);
            Assert.False(result.Materials[2].IsCore);
            
            // Kiểm tra số lượng combo tối đa tính được cho từng nguyên liệu
            var core1Item = result.Materials.First(m => m.MaterialId == coreMat1.Id.ToString());
            Assert.Equal(5, core1Item.MaxComboPossible);
            Assert.True(core1Item.IsCore);

            var core2Item = result.Materials.First(m => m.MaterialId == coreMat2.Id.ToString());
            Assert.Equal(8, core2Item.MaxComboPossible);
            Assert.True(core2Item.IsCore);

            var supportItem = result.Materials.First(m => m.MaterialId == supportMat.Id.ToString());
            Assert.Equal(0, supportItem.MaxComboPossible);
            Assert.False(supportItem.IsCore);
        }

        /// <summary>
        /// Test: Kiểm tra mapping đầy đủ các trường thông tin bao gồm IsCore, MaterialId, MaterialName, Unit, AvailableForSale, QuantityPerCombo.
        /// </summary>
        [Fact]
        public async Task GetComboSpecification_FieldMapping_ReturnsExpectedFields()
        {
            // Arrange
            var comboId = Guid.NewGuid();
            var combo = new ProductEntity
            {
                Id = comboId,
                ProductCode = "COMBO-FLOWER-01",
                ProductName = "Bó Hoa Tình Yêu",
                IsDeleted = false
            };
            var matId = Guid.NewGuid();
            var material = new ProductEntity
            {
                Id = matId,
                ProductName = "Hoa Hồng Đỏ",
                Unit = "Cành",
                AvailableForSale = 100,
                IsDeleted = false
            };

            var products = new List<ProductEntity> { combo, material };
            var comboSpecs = new List<ComboSpecEntity>
            {
                new ComboSpecEntity
                {
                    SubId = comboId,
                    Sub = combo,
                    MatId = matId,
                    Mat = material,
                    Quantity = 10,
                    IsCore = true,
                    IsActive = true,
                    IsDeleted = false
                }
            };

            var dbContext = CreateMockDbContext(products, comboSpecs);
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetComboSpecificationAsync(comboId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(comboId.ToString(), result.SubId);
            Assert.Equal("COMBO-FLOWER-01", result.ProductCode);
            Assert.Equal("Bó Hoa Tình Yêu", result.ProductName);
            Assert.Single(result.Materials);

            var matItem = result.Materials[0];
            Assert.Equal(matId.ToString(), matItem.MaterialId);
            Assert.Equal("Hoa Hồng Đỏ", matItem.MaterialName);
            Assert.True(matItem.IsCore);
            Assert.Equal(10, matItem.QuantityPerCombo);
            Assert.Equal("Cành", matItem.Unit);
            Assert.Equal(100, matItem.AvailableForSale);
            Assert.Equal(10, matItem.MaxComboPossible); // 100 / 10 = 10
        }

        /// <summary>
        /// Test: Khi AvailableForSale của nguyên liệu bằng null, hàm xử lý an toàn trả về AvailableForSale = 0 và MaxComboPossible = 0.
        /// </summary>
        [Fact]
        public async Task GetComboSpecification_MaterialWithNullAvailableForSale_ReturnsZeroForSaleAndMaxCombo()
        {
            // Arrange
            var comboId = Guid.NewGuid();
            var combo = new ProductEntity
            {
                Id = comboId,
                ProductName = "Combo Test",
                IsDeleted = false
            };
            var material = new ProductEntity
            {
                Id = Guid.NewGuid(),
                ProductName = "Ruy Băng",
                Unit = "Cuộn",
                AvailableForSale = null,
                IsDeleted = false
            };

            var products = new List<ProductEntity> { combo, material };
            var comboSpecs = new List<ComboSpecEntity>
            {
                new ComboSpecEntity
                {
                    SubId = comboId,
                    Sub = combo,
                    MatId = material.Id,
                    Mat = material,
                    Quantity = 2,
                    IsCore = false,
                    IsActive = true,
                    IsDeleted = false
                }
            };

            var dbContext = CreateMockDbContext(products, comboSpecs);
            var service = CreateService(dbContext);

            // Act
            var result = await service.GetComboSpecificationAsync(comboId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Materials);
            var item = result.Materials[0];
            Assert.Equal(0, item.AvailableForSale);
            Assert.Equal(0, item.MaxComboPossible);
            Assert.False(item.IsCore);
        }
    }
}
