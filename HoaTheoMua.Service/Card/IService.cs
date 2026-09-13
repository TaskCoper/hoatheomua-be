namespace HoaTheoMua.Service.Card;

public interface IService
{
    public Task<Response.CreateCardResponse> CreateHandMadeCard(Request.CreateCardRequest request);
    public Task<Response.CreateCardResponse> CreateCardAi(Request.CreateCardAiRequest request);
}