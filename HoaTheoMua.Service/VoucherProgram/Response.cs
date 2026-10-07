namespace HoaTheoMua.Service.VoucherProgram;

public class Response
{
    public class GenerateProgramVouchersResponse
    {
        public Guid ProgramId { get; set; }
        public int CreatedCount { get; set; }
        public List<string> Codes { get; set; } = new();
    }

    public class VoucherProgramResponse
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxOrderAmount { get; set; }
        public int VoucherCount { get; set; }
        public int UsedVoucherCount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
