namespace HoaTheoMua.Service.Product;

public interface IService
{
    Task<object> GetListProducts(Request.ProductListRequest request);
    Task<object> GetProductDetails(Request.ProductDetailRequest request);
}