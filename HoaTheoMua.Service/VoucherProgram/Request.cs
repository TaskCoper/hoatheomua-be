namespace HoaTheoMua.Service.VoucherProgram;

public class Request
{
    public class UpdateVoucherProgramRequest
    {
        public string? Name { get; set; }
        public string? StartAt { get; set; }
        public string? EndAt { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxOrderAmount { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GenerateProgramVouchersRequest
    {
        public string? CodePrefix { get; set; }
        public int CodeLength { get; set; }
        public string? CodeSuffix { get; set; }
        public int Quantity { get; set; }
        public int MaxUsageCount { get; set; }
        public required string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public string? Description { get; set; }
    }

    public class CreateVoucherProgramRequest
    {
        public required string Name { get; set; }
        public required string StartAt { get; set; }
        public required string EndAt { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxOrderAmount { get; set; }
    }

    public class GetVoucherProgramsRequest
    {
        public int pageIndex { get; set; } = 1;
        public int pageSize { get; set; } = 20;
        public string? keyword { get; set; }
        public bool? isActive { get; set; }
        public bool? isDelete { get; set; }
    }
}
