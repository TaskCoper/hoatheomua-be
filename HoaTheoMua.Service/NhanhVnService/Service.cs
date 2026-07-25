using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
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
                    return await WebhookNhanhVnProductAdd(req);
                }

                case NhanhWebhookEvents.ProductUpdate:
                {
                    var req = TryDeserialize<NhanhProductAddWebhookRequest>(json);
                    if (req == null) return "Thành công";
                    return await WebhookNhanhVnProductUpdate(req);
                }

                case NhanhWebhookEvents.ProductDelete:
                {
                    var req = TryDeserialize<NhanhProductDeleteWebhookRequest>(json);
                    if (req == null) return "Thành công";
                    return await WebhookNhanhVnProductDelete(req);
                }

                // case NhanhWebhookEvents.OrderAdd:
                // {
                //     var req = TryDeserialize<NhanhOrderAddWebhookRequest>(json);
                //     if (req == null) return "Thành công";
                //     return await WebhookNhanhVnOrderAdd(req);
                // }
                //
                // case NhanhWebhookEvents.OrderUpdate:
                // {
                //     var req = TryDeserialize<NhanhOrderUpdateWebhookRequest>(json);
                //     if (req == null) return "Thành công";
                //     return await WebhookNhanhVnOrderUpdate(req);
                // }
                //
                // case NhanhWebhookEvents.OrderDelete:
                // {
                //     var req = TryDeserialize<NhanhOrderDeleteWebhookRequest>(json);
                //     if (req == null) return "Thành công";
                //     return await WebhookNhanhVnOrderDelete(req);
                // }

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
               return $"Sản phẩm với id: {itemId} không tồn tại trong hệ thống";
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
            
            product.ParentProductId = item.ParentId != 0 ? item.ParentId.ToString() : null;
            product.ProductCode = item.Code;
            product.Barcode = item.Barcode;
            product.ProductName = item.Name;
            product.OtherName = item.OtherName;
            product.Status = item.Status?.ToString();

            // Danh mục
            product.CategoryCode = item.Category?.Code;
            product.CategoryName = item.Category?.Name;
            product.InternalCategoryCode = item.InternalCategory?.Code;
            product.ProductType = item.Type?.Name;

            // Giá
            if (item.Prices != null)
            {
                product.Price = (decimal?)item.Prices.Retail;
                product.ImportPrice = (decimal?)item.Prices.Import;
                product.OldPrice = (decimal?)item.Prices.Old;
                product.WholesalePrice = (decimal?)item.Prices.Wholesale;
                product.CostPrice = (decimal?)item.Prices.AvgCost;
            }
            product.Vat = item.Vat;

            // Ảnh
            product.CoverImage = item.Images?.Avatar;

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

        await _appDbContext.Products.AddAsync(product);
        await _appDbContext.SaveChangesAsync();

        return "Thêm mới thành công sản phẩm";
    }

    // public async Task<string> WebhookNhanhVnOrderAdd(NhanhOrderAddWebhookRequest? request)
    // {
    //     if (request == null || request.Data == null)
    //     {
    //         return "Không có dữ liệu thay đổi đơn hàng";
    //     }
    //
    //     var item = request.Data;
    //     var info = item.Info;
    //     var address = item.ShippingAddress;
    //     var carrier = item.Carrier; 
    //     var payment = item.Payment;
    //
    //     var idDonHangNhanh = info.Id.ToString();
    //
    //     var existingOrder = await _appDbContext.OrderNhanhVns
    //         .FirstOrDefaultAsync(x => !x.IsDeleted && x.IdDonHangNhanh == idDonHangNhanh);
    //
    //     if (existingOrder != null)
    //     {
    //         return "Có đơn hàng đã tồn tại trong hệ thống.";
    //     }
    //
    //     var products = item.Products ?? new List<NhanhOrderProductDto>();
    //     var firstProduct = products.FirstOrDefault();
    //
    //     var orderNhanhVn = new HoaTheoMua.Repository.Entity.OrderNhanhVn
    //     {
    //         Id = Guid.NewGuid(),
    //         IdDonHangNhanh = idDonHangNhanh,
    //         LoaiDon = info.Type?.ToString(),
    //         ThoiGianDonHang = info.CreatedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(info.CreatedAt.Value) : null,
    //         NguoiTaoDon = info.CreatedBy?.Name,
    //         NhanVienBanHang = info.Sale?.Name,
    //         NhanVienChamSocKhachHang = info.CustomerCare?.Name != null && info.CustomerCare.Id != 0 ? info.CustomerCare.Name : null,
    //         Nguon = item.Channel?.TrafficSource?.Name,
    //         KhoTaoDon = info.DepotId?.ToString(),
    //
    //         TenKhachHang = address?.Name,
    //         SoDienThoaiKhachHang = address?.Mobile,
    //         EmailKhachHang = address?.Email,
    //         DiaChiKhachHang = address?.Address,
    //         ThanhPhoKhachHang = address?.CityId?.ToString(),
    //         QuanHuyenKhachHang = address?.DistrictId?.ToString(),
    //         PhuongXaKhachHang = address?.WardId?.ToString(),
    //
    //         TenSanPham = string.Join(", ", products.Select(p => p.Name)),
    //         MaSanPham = string.Join(", ", products.Select(p => p.Code)),
    //         SoLuong = (int?)products.Sum(p => p.Quantity ?? 0),
    //         GiaBan = (decimal?)(firstProduct?.Price ?? 0),
    //         TienVatSanPham = (decimal?)products.Sum(p => p.Vat?.Amount ?? 0),
    //         ChietKhauMotSanPham = (decimal?)(firstProduct?.Discount ?? 0),
    //         ChietKhauSanPham = (decimal?)products.Sum(p => p.Discount ?? 0),
    //         GiaSanPhamSauChietKhau = (decimal?)products.Sum(p => (p.Price ?? 0) - (p.Discount ?? 0)),
    //         GhiChuSanPham = string.Join("; ", products.Select(p => p.Description).Where(d => !string.IsNullOrEmpty(d))),
    //         DonViTinh = string.Join(", ", products.Select(p => p.Unit?.Name).Where(u => !string.IsNullOrEmpty(u)).Distinct()),
    //         KhoiLuongSanPham = (decimal?)(firstProduct?.Weight ?? 0),
    //
    //         TongKhoiLuong = (decimal?)products.Sum(p => (p.Weight ?? 0) * (p.Quantity ?? 0)),
    //         GiaTriDonHang = (decimal?)(products.Sum(p => (p.Price ?? 0) * (p.Quantity ?? 0))) + (decimal?)(payment?.Vat?.Amount ?? 0),
    //         PhiVanChuyen = (decimal?)(carrier?.ShipFee ?? 0),
    //         PhiShipBaoKhach = (decimal?)(carrier?.CustomerShipFee ?? 0),
    //         PhiThuTienHo = (decimal?)(carrier?.CodFee ?? 0),
    //         PhiKhaiGia = (decimal?)(carrier?.DeclaredFee ?? 0),
    //         PhiChuyenHoan = (decimal?)(carrier?.ReturnFee ?? 0),
    //
    //         HangVanChuyen = carrier?.Name,
    //         MaVanDon = carrier?.CarrierCode,
    //         DichVuVanChuyen = carrier?.Service?.Name,
    //         CanNangHangVanChuyen = carrier?.Weight,
    //
    //         TienChuyenKhoan = (decimal?)(payment?.Transfer?.Amount ?? 0),
    //         TaiKhoanChuyenKhoan = payment?.Transfer?.AccountId?.ToString(),
    //         TienDatCoc = (decimal?)(payment?.Deposit?.Amount ?? 0),
    //         TaiKhoanDatCoc = payment?.Deposit?.AccountId?.ToString(),
    //         TienChietKhau = (decimal?)(payment?.Discount?.Amount ?? 0),
    //         TienVat = (decimal?)(payment?.Vat?.Amount ?? 0),
    //         TyLeVat = payment?.Vat?.Type?.Equals("percent", StringComparison.OrdinalIgnoreCase) == true ? payment?.Vat?.Value?.ToString() : null,
    //         TienQuetThe = (decimal?)(payment?.Credit?.Amount ?? 0),
    //         TaiKhoanQuetThe = payment?.Credit?.AccountId?.ToString(),
    //         MaGiaoDichQuetThe = payment?.Credit?.Code,
    //         TienTieuDiem = (decimal?)(payment?.UsedPoints?.Amount ?? 0),
    //         TrangThaiThanhToanDoanhNghiep = payment?.BusinessPaymentStatus?.ToString(),
    //
    //         GhiChuCuaKhach = info.Description,
    //         GhiChuNoiBo = info.PrivateDescription,
    //         TrangThaiDonHang = info.Status?.ToString(),
    //
    //         NgayXacNhan = info.ConfirmedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(info.ConfirmedAt.Value) : null,
    //         NgayGuiHangVanChuyen = !string.IsNullOrEmpty(carrier?.SendCarrier?.Date) && DateTimeOffset.TryParse(carrier.SendCarrier.Date, out var sendDate) ? sendDate.ToUniversalTime() : null,
    //         NgayThanhCong = !string.IsNullOrEmpty(carrier?.DeliveryDate) && DateTimeOffset.TryParse(carrier.DeliveryDate, out var deliveryDate) ? deliveryDate.ToUniversalTime() : null,
    //
    //         MaGiamGia = payment?.CouponCode,
    //         IdDonGoc = item.OriginalOrder?.Id?.ToString(),
    //
    //         CreatedAt = DateTimeOffset.UtcNow.ToUniversalTime(),
    //         UpdatedAt = DateTimeOffset.UtcNow.ToUniversalTime()
    //     };
    //
    //     await _appDbContext.OrderNhanhVns.AddAsync(orderNhanhVn);
    //     await _appDbContext.SaveChangesAsync();
    //
    //     return "Thêm mới thành công 1 đơn hàng vào bảng OrderNhanhVn";
    // }

    // public async Task<string> WebhookNhanhVnOrderUpdate(NhanhOrderUpdateWebhookRequest request)
    // {
    //     if (request == null || request.Data == null)
    //     {
    //         return "Không có dữ liệu cập nhật đơn hàng";
    //     }
    //
    //     var item = request.Data;
    //     var info = item.Info;
    //     var address = item.ShippingAddress;
    //     var carrier = item.Carrier;
    //     var payment = item.Payment;
    //
    //     var idDonHangNhanh = info.Id.ToString();
    //
    //     var order = await _appDbContext.OrderNhanhVns
    //         .FirstOrDefaultAsync(x => !x.IsDeleted && x.IdDonHangNhanh == idDonHangNhanh);
    //
    //     if (order == null)
    //     {
    //         return "Đơn hàng không tồn tại trong hệ thống.";
    //     }
    //
    //     var products = item.Products ?? new List<NhanhOrderProductDto>();
    //     var firstProduct = products.FirstOrDefault();
    //
    //     order.LoaiDon = info.Type?.ToString() ?? order.LoaiDon;
    //     order.ThoiGianDonHang = info.CreatedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(info.CreatedAt.Value) : order.ThoiGianDonHang;
    //     order.NguoiTaoDon = info.CreatedBy?.Name ?? order.NguoiTaoDon;
    //     order.NhanVienBanHang = info.Sale?.Name ?? order.NhanVienBanHang;
    //     order.NhanVienChamSocKhachHang = info.CustomerCare?.Name != null && info.CustomerCare.Id != 0 ? info.CustomerCare.Name : order.NhanVienChamSocKhachHang;
    //     order.Nguon = item.Channel?.TrafficSource?.Name ?? order.Nguon;
    //     order.KhoTaoDon = info.DepotId?.ToString() ?? order.KhoTaoDon;
    //
    //     if (address != null)
    //     {
    //         order.TenKhachHang = address.Name ?? order.TenKhachHang;
    //         order.SoDienThoaiKhachHang = address.Mobile ?? order.SoDienThoaiKhachHang;
    //         order.EmailKhachHang = address.Email ?? order.EmailKhachHang;
    //         order.DiaChiKhachHang = address.Address ?? order.DiaChiKhachHang;
    //         order.ThanhPhoKhachHang = address.CityId?.ToString() ?? order.ThanhPhoKhachHang;
    //         order.QuanHuyenKhachHang = address.DistrictId?.ToString() ?? order.QuanHuyenKhachHang;
    //         order.PhuongXaKhachHang = address.WardId?.ToString() ?? order.PhuongXaKhachHang;
    //     }
    //
    //     if (products.Any())
    //     {
    //         order.TenSanPham = string.Join(", ", products.Select(p => p.Name));
    //         order.MaSanPham = string.Join(", ", products.Select(p => p.Code));
    //         order.SoLuong = (int?)products.Sum(p => p.Quantity ?? 0);
    //         order.GiaBan = (decimal?)(firstProduct?.Price ?? 0);
    //         order.TienVatSanPham = (decimal?)products.Sum(p => p.Vat?.Amount ?? 0);
    //         order.ChietKhauMotSanPham = (decimal?)(firstProduct?.Discount ?? 0);
    //         order.ChietKhauSanPham = (decimal?)products.Sum(p => p.Discount ?? 0);
    //         order.GiaSanPhamSauChietKhau = (decimal?)products.Sum(p => (p.Price ?? 0) - (p.Discount ?? 0));
    //         order.GhiChuSanPham = string.Join("; ", products.Select(p => p.Description).Where(d => !string.IsNullOrEmpty(d)));
    //         order.DonViTinh = string.Join(", ", products.Select(p => p.Unit?.Name).Where(u => !string.IsNullOrEmpty(u)).Distinct());
    //         order.KhoiLuongSanPham = (decimal?)(firstProduct?.Weight ?? 0);
    //         order.TongKhoiLuong = (decimal?)products.Sum(p => (p.Weight ?? 0) * (p.Quantity ?? 0));
    //         order.GiaTriDonHang = (decimal?)(products.Sum(p => (p.Price ?? 0) * (p.Quantity ?? 0))) + (decimal?)(payment?.Vat?.Amount ?? 0);
    //     }
    //
    //     if (carrier != null)
    //     {
    //         order.PhiVanChuyen = carrier.ShipFee ?? order.PhiVanChuyen;
    //         order.PhiShipBaoKhach = carrier.CustomerShipFee ?? order.PhiShipBaoKhach;
    //         order.PhiThuTienHo = carrier.CodFee ?? order.PhiThuTienHo;
    //         order.PhiKhaiGia = carrier.DeclaredFee ?? order.PhiKhaiGia;
    //         order.PhiChuyenHoan = carrier.ReturnFee ?? order.PhiChuyenHoan;
    //         order.HangVanChuyen = carrier.Name ?? order.HangVanChuyen;
    //         order.MaVanDon = carrier.CarrierCode ?? order.MaVanDon;
    //         order.DichVuVanChuyen = carrier.Service?.Name ?? order.DichVuVanChuyen;
    //         order.CanNangHangVanChuyen = carrier.Weight ?? order.CanNangHangVanChuyen;
    //
    //         if (!string.IsNullOrEmpty(carrier.SendCarrier?.Date) && DateTimeOffset.TryParse(carrier.SendCarrier.Date, out var sendDate))
    //             order.NgayGuiHangVanChuyen = sendDate.ToUniversalTime();
    //
    //         if (!string.IsNullOrEmpty(carrier.DeliveryDate) && DateTimeOffset.TryParse(carrier.DeliveryDate, out var deliveryDate))
    //             order.NgayThanhCong = deliveryDate.ToUniversalTime();
    //     }
    //
    //     if (payment != null)
    //     {
    //         order.TienChuyenKhoan = (decimal?)(payment.Transfer?.Amount ?? (long?)order.TienChuyenKhoan);
    //         order.TaiKhoanChuyenKhoan = payment.Transfer?.AccountId?.ToString() ?? order.TaiKhoanChuyenKhoan;
    //         order.TienDatCoc = (decimal?)(payment.Deposit?.Amount ?? (long?)order.TienDatCoc);
    //         order.TaiKhoanDatCoc = payment.Deposit?.AccountId?.ToString() ?? order.TaiKhoanDatCoc;
    //         order.TienChietKhau = (decimal?)(payment.Discount?.Amount ?? (long?)order.TienChietKhau);
    //         order.TienVat = (decimal?)(payment.Vat?.Amount ?? (long?)order.TienVat);
    //         order.TyLeVat = payment.Vat?.Type?.Equals("percent", StringComparison.OrdinalIgnoreCase) == true ? payment.Vat.Value?.ToString() : order.TyLeVat;
    //         order.TienQuetThe = (decimal?)(payment.Credit?.Amount ?? (decimal?)order.TienQuetThe);
    //         order.TaiKhoanQuetThe = payment.Credit?.AccountId?.ToString() ?? order.TaiKhoanQuetThe;
    //         order.MaGiaoDichQuetThe = payment.Credit?.Code ?? order.MaGiaoDichQuetThe;
    //         order.TienTieuDiem = (decimal?)(payment.UsedPoints?.Amount ?? (decimal?)order.TienTieuDiem);
    //         order.TrangThaiThanhToanDoanhNghiep = payment.BusinessPaymentStatus?.ToString() ?? order.TrangThaiThanhToanDoanhNghiep;
    //         order.MaGiamGia = payment.CouponCode ?? order.MaGiamGia;
    //     }
    //
    //     order.GhiChuCuaKhach = info.Description ?? order.GhiChuCuaKhach;
    //     order.GhiChuNoiBo = info.PrivateDescription ?? order.GhiChuNoiBo;
    //     order.TrangThaiDonHang = info.Status?.ToString() ?? order.TrangThaiDonHang;
    //     order.NgayXacNhan = info.ConfirmedAt.HasValue ? DateTimeOffset.FromUnixTimeSeconds(info.ConfirmedAt.Value).ToUniversalTime() : order.NgayXacNhan;
    //
    //     order.NguonDonHang = item.Channel?.SaleChannel?.ToString() ?? order.NguonDonHang;
    //     order.UpdatedAt = DateTimeOffset.UtcNow.ToUniversalTime();
    //
    //     await _appDbContext.SaveChangesAsync();
    //
    //     return "Cập nhật thành công đơn hàng trong bảng OrderNhanhVn";
    // }
    //
    // public async Task<string> WebhookNhanhVnOrderDelete(NhanhOrderDeleteWebhookRequest request)
    // {
    //     if (request?.Data == null || request.Data.Count == 0)
    //     {
    //         return "Không có dữ liệu xoá đơn hàng";
    //     }
    //
    //     var validNhanhOrderIds = request.Data
    //         .Select(id => id.ToString())
    //         .ToHashSet();
    //
    //     var orders = await _appDbContext.OrderNhanhVns
    //         .Where(order => !order.IsDeleted
    //                         && validNhanhOrderIds.Contains(order.IdDonHangNhanh!))
    //         .ToListAsync();
    //
    //     foreach (var order in orders)
    //     {
    //         order.IsDeleted = true;
    //         order.UpdatedAt = DateTimeOffset.UtcNow.ToUniversalTime();
    //     }
    //
    //     if (orders.Count > 0)
    //     {
    //         await _appDbContext.SaveChangesAsync();
    //     }
    //
    //     var ignoredCount = request.Data.Count - orders.Count;
    //     return $"Xoá {orders.Count} đơn trong bảng OrderNhanhVn; bỏ qua {ignoredCount} đơn.";
    // }

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
            return $"Sản phẩm với id: {nhanhProductId} không tồn tại trong hệ thống";
        }
        
        product.UpdatedAt = DateTimeOffset.UtcNow;
        product.ParentProductId = item.ParentId != 0 ? item.ParentId.ToString() : null;
        product.ProductCode = item.Code;
        product.Barcode = item.Barcode;
        product.ProductName = item.Name;
        product.OtherName = item.OtherName;
        product.Status = item.Status?.ToString();
    
        // Danh mục
        product.CategoryCode = item.Category?.Code;
        product.CategoryName = item.Category?.Name;
        product.InternalCategoryCode = item.InternalCategory?.Code;
        product.ProductType = item.Type?.Name;
    
        // Giá
        if (item.Prices != null)
        {
            product.Price = (decimal?)item.Prices.Retail;
            product.ImportPrice = (decimal?)item.Prices.Import;
            product.OldPrice = (decimal?)item.Prices.Old;
            product.WholesalePrice = (decimal?)item.Prices.Wholesale;
            product.CostPrice = (decimal?)item.Prices.AvgCost;
        }
        product.Vat = item.Vat;
    
        // Ảnh
        product.CoverImage = item.Images?.Avatar;
    
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
    
        await _appDbContext.SaveChangesAsync();
    
        return "Cập nhật thành công";
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

        if (productsToDelete.Any())
        {
            foreach (var p in productsToDelete)
            {
                p.IsDeleted = true;
                p.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await _appDbContext.SaveChangesAsync();
        }
        else
        {
            return "Sản phẩm không tồn tại";
        }

        return $"Xóa thành công {productsToDelete.Count} sản phẩm từ Nhanh.vn.";
    }
}