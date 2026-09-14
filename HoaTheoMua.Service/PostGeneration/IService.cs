namespace HoaTheoMua.Service.PostGeneration;

public interface IService
{
    Task<PostCore.Response.PostCoreDetail> GeneratePlatformPosts(Guid id, Request.GenerateRequest request, CancellationToken cancellationToken = default);
}
