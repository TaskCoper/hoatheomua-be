using HoaTheoMua.Repository.Enum;

namespace HoaTheoMua.Repository.Entity;

public class ClientHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ClientHistoryType Type { get; set; }
    public string? SystemForm { get; set; }
    public string Metadata { get; set; } = null!;
    public Guid RootID { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid BaseId { get; set; }
    public Guid OutputId { get; set; }
}
