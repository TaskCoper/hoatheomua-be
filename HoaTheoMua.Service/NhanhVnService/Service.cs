using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.NhanhVnService;

public class Service: IService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _appDbContext;
    private readonly NhanhVnOption _nhanhVnOption;
    private readonly Revalidate.IService _revalidateService;

    public Service(HttpClient httpClient, AppDbContext appDbContext, 
        IConfiguration configuration, Revalidate.IService revalidateService)
    {
        _httpClient = httpClient;
        _appDbContext = appDbContext;
        _nhanhVnOption = new NhanhVnOption();
        configuration.GetSection(nameof(NhanhVnOption)).Bind(_nhanhVnOption);
        _revalidateService = revalidateService;
    }
    //
    public async Task<ProductLabelResponse> GetProductLabel(ProductLabelRequest request)  
    {  
        var configs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_appDbContext, "nhanhvn");  
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);  
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);  
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);  
        var nhanhApi = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl) + "/product/tags?appId=" + appId + "&businessId=" + businessId;    
      
      
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);  
        nhanhHttpRequest.Headers.Add("Authorization", token);  
        nhanhHttpRequest.Content = JsonContent.Create(request);  
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);  
  
        ProductLabelResponse? nhanhProductLable = null;  
        if (nhanhVnResponse.IsSuccessStatusCode)  
        {  
            //string rawJsonString = await nhanhVnResponse.Content.ReadAsStringAsync();  
  
            // Đặt đoạn mã này để bạn đặt Breakpoint hoặc dùng NLog/Serilog in ra Tab Console xem giá trị thô:        // Console.WriteLine($"[DEBUG NHANH.VN RAW DATA]: {rawJsonString}");  
  
            nhanhProductLable = await nhanhVnResponse.Content.ReadFromJsonAsync<ProductLabelResponse>();  
        }    else  
        {  
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();  
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");  
        }  
        if (nhanhProductLable == null)  
        {        throw new BadRequestException("NhanhVnResponse returned null or empty data");  
        }    if (nhanhProductLable.Code == 0)  
        {        throw new BadRequestException("ERR_INVALID_FORM_FIELDS");  
        }  
        return nhanhProductLable;  
    }

    private const string ProductLabelsConfigKey = "nhanhvn.product_labels";
    public async Task<string?> GetProductLabelByTagIds(List<int>? tagIds)
    {
        if (tagIds == null || tagIds.Count == 0)
        {
            return null;
        }

        var productLabelsConfigValue = await _appDbContext.Configs
            .AsNoTracking()
            .Where(x => !x.IsDeleted &&
                        x.Group == "nhanhvn" &&
                        x.Key == ProductLabelsConfigKey)
            .Select(x => x.Value)
            .FirstOrDefaultAsync();

        if (productLabelsConfigValue == null)
        {
            return null;
        }

        HoaTheoMua.Service.Config.Response.ConfigItem? productLabelsConfig;
        try
        {
            productLabelsConfig = JsonSerializer.Deserialize<HoaTheoMua.Service.Config.Response.ConfigItem>(
                productLabelsConfigValue);
        }
        catch (JsonException)
        {
            return null;
        }

        if (productLabelsConfig?.Options == null)
        {
            return null;
        }

        var labelsById = productLabelsConfig.Options
            .Where(x => x.Value.HasValue && !string.IsNullOrWhiteSpace(x.Label))
            .GroupBy(x => x.Value!.Value)
            .ToDictionary(x => x.Key, x => x.First().Label!.Trim());

        var labels = tagIds
            .Distinct()
            .Where(labelsById.ContainsKey)
            .Select(tagId => labelsById[tagId]);

        var productLabel = string.Join(", ", labels);
        return string.IsNullOrWhiteSpace(productLabel) ? null : productLabel;
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

    public async Task<string> HandleWebhook(JsonElement? body)
    {

        if (body == null)
        {
            return "Thành công";
        }

        var json = body.Value;
        

        // var businessIdWebhook = json.TryGetProperty("businessId", out var value) ? value.ToString() : null;
        //
        // if (businessIdWebhook == null)
        // {
        //     throw new BadRequestException("Không tìm thấy businessId");
        // }
        //
        // var configs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_appDbContext, "nhanhvn");
        // var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        // if (businessIdWebhook != businessId.ToString())
        // {
        //     throw new ConflictException("Business Id không khớp");
        // }
        
        try
        {
            var evt = json.TryGetProperty("event", out var e) ? e.GetString() : null;

            switch (evt)
            {
                case NhanhWebhookEvents.InventoryChange:
                {
                    var req = TryDeserialize<NhanhInventoryChangeWebhookRequest>(json);
                    if (req == null) return "Thành công";
                    return await WebhookNhanhVnInventory(req);
                }

                case NhanhWebhookEvents.ProductAdd:
                {
                    var req = TryDeserialize<NhanhProductAddWebhookRequest>(json);
                    if (req == null) return "Thành công";
                    JsonElement jsonData = JsonSerializer.SerializeToElement(new
                    {
                        entity = "productList"
                    });
                    await _revalidateService.RevalidateApi(jsonData);
                    return await WebhookNhanhVnProductAdd(req);
                }

                case NhanhWebhookEvents.ProductUpdate:
                {
                    var req = TryDeserialize<NhanhProductAddWebhookRequest>(json);
                    if (req == null) return "Thành công";
                    var result =  await WebhookNhanhVnProductUpdate(req);
                    JsonElement jsonData = JsonSerializer.SerializeToElement(new
                    {
                        entity = "product",
                        id = new[] { result }
                    });
                     await _revalidateService.RevalidateApi(jsonData);
                     return result;
                }

                case NhanhWebhookEvents.ProductDelete:
                {
                    var req = TryDeserialize<NhanhProductDeleteWebhookRequest>(json);
                    if (req == null) return "Thành công";
                    JsonElement jsonData = JsonSerializer.SerializeToElement(new
                    {
                        entity = "productList"
                    });
                    await _revalidateService.RevalidateApi(jsonData);
                    return await WebhookNhanhVnProductDelete(req);
                }

                case NhanhWebhookEvents.OrderAdd:
                {
                    var req = TryDeserialize<NhanhOrderAddWebhookRequest>(json);
                    if (req == null) return "Thành công";
                    return await WebhookNhanhVnOrderAdd(req);
                }
                
                case NhanhWebhookEvents.OrderUpdate:
                {
                    var req = TryDeserialize<NhanhOrderUpdateWebhookRequest>(json);
                    if (req == null) return "Thành công";
                    return await WebhookNhanhVnOrderUpdate(req);
                }
                
                case NhanhWebhookEvents.OrderDelete:
                {
                    var req = TryDeserialize<NhanhOrderDeleteWebhookRequest>(json);
                    if (req == null) return "Thành công";
                    return await WebhookNhanhVnOrderDelete(req);
                }

                default:
                    return "Thành công";
            }
        }
        catch (Exception ex)
        {
            return "Có lỗi khi parse json";
        }
    }
    private static T? TryDeserialize<T>(JsonElement body) where T : class
    {
        try
        {
            return body.Deserialize<T>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            });
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<string> WebhookNhanhVnInventory(NhanhInventoryChangeWebhookRequest? request)
    {
        if (request == null || request.Data == null || !request.Data.Any())
        {
            return "Thành công";
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

            if (product == null)
            {
               continue;
            }
            
            // Parent.NhanhProductId == Child.ParentProductId.
            var existParent = await _appDbContext.Products
                .FirstOrDefaultAsync(x => x.NhanhProductId == product.ParentProductId);
            //hỏi thử rằng là: product đó, nếu nó đã tồn tại rồi, thì hỏi thử có tồn tại 
            //parent của nó hay không
            if (existParent != null)
            {
                //update parent
                existParent.AvailableForSale += (decimal?)item.Available;
                existParent.TotalStock += (decimal?)item.Remain;//Tổng tồn = tổng số lượng tồn các kho
                existParent.Stock += (decimal?)item.Remain;
                existParent.DeliveringStock += (decimal?)item.Shipping;
                existParent.StockError += (decimal?)item.Damaged;
                existParent.Held += (decimal?)item.Holding;//tạm giữ các kho
                existParent.StockTransferring += (decimal?)item.Transfering;
                // product.HeldComponents = item.Holding;
            
                // product.WarehouseStock += (decimal?)item.Warranty.Remain;
                existParent.WarrantyStock += (decimal?)item.Warranty?.Remain;
                existParent.HeldComponents += (decimal?)item.Warranty?.Holding;
                existParent.UpdatedAt = DateTimeOffset.UtcNow;
            }
            
            
            product.AvailableForSale = (decimal?)item.Available;
            product.ProductCode = item.Code;
            product.TotalStock = (decimal?)item.Remain;//Tổng tồn = tổng số lượng tồn các kho
            product.Stock = (decimal?)item.Remain;
            product.DeliveringStock = (decimal?)item.Shipping;
            product.StockError = (decimal?)item.Damaged;
            product.Held = (decimal?)item.Holding;//tạm giữ các kho
            product.StockTransferring = (decimal?)item.Transfering;
            // product.HeldComponents = item.Holding;
            
            // product.WarehouseStock = (decimal?)item.Warranty.Remain;
            product.WarrantyStock = (decimal?)item.Warranty?.Remain;
            product.HeldComponents = (decimal?)item.Warranty?.Holding;
            product.UpdatedAt = DateTimeOffset.UtcNow;
            
        }

        await _appDbContext.SaveChangesAsync();

        return "Cập nhật tồn kho thành công";
    }

    public async Task<string> WebhookNhanhVnProductAdd(NhanhProductAddWebhookRequest? request)
    {
        if (request == null || request.Data == null)
        {
            return "Thành công";
        }

        var item = request.Data;
        var nhanhProductId = item.Id.ToString();

        var existingProduct = await _appDbContext.Products
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.NhanhProductId == nhanhProductId);

        if (existingProduct != null)
        {
           return "Có sản phẩm đã tồn tại trong hệ thống";
        }

        var product = new HoaTheoMua.Repository.Entity.Product
        {
            Id = Guid.NewGuid(),
            NhanhProductId = item.Id.ToString(),
            CreatedAt = DateTimeOffset.UtcNow.ToUniversalTime(),
            UpdatedAt = DateTimeOffset.UtcNow.ToUniversalTime()
        };

            product.ParentProductId = item.ParentId?.ToString();
            product.ProductCode = NormalizeText(item.Code);
            product.Barcode = NormalizeText(item.Barcode);
            product.ProductName = NormalizeText(item.Name);
            product.OtherName = NormalizeText(item.OtherName);
            product.Status = NormalizeText(item.Status?.ToString());
            product.ProductLabel = await GetProductLabelByTagIds(item.TagIds);

            // Danh mục — webhook chỉ gửi categoryId/brandId phẳng, không gửi object category/brand,
            // nên CategoryCode/CategoryName dưới đây luôn null với dữ liệu từ webhook.
            product.NhanhCategoryId = item.CategoryId;
            product.CategoryCode = NormalizeText(item.Category?.Code);
            product.CategoryName = NormalizeText(item.Category?.Name);
            product.InternalCategoryCode = NormalizeText(item.InternalCategory?.Code);
            product.NhanhTypeId = item.Type?.Id;
            product.ProductType = NormalizeText(item.Type?.Name);
            product.NhanhBrandId = item.BrandId;


            // Giá
            if (item.Prices != null)
            {
                product.Price = (decimal?)item.Prices.Retail;
                product.PriceWithVat = (decimal?)item.Prices.RetailVat;
                product.ImportPrice = (decimal?)item.Prices.Import;
                product.ImportPriceWithVat = (decimal?)item.Prices.ImportVat;
                product.OldPrice = (decimal?)item.Prices.Old;
                product.WholesalePrice = (decimal?)item.Prices.Wholesale;
                product.CostPrice = (decimal?)item.Prices.AvgCost;
            }
            product.Vat = item.Vat;
            product.SaleVatMode = item.PriceVatMode?.ToString();
            product.ImportVatMode = item.ImportPriceVatMode?.ToString();

            // Ảnh
            product.CoverImage = item.Images?.Avatar;
            product.OtherImages = item.Images?.Others;

            // Bảo hành
            NhanhWarrantyDto? warranty = null;
            if (item.Warranty.HasValue && item.Warranty.Value.ValueKind == JsonValueKind.Object)
            {
                warranty = item.Warranty.Value.Deserialize<NhanhWarrantyDto>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            product.WarrantyMonths = warranty?.Month;
            product.WarrantyAddress = warranty?.Address;

            // Thương hiệu & nhà cung cấp
            product.Brand = item.Brand?.Name;
            product.Supplier = item.Suppliers?.FirstOrDefault()?.Name;

            // Kích thước & cân nặng
            if (item.Shipping != null)
            {
                product.Length = (decimal?)item.Shipping.Length;
                product.Width = (decimal?)item.Shipping.Width;
                product.Height = (decimal?)item.Shipping.Height;
                product.Weight = (decimal?)item.Shipping.Weight;
            }

            // Xuất xứ & đơn vị tính
            product.Origin = item.CountryName;
            product.Unit = item.Units?.Name;

            // Tồn kho
            if (item.Inventory != null)
            {
                product.TotalStock = (decimal?)item.Inventory.Remain;//tổng tồn
                //product.Stock = (decimal)item.Inventory.Remain;// tồn
                product.WarehouseStock = (decimal?)item.Inventory.Remain;//tồn trong kho
                product.DeliveringStock = (decimal?)item.Inventory.Shipping;
                product.StockError = (decimal?)item.Inventory.Damaged;
                product.Held = (decimal?)item.Inventory.Holding;
                product.StockTransferring = (decimal?)item.Inventory.Transfering;
                product.AvailableForSale = (decimal?)item.Inventory.Available;
                product.WarrantyStock = (decimal?)item.Inventory.Warranty?.Remain;
                product.HeldComponents = (decimal?)item.Inventory.Warranty?.Holding;
            }

            // Branch Prices (Lấy chi nhánh đầu tiên nếu có)
            if (item.BranchPrices != null && item.BranchPrices.Any())
            {
                var firstBranchPrice = item.BranchPrices.First();
                product.BranchPrice = (decimal?)firstBranchPrice.Price;
                product.WholesaleBranchPrice = (decimal?)firstBranchPrice.WholesalePrice;
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
            if (item.CreatedAt is > 0)
            {
                product.NhanhCreatedAt = DateTimeOffset.FromUnixTimeSeconds(item.CreatedAt.Value);
            }
            
            if (item.UpdatedAt is > 0)
            {
                product.NhanhUpdatedAt = DateTimeOffset.FromUnixTimeSeconds(item.UpdatedAt.Value);
            }
            
            if (item.ParentId is > 0)
            {
                var parent = await _appDbContext.Products
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.NhanhProductId == item.ParentId.ToString());

                if (parent != null)
                {
                    parent.NhanhUpdatedAt = DateTimeOffset.UtcNow;
                }
            }
            

        await _appDbContext.Products.AddAsync(product);
        await _appDbContext.SaveChangesAsync();

        return "Thêm mới thành công sản phẩm";
    }

    public async Task<string> WebhookNhanhVnOrderAdd(NhanhOrderAddWebhookRequest? request)
    {
        if (request == null || request.Data == null)
        {
            return "Không có dữ liệu thay đổi đơn hàng";
        }
    
        var item = request.Data;
        if (item.Info == null)
        {
            return "Không có dữ liệu thay đổi đơn hàng";
        }

        var info = item.Info;
        var address = item.ShippingAddress;
        var carrier = item.Carrier;
        var payment = item.Payment;
    
        var idDonHangNhanh = info.Id.ToString();
    
        var existingOrder = await _appDbContext.OrderNhanhVns
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.IdDonHangNhanh == idDonHangNhanh);
    
        if (existingOrder != null)
        {
            return "Có đơn hàng đã tồn tại trong hệ thống.";
        }
    
        var products = item.Products ?? new List<NhanhOrderProductDto>();
    
        var orderNhanhVn = new HoaTheoMua.Repository.Entity.OrderNhanhVn
        {
            Id = Guid.NewGuid(),
            IdDonHangNhanh = idDonHangNhanh,
            LoaiDon = info.Type?.ToString(),
            ThoiGianDonHang = info.CreatedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(info.CreatedAt.Value) : null,
            NguoiTaoDon = info.CreatedBy?.Name,
            NhanVienBanHang = info.Sale?.Name,
            NhanVienChamSocKhachHang = info.CustomerCare?.Name != null && info.CustomerCare.Id != 0 ? info.CustomerCare.Name : null,
            Nguon = item.Channel?.TrafficSource?.Name,
            KhoTaoDon = info.DepotId?.ToString(),
    
            TenKhachHang = address?.Name,
            SoDienThoaiKhachHang = address?.Mobile,
            EmailKhachHang = address?.Email,
            DiaChiKhachHang = address?.Address,
            ThanhPhoKhachHang = address?.CityId?.ToString(),
            QuanHuyenKhachHang = address?.DistrictId?.ToString(),
            PhuongXaKhachHang = address?.WardId?.ToString(),
            
            TongKhoiLuong = products.Sum(p => (p.Weight ?? 0) * (p.Quantity ?? 0)),
            GiaTriDonHang = (products.Sum(p => (p.Price ?? 0) * (p.Quantity ?? 0))) + (payment?.Vat?.Amount ?? 0),
            PhiVanChuyen = (carrier?.ShipFee ?? 0),
            PhiShipBaoKhach = (carrier?.CustomerShipFee ?? 0),
            PhiThuTienHo = (carrier?.CodFee ?? 0),
            PhiKhaiGia = (carrier?.DeclaredFee ?? 0),
            PhiChuyenHoan = (carrier?.ReturnFee ?? 0),
    
            HangVanChuyen = carrier?.Name,
            MaVanDon = carrier?.CarrierCode,
            DichVuVanChuyen = carrier?.Service?.Name,
            CanNangHangVanChuyen = carrier?.Weight,
    
            TienChuyenKhoan = (payment?.Transfer?.Amount ?? 0),
            TaiKhoanChuyenKhoan = payment?.Transfer?.AccountId?.ToString(),
            TienDatCoc = (payment?.Deposit?.Amount ?? 0),
            TaiKhoanDatCoc = payment?.Deposit?.AccountId?.ToString(),
            TienChietKhau = payment?.Discount?.Type?.Equals("percent", StringComparison.OrdinalIgnoreCase) == true   // ① có phải loại % không?
                                ? (products.Sum(p => (p.Price ?? 0) * (p.Quantity ?? 0))                            // ② tổng tiền hàng
                                * (payment?.Discount?.Amount ?? 0) / 100)                                        //    × % / 100
                                : (payment?.Discount?.Amount ?? 0),                                                 // ③ loại cash: lấy thẳng số tiền
            TienVat = (payment?.Vat?.Amount ?? 0),
            TyLeVat = payment?.Vat?.Type?.Equals("percent", StringComparison.OrdinalIgnoreCase) == true ? payment?.Vat?.Value?.ToString() : null,
            TienQuetThe = (payment?.Credit?.Amount ?? 0),
            TaiKhoanQuetThe = payment?.Credit?.AccountId?.ToString(),
            MaGiaoDichQuetThe = payment?.Credit?.Code,
            TienTieuDiem = (payment?.UsedPoints?.Amount ?? 0),
            TrangThaiThanhToanDoanhNghiep = payment?.BusinessPaymentStatus?.ToString(),
    
            GhiChuCuaKhach = info.Description,
            GhiChuNoiBo = info.PrivateDescription,
            TrangThaiDonHang = info.Status?.ToString(),
    
            NgayXacNhan = info.ConfirmedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(info.ConfirmedAt.Value) : null,
            NgayGuiHangVanChuyen = !string.IsNullOrEmpty(carrier?.SendCarrier?.Date) && DateTimeOffset.TryParse(carrier.SendCarrier.Date, out var sendDate) ? sendDate.ToUniversalTime() : null,
            NgayThanhCong = !string.IsNullOrEmpty(carrier?.DeliveryDate) && DateTimeOffset.TryParse(carrier.DeliveryDate, out var deliveryDate) ? deliveryDate.ToUniversalTime() : null,
    
            MaGiamGia = payment?.CouponCode,
            IdDonGoc = item.OriginalOrder?.Id?.ToString(),
    
            CreatedAt = DateTimeOffset.UtcNow.ToUniversalTime(),
            UpdatedAt = DateTimeOffset.UtcNow.ToUniversalTime()
        };

        foreach (var p in products)
        {
            orderNhanhVn.OrderNhanhVnDetails.Add(new HoaTheoMua.Repository.Entity.OrderNhanhVnDetail
            {
                Id = Guid.NewGuid(),
                OrderNhanhVnId = orderNhanhVn.Id,
                TenSanPham = p.Name,
                MaSanPham = p.Code,
                SoLuong = (int?)p.Quantity,
                GiaBan = p.Price,
                TienVatSanPham = p.Vat?.Amount,
                ChietKhauSanPham = p.Discount,
                GiaSanPhamSauChietKhau = (p.Price ?? 0) - (p.Discount ?? 0),
                GhiChuSanPham = p.Description,
                DonViTinh = p.Unit?.Name,
                KhoiLuongSanPham = p.Weight,
                Imei = p.Imei?.Code,
                LoHang = p.Batch?.Name,
                GiaVon = p.AverageCost,
                CreatedAt = DateTimeOffset.UtcNow.ToUniversalTime(),
                UpdatedAt = DateTimeOffset.UtcNow.ToUniversalTime()
            });
        }

        await _appDbContext.OrderNhanhVns.AddAsync(orderNhanhVn);
        await _appDbContext.SaveChangesAsync();
    
        return "Thêm mới thành công 1 đơn hàng vào bảng OrderNhanhVn";
    }

    public async Task<string> WebhookNhanhVnOrderUpdate(NhanhOrderUpdateWebhookRequest request)
    {
        if (request == null || request.Data == null)
        {
            return "Không có dữ liệu cập nhật đơn hàng";
        }
    
        var item = request.Data;
        if (item.Info == null)
        {
            return "Không có dữ liệu cập nhật đơn hàng";
        }

        var info = item.Info;
        var address = item.ShippingAddress;
        var carrier = item.Carrier;
        var payment = item.Payment;
    
        var idDonHangNhanh = info.Id.ToString();
    
        var order = await _appDbContext.OrderNhanhVns
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.IdDonHangNhanh == idDonHangNhanh);
    
        if (order == null)
        {
            return "Đơn hàng không tồn tại trong hệ thống.";
        }
    
        var products = item.Products ?? new List<NhanhOrderProductDto>();
    
        order.LoaiDon = info.Type?.ToString() ?? order.LoaiDon;
        order.ThoiGianDonHang = info.CreatedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(info.CreatedAt.Value) : order.ThoiGianDonHang;
        order.NguoiTaoDon = info.CreatedBy?.Name ?? order.NguoiTaoDon;
        order.NhanVienBanHang = info.Sale?.Name ?? order.NhanVienBanHang;
        order.NhanVienChamSocKhachHang = info.CustomerCare?.Name != null && info.CustomerCare.Id != 0 ? info.CustomerCare.Name : order.NhanVienChamSocKhachHang;
        order.Nguon = item.Channel?.TrafficSource?.Name ?? order.Nguon;
        order.KhoTaoDon = info.DepotId?.ToString() ?? order.KhoTaoDon;
    
        if (address != null)
        {
            order.TenKhachHang = address.Name ?? order.TenKhachHang;
            order.SoDienThoaiKhachHang = address.Mobile ?? order.SoDienThoaiKhachHang;
            order.EmailKhachHang = address.Email ?? order.EmailKhachHang;
            order.DiaChiKhachHang = address.Address ?? order.DiaChiKhachHang;
            order.ThanhPhoKhachHang = address.CityId?.ToString() ?? order.ThanhPhoKhachHang;
            order.QuanHuyenKhachHang = address.DistrictId?.ToString() ?? order.QuanHuyenKhachHang;
            order.PhuongXaKhachHang = address.WardId?.ToString() ?? order.PhuongXaKhachHang;
        }
    
        if (products.Any())
        {
            order.TongKhoiLuong = products.Sum(p => (p.Weight ?? 0) * (p.Quantity ?? 0));
            order.GiaTriDonHang = (products.Sum(p => (p.Price ?? 0) * (p.Quantity ?? 0))) + (decimal?)(payment?.Vat?.Amount ?? 0);

            var existingDetails = await _appDbContext.OrderNhanhVnDetails
                .Where(x => x.OrderNhanhVnId == order.Id && !x.IsDeleted)
                .ToListAsync();

            foreach (var detail in existingDetails)
            {
                detail.IsDeleted = true;
                detail.UpdatedAt = DateTimeOffset.UtcNow.ToUniversalTime();
            }

            foreach (var p in products)
            {
                _appDbContext.OrderNhanhVnDetails.Add(new HoaTheoMua.Repository.Entity.OrderNhanhVnDetail
                {
                    Id = Guid.NewGuid(),
                    OrderNhanhVnId = order.Id,
                    TenSanPham = p.Name,
                    MaSanPham = p.Code,
                    SoLuong = (int?)p.Quantity,
                    GiaBan = p.Price,
                    TienVatSanPham = p.Vat?.Amount,
                    ChietKhauSanPham = p.Discount,
                    GiaSanPhamSauChietKhau = (p.Price ?? 0) - (p.Discount ?? 0),
                    GhiChuSanPham = p.Description,
                    DonViTinh = p.Unit?.Name,
                    KhoiLuongSanPham = p.Weight,
                    Imei = p.Imei?.Code,
                    LoHang = p.Batch?.Name,
                    GiaVon = p.AverageCost,
                    CreatedAt = DateTimeOffset.UtcNow.ToUniversalTime(),
                    UpdatedAt = DateTimeOffset.UtcNow.ToUniversalTime()
                });
            }
        }
    
        if (carrier != null)
        {
            order.PhiVanChuyen = carrier.ShipFee ?? order.PhiVanChuyen;
            order.PhiShipBaoKhach = carrier.CustomerShipFee ?? order.PhiShipBaoKhach;
            order.PhiThuTienHo = carrier.CodFee ?? order.PhiThuTienHo;
            order.PhiKhaiGia = carrier.DeclaredFee ?? order.PhiKhaiGia;
            order.PhiChuyenHoan = carrier.ReturnFee ?? order.PhiChuyenHoan;
            order.HangVanChuyen = carrier.Name ?? order.HangVanChuyen;
            order.MaVanDon = carrier.CarrierCode ?? order.MaVanDon;
            order.DichVuVanChuyen = carrier.Service?.Name ?? order.DichVuVanChuyen;
            order.CanNangHangVanChuyen = carrier.Weight ?? order.CanNangHangVanChuyen;
    
            if (!string.IsNullOrEmpty(carrier.SendCarrier?.Date) && DateTimeOffset.TryParse(carrier.SendCarrier.Date, out var sendDate))
                order.NgayGuiHangVanChuyen = sendDate.ToUniversalTime();
    
            if (!string.IsNullOrEmpty(carrier.DeliveryDate) && DateTimeOffset.TryParse(carrier.DeliveryDate, out var deliveryDate))
                order.NgayThanhCong = deliveryDate.ToUniversalTime();
        }
    
        if (payment != null)
        {
            order.TienChuyenKhoan = (payment.Transfer?.Amount ?? (long?)order.TienChuyenKhoan);
            order.TaiKhoanChuyenKhoan = payment.Transfer?.AccountId?.ToString() ?? order.TaiKhoanChuyenKhoan;
            order.TienDatCoc = (payment.Deposit?.Amount ?? (long?)order.TienDatCoc);
            order.TaiKhoanDatCoc = payment.Deposit?.AccountId?.ToString() ?? order.TaiKhoanDatCoc;
            order.TienChietKhau = payment?.Discount?.Type?.Equals("percent", StringComparison.OrdinalIgnoreCase) == true   // ① có phải loại % không?
                                ? (products.Sum(p => (p.Price ?? 0) * (p.Quantity ?? 0))                            // ② tổng tiền hàng
                                * (payment?.Discount?.Amount ?? 0) / 100)                                        //    × % / 100
                                : (payment?.Discount?.Amount ?? 0);
            order.TienVat = (payment.Vat?.Amount ?? (long?)order.TienVat);
            order.TyLeVat = payment.Vat?.Type?.Equals("percent", StringComparison.OrdinalIgnoreCase) == true ? payment.Vat.Value?.ToString() : order.TyLeVat;
            order.TienQuetThe = (payment.Credit?.Amount ?? order.TienQuetThe);
            order.TaiKhoanQuetThe = payment.Credit?.AccountId?.ToString() ?? order.TaiKhoanQuetThe;
            order.MaGiaoDichQuetThe = payment.Credit?.Code ?? order.MaGiaoDichQuetThe;
            order.TienTieuDiem = (payment.UsedPoints?.Amount ?? order.TienTieuDiem);
            order.TrangThaiThanhToanDoanhNghiep = payment.BusinessPaymentStatus?.ToString() ?? order.TrangThaiThanhToanDoanhNghiep;
            order.MaGiamGia = payment.CouponCode ?? order.MaGiamGia;
        }
    
        order.GhiChuCuaKhach = info.Description ?? order.GhiChuCuaKhach;
        order.GhiChuNoiBo = info.PrivateDescription ?? order.GhiChuNoiBo;
        order.TrangThaiDonHang = info.Status?.ToString() ?? order.TrangThaiDonHang;
        order.NgayXacNhan = info.ConfirmedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(info.ConfirmedAt.Value).ToUniversalTime() : order.NgayXacNhan;
    
        order.NguonDonHang = item.Channel?.SaleChannel?.ToString() ?? order.NguonDonHang;
        order.UpdatedAt = DateTimeOffset.UtcNow.ToUniversalTime();
    
        await _appDbContext.SaveChangesAsync();
    
        return "Cập nhật thành công đơn hàng trong bảng OrderNhanhVn";
    }
    
    public async Task<string> WebhookNhanhVnOrderDelete(NhanhOrderDeleteWebhookRequest request)
    {
        if (request?.Data == null || request.Data.Count == 0)
        {
            return "Không có dữ liệu xoá đơn hàng";
        }
    
        var validNhanhOrderIds = request.Data
            .Select(id => id.ToString())
            .ToHashSet();
    
        var orders = await _appDbContext.OrderNhanhVns
            .Where(order => !order.IsDeleted
                            && validNhanhOrderIds.Contains(order.IdDonHangNhanh!))
            .Include(x => x.OrderNhanhVnDetails)
            .ToListAsync();

        foreach (var order in orders)
        {
            order.IsDeleted = true;
            order.UpdatedAt = DateTimeOffset.UtcNow.ToUniversalTime();

            foreach (var detail in order.OrderNhanhVnDetails.Where(x => !x.IsDeleted))
            {
                detail.IsDeleted = true;
                detail.UpdatedAt = DateTimeOffset.UtcNow.ToUniversalTime();
            }
        }
    
        if (orders.Count > 0)
        {
            await _appDbContext.SaveChangesAsync();
        }
    
        var ignoredCount = request.Data.Count - orders.Count;
        return $"Xoá {orders.Count} đơn trong bảng OrderNhanhVn; bỏ qua {ignoredCount} đơn.";}

    public async Task<string> WebhookNhanhVnProductUpdate(NhanhProductAddWebhookRequest? request)
    {
        if (request == null || request.Data == null)
        {
            return "Thành công";        
        }
    
        var item = request.Data;
        var nhanhProductId = item.Id.ToString();
    
        var product = await _appDbContext.Products
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.NhanhProductId == nhanhProductId);
    
        if (product == null)
        {
            product = new Repository.Entity.Product()
            {
                Id = Guid.NewGuid(),
                NhanhProductId = item.Id.ToString(),
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _appDbContext.Products.AddAsync(product);
        }

        
        product.UpdatedAt = DateTimeOffset.UtcNow;
        product.ParentProductId = item.ParentId?.ToString();
        product.ProductCode = NormalizeText(item.Code);
        product.Barcode = NormalizeText(item.Barcode);
        product.ProductName = NormalizeText(item.Name);
        product.OtherName = NormalizeText(item.OtherName);
        product.Status = NormalizeText(item.Status?.ToString());
        if (item.TagIds != null)
        {
            product.ProductLabel = await GetProductLabelByTagIds(item.TagIds);
        }

        
        // Danh mục
        product.NhanhCategoryId = item.CategoryId;
        product.CategoryCode = NormalizeText(item.Category?.Code);
        product.CategoryName = NormalizeText(item.Category?.Name);
        product.InternalCategoryCode = NormalizeText(item.InternalCategory?.Code);
        product.NhanhTypeId = item.Type?.Id;
        product.ProductType = NormalizeText(item.Type?.Name);
        product.NhanhBrandId = item.BrandId;
        // Giá
        if (item.Prices != null)
        {
            product.Price = (decimal?)item.Prices.Retail;
            product.PriceWithVat = (decimal?)item.Prices.RetailVat;
            product.ImportPrice = (decimal?)item.Prices.Import;
            product.ImportPriceWithVat = (decimal?)item.Prices.ImportVat;
            product.OldPrice = (decimal?)item.Prices.Old;
            product.WholesalePrice = (decimal?)item.Prices.Wholesale;
            product.CostPrice = (decimal?)item.Prices.AvgCost;
        }
        product.Vat = item.Vat;
        product.SaleVatMode = item.PriceVatMode?.ToString();
        product.ImportVatMode = item.ImportPriceVatMode?.ToString();
    
        // Ảnh
        product.CoverImage = item.Images?.Avatar;
        product.OtherImages = item.Images?.Others;
        
        // Bảo hành
        NhanhWarrantyDto? warranty = null;
        if (item.Warranty.HasValue && item.Warranty.Value.ValueKind == JsonValueKind.Object)
        {
            warranty = item.Warranty.Value.Deserialize<NhanhWarrantyDto>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        product.WarrantyMonths = warranty?.Month;
        product.WarrantyAddress = warranty?.Address;
    
        // Thương hiệu & nhà cung cấp
        product.Brand = item.Brand?.Name;
        product.Supplier = item.Suppliers?.FirstOrDefault()?.Name;
    
        // Kích thước & cân nặng
        if (item.Shipping != null)
        {
            product.Length = (decimal?)item.Shipping.Length;
            product.Width = (decimal?)item.Shipping.Width;
            product.Height = (decimal?)item.Shipping.Height;
            product.Weight = (decimal?)item.Shipping.Weight;
        }
    
        // Xuất xứ & đơn vị tính
        product.Origin = item.CountryName;
        product.Unit = item.Units?.Name;
    
        // Tồn kho
        if (item.Inventory != null)
        {
            product.TotalStock = (decimal?)item.Inventory.Remain;//tổng tồn
            //product.Stock = (decimal)item.Inventory.Remain;// tồn
            product.WarehouseStock = (decimal?)item.Inventory.Remain;//tồn trong kho
            product.DeliveringStock = (decimal?)item.Inventory.Shipping;
            product.StockError = (decimal?)item.Inventory.Damaged;
            product.Held = (decimal?)item.Inventory.Holding;
            product.StockTransferring = (decimal?)item.Inventory.Transfering;
            product.AvailableForSale = (decimal?)item.Inventory.Available;
            product.WarrantyStock = (decimal?)item.Inventory.Warranty?.Remain;
            product.HeldComponents = (decimal?)item.Inventory.Warranty?.Holding;
        }
    
        // Branch Prices (Lấy chi nhánh đầu tiên nếu có)
        if (item.BranchPrices != null && item.BranchPrices.Any())
        {
            var firstBranchPrice = item.BranchPrices.First();
            product.BranchPrice = (decimal?)firstBranchPrice.Price;
            product.WholesaleBranchPrice = (decimal?)firstBranchPrice.WholesalePrice;
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
        if (item.CreatedAt is > 0)
        {
            product.NhanhCreatedAt = DateTimeOffset.FromUnixTimeSeconds(item.CreatedAt.Value);
        }
        
        if (item.UpdatedAt is > 0)
        {
            product.NhanhUpdatedAt = DateTimeOffset.FromUnixTimeSeconds(item.UpdatedAt.Value);
        }
        
        if (item.ParentId is > 0)
        {
            var parent = await _appDbContext.Products
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.NhanhProductId == item.ParentId.ToString());

            if (parent != null)
            {
                parent.NhanhUpdatedAt = DateTimeOffset.UtcNow;
            }
        }
        
        await _appDbContext.SaveChangesAsync();
    
        return product.Id.ToString();
    }

    public async Task<string> WebhookNhanhVnProductDelete(NhanhProductDeleteWebhookRequest? request)
    {
        if (request == null || request.Data == null || !request.Data.Any())
        {
            return "Thành công";
        }

        var deletedIds = request.Data.Select(id => id.ToString()).ToList();

        var productsToDelete = await _appDbContext.Products
            .Where(x => !x.IsDeleted && deletedIds.Contains(x.NhanhProductId!))
            .ToListAsync();
        
        var parentIds = productsToDelete
            .Select(x => x.ParentProductId)
            .Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("-"))
            .Distinct()
            .ToList();
        
        var parents = new List<Repository.Entity.Product>();
        
        if (parentIds.Count > 0)
        {
            parents = await _appDbContext.Products
                .Where(x =>
                    !x.IsDeleted &&
                    parentIds.Contains(x.NhanhProductId!))
                .ToListAsync();
        }
        
        foreach (var p in productsToDelete)
        {
            p.IsDeleted = true;
            p.UpdatedAt = DateTimeOffset.UtcNow;
            p.NhanhUpdatedAt =  DateTimeOffset.UtcNow;
        }
        
        foreach (var parent in parents)
        {
            parent.NhanhUpdatedAt = DateTimeOffset.UtcNow;
        }

        await _appDbContext.SaveChangesAsync();

        return $"Xóa thành công {productsToDelete.Count} sản phẩm từ Nhanh.vn.";
    }

    public async Task<OrderNhanhVnDto> GetOrderNhanhVnById(Guid id)
    {
        var orderQuery = _appDbContext.OrderNhanhVns
            .Where(x => !x.IsDeleted && x.Id == id)
            .Select(MapOrderNhanhVnToDto);

        var order = await orderQuery.FirstOrDefaultAsync();

        if (order == null)
        {
            throw new NotFoundException($"OrderNhanhVn with ID {id} not found.");
        }
        
        return order;
    }

    public async Task<BasePaginationResponse> GetOrderNhanhVnPagination(int pageIndex, int pageSize, string? search)
    {
        if (pageIndex <= 0)
        {
            pageIndex = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 10;
        }
        
        var orderQuery = _appDbContext.OrderNhanhVns
           .Where(x => !x.IsDeleted);


        if (!string.IsNullOrEmpty(search))
        {
            orderQuery = orderQuery.Where(x => x.IdDonHangNhanh.Contains(search));
        }
        
        
        var totalCount = await orderQuery.CountAsync();
        
        orderQuery = orderQuery
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize);

        var orders = await orderQuery
            .Select(MapOrderNhanhVnToDto)
            .ToListAsync();
        
        var response = ApiResponseFactory.BasePagination(orders, pageIndex, pageSize, totalCount);

        return response;
    }

    private static readonly Expression<Func<OrderNhanhVn, OrderNhanhVnDto>> MapOrderNhanhVnToDto =
        order => new OrderNhanhVnDto()
        {
            Id = order.Id,
            IdDonHangNhanh = order.IdDonHangNhanh,
            IdDonHangApi = order.IdDonHangApi,
            LoaiDon = order.LoaiDon,
            ThoiGianDonHang = order.ThoiGianDonHang,
            NguoiTaoDon = order.NguoiTaoDon,
            CuaHang = order.CuaHang,
            Nguon = order.Nguon,
            KhoTaoDon = order.KhoTaoDon,
            ThanhPhoKhoHang = order.ThanhPhoKhoHang,
            QuanHuyenKhoHang = order.QuanHuyenKhoHang,
            KhoLayHang = order.KhoLayHang,
            ThanhPhoLayHang = order.ThanhPhoLayHang,
            QuanHuyenLayHang = order.QuanHuyenLayHang,
            TenKhachHang = order.TenKhachHang,
            MaKhachHang = order.MaKhachHang,
            SoDienThoaiKhachHang = order.SoDienThoaiKhachHang,
            EmailKhachHang = order.EmailKhachHang,
            CapDoKhachHang = order.CapDoKhachHang,
            NgaySinhKhachHang = order.NgaySinhKhachHang,
            ThanhPhoKhachHang = order.ThanhPhoKhachHang,
            QuanHuyenKhachHang = order.QuanHuyenKhachHang,
            PhuongXaKhachHang = order.PhuongXaKhachHang,
            DiaChiKhachHang = order.DiaChiKhachHang,
            TongKhoiLuong = order.TongKhoiLuong,
            GiaTriDonHang = order.GiaTriDonHang,
            PhiVanChuyen = order.PhiVanChuyen,
            PhiVuotCan = order.PhiVuotCan,
            PhiKhaiGia = order.PhiKhaiGia,
            PhiChuyenHoan = order.PhiChuyenHoan,
            PhiThuTienHo = order.PhiThuTienHo,
            PhiShipBaoKhach = order.PhiShipBaoKhach,
            CongVanChuyen = order.CongVanChuyen,
            HangVanChuyen = order.HangVanChuyen,
            DichVuVanChuyen = order.DichVuVanChuyen,
            SoNgayChuyenHang = order.SoNgayChuyenHang,
            MaVanDon = order.MaVanDon,
            BienBan = order.BienBan,
            TienChuyenKhoan = order.TienChuyenKhoan,
            TaiKhoanChuyenKhoan = order.TaiKhoanChuyenKhoan,
            TienChietKhau = order.TienChietKhau,
            TienDatCoc = order.TienDatCoc,
            TaiKhoanDatCoc = order.TaiKhoanDatCoc,
            TongThu = order.TongThu,
            TaiKhoanQuetThe = order.TaiKhoanQuetThe,
            TienQuetThe = order.TienQuetThe,
            MaGiaoDichQuetThe = order.MaGiaoDichQuetThe,
            GhiChuNoiBo = order.GhiChuNoiBo,
            GhiChuCuaKhach = order.GhiChuCuaKhach,
            TrangThaiHoaDonDienTu = order.TrangThaiHoaDonDienTu,
            SoHoaDonDienTu = order.SoHoaDonDienTu,
            NgayXuatHoaDonDienTu = order.NgayXuatHoaDonDienTu,
            MaCoQuanThueHoaDonDienTu = order.MaCoQuanThueHoaDonDienTu,
            TrangThaiDonHang = order.TrangThaiDonHang,
            LyDoHuy = order.LyDoHuy,
            NhanVienChamSocKhachHang = order.NhanVienChamSocKhachHang,
            NgayXacNhan = order.NgayXacNhan,
            NgayGuiHangVanChuyen = order.NgayGuiHangVanChuyen,
            NgayBanGiao = order.NgayBanGiao,
            NgayThanhCong = order.NgayThanhCong,
            GioThanhCong = order.GioThanhCong,
            NguonDonHang = order.NguonDonHang,
            GhiChuHangVanChuyen = order.GhiChuHangVanChuyen,
            CanNangHangVanChuyen = order.CanNangHangVanChuyen,
            MaGiamGia = order.MaGiamGia,
            IdDonTra = order.IdDonTra,
            IdDonGoc = order.IdDonGoc,
            SoThuTuTheoNgay = order.SoThuTuTheoNgay,
            BienBanBanGiao = order.BienBanBanGiao,
            MaAffiliate = order.MaAffiliate,
            NhomKhachHang = order.NhomKhachHang,
            TienVat = order.TienVat,
            TyLeVat = order.TyLeVat,
            TienTieuDiem = order.TienTieuDiem,
            Nhan = order.Nhan,
            NgayHenThanhToan = order.NgayHenThanhToan,
            TrangThaiDoiSoat = order.TrangThaiDoiSoat,
            NgayDoiSoat = order.NgayDoiSoat,
            TrangThaiThanhToanDoanhNghiep = order.TrangThaiThanhToanDoanhNghiep,
            NgayThanhToanDoanhNghiep = order.NgayThanhToanDoanhNghiep,
            NhanVienBanHang = order.NhanVienBanHang,
            LoiNhuan = order.LoiNhuan,
            TyLeLoiNhuan = order.TyLeLoiNhuan,
            FacebookPsid = order.FacebookPsid,
            FacebookPageUserId = order.FacebookPageUserId,
            SoDienThoaiFacebook = order.SoDienThoaiFacebook,
            GiaTriFacebook = order.GiaTriFacebook,
            IdPhapNhan = order.IdPhapNhan,
            MaSoThue = order.MaSoThue,
            EmailNguoiLienHe = order.EmailNguoiLienHe,
            EmailNguoiLienHeThuHai = order.EmailNguoiLienHeThuHai,
            SoDienThoaiNguoiLienHe = order.SoDienThoaiNguoiLienHe,
            TenNguoiLienHe = order.TenNguoiLienHe,
            MaThamChieu = order.MaThamChieu,
            DiaChiKhachHangTrenHoaDonDienTu = order.DiaChiKhachHangTrenHoaDonDienTu,
            TenKhachHangTrenHoaDonDienTu = order.TenKhachHangTrenHoaDonDienTu,
            LoaiKhachHangHoaDon = order.LoaiKhachHangHoaDon,
            CoLayHoaDon = order.CoLayHoaDon,
            MaCoQuanThueHoaDon = order.MaCoQuanThueHoaDon,
            SoHoaDon = order.SoHoaDon,
            NgayPhatHanhHoaDon = order.NgayPhatHanhHoaDon,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            
            OrderNhanhVnDetails = order.OrderNhanhVnDetails
                .Where(detail => !detail.IsDeleted)
                .Select(detail => new OrderNhanhVnDetailDto
                {
                    Id = detail.Id,
                    OrderNhanhVnId = detail.OrderNhanhVnId,
                    TenSanPham = detail.TenSanPham,
                    QuaTangKem = detail.QuaTangKem,
                    Imei = detail.Imei,
                    LoHang = detail.LoHang,
                    MaSanPhamCha = detail.MaSanPhamCha,
                    TenSanPhamCha = detail.TenSanPhamCha,
                    MaSanPham = detail.MaSanPham,
                    MaVach = detail.MaVach,
                    DanhMucSanPham = detail.DanhMucSanPham,
                    KhoiLuongSanPham = detail.KhoiLuongSanPham,
                    GiaBan = detail.GiaBan,
                    LoaiGiaSanPham = detail.LoaiGiaSanPham,
                    GiaBanTruocVat = detail.GiaBanTruocVat,
                    TienVatSanPham = detail.TienVatSanPham,
                    SoLuong = detail.SoLuong,
                    ChietKhauMotSanPham = detail.ChietKhauMotSanPham,
                    ChietKhauSanPham = detail.ChietKhauSanPham,
                    GiaSanPhamSauChietKhau = detail.GiaSanPhamSauChietKhau,
                    GhiChuSanPham = detail.GhiChuSanPham,
                    GiaVon = detail.GiaVon,
                    GiaVonDonHang = detail.GiaVonDonHang,
                    DonViTinh = detail.DonViTinh,
                    SoLuongDonViNhoNhat = detail.SoLuongDonViNhoNhat,
                    NhanVienDongGoi = detail.NhanVienDongGoi,
                    CreatedAt = detail.CreatedAt,
                    UpdatedAt = detail.UpdatedAt
                }).ToList()
        };

    private static string? NormalizeText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var cleaned = text.Replace('\u00A0', ' ').Trim().Normalize(NormalizationForm.FormC);
        return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
    }
}
