using AutoMapper;
using HoaTheoMua.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Xunit;
using ProductServiceImpl = HoaTheoMua.Service.Product.Service;
using ProductRequest = HoaTheoMua.Service.Product.Request;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;
using CategoryProductDetailEntity = HoaTheoMua.Repository.Entity.CategoryProductDetail;
using ProductDetailResponse = HoaTheoMua.Service.Product.Response.ProductDetailResponse;

namespace HoaTheoMua.Service.Test.ProductTest
{
    public class GetLocalProductsV2Tests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContext;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IMapper> _mockMapper;
        private readonly HttpClient _httpClient;

        public GetLocalProductsV2Tests()
        {
            _mockHttpContext = new Mock<IHttpContextAccessor>();
            _mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
            _mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);

            _mockMapper = new Mock<IMapper>();
            _mockMapper.Setup(m => m.Map<ProductDetailResponse>(It.IsAny<ProductEntity>()))
                .Returns((ProductEntity src) => new ProductDetailResponse { Id = Guid.NewGuid(), ProductName = src.ProductName, Price = src.Price });

            _httpClient = new HttpClient();
        }

        private AppDbContext CreateMockDbContext(List<ProductEntity> products)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var mockDbContext = new Mock<AppDbContext>(options);

            var mockProductDbSet = products.BuildMockDbSet();
            mockDbContext.Setup(x => x.Products).Returns(mockProductDbSet.Object);

            // Need to mock Configs for the constructor, similar to GetProductDetails.cs
            var configs = new List<ConfigEntity>().BuildMockDbSet();
            mockDbContext.Setup(x => x.Configs).Returns(configs.Object);

            return mockDbContext.Object;
        }

        private ProductServiceImpl CreateService(AppDbContext dbContext)
        {
            return new ProductServiceImpl(
                dbContext,
                _mockHttpContext.Object,
                _httpClient,
                _mockConfig.Object,
                _mockMapper.Object);
        }

        [Fact]
        public async Task GetLocalProductsV2_NoFilters_ReturnsAllValidProducts()
        {
            // Arrange
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Product 1", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Product 2", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Deleted Product", IsDeleted = true },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Child Product", IsDeleted = false, ParentProductId = "parent1" }
            };
            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);
            var request = new ProductRequest.LocalProductQueryRequest();

            // Act
            var result = await service.GetLocalProductsV2(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Value.TotalCount); // Excludes deleted and child products
            Assert.Equal(2, result.Value.Items.Count());
        }

        [Fact]
        public async Task GetLocalProductsV2_SearchKeyword_ReturnsMatchingProducts()
        {
            // Arrange
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Rose Combo", Barcode = "RC123", NhanhProductId = "", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Lily", Barcode = "LL456", NhanhProductId = "", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Sunflower", Barcode = "", NhanhProductId = "NHRC99", IsDeleted = false }
            };
            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);
            var request = new ProductRequest.LocalProductQueryRequest { search = "rc" };

            // Act
            var result = await service.GetLocalProductsV2(request);

            // Assert
            Assert.Equal(2, result.Value.TotalCount); // "Rose Combo" (Barcode has rc) and "Sunflower" (NhanhProductId has rc)
        }

        [Theory]
        [InlineData("Thành phần", 1)]
        [InlineData("Combo", 1)]
        [InlineData("Sản phẩm", 1)]
        [InlineData("InvalidType", 3)]
        public async Task GetLocalProductsV2_FilterByProductType_ReturnsCorrectTypes(string productType, int expectedCount)
        {
            // Arrange
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Material 1", ProductLabel = "Thành phần", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Combo 1", ProductType = "Combo", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Product 1", ProductType = "Sản phẩm", IsDeleted = false }
            };
            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);
            var request = new ProductRequest.LocalProductQueryRequest { productType = productType };

            // Act
            var result = await service.GetLocalProductsV2(request);

            // Assert
            Assert.Equal(expectedCount, result.Value.TotalCount);
        }

        [Fact]
        public async Task GetLocalProductsV2_FilterByPriceRange_ReturnsCorrectProducts()
        {
            // Arrange
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Cheap", Price = 50, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Medium", Price = 150, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Expensive", Price = 300, IsDeleted = false }
            };
            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);
            var request = new ProductRequest.LocalProductQueryRequest { priceFrom = 100, priceTo = 200 };

            // Act
            var result = await service.GetLocalProductsV2(request);

            // Assert
            Assert.Equal(1, result.Value.TotalCount);
            Assert.Contains(result.Value.Items, p => ((ProductDetailResponse)p).ProductName == "Medium");
        }

        [Fact]
        public async Task GetLocalProductsV2_FilterByCategory_ReturnsProductsInCat()
        {
            // Arrange
            var catId1 = Guid.NewGuid();
            var catId2 = Guid.NewGuid();
            var products = new List<ProductEntity>
            {
                new ProductEntity
                {
                    Id = Guid.NewGuid(), ProductName = "In Cat 1", IsDeleted = false,
                    CategoryProductDetails = new List<CategoryProductDetailEntity>
                    {
                        new CategoryProductDetailEntity { CategoryProductId = catId1, IsDeleted = false }
                    }
                },
                new ProductEntity
                {
                    Id = Guid.NewGuid(), ProductName = "In Cat 2", IsDeleted = false,
                    CategoryProductDetails = new List<CategoryProductDetailEntity>
                    {
                        new CategoryProductDetailEntity { CategoryProductId = catId2, IsDeleted = false }
                    }
                }
            };
            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);
            var request = new ProductRequest.LocalProductQueryRequest { categoryIds = new List<Guid> { catId1 } };

            // Act
            var result = await service.GetLocalProductsV2(request);

            // Assert
            Assert.Equal(1, result.Value.TotalCount);
            Assert.Contains(result.Value.Items, p => ((ProductDetailResponse)p).ProductName == "In Cat 1");
        }

        [Fact]
        public async Task GetLocalProductsV2_SortByPriceDesc_ReturnsSorted()
        {
            // Arrange
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "P1", Price = 100, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "P2", Price = 300, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "P3", Price = 200, IsDeleted = false }
            };
            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);
            var request = new ProductRequest.LocalProductQueryRequest { sortBy = new List<string> { "-price" } };

            // Act
            var result = await service.GetLocalProductsV2(request);

            // Assert
            var resultList = result.Value.Items.Cast<ProductDetailResponse>().ToList();
            Assert.Equal("P2", resultList[0].ProductName);
            Assert.Equal("P3", resultList[1].ProductName);
            Assert.Equal("P1", resultList[2].ProductName);
        }
    }
}
