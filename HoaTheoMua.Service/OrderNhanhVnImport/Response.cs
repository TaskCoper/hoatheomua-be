using MiniExcelLibs.Attributes;

namespace HoaTheoMua.Service.OrderNhanhVnImport;

public class Response
{
    public class ImportResult
    {
        public int TotalRecords { get; set; }
        public int InsertedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int ErrorCount { get; set; }
    }
}