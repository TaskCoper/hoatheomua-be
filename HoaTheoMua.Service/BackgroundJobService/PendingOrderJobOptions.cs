namespace HoaTheoMua.Service.BackgroundJobService;

public sealed class PendingOrderJobOptions
{
    public const string SectionName = "PendingOrderJob";

    public int TimeoutMinutes { get; set; } = 15;
    public int IntervalMinutes { get; set; } = 1;
}
