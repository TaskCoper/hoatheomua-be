using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class PlatformStandard
{
    public Guid Id { get; set; }
    public PlatformType Platform { get; set; }
    public string Description { get; set; } = null!;
}
