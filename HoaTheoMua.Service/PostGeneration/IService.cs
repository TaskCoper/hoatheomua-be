namespace HoaTheoMua.Service.PostGeneration;

public interface IService
{
    Task<PostCore.Response.PostCoreDetail> GeneratePlatformPosts(Guid id, Request.GenerateRequest request, CancellationToken cancellationToken = default);
    Task<PostCore.Response.PostCoreDetail> RegeneratePlatformContent(Guid id, string platform, Request.RegenerateContentRequest request, CancellationToken cancellationToken = default);
    Task<PostCore.Response.PostCoreDetail> EditPlatformContent(Guid id, string platform, Request.EditContentRequest request, CancellationToken cancellationToken = default);
    Task<Response.ContentHistory> GetContentVersions(Guid id, string platform, CancellationToken cancellationToken = default);
    Task<PostCore.Response.PostCoreDetail> SelectOfficialContent(Guid id, string platform, Request.SelectContentRequest request, CancellationToken cancellationToken = default);
}
