using HoaTheoMua.Repository.Abtraction;
using Quartz.Impl.Calendar;

namespace HoaTheoMua.Repository.Entity;

public class OrderDetails: BaseEntity<Guid>, IAuditableEntity
{
    public string Name { get; set; }
    public int ItemId { get; set; }
    public Guid ProductId { get; set; }
    public string? Metadata { get; set; }
    public int Number { get; set; }
    public decimal Price { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
    public double Length { get; set; }
    public double Width { get; set; }
    public Order Order { get; set; }
    public Guid OrderId { get; set; }
    public string? SnapshotCard { get; set; }
    public Guid? GeneratedFlower { get; set; }
    public Guid? GeneratedCard { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
