using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

using HoaTheoMua.Repository;
using HoaTheoMua.Service.CompanyAddress;
using HoaTheoMua.Service.Exceptions;

using CompanyAddressEntity = HoaTheoMua.Repository.Entity.CompanyAddress;
using CompanyAddressService = HoaTheoMua.Service.CompanyAddress.Service;

namespace HoaTheoMua.Service.Test.CompanyAddressTest;

public class CreateCompanyAddressTest
{
    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }

    private Request.CreateCompanyAddressRequest CreateValidRequest()
    {
        return new Request.CreateCompanyAddressRequest
        {
            AddressLine = "123 Nguyen Hue",
            Ward = "Ben Nghe",
            City = "Ho Chi Minh",
            Country = "Vietnam",
            FullName = "Hoa Theo Mua",
            Phone = "0909123456",
            Remarks = "Company address",
            Latitude = 10.776889m, //m:so kieu decimal
            Longitude = 106.700806m,
            MapId = "map-new"
        };
    }

    private List<ValidationResult> ValidateModel(object model)
    // kiem tra annotation( [Required], [StringLength], [Range]
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);

        Validator.TryValidateObject(model, context, results, true);

        return results;
    }

    // TC01: Tạo địa chỉ công ty thành công
    [Fact]
    public async Task Create_Success()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var service = new CompanyAddressService(dbContext);
        var request = CreateValidRequest();

        // Act
        var result = await service.CreateCompanyAddress(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("123 Nguyen Hue", result.AddressLine);
        Assert.Equal("Ben Nghe", result.Ward);
        Assert.Equal("Ho Chi Minh", result.City);
        Assert.Equal("Vietnam", result.Country);
        Assert.Equal("0909123456", result.Phone);
        Assert.Equal("Hoa Theo Mua", result.FullName);
        Assert.Equal("Company address", result.Remarks);
        Assert.Equal(10.776889m, result.Latitude);
        Assert.Equal(106.700806m, result.Longitude);
        Assert.Equal("map-new", result.MapId);
        Assert.Null(result.UpdatedAt);

        var addressInDb = await dbContext.CompanyAddresses.SingleAsync();

        Assert.Equal(result.Id, addressInDb.Id);
        Assert.False(addressInDb.IsDeleted);
        Assert.NotEqual(default, addressInDb.CreatedAt);
    }

    // TC02: Đã tồn tại địa chỉ active
    [Fact]
    public async Task Create_ActiveAddressExists_ThrowConflict()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        dbContext.CompanyAddresses.Add(new CompanyAddressEntity
        {
            Id = Guid.NewGuid(),
            AddressLine = "Old Address",
            Ward = "Ward 1",
            City = "Ho Chi Minh",
            PhoneNumber = "0111111111",
            FullName = "Old Company",
            Remarks = "Old address remark",
            Country = "Vietnam",
            Latitude = 10.776889m,
            Longitude = 106.700806m,
            MapId = "map-old",
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = null
        });

        await dbContext.SaveChangesAsync();

        var service = new CompanyAddressService(dbContext);
        var request = CreateValidRequest();

        // Act
        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => service.CreateCompanyAddress(request)
        );

        // Assert
        Assert.Equal(
            "Chỉ được phép tồn tại một địa chỉ công ty trong cùng một khoảng thời gian",
            exception.Message
        );

        Assert.Equal(1, await dbContext.CompanyAddresses.CountAsync());
    }

    // TC03: Request thiếu required fields
    [Fact]
    public void Create_MissingRequiredFields_Invalid()
    {
        // Arrange
        var request = CreateValidRequest();

        request.AddressLine = null!;
        request.City = null!;
        request.Country = null!;

        // Act
        var errors = ValidateModel(request);

        // Assert
        Assert.Contains(errors, x => x.ErrorMessage == "Địa chỉ không được để trống");
        Assert.Contains(errors, x => x.ErrorMessage == "Tỉnh/Thành phố không được để trống");
        Assert.Contains(errors, x => x.ErrorMessage == "Quốc gia không được để trống");
    }

    // TC04: Các field required bị để trống
    [Fact]
    public void Create_EmptyRequiredFields_Invalid()
    {
        // Arrange
        var request = CreateValidRequest();

        request.AddressLine = "";
        request.City = "";
        request.Country = "";
        request.FullName = "";

        // Act
        var errors = ValidateModel(request);

        // Assert
        Assert.Contains(errors, x => x.ErrorMessage == "Địa chỉ không được để trống");
        Assert.Contains(errors, x => x.ErrorMessage == "Tỉnh/Thành phố không được để trống");
        Assert.Contains(errors, x => x.ErrorMessage == "Quốc gia không được để trống");
        Assert.Contains(errors, x => x.ErrorMessage == "Họ và tên phải từ 2 đến 100 ký tự");
    }

    // TC05: Latitude không hợp lệ
    [Theory]
    [InlineData(91)]
    [InlineData(-91)]
    // latitude hop le tu -90 -> 90
    public void Create_InvalidLatitude_Invalid(decimal latitude)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Latitude = latitude;

        // Act
        var errors = ValidateModel(request);

        // Assert
        Assert.Contains(errors, x => x.ErrorMessage == "Vĩ độ phải nằm trong khoảng -90 đến 90");
    }

    // TC05: Longitude không hợp lệ
    [Theory]
    [InlineData(181)]
    [InlineData(-181)]
    // longitude hop le tu -180 -> 180
    public void Create_InvalidLongitude_Invalid(decimal longitude)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Longitude = longitude;

        // Act
        var errors = ValidateModel(request);

        // Assert
        Assert.Contains(errors, x => x.ErrorMessage == "Kinh độ phải nằm trong khoảng -180 đến 180");
    }

    // TC06: AddressLine quá dài
    [Fact]
    public void Create_AddressLineTooLong_Invalid()
    {
        // Arrange
        var request = CreateValidRequest();

        request.AddressLine = new string('A', 256);

        // Act
        var errors = ValidateModel(request);

        // Assert
        Assert.Contains(errors, x => x.ErrorMessage == "Địa chỉ phải từ 5 đến 255 ký tự");
    }

    // TC07: Địa chỉ cũ đã soft delete thì tạo lại được
    [Fact]
    public async Task Create_SoftDeletedAddressExists_Success()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        dbContext.CompanyAddresses.Add(new CompanyAddressEntity
        {
            Id = Guid.NewGuid(),
            AddressLine = "123 Nguyen Hue",
            Ward = "Ben Nghe",
            City = "Ho Chi Minh",
            PhoneNumber = "0909123456",
            FullName = "Hoa Theo Mua",
            Remarks = "Company address",
            Country = "Vietnam",
            Latitude = 10.776889m,
            Longitude = 106.700806m,
            MapId = "map-new",
            IsDeleted = true,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();

        var service = new CompanyAddressService(dbContext);
        var request = CreateValidRequest();

        // Act
        var result = await service.CreateCompanyAddress(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123 Nguyen Hue", result.AddressLine);
        Assert.Equal("Ho Chi Minh", result.City);
        Assert.Equal("Vietnam", result.Country);

        Assert.Equal(2, await dbContext.CompanyAddresses.CountAsync());
        Assert.Equal(1, await dbContext.CompanyAddresses.CountAsync(x => !x.IsDeleted));
        Assert.Equal(1, await dbContext.CompanyAddresses.CountAsync(x => x.IsDeleted));
    }
}