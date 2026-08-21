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

        /// Test: Lọc đúng nguyên liệu theo cả 2 nhãn "Thành phần" và "Phụ liệu" (không phân biệt hoa/thường),
        /// đồng thời bỏ qua các sản phẩm đã xóa (IsDeleted = true) và các loại khác (Combo, Sản phẩm thường).
        [Fact]
        public async Task GetMaterialsAsync_FiltersByProductLabel_ReturnsOnlyMaterials()
        {
            // Arrange: Cả "Thành phần" và "Phụ liệu" (không phân biệt hoa thường), loại bỏ IsDeleted = true và các loại khác
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Hoa Hồng", ProductLabel = "Thành phần", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Hoa Cúc", ProductLabel = "thành phần", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Giấy Gói Hàn Quốc", ProductLabel = "Phụ liệu", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Ruy Băng Satin", ProductLabel = "phụ liệu", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Combo 1", ProductLabel = "Combo", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Sản Phẩm Đơn", ProductLabel = "Sản phẩm", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Deleted Material", ProductLabel = "Thành phần", IsDeleted = true },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Deleted Accessory", ProductLabel = "Phụ liệu", IsDeleted = true }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10 };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(1, result.Value.PageIndex);
            Assert.Equal(4, result.Value.TotalCount);
            Assert.Equal(4, result.Value.Items.Count);
        }

        /// Test: Trường hợp DB chỉ có các sản phẩm nhãn "Phụ liệu",
        /// kiểm tra hàm lọc và trả về chính xác danh sách phụ liệu đang hoạt động.
        [Fact]
        public async Task GetMaterialsAsync_OnlyPhuLieuInDb_ReturnsOnlyPhuLieuMaterials()
        {
            // Arrange: DB chỉ có các sản phẩm nhãn "Phụ liệu"
            var phuLieu1Id = Guid.NewGuid();
            var phuLieu2Id = Guid.NewGuid();
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = phuLieu1Id, ProductName = "Giấy gói báo", ProductLabel = "Phụ liệu", IsDeleted = false },
                new ProductEntity { Id = phuLieu2Id, ProductName = "Xốp cắm hoa Oasis", ProductLabel = "phụ liệu", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Giỏ hoa đã xóa", ProductLabel = "Phụ liệu", IsDeleted = true },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Bó hoa hoàn chỉnh", ProductLabel = "Sản phẩm", IsDeleted = false }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10 };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(2, result.Value.TotalCount);

            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Equal(2, items.Count);
            Assert.Contains(items, x => x.MaterialId == phuLieu1Id.ToString() && x.MaterialName == "Giấy gói báo");
            Assert.Contains(items, x => x.MaterialId == phuLieu2Id.ToString() && x.MaterialName == "Xốp cắm hoa Oasis");
        }

        /// Test: Trường hợp DB chỉ có các sản phẩm nhãn "Thành phần",
        /// đảm bảo logic lọc hoa/thành phần nguyên bản vẫn hoạt động chính xác.
        [Fact]
        public async Task GetMaterialsAsync_OnlyThanhPhanInDb_ReturnsOnlyThanhPhanMaterials()
        {
            // Arrange: DB chỉ có các sản phẩm nhãn "Thành phần"
            var thanhPhan1Id = Guid.NewGuid();
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = thanhPhan1Id, ProductName = "Hoa Baby Trắng", ProductLabel = "Thành phần", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Lẵng hoa chúc mừng", ProductLabel = "Combo", IsDeleted = false }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10 };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(1, result.Value.TotalCount);

            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Single(items);
            Assert.Equal(thanhPhan1Id.ToString(), items[0].MaterialId);
            Assert.Equal("Hoa Baby Trắng", items[0].MaterialName);
        }

        /// Test: Đảm bảo các sản phẩm có nhãn null hoặc mang các nhãn khác (Combo, Sản phẩm, Khác)
        /// sẽ bị loại trừ hoàn toàn, không trả về trong danh sách nguyên liệu.
        [Fact]
        public async Task GetMaterialsAsync_ProductLabelNullOrOther_Excluded()
        {
            // Arrange: Các sản phẩm có ProductLabel = null hoặc các nhãn khác không phải Thành phần / Phụ liệu
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Null Label Product", ProductLabel = null, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Combo Product", ProductLabel = "Combo", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Finished Product", ProductLabel = "Sản phẩm", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Other Label", ProductLabel = "Khác", IsDeleted = false }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10 };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(0, result.Value.TotalCount);
            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Empty(items);
        }

        /// Test: Kiểm tra phân trang (PageIndex, PageSize, Skip/Take) hoạt động chính xác
        /// khi có nhiều nguyên liệu (kết hợp cả Thành phần và Phụ liệu).
        [Fact]
        public async Task GetMaterialsAsync_Pagination_ReturnsCorrectPageSize()
        {
            // Arrange: 15 materials (mix giữa Thành phần và Phụ liệu)
            var products = new List<ProductEntity>();
            for (int i = 0; i < 15; i++)
            {
                var label = i % 2 == 0 ? "Thành phần" : "Phụ liệu";
                products.Add(new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    ProductName = $"Material {i}",
                    ProductLabel = label,
                    IsDeleted = false
                });
            }

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 2, PageSize = 5 };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(2, result.Value.PageIndex);
            Assert.Equal(5, result.Value.PageSize);
            Assert.Equal(15, result.Value.TotalCount);
            
            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Equal(5, items.Count);
        }

        /// Test: Kiểm tra tự động điều chỉnh tham số phân trang không hợp lệ:
        /// PageIndex <= 0 về 1, PageSize <= 0 về 10, PageSize > 100 giới hạn tối đa 100.
        [Fact]
        public async Task GetMaterialsAsync_InvalidPagination_AdjustsToDefaults()
        {
            // Arrange: PageIndex <= 0 -> 1, PageSize <= 0 -> 10, PageSize > 100 -> 100
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Phụ liệu 1", ProductLabel = "Phụ liệu", IsDeleted = false }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            // Test 1: PageIndex <= 0 and PageSize <= 0
            var request1 = new Request.GetMaterialsRequest { PageIndex = -1, PageSize = 0 };
            var result1 = await service.GetMaterialsAsync(request1);
            Assert.Equal(1, result1.Value.PageIndex);
            Assert.Equal(10, result1.Value.PageSize);

            // Test 2: PageSize > 100 caps at 100
            var request2 = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 500 };
            var result2 = await service.GetMaterialsAsync(request2);
            Assert.Equal(1, result2.Value.PageIndex);
            Assert.Equal(100, result2.Value.PageSize);
        }

        /// Test: Kiểm tra việc ánh xạ (mapping) dữ liệu từ Product Entity sang MaterialResponse DTO
        /// đầy đủ và chính xác tất cả các trường (Id, Mã, Tên, Đơn vị, Tồn kho, Ảnh đại diện).
        [Fact]
        public async Task GetMaterialsAsync_FieldMapping_ReturnsExpectedFields()
        {
            var materialId = Guid.NewGuid();
            var products = new List<ProductEntity>
            {
                new ProductEntity
                {
                    Id = materialId,
                    ProductCode = "PL001",
                    ProductName = "Ruy băng đỏ 2cm",
                    Unit = "Cuộn",
                    AvailableForSale = 45.5m,
                    CoverImage = "https://cdn.example.com/ruy-bang-do.png",
                    ProductLabel = "Phụ liệu",
                    IsDeleted = false
                }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10 };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Single(items);

            var item = items[0];
            Assert.Equal(materialId.ToString(), item.MaterialId);
            Assert.Equal("PL001", item.MaterialCode);
            Assert.Equal("Ruy băng đỏ 2cm", item.MaterialName);
            Assert.Equal("Cuộn", item.Unit);
            Assert.Equal(45.5m, item.AvailableForSale);
            Assert.Equal("https://cdn.example.com/ruy-bang-do.png", item.CoverImage);
        }

        /// Test: Trường hợp DB hoàn toàn không có nguyên liệu nào (không có Thành phần lẫn Phụ liệu),
        /// API trả về mảng rỗng [] và TotalCount = 0 mà không bị lỗi.
        [Fact]
        public async Task GetMaterialsAsync_NoMaterialsInDb_ReturnsEmptyList()
        {
            // Arrange: DB không có sản phẩm "Thành phần" hay "Phụ liệu"
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Combo 1", ProductLabel = "Combo", IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Product 1", ProductLabel = "Sản phẩm thông thường", IsDeleted = false }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10 };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(0, result.Value.TotalCount);
            
            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Empty(items);
        }
    }
}