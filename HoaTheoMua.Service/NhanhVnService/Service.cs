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

    public async Task<string> WebhookNhanhVnOrderAdd(NhanhOrderAddWebhookRequest request)
    {
        if (request == null || request.Data == null || !request.Data.Any())
        {
            throw new BadRequestException("Không có dữ liệu thay đổi đơn hàng");
        }
        
        var nhanhOrderIds = request.Data
            .Select(x => x.Info.Id)
            .Distinct()
            .ToList();
        
        var existingOrders = await _appDbContext.Orders
            .Where(x => !x.IsDeleted
                        && x.NhanhOrderId.HasValue
                        && nhanhOrderIds.Contains(x.NhanhOrderId.Value))
            .ToListAsync();

        if (existingOrders.Any())
        {
            throw new BadRequestException("Có đơn hàng đã tồn tại trong hệ thống.");
        }
        
        var newOrders = new List<HoaTheoMua.Repository.Entity.Order>();
        var newOrderDetails = new List<HoaTheoMua.Repository.Entity.OrderDetails>();
        
        foreach(var item in request.Data)
        {
            if (item.Info.Id <= 0 || item.Info.Id > int.MaxValue)
            {
                throw new BadRequestException("ID đơn hàng từ Nhanh.vn không hợp lệ.");
            }

            var productFee = item.Products.Sum(x =>
                ((x.Price ?? 0) * (x.Quantity ?? 0)) - (x.Discount ?? 0));

            var shippingFee = item.Carrier?.CustomerShipFee
                              ?? item.Carrier?.ShipFee
                              ?? 0;

            var order = new HoaTheoMua.Repository.Entity.Order
            {
                //field required
                Id = Guid.NewGuid(),
                Status = item.Info.Status?.ToString() ?? string.Empty,
                TrackNumber = item.Carrier?.CarrierCode ?? string.Empty,
                FullName = item.ShippingAddress?.Name ?? string.Empty,
                Phone = item.ShippingAddress?.Mobile ?? string.Empty,
                Address = item.ShippingAddress?.Address ?? string.Empty,
                ShortAddress = item.ShippingAddress?.Address ?? string.Empty, //Address
                Ward = (int)(item.ShippingAddress?.WardId ?? 0),
                City = (int)(item.ShippingAddress?.CityId ?? 0),
                Remarks = item.Info.Description ?? string.Empty,
                OrderTime = item.Info.CreatedAt ?? 0,
            };

            order.NhanhOrderId = (int)item.Info.Id;
            order.CreatedAt = DateTimeOffset.UtcNow;
            order.UpdatedAt = DateTimeOffset.UtcNow;
            order.UserId = Guid.TryParse(item.ShippingAddress?.Id, out var userId)
                ? userId
                : Guid.Empty; //UserId from Nhanh.vn
            order.ShippingFee = shippingFee;
            order.ProductFee = productFee;
            order.TotalFee = productFee + shippingFee; //(Price * Quantity - Discount) + ShipFee

            order.TaxCode = item.Payment!.Vat.TaxCode ?? string.Empty;
            order.Vat = item.Payment!.Vat.Value ?? 0;

            // Thời gian tạo đơn phía Nhanh.vn
            if (item.Info.CreatedAt > 0)
            {
                order.CreatedAt = DateTimeOffset.FromUnixTimeSeconds(
                    item.Info.CreatedAt.Value);
            }

            // Thời gian cập nhật đơn phía Nhanh.vn
            if (item.Info.UpdatedAt > 0)
            {
                order.UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(
                    item.Info.UpdatedAt.Value);
            }
            
            var nhanhProductIds = request.Data
                .SelectMany(x => x.Products)
                .Select(x => x.Id.ToString())
                .Distinct()
                .ToList();

            var productsInDb = await _appDbContext.Products
                .Where(x => !x.IsDeleted
                            && x.NhanhProductId != null
                            && nhanhProductIds.Contains(x.NhanhProductId))
                .ToListAsync();

            var productByNhanhId = productsInDb.ToDictionary(x => x.NhanhProductId!);

            // Sản phẩm trong đơn
            foreach (var productItem in item.Products)
            {
                if (productItem.Id <= 0 || productItem.Id > int.MaxValue)
                {
                    throw new BadRequestException(
                        $"ID sản phẩm Nhanh.vn không hợp lệ: {productItem.Id}");
                }

                if (!productItem.Quantity.HasValue || productItem.Quantity <= 0)
                {
                    throw new BadRequestException(
                        $"Số lượng sản phẩm không hợp lệ: {productItem.Name}");
                }
                
                productByNhanhId.TryGetValue(productItem.Id.ToString(), out var productInDb);
                
                var orderDetail = new HoaTheoMua.Repository.Entity.OrderDetails
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,

                    ItemId = (int)productItem.Id,
                    Name = productItem.Name ?? productItem.Code ?? string.Empty,
                    Number = (int)productItem.Quantity.Value,
                    Price = productItem.Price ?? 0,
                    
                    Weight = (double)(productInDb?.Weight ?? productItem.Weight ?? 0m),
                    Length = (double)(productInDb?.Length ?? 0m),
                    Width = (double)(productInDb?.Width ?? 0m),
                    Height = (double)(productInDb?.Height ?? 0m),

                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                };

                orderDetail.ItemId = (int)productItem.Id;
                orderDetail.Name = productItem.Name ?? productItem.Code ?? string.Empty;
                orderDetail.Number = (int)productItem.Quantity.Value;
                orderDetail.Price = productItem.Price ?? 0;
                orderDetail.Weight = (double)(productInDb?.Weight ?? productItem.Weight ?? 0m);
                orderDetail.Length = (double)(productInDb?.Length ?? 0m);
                orderDetail.Width = (double)(productInDb?.Width ?? 0m);
                orderDetail.Height = (double)(productInDb?.Height ?? 0m);
                
                newOrderDetails.Add(orderDetail);
            }

            newOrders.Add(order);
        }

        if (newOrders.Any())
        {
            await _appDbContext.Orders.AddRangeAsync(newOrders);
            await _appDbContext.OrderDetails.AddRangeAsync(newOrderDetails);
            await _appDbContext.SaveChangesAsync();
        }

        return $"Thêm mới thành công {newOrders.Count} đơn hàng từ Nhanh.vn.";
    }

    public async Task<string> WebhookNhanhVnOrderUpdate(NhanhOrderAddWebhookRequest request)
    {
        if (request.Data == null || request.Data.Count == 0)
        {
            throw new BadRequestException("Không có dữ liệu cập nhật đơn hàng");
        }
        
        var nhanhOrderIds = request.Data
            .Where(x => x.Info.Id > 0 && x.Info.Id <= int.MaxValue)
            .Select(x => (int)x.Info.Id)
            .Distinct()
            .ToList();
        
        var ordersByNhanhId = await _appDbContext.Orders
            .Where(x => !x.IsDeleted
                        && x.NhanhOrderId.HasValue
                        && nhanhOrderIds.Contains(x.NhanhOrderId.Value))
            .ToDictionaryAsync(x => x.NhanhOrderId!.Value); //chuyển thành dictionary 

        var existingOrders = await _appDbContext.Orders
            .Where(x => !x.IsDeleted
                        && x.NhanhOrderId.HasValue
                        && nhanhOrderIds.Contains(x.NhanhOrderId.Value))
            .ToListAsync();
        
        var updatedCount = 0; //hợp lệ và cập nhật thành công
        var ignoredCount = 0; //order có ID ko hợp lệ
        
        foreach (var item in request.Data)
        {
            if (item.Info.Id <= 0 || item.Info.Id > int.MaxValue)
            {
                ignoredCount++;
                continue;
            }

            var nhanhOrderId = (int)item.Info.Id;

            // Không tạo đơn mới trong event update.
            if (!ordersByNhanhId.TryGetValue(nhanhOrderId, out var order))
            {
                ignoredCount++;
                continue;
            }

            var incomingUpdatedAt = item.Info.UpdatedAt.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(item.Info.UpdatedAt.Value)
                : (DateTimeOffset?)null;

            // Webhook đến trễ hoặc bị gửi lại: bỏ qua, vẫn trả HTTP 200.
            if (incomingUpdatedAt.HasValue
                && order.UpdatedAt.HasValue
                && incomingUpdatedAt <= order.UpdatedAt.Value)
            {
                ignoredCount++;
                continue;
            }

            // Nhanh.vn là nguồn sự thật cho các trường bên dưới.
            order.Status = item.Info.Status.ToString()!;
            order.TrackNumber = item.Carrier?.CarrierCode ?? order.TrackNumber;
            order.NhanhTrackingUrl = item.Info.TrackingUrl ?? order.NhanhTrackingUrl;

            if (!string.IsNullOrWhiteSpace(item.Info.Description))
            {
                order.Remarks = item.Info.Description;
            }

            if (item.Carrier?.CustomerShipFee != null)
            {
                order.ShippingFee = item.Carrier.CustomerShipFee.Value;
                order.TotalFee = order.ProductFee + order.ShippingFee;
            }

            // Chỉ cập nhật địa chỉ nếu payload thật sự có gửi.
            if (item.ShippingAddress != null)
            {
                order.FullName = item.ShippingAddress.Name ?? order.FullName;
                order.Phone = item.ShippingAddress.Mobile ?? order.Phone;
                order.Address = item.ShippingAddress.Address ?? order.Address;
                order.ShortAddress = item.ShippingAddress.Address ?? order.ShortAddress;

                if (item.ShippingAddress.CityId is <= int.MaxValue and >= 0)
                {
                    order.City = (int)item.ShippingAddress.CityId.Value;
                }

                if (item.ShippingAddress.WardId is <= int.MaxValue and >= 0)
                {
                    order.Ward = (int)item.ShippingAddress.WardId.Value;
                }
            }

            order.UpdatedAt = incomingUpdatedAt ?? DateTimeOffset.UtcNow;
            updatedCount++;
        }

        await _appDbContext.SaveChangesAsync();

        return $"Cập nhật {updatedCount} đơn; bỏ qua {ignoredCount} đơn.";
    }

    public async Task<string> WebhookNhanhVnOrderDelete(NhanhOrderDeleteWebhookRequest request)
    {
        if (request?.Data == null || request.Data.Count == 0)
        {
            throw new BadRequestException("Không có dữ liệu xoá đơn hàng");
        }

        var validNhanhOrderIds = request.Data
            .Where(id => id > 0 && id <= int.MaxValue)
            .Select(id => (int)id)
            .ToHashSet();

        var orders = await _appDbContext.Orders
            .Where(order => !order.IsDeleted
                            && order.NhanhOrderId.HasValue
                            && validNhanhOrderIds.Contains(order.NhanhOrderId.Value))
            .ToListAsync();

        foreach (var order in orders)
        {
            order.IsDeleted = true;
            order.UpdatedAt = DateTimeOffset.UtcNow;
        }

        if (orders.Count > 0)
        {
            await _appDbContext.SaveChangesAsync();
        }

        var ignoredCount = request.Data.Count - orders.Count;
        return $"Xoá {orders.Count} đơn; bỏ qua {ignoredCount} đơn.";
    }
}
