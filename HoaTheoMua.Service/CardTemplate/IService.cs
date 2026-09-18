using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.CardTemplate;

public interface IService
{
    public Task<BasePaginationResponse> GetCardTemplate(int pageIndex, int pageSize, bool? isActive, string? templateType, string? search = null);
    public Task<Response.CardTemplateResponse> GetCardTemplateDetail(Guid id);
    public Task<Response.CardTemplateResponse> CreateCardTemplate(Request.CreateCartTemplateRequest request);
    public Task<Response.CardTemplateResponse> UpdateCardTemplate(Guid id, Request.UpdateCardTemplateRequest request);
    public Task<string> DeleteCardTemplate(Guid id);
    
}
