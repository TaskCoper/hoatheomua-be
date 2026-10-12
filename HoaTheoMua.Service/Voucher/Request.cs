namespace HoaTheoMua.Service.Voucher;

public class Request
{
    public class CreateVoucherRequest
    {
        // Send Code (manual) OR CodePrefix + CodeLength + CodeSuffix (generated), not both
        public string? Code { get; set; }
        public string? CodePrefix { get; set; }
        public int? CodeLength { get; set; }
        public string? CodeSuffix { get; set; }

        public string? Description { get; set; }
        public string? Visibility { get; set; }             // public | private
        public string? DiscountType { get; set; }           // cash | percent
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }     // only for percent
        public int MaxUsageCount { get; set; }
        public DateTimeOffset? StartAt { get; set; }
        public DateTimeOffset? EndAt { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxOrderAmount { get; set; }
    }

    public class SetVouchersActiveRequest
    {
        public List<Guid> Ids { get; set; } = new();
        public bool? IsActive { get; set; }
    }

    public class UpdateVouchersRequest
    {
        public List<UpdateVoucherItem> Items { get; set; } = new();
    }

    public class UpdateVoucherItem
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string? Visibility { get; set; }
        public string? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; } // new value, not sent = null
        public int? MaxUsageCount { get; set; }
        public DateTimeOffset? StartAt { get; set; }
        public DateTimeOffset? EndAt { get; set; } // new value, not sent = null
        public decimal? MinOrderAmount { get; set; } // new value, not sent = null
        public decimal? MaxOrderAmount { get; set; } // new value, not sent = null
    }

    public class EstimateVoucherRequest
    {
        public string? code { get; set; }
        public decimal productFee { get; set; }
    }
}

public static class VoucherDiscountTypes
{
    public const string Cash = "cash";
    public const string Percent = "percent";
}

public static class VoucherVisibilities
{
    public const string Public = "public";
    public const string Private = "private";
}
