namespace HoaTheoMua.Service.Voucher;

public class Response
{
    public class VoucherResponse
    {
        public Guid Id { get; set; }
        // public Guid? ProgramId { get; set; }
        // public string? ProgramName { get; set; }
        public required string Code { get; set; }
        public string? Description { get; set; }
        public required string Visibility { get; set; }       // public | private
        public bool IsActive { get; set; }
        public required string DiscountType { get; set; }     // cash | percent
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public int MaxUsageCount { get; set; }
        public int UsedCount { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset? EndAt { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxOrderAmount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }

    public class VoucherExportFile
    {
        public required byte[] Content { get; set; }
        public required string FileName { get; set; }
    }

    // One Excel row; property order = column order (19 columns, docs section 10.2)
    public class VoucherExportRow
    {
        public required string Id { get; set; }
        public string? ProgramId { get; set; }
        public string? ProgramName { get; set; }
        public required string Code { get; set; }
        public string? Description { get; set; }
        public required string Visibility { get; set; }       // public | private
        public required string IsActive { get; set; }         // true | false
        public required string DiscountType { get; set; }     // cash | percent
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public int MaxUsageCount { get; set; }
        public int UsedCount { get; set; }
        public required string StartAt { get; set; }          // effective value, ISO 8601 UTC
        public string? EndAt { get; set; }                    // effective value, ISO 8601 UTC; blank when unlimited
        public decimal? MinOrderAmount { get; set; }          // effective value
        public decimal? MaxOrderAmount { get; set; }          // effective value
        public required string CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
        public required string IsDeleted { get; set; }        // always false
    }

    public class PublicVoucherResponse
    {
        public Guid Id { get; set; }
        public required string Code { get; set; }
        public string? Description { get; set; }
        public required string DiscountType { get; set; }     // cash | percent
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public DateTimeOffset? EndAt { get; set; }
        public decimal DiscountAmount { get; set; } // preview on productFee, 0 when productFee is not sent
    }

    public class EstimateVoucherResponse
    {
        public Guid VoucherId { get; set; }
        public required string Code { get; set; }
        public string? Description { get; set; }
        public required string DiscountType { get; set; }     // cash | percent
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ProductFeeAfterDiscount { get; set; }
    }

    public class VoucherUsageResponse
    {
        public Guid OrderId { get; set; }
        public Guid UserId { get; set; }
        public required string CustomerName { get; set; }     // Order.FullName
        public required string CustomerPhone { get; set; }    // Order.Phone
        public decimal DiscountAmount { get; set; }           // VoucherSnapshot.DiscountAmount
        public DateTimeOffset? UsedAt { get; set; }            // earliest Transaction.CreatedAt of the order
    }

    public class FilterVoucherResponse : EstimateVoucherResponse
    {
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxOrderAmount { get; set; }
        public bool IsValid { get; set; }
    }
}
