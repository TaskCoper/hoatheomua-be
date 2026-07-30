using AutoMapper;
namespace HoaTheoMua.Service.Product;

public class ProductMapping: Profile
{
    public  ProductMapping()
    {
        CreateMap<Request.UpdateProductRequest, Repository.Entity.Product>()
            .ForAllMembers(opts => opts.Condition(
                (src, dest, srcMember) => srcMember != null));
    }
}