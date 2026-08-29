namespace HoaTheoMua.Service.OrderNhanhVnImport;

public interface IService
{
    Task<Response.ImportResult> ImportFromExcelOrderNhanhVn(Stream fileStream);

}