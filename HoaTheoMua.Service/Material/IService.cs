using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.Material;

public interface IService
{
    public Task<string> AddMaterialInCombo(Guid comboId, Request.MaterialComboRequest request);
    public Task<string> UpdateMaterialInCombo(Guid comboId, Request.MaterialComboRequest request);
    public Task<string> DeleteMaterialInCombo(Guid comboId, Request.DeleteMaterial request);
    public Task<string> UpdateChildProductWithFactor(Request.UpdateMaterialChildRequest request);
    public Task<string> PublicActiveCombo(Guid productId);
    public Task<string> UnPublicActiveCombo(Guid productId);
}