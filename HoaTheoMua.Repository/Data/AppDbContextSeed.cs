using System.Security.Cryptography;
using System.Text;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Repository.Data;

public static class AppDbContextSeed
{
    // Hàm băm mật khẩu giả lập (Phù hợp với cấu trúc HashPassword của thực thể User)
    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // ĐIỂM KÍCH HOẠT CHÍNH (ENTRY POINT)
    // ═══════════════════════════════════════════════════════════════════════
    public static async Task SeedAsync(AppDbContext context)
    {
        // 1. Seed bảng độc lập cao nhất: Users và CompanyAddresses
        var seededUsers = await SeedUsersAsync(context);
        await SeedCompanyAddressesAsync(context);

        // Nếu không có user nào (do db đã có sẵn dữ liệu và bị skip), 
        // lấy một user bất kỳ trong DB để làm tác giả tạo danh mục tin tức.
        var author = seededUsers ?? await context.Users.FirstAsync();

        // 2. Seed danh mục tin tức (Cần UserID)
        var seededCategories = await SeedCategoryNewsAsync(context, author.Id);

        // 3. Seed bài viết tin tức
        var seededNews = await SeedNewsAsync(context);

        // 4. Seed bảng trung gian liên kết Nhiều - Nhiều giữa Tin tức và Danh mục
        await SeedCategoryNewsDetailsAsync(context, seededCategories, seededNews);

        // 5. Seed nhật ký hệ thống
        await SeedAuditLogsAsync(context, author.Id);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // CHI TIẾT CÁC HÀM SEED THÀNH PHẦN
    // ═══════════════════════════════════════════════════════════════════════

    private static async Task<User?> SeedUsersAsync(AppDbContext context)
    {
        // Kiểm tra Idempotent: Nếu bảng đã có dữ liệu, thoát ngay lập tức
        if (await context.Users.AnyAsync()) return null;

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@hoatheomua.vn",
            Name = "Quản trị viên Hệ thống", // <--- BỔ SUNG TRƯỜNG NÀY (Không được để null)
            PhoneNumber = "0961002445",
            HashPassword = HashPassword("Admin@123"),
            Role = Role.User, // Đã cập nhật theo dạng Enum Role của bạn từ lượt trước
            IsDeleted = false
        };

        var staff = new User
        {
            Id = Guid.NewGuid(),
            Email = "editor@hoatheomua.vn",
            Name = "Biên tập viên Tin tức", // <--- BỔ SUNG TRƯỜNG NÀY (Không được để null)
            PhoneNumber = "0912345678",
            HashPassword = HashPassword("Editor@123"),
            Role = Role.User, 
            IsDeleted = false
        };

        await context.Users.AddRangeAsync(admin, staff);
        await context.SaveChangesAsync(); // Lỗi ném ra tại dòng này (dòng 75 của bạn) sẽ biến mất

        return admin;
    }

    private static async Task SeedCompanyAddressesAsync(AppDbContext context)
    {
        if (await context.CompanyAddresses.AnyAsync()) return;

        var addresses = new List<CompanyAddress>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AddressLine = "Lô E2a-7, Đường D1, Khu Công nghệ cao",
                Ward = "Phường Long Thạnh Mỹ",
                District = "Thành phố Thủ Đức",
                Province = "Thành phố Hồ Chí Minh",
                City = "Thành phố Hồ Chí Minh",
                Country = "Việt Nam"
            },
            new()
            {
                Id = Guid.NewGuid(),
                AddressLine = "Số 1 Đại lộ Thăng Long",
                Ward = "Phường Mễ Trì",
                District = "Quận Nam Từ Liêm",
                Province = "Thành phố Hà Nội",
                City = "Thành phố Hà Nội",
                Country = "Việt Nam"
            }
        };

        await context.CompanyAddresses.AddRangeAsync(addresses);
        await context.SaveChangesAsync();
    }

    private static async Task<List<CategoryNews>?> SeedCategoryNewsAsync(AppDbContext context, Guid userId)
    {
        if (await context.CategoryNewsList.AnyAsync()) return null;

        // Xử lý dữ liệu phân cấp: Khởi tạo danh mục cha trước
        var parentCategories = new List<CategoryNews>
        {
            new() { Id = Guid.NewGuid(), Title = "Tin tức Khuyến mãi", UserID = userId, ParentCategoryNewsID = null },
            new() { Id = Guid.NewGuid(), Title = "Xu hướng Hoa theo mùa", UserID = userId, ParentCategoryNewsID = null },
            new() { Id = Guid.NewGuid(), Title = "Hướng dẫn chăm sóc hoa", UserID = userId, ParentCategoryNewsID = null }
        };
        await context.CategoryNewsList.AddRangeAsync(parentCategories);
        await context.SaveChangesAsync(); // Lưu để đồng bộ ID gốc lên bộ nhớ tạm EF

        // Khởi tạo danh mục con liên kết trực tiếp tới ID danh mục cha
        var childCategories = new List<CategoryNews>
        {
            new() { Id = Guid.NewGuid(), Title = "Sự kiện Valentine", UserID = userId, ParentCategoryNewsID = parentCategories[0].Id },
            new() { Id = Guid.NewGuid(), Title = "Khuyến mãi Mùa Cưới", UserID = userId, ParentCategoryNewsID = parentCategories[0].Id },
            new() { Id = Guid.NewGuid(), Title = "Hoa tươi mùa Xuân", UserID = userId, ParentCategoryNewsID = parentCategories[1].Id },
            new() { Id = Guid.NewGuid(), Title = "Hoa tươi mùa Hạ", UserID = userId, ParentCategoryNewsID = parentCategories[1].Id },
            new() { Id = Guid.NewGuid(), Title = "Chăm sóc Hoa Hồng", UserID = userId, ParentCategoryNewsID = parentCategories[2].Id }
        };
        await context.CategoryNewsList.AddRangeAsync(childCategories);
        await context.SaveChangesAsync();

        // Gộp cả cha và con để trả về cho phân hệ kết nối Nhiều - Nhiều
        return parentCategories.Concat(childCategories).ToList();
    }

    private static async Task<List<News>?> SeedNewsAsync(AppDbContext context)
{
    // 1. Kiểm tra nếu đã có dữ liệu thì tự động bỏ qua
    if (await context.NewsList.AnyAsync()) return null;

    // 2. Khởi tạo danh sách tin tức đầy đủ các thuộc tính bắt buộc (Không để Description bị null)
    var newsList = new List<News>
    {
        new() 
        { 
            Id = Guid.NewGuid(), 
            Title = "Chương trình ưu đãi giảm giá 20% cho ngày lễ tình nhân Valentine", 
            Description = "Chi tiết chương trình khuyến mãi đặc biệt áp dụng cho tất cả các đơn hàng hoa tươi đặt trước ngày 14/02.", // <--- BỔ SUNG TRƯỜNG NÀY
            ViewCount = 150, 
            Status = "Published" 
        },
        new() 
        { 
            Id = Guid.NewGuid(), 
            Title = "Top 10 loại hoa rực rỡ và bền bỉ nhất thích hợp cho tiết trời mùa hạ", 
            Description = "Tổng hợp danh sách các loại hoa chịu nhiệt tốt, thích hợp trồng ban công hoặc trang trí phòng khách vào mùa hè oi bức.", // <--- BỔ SUNG TRƯỜNG NÀY
            ViewCount = 85, 
            Status = "Published" 
        },
        new() 
        { 
            Id = Guid.NewGuid(), 
            Title = "Bí quyết giữ hoa hồng tươi lâu thêm 7 ngày bằng nguyên liệu nhà bếp", 
            Description = "Hướng dẫn cách sử dụng đường, chanh và nước tẩy quần áo theo tỷ lệ chuẩn để nuôi dưỡng hoa hồng cắt cành tươi lâu.", // <--- BỔ SUNG TRƯỜNG NÀY
            ViewCount = 320, 
            Status = "Published" 
        },
        new() 
        { 
            Id = Guid.NewGuid(), 
            Title = "Bộ sưu tập mẫu hoa cưới phong cách tối giản dẫn đầu xu hướng năm nay", 
            Description = "Giới thiệu các thiết kế hoa cầm tay cô dâu sử dụng tone màu pastel thuần khiết, tối giản nhưng vô cùng tinh tế.", // <--- BỔ SUNG TRƯỜNG NÀY
            ViewCount = 12, 
            Status = "Draft" 
        }
    };

    // 3. Đưa dữ liệu vào bộ thay đổi và lưu xuống DB
    await context.NewsList.AddRangeAsync(newsList);
    await context.SaveChangesAsync(); // Chạy qua dòng này thành công và không còn crash nữa

    return newsList;
}

    private static async Task SeedCategoryNewsDetailsAsync(AppDbContext context, List<CategoryNews>? categories, List<News>? newsList)
    {
        if (await context.CategoryNewsDetails.AnyAsync()) return;
        
        // Nếu các hàm seed trước bị skip, truy vấn dữ liệu hiện tại từ database để tạo liên kết
        categories ??= await context.CategoryNewsList.ToListAsync();
        newsList ??= await context.NewsList.ToListAsync();

        if (!categories.Any() || !newsList.Any()) return;

        var details = new List<CategoryNewsDetail>
        {
            // Bài viết 1 nằm trong danh mục "Tin tức Khuyến mãi" và danh mục con "Sự kiện Valentine"
            new() { Id = Guid.NewGuid(), NewsID = newsList[0].Id, CategoryNewsID = categories[0].Id },
            new() { Id = Guid.NewGuid(), NewsID = newsList[0].Id, CategoryNewsID = categories[3].Id },
            
            // Bài viết 2 thuộc danh mục con "Hoa tươi mùa Hạ"
            new() { Id = Guid.NewGuid(), NewsID = newsList[1].Id, CategoryNewsID = categories[6].Id },
            
            // Bài viết 3 thuộc danh mục con "Chăm sóc Hoa Hồng"
            new() { Id = Guid.NewGuid(), NewsID = newsList[2].Id, CategoryNewsID = categories[7].Id }
        };

        await context.CategoryNewsDetails.AddRangeAsync(details);
        await context.SaveChangesAsync();
    }

    private static async Task SeedAuditLogsAsync(AppDbContext context, Guid userId)
    {
        if (await context.AuditLogs.AnyAsync()) return;

        var logs = new List<AuditLog>
        {
            new() { Id = Guid.NewGuid(), UserID = userId, Action = "CREATE_USER", Entity = "User" },
            new() { Id = Guid.NewGuid(), UserID = userId, Action = "SEED_DATA", Entity = "System" }
        };

        await context.AuditLogs.AddRangeAsync(logs);
        await context.SaveChangesAsync();
    }
}