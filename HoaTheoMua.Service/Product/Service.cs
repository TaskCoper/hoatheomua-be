using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.AhamoveService;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HoaTheoMua.Service.Product;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;
    private readonly HttpClient _httpClient;
    private readonly NhanhVnOption _nhanhVnOption;
    private readonly IMapper _mapper;

    public Service(AppDbContext dbContext,
        IHttpContextAccessor httpContext,
        HttpClient httpClient,
        IConfiguration configuration, IMapper  mapper)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
        _httpClient = httpClient;
        _nhanhVnOption = new NhanhVnOption();
        configuration.GetSection(nameof(NhanhVnOption)).Bind(_nhanhVnOption);
        _mapper = mapper;
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


            nhanhProductList = await nhanhVnResponse.Content.ReadFromJsonAsync<NhanhVnProductListResponse.NhanhVnProductResponse>();
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
        var search = request.search?.Trim();
        var categoryIds = request.categoryIds?
            .Distinct()
            .ToList() ?? new List<Guid>();

        var query = _dbContext.Products
            .AsNoTracking()
            .Include(p => p.CategoryProductDetails)
            .ThenInclude(cpd => cpd.CategoryProduct)
            .AsQueryable();

        query = query.Where(x => !x.IsDeleted && (x.ParentProductId == null || x.ParentProductId == string.Empty));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.ToLower();

            query = query.Where(x =>
                (x.ProductName != null && x.ProductName.ToLower().Contains(normalizedSearch)) ||
                (x.Barcode != null && x.Barcode.ToLower().Contains(normalizedSearch)) ||
                (x.NhanhProductId != null && x.NhanhProductId.ToLower().Contains(normalizedSearch)));
        }

        if (request.priceFrom.HasValue)
        {
            query = query.Where(x => x.Price >= request.priceFrom.Value);
        }
        if (request.priceTo.HasValue)
        {
            query = query.Where(x => x.Price <= request.priceTo.Value);
        }

        if (categoryIds.Count > 0)
        {
            query = query.Where(x => x.CategoryProductDetails.Any(cpd =>
                !cpd.IsDeleted &&
                categoryIds.Contains(cpd.CategoryProductId)));
        }

        IOrderedQueryable<HoaTheoMua.Repository.Entity.Product> orderedQuery;

        if (categoryIds.Count == 1)
        {
            var categoryId = categoryIds[0];

            orderedQuery = query
                .OrderBy(x => x.CategoryProductDetails
                    .Where(cpd => !cpd.IsDeleted && cpd.CategoryProductId == categoryId)
                    .Select(cpd => cpd.Position)
                    .FirstOrDefault());
        }
        else
        {
            orderedQuery = query
                .OrderBy(x => x.Position);
        }

        if (request.sortBy != null && request.sortBy.Any())
        {
            foreach (var sortItem in request.sortBy)
            {
                if (string.IsNullOrWhiteSpace(sortItem)) continue;

                var isAsc = !sortItem.StartsWith("-");
                var field = sortItem.TrimStart('+', '-').Trim().ToLower();

                orderedQuery = (field, isAsc) switch
                {
                    ("price", true) => orderedQuery.ThenBy(x => x.Price),
                    ("price", false) => orderedQuery.ThenByDescending(x => x.Price),
                    ("name", true) => orderedQuery.ThenBy(x => x.ProductName),
                    ("name", false) => orderedQuery.ThenByDescending(x => x.ProductName),
                    (_, true) => orderedQuery.ThenBy(x => x.CreatedAt),
                    (_, false) => orderedQuery.ThenByDescending(x => x.CreatedAt)
                };
            }
        }
        else
        {
            orderedQuery = orderedQuery.ThenByDescending(x => x.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await orderedQuery
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var mappedItems = items.Select(MapToProductDetailResponse).ToList();

        return ApiResponseFactory.BasePagination(mappedItems, pageIndex, pageSize, totalCount);
    }

    public async Task<BasePaginationResponse> GetLocalProductsV2(Request.LocalProductQueryRequest request)
    {
        var pageIndex = request.pageIndex <= 0 ? 1 : request.pageIndex;
        var pageSize = request.pageSize <= 0 ? 10 : Math.Min(request.pageSize, 100);
        var search = request.search?.Trim();
        var categoryIds = request.categoryIds?
            .Distinct()
            .ToList() ?? new List<Guid>();

        var query = _dbContext.Products
            .AsNoTracking()
            .Include(p => p.CategoryProductDetails)
            .ThenInclude(cpd => cpd.CategoryProduct)
            .AsQueryable();

        query = query.Where(x => !x.IsDeleted && (x.ParentProductId == null || x.ParentProductId == string.Empty));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.ToLower();

            query = query.Where(x =>
                (x.ProductName != null && x.ProductName.ToLower().Contains(normalizedSearch)) ||
                (x.Barcode != null && x.Barcode.ToLower().Contains(normalizedSearch)) ||
                (x.NhanhProductId != null && x.NhanhProductId.ToLower().Contains(normalizedSearch)));
        }

        if (request.priceFrom.HasValue)
        {
            query = query.Where(x => x.Price >= request.priceFrom.Value);
        }
        if (request.priceTo.HasValue)
        {
            query = query.Where(x => x.Price <= request.priceTo.Value);
        }

        if (categoryIds.Count > 0)
        {
            if (request.isPin.HasValue)
            {
                query = query.Where(x => x.CategoryProductDetails.Any(cpd =>
                    !cpd.IsDeleted &&
                    categoryIds.Contains(cpd.CategoryProductId) &&
                    cpd.IsPin == request.isPin.Value));
            }
            else
            {
                query = query.Where(x => x.CategoryProductDetails.Any(cpd =>
                    !cpd.IsDeleted &&
                    categoryIds.Contains(cpd.CategoryProductId)));
            }
        }
        else
        {
            if (request.isPin.HasValue)
            {
                query = query.Where(x => x.IsPin == request.isPin.Value);
            }
        }

        IOrderedQueryable<HoaTheoMua.Repository.Entity.Product> orderedQuery;

        if (categoryIds.Count == 1)
        {
            var categoryId = categoryIds[0];

            orderedQuery = query
                .OrderBy(x => x.CategoryProductDetails
                    .Where(cpd => !cpd.IsDeleted && cpd.CategoryProductId == categoryId)
                    .Select(cpd => cpd.Position)
                    .FirstOrDefault());
        }
        else
        {
            orderedQuery = query
                .OrderBy(x => x.Position);
        }

        if (request.sortBy != null && request.sortBy.Any())
        {
            foreach (var sortItem in request.sortBy)
            {
                if (string.IsNullOrWhiteSpace(sortItem)) continue;

                var isAsc = !sortItem.StartsWith("-");
                var field = sortItem.TrimStart('+', '-').Trim().ToLower();

                orderedQuery = (field, isAsc) switch
                {
                    ("price", true) => orderedQuery.ThenBy(x => x.Price),
                    ("price", false) => orderedQuery.ThenByDescending(x => x.Price),
                    ("name", true) => orderedQuery.ThenBy(x => x.ProductName),
                    ("name", false) => orderedQuery.ThenByDescending(x => x.ProductName),
                    (_, true) => orderedQuery.ThenBy(x => x.CreatedAt),
                    (_, false) => orderedQuery.ThenByDescending(x => x.CreatedAt)
                };
            }
        }
        else
        {
            orderedQuery = orderedQuery.ThenByDescending(x => x.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var items = await orderedQuery
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var mappedItems = items.Select(MapToProductDetailResponse).ToList();

        return ApiResponseFactory.BasePagination(mappedItems, pageIndex, pageSize, totalCount);
    }

    // public async Task<Response.ProductDetailResponse> GetLocalProductDetail(Guid id)
    // {
    //     var product = await _dbContext.Products
    //         .Include(p => p.CategoryProductDetails)
    //         .ThenInclude(cpd => cpd.CategoryProduct)
    //         .ThenInclude(x => x.ParentCategoryProduct)
    //         .ThenInclude(x => x!.ChildCategoryProducts)
    //         .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    //
    //     if (product == null)
    //     {
    //         throw new NotFoundException("PRODUCT_NOT_FOUND");
    //     }
    //
    //     return MapToProductDetailResponse(product);
    // }
    
    public async Task<Response.LocalProductDetailResponse> GetLocalProductDetail(Guid id)
    {
        var response = await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.CategoryProductDetails)
                .ThenInclude(x => x.CategoryProduct)
            .Where(p => p.Id == id && !p.IsDeleted)
            .Select(LocalProductDetailSelector)
            .FirstOrDefaultAsync();

        if (response == null)
        {
            throw new NotFoundException("PRODUCT_NOT_FOUND");
        }

        if (!string.IsNullOrWhiteSpace(response.ParentProductId))
        {
            response.Parent = await _dbContext.Products
                .AsNoTracking()
                .Where(p =>
                    !p.IsDeleted &&
                    p.NhanhProductId == response.ParentProductId)
                .Select(LocalProductDetailSelector)
                .FirstOrDefaultAsync();
        }

        if (!string.IsNullOrWhiteSpace(response.NhanhProductId))
        {
            response.Children = await _dbContext.Products
                .AsNoTracking()
                .Where(p =>
                    !p.IsDeleted &&
                    p.ParentProductId == response.NhanhProductId)
                .Select(LocalProductDetailSelector)
                .ToListAsync();
        }

        return response;
    }
    
    private static readonly Expression<Func<HoaTheoMua.Repository.Entity.Product, Response.LocalProductDetailResponse>> 
        LocalProductDetailSelector = product => new Response.LocalProductDetailResponse
    {
        Id = product.Id,
        IsPinned = product.IsPin,
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
        OtherImages = product.OtherImages,
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
        NhanhCategoryId = product.NhanhCategoryId,
        NhanhBrandId = product.NhanhBrandId,
        NhanhTypeId = product.NhanhTypeId,
        Supplier = product.Supplier,
        WarrantyAddress = product.WarrantyAddress,
        Slug = product.Slug,
        Length = product.Length,
        Width = product.Width,
        Height = product.Height,
        WebOrder = product.WebOrder,
        NhanhCreatedAt = product.NhanhCreatedAt,
        NhanhUpdatedAt = product.NhanhUpdatedAt,
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
            .Where(cpd =>
                !cpd.IsDeleted &&
                !cpd.CategoryProduct.IsDeleted)
            .Select(cpd => new HoaTheoMua.Service.CategoryProduct.Response.CategoryProductResponse
            {
                categoryId = cpd.CategoryProduct.Id,
                isPinned = cpd.IsPin,
                name = cpd.CategoryProduct.Name,
                name_En = cpd.CategoryProduct.Name_En,
                description = cpd.CategoryProduct.Description,
                description_En = cpd.CategoryProduct.Description_En,
                position = cpd.CategoryProduct.Position,
                metaData = cpd.CategoryProduct.MetaData,
                page = cpd.CategoryProduct.Page,
                startTime = cpd.CategoryProduct.StartTime,
                endTime = cpd.CategoryProduct.EndTime,
                type = cpd.CategoryProduct.Type,
                isActive = cpd.CategoryProduct.IsActive,
                userId = cpd.CategoryProduct.UserId,
                parentId = cpd.CategoryProduct.ParentCategoryProductId,
                Parent = cpd.CategoryProduct.ParentCategoryProduct == null ||
                         cpd.CategoryProduct.ParentCategoryProduct.IsDeleted
                    ? null
                    : new HoaTheoMua.Service.CategoryProduct.Response.CategoryProductResponse
                    {
                        categoryId = cpd.CategoryProduct.ParentCategoryProduct.Id,
                        name = cpd.CategoryProduct.ParentCategoryProduct.Name,
                        name_En = cpd.CategoryProduct.ParentCategoryProduct.Name_En,
                        description = cpd.CategoryProduct.ParentCategoryProduct.Description,
                        description_En = cpd.CategoryProduct.ParentCategoryProduct.Description_En,
                        position = cpd.CategoryProduct.ParentCategoryProduct.Position,
                        metaData = cpd.CategoryProduct.ParentCategoryProduct.MetaData,
                        page = cpd.CategoryProduct.ParentCategoryProduct.Page,
                        startTime = cpd.CategoryProduct.ParentCategoryProduct.StartTime,
                        endTime = cpd.CategoryProduct.ParentCategoryProduct.EndTime,
                        type = cpd.CategoryProduct.ParentCategoryProduct.Type,
                        isActive = cpd.CategoryProduct.ParentCategoryProduct.IsActive,
                        userId = cpd.CategoryProduct.ParentCategoryProduct.UserId,
                        parentId = cpd.CategoryProduct.ParentCategoryProduct.ParentCategoryProductId,
                        children = new List<HoaTheoMua.Service.CategoryProduct.Response.CategoryProductResponse>()
                    },
                children = cpd.CategoryProduct.ChildCategoryProducts!
                    .Where(child => !child.IsDeleted)
                    .OrderBy(child => child.Position)
                    .Select(child => new HoaTheoMua.Service.CategoryProduct.Response.CategoryProductResponse
                    {
                        categoryId = child.Id,
                        name = child.Name,
                        name_En = child.Name_En,
                        description = child.Description,
                        description_En = child.Description_En,
                        position = child.Position,
                        metaData = child.MetaData,
                        page = child.Page,
                        startTime = child.StartTime,
                        endTime = child.EndTime,
                        type = child.Type,
                        isActive = child.IsActive,
                        userId = child.UserId,
                        parentId = child.ParentCategoryProductId,
                        children = new List<HoaTheoMua.Service.CategoryProduct.Response.CategoryProductResponse>()
                    })
                    .ToList()
            })
            .ToList()
    };
    

     private static Response.ProductDetailResponse MapToProductDetailResponse(HoaTheoMua.Repository.Entity.Product product)
    {
        return new Response.ProductDetailResponse
        {
            Id = product.Id,
            IsPinned =  product.IsPin,
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
            OtherImages = product.OtherImages,
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
            NhanhCategoryId = product.NhanhCategoryId,
            NhanhBrandId = product.NhanhBrandId,
            NhanhTypeId = product.NhanhTypeId,
            Supplier = product.Supplier,
            WarrantyAddress = product.WarrantyAddress,
            Slug = product.Slug,
            Length = product.Length,
            Width = product.Width,
            Height = product.Height,
            WebOrder = product.WebOrder,
            NhanhCreatedAt = product.NhanhCreatedAt,
            NhanhUpdatedAt = product.NhanhUpdatedAt,
            ProductLabel = product.ProductLabel,
            BranchPriceDuplicate = product.BranchPriceDuplicate,
            WholesaleBranchPriceDuplicate = product.WholesaleBranchPriceDuplicate,
            PriceLabel = product.PriceLabel,
            Size = product.Size,
            Color = product.Color,
            Origin = product.Origin,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Categories = product.CategoryProductDetails?
                .Where(cpd => !cpd.IsDeleted && cpd.CategoryProduct != null && !cpd.CategoryProduct.IsDeleted)
                .Select(cpd => new HoaTheoMua.Service.CategoryProduct.Response.CategoryProductResponse
                {
                    categoryId = cpd.CategoryProduct.Id,
                    isPinned = cpd.IsPin,
                    name = cpd.CategoryProduct.Name,
                    name_En = cpd.CategoryProduct.Name_En,
                    description = cpd.CategoryProduct.Description,
                    description_En = cpd.CategoryProduct.Description_En,
                    position = cpd.CategoryProduct.Position,
                    metaData = cpd.CategoryProduct.MetaData,
                    page = cpd.CategoryProduct.Page,
                    startTime = cpd.CategoryProduct.StartTime,
                    endTime = cpd.CategoryProduct.EndTime,
                    type = cpd.CategoryProduct.Type,
                    isActive = cpd.CategoryProduct.IsActive,
                    userId = cpd.CategoryProduct.UserId,
                    parentId = cpd.CategoryProduct.ParentCategoryProductId,
                    Parent = cpd.CategoryProduct.ParentCategoryProduct == null ||
                             cpd.CategoryProduct.ParentCategoryProduct.IsDeleted
                        ? null
                        : new HoaTheoMua.Service.CategoryProduct.Response.CategoryProductResponse
                        {
                            categoryId = cpd.CategoryProduct.ParentCategoryProduct.Id,
                            name = cpd.CategoryProduct.ParentCategoryProduct.Name,
                            name_En = cpd.CategoryProduct.ParentCategoryProduct.Name_En,
                            description = cpd.CategoryProduct.ParentCategoryProduct.Description,
                            description_En = cpd.CategoryProduct.ParentCategoryProduct.Description_En,
                            position = cpd.CategoryProduct.ParentCategoryProduct.Position,
                            metaData = cpd.CategoryProduct.ParentCategoryProduct.MetaData,
                            page = cpd.CategoryProduct.ParentCategoryProduct.Page,
                            startTime = cpd.CategoryProduct.ParentCategoryProduct.StartTime,
                            endTime = cpd.CategoryProduct.ParentCategoryProduct.EndTime,
                            type = cpd.CategoryProduct.ParentCategoryProduct.Type,
                            isActive = cpd.CategoryProduct.ParentCategoryProduct.IsActive,
                            userId = cpd.CategoryProduct.ParentCategoryProduct.UserId,
                            parentId = cpd.CategoryProduct.ParentCategoryProduct.ParentCategoryProductId,
                            children = new List<HoaTheoMua.Service.CategoryProduct.Response.CategoryProductResponse>()
                        },
                    children = cpd.CategoryProduct.ChildCategoryProducts != null
                        ? cpd.CategoryProduct.ChildCategoryProducts
                            .Where(child => !child.IsDeleted)
                            .OrderBy(child => child.Position)
                            .Select(child => new HoaTheoMua.Service.CategoryProduct.Response.CategoryProductResponse
                            {
                                categoryId = child.Id,
                                name = child.Name,
                                name_En = child.Name_En,
                                description = child.Description,
                                description_En = child.Description_En,
                                position = child.Position,
                                metaData = child.MetaData,
                                page = child.Page,
                                startTime = child.StartTime,
                                endTime = child.EndTime,
                                type = child.Type,
                                isActive = child.IsActive,
                                userId = child.UserId,
                                parentId = child.ParentCategoryProductId,
                                children = new List<HoaTheoMua.Service.CategoryProduct.Response.CategoryProductResponse>()
                            }).ToList()
                        : new List<HoaTheoMua.Service.CategoryProduct.Response.CategoryProductResponse>()
                })
                .ToList() ?? new List<HoaTheoMua.Service.CategoryProduct.Response.CategoryProductResponse>(),
        };
    }

    public async Task<decimal> GetMaxPrice(List<Guid>? categoryIds = null)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .AsQueryable();

        query = query.Where(x => !x.IsDeleted && x.Price.HasValue);

        if (categoryIds != null && categoryIds.Any())
        {
            query = query.Where(x => x.CategoryProductDetails.Any(cpd => !cpd.IsDeleted && categoryIds.Contains(cpd.CategoryProductId)));
        }

        var maxPrice = await query.MaxAsync(x => x.Price);

        return maxPrice ?? 0;
    }

    public async Task<BaseResponse> MoveProduct(Guid id, Request.MoveProductRequest request)
    {
        if (request == null)
        {
            throw new BadRequestException("Invalid movement data.");
        }

        if (id == Guid.Empty)
        {
            throw new BadRequestException("Product ID is required.");
        }

        if (request.CategoryId.HasValue && request.CategoryId.Value == Guid.Empty)
        {
            throw new BadRequestException("Category ID cannot be empty.");
        }

        if (request.CategoryId.HasValue)
        {
            var categoryProductDetail = await _dbContext.CategoryProductDetails
                .FirstOrDefaultAsync(x =>
                    x.ProductId == id &&
                    x.CategoryProductId == request.CategoryId.Value &&
                    !x.IsDeleted &&
                    x.IsPin &&
                    !x.CategoryProduct.IsDeleted &&
                    !x.Product.IsDeleted &&
                    (x.Product.ParentProductId == null || x.Product.ParentProductId == string.Empty));

            if (categoryProductDetail == null)
            {
                throw new BadRequestException("Only product pinned in this category can update position");
            }

            categoryProductDetail.Position = request.Position;
            categoryProductDetail.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(x =>
                                                                                    x.Id == id &&
                                                                                    !x.IsDeleted &&
                                                                                    x.IsPin &&
                                                                                    (x.ParentProductId == null || x.ParentProductId == string.Empty));

            if (product == null)
            {
                throw new BadRequestException("Only product with pin can update position");
            }

            product.Position = request.Position;
            product.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        return ApiResponseFactory.Base("Product position updated successfully.");

    }

    public async Task<BaseResponse> PinProduct(Request.PinProductRequest request)
    {
        if (request == null)
        {
            throw new BadRequestException("Invalid pin data.");
        }

        if (request.ProductId == Guid.Empty)
        {
            throw new BadRequestException("Product ID is required.");
        }

        if (request.CategoryId.HasValue && request.CategoryId.Value == Guid.Empty)
        {
            throw new BadRequestException("Category ID cannot be empty.");
        }
        
        var product = await _dbContext.Products.FirstOrDefaultAsync(x =>
                                                                    x.Id == request.ProductId &&
                                                                    !x.IsDeleted &&
                                                                    (x.ParentProductId == null || x.ParentProductId == string.Empty));

        if (product == null)
        {
            throw new BadRequestException("Product not found or is not a parent product.");
        }

        if (request.IsPin)
        {
            if (request.CategoryId.HasValue)
            {
                var pinnedCategoryProductCount = await _dbContext.CategoryProductDetails
                    .CountAsync(x =>
                        !x.IsDeleted &&
                        x.CategoryProductId == request.CategoryId.Value &&
                        x.IsPin &&
                        (x.Product.ParentProductId == null || x.Product.ParentProductId == string.Empty));

                if (pinnedCategoryProductCount >= 32)
                {
                    throw new BadRequestException("Only 32 products can be pinned in this category.");
                }
            }
            else
            {
                var pinnedProductCount = await _dbContext.Products
                    .CountAsync(x =>
                        !x.IsDeleted &&
                        x.IsPin &&
                        (x.ParentProductId == null || x.ParentProductId == string.Empty));

                if (pinnedProductCount >= 32)
                {
                    throw new BadRequestException("Only 32 products can be pinned.");
                }
            }
        }

        if (request.CategoryId.HasValue)
        {
            var categoryProductDetail = await _dbContext.CategoryProductDetails
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.CategoryProductId == request.CategoryId.Value &&
                    x.ProductId == request.ProductId);

            if (categoryProductDetail == null)
            {
                throw new BadRequestException("Product does not belong to this category.");
            }

            categoryProductDetail.IsPin = request.IsPin;
            categoryProductDetail.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            product.IsPin = request.IsPin;
            product.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        var message = request.IsPin
            ? "Product pinned successfully."
            : "Product unpinned successfully.";

        return ApiResponseFactory.Base(message);
    }

    public async Task<List<Response.PinnedProductResponse>> GetProductPin(Request.GetProductPinRequest request)
    {
        if (request == null)
        {
            throw new BadRequestException("Invalid request data.");
        }

        if (request.CategoryId.HasValue && request.CategoryId.Value == Guid.Empty)
        {
            throw new BadRequestException("Category ID cannot be empty.");
        }

        if (request.CategoryId.HasValue)
        {
            var categoryPinnedQuery = _dbContext.CategoryProductDetails
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsPin &&
                    x.CategoryProductId == request.CategoryId.Value &&
                    !x.CategoryProduct.IsDeleted &&
                    !x.Product.IsDeleted &&
                    (x.Product.ParentProductId == null || x.Product.ParentProductId == string.Empty))
                .OrderBy(x => x.Position) //
                .ThenBy(x => x.Product.ProductName);

            var result = await categoryPinnedQuery
                .Select(x => new Response.PinnedProductResponse
                {
                    ProductId = x.Product.Id,
                    CategoryId = x.CategoryProductId,
                    ProductCode = x.Product.ProductCode,
                    ProductName = x.Product.ProductName,
                    CoverImage = x.Product.CoverImage,
                    Price = x.Product.Price,
                    OldPrice = x.Product.OldPrice,
                    Position = x.Position
                })
                .ToListAsync();

            return result;
        }

        var pinnedProductQuery = _dbContext.Products
            .Where(x =>
                !x.IsDeleted &&
                x.IsPin &&
                x.ParentProductId == null)
            .OrderBy(x => x.Position) //
            .ThenBy(x => x.ProductName);

        var pinnedProducts = await pinnedProductQuery
            .Select(x => new Response.PinnedProductResponse
            {
                ProductId = x.Id,
                CategoryId = null,
                ProductCode = x.ProductCode,
                ProductName = x.ProductName,
                CoverImage = x.CoverImage,
                Price = x.Price,
                OldPrice = x.OldPrice,
                Position = x.Position
            })
            .ToListAsync();

        return pinnedProducts;
    }

    public async Task<string> UpdateProduct(Guid id, Request.UpdateProductRequest request)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (product == null)
        {
            throw new BadRequestException("Product does not exist.");
        }
        
        _mapper.Map(request, product);
        product.UpdatedAt = DateTimeOffset.UtcNow;
        
        _dbContext.Products.Update(product);
        await _dbContext.SaveChangesAsync();

        return "Cập nhật thành công";
    }
    
    public async Task<Response.ComboSpecificationResponse> GetComboSpecificationAsync(string nhanhProductId)
    {
        var combo = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.NhanhProductId == nhanhProductId && !p.IsDeleted);

        if (combo == null)
            throw new NotFoundException($"Không tìm thấy combo với NhanhProductId: {nhanhProductId}");

        var specs = await _dbContext.ComboSpecification
            .Include(s => s.Mat)
            .Where(s => s.SubId == combo.Id && s.IsActive)
            .ToListAsync();

        var response = new Response.ComboSpecificationResponse
        {
            SubId = combo.NhanhProductId,
            ProductCode = combo.ProductCode,
            ProductName = combo.ProductName,
            Materials = new List<Response.MaterialItemResponse>()
        };

        foreach (var spec in specs)
        {
            var mat = spec.Mat;
            decimal availableForSale = mat?.AvailableForSale ?? 0;
            int maxComboPossible = spec.Quantity > 0 ? (int)(availableForSale / spec.Quantity) : 0;
            
            response.Materials.Add(new Response.MaterialItemResponse
            {
                MaterialId = mat?.NhanhProductId ?? spec.MatId.ToString(),
                MaterialName = mat?.ProductName ?? "Unknown",
                Role = spec.IsCore ? "1" : "2",
                RoleName = spec.IsCore ? "Hoa chính" : "Phụ kiện",
                QuantityPerCombo = spec.Quantity,
                Unit = mat?.Unit ?? "Cành",
                AvailableForSale = availableForSale,
                MaxComboPossible = maxComboPossible
            });
        }

        // Ưu tiên Core (Role = 1) lên trước
        response.Materials = response.Materials.OrderBy(m => m.Role).ToList();

        return response;
    }

    public async Task<Response.MaterialPaginationResponse> GetMaterialsAsync(Request.GetMaterialsRequest request)
    {
        if (request.Page < 1 || request.PageSize < -1)
        {
            throw new BadRequestException("Tham số phân trang page hoặc pageSize không hợp lệ");
        }

        var query = _dbContext.Products.AsNoTracking()
            .Where(p => !p.IsDeleted && p.ProductType == "Thành phần");

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(p => 
                (p.ProductName != null && p.ProductName.ToLower().Contains(search)) || 
                (p.ProductCode != null && p.ProductCode.ToLower().Contains(search)));
        }

        int totalItems = await query.CountAsync();
        int totalPages = 1;

        if (request.PageSize > 0)
        {
            totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
            query = query.OrderBy(p => p.CreatedAt)
                         .Skip((request.Page - 1) * request.PageSize)
                         .Take(request.PageSize);
        }

        var products = await query.ToListAsync();

        var items = products.Select(p => new Response.MaterialResponse
        {
            MaterialId = p.NhanhProductId ?? p.Id.ToString(),
            MaterialCode = p.ProductCode,
            MaterialName = p.ProductName,
            Unit = p.Unit ?? "Cành",
            AvailableStock = p.AvailableForSale ?? 0,
            CoverImage = p.CoverImage,
            Tags = new List<string> { "Thành phần" }
        }).ToList();

        return new Response.MaterialPaginationResponse
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = items
        };
    }
}


