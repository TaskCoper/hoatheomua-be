using System.Net.Http.Json;
using System.Text.Json;
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
        
        var evt = json.TryGetProperty("event", out var e) ? e.GetString() : null;

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
        
        return evt switch
        {
            NhanhWebhookEvents.InventoryChange =>
                await WebhookNhanhVnInventory(Deserialize<NhanhInventoryChangeWebhookRequest>(json)),

            NhanhWebhookEvents.ProductAdd =>
                await WebhookNhanhVnProductAdd(Deserialize<NhanhProductAddWebhookRequest>(json)),

            NhanhWebhookEvents.ProductUpdate =>
                await WebhookNhanhVnProductUpdate(Deserialize<NhanhProductAddWebhookRequest>(json)),

            NhanhWebhookEvents.ProductDelete =>
                await WebhookNhanhVnProductDelete(Deserialize<NhanhProductDeleteWebhookRequest>(json)),

            // NhanhWebhookEvents.OrderAdd =>
            //     await WebhookNhanhVnOrderAdd(Deserialize<NhanhOrderAddWebhookRequest>(body)),
            //
            // NhanhWebhookEvents.OrderUpdate =>
            //     await WebhookNhanhVnOrderUpdate(Deserialize<NhanhOrderUpdateWebhookRequest>(body)),
            //
            // NhanhWebhookEvents.OrderDelete =>
            //     await WebhookNhanhVnOrderDelete(Deserialize<NhanhOrderDeleteWebhookRequest>(body)),

            _ => throw new BadRequestException("Không tồn tại webhooks")
        };
    }
    private static T Deserialize<T>(JsonElement body)
    {
        var result = body.Deserialize<T>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result == null)
        {
            throw new BadRequestException("Payload webhook Nhanh.vn không hợp lệ.");
        }
    
        return result;
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
                throw new ConflictException($"Sản phẩm với id: {itemId} không tồn tại trong hệ thống");
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
            product.WarrantyStock = (decimal?)item.Warranty.Remain;
            product.HeldComponents = (decimal?)item.Warranty.Holding;
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
            throw new BadRequestException("Có sản phẩm đã tồn tại trong hệ thống");
        }

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
                product.TotalStock = (decimal)item.Inventory.Remain;//tổng tồn
                //product.Stock = (decimal)item.Inventory.Remain;// tồn
                product.WarehouseStock = (decimal)item.Inventory.Remain;//tồn trong kho
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

        await _appDbContext.Products.AddAsync(product);
        await _appDbContext.SaveChangesAsync();

        return "Thêm mới thành công sản phẩm";
    }

    public async Task<string> WebhookNhanhVnOrderAdd(NhanhOrderAddWebhookRequest? request)
    {
        if (request == null || request.Data == null)
        {
            throw new BadRequestException("Không có dữ liệu thay đổi đơn hàng");
        }

        var item = request.Data;

        if (item.Info.Id <= 0 || item.Info.Id > int.MaxValue)
        {
            throw new BadRequestException("ID đơn hàng từ Nhanh.vn không hợp lệ.");
        }

        var existingOrder = await _appDbContext.Orders
            .FirstOrDefaultAsync(x => !x.IsDeleted
                                      && x.NhanhOrderId.HasValue
                                      && x.NhanhOrderId.Value == item.Info.Id);

        if (existingOrder != null)
        {
            throw new BadRequestException("Có đơn hàng đã tồn tại trong hệ thống.");
        }

        var totalProductFee = item.Products.Sum(x =>
            (x.Price ?? 0) * (x.Quantity ?? 0));

        var totalShippingFee = item.Carrier?.CustomerShipFee
                               ?? item.Carrier?.ShipFee
                               ?? 0;

        var taxCode = item.Payment?.Vat?.TaxCode;
        var vatFee = string.IsNullOrEmpty(taxCode) ? 0 : totalProductFee * 0.08m;
        if (vatFee == 0 && item.Payment?.Vat?.Amount is > 0)
        {
            vatFee = item.Payment.Vat.Amount.Value;
        }

        var privateDescription = item.Info.PrivateDescription ?? string.Empty;
        string? parsedTaxCode = null;
        string? receiptFullName = null;
        string? receiptPhoneNumber = null;
        string? receiptAddress = null;
        string? receiptEmail = null;
        string? identityNumber = null;
        bool? isExportInvoice = false;
        HoaTheoMua.Repository.Enum.InvoiceType? invoiceType = null;

        var lines = privateDescription
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            if (line.StartsWith("MST:", StringComparison.OrdinalIgnoreCase))
            {
                isExportInvoice = true;
                invoiceType = HoaTheoMua.Repository.Enum.InvoiceType.Business;
                parsedTaxCode = line.Replace("MST:", string.Empty).Trim();
            }
            else if (line.StartsWith("Tên:", StringComparison.OrdinalIgnoreCase))
            {
                isExportInvoice = true;
                invoiceType = HoaTheoMua.Repository.Enum.InvoiceType.Personal;
                receiptFullName = line.Replace("Tên:", string.Empty).Trim();
            }
            else if (line.StartsWith("CCCD:", StringComparison.OrdinalIgnoreCase))
            {
                isExportInvoice = true;
                invoiceType = HoaTheoMua.Repository.Enum.InvoiceType.Personal;
                identityNumber = line.Replace("CCCD:", string.Empty).Trim();
            }
            else if (line.StartsWith("ĐC:", StringComparison.OrdinalIgnoreCase))
            {
                isExportInvoice = true;
                invoiceType = HoaTheoMua.Repository.Enum.InvoiceType.Personal;
                receiptAddress = line.Replace("ĐC:", string.Empty).Trim();
            }
            else if (line.StartsWith("Email:", StringComparison.OrdinalIgnoreCase))
            {
                isExportInvoice = true;
                receiptEmail = line.Replace("Email:", string.Empty).Trim();
            }
            else if (line.StartsWith("SĐT:", StringComparison.OrdinalIgnoreCase))
            {
                isExportInvoice = true;
                receiptPhoneNumber = line.Replace("SĐT:", string.Empty).Trim();
            }
        }

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
            NhanhTrackingUrl = item.Info.TrackingUrl,

            ShippingFee = totalShippingFee,
            ProductFee = totalProductFee,
            TotalFee = totalShippingFee + totalProductFee + vatFee,
            Vat = vatFee,
            // PaymentMethod = HoaTheoMua.Repository.Enum.PaymentMethod.VA,
            TaxCode = parsedTaxCode ?? taxCode,
            ReceiptFullName = receiptFullName,
            ReceiptPhoneNumber = receiptPhoneNumber,
            ReceiptAddress = receiptAddress,
            ReceiptEmail = receiptEmail,
            IdentityNumber = identityNumber,
            IsExportInvoice = isExportInvoice,
            InvoiceType = invoiceType,
        };

        order.NhanhOrderId = (int)item.Info.Id;
        order.CreatedAt = DateTimeOffset.UtcNow;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        order.UserId = Guid.TryParse(item.ShippingAddress?.Id, out var userId)
            ? userId
            : Guid.Empty; //UserId from Nhanh.vn

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

        var nhanhProductIds = item.Products
            .Select(x => x.Id.ToString())
            .Distinct()
            .ToList();

        var productsInDb = await _appDbContext.Products
            .Where(x => !x.IsDeleted
                        && x.NhanhProductId != null
                        && nhanhProductIds.Contains(x.NhanhProductId))
            .ToListAsync();

        var productByNhanhId = productsInDb.ToDictionary(x => x.NhanhProductId!);
        var newOrderDetails = new List<HoaTheoMua.Repository.Entity.OrderDetails>();

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
                Name = productItem.Name ?? string.Empty,
                Number = (int)productItem.Quantity.Value,
                Price = productItem.Price ?? 0,

                Weight = (double)(productInDb?.Weight ?? productItem.Weight ?? 0m),
                Length = (double)(productInDb?.Length ?? 0m),
                Width = (double)(productInDb?.Width ?? 0m),
                Height = (double)(productInDb?.Height ?? 0m),

                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };

            // orderDetail.ItemId = (int)productItem.Id;
            // orderDetail.Name = productItem.Name ?? productItem.Code ?? string.Empty;
            // orderDetail.Number = (int)productItem.Quantity.Value;
            // orderDetail.Price = productItem.Price ?? 0;
            // orderDetail.Weight = (double)(productInDb?.Weight ?? productItem.Weight ?? 0m);
            // orderDetail.Length = (double)(productInDb?.Length ?? 0m);
            // orderDetail.Width = (double)(productInDb?.Width ?? 0m);
            // orderDetail.Height = (double)(productInDb?.Height ?? 0m);

            newOrderDetails.Add(orderDetail);
        }

        await _appDbContext.Orders.AddAsync(order);
        await _appDbContext.OrderDetails.AddRangeAsync(newOrderDetails);
        await _appDbContext.SaveChangesAsync();

        return $"Thêm mới thành công 1 đơn hàng ";
    }

    public async Task<string> WebhookNhanhVnOrderUpdate(NhanhOrderUpdateWebhookRequest request)
    {
        if (request == null || request.Data == null)
        {
            throw new BadRequestException("Không có dữ liệu cập nhật đơn hàng");
        }

        var item = request.Data;
        var updatedCount = 0;
        var ignoredCount = 0;

        var nhanhOrderId = (int)item.Info.Id;

        var order = await _appDbContext.Orders
            .FirstOrDefaultAsync(x => !x.IsDeleted
                                      && x.NhanhOrderId.HasValue
                                      && x.NhanhOrderId.Value == nhanhOrderId);

        // Không tạo đơn mới trong event update.
        if (order == null)
        {
            throw new ConflictException("Đơn hàng không tồn tại");
        }

        var incomingUpdatedAt = item.Info.UpdatedAt.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(item.Info.UpdatedAt.Value)
            : (DateTimeOffset?)null;

        // Webhook đến trễ hoặc bị gửi lại: bỏ qua, vẫn trả HTTP 200.
        if (incomingUpdatedAt.HasValue
            && order.UpdatedAt.HasValue
            && incomingUpdatedAt <= order.UpdatedAt.Value)
        {
            return $"Cập nhật {updatedCount} đơn; bỏ qua {ignoredCount + 1} đơn.";
        }

        // Nhanh.vn là nguồn sự thật cho các trường bên dưới.
        order.Status = item.Info.Status?.ToString() ?? order.Status;
        order.TrackNumber = item.Carrier?.CarrierCode ?? order.TrackNumber;
        order.NhanhTrackingUrl = item.Info.TrackingUrl ?? order.NhanhTrackingUrl;

        if (!string.IsNullOrWhiteSpace(item.Info.Description))
        {
            order.Remarks = item.Info.Description;
        }

        if (item.Info.CreatedAt > 0)
        {
            order.OrderTime = item.Info.CreatedAt.Value;
        }

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

        var taxCode = item.Payment?.Vat?.TaxCode;
        var totalProductFee = order.ProductFee;
        if (item.Products != null && item.Products.Any())
        {
            totalProductFee = item.Products.Sum(x =>
                (x.Price ?? 0) * (x.Quantity ?? 0));
            order.ProductFee = totalProductFee;
        }

        var totalShippingFee = item.Carrier?.CustomerShipFee
                               ?? item.Carrier?.ShipFee
                               ?? order.ShippingFee;
        order.ShippingFee = totalShippingFee;

        var vatFee = string.IsNullOrEmpty(taxCode) ? 0 : totalProductFee * 0.08m;
        if (vatFee == 0 && item.Payment?.Vat?.Amount is > 0)
        {
            vatFee = item.Payment.Vat.Amount.Value;
        }

        var privateDescription = item.Info.PrivateDescription ?? string.Empty;
        string? parsedTaxCode = null;
        string? receiptFullName = null;
        string? receiptPhoneNumber = null;
        string? receiptAddress = null;
        string? receiptEmail = null;
        string? identityNumber = null;
        bool? isExportInvoice = false;
        HoaTheoMua.Repository.Enum.InvoiceType? invoiceType = null;

        foreach (var line in privateDescription.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (line.StartsWith("MST:", StringComparison.OrdinalIgnoreCase))
            {
                isExportInvoice = true;
                invoiceType = HoaTheoMua.Repository.Enum.InvoiceType.Business;
                parsedTaxCode = line.Replace("MST:", string.Empty).Trim();
            }
            else if (line.StartsWith("Tên:", StringComparison.OrdinalIgnoreCase))
            {
                isExportInvoice = true;
                invoiceType = HoaTheoMua.Repository.Enum.InvoiceType.Personal;
                receiptFullName = line.Replace("Tên:", string.Empty).Trim();
            }
            else if (line.StartsWith("CCCD:", StringComparison.OrdinalIgnoreCase))
            {
                isExportInvoice = true;
                invoiceType = HoaTheoMua.Repository.Enum.InvoiceType.Personal;
                identityNumber = line.Replace("CCCD:", string.Empty).Trim();
            }
            else if (line.StartsWith("ĐC:", StringComparison.OrdinalIgnoreCase))
            {
                isExportInvoice = true;
                invoiceType = HoaTheoMua.Repository.Enum.InvoiceType.Personal;
                receiptAddress = line.Replace("ĐC:", string.Empty).Trim();
            }
            else if (line.StartsWith("Email:", StringComparison.OrdinalIgnoreCase))
            {
                isExportInvoice = true;
                receiptEmail = line.Replace("Email:", string.Empty).Trim();
            }
            else if (line.StartsWith("SĐT:", StringComparison.OrdinalIgnoreCase))
            {
                isExportInvoice = true;
                receiptPhoneNumber = line.Replace("SĐT:", string.Empty).Trim();
            }
        }

        order.TaxCode = parsedTaxCode ?? taxCode;
        order.ReceiptFullName = receiptFullName;
        order.ReceiptPhoneNumber = receiptPhoneNumber;
        order.ReceiptAddress = receiptAddress;
        order.ReceiptEmail = receiptEmail;
        order.IdentityNumber = identityNumber;
        order.IsExportInvoice = isExportInvoice;
        order.InvoiceType = invoiceType;

        order.ProductFee = totalProductFee;
        order.ShippingFee = totalShippingFee;
        order.Vat = vatFee;
        order.TotalFee = totalProductFee + totalShippingFee + vatFee;
        order.UpdatedAt = incomingUpdatedAt ?? DateTimeOffset.UtcNow;
        updatedCount++;

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

        var ignoredCount = request.Data.Count - orders.Count;//những đơn hàng tồn tại trong request nhưng không tồn tại trong db
        return $"Xoá {orders.Count} đơn; bỏ qua {ignoredCount} đơn.";
    }

    public async Task<string> WebhookNhanhVnProductUpdate(NhanhProductAddWebhookRequest request)
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
            throw new ConflictException($"Sản phẩm với id: {nhanhProductId} không tồn tại trong hệ thống");
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
            // product.Stock = (decimal)item.Inventory.Remain;
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

        await _appDbContext.SaveChangesAsync();

        return $"Cập nhật thành công";
    }

    public async Task<string> WebhookNhanhVnProductDelete(NhanhProductDeleteWebhookRequest request)
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
            throw new ConflictException("Sản phẩm không tồn tại");
        }

        return $"Xóa thành công {productsToDelete.Count} sản phẩm từ Nhanh.vn.";
    }
}