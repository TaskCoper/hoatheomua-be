namespace HoaTheoMua.Service.VoucherProgram;

public class Request
{
    public class CreateVoucherProgramRequest
    {
        public required string Name { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxOrderAmount { get; set; }
    }

    public class GetVoucherProgramsRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Keyword { get; set; }
        public bool? IsActive { get; set; }
    }
}
