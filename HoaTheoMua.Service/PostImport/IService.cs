namespace HoaTheoMua.Service.PostImport;

public interface IService
{
    Task<Response.Settings> GetSettings(CancellationToken ct = default);
    Task<Response.Settings> SaveSettings(Request.Settings request, CancellationToken ct = default);
    Task<Response.Summary> Import(Stream stream, string fileName, string requestKey, CancellationToken ct = default);
    Task<IReadOnlyList<Response.Summary>> List(CancellationToken ct = default);
    Task<Response.Detail> Get(Guid id, int pageIndex = 1, int pageSize = 50, CancellationToken ct = default);
    Task<byte[]> Export(Guid id, CancellationToken ct = default);
}
