using System.ComponentModel.DataAnnotations.Schema;
using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class Config: BaseEntity<Guid>, IAuditableEntity
{

    public required string Key { get; set; }
    [Column(TypeName = "jsonb")]
    public required string Value { get; set; }
    public bool IsPublic { get; set; }

    // External user id (from taskcoper-be's token); no FK — this service has no User table.
    public Guid UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}