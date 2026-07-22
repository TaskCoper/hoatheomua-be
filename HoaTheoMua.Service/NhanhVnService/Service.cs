using System.Net.Http.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.NhanhVnService;

public class Service: IService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _appDbContext;
    private readonly NhanhVnOption _nhanhVnOption;

    public Service(HttpClient httpClient, AppDbContext appDbContext, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _appDbContext = appDbContext;
        _nhanhVnOption = new NhanhVnOption();
        configuration.GetSection(nameof(NhanhVnOption)).Bind(_nhanhVnOption);
    }
    
    public async Task<OrderLabelResponse> GetOrderLabel(OrderLabelRequest orderLabelRequest)
    {
        var configs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_appDbContext, "nhanhvn");
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);
        var nhanhApi = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl) + "/order/tags?appId=" + appId + "&businessId=" + businessId;  
        
        
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(orderLabelRequest);
        
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        OrderLabelResponse? nhanhProductOrderLable = null;
        
        if (nhanhVnResponse.IsSuccessStatusCode)
        {

            string rawJsonString = await nhanhVnResponse.Content.ReadAsStringAsync();

            // Đặt đoạn mã này để bạn đặt Breakpoint hoặc dùng NLog/Serilog in ra Tab Console xem giá trị thô:
            // Console.WriteLine($"[DEBUG NHANH.VN RAW DATA]: {rawJsonString}");


            nhanhProductOrderLable = await nhanhVnResponse.Content.ReadFromJsonAsync<OrderLabelResponse>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (nhanhProductOrderLable == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }
        
        if (nhanhProductOrderLable.Code == 0)
        {
            throw new BadRequestException("ERR_INVALID_FORM_FIELDS");
        }

        return nhanhProductOrderLable;
    }

    public async Task<OrderSourceResponse> GetOrderSource(OrderSourceRequest orderSourceRequest)
    {
        var configs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_appDbContext, "nhanhvn");
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);
        var nhanhApi = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl) + "/order/source?appId=" + appId + "&businessId=" + businessId;  
        
        
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(orderSourceRequest);
        
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        OrderSourceResponse? nhanhProductOrderSource = null;
        
        if (nhanhVnResponse.IsSuccessStatusCode)
        {

            string rawJsonString = await nhanhVnResponse.Content.ReadAsStringAsync();

            // Đặt đoạn mã này để bạn đặt Breakpoint hoặc dùng NLog/Serilog in ra Tab Console xem giá trị thô:
            // Console.WriteLine($"[DEBUG NHANH.VN RAW DATA]: {rawJsonString}");


            nhanhProductOrderSource = await nhanhVnResponse.Content.ReadFromJsonAsync<OrderSourceResponse>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (nhanhProductOrderSource == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }
        
        if (nhanhProductOrderSource.Code == 0)
        {
            throw new BadRequestException("ERR_INVALID_FORM_FIELDS");
        }

        return nhanhProductOrderSource;
    }

    public async Task<string> WebhookNhanhVnInventory(NhanhInventoryChangeWebhookRequest request)
    {
        if (request == null || request.Data == null || !request.Data.Any())
        {
            throw new BadRequestException("Không có dữ liệu thay đổi tồn kho.");
        }

        var nhanhProductIds = request.Data
            .Select(x => x.Id.ToString()).Distinct().ToList();

        var products = await _appDbContext.Products
            .Where(x => !x.IsDeleted && nhanhProductIds.Contains(x.NhanhProductId!))
            .ToListAsync();

        foreach (var item in request.Data)
        {
            var itemId = item.Id.ToString();

            var product = products.FirstOrDefault(x => x.NhanhProductId == itemId);

            if (product != null)
            {
                product.AvailableForSale = (decimal?)item.Available;
                product.ProductCode = item.Code;
                product.TotalStock = (decimal?)item.Remain;
                product.Stock = (decimal?)item.Remain;
                product.DeliveringStock = (decimal?)item.Shipping;
                product.StockError = (decimal?)item.Damaged;
                product.Held = (decimal?)item.Holding;
                product.StockTransferring = (decimal?)item.Transfering;
                
                product.WarehouseStock = (decimal?)item.Warranty!.Remain;
                product.WarrantyStock = (decimal?)item.Warranty.Remain;
                product.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await _appDbContext.SaveChangesAsync();

        return "Cập nhật tồn kho thành công";
    }

    public async Task<string> WebhookNhanhVnProductAdd(NhanhProductAddWebhookRequest request)
    {
        if (request == null || request.Data == null || !request.Data.Any())
        {
            throw new BadRequestException("Không có dữ liệu thay đổi khi thêm sản phẩm.");
        }

        var nhanhProductIds = request.Data.Select(x => x.Id.ToString()).Distinct().ToList();

        var existingProducts = await _appDbContext.Products
            .Where(x => !x.IsDeleted && nhanhProductIds.Contains(x.NhanhProductId!))
            .ToListAsync();

        if (existingProducts.Any())
        {
            throw new BadRequestException("Có sản phẩm đã tồn tại trong hệ thống");
        }

        var newProducts = new List<HoaTheoMua.Repository.Entity.Product>();

        foreach (var item in request.Data)
        {
            var product = new HoaTheoMua.Repository.Entity.Product
            {
                Id = Guid.NewGuid(),
                NhanhProductId = item.Id.ToString(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            
            product.ParentProductId = item.ParentId != 0 ? item.ParentId.ToString() : null;
            product.ProductCode = item.Code;
            product.Barcode = item.Barcode;
            product.ProductName = item.Name;
            product.OtherName = item.OtherName;
            product.Status = item.Status;

            // Danh mục
            product.CategoryCode = item.Category?.Code;
            product.CategoryName = item.Category?.Name;
            product.InternalCategoryCode = item.InternalCategory?.Code;
            product.ProductType = item.Type?.Name;

            // Giá
            if (item.Prices != null)
            {
                product.Price = (decimal)item.Prices.Retail;
                product.ImportPrice = (decimal)item.Prices.Import;
                product.OldPrice = (decimal)item.Prices.Old;
                product.WholesalePrice = (decimal)item.Prices.Wholesale;
                product.CostPrice = (decimal)item.Prices.AvgCost;
            }
            product.Vat = item.Vat;

            // Ảnh
            product.CoverImage = item.Images?.Avatar;

            // Bảo hành
            product.WarrantyMonths = item.Warranty?.Month;
            product.WarrantyAddress = item.Warranty?.Address;

            // Thương hiệu & nhà cung cấp
            product.Brand = item.Brand?.Name;
            product.Supplier = item.Suppliers?.Name;

            // Kích thước & cân nặng
            if (item.Shipping != null)
            {
                product.Length = (decimal)item.Shipping.Length;
                product.Width = (decimal)item.Shipping.Width;
                product.Height = (decimal)item.Shipping.Height;
                product.Weight = (decimal)item.Shipping.Weight;
            }

            // Xuất xứ & đơn vị tính
            product.Origin = item.CountryName;
            product.Unit = item.Units?.Name;

            // Tồn kho
            if (item.Inventory != null)
            {
                product.TotalStock = (decimal)item.Inventory.Remain;
                product.Stock = (decimal)item.Inventory.Remain;
                product.WarehouseStock = (decimal)item.Inventory.Remain;
                product.DeliveringStock = (decimal)item.Inventory.Shipping;
                product.StockError = (decimal)item.Inventory.Damaged;
                product.Held = (decimal)item.Inventory.Holding;
                product.StockTransferring = (decimal)item.Inventory.Transfering;
                product.AvailableForSale = (decimal)item.Inventory.Available;
                product.WarrantyStock = (decimal)item.Inventory.Warranty;
                product.HeldComponents = (decimal)item.Inventory.WarrantyHolding;
            }

            // Branch Prices (Lấy chi nhánh đầu tiên nếu có)
            if (item.BranchPrices != null && item.BranchPrices.Any())
            {
                var firstBranchPrice = item.BranchPrices.First();
                product.BranchPrice = (decimal)firstBranchPrice.Price;
                product.WholesaleBranchPrice = (decimal)firstBranchPrice.WholesalePrice;
            }

            // Attributes (Size, Color)
            if (item.Attributes != null && item.Attributes.Any())
            {
                var sizeAttr = item.Attributes
                    .FirstOrDefault(a => 
                        a.Name != null && (a.Name.Equals("Kích thước", StringComparison.OrdinalIgnoreCase) 
                                           || a.Name.Equals("Size", StringComparison.OrdinalIgnoreCase)));
                if (sizeAttr != null) product.Size = sizeAttr.Value;

                var colorAttr = item.Attributes
                    .FirstOrDefault(a => 
                        a.Name != null && (a.Name.Equals("Màu sắc", StringComparison.OrdinalIgnoreCase) 
                                           || a.Name.Equals("Color", StringComparison.OrdinalIgnoreCase)));
                if (colorAttr != null) product.Color = colorAttr.Value;
            }

            // Nhanh CreatedAt
            if (item.CreatedAt > 0)
            {
                product.NhanhCreatedAt = DateTimeOffset.FromUnixTimeSeconds(item.CreatedAt);
            }

            newProducts.Add(product);
        }

        if (newProducts.Any())
        {
            await _appDbContext.Products.AddRangeAsync(newProducts);
            await _appDbContext.SaveChangesAsync();
        }

        return $"Thêm mới thành công {newProducts.Count} sản phẩm từ Nhanh.vn.";
    }

    public async Task<string> WebhookNhanhVnProductUpdate(NhanhProductAddWebhookRequest request)
    {
        if (request == null || request.Data == null || !request.Data.Any())
        {
            throw new BadRequestException("Không có dữ liệu thay đổi khi cập nhật sản phẩm.");
        }

        var nhanhProductIds = request.Data.Select(x => x.Id.ToString()).Distinct().ToList();

        var existingProducts = await _appDbContext.Products
            .Where(x => !x.IsDeleted && nhanhProductIds.Contains(x.NhanhProductId!))
            .ToListAsync();

        var newProducts = new List<HoaTheoMua.Repository.Entity.Product>();
        var updatedCount = 0;

        foreach (var item in request.Data)
        {
            var product = existingProducts.FirstOrDefault(x => x.NhanhProductId == item.Id.ToString());
            var isNew = false;

            if (product == null)
            {
                product = new HoaTheoMua.Repository.Entity.Product
                {
                    Id = Guid.NewGuid(),
                    NhanhProductId = item.Id.ToString(),
                    CreatedAt = DateTimeOffset.UtcNow
                };
                isNew = true;
            }

            product.UpdatedAt = DateTimeOffset.UtcNow;
            product.ParentProductId = item.ParentId != 0 ? item.ParentId.ToString() : null;
            product.ProductCode = item.Code;
            product.Barcode = item.Barcode;
            product.ProductName = item.Name;
            product.OtherName = item.OtherName;
            product.Status = item.Status;

            // Danh mục
            product.CategoryCode = item.Category?.Code;
            product.CategoryName = item.Category?.Name;
            product.InternalCategoryCode = item.InternalCategory?.Code;
            product.ProductType = item.Type?.Name;

            // Giá
            if (item.Prices != null)
            {
                product.Price = (decimal)item.Prices.Retail;
                product.ImportPrice = (decimal)item.Prices.Import;
                product.OldPrice = (decimal)item.Prices.Old;
                product.WholesalePrice = (decimal)item.Prices.Wholesale;
                product.CostPrice = (decimal)item.Prices.AvgCost;
            }
            product.Vat = item.Vat;

            // Ảnh
            product.CoverImage = item.Images?.Avatar;

            // Bảo hành
            product.WarrantyMonths = item.Warranty?.Month;
            product.WarrantyAddress = item.Warranty?.Address;

            // Thương hiệu & nhà cung cấp
            product.Brand = item.Brand?.Name;
            product.Supplier = item.Suppliers?.Name;

            // Kích thước & cân nặng
            if (item.Shipping != null)
            {
                product.Length = (decimal)item.Shipping.Length;
                product.Width = (decimal)item.Shipping.Width;
                product.Height = (decimal)item.Shipping.Height;
                product.Weight = (decimal)item.Shipping.Weight;
            }

            // Xuất xứ & đơn vị tính
            product.Origin = item.CountryName;
            product.Unit = item.Units?.Name;

            // Tồn kho
            if (item.Inventory != null)
            {
                product.TotalStock = (decimal)item.Inventory.Remain;
                product.Stock = (decimal)item.Inventory.Remain;
                product.WarehouseStock = (decimal)item.Inventory.Remain;
                product.DeliveringStock = (decimal)item.Inventory.Shipping;
                product.StockError = (decimal)item.Inventory.Damaged;
                product.Held = (decimal)item.Inventory.Holding;
                product.StockTransferring = (decimal)item.Inventory.Transfering;
                product.AvailableForSale = (decimal)item.Inventory.Available;
                product.WarrantyStock = (decimal)item.Inventory.Warranty;
                product.HeldComponents = (decimal)item.Inventory.WarrantyHolding;
            }

            // Branch Prices (Lấy chi nhánh đầu tiên nếu có)
            if (item.BranchPrices != null && item.BranchPrices.Any())
            {
                var firstBranchPrice = item.BranchPrices.First();
                product.BranchPrice = (decimal)firstBranchPrice.Price;
                product.WholesaleBranchPrice = (decimal)firstBranchPrice.WholesalePrice;
            }

            // Attributes (Size, Color)
            if (item.Attributes != null && item.Attributes.Any())
            {
                var sizeAttr = item.Attributes
                    .FirstOrDefault(a => 
                        a.Name != null && (a.Name.Equals("Kích thước", StringComparison.OrdinalIgnoreCase) 
                                           || a.Name.Equals("Size", StringComparison.OrdinalIgnoreCase)));
                if (sizeAttr != null) product.Size = sizeAttr.Value;

                var colorAttr = item.Attributes
                    .FirstOrDefault(a => 
                        a.Name != null && (a.Name.Equals("Màu sắc", StringComparison.OrdinalIgnoreCase) 
                                           || a.Name.Equals("Color", StringComparison.OrdinalIgnoreCase)));
                if (colorAttr != null) product.Color = colorAttr.Value;
            }

            // Nhanh CreatedAt
            if (item.CreatedAt > 0)
            {
                product.NhanhCreatedAt = DateTimeOffset.FromUnixTimeSeconds(item.CreatedAt);
            }

            if (isNew)
            {
                newProducts.Add(product);
            }
            else
            {
                updatedCount++;
            }
        }

        if (newProducts.Any())
        {
            await _appDbContext.Products.AddRangeAsync(newProducts);
        }

        if (newProducts.Any() || updatedCount > 0)
        {
            await _appDbContext.SaveChangesAsync();
        }

        return $"Cập nhật thành công {updatedCount} sản phẩm, thêm mới {newProducts.Count} sản phẩm từ Nhanh.vn.";
    }

    public async Task<string> WebhookNhanhVnProductDelete(string productId)
    {
        throw new NotImplementedException();
    }
}