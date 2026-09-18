namespace HoaTheoMua.Service.Flower;

public interface IService
{
    Task<Response.GeneratedFlowerResponse> CreateAiFlower(Request.CreateFlowerRequest request);
    Task<Response.GeneratedFlowerResponse> RegenerateAiFlower(Guid sourceFlowerId, Request.RegenerateFlowerRequest request);
}
