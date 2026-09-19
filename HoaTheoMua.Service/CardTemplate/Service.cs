using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using HoaTheoMua.Service.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.CardTemplate;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContext)
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }

    public async Task<BasePaginationResponse> GetCardTemplate(int pageIndex, int pageSize, bool? isActive, string? templateType, string? search = null,
        List<Guid>? productIds = null)
    {
        pageIndex = pageIndex <= 0 ? 1 : pageIndex;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        CardTemplateType? parsedTemplateType = null;
        if (!string.IsNullOrWhiteSpace(templateType))
        {
            var normalizedTemplateType = templateType.Trim();
            if (normalizedTemplateType != "ai" && normalizedTemplateType != "handmade" ||
                !Enum.TryParse<CardTemplateType>(normalizedTemplateType, ignoreCase: true, out var parsedType))
            {
                throw new BadRequestException("CARD_TEMPLATE_TYPE_INVALID");
            }

            parsedTemplateType = parsedType;
        }

        var isAdmin = _httpContext.HttpContext?.User.IsInRole("Admin") == true;
        var query = _dbContext.CardTemplates
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (isAdmin && isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }
        else if (!isAdmin)
        {
            if (!parsedTemplateType.HasValue)
            {
                throw new BadRequestException("CARD_TEMPLATE_TYPE_REQUIRED");
            }

            query = query.Where(x => x.IsActive);
        }

        if (parsedTemplateType.HasValue)
        {
            query = query.Where(x => x.TemplateType == parsedTemplateType.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(normalizedSearch));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new Response.CardTemplateResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                TemplateType = x.TemplateType == CardTemplateType.Ai ? "ai" : "handmade",
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        var productIdsToFind = productIds?.Where(id => id != Guid.Empty).Distinct().ToArray() ?? [];
        var products = productIdsToFind.Length == 0
            ? []
            : await _dbContext.Products.AsNoTracking()
                .Where(product => productIdsToFind.Contains(product.Id) && !product.IsDeleted)
                .Select(product => new { product.Id, product.ProductName })
                .ToListAsync();

        var productCardHistories = productIdsToFind.Length == 0
            ? []
            : await _dbContext.ClientHistories.AsNoTracking()
                .Where(history => productIdsToFind.Contains(history.RootId)
                    && (history.Type == ClientHistoryType.Card || history.Type == ClientHistoryType.HandmadeCard))
                .Select(history => new { history.RootId, history.OutputId })
                .ToListAsync();

        var flowerHistories = productIdsToFind.Length == 0
            ? []
            : await _dbContext.ClientHistories.AsNoTracking()
                .Where(history => history.Type == ClientHistoryType.Flower && productIdsToFind.Contains(history.RootId))
                .Select(history => new { ProductId = history.RootId, GeneratedFlowerId = history.OutputId })
                .ToListAsync();
        var flowerProductById = flowerHistories
            .GroupBy(history => history.GeneratedFlowerId)
            .ToDictionary(group => group.Key, group => group.First().ProductId);
        var flowerIds = flowerProductById.Keys.ToArray();
        var flowerCardHistories = flowerIds.Length == 0
            ? []
            : await _dbContext.ClientHistories.AsNoTracking()
                .Where(history => flowerIds.Contains(history.RootId)
                    && (history.Type == ClientHistoryType.Card || history.Type == ClientHistoryType.HandmadeCard))
                .Select(history => new { history.RootId, history.OutputId })
                .ToListAsync();

        var cardLinks = productCardHistories
            .Select(history => new { ProductId = history.RootId, CardId = history.OutputId })
            .Concat(flowerCardHistories.Select(history => new
            {
                ProductId = flowerProductById[history.RootId],
                CardId = history.OutputId
            }))
            .Distinct()
            .ToList();
        var cardIds = cardLinks.Select(link => link.CardId).Distinct().ToArray();
        var cardsById = cardIds.Length == 0
            ? new Dictionary<Guid, Repository.Entity.GeneratedCard>()
            : (await _dbContext.GeneratedCards.AsNoTracking()
                    .Where(card => cardIds.Contains(card.Id))
                    .ToListAsync())
                .ToDictionary(card => card.Id);

        var generatedCardsByProduct = cardLinks
            .Where(link => cardsById.ContainsKey(link.CardId))
            .GroupBy(link => link.ProductId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(link => cardsById[link.CardId])
                    .OrderByDescending(card => card.CreatedAt)
                    .ThenByDescending(card => card.Id)
                    .Select(card => new Response.GeneratedCardResponse { Id = card.Id, Name = card.Content })
                    .ToList());

        var response = ApiResponseFactory.BasePagination(items, pageIndex, pageSize, totalCount);
        return new CardTemplatePaginationResponse
        {
            IsSuccess = response.IsSuccess,
            IsFailed = response.IsFailed,
            Error = response.Error,
            TraceId = response.TraceId,
            TimestampUtc = response.TimestampUtc,
            Value = response.Value,
            Products = products.Select(product => new Response.ProductGeneratedCardResponse
            {
                ProductId = product.Id,
                ProductName = product.ProductName ?? string.Empty,
                GeneratedCards = generatedCardsByProduct.GetValueOrDefault(product.Id) ?? []
            }).ToList()
        };
    }

    public async Task<Response.CardTemplateResponse> GetCardTemplateDetail(Guid id)
    {
        var template = await _dbContext.CardTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (template is null)
        {
            throw new NotFoundException("CARD_TEMPLATE_NOT_FOUND");
        }

        var isAdmin = _httpContext.HttpContext?.User.IsInRole("Admin") == true;
        if (!isAdmin && !template.IsActive)
        {
            throw new ConflictException("CARD_TEMPLATE_INACTIVE");
        }

        return new Response.CardTemplateResponse
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            ImageUrl = template.ImageUrl,
            TemplateType = template.TemplateType == CardTemplateType.Ai ? "ai" : "handmade",
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }

    public async Task<Response.CardTemplateResponse> CreateCardTemplate(Request.CreateCartTemplateRequest request)
    {
        if (!Enum.TryParse<CardTemplateType>(request.TemplateType, ignoreCase: true, out var templateType))
        {
            throw new BadRequestException("CARD_TEMPLATE_TYPE_INVALID");
        }

        var newCardTemplate = new Repository.Entity.CardTemplate()
        {
            Id = Guid.NewGuid(),
            Name = request.Name!,
            Description = request.Description,
            ImageUrl = request.ImageUrl!,
            TemplateType = templateType,
            IsActive = false, //
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        _dbContext.CardTemplates.Add(newCardTemplate);
        await _dbContext.SaveChangesAsync();

        var cardTemplateResponse = new Response.CardTemplateResponse()
        {
            Id = newCardTemplate.Id,
            Name = newCardTemplate.Name,
            Description = newCardTemplate.Description,
            ImageUrl = newCardTemplate.ImageUrl!,
            TemplateType = newCardTemplate.TemplateType.ToString().ToLowerInvariant(),
            IsActive = newCardTemplate.IsActive,
            CreatedAt = newCardTemplate.CreatedAt,
            UpdatedAt = newCardTemplate.UpdatedAt,
        };

        return cardTemplateResponse;
    }

    public async Task<Response.CardTemplateResponse> UpdateCardTemplate(Guid id, Request.UpdateCardTemplateRequest request)
    {
        if (request.Name is null && request.Description is null && request.ImageUrl is null && !request.IsActive.HasValue)
        {
            throw new BadRequestException("CARD_TEMPLATE_UPDATE_REQUIRED");
        }

        var cardTemplate = await _dbContext.CardTemplates.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (cardTemplate == null)
        {
            throw new NotFoundException("TEMPLATE_NOT_FOUND");
        }
        
        if (request.Name is not null)
            cardTemplate.Name = request.Name;

        if (request.Description is not null)
            cardTemplate.Description = request.Description;

        if (request.IsActive.HasValue)
            cardTemplate.IsActive = request.IsActive.Value;
        
        cardTemplate.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        var cardTemplateResponse = new Response.CardTemplateResponse()
        {
            Id = cardTemplate.Id,
            Name = cardTemplate.Name!,
            Description = cardTemplate.Description,
            ImageUrl = cardTemplate.ImageUrl!,
            TemplateType = cardTemplate.TemplateType.ToString().ToLowerInvariant(),
            IsActive = cardTemplate.IsActive,
            IsDeleted = cardTemplate.IsDeleted,
            CreatedAt = cardTemplate.CreatedAt,
            UpdatedAt = cardTemplate.UpdatedAt
        };
        
        return cardTemplateResponse;
    }

    public async Task<string> DeleteCardTemplate(Guid id)
    {
        var cardTemplate = await _dbContext.CardTemplates.FirstOrDefaultAsync(x => x.Id == id);

        if (cardTemplate == null)
        {
            throw new NotFoundException("CARD_TEMPLATE_NOT_FOUND");
        }

        if (cardTemplate.IsDeleted)
        {
            throw new BadRequestException("CARD_TEMPLATE_ALREADY_DELETED");
        }

        cardTemplate.IsDeleted = true;
        cardTemplate.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return "Đã xóa mẫu thiệp thành công.";
    }
    private sealed class CardTemplatePaginationResponse : BasePaginationResponse
    {
        public List<Response.ProductGeneratedCardResponse> Products { get; set; } = [];
    }
}
