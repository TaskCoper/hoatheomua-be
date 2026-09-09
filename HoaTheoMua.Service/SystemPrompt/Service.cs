using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.SystemPrompt;

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> CreateSystemPromptDefault(Request.SystemPromptRequest request)
    {
        var query = _dbContext.SystemPrompts.Where(x => request.Type == x.Type && !x.IsDeleted);

        var isExit = await query.AnyAsync();

        if (isExit)
        {
            throw new ConflictException("System Prompt đã tồn tại.");
        }
        
        var now = DateTimeOffset.UtcNow;

        var newSystemPrompt = new Repository.Entity.SystemPrompt()
        {
            Id = Guid.NewGuid(),
            Content = request.Content,
            Type = request.Type,
            CreatedAt = now,
            UpdatedAt = now //Business rule
        };
        
        _dbContext.SystemPrompts.Add(newSystemPrompt);
        await _dbContext.SaveChangesAsync();
        return "Thêm mới System Prompt thành công.";
    }

    public async Task<List<Response.AllSystemPromptResponse>> GetSystemPrompt()
    {
        var query = _dbContext.SystemPrompts
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        query = query.OrderByDescending(x => x.UpdatedAt);

        var selectedQuery = query.Select(x => new Response.AllSystemPromptResponse()
        {
            Id = x.Id,
            Type = x.Type
        });

        var result = await selectedQuery.ToListAsync();
        return result;
    }

    public async Task<Response.SystemPromptResponse> GetSystemPromptInfo(Guid id)
    {
        var query = _dbContext.SystemPrompts
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted);

        var selectedQuery = query.Select(x => new Response.SystemPromptResponse()
        {
            Id = x.Id,
            Content =  x.Content,
            Type = x.Type
        });
        
        var result = await selectedQuery
                         .AsNoTracking()
                         .FirstOrDefaultAsync()
                     ?? throw new NotFoundException("System Prompt không còn tồn tại");

        return result;
    }
    
    public async Task<Response.SystemPromptResponse> UpdateSystemPrompt(Guid id, Request.UpdateSystemPromptRequest request)
    {
        var query = _dbContext.SystemPrompts.Where(x => x.Id == id && !x.IsDeleted);

        var systemPrompt = await query.FirstOrDefaultAsync();

        if (systemPrompt == null)
        {
            throw new NotFoundException("System prompt không tồn tại.");
        }

        systemPrompt.Content = request.Content!;
        systemPrompt.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        var systemPromptResponse = new Response.SystemPromptResponse()
        {
            Id = systemPrompt.Id,
            Content = systemPrompt.Content!,
            Type = systemPrompt.Type
        };

        return systemPromptResponse;
    }
}