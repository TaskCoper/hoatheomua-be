namespace HoaTheoMua.Service.Flower;

public class Request
{
    public class CustomerGeneratedFlowerListRequest
    {
        public Guid? SessionId { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AdminGeneratedFlowerListRequest
    {
        public Guid? GeneratedFlowerId { get; set; }
        public Guid? ProductId { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class CreateFlowerRequest
    {
        public string? Name { get; set; }
        public Guid ProductId { get; set; }
        public List<MaterialTotalRequest> MaterialTotals { get; set; } = [];
        public Guid BundleId { get; set; }
        public Guid MockupId { get; set; }
        public Guid RibbonProductId { get; set; }
        public Guid WrappingPaperProductId { get; set; }
    }

    public class MaterialTotalRequest
    {
        public Guid MatId { get; set; }
        public int Quantity { get; set; }
    }

    public class RegenerateFlowerRequest
    {
        public Guid? BundleId { get; set; }
        public Guid? MockupId { get; set; }
    }
}
