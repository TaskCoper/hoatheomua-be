using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
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
        IConfiguration configuration, IMapper mapper)
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

        var labels = request.labels?
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Select(label => label.Trim().ToLower().Replace(" ", ""))
            .Distinct()
            .ToList() ?? new List<string>();

        var query = _dbContext.Products
            .AsNoTracking()
            .Include(p => p.CategoryProductDetails)
            .ThenInclude(cpd => cpd.CategoryProduct)
            .Include(p => p.SubIds)
            .ThenInclude(cs => cs.Mat)
            .AsQueryable();

        query = query.Where(x => !x.IsDeleted && (x.ParentProductId == null || x.ParentProductId == string.Empty) && x.IsActive);
        
        //sort theo sellableQuantiy giảm dần

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

        if (labels.Count > 0)
        {
            query = query.Where(BuildProductLabelFilter(labels));
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

        var sellableQuantitySortItem = request.sortBy?
            .FirstOrDefault(sortItem =>
                !string.IsNullOrWhiteSpace(sortItem) &&
                sortItem.TrimStart('+', '-').Trim()
                    .Equals("sellablequantity", StringComparison.OrdinalIgnoreCase));
        var sortBySellableQuantity = sellableQuantitySortItem != null;
        var sortSellableQuantityDescending = sortBySellableQuantity &&
            sellableQuantitySortItem!.TrimStart().StartsWith("-", StringComparison.Ordinal);

        var totalCount = await query.CountAsync();
        List<HoaTheoMua.Repository.Entity.Product> items;
        Dictionary<Guid, (int? SellableQuantity, string? StockStatus, string? UnavailableReason)> quantities;

        if (sortBySellableQuantity)
        {
            // SellableQuantity là giá trị tính toán, nên phải tính trước khi phân trang.
            var allItems = await query.ToListAsync();
            quantities = await CalculateAvailableQty(allItems
                .Select(product => new ProductQtyProjection
                {
                    Id = product.Id,
                    ProductType = product.ProductType,
                    ParentProductId = product.ParentProductId,
                    NhanhProductId = product.NhanhProductId,
                    Stock = product.Stock
                })
                .ToList());

            var sortedItems = sortSellableQuantityDescending
                ? allItems.OrderByDescending(product => GetQuantityInfo(product, quantities).SellableQuantity ?? 0)
                : allItems.OrderBy(product => GetQuantityInfo(product, quantities).SellableQuantity ?? 0);

            items = sortedItems
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
        else
        {
            items = await orderedQuery
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Tính tồn kho khả dụng cho đúng các sản phẩm của trang hiện tại, giống V2.
            quantities = await CalculateAvailableQty(items
                .Select(product => new ProductQtyProjection
                {
                    Id = product.Id,
                    ProductType = product.ProductType,
                    ParentProductId = product.ParentProductId,
                    NhanhProductId = product.NhanhProductId,
                    Stock = product.Stock
                })
                .ToList());
        }

        var mappedItems = new List<Response.ProductDetailResponse>(items.Count);
        foreach (var product in items)
        {
            var response = MapToProductDetailResponse(product);
            var quantityInfo = GetQuantityInfo(product, quantities);
            response.SellableQuantity = quantityInfo.SellableQuantity;
            response.StockStatus = quantityInfo.StockStatus;
            response.UnavailableReason = quantityInfo.UnavailableReason;
            mappedItems.Add(response);
        }

        return ApiResponseFactory.BasePagination(mappedItems, pageIndex, pageSize, totalCount);
    }

    private static Expression<Func<HoaTheoMua.Repository.Entity.Product, bool>> BuildProductLabelFilter(
        IReadOnlyCollection<string> labels)
    {
        // Chuẩn hóa nhãn cần tìm: viết thường, bỏ khoảng trắng, bọc dấu phẩy để so khớp nguyên nhãn
        var normalizedLabels = labels
            .Select(label => $",{label.ToLower().Replace(" ", "")},")
            .ToList();

        return product =>
            product.ProductLabel != null &&
            normalizedLabels.Any(label =>
                ("," + product.ProductLabel.ToLower().Replace(" ", "") + ",").Contains(label));
    }

    public async Task<BasePaginationResponse> GetLocalProductsV2(
        Request.LocalProductQueryRequest request,
        ProductHierarchyType? productHierarchy = null)
    {
        if (productHierarchy.HasValue && !Enum.IsDefined(productHierarchy.Value))
        {
            throw new BadRequestException("Phạm vi sản phẩm không hợp lệ. Vui lòng chọn \"Parent\" hoặc \"All\".");
        }
        // Giai đoạn 1: Chuẩn hoá dữ liệu đầu vào.
        var pageIndex = request.pageIndex <= 0 ? 1 : request.pageIndex;
        var pageSize = request.pageSize <= 0 ? 10 : Math.Min(request.pageSize, 100);
        var search = request.search?.Trim();
        var categoryIds = request.categoryIds?
            .Distinct()
            .ToList() ?? new List<Guid>();

        // Giai đoạn 2: Khởi tạo query chỉ lấy sản phẩm gốc chưa bị xoá.
        var query = _dbContext.Products
            .AsNoTracking()
            .AsQueryable();

        query = query.Where(x => !x.IsDeleted && (x.ParentProductId == null || x.ParentProductId == string.Empty));

        // Giai đoạn 3: Áp dụng các điều kiện lọc tại cơ sở dữ liệu.
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.ToLower();

            query = query.Where(x =>
                (x.ProductName != null && x.ProductName.ToLower().Contains(normalizedSearch)) ||
                (x.Barcode != null && x.Barcode.ToLower().Contains(normalizedSearch)) ||
                (x.NhanhProductId != null && x.NhanhProductId.ToLower().Contains(normalizedSearch)) || 
                (x.ProductCode != null && x.ProductCode.ToLower().Contains(normalizedSearch)));
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
        
        if (request.isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.isActive.Value);
        }
        
        if (request.productType.HasValue)
        {
            switch (request.productType.Value)
            {
                case ProductType.Material:
                    query = query.Where(x =>
                        x.ProductLabel != null &&
                        ("," + x.ProductLabel.ToLower().Replace(" ", "") + ",")
                            .Contains(",vậtliệu,"));
                    break;
                
                case ProductType.Combo:
                    query = query.Where(x =>
                        // Combo độc lập: không có product con và chính nó có type Combo.
                        (x.ProductType == "Combo" &&
                          !_dbContext.Products.Any(child =>
                              !child.IsDeleted &&
                              child.ParentProductId == x.NhanhProductId))
                        ||
                        // Combo cha: parent có type Sản phẩm, có con và tất cả con đều có type Combo.
                        (x.ProductType == "Sản phẩm" &&
                         _dbContext.Products.Any(child =>
                             !child.IsDeleted &&
                             child.ParentProductId == x.NhanhProductId) &&
                         !_dbContext.Products.Any(child =>
                             !child.IsDeleted &&
                             child.ParentProductId == x.NhanhProductId &&
                             child.ProductType != "Combo")));
                    break;
                
                case ProductType.Product:
                    query = query.Where(x =>
                        x.ProductType == "Sản phẩm" &&
                        (
                            // Sản phẩm độc lập
                            !_dbContext.Products.Any(child =>
                                !child.IsDeleted &&
                                child.ParentProductId == x.NhanhProductId) ||
                            // Sản phẩm có con nhưng không phải full combo
                            _dbContext.Products.Any(child =>
                                !child.IsDeleted &&
                                child.ParentProductId == x.NhanhProductId &&
                                child.ProductType != "Combo")
                        ));
                    break;
            }
        }

        if (productHierarchy == ProductHierarchyType.All)
        {
            var matchedParents = await query
                .Select(parent => new
                {
                    parent.Id,
                    parent.NhanhProductId
                })
                .ToListAsync();

            var parentIds = matchedParents
                .Select(parent => parent.Id)
                .ToList();

            var parentNhanhProductIds = matchedParents
                .Where(parent => !string.IsNullOrWhiteSpace(parent.NhanhProductId))
                .Select(parent => parent.NhanhProductId!)
                .Distinct()
                .ToList();

            var childIds = parentNhanhProductIds.Count == 0
                ? new List<Guid>()
                : await _dbContext.Products
                    .AsNoTracking()
                    .Where(child =>
                        !child.IsDeleted &&
                        child.ParentProductId != null &&
                        parentNhanhProductIds.Contains(child.ParentProductId))
                    .Select(child => child.Id)
                    .ToListAsync();

            var productIds = parentIds
                .Concat(childIds)
                .Distinct()
                .ToList();

            query = _dbContext.Products
                .AsNoTracking()
                .Where(product => productIds.Contains(product.Id));
        }

        // Giai đoạn 4: Thiết lập thứ tự cơ sở trước khi tính tồn kho khả dụng.
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

        // Giai đoạn 5: Đếm và phân trang ngay tại DB, chỉ nạp đúng một trang entity.
        var totalCount = await query.CountAsync();

        var pageItems = await orderedQuery
            .Include(p => p.CategoryProductDetails)
                .ThenInclude(cpd => cpd.CategoryProduct)
                .ThenInclude(c => c.ParentCategoryProduct)
            .Include(p => p.CategoryProductDetails)
                .ThenInclude(cpd => cpd.CategoryProduct)
                .ThenInclude(c => c.ChildCategoryProducts)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Giai đoạn 6: Chỉ tính tồn kho khả dụng cho các sản phẩm thực sự được trả về.
        var quantities = await CalculateAvailableQty(pageItems
            .Select(product => new ProductQtyProjection
            {
                Id = product.Id,
                ProductType = product.ProductType,
                ParentProductId = product.ParentProductId,
                NhanhProductId = product.NhanhProductId,
                Stock = product.Stock
            })
            .ToList());

        // Giai đoạn 7: Map response và gán thông tin tồn kho khả dụng.
        var items = new List<Response.ProductDetailResponse>(pageItems.Count);
        foreach (var product in pageItems)
        {
            var response = MapToProductDetailResponse(product);
            var quantityInfo = GetQuantityInfo(product, quantities);
            response.SellableQuantity = quantityInfo.SellableQuantity;
            response.StockStatus = quantityInfo.StockStatus;
            response.UnavailableReason = quantityInfo.UnavailableReason;
            items.Add(response);
        }

        return ApiResponseFactory.BasePagination(items, pageIndex, pageSize, totalCount);
    }

    public async Task<Response.LocalProductDetailResponse> GetLocalProductDetail(Guid id)
    {
        var isAdmin = _httpContext.HttpContext?.User.IsInRole("Admin") == true;

        // Giai đoạn 1: Lấy thông tin sản phẩm chính.
        var response = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == id &&
                        !p.IsDeleted &&//nếu nó là con, ko phải là admin thì không cho xem details
                        (isAdmin || (p.IsActive &&
                                     (p.ParentProductId == null || p.ParentProductId == string.Empty))))
            .Select(LocalProductDetailSelector)
            .FirstOrDefaultAsync();

        if (response == null)
        {
            throw new NotFoundException("PRODUCT_NOT_FOUND", "PRODUCT_NOT_FOUND");
        }

        response.Materials = await _dbContext.ComboSpecifications
            .AsNoTracking()
            .Where(cs => cs.SubId == id && !cs.IsDeleted && !cs.Mat.IsDeleted)
            .Select(cs => new Response.ComboSpecification
            {
                Id = cs.Mat.Id,
                ProductName = cs.Mat.ProductName,
                CoverImage = cs.Mat.CoverImage,
                Quantity = cs.Quantity ?? 0m,
                Price = cs.Mat.Price ?? 0m
            })
            .ToListAsync();

        // Giai đoạn 2: Xác định sản phẩm cha nếu sản phẩm hiện tại là biến thể.
        var isParentProduct = string.IsNullOrWhiteSpace(response.ParentProductId);

        if (!isParentProduct)
        {
            var parentId = await _dbContext.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.NhanhProductId == response.ParentProductId)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            response.ParentId = parentId == Guid.Empty ? null : parentId;
            response.ParentProductGuidId = response.ParentId;
        }
        else
        {
            response.ParentId = null;
            response.ParentProductGuidId = null;
        }

        // Giai đoạn 3: Lấy các biến thể con của sản phẩm gốc.
        response.Children = !isParentProduct || string.IsNullOrWhiteSpace(response.NhanhProductId)
            ? new List<Response.LocalProductDetailResponse>()
            : await _dbContext.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.ParentProductId == response.NhanhProductId)
                .Select(LocalProductDetailSelector)
                .ToListAsync();

        foreach (var child in response.Children)
        {
            child.ParentProductGuidId = response.Id;
        }

        // Giai đoạn 4: Chuẩn bị sản phẩm chính và biến thể để tính tồn kho khả dụng.
        // Biến thể dùng cùng cấu tạo với Product cha. Quantity của mỗi vật liệu
        // được điều chỉnh theo Factor riêng của biến thể; Materials của cha giữ nguyên.
        var parentMaterials = response.Materials ?? new List<Response.ComboSpecification>();
        foreach (var child in response.Children)
        {
            var factor = child.Factor ?? 1m;
            child.Materials = parentMaterials.Select(material => new Response.ComboSpecification
            {
                Id = material.Id,
                ProductName = material.ProductName,
                CoverImage = material.CoverImage,
                Quantity = material.Quantity * factor,
                Price = material.Price
            }).ToList();
        }

        var productsToCalculate = new List<ProductQtyProjection>
        {
            new()
            {
                Id = response.Id,
                ProductType = response.ProductType,
                ParentProductId = response.ParentProductId,
                NhanhProductId = response.NhanhProductId,
                Stock = response.Stock
            }
        };
        productsToCalculate.AddRange(response.Children.Select(c => new ProductQtyProjection
        {
            Id = c.Id,
            ProductType = c.ProductType,
            ParentProductId = c.ParentProductId,
            NhanhProductId = c.NhanhProductId,
            Stock = c.Stock
        }));

        // Giai đoạn 5: Tính và gán số lượng khả dụng cho sản phẩm chính và từng biến thể.
        var quantities = await CalculateAvailableQty(
            productsToCalculate,
            variantsArePreloaded: isParentProduct && !string.IsNullOrWhiteSpace(response.NhanhProductId));

        if (quantities.TryGetValue(response.Id, out var mainInfo))
        {
            response.SellableQuantity = mainInfo.SellableQuantity;
            response.StockStatus = mainInfo.StockStatus;
            response.UnavailableReason = mainInfo.UnavailableReason;
        }
        else
        {
            response.SellableQuantity = 0;
            response.StockStatus = "Hết hàng";
            response.UnavailableReason = null;
        }

        foreach (var child in response.Children)
        {
            if (quantities.TryGetValue(child.Id, out var childInfo))
            {
                child.SellableQuantity = childInfo.SellableQuantity;
                child.StockStatus = childInfo.StockStatus;
                child.UnavailableReason = childInfo.UnavailableReason;
            }
            else
            {
                child.SellableQuantity = 0;
                child.StockStatus = "Hết hàng";
                child.UnavailableReason = null;
            }
        }

        return response;
    }

    public async Task<Response.ProductFamilyResponse> GetProductFamily(Guid id)
    {
        var selectedProduct = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Id == id && !product.IsDeleted && product.IsActive)
            .Select(product => new
            {
                product.Id,
                product.NhanhProductId,
                product.ParentProductId
            })
            .FirstOrDefaultAsync();

        if (selectedProduct == null)
            throw new NotFoundException("PRODUCT_NOT_FOUND", "PRODUCT_NOT_FOUND");

        var parentNhanhProductId = string.IsNullOrWhiteSpace(selectedProduct.ParentProductId)
            ? selectedProduct.NhanhProductId
            : selectedProduct.ParentProductId;

        if (string.IsNullOrWhiteSpace(parentNhanhProductId))
            throw new NotFoundException("PRODUCT_NOT_FOUND", "PRODUCT_NOT_FOUND");

        var parentRecord = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.NhanhProductId == parentNhanhProductId &&
                              !product.IsDeleted && product.IsActive)
            .Select(product => new
            {
                Item = new Response.ProductFamilyItem
                {
                    Id = product.Id,
                    ProductName = product.ProductName,
                    CoverImage = product.CoverImage,
                    Description = product.Description,
                    Price = product.Price
                },
                product.ProductType,
                product.ParentProductId,
                product.NhanhProductId,
                product.Stock
            })
            .FirstOrDefaultAsync();

        if (parentRecord == null)
            throw new NotFoundException("PRODUCT_PARENT_NOT_FOUND", "PRODUCT_PARENT_NOT_FOUND");

        var parent = parentRecord.Item;
        parent.Materials = await _dbContext.ComboSpecifications
            .AsNoTracking()
            .Where(specification => specification.SubId == parent.Id &&
                                    !specification.IsDeleted &&
                                    !specification.Mat.IsDeleted)
            .Select(specification => new Response.ComboSpecification
            {
                Id = specification.Mat.Id,
                ProductName = specification.Mat.ProductName,
                CoverImage = specification.Mat.CoverImage,
                Quantity = specification.Quantity ?? 0m,
                Price = specification.Mat.Price ?? 0m
            })
            .ToListAsync();

        var children = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.ParentProductId == parentNhanhProductId &&
                              !product.IsDeleted && product.IsActive)
            .Select(product => new
            {
                Item = new Response.ProductFamilyItem
                {
                    Id = product.Id,
                    ProductName = product.ProductName,
                    CoverImage = product.CoverImage,
                    Description = product.Description,
                    Price = product.Price
                },
                product.Factor,
                product.ProductType,
                product.ParentProductId,
                product.NhanhProductId,
                product.Stock
            })
            .ToListAsync();

        foreach (var child in children)
        {
            var factor = child.Factor ?? 1m;
            child.Item.Materials = parent.Materials.Select(material => new Response.ComboSpecification
            {
                Id = material.Id,
                ProductName = material.ProductName,
                CoverImage = material.CoverImage,
                Quantity = material.Quantity * factor,
                Price = material.Price
            }).ToList();
        }

        var productsToCalculate = new List<ProductQtyProjection>
        {
            new()
            {
                Id = parent.Id,
                ProductType = parentRecord.ProductType,
                ParentProductId = parentRecord.ParentProductId,
                NhanhProductId = parentRecord.NhanhProductId,
                Stock = parentRecord.Stock
            }
        };
        productsToCalculate.AddRange(children.Select(child => new ProductQtyProjection
        {
            Id = child.Item.Id,
            ProductType = child.ProductType,
            ParentProductId = child.ParentProductId,
            NhanhProductId = child.NhanhProductId,
            Stock = child.Stock
        }));

        var quantities = await CalculateAvailableQty(productsToCalculate, variantsArePreloaded: true);
        parent.SellableQuantity = quantities.TryGetValue(parent.Id, out var parentQuantity)
            ? parentQuantity.SellableQuantity
            : 0;

        foreach (var child in children)
        {
            child.Item.SellableQuantity = quantities.TryGetValue(child.Item.Id, out var childQuantity)
                ? childQuantity.SellableQuantity
                : 0;
        }

        return new Response.ProductFamilyResponse
        {
            Parent = parent,
            Children = children.Select(child => child.Item).ToList()
        };
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
            Description = product.Description,
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
            Position = product.Position,
            Factor = product.Factor,
            IsActive = product.IsActive,
            IsDeleted = product.IsDeleted,
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
            Description = product.Description,
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
            IsActive = product.IsActive,
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
            Materials = product.SubIds?
                .Where(cs => !cs.IsDeleted && cs.Mat != null && !cs.Mat.IsDeleted)
                .Select(cs => new Response.ComboSpecification
                {
                    Id = cs.Mat.Id,
                    ProductName = cs.Mat.ProductName,
                    CoverImage = cs.Mat.CoverImage,
                    Quantity = cs.Quantity ?? 0m,
                    Price = cs.Mat.Price ?? 0m
                })
                .ToList() ?? new List<Response.ComboSpecification>()
        };
    }

    private class ProductQtyProjection
    {
        public Guid Id { get; set; }
        public string? ProductType { get; set; }
        public string? ParentProductId { get; set; }
        public string? NhanhProductId { get; set; }
        public decimal? Stock { get; set; }
    }

    private static (int? SellableQuantity, string? StockStatus, string? UnavailableReason) GetQuantityInfo(
        HoaTheoMua.Repository.Entity.Product product,
        IReadOnlyDictionary<Guid, (int? SellableQuantity, string? StockStatus, string? UnavailableReason)> quantities)
    {
        if (quantities.TryGetValue(product.Id, out var quantityInfo))
            return quantityInfo;

        var stock = Math.Max(0, (int)(product.Stock ?? 0));
        return (stock, stock > 0 ? "Còn hàng" : "Hết hàng", null);
    }

    public async Task<Dictionary<Guid, (int? SellableQuantity, string? StockStatus, string? UnavailableReason)>> CalculateAvailableQty(List<Guid> productIds)
    {
        // Giai đoạn 1: Kết thúc sớm khi không có sản phẩm cần tính.
        if (productIds == null || productIds.Count == 0)
            return new Dictionary<Guid, (int?, string?, string?)>();

        // Giai đoạn 2: Loại ID trùng và chỉ lấy các trường phục vụ tính toán.
        var distinctProductIds = productIds.Distinct().ToList();
        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(p => distinctProductIds.Contains(p.Id) && !p.IsDeleted)
            .Select(p => new ProductQtyProjection
            {
                Id = p.Id,
                ProductType = p.ProductType,
                ParentProductId = p.ParentProductId,
                NhanhProductId = p.NhanhProductId,
                Stock = p.Stock
            })
            .ToListAsync();

        // Giai đoạn 3: Chuyển phần tính toán nghiệp vụ cho overload nội bộ.
        return await CalculateAvailableQty(products);
    }

    private async Task<Dictionary<Guid, (int? SellableQuantity, string? StockStatus, string? UnavailableReason)>> CalculateAvailableQty(
        IReadOnlyCollection<ProductQtyProjection> products,
        bool variantsArePreloaded = false)
    {
        // Giai đoạn 1: Khởi tạo kết quả và xác định combo trực tiếp/các sản phẩm cha.
        if (products.Count == 0)
            return new Dictionary<Guid, (int?, string?, string?)>();

        var result = new Dictionary<Guid, (int? SellableQuantity, string? StockStatus, string? UnavailableReason)>(products.Count);

        var directComboIds = products
            .Where(p => p.ProductType == "Combo")
            .Select(p => p.Id)
            .ToHashSet();

        var parentNhanhIds = products
            .Where(p => p.ProductType == "Sản phẩm" && string.IsNullOrEmpty(p.ParentProductId))
            .Select(p => p.NhanhProductId)
            .Where(id => !string.IsNullOrEmpty(id))
            .Select(id => id!)
            .ToHashSet();

        // Giai đoạn 2: Lấy combo biến thể; detail có thể tái sử dụng children đã nạp.
        List<ProductQtyProjection> comboVariants;
        if (parentNhanhIds.Count == 0)
        {
            comboVariants = new List<ProductQtyProjection>();
        }
        else if (variantsArePreloaded)
        {
            comboVariants = products
                .Where(p => p.ProductType == "Combo" && parentNhanhIds.Contains(p.ParentProductId!))
                .ToList();
        }
        else
        {
            comboVariants = await _dbContext.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted &&
                            p.ProductType == "Combo" &&
                            parentNhanhIds.Contains(p.ParentProductId!))
                .Select(p => new ProductQtyProjection
                {
                    Id = p.Id,
                    ParentProductId = p.ParentProductId
                })
                .ToListAsync();
        }

        var comboVariantsByParent = comboVariants.ToLookup(v => v.ParentProductId);

        var comboIdsToCalculate = directComboIds
            .Concat(comboVariants.Select(v => v.Id))
            .ToHashSet();

        // Giai đoạn 3: Lưu quantity và mã lý do không khả dụng cho từng combo.
        // Các mã no_formula, no_core, calculation_error là contract cho frontend, không phải message hiển thị.
        var comboQuantities = new Dictionary<Guid, (int Quantity, string? UnavailableReason)>(comboIdsToCalculate.Count);

        if (comboIdsToCalculate.Count > 0)
        {
            // Giai đoạn 4: Nạp công thức active/chưa xoá để phân biệt thiếu công thức và thiếu CORE.
            var allSpecs = await _dbContext.ComboSpecifications
                .AsNoTracking()
                .Where(cs => !cs.IsDeleted &&
                             cs.IsActive &&
                             comboIdsToCalculate.Contains(cs.SubId))
                .Select(cs => new { cs.SubId, cs.MatId, cs.Quantity, cs.IsCore })
                .ToListAsync();

            // Nhóm toàn bộ công thức theo combo để xác định combo có công thức hay không.
            var hasAnySpecsByCombo = allSpecs.ToLookup(s => s.SubId);

            // Chỉ giữ CORE spec có định mức dương; spec định mức <= 0 không ràng buộc được số lượng bán.
            var coreSpecsByCombo = new Dictionary<Guid, List<(Guid MatId, decimal Quantity)>>();
            foreach (var spec in allSpecs.Where(s => s.IsCore && s.Quantity.HasValue && s.Quantity.Value > 0))
            {
                if (!coreSpecsByCombo.TryGetValue(spec.SubId, out var specs))
                {
                    specs = new List<(Guid MatId, decimal Quantity)>();
                    coreSpecsByCombo[spec.SubId] = specs;
                }

                specs.Add((spec.MatId, spec.Quantity!.Value));
            }

            var materialIds = coreSpecsByCombo.Values
                .SelectMany(specs => specs)
                .Select(spec => spec.MatId)
                .ToHashSet();

            // Số lượng bán được của combo phải dựa trên AvailableForSale của nguyên liệu.
            // Với dữ liệu cũ chưa đồng bộ field này, fallback về Stock để không làm mất
            // khả năng bán của các sản phẩm lịch sử. Giá trị âm luôn được coi là 0.
            var inventories = new Dictionary<Guid, decimal>();
            if (materialIds.Count > 0)
            {
                inventories = await _dbContext.Products
                    .AsNoTracking()
                    .Where(p => materialIds.Contains(p.Id) && !p.IsDeleted)
                    .Select(p => new { p.Id, p.AvailableForSale, p.Stock })
                    .ToDictionaryAsync(
                        p => p.Id,
                        p => Math.Max(0m, p.AvailableForSale ?? p.Stock ?? 0m));
            }

            // Giai đoạn 5: Tính số combo bán được theo vật liệu CORE giới hạn nhất.
            foreach (var comboId in comboIdsToCalculate)
            {
                try
                {
                    var hasAnySpecs = hasAnySpecsByCombo[comboId].Any();

                    if (!hasAnySpecs)
                    {
                        comboQuantities[comboId] = (0, "no_formula");
                        continue;
                    }

                    // coreSpecsByCombo chỉ chứa CORE spec định mức dương nên thiếu key nghĩa là không có CORE dùng được.
                    if (!coreSpecsByCombo.TryGetValue(comboId, out var coreSpecsList) || coreSpecsList.Count == 0)
                    {
                        comboQuantities[comboId] = (0, "no_core");
                        continue;
                    }

                    var minQty = int.MaxValue;

                    foreach (var spec in coreSpecsList)
                    {
                        // Bảo vệ bổ sung: lượng khả dụng của vật liệu âm được coi là 0.
                        var availableForSale = Math.Max(0m, inventories.TryGetValue(spec.MatId, out var s) ? s : 0m);
                        var availableCombos = decimal.Floor(availableForSale / spec.Quantity);
                        var availableCombosAsInt = availableCombos >= int.MaxValue
                            ? int.MaxValue
                            : (int)availableCombos;
                        minQty = Math.Min(minQty, availableCombosAsInt);
                    }

                    comboQuantities[comboId] = (Math.Max(0, minQty), null);
                }
                catch
                {
                    // Lỗi tại một combo không làm gián đoạn việc tính các combo khác.
                    comboQuantities[comboId] = (0, "calculation_error");
                }
            }
        }

        // Giai đoạn 6: Chuyển kết quả combo thành kết quả cho từng sản phẩm đầu vào.
        foreach (var product in products)
        {
            int? quantity;
            string? stockStatus;
            string? unavailableReason = null;

            if (product.ProductType == "Combo")
            {
                var combo = comboQuantities.GetValueOrDefault(product.Id);
                quantity = combo.Quantity;
                unavailableReason = combo.UnavailableReason;

                if (unavailableReason == "calculation_error")
                {
                    stockStatus = null;  // null nghĩa là chưa tính được.
                }
                else if (quantity > 0)
                {
                    stockStatus = "Còn hàng";
                }
                else
                {
                    stockStatus = null;  // Frontend hiển thị nhãn riêng dựa trên mã lý do.
                }
            }
            else if (product.ProductType == "Sản phẩm" &&
                     string.IsNullOrEmpty(product.ParentProductId) &&
                     !string.IsNullOrEmpty(product.NhanhProductId))
            {
                var variants = comboVariantsByParent[product.NhanhProductId];
                if (variants.Any())
                {
                    // Các biến thể dùng cùng một tập nguyên liệu và chỉ khác định mức. Không cộng
                    // quantity vì như vậy sẽ đếm cùng một kho nhiều lần. Quantity của cha biểu diễn
                    // số lượng tối đa nếu chọn biến thể tiết kiệm nguyên liệu nhất.
                    var variantQuantities = variants
                        .Select(v => comboQuantities.GetValueOrDefault(v.Id))
                        .ToList();

                    quantity = variantQuantities.Max(combo => combo.Quantity);

                    if (quantity > 0)
                    {
                        stockStatus = "Còn hàng";
                        unavailableReason = null;
                    }
                    else
                    {
                        stockStatus = null;

                        // Chỉ giữ mã lý do khi mọi biến thể đều không bán được và có cùng lý do.
                        var reasons = variantQuantities
                            .Select(combo => combo.UnavailableReason)
                            .Distinct()
                            .ToList();
                        unavailableReason = reasons.Count == 1 ? reasons[0] : null;
                    }
                }
                else
                {
                    // Sản phẩm cha không có biến thể combo dùng tồn kho trực tiếp, có clamp âm.
                    quantity = Math.Max(0, (int)(product.Stock ?? 0));
                    stockStatus = quantity > 0 ? "Còn hàng" : "Hết hàng";
                }
            }
            else
            {
                // Sản phẩm thường hoặc biến thể không phải combo dùng tồn kho trực tiếp, có clamp âm.
                quantity = Math.Max(0, (int)(product.Stock ?? 0));
                stockStatus = quantity > 0 ? "Còn hàng" : "Hết hàng";
            }

            result[product.Id] = (quantity, stockStatus, unavailableReason);
        }

        return result;
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
                .OrderBy(x => x.Position)
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
            .OrderBy(x => x.Position)
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

    public async Task<List<Response.ProductResponseWithList>> GetProductWithList(Request.GetProductWithList request)
    {
        var queryProduct = _dbContext.Products.Where(x => request.ProductIds.Contains(x.Id) && !x.IsDeleted);

        var selectedQuery = queryProduct.Select(x => new Response.ProductResponseWithList()
        {
            Id = x.Id,
            CoverImage = x.CoverImage,
            ProductName = x.ProductName,
        });

        var result = await selectedQuery.ToListAsync();
        
        return result;
    }

    public async Task<List<Response.GetProductByIdsResponse>> GetProductByIds(Request.GetProductByIds request)
    {
        // Business rule của IsActive:
        // - Sản phẩm con chỉ active khi cả cha và chính nó đều active.
        // - Sản phẩm cha có con chỉ active khi chính nó và toàn bộ con chưa bị xóa đều active.
        // - Sản phẩm độc lập dùng IsActive của chính nó.
        var productIds = request?.ProductIds?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (productIds == null || productIds.Count == 0)
        {
            throw new BadRequestException("Product IDs are required");
        }

        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
            .Select(p => new { p.Id, p.ProductName, p.ProductType, p.ParentProductId, p.NhanhProductId, p.Stock, p.AvailableForSale, p.IsActive })
            .ToListAsync();

        if (products.Count == 0)
        {
            return new List<Response.GetProductByIdsResponse>();
        }

        // Với input là sản phẩm con, thu thập ID cha để tra trạng thái cha.
        // ParentProductId lưu NhanhProductId của cha, không phải Guid Id nội bộ.
        var parentNhanhProductIds = products
            .Where(p => !string.IsNullOrWhiteSpace(p.ParentProductId))
            .Select(p => p.ParentProductId!)
            .Distinct()
            .ToList();
        
        var parentActiveByNhanhProductId = parentNhanhProductIds.Count == 0
            ? new Dictionary<string, bool>()
            : await _dbContext.Products
                .AsNoTracking()
                .Where(p => p.NhanhProductId != null &&
                            parentNhanhProductIds.Contains(p.NhanhProductId))
                .ToDictionaryAsync(p => p.NhanhProductId!, p => p.IsActive);

        // Với input là sản phẩm cha hoặc sản phẩm độc lập, thu thập NhanhProductId
        // để kiểm tra xem chúng có sản phẩm con hay không.
        var requestedParentNhanhProductIds = products
            .Where(p => string.IsNullOrWhiteSpace(p.ParentProductId) &&
                        !string.IsNullOrWhiteSpace(p.NhanhProductId))
            .Select(p => p.NhanhProductId!)
            .Distinct()
            .ToList();

        // Lookup này chỉ chứa các sản phẩm thực sự có con.
        // Value = true khi toàn bộ con chưa bị xóa đều active; chỉ cần một con false thì value = false.
        var allChildrenActiveByParentNhanhProductId = new Dictionary<string, bool>();

        if (requestedParentNhanhProductIds.Count > 0)
        {
            // Nạp trạng thái con theo ParentProductId và bỏ qua các con đã bị xóa mềm.
            var childActiveStates = await _dbContext.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted &&
                            p.ParentProductId != null &&
                            requestedParentNhanhProductIds.Contains(p.ParentProductId))
                .Select(p => new { p.ParentProductId, p.IsActive })
                .ToListAsync();

            // Group theo cha và dùng All để áp dụng rule: mọi sản phẩm con đều phải active.
            allChildrenActiveByParentNhanhProductId = childActiveStates
                .GroupBy(p => p.ParentProductId!)
                .ToDictionary(group => group.Key, group => group.All(child => child.IsActive));
        }
        
        var quantities = await CalculateAvailableQty(products.Select(p => new ProductQtyProjection
        {
            Id = p.Id,
            ProductType = p.ProductType,
            ParentProductId = p.ParentProductId,
            NhanhProductId = p.NhanhProductId,
            Stock = p.Stock
        }).ToList());

        // Tạo lookup để map response đúng thứ tự productIds trong request.
        var productsById = products.ToDictionary(p => p.Id);

        return productIds
            .Where(productsById.ContainsKey)
            .Select(productId =>
            {
                var product = productsById[productId];
                var sellableQty = quantities.TryGetValue(productId, out var qtyInfo)
                    ? qtyInfo.SellableQuantity
                    : Math.Max(0, (int)(product.AvailableForSale ?? 0));

                // Mặc định dùng trạng thái của chính sản phẩm. Sản phẩm độc lập sẽ giữ giá trị này.
                var isActive = product.IsActive;

                // Input là con: cha false sẽ ép kết quả false; cha true thì dùng trạng thái của con.
                if (!string.IsNullOrWhiteSpace(product.ParentProductId) &&
                    parentActiveByNhanhProductId.TryGetValue(product.ParentProductId, out var parentIsActive))
                {
                    isActive = parentIsActive && product.IsActive;
                }
                // Input là cha có con: cha và toàn bộ con đều phải active.
                else if (!string.IsNullOrWhiteSpace(product.NhanhProductId) &&
                         allChildrenActiveByParentNhanhProductId.TryGetValue(
                             product.NhanhProductId,
                             out var allChildrenActive))
                {
                    isActive = product.IsActive && allChildrenActive;
                }
                
                return new Response.GetProductByIdsResponse
                {
                    Id = product.Id,
                    ProductName = product.ProductName,
                    SellableQuantity = sellableQty,
                    IsActive = isActive
                };
            })
            .ToList();
    }

    public async Task<Response.GetProductDetailByCustomerResponse> GetProductDetailByCustomer(Guid id)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Where(p => p.Id == id && !p.IsDeleted)
            .Select(p => new
            {
                p.Id,
                p.ProductName,
                p.CoverImage,
                p.Price,
                p.OldPrice,
                p.ProductType,
                p.ParentProductId,
                p.NhanhProductId,
                p.Stock,
                p.AvailableForSale,
                p.IsActive
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            // API detail cho customer luôn trả payload thành công. Khi sản phẩm không
            // tồn tại (hoặc đã bị xóa), trả response mặc định để các field rỗng thay
            // vì phát sinh lỗi PRODUCT_NOT_FOUND.
            return new Response.GetProductDetailByCustomerResponse();
        }

        // ParentProductId của Nhanh.vn là NhanhProductId của sản phẩm cha,
        // không phải Id (Guid) của entity Product.
        Response.ParentProductInfo? parent = null;
        if (!string.IsNullOrWhiteSpace(product.ParentProductId))
        {
            parent = await _dbContext.Products
                .AsNoTracking()
                .Where(p => p.NhanhProductId == product.ParentProductId &&
                            p.IsActive &&
                            !p.IsDeleted)
                .Select(p => new Response.ParentProductInfo
                {
                    Id = p.Id,
                    ProductName = p.ProductName,
                    NhanhProductId = p.NhanhProductId,
                    IsActive = p.IsActive
                })
                .FirstOrDefaultAsync();
        }

        var quantities = await CalculateAvailableQty(new List<ProductQtyProjection>
        {
            new()
            {
                Id = product.Id,
                ProductType = product.ProductType,
                ParentProductId = product.ParentProductId,
                NhanhProductId = product.NhanhProductId,
                Stock = product.Stock
            }
        });

        var sellableQuantity = quantities.TryGetValue(product.Id, out var quantityInfo)
            ? quantityInfo.SellableQuantity
            : Math.Max(0, (int)(product.AvailableForSale ?? 0));

        return new Response.GetProductDetailByCustomerResponse
        {
            Id = product.Id,
            ProductName = product.ProductName,
            CoverImage = product.CoverImage,
            Price = product.Price,
            OldPrice = product.OldPrice,
            NhanhProductId = product.NhanhProductId,
            SellableQuantity = sellableQuantity,
            IsActive = product.IsActive,
            Parent = parent
        };
    }

    public async Task<Response.ComboSpecificationResponse?> GetComboSpecificationAsync(Guid id)  
    {  
        var comboSpec = await _dbContext.ComboSpecifications  
            .AsNoTracking()  
            .Include(c => c.Sub)  
            .FirstOrDefaultAsync(c => c.SubId == id  
                                      && !c.IsDeleted  
                                      && !c.Sub.IsDeleted);  
  
        if (comboSpec == null)  
            return null;  
  
        var materialSpecs = await _dbContext.ComboSpecifications
            .AsNoTracking()  
            .Include(c => c.Mat)  
            .Where(c => c.SubId == id  
                        && !c.IsDeleted  
                        && !c.Mat.IsDeleted)  
            .ToListAsync();  
  
        var response = new Response.ComboSpecificationResponse  
        {  
            SubId = comboSpec.Sub.Id.ToString(),  
            ProductCode = comboSpec.Sub.ProductCode,  
            ProductName = comboSpec.Sub.ProductName,  
            CoverImage = comboSpec.Sub.CoverImage,
            Materials = materialSpecs.Select(c => new Response.MaterialItemResponse  
                {  
                    MaterialId = c.Mat.Id.ToString(),  
                    MaterialName = c.Mat.ProductName,  
                    CoverImage = c.Mat.CoverImage,
                    IsCore = c.IsCore, 
                    QuantityPerCombo = c.Quantity,  
                    Unit = c.Mat.Unit,  
                    AvailableForSale = c.Mat.AvailableForSale ?? 0,  
                    MaxComboPossible = (c.Mat.AvailableForSale ?? 0) > 0 && c.Quantity > 0  
                        ? Math.Floor((decimal)((c.Mat.AvailableForSale ?? 0) / c.Quantity))  
                        : 0  
                })  
                .OrderByDescending(x => x.IsCore)  
                .ToList()  
        };    return response;  
    }

    public async Task<BasePaginationResponse> GetMaterialsAsync(Request.GetMaterialsRequest request)
    {
        request.PageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
        request.PageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 100);

        var query = _dbContext.Products
            .AsNoTracking()
            .Where(product => !product.IsDeleted &&
                              product.ProductLabel != null &&
                              (("," + product.ProductLabel.ToLower().Replace(" ", "") + ",")
                                   .Contains(",vậtliệu,") ||
                               ("," + product.ProductLabel.ToLower().Replace(" ", "") + ",")
                                   .Contains(",phụliệu,")));

        if (request.ExcludeIds != null && request.ExcludeIds.Any())
        {
            query = query.Where(product => !request.ExcludeIds.Contains(product.Id));
        }

        if (!string.IsNullOrWhiteSpace(request.ProductLabel))
        {
            var productLabel = request.ProductLabel
                .Trim()
                .ToLowerInvariant()
                .Replace(" ", "");
            var productLabelToken = $",{productLabel},";

            query = query.Where(product =>
                product.ProductLabel != null &&
                ("," + product.ProductLabel.ToLower().Replace(" ", "") + ",")
                    .Contains(productLabelToken));
        }
        
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var normalizedSearch = request.Search.Trim().ToLower();

            query = query.Where(product =>
                (product.ProductName != null &&
                 product.ProductName.ToLower().Trim().Contains(normalizedSearch)) ||
                (product.ProductCode != null &&
                 product.ProductCode.ToLower().Trim().Contains(normalizedSearch)));
        }
        
        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(product => new Response.MaterialResponse
            {
                MaterialId = product.Id.ToString(),
                MaterialCode = product.ProductCode,
                MaterialName = product.ProductName,
                Unit = product.Unit,
                AvailableForSale = product.AvailableForSale ?? 0,
                CoverImage = product.CoverImage,
                ProductLabel = product.ProductLabel
            })
            .ToListAsync();

        return ApiResponseFactory.BasePagination(items, request.PageIndex, request.PageSize, totalItems);
    }

    public async Task<List<Response.InventoryCheckError>?> CheckInventoryAvailability(Request.CheckInventoryAvailabilityRequest request)
    {
        // Bước 1: Validate từng item trước khi gộp các dòng trùng product.
        if (request?.Items == null || request.Items.Count == 0)
        {
            throw new BadRequestException("Items are required");
        }

        if (request.Items.Any(x => x.ProductId == Guid.Empty || x.Quantity <= 0))
        {
            throw new BadRequestException("Each item must have a valid ProductId and Quantity greater than 0");
        }

        var requestedItems = request.Items
            .GroupBy(x => x.ProductId)
            .Select(g => new Request.CartItem
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .ToList();

        var productIds = requestedItems.Select(x => x.ProductId).ToList();

        // Bước 2: Lấy thông tin sản phẩm trong một query.
        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
            .Select(p => new { p.Id, p.ProductName, p.ProductType, p.NhanhProductId, p.IsActive })
            .ToListAsync();

        if (products.Count != productIds.Count)
        {
            throw new BadRequestException("Products not found");
        }

        var inactiveProductIds = products
            .Where(p => !p.IsActive)
            .Select(p => p.Id)
            .ToList();

        if (inactiveProductIds.Count > 0)
        {
            throw new BadRequestException("One or more products are inactive");
        }

        var productById = products.ToDictionary(p => p.Id);

        // Mặc định lấy công thức trực tiếp của product.
        // Nếu product có combo con theo cấu trúc Nhanh.vn thì dùng công thức của các combo con.
        var sourceIdsByProductId = products.ToDictionary(
            p => p.Id,
            p => new List<Guid> { p.Id });

        var parentNhanhIds = products
            .Where(p => !string.IsNullOrEmpty(p.NhanhProductId))
            .Select(p => p.NhanhProductId!)
            .ToList();

        if (parentNhanhIds.Count > 0)
        {
            var childCombos = await _dbContext.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted &&
                            p.IsActive &&
                            p.ProductType == "Combo" &&
                            parentNhanhIds.Contains(p.ParentProductId!))
                .Select(p => new { p.Id, p.ParentProductId })
                .ToListAsync();

            foreach (var product in products)
            {
                var childIds = childCombos
                    .Where(x => x.ParentProductId == product.NhanhProductId)
                    .Select(x => x.Id)
                    .ToList();

                if (childIds.Count > 0)
                {
                    sourceIdsByProductId[product.Id] = childIds;
                }
            }
        }

        var sourceIds = sourceIdsByProductId.Values.SelectMany(x => x).Distinct().ToList();

        var coreSpecs = await _dbContext.ComboSpecifications
            .AsNoTracking()
            .Where(cs => sourceIds.Contains(cs.SubId) &&
                         cs.IsCore &&
                         cs.IsActive &&
                         !cs.IsDeleted &&
                         cs.Quantity.HasValue &&
                         cs.Quantity > 0)
            .Select(cs => new { cs.SubId, cs.MatId, cs.Quantity })
            .ToListAsync();
        

        // Gắn mỗi spec với item trong giỏ để quantity của giỏ được tính vào.
        var requestedSpecs = requestedItems
            .SelectMany(item => coreSpecs
                .Where(spec => sourceIdsByProductId[item.ProductId].Contains(spec.SubId))
                .Select(spec => new
                {
                    Item = item,
                    Spec = spec
                }))
            .ToList();

        var materialRequirements = requestedSpecs
            .GroupBy(x => x.Spec.MatId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.Item.Quantity * x.Spec.Quantity!.Value));

        var materialIds = materialRequirements.Keys.ToList();
        var inventories = await _dbContext.Products
            .AsNoTracking()
            .Where(p => materialIds.Contains(p.Id) && !p.IsDeleted)
            .Select(p => new { p.Id, p.ProductName, p.AvailableForSale, p.IsActive })
            .ToListAsync();

        var inventoryById = inventories.ToDictionary(x => x.Id);

        var list = new List<Response.InventoryCheckError>();

        foreach (var (materialId, requiredQty) in materialRequirements)
        {
            inventoryById.TryGetValue(materialId, out var material);
            var availableQty = material?.IsActive == true
                ? Math.Max(0m, material.AvailableForSale ?? 0m)
                : 0m;
            if (requiredQty > availableQty)
            {
                var affectedProducts = requestedSpecs
                    .Where(x => x.Spec.MatId == materialId)
                    .GroupBy(x => x.Item.ProductId)
                    .Select(g => new Response.AffectedProductInfo
                    {
                        ProductId = g.Key,
                        ProductName = productById[g.Key].ProductName
                    })
                    .ToList();

                list.Add(new Response.InventoryCheckError
                {
                    MaterialId = materialId,
                    MaterialName = material?.ProductName ?? "Unknown",
                    RequiredQuantity = requiredQty,
                    AvailableQuantity = availableQty,
                    AffectedProducts = affectedProducts
                });
            }
        }

        return list.Count > 0 ? list : null;
    }

    public async Task<string> UpdateProductV2(Guid id, Request.UpdateProductRequestV2 request)
    {
        if (request.Description?.Length > 20000)
        {
            throw new BadRequestException("Description must not exceed 20000 characters.");
        }

        var product = await _dbContext.Products
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (product == null)
        {
            throw new BadRequestException("Product does not exist.");
        }

        if (request.Description != null)
        {
            product.Description = request.Description;
        }
        
        await _dbContext.SaveChangesAsync();

        return product.Id.ToString();
    }

    public async Task<List<Response.ProductByCategoryResponse>> GetProductsByCategories(List<Guid>? categoryIds)
    {
        var ids = categoryIds?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList() ?? new List<Guid>();

        if (ids.Count == 0)
        {
            return new List<Response.ProductByCategoryResponse>();
        }

        // Chỉ lấy sản phẩm cha (trang sản phẩm trên web là trang của cha), chưa xoá, đang bán
        return await _dbContext.Products
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive
                                     && (x.ParentProductId == null || x.ParentProductId == string.Empty)
                                     && x.CategoryProductDetails.Any(cpd => !cpd.IsDeleted && ids.Contains(cpd.CategoryProductId)))
            .OrderBy(x => x.ProductName)
            .Select(x => new Response.ProductByCategoryResponse
            {
                Id = x.Id,
                Name = x.ProductName,
                NhanhCreatedAt = x.NhanhCreatedAt,
                NhanhUpdatedAt = x.NhanhUpdatedAt
            })
            .ToListAsync();
    }

    public async Task<string> UpdateLabelProductNhanhVn(List<Request.UpdateLabelProductNhanhVnRequest> requests)
    {
        if (requests == null || requests.Count == 0)
        {
            throw new BadRequestException("Products are required");
        }

        var labelsByProductId = new Dictionary<Guid, List<string>>();

        foreach (var request in requests)
        {
            if (string.IsNullOrWhiteSpace(request.Label))
            {
                throw new BadRequestException("Label is required");
            }

            if (request.ProductIds == null || request.ProductIds.Count == 0)
            {
                throw new BadRequestException($"ProductIds are required for label {request.Label}");
            }

            var label = request.Label.Trim();

            foreach (var productIdValue in request.ProductIds.Distinct())
            {
                if (!Guid.TryParse(productIdValue, out var productId))
                {
                    throw new BadRequestException($"ProductId is invalid: {productIdValue}");
                }

                if (!labelsByProductId.TryGetValue(productId, out var labels))
                {
                    labels = new List<string>();
                    labelsByProductId[productId] = labels;
                }

                if (!labels.Contains(label, StringComparer.OrdinalIgnoreCase))
                {
                    labels.Add(label);
                }
            }
        }

        var productIds = labelsByProductId.Keys.ToList();
        var products = await _dbContext.Products
            .Where(product => productIds.Contains(product.Id) && !product.IsDeleted)
            .ToListAsync();

        if (products.Count != productIds.Count)
        {
            throw new BadRequestException("One or more products were not found or have been deleted");
        }

        var updatedAt = DateTimeOffset.UtcNow;
        foreach (var product in products)
        {
            product.ProductLabel = string.Join(", ", labelsByProductId[product.Id]);
            product.UpdatedAt = updatedAt;
        }

        await _dbContext.SaveChangesAsync();

        return "Cập nhật nhãn cho sản phẩm thành công";
    }
}
