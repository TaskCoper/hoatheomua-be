namespace HoaTheoMua.Service.Material;

public class Request
{
    public class MaterialComboRequest
    {
        public List<ComboSpecification> Items { get; set; }
    }

    public class ComboSpecification
    {
        public Guid MatId { get; set; }
        public int Quantity { get; set; }
        public bool IsCore { get; set; }
        public bool IsActive { get; set; }
    }

    public class DeleteMaterial
    {
        public List<Guid> MaterialIds { get; set; }
    }
    

    public class UpdateMaterialChildRequest
    {
        public Guid ProductId { get; set; }
        public decimal Factor { get; set; }
    }
}