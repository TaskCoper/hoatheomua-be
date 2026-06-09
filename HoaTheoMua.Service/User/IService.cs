namespace HoaTheoMua.Service.User;

public interface IService
{
    Task<Response.GetMeResponse> GetMe();
}