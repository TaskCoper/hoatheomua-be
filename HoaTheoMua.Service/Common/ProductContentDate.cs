using HoaTheoMua.Repository;
using ProductEntity = HoaTheoMua.Repository.Entity.Product;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.Common;

/// <summary>
/// Quản lý "ngày nội dung sản phẩm được sửa lần cuối" (Product.NhanhUpdatedAt) dùng cho sitemap lastmod.
/// Chỉ ghi khi khách xem trang thấy khác đi. Tồn kho, ghim, thứ tự không tính.
/// </summary>
public static class ProductContentDate
{
    /// <summary>Chỉ ghi tiến: giữ ngày mới hơn. Chưa có ngày thì ghi luôn.</summary>
    public static void Mark(ProductEntity product, DateTimeOffset at)
    {
        if (product.NhanhUpdatedAt == null || at > product.NhanhUpdatedAt)
        {
            product.NhanhUpdatedAt = at;
        }
    }

    /// <summary>
    /// Ghi ngày cho sản phẩm, và cho sản phẩm cha nếu nó là biến thể
    /// (cha = sản phẩm có NhanhProductId == ParentProductId của con).
    /// </summary>
    public static async Task MarkWithParentAsync(AppDbContext dbContext, ProductEntity product, DateTimeOffset at)
    {
        Mark(product, at);
        await MarkParentAsync(dbContext, product, at);
    }

    /// <summary>Chỉ ghi ngày cho sản phẩm cha (dùng khi sản phẩm con bị xoá).</summary>
    public static async Task MarkParentAsync(AppDbContext dbContext, ProductEntity product, DateTimeOffset at)
    {
        if (string.IsNullOrWhiteSpace(product.ParentProductId))
        {
            return;
        }

        var parent = await dbContext.Products
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.NhanhProductId == product.ParentProductId);

        if (parent != null)
        {
            Mark(parent, at);
        }
    }

    /// <summary>Các field khách nhìn thấy trên trang sản phẩm. Dùng để biết sản phẩm có thật sự bị sửa không.</summary>
    public readonly record struct VisibleFields(
        string? ProductName,
        string? CoverImage,
        decimal? Price,
        decimal? PriceWithVat,
        decimal? OldPrice,
        string? Slug,
        string? ProductLabel,
        string? PriceLabel,
        string? Size,
        string? Color,
        string? Origin,
        string? CategoryName);

    public static VisibleFields Snapshot(ProductEntity product) => new(
        product.ProductName,
        product.CoverImage,
        product.Price,
        product.PriceWithVat,
        product.OldPrice,
        product.Slug,
        product.ProductLabel,
        product.PriceLabel,
        product.Size,
        product.Color,
        product.Origin,
        product.CategoryName);
}
