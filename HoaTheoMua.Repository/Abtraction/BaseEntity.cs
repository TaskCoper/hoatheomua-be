namespace HoaTheoMua.Repository.Abtraction;

public abstract class BaseEntity<T>
{
    public T Id { get; set; }
    public bool IsDeleted { get; set; }
}