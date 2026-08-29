using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class CategoryNews: BaseEntity<Guid>, IAuditableEntity
{
    public required string Title { get; set; }

    // External user id (from taskcoper-be's token); no FK — this service has no User table.
    public Guid UserId { get; set; }
    public CategoryNews? ParentCategoryNews{ get; set; }
    public Guid? ParentCategoryNewsId { get; set; }
    
    public ICollection<CategoryNews>? ChildCategoryNews { get; set; } = new List<CategoryNews>();
    public ICollection<CategoryNewsDetail> CategoryNewsDetail { get; set; } = new List<CategoryNewsDetail>();

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}


