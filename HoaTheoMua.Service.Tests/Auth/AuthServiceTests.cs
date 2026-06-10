using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Auth;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.MailService;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

// Alias to avoid ambiguity with HoaTheoMua.Service.User namespace
using UserEntity = HoaTheoMua.Repository.Entity.User;

namespace HoaTheoMua.Service.Tests.Auth;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<MailService.IService> _mailServiceMock;
    private readonly Mock<JwtService.IService> _jwtServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly IConfiguration _configuration;

    public AuthServiceTests()
    {
        // 1. Setup InMemory Database and configure to ignore Transactions for tests using BeginTransactionAsync
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _dbContext = new AppDbContext(options);

        // 2. Setup Configuration
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "SecurityOptions:Pepper", "test_pepper" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // 3. Setup Mocks
        _mailServiceMock = new Mock<MailService.IService>();
        _jwtServiceMock = new Mock<JwtService.IService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    }

    private HoaTheoMua.Service.Auth.Service CreateService()
    {
        return new HoaTheoMua.Service.Auth.Service(
            _dbContext,
            _configuration,
            _mailServiceMock.Object,
            _httpContextAccessorMock.Object,
            _jwtServiceMock.Object
        );
    }

    private void SetupHttpContext(Guid? userId = null, long? expirationSeconds = null, string? refreshTokenCookie = null, string? userAgent = null)
    {
        var httpContextMock = new Mock<HttpContext>();
        var httpRequestMock = new Mock<HttpRequest>();
        var cookieCollectionMock = new Mock<IRequestCookieCollection>();
        var connectionMock = new Mock<ConnectionInfo>();

        // Claims Principal
        var claims = new List<Claim>();
        if (userId.HasValue)
        {
            claims.Add(new Claim("UserId", userId.Value.ToString()));
        }
        if (expirationSeconds.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.Expiration, expirationSeconds.Value.ToString()));
        }
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        httpContextMock.Setup(h => h.User).Returns(principal);

        // Cookies
        cookieCollectionMock.Setup(c => c["HTM-Refresh-Token"]).Returns(refreshTokenCookie);
        httpRequestMock.Setup(r => r.Cookies).Returns(cookieCollectionMock.Object);

        // Headers
        var headers = new HeaderDictionary();
        if (userAgent != null)
        {
            headers["User-Agent"] = userAgent;
        }
        httpRequestMock.Setup(r => r.Headers).Returns(headers);
        httpContextMock.Setup(h => h.Request).Returns(httpRequestMock.Object);

        // Connection IP
        connectionMock.Setup(c => c.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("127.0.0.1"));
        httpContextMock.Setup(h => h.Connection).Returns(connectionMock.Object);

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Register Tests

    [Fact]
    public async Task Register_WhenPasswordsDoNotMatch_ThrowsArgumentException()
    {
        // Arrange
        var request = new Request.RegisterRequest
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "password123",
            ConfirmPassword = "different_password"
        };
        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.Register(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Password không khớp!");
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ThrowsBadHttpRequestException()
    {
        // Arrange
        var email = "existing@example.com";
        var existingUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "Jane",
            LastName = "Doe",
            HashPassword = "hashedpassword",
            IsVerify = true
        };
        await _dbContext.Users.AddAsync(existingUser);
        await _dbContext.SaveChangesAsync();

        var request = new Request.RegisterRequest
        {
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            Password = "password123",
            ConfirmPassword = "password123"
        };
        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.Register(request);

        // Assert
        await act.Should().ThrowAsync<BadHttpRequestException>()
            .WithMessage($"Email {email} đã tồn tại trong hệ thống");
    }

    [Fact]
    public async Task Register_WhenRequestIsValid_CreatesUserAndSendsVerificationEmail()
    {
        // Arrange
        var request = new Request.RegisterRequest
        {
            Email = "newuser@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        _jwtServiceMock.Setup(x => x.GenerateEmailVerificationToken(It.IsAny<List<Claim>>(), 2))
            .Returns("email_verification_token_abc");

        _mailServiceMock.Setup(x => x.SendMail(It.IsAny<MailContent>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.Register(request);

        // Assert
        // 1. Verify result message
        result.Should().Be("Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản.");

        // 2. Verify user is in DB
        var userInDb = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == "newuser@example.com");
        userInDb.Should().NotBeNull();
        userInDb!.FirstName.Should().Be("John");
        userInDb.LastName.Should().Be("Doe");
        userInDb.IsVerify.Should().BeFalse();
        userInDb.Role.Should().Be(Role.User);

        // Verify password was hashed (using BCrypt)
        BCrypt.Net.BCrypt.EnhancedVerify(request.Password + "test_pepper", userInDb.HashPassword, BCrypt.Net.HashType.SHA256)
            .Should().BeTrue();

        // 3. Verify mail was sent
        _mailServiceMock.Verify(x => x.SendMail(It.Is<MailContent>(m =>
            m.To == "newuser@example.com" &&
            m.Subject == "Hoa Theo Mua" &&
            m.Body.Contains("email_verification_token_abc")
        )), Times.Once);
    }

    #endregion

    #region VerifyEmail Tests

    [Fact]
    public async Task VerifyEmail_WhenTokenInvalid_ThrowsBadHttpRequestException()
    {
        // Arrange
        var request = new Request.VerifyEmailRequest { Token = "invalid_token" };
        _jwtServiceMock.Setup(x => x.ValidateToken("invalid_token")).Returns((ClaimsPrincipal)null!);

        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.VerifyEmail(request);

        // Assert
        await act.Should().ThrowAsync<BadHttpRequestException>()
            .WithMessage("Liên kết xác thực đã hết hạn hoặc không hợp lệ.");
    }

    [Fact]
    public async Task VerifyEmail_WhenUserNotFound_ThrowsBadHttpRequestException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new Request.VerifyEmailRequest { Token = "valid_token" };
        
        // Mock token validation to return claims principal with UserId
        var claims = new List<Claim> { new Claim("UserId", userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        _jwtServiceMock.Setup(x => x.ValidateToken("valid_token")).Returns(principal);

        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.VerifyEmail(request);

        // Assert
        await act.Should().ThrowAsync<BadHttpRequestException>()
            .WithMessage("Người dùng không tồn tại trên hệ thống.");
    }

    [Fact]
    public async Task VerifyEmail_WhenUserAlreadyVerified_ReturnsVerifyEmailResponseWithNoTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "verified@example.com",
            FirstName = "John",
            LastName = "Doe",
            HashPassword = "hashedpassword",
            IsVerify = true
        };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new Request.VerifyEmailRequest { Token = "valid_token" };
        
        var claims = new List<Claim> { new Claim("UserId", userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        _jwtServiceMock.Setup(x => x.ValidateToken("valid_token")).Returns(principal);

        var service = CreateService();

        // Act
        var result = await service.VerifyEmail(request);

        // Assert
        result.Should().NotBeNull();
        result!.AccessToken.Should().BeNull();
        result.RefreshToken.Should().BeNull();
        result.Message.Should().Be("Người dùng đã xác thực");
    }

    [Fact]
    public async Task VerifyEmail_WhenRequestIsValid_VerifiesUserAndGeneratesTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "unverified@example.com",
            FirstName = "John",
            LastName = "Doe",
            HashPassword = "hashedpassword",
            IsVerify = false
        };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new Request.VerifyEmailRequest { Token = "valid_token" };
        
        var claims = new List<Claim> { new Claim("UserId", userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        _jwtServiceMock.Setup(x => x.ValidateToken("valid_token")).Returns(principal);

        // Mock token generation
        _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<List<Claim>>())).Returns("new_access_token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("new_refresh_token");

        SetupHttpContext(userAgent: "Firefox/2.0");

        var service = CreateService();

        // Act
        var result = await service.VerifyEmail(request);

        // Assert
        // 1. Verify result response
        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("new_access_token");
        result.RefreshToken.Should().Be("new_refresh_token");
        result.Message.Should().Be("Xác thực tài khoản thành công! Chào mừng bạn đến với Hoa Theo Mùa.");

        // 2. Verify user state in DB
        var userInDb = await _dbContext.Users.FindAsync(userId);
        userInDb.Should().NotBeNull();
        userInDb!.IsVerify.Should().BeTrue();
        userInDb.UpdatedAt.Should().NotBeNull();

        // 3. Verify new refresh token created in DB
        var tokenInDb = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.UserId == userId);
        tokenInDb.Should().NotBeNull();
        tokenInDb!.RefreshTokenHash.Should().Be("new_refresh_token");
        tokenInDb.IpAddress.Should().Be("127.0.0.1"); // setup in SetupHttpContext
        tokenInDb.UserAgent.Should().Be("Firefox/2.0");
        tokenInDb.IsActive.Should().BeTrue();
    }

    #endregion

    #region GetMe Tests

    [Fact]
    public async Task GetMe_WhenAccessTokenInvalid_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        SetupHttpContext(userId: null);
        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.GetMe();

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Access token is invalid");
    }

    [Fact]
    public async Task GetMe_WhenUserNotFoundInDatabase_ThrowsBadHttpRequestException()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        SetupHttpContext(userId: nonExistentUserId);
        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.GetMe();

        // Assert
        await act.Should().ThrowAsync<BadHttpRequestException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task GetMe_WhenUserExists_ReturnsGetMeResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Avatar = "avatar.png",
            HashPassword = "hashedpassword",
            IsVerify = true
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        SetupHttpContext(userId: userId);
        var service = CreateService();

        // Act
        var result = await service.GetMe();

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("test@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Avatar.Should().Be("avatar.png");
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshToken_WhenAccessTokenStillValid_ThrowsBadHttpRequestException()
    {
        // Arrange
        var futureExpiration = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
        SetupHttpContext(userId: Guid.NewGuid(), expirationSeconds: futureExpiration);
        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.RefreshToken();

        // Assert
        await act.Should().ThrowAsync<BadHttpRequestException>()
            .WithMessage("ACCESS_TOKEN_STILL_VALID");
    }

    [Fact]
    public async Task RefreshToken_WhenMissingRefreshTokenCookie_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var pastExpiration = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds();
        SetupHttpContext(userId: Guid.NewGuid(), expirationSeconds: pastExpiration, refreshTokenCookie: null);
        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.RefreshToken();

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("INVALID_REFRESH_TOKEN");
    }

    [Fact]
    public async Task RefreshToken_WhenRefreshTokenNotFoundInDatabase_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var pastExpiration = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds();
        var rawRefreshToken = "non_existent_token";
        SetupHttpContext(userId: Guid.NewGuid(), expirationSeconds: pastExpiration, refreshTokenCookie: rawRefreshToken);
        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.RefreshToken();

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("INVALID_REFRESH_TOKEN");
    }

    [Fact]
    public async Task RefreshToken_WhenRefreshTokenIsRevoked_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            HashPassword = "hashedpassword",
            IsVerify = true
        };

        var revokedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            RefreshTokenHash = "revoked_refresh_token",
            UserId = userId,
            User = user,
            ExpiredAt = DateTimeOffset.UtcNow.AddDays(7),
            RevokedAt = DateTimeOffset.UtcNow.AddDays(-1), // Revoked 1 day ago
            IpAddress = "127.0.0.1",
            UserAgent = "Mozilla",
            DeviceLabel = "Desktop",
            IsActive = false
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.RefreshTokens.AddAsync(revokedToken);
        await _dbContext.SaveChangesAsync();

        var pastExpiration = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds();
        SetupHttpContext(userId: userId, expirationSeconds: pastExpiration, refreshTokenCookie: "revoked_refresh_token");
        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.RefreshToken();

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("EXPIRED_REFRESH_TOKEN");
    }

    [Fact]
    public async Task RefreshToken_WhenRefreshTokenIsExpired_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            HashPassword = "hashedpassword",
            IsVerify = true
        };

        var expiredToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            RefreshTokenHash = "expired_refresh_token",
            UserId = userId,
            User = user,
            ExpiredAt = DateTimeOffset.UtcNow.AddDays(-1), // Expired 1 day ago
            RevokedAt = null,
            IpAddress = "127.0.0.1",
            UserAgent = "Mozilla",
            DeviceLabel = "Desktop",
            IsActive = true
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.RefreshTokens.AddAsync(expiredToken);
        await _dbContext.SaveChangesAsync();

        var pastExpiration = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds();
        SetupHttpContext(userId: userId, expirationSeconds: pastExpiration, refreshTokenCookie: "expired_refresh_token");
        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.RefreshToken();

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("EXPIRED_REFRESH_TOKEN");
    }

    [Fact]
    public async Task RefreshToken_WhenRefreshTokenIsValid_RevokesOldTokenAndGeneratesNewToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            HashPassword = "hashedpassword",
            IsVerify = true
        };

        var validToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            RefreshTokenHash = "valid_refresh_token",
            UserId = userId,
            User = user,
            ExpiredAt = DateTimeOffset.UtcNow.AddDays(7),
            RevokedAt = null,
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            DeviceLabel = "Chrome-Desktop",
            IsActive = true
        };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.RefreshTokens.AddAsync(validToken);
        await _dbContext.SaveChangesAsync();

        var pastExpiration = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds();
        SetupHttpContext(userId: userId, expirationSeconds: pastExpiration, refreshTokenCookie: "valid_refresh_token", userAgent: "Firefox/1.0");

        _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("new_generated_refresh_token");
        _jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<List<Claim>>())).Returns("new_generated_access_token");

        var service = CreateService();

        // Act
        var result = await service.RefreshToken();

        // Assert
        // 1. Verify result contains new tokens
        result.Should().NotBeNull();
        result.RefreshToken.Should().Be("new_generated_refresh_token");
        result.AccessToken.Should().Be("new_generated_access_token");

        // 2. Verify old refresh token in DB was updated correctly
        var updatedOldToken = await _dbContext.RefreshTokens.FindAsync(validToken.Id);
        updatedOldToken.Should().NotBeNull();
        updatedOldToken!.RevokedAt.Should().NotBeNull();
        updatedOldToken.IsActive.Should().BeFalse();

        // 3. Verify new refresh token was inserted in DB
        var insertedNewToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.RefreshTokenHash == "new_generated_refresh_token");
        insertedNewToken.Should().NotBeNull();
        insertedNewToken!.UserId.Should().Be(userId);
        insertedNewToken.IpAddress.Should().Be("127.0.0.1"); // mocked connection RemoteIpAddress
        insertedNewToken.UserAgent.Should().Be("Firefox/1.0"); // mocked user agent
        insertedNewToken.DeviceLabel.Should().Be("Chrome-Desktop"); // copied from old device label
        insertedNewToken.IsActive.Should().BeTrue();
        insertedNewToken.ExpiredAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    #endregion
}
