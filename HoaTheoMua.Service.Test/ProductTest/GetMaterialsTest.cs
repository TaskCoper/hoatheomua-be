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

        /// Test: Khi không truyền ProductLabel, API mặc định trả về tất cả nguyên vật liệu (Thành phần và Phụ liệu) chưa xóa.
        [Fact]
        public async Task GetMaterialsAsync_NoProductLabelProvided_ReturnsAllNonDeletedMaterials()
        {
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Hoa Hồng", ProductLabel = new List<string> { "Thành phần" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Hoa Cúc", ProductLabel = new List<string> { "thành phần" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Giấy Gói Hàn Quốc", ProductLabel = new List<string> { "Phụ liệu" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Ruy Băng Satin", ProductLabel = new List<string> { "phụ liệu" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Combo 1", ProductLabel = new List<string> { "Combo" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Sản Phẩm Đơn", ProductLabel = new List<string> { "Sản phẩm" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Deleted Material", ProductLabel = new List<string> { "Thành phần" }, IsDeleted = true },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Deleted Accessory", ProductLabel = new List<string> { "Phụ liệu" }, IsDeleted = true }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10, ProductLabel = null };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(1, result.Value.PageIndex);
            Assert.Equal(4, result.Value.TotalCount);
            Assert.Equal(4, result.Value.Items.Count);

            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.DoesNotContain(items, item => item.ProductLabel?.Contains("Combo") == true);
            Assert.DoesNotContain(items, item => item.ProductLabel?.Contains("Sản phẩm") == true);
            Assert.DoesNotContain(items, item => item.MaterialName == "Deleted Material");
            Assert.DoesNotContain(items, item => item.MaterialName == "Deleted Accessory");
        }

        /// Test: Khi FE truyền ProductLabel = "Thành phần" (không phân biệt hoa/thường),
        /// API chỉ lọc và trả về các sản phẩm mang nhãn "Thành phần".
        [Fact]
        public async Task GetMaterialsAsync_ProductLabelThanhPhanProvided_ReturnsOnlyThanhPhan()
        {
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Hoa Hồng", ProductLabel = new List<string> { "Thành phần" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Hoa Cúc", ProductLabel = new List<string> { "thành phần" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Giấy Gói", ProductLabel = new List<string> { "Phụ liệu" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Ruy Băng", ProductLabel = new List<string> { "phụ liệu" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Hoa Đã Xóa", ProductLabel = new List<string> { "Thành phần" }, IsDeleted = true }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10, ProductLabel = "Thành phần" };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(2, result.Value.TotalCount);

            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Equal(2, items.Count);
            Assert.All(items, item => Assert.True(item.ProductLabel?.Any(label =>
                label.Equals("thành phần", StringComparison.OrdinalIgnoreCase)) == true));
        }

        /// Test: Khi FE truyền ProductLabel = "Phụ liệu" (không phân biệt hoa/thường),
        /// API chỉ lọc và trả về các sản phẩm mang nhãn "Phụ liệu".
        [Fact]
        public async Task GetMaterialsAsync_ProductLabelPhuLieuProvided_ReturnsOnlyPhuLieu()
        {
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Hoa Hồng", ProductLabel = new List<string> { "Thành phần" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Giấy Gói Hàn Quốc", ProductLabel = new List<string> { "Phụ liệu" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Ruy Băng Satin", ProductLabel = new List<string> { "phụ liệu" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Phụ Liệu Đã Xóa", ProductLabel = new List<string> { "Phụ liệu" }, IsDeleted = true }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10, ProductLabel = "phụ liệu" };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(2, result.Value.TotalCount);

            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Equal(2, items.Count);
            Assert.All(items, item => Assert.True(item.ProductLabel?.Any(label =>
                label.Equals("phụ liệu", StringComparison.OrdinalIgnoreCase)) == true));
        }

        /// Test: Khi FE truyền ProductLabel không tồn tại trong DB,
        /// API trả về danh sách rỗng và TotalCount = 0.
        [Fact]
        public async Task GetMaterialsAsync_ProductLabelNonExistentProvided_ReturnsEmpty()
        {
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Hoa Hồng", ProductLabel = new List<string> { "Thành phần" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Giấy Gói", ProductLabel = new List<string> { "Phụ liệu" }, IsDeleted = false }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10, ProductLabel = "Nhãn không tồn tại" };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(0, result.Value.TotalCount);
            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Empty(items);
        }

        /// Test: Tìm kiếm theo tên sản phẩm (Search theo ProductName, không phân biệt hoa/thường).
        [Fact]
        public async Task GetMaterialsAsync_SearchByProductName_ReturnsMatchingMaterials()
        {
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductCode = "HH01", ProductName = "Hoa Hồng Đỏ", ProductLabel = new List<string> { "Thành phần" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductCode = "HH02", ProductName = "Hoa Hồng Trắng", ProductLabel = new List<string> { "Thành phần" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductCode = "HC01", ProductName = "Hoa Cúc Mẫu Đơn", ProductLabel = new List<string> { "Thành phần" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductCode = "PL01", ProductName = "Giấy Gói", ProductLabel = new List<string> { "Phụ liệu" }, IsDeleted = false }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10, Search = "hồng" };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(2, result.Value.TotalCount);

            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Equal(2, items.Count);
            Assert.All(items, item => Assert.Contains("Hồng", item.MaterialName, StringComparison.OrdinalIgnoreCase));
        }

        /// Test: Tìm kiếm theo mã thành phần/phụ liệu (Search theo ProductCode, không phân biệt hoa/thường).
        [Fact]
        public async Task GetMaterialsAsync_SearchByProductCode_ReturnsMatchingMaterials()
        {
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductCode = "MAT_001", ProductName = "Hoa Hồng Đỏ", ProductLabel = new List<string> { "Thành phần" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductCode = "MAT_002", ProductName = "Hoa Cúc", ProductLabel = new List<string> { "Thành phần" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductCode = "ACC_001", ProductName = "Giấy Gói", ProductLabel = new List<string> { "Phụ liệu" }, IsDeleted = false }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10, Search = "acc_001" };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(1, result.Value.TotalCount);

            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Single(items);
            Assert.Equal("ACC_001", items[0].MaterialCode);
        }

        /// Test: Kết hợp cả ProductLabel và Search theo tên/mã thành phần.
        [Fact]
        public async Task GetMaterialsAsync_SearchCombinedWithProductLabel_ReturnsCorrectResults()
        {
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductCode = "HH01", ProductName = "Hoa Hồng Đỏ", ProductLabel = new List<string> { "Thành phần" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductCode = "PL01", ProductName = "Nơ Ruy Băng Hồng", ProductLabel = new List<string> { "Phụ liệu" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductCode = "PL02", ProductName = "Giấy Gói Xanh", ProductLabel = new List<string> { "Phụ liệu" }, IsDeleted = false }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            // Tìm kiếm chữ "Hồng" nhưng chỉ lấy loại "Phụ liệu"
            var request = new Request.GetMaterialsRequest
            {
                PageIndex = 1,
                PageSize = 10,
                ProductLabel = "Phụ liệu",
                Search = "hồng"
            };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(1, result.Value.TotalCount);

            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Single(items);
            Assert.Equal("Nơ Ruy Băng Hồng", items[0].MaterialName);
            Assert.Contains("Phụ liệu", items[0].ProductLabel!);
        }

        /// Test: Kiểm tra phân trang (PageIndex, PageSize, Skip/Take) hoạt động chính xác
        /// khi có nhiều nguyên liệu (kết hợp cả Thành phần và Phụ liệu).
        [Fact]
        public async Task GetMaterialsAsync_Pagination_ReturnsCorrectPageSize()
        {
            var products = new List<ProductEntity>();
            for (int i = 0; i < 15; i++)
            {
                var label = i % 2 == 0 ? "Thành phần" : "Phụ liệu";
                products.Add(new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    ProductName = $"Material {i}",
                    ProductLabel = new List<string> { label },
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
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Phụ liệu 1", ProductLabel = new List<string> { "Phụ liệu" }, IsDeleted = false }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            // Case 1: PageIndex <= 0 và PageSize <= 0
            var request1 = new Request.GetMaterialsRequest { PageIndex = -1, PageSize = 0 };
            var result1 = await service.GetMaterialsAsync(request1);
            Assert.Equal(1, result1.Value.PageIndex);
            Assert.Equal(10, result1.Value.PageSize);

            // Case 2: PageSize > 100 giới hạn về 100
            var request2 = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 500 };
            var result2 = await service.GetMaterialsAsync(request2);
            Assert.Equal(1, result2.Value.PageIndex);
            Assert.Equal(100, result2.Value.PageSize);
        }

        /// Test: Kiểm tra việc ánh xạ (mapping) dữ liệu từ Product Entity sang MaterialResponse DTO
        /// đầy đủ và chính xác tất cả các trường (Id, Mã, Tên, Đơn vị, Tồn kho, Ảnh đại diện, ProductLabel).
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
                    ProductLabel = new List<string> { "Phụ liệu" },
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
            Assert.Contains("Phụ liệu", item.ProductLabel!);
        }

        /// Test: Khi lọc Thành phần nhưng DB chỉ có Combo và Sản phẩm thường,
        /// API trả về mảng rỗng và TotalCount = 0.
        [Fact]
        public async Task GetMaterialsAsync_NoMaterialsInDb_ReturnsEmptyList()
        {
            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Combo 1", ProductLabel = new List<string> { "Combo" }, IsDeleted = false },
                new ProductEntity { Id = Guid.NewGuid(), ProductName = "Product 1", ProductLabel = new List<string> { "Sản phẩm thông thường" }, IsDeleted = false }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest
            {
                PageIndex = 1,
                PageSize = 10,
                ProductLabel = "Thành phần"
            };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(0, result.Value.TotalCount);
            
            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Empty(items);
        }

        /// Test: Khi truyền ExcludeIds, API loại trừ các nguyên vật liệu có ID nằm trong danh sách ExcludeIds.
        [Fact]
        public async Task GetMaterialsAsync_WithExcludeIds_ExcludesSpecifiedMaterials()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            var id4 = Guid.NewGuid();

            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = id1, ProductName = "Hoa Hồng Đỏ", ProductLabel = new List<string> { "Thành phần" }, IsDeleted = false },
                new ProductEntity { Id = id2, ProductName = "Hoa Cúc Trắng", ProductLabel = new List<string> { "Thành phần" }, IsDeleted = false },
                new ProductEntity { Id = id3, ProductName = "Giấy Gói Hàn Quốc", ProductLabel = new List<string> { "Phụ liệu" }, IsDeleted = false },
                new ProductEntity { Id = id4, ProductName = "Ruy Băng Satin", ProductLabel = new List<string> { "Phụ liệu" }, IsDeleted = false }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            // Loại trừ id1 và id3
            var request = new Request.GetMaterialsRequest
            {
                PageIndex = 1,
                PageSize = 10,
                ExcludeIds = new List<Guid> { id1, id3 }
            };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(2, result.Value.TotalCount);

            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Equal(2, items.Count);
            Assert.DoesNotContain(items, item => item.MaterialId == id1.ToString());
            Assert.DoesNotContain(items, item => item.MaterialId == id3.ToString());
            Assert.Contains(items, item => item.MaterialId == id2.ToString());
            Assert.Contains(items, item => item.MaterialId == id4.ToString());
        }

        /// Test: Khi truyền ExcludeIds rỗng hoặc null, API trả về đầy đủ các nguyên vật liệu.
        [Fact]
        public async Task GetMaterialsAsync_WithExcludeIdsEmpty_ReturnsAllMaterials()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var products = new List<ProductEntity>
            {
                new ProductEntity { Id = id1, ProductName = "Hoa Hồng", ProductLabel = new List<string> { "Thành phần" }, IsDeleted = false },
                new ProductEntity { Id = id2, ProductName = "Giấy Gói", ProductLabel = new List<string> { "Phụ liệu" }, IsDeleted = false }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest
            {
                PageIndex = 1,
                PageSize = 10,
                ExcludeIds = new List<Guid>()
            };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(2, result.Value.TotalCount);

            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public async Task GetMaterialsAsync_WithWhitespaceAndMixedCaseLabels_ReturnsExpectedMaterials()
        {
            var targetId = Guid.Parse("4c47cda3-f9be-497a-987c-13be8b1827c0");
            var excludedId = Guid.Parse("170efdad-c33a-4bdb-88a8-c6618febc5d1");

            var products = new List<ProductEntity>
            {
                new ProductEntity 
                { 
                    Id = targetId, 
                    ProductCode = "TGK_L", 
                    ProductName = "Trà Giảm Cân TGK_L", 
                    ProductLabel = new List<string> { "  Thành phần  " }, 
                    IsDeleted = false 
                },
                new ProductEntity 
                { 
                    Id = excludedId, 
                    ProductCode = "TGK_L_EX", 
                    ProductName = "Sản phẩm loại trừ", 
                    ProductLabel = new List<string> { "THÀNH PHẦN" }, 
                    IsDeleted = false 
                },
                new ProductEntity 
                { 
                    Id = Guid.NewGuid(), 
                    ProductCode = "PL_01", 
                    ProductName = "Phụ liệu 1", 
                    ProductLabel = new List<string> { "PHỤ LIỆU" }, 
                    IsDeleted = false 
                },
                new ProductEntity 
                { 
                    Id = Guid.NewGuid(), 
                    ProductCode = "PL_02", 
                    ProductName = "Phụ liệu 2", 
                    ProductLabel = new List<string> { "Thành Phần" }, 
                    IsDeleted = false 
                }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            var request = new Request.GetMaterialsRequest
            {
                Search = "TGK_L",
                ExcludeIds = new List<Guid> { excludedId },
                PageIndex = 1,
                PageSize = 10
            };

            var result = await service.GetMaterialsAsync(request);

            Assert.NotNull(result);
            Assert.Equal(1, result.Value.TotalCount);

            var items = result.Value.Items.Cast<Response.MaterialResponse>().ToList();
            Assert.Single(items);
            Assert.Equal(targetId.ToString(), items[0].MaterialId);
            Assert.Equal("TGK_L", items[0].MaterialCode);
        }

        [Fact]
        public async Task GetMaterialsAsync_WithNfdUnicodeLabelsInDb_SuccessfullyMatches()
        {
            // "Thành phần" in NFD form
            var nfdThanhPhan = "Thành phần".Normalize(System.Text.NormalizationForm.FormD);
            var nfdPhuLieu = "Phụ liệu".Normalize(System.Text.NormalizationForm.FormD);

            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            var products = new List<ProductEntity>
            {
                new ProductEntity 
                { 
                    Id = id1, 
                    ProductCode = "MAT_NFD_01", 
                    ProductName = "Nguyên liệu NFD 1", 
                    ProductLabel = new List<string> { nfdThanhPhan }, 
                    IsDeleted = false 
                },
                new ProductEntity 
                { 
                    Id = id2, 
                    ProductCode = "MAT_NFD_02", 
                    ProductName = "Nguyên liệu NFD 2", 
                    ProductLabel = new List<string> { nfdPhuLieu }, 
                    IsDeleted = false 
                }
            };

            var dbContext = CreateMockDbContext(products);
            var service = CreateService(dbContext);

            // 1. Without ProductLabel filter: should return both
            var req1 = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10 };
            var res1 = await service.GetMaterialsAsync(req1);
            Assert.Equal(2, res1.Value.TotalCount);

            // 2. With NFC ProductLabel filter "Thành phần" (default typed in NFC)
            var req2 = new Request.GetMaterialsRequest { PageIndex = 1, PageSize = 10, ProductLabel = "Thành phần" };
            var res2 = await service.GetMaterialsAsync(req2);
            Assert.Equal(1, res2.Value.TotalCount);
            var item = Assert.Single(res2.Value.Items.Cast<Response.MaterialResponse>());
            Assert.Equal(id1.ToString(), item.MaterialId);
        }
    }
}
