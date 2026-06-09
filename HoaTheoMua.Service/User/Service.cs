using HoaTheoMua.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.User;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid CheckAccessToken()
    {
        var accessToken = _httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
        if (accessToken != null)
        {
            return Guid.Parse(accessToken);
        }

        return Guid.Empty;
    }

    public async Task<Response.GetMeResponse> GetMe()
    {
        var userId = CheckAccessToken();
        if (userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Access token is invalid");
        }

        var query = await _dbContext.Users
            .Where(x => x.Id == userId)
            .Select(y => new Response.GetMeResponse()
            {
                Email = y.Email,
                FirstName =  y.FirstName,
                LastName = y.LastName,
                Avatar =  y.Avatar,
            }).FirstOrDefaultAsync();

        if (query == null)
        {
            throw new BadHttpRequestException("User not found");
        }
        
        return query;
    }
}