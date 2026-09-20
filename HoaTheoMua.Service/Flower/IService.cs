namespace HoaTheoMua.Service.Flower;

public interface IService
{
    Task<HoaTheoMua.Service.Models.BasePaginationResponse> GetCustomerGeneratedFlowers(
        Request.CustomerGeneratedFlowerListRequest request,
        CancellationToken cancellationToken = default);
    Task<HoaTheoMua.Service.Models.BasePaginationResponse> GetAdminGeneratedFlowers(
        Request.AdminGeneratedFlowerListRequest request,
        CancellationToken cancellationToken = default);
    Task<Response.GeneratedFlowerResponse> CreateAiFlower(Request.CreateFlowerRequest request);
    Task<Response.GeneratedFlowerResponse> RegenerateAiFlower(Guid sourceFlowerId, Request.RegenerateFlowerRequest request);
    Task<Response.GeneratedFlowerCompositionResponse?> GetGeneratedFlowerComposition(Guid generatedFlowerId);
}
