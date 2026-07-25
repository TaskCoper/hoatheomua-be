using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.Product;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    private readonly HttpClient _httpClient;
    private readonly NhanhVnOption _nhanhVnOption;

    public Service(AppDbContext dbContext,
        IHttpContextAccessor httpContext,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        _httpClient = httpClient;
        _nhanhVnOption = new NhanhVnOption();
        configuration.GetSection(nameof(NhanhVnOption)).Bind(_nhanhVnOption);
    }
    
    public async Task<NhanhVnProductListResponse.NhanhVnProductResponse> GetListProducts(Request.ProductListRequest request)
    {
        var configs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "nhanhvn");
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);
        var nhanhApi = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl) + "/product/list?appId=" + appId + "&businessId=" + businessId;
        
        
        
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(request, options);

        var newRequest = JsonSerializer.Deserialize<object>(json);

        // Console.WriteLine(
        //     $"[DEBUG NHANH.VN RAW DATA]: {JsonSerializer.Serialize(newRequest)}"
        // );
        
        
        // var nhanhProductListRequest = new Request.ProductListRequest()
        // {
        //     Filters = new Request.ProductFilters()
        //     {
        //         Ids = request.Filters?.Ids ?? new List<int>(),
        //         Status = request.Filters?.Status ?? new List<int>()
        //     },
        //     Paginator = new Request.ProductPaginator()
        //     {
        //         Size = request.Paginator?.Size ?? 50,
        //         Sort = new Request.ProductSort()
        //         {
        //             Id = request.Paginator?.Sort.Id ?? "desc"
        //         },                                                                                                                                      
        //         Next = request.Paginator?.Next != null 
        //             ? new Request.ProductNextPage { Id = request.Paginator.Next.Id }                    
        //             : null
        //     }
        // };
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(newRequest);
        
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);

        NhanhVnProductListResponse.NhanhVnProductResponse? nhanhProductList = null;
        
        if (nhanhVnResponse.IsSuccessStatusCode)
        {

            string rawJsonString = await nhanhVnResponse.Content.ReadAsStringAsync();

            // Đặt đoạn mã này để bạn đặt Breakpoint hoặc dùng NLog/Serilog in ra Tab Console xem giá trị thô:
            // Console.WriteLine($"[DEBUG NHANH.VN RAW DATA]: {rawJsonString}");


            nhanhProductList = await nhanhVnResponse.Content.ReadFromJsonAsync< NhanhVnProductListResponse.NhanhVnProductResponse>();
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (nhanhProductList == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }
        
        if (nhanhProductList.Code == 0)
        {
            throw new BadRequestException("ERR_INVALID_FORM_FIELDS");
        }

        // int totalRequests = nhanhProductList.Data.Count(x => true);
        //
        // int actualPageIndex = pageIndex < 1 ? 1 : pageIndex;
        // int actualPageSize = pageSize < 1 ? 10 : pageSize;
        //
        // var result = nhanhProductList.Data
        //     .Where(x => true)
        //     .Skip((actualPageIndex - 1) * actualPageSize)
        //     .Take(actualPageSize)
        //     .ToList();
        //
        // var paginationResult = ApiResponseFactory.BasePagination(
        //     items: result, 
        //     pageIndex: actualPageIndex, 
        //     pageSize: actualPageSize, 
        //     totalCount: totalRequests
        // );
        
        return nhanhProductList;
        
    }

    public async Task<NhanhVnProductDetailsResponse.NhanhVnProductDetailResponse> GetProductDetails(Request.ProductDetailRequest request)
    {
        
        var configs = await HoaTheoMua.Service.Common.ConfigHelper.GetConfigsByGroupsAsync(_dbContext, "nhanhvn");
        var appId = configs.GetValue("nhanhvn.app_id", _nhanhVnOption.AppId);
        var businessId = configs.GetValue("nhanhvn.business_id", _nhanhVnOption.BusinessId);
        var token = configs.GetValue("nhanhvn.token", _nhanhVnOption.Token);
        var nhanhApi = configs.GetValue("nhanhvn.base_url", _nhanhVnOption.BaseUrl) + "/product/detail?appId=" + appId + "&businessId=" + businessId;
        var nhanhProductDetailsRequest = request;
        
        var nhanhHttpRequest = new HttpRequestMessage(HttpMethod.Post, nhanhApi);
        nhanhHttpRequest.Headers.Add("Authorization", token);
        nhanhHttpRequest.Content = JsonContent.Create(nhanhProductDetailsRequest);
        
        
        
        
        HttpResponseMessage nhanhVnResponse = await _httpClient.SendAsync(nhanhHttpRequest);
        
        
        // var options = new JsonSerializerOptions
        // {
        //     DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        // };
        //
        // var json = JsonSerializer.Serialize(request, options);
        //
        // var newRequest = JsonSerializer.Deserialize<object>(json);
        //
        // Console.WriteLine(
        //     $"[DEBUG NHANH.VN RAW DATA]: {JsonSerializer.Serialize(newRequest)}"
        // );
        
        
        
        NhanhVnProductDetailsResponse.NhanhVnProductDetailResponse? nhanhProductDetails = null;
        
        if (nhanhVnResponse.IsSuccessStatusCode)
        {
            // nhanhProductDetails = await nhanhVnResponse.Content.ReadFromJsonAsync< ResponseProductDetails.NhanhVnProductDetailResponse>();
            string rawJsonString = await nhanhVnResponse.Content.ReadAsStringAsync();
    
            // Đặt đoạn mã này để bạn đặt Breakpoint hoặc dùng NLog/Serilog in ra Tab Console xem giá trị thô:
            // Console.WriteLine($"[DEBUG NHANH.VN RAW DATA]: {rawJsonString}");

            // 2. Cấu hình bắt buộc phải truyền kèm để kích hoạt Custom Converter của bạn
            var options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                // Ép kiểu số linh hoạt nếu Nhanh.vn trả về string số
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString 
            };

            // 3. Tiến hành phân rã chuỗi JSON an toàn thông qua bộ options đã có Converter
            nhanhProductDetails = JsonSerializer.Deserialize<NhanhVnProductDetailsResponse.NhanhVnProductDetailResponse>(rawJsonString, options);
        }
        else
        {
            // string nhanhErrorContent = await nhanhVnResponse.Content.ReadAsStringAsync();
            throw new BadRequestException($"NhanhVn API HTTP Error ({nhanhVnResponse.StatusCode})");
        }

        if (nhanhProductDetails == null)
        {
            throw new BadRequestException("NhanhVnResponse returned null or empty data");
        }
        if (nhanhProductDetails.Code == 0)
        {
            throw new BadRequestException("ERR_INVALID_FORM_FIELDS");
        }

        return nhanhProductDetails;
    }

    public async Task<BasePaginationResponse> GetLocalProducts(Request.LocalProductQueryRequest request)
    {
        var pageIndex = request.pageIndex <= 0 ? 1 : request.pageIndex;
        var pageSize = request.pageSize <= 0 ? 10 : Math.Min(request.pageSize, 100);

        var query = _dbContext.Products.AsQueryable();

        query = query.Where(x => !x.IsDeleted);

        if (request.priceFrom.HasValue)
        {
            query = query.Where(x => x.Price >= request.priceFrom.Value);
        }
        if (request.priceTo.HasValue)
        {
            query = query.Where(x => x.Price <= request.priceTo.Value);
        }

        if (request.categoryIds != null && request.categoryIds.Any())
        {
            query = query.Where(x => x.CategoryProductDetails.Any(cpd => !cpd.IsDeleted && request.categoryIds.Contains(cpd.CategoryProductId)));
        }

        if (request.sortBy != null && request.sortBy.Any())
        {
            IOrderedQueryable<HoaTheoMua.Repository.Entity.Product>? orderedQuery = null;

            foreach (var sortItem in request.sortBy)
            {
                if (string.IsNullOrWhiteSpace(sortItem)) continue;

                var isAsc = !sortItem.StartsWith("-");
                var field = sortItem.TrimStart('+', '-').Trim().ToLower();

                orderedQuery = (field, isAsc, orderedQuery) switch
                {
                    ("price", true, null) => query.OrderBy(x => x.Price),
                    ("price", false, null) => query.OrderByDescending(x => x.Price),
                    ("price", true, var o) => o.ThenBy(x => x.Price),
                    ("price", false, var o) => o.ThenByDescending(x => x.Price),

                    ("name", true, null) => query.OrderBy(x => x.ProductName),
                    ("name", false, null) => query.OrderByDescending(x => x.ProductName),
                    ("name", true, var o) => o.ThenBy(x => x.ProductName),
                    ("name", false, var o) => o.ThenByDescending(x => x.ProductName),

                    (_, true, null) => query.OrderBy(x => x.CreatedAt),
                    (_, false, null) => query.OrderByDescending(x => x.CreatedAt),
                    (_, true, var o) => o.ThenBy(x => x.CreatedAt),
                    (_, false, var o) => o.ThenByDescending(x => x.CreatedAt)
                };
            }

            if (orderedQuery != null)
            {
                query = orderedQuery;
            }
        }
        else
        {
            query = query.OrderByDescending(x => x.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ApiResponseFactory.BasePagination(items, pageIndex, pageSize, totalCount);
    }

    public async Task<Response.ProductDetailResponse> GetLocalProductDetail(Guid id)
    {
        var product = await _dbContext.Products
            .Include(p => p.CategoryProductDetails)
            .ThenInclude(cpd => cpd.CategoryProduct)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (product == null)
        {
            throw new NotFoundException("PRODUCT_NOT_FOUND");
        }

        var response = new Response.ProductDetailResponse
        {
            Id = product.Id,
            SourceRowNumber = product.SourceRowNumber,
            NhanhProductId = product.NhanhProductId,
            ParentProductId = product.ParentProductId,
            ParentProductCode = product.ParentProductCode,
            ParentProductName = product.ParentProductName,
            Barcode = product.Barcode,
            ProductType = product.ProductType,
            ProductCode = product.ProductCode,
            ProductName = product.ProductName,
            OtherName = product.OtherName,
            Unit = product.Unit,
            CoverImage = product.CoverImage,
            WarrantyMonths = product.WarrantyMonths,
            Weight = product.Weight,
            ImportPrice = product.ImportPrice,
            ImportVat = product.ImportVat,
            ImportPriceWithVat = product.ImportPriceWithVat,
            ImportVatMode = product.ImportVatMode,
            Price = product.Price,
            PriceWithVat = product.PriceWithVat,
            BranchPrice = product.BranchPrice,
            BranchPriceWithVat = product.BranchPriceWithVat,
            SaleVatMode = product.SaleVatMode,
            OldPrice = product.OldPrice,
            Vat = product.Vat,
            ProfitPercentage = product.ProfitPercentage,
            CostPrice = product.CostPrice,
            WholesalePrice = product.WholesalePrice,
            WholesalePriceWithVat = product.WholesalePriceWithVat,
            WholesaleBranchPrice = product.WholesaleBranchPrice,
            WholesaleBranchPriceWithVat = product.WholesaleBranchPriceWithVat,
            TotalStock = product.TotalStock,
            StockTransferring = product.StockTransferring,
            Stock = product.Stock,
            DeliveringStock = product.DeliveringStock,
            WarehouseStock = product.WarehouseStock,
            StockError = product.StockError,
            Held = product.Held,
            AvailableForSale = product.AvailableForSale,
            WarrantyStock = product.WarrantyStock,
            HeldComponents = product.HeldComponents,
            PendingImportStock = product.PendingImportStock,
            PreOrder = product.PreOrder,
            Status = product.Status,
            CategoryCode = product.CategoryCode,
            CategoryName = product.CategoryName,
            InternalCategoryCode = product.InternalCategoryCode,
            Brand = product.Brand,
            Supplier = product.Supplier,
            WarrantyAddress = product.WarrantyAddress,
            Slug = product.Slug,
            Length = product.Length,
            Width = product.Width,
            Height = product.Height,
            WebOrder = product.WebOrder,
            NhanhCreatedAt = product.NhanhCreatedAt,
            ProductLabel = product.ProductLabel,
            BranchPriceDuplicate = product.BranchPriceDuplicate,
            WholesaleBranchPriceDuplicate = product.WholesaleBranchPriceDuplicate,
            PriceLabel = product.PriceLabel,
            Size = product.Size,
            Color = product.Color,
            Origin = product.Origin,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Categories = product.CategoryProductDetails
                .Where(cpd => !cpd.IsDeleted && cpd.CategoryProduct != null && !cpd.CategoryProduct.IsDeleted)
                .Select(cpd => new HoaTheoMua.Service.CategoryProduct.Response.CategoryProductResponse
                {
                    categoryId = cpd.CategoryProduct.Id,
                    name = cpd.CategoryProduct.Name,
                    name_En = cpd.CategoryProduct.Name_En,
                    description = cpd.CategoryProduct.Description,
                    description_En = cpd.CategoryProduct.Description_En,
                    position = cpd.CategoryProduct.Position,
                    metaData = cpd.CategoryProduct.MetaData,
                    startTime = cpd.CategoryProduct.StartTime,
                    endTime = cpd.CategoryProduct.EndTime,
                    type = cpd.CategoryProduct.Type,
                    isActive = cpd.CategoryProduct.IsActive,
                    userId = cpd.CategoryProduct.UserId,
                    parentId = cpd.CategoryProduct.ParentCategoryProductId
                })
                .ToList()
        };

        return response;
    }

    public async Task<decimal> GetMaxPrice(Guid? categoryId = null)
    {
        var query = _dbContext.Products.AsQueryable();

        query = query.Where(x => !x.IsDeleted && x.Price.HasValue);

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryProductDetails.Any(cpd => !cpd.IsDeleted && cpd.CategoryProductId == categoryId.Value));
        }

        var maxPrice = await query.MaxAsync(x => x.Price);

        return maxPrice ?? 0;
    }
}