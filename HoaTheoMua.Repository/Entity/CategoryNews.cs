using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class CategoryNews: BaseEntity<Guid>, IAuditableEntity
{
    public String Title { get; set; }
    
    public User User { get; set; }
    public Guid UserID { get; set; }
    public CategoryNews? ParentCategoryNews{ get; set; }
    public Guid? ParentCategoryNewsID { get; set; }
    
    public ICollection<CategoryNews>? ChildCategoryNews { get; set; } = new List<CategoryNews>();
    public ICollection<CategoryNewsDetail> CategoryNewsDetail { get; set; } = new List<CategoryNewsDetail>();

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}


