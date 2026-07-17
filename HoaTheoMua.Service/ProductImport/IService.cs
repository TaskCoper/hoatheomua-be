using System.IO;
using System.Threading.Tasks;
using HoaTheoMua.Service.ProductImport;

namespace HoaTheoMua.Service.ProductImport;

public interface IService
{
    Task<Response.ImportResult> ImportFromExcel(Stream fileStream);
}
