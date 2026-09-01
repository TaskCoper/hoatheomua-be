using AutoMapper;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
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
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var dbContext = new AppDbContext(options);

            if (products != null && products.Any())
            {
                dbContext.Products.AddRange(products);
                dbContext.SaveChanges();
            }

            return dbContext;
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

        [Fact]
        public async Task GetLocalProductsV2_FilterByProductType_Components_Success()
        {
            // Arrange
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Material 1", ProductLabel = "Vật liệu", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Material 2", ProductLabel = "PHỤ LIỆU, VẬT LIỆU", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Product 1", ProductLabel = "Hoa hồng", ProductType = "Sản phẩm", IsDeleted = false }
            };
            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);
            var request = new ProductRequest.LocalProductQueryRequest { productType = ProductType.Material };

            // Act
            var result = await service.GetLocalProductsV2(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Value.TotalCount);
            var items = result.Value.Items.Cast<ProductDetailResponse>().ToList();
            Assert.Contains(items, x => x.ProductName == "Material 1");
            Assert.Contains(items, x => x.ProductName == "Material 2");
        }

        [Fact]
        public async Task GetLocalProductsV2_FilterByProductType_Combo_BothCases_Success()
        {
            // Arrange
            var products = new List<ProductEntity>
            {
                // TH1: Độc lập (không có con) + ProductType = Combo
                new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "COMBO_IND", ProductName = "Combo Doc Lap", ProductType = "Combo", IsDeleted = false },

                // TH2: Cha (ProductType = "Sản phẩm"), có con và TẤT CẢ con là Combo
                new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "PARENT_ALL_COMBO", ProductName = "Cha Co Con Toan Combo", ProductType = "Sản phẩm", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "CHILD_1", ParentProductId = "PARENT_ALL_COMBO", ProductName = "Con Combo 1", ProductType = "Combo", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "CHILD_2", ParentProductId = "PARENT_ALL_COMBO", ProductName = "Con Combo 2", ProductType = "Combo", IsDeleted = false },

                // Case loại trừ: Cha có con nhưng có con KHÔNG PHẢI Combo
                new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "PARENT_MIXED", ProductName = "Cha Co Con Hon Hop", ProductType = "Sản phẩm", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "CHILD_3", ParentProductId = "PARENT_MIXED", ProductName = "Con Combo 3", ProductType = "Combo", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "CHILD_4", ParentProductId = "PARENT_MIXED", ProductName = "Con San Pham 4", ProductType = "Sản phẩm", IsDeleted = false },

                // Sản phẩm thường độc lập
                new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "NORMAL_PROD", ProductName = "San Pham Thuong", ProductType = "Sản phẩm", IsDeleted = false }
            };
            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);
            var request = new ProductRequest.LocalProductQueryRequest { productType = ProductType.Combo };

            // Act
            var result = await service.GetLocalProductsV2(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Value.TotalCount); // "Combo Doc Lap" và "Cha Co Con Toan Combo"
            var items = result.Value.Items.Cast<ProductDetailResponse>().ToList();
            Assert.Contains(items, x => x.ProductName == "Combo Doc Lap");
            Assert.Contains(items, x => x.ProductName == "Cha Co Con Toan Combo");
        }

        [Fact]
        public async Task GetLocalProductsV2_FilterByProductType_Product_Success()
        {
            // Arrange
            var products = new List<ProductEntity>
            {
                // TH Sản phẩm thường độc lập -> ĐƯỢC CHỌN
                new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "PROD_IND", ProductName = "San Pham Doc Lap", ProductType = "Sản phẩm", IsDeleted = false },

                // TH cha có con hỗn hợp -> ĐƯỢC CHỌN (không phải full combo)
                new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "PARENT_MIXED", ProductName = "Cha Co Con Hon Hop", ProductType = "Sản phẩm", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "CHILD_1", ParentProductId = "PARENT_MIXED", ProductName = "Con Combo 1", ProductType = "Combo", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "CHILD_2", ParentProductId = "PARENT_MIXED", ProductName = "Con San Pham 2", ProductType = "Sản phẩm", IsDeleted = false },

                // TH Cha có con và TẤT CẢ con là Combo -> BỊ LOẠI TRỪ (thuộc nhóm Combo)
                new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "PARENT_ALL_COMBO", ProductName = "Cha Toan Con Combo", ProductType = "Sản phẩm", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "CHILD_3", ParentProductId = "PARENT_ALL_COMBO", ProductName = "Con Combo 3", ProductType = "Combo", IsDeleted = false },

                // Combo độc lập -> BỊ LOẠI TRỪ
                new ProductEntity { Id = Guid.NewGuid(), NhanhProductId = "COMBO_IND", ProductName = "Combo Doc Lap", ProductType = "Combo", IsDeleted = false }
            };
            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);
            var request = new ProductRequest.LocalProductQueryRequest { productType = ProductType.Product };

            // Act
            var result = await service.GetLocalProductsV2(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Value.TotalCount);
            var items = result.Value.Items.Cast<ProductDetailResponse>().ToList();
            Assert.Contains(items, x => x.ProductName == "San Pham Doc Lap");
            Assert.Contains(items, x => x.ProductName == "Cha Co Con Hon Hop");
            Assert.DoesNotContain(items, x => x.ProductName == "Cha Toan Con Combo");
        }

        [Fact]
        public async Task GetLocalProductsV2_FilterByProductType_NotProvided_ReturnsAllProducts()
        {
            // Arrange
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "P1", ProductType = "Combo", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "P2", ProductType = "Sản phẩm", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "P3", ProductLabel = "Thành phần", IsDeleted = false }
            };
            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);
            var request = new ProductRequest.LocalProductQueryRequest { productType = null };

            // Act
            var result = await service.GetLocalProductsV2(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Value.TotalCount);
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
