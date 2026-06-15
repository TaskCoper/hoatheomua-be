using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class ConfigDynamic: BaseEntity<Guid>, IAuditableEntity
{

    public required string Key { get; set; }
    public required string Value { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}