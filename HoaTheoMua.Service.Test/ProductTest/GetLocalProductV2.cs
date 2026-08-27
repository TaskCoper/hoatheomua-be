using System.Reflection;
using System.Security.Claims;
using HoaTheoMua.Constants;
using HoaTheoMua.Controllers;
using HoaTheoMua.Extentions;
using HoaTheoMua.Service.Models;
using HoaTheoMua.Service.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HoaTheoMua.Service.Test.ProductTest;

public class GetLocalProductV2
{
    [Fact]
    public void V1Detail_HasAdminAuthorizationPolicy()
    {
        // Arrange
        var method = typeof(ProductController).GetMethod(nameof(ProductController.GetLocalProductDetail));

        // Act
        var authorize = method?.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();

        // Assert
        Assert.NotNull(authorize);
        Assert.Equal(JwtExtensions.AdminPolicy, authorize!.Policy);
    }

    [Fact]
    public void V2Detail_DoesNotRequireAuthorizationAttribute()
    {
        // Arrange
        var method = typeof(ProductController).GetMethod(nameof(ProductController.GetLocalProductDetailV2));

        // Act
        var authorize = method?.GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();

        // Assert
        Assert.Null(authorize);
    }

    [Fact]
    public async Task V1Detail_AuthorizedAdminRequest_ReturnsOkWithServiceResult()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var serviceResult = new Response.LocalProductDetailResponse
        {
            Id = productId,
            ProductName = "Admin product"
        };
        var productService = new Mock<IService>();
        productService
            .Setup(service => service.GetLocalProductDetail(productId))
            .ReturnsAsync(serviceResult);

        var controller = CreateController(productService, RoleNames.Admin, "trace-v1");

        // Act
        var actionResult = await controller.GetLocalProductDetail(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var response = Assert.IsType<BaseResponse>(okResult.Value);
        Assert.True(response.IsSuccess);
        Assert.Same(serviceResult, response.Value);
        Assert.Equal("trace-v1", response.TraceId);
        productService.Verify(service => service.GetLocalProductDetail(productId), Times.Once);
    }

    [Fact]
    public async Task V2Detail_AdminRequest_IsAllowedAndReturnsOk()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var serviceResult = new Response.LocalProductDetailResponse
        {
            Id = productId,
            ProductName = "Public product"
        };
        var productService = new Mock<IService>();
        productService
            .Setup(service => service.GetLocalProductDetailV2(productId))
            .ReturnsAsync(serviceResult);

        var controller = CreateController(productService, RoleNames.Admin, "trace-v2");

        // Act
        var actionResult = await controller.GetLocalProductDetailV2(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var response = Assert.IsType<BaseResponse>(okResult.Value);
        Assert.True(response.IsSuccess);
        Assert.Same(serviceResult, response.Value);
        Assert.Equal("trace-v2", response.TraceId);
        productService.Verify(service => service.GetLocalProductDetailV2(productId), Times.Once);
    }

    [Fact]
    public async Task V2Detail_AnonymousRequest_IsAllowedBecauseEndpointIsPublic()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productService = new Mock<IService>();
        productService
            .Setup(service => service.GetLocalProductDetailV2(productId))
            .ReturnsAsync(new Response.LocalProductDetailResponse { Id = productId });

        var controller = CreateController(productService, role: null, "trace-v2-anonymous");

        // Act
        var actionResult = await controller.GetLocalProductDetailV2(productId);

        // Assert
        Assert.IsType<OkObjectResult>(actionResult);
        productService.Verify(service => service.GetLocalProductDetailV2(productId), Times.Once);
    }

    private static ProductController CreateController(
        Mock<IService> productService,
        string? role,
        string traceIdentifier)
    {
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = traceIdentifier
        };

        if (role != null)
        {
            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.Role, role) },
                    authenticationType: "Test"));
        }

        return new ProductController(
            productService.Object,
            Mock.Of<HoaTheoMua.Service.Revalidate.IService>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }
}
