namespace HoaTheoMua.Service.Auth;

public interface IService
{
    Task<Response> LoginAsync(
        Request request
    );
}