using System.Text.Json;

namespace HoaTheoMua.Service.Revalidate;

public interface IService
{
    Task<string> RevalidateApi(JsonElement body);
}