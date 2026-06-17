using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class Config: BaseEntity<Guid>, IAuditableEntity
{

    public Guid Id { get; set; }
    public required string Key { get; set; }
    public required JsonElement Value { get; set; }
    public User User { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } 
}