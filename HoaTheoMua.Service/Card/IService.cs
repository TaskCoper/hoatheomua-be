namespace HoaTheoMua.Service.Card;

public interface IService
{
    Task<HoaTheoMua.Service.Models.BasePaginationResponse> GetAdminGeneratedCards(
        Request.AdminGeneratedCardListRequest request,
        CancellationToken cancellationToken = default);
    public Task<Response.CreateCardResponse> CreateHandMadeCard(Request.CreateCardRequest request);
    public Task<Response.CreateCardResponse> CreateCardAi(Request.CreateCardAiRequest request);
    public Task<string> UpdateFinalGeneratedCardId(Request.UpdateFinalGeneratedCardId request);
    public Task<Response.OrderDetailCardPreviewResponse> GetCardByOrderDetailId(
        Guid orderDetailId,
        Guid virtualId);
    public Task<List<Response.ProductGeneratedCardsResponse>> GetGeneratedCardsByProductIds(
        Request.GetGeneratedCardsByProductIdsRequest request);
}
