using FluentValidation;
using HoaTheoMua.Service.Common;
namespace HoaTheoMua.Service.PostCore.Validate;

public class SavePostCoreValidator : AbstractValidator<Request.SavePostCoreRequest>
{
    public SavePostCoreValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(20000);
        RuleFor(x => x.Hashtag).MaximumLength(1000);
        RuleFor(x => x.Images).Must(x => x == null || x.Count <= 20)
            .WithMessage("Post must have at most 20 images");
        RuleFor(x => x.Images).NotNull().When(x => x is Request.UpdatePostCoreRequest)
            .WithMessage("Images must be supplied when updating a post; use an empty array for no images");
        RuleForEach(x => x.Images).NotNull().SetValidator(new ImageValidator());
        RuleFor(x => x.Images).Must(x => x == null || x.Where(i => i != null).Select(i => i.SortOrder).Distinct().Count() == x.Count)
            .WithMessage("Duplicate image order");
        RuleFor(x => x.Images).Must(x => x == null || x.Where(i => i?.Id != null).Select(i => i.Id).Distinct().Count() == x.Count(i => i?.Id != null))
            .WithMessage("Duplicate image id");
    }
}
public class CreatePostCoreValidator : AbstractValidator<Request.CreatePostCoreRequest>
{
    public CreatePostCoreValidator()
    {
        Include(new SavePostCoreValidator());
        RuleForEach(x => x.Images).Must(x => x?.Id == null).WithMessage("New images must not have an id");
    }
}
public class ImageValidator : AbstractValidator<Request.ImageInput>
{
    public ImageValidator()
    {
        RuleFor(x => x.Id).NotEqual(Guid.Empty).When(x => x.Id.HasValue);
        RuleFor(x => x.ImageUrl).NotEmpty().MaximumLength(2048).HttpUrl();
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Width).GreaterThan(0).When(x => x.Width.HasValue);
        RuleFor(x => x.Height).GreaterThan(0).When(x => x.Height.HasValue);
    }
}
public class GetPostCoreListValidator : AbstractValidator<Request.GetPostCoreListRequest>
{
    public GetPostCoreListValidator()
    {
        RuleFor(x => x.Platform).IsInEnum().When(x => x.Platform.HasValue);
    }
}
