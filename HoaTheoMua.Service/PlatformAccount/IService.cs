namespace HoaTheoMua.Service.PlatformAccount;

public interface IService
{
    Task<List<Response.Account>> List(string? platform = null, CancellationToken ct = default);
    Task<Response.Connection> Connect(string platform, string browserProof, Guid? accountId = null, CancellationToken ct = default);
    Task<Response.Connection> Reconnect(Guid id, string browserProof, CancellationToken ct = default);
    Task<List<Response.Account>> Callback(string platform, string? state, string? code, string? error, string? browserProof, CancellationToken ct = default);
    Task<Response.Account> Refresh(Guid id, CancellationToken ct = default);
    Task<Response.Account> Disconnect(Guid id, CancellationToken ct = default);
}
