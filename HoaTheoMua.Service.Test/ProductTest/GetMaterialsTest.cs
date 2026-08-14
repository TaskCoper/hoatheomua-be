using AutoMapper;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using Xunit;
using ProductService = HoaTheoMua.Service.Product.Service;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using ConfigEntity = HoaTheoMua.Repository.Entity.Config;

namespace HoaTheoMua.Service.Test.ProductTest
{
    public class GetMaterialsTest
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContext;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IMapper> _mockMapper;
        private readonly HttpClient _httpClient;

        public GetMaterialsTest()
        {
            _mockHttpContext = new Mock<IHttpContextAccessor>();
            _mockConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("dummy_value");
            _mockConfig.Setup(x => x.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);

            _mockMapper = new Mock<IMapper>();
            _httpClient = new HttpClient();
        }

        private AppDbContext CreateMockDbContext(List<ProductEntity> products)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            var mockDbContext = new Mock<AppDbContext>(options);

            var mockProductDbSet = products.BuildMockDbSet();
            mockDbContext.Setup(x => x.Products).Returns(mockProductDbSet.Object);

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

        [Fact]
        public async Task GetMaterialsAsync_FiltersByProductLabel_ReturnsOnlyMaterials()
        {
            // Arrange
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Material 1", ProductLabel = "Thành phần", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Material 2", ProductLabel = "thành phần", IsDeleted = false }, // case-insensitive
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Combo 1", ProductLabel = "Combo", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Deleted Material", ProductLabel = "Thành phần", IsDeleted = true }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10 };

            // Act
            var result = await service.GetMaterialsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Value.PageIndex); // PaginationValue uses pageIndex
            Assert.Equal(2, result.Value.Items.Count);
        }

        [Fact]
        public async Task GetMaterialsAsync_Pagination_ReturnsCorrectPageSize()
        {
            // Arrange
            var products = new List<ProductEntity>();
            for (int i = 0; i < 15; i++)
            {
                products.Add(new ProductEntity { Id = Guid.NewGuid(), ProductName = $"Material {i}", ProductLabel = "Thành phần", IsDeleted = false });
            }

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 2, PageSize = 5 };

            // Act
            var result = await service.GetMaterialsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Value.PageIndex);
            
            var items = result.Value.Items.Cast<HoaTheoMua.Service.Product.Response.MaterialResponse>().ToList();
            Assert.Equal(5, items.Count);
        }
        
        [Fact]
        public async Task GetMaterialsAsync_NoMaterialsInDb_ReturnsEmptyList()
        {
            // Arrange
            // Tạo ra danh sách sản phẩm nhưng KHÔNG CÓ cái nào là "Thành phần"
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Combo 1", ProductLabel = "Combo", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Product 1", ProductLabel = "Sản phẩm thông thường", IsDeleted = false }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10 };

            // Act
            var result = await service.GetMaterialsAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Value.TotalCount);
            
            var items = result.Value.Items.Cast<HoaTheoMua.Service.Product.Response.MaterialResponse>().ToList();
            Assert.Empty(items); // Đảm bảo mảng trả ra là mảng rỗng
        }
    }
}