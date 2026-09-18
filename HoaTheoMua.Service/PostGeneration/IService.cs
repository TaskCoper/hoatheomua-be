namespace HoaTheoMua.Service.PostGeneration;

public interface IService
{
    Task<Response.OfficialVersion> GetOfficialVersion(Guid id, string platform, CancellationToken cancellationToken = default);
    Task<Response.ImageVersion> RegenerateImage(Guid id, Request.RegenerateImageRequest request, CancellationToken cancellationToken = default);
    Task<Response.ImageHistory> GetImageVersions(Guid id, CancellationToken cancellationToken = default);
    Task<PostCore.Response.PostCoreDetail> UpdatePlatformImages(Guid id, string platform, Request.UpdatePlatformImagesRequest request, CancellationToken cancellationToken = default);
    Task<PostCore.Response.PostCoreDetail> GeneratePlatformPosts(Guid id, Request.GenerateRequest request, CancellationToken cancellationToken = default);
    Task<PostCore.Response.PostCoreDetail> RegeneratePlatformContent(Guid id, string platform, Request.RegenerateContentRequest request, CancellationToken cancellationToken = default);
    Task<PostCore.Response.PostCoreDetail> EditPlatformContent(Guid id, string platform, Request.EditContentRequest request, CancellationToken cancellationToken = default);
    Task<Response.ContentHistory> GetContentVersions(Guid id, string platform, CancellationToken cancellationToken = default);
    Task<PostCore.Response.PostCoreDetail> SelectOfficialContent(Guid id, string platform, Request.SelectContentRequest request, CancellationToken cancellationToken = default);
}
