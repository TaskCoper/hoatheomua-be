using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HoaTheoMua.Service.Material;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<string> AddMaterialInCombo(Guid comboId, Request.MaterialComboRequest request)
    {
        var queryCombo = _dbContext.Products.Where(x => x.Id == comboId && x.ParentProductId == null && !x.IsDeleted);

        var existingCombo = await queryCombo.FirstOrDefaultAsync();

        if (existingCombo == null)
        {
            throw new NotFoundException("COMBO_NOT_FOUND");
        }

        var productLabel = "Thành phần";
        
        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                throw new BadRequestException("INVALID_QUANTITY");
            }
            
            var material = await _dbContext.Products
                .FirstOrDefaultAsync(x => x.Id == item.MatId && x.ProductLabel == productLabel && !x.IsDeleted);

            if (material == null)
            {
                throw new NotFoundException("MATERIAL_NOT_FOUND");
            }
        }

        var queryChild = _dbContext.Products
            .Where(x => x.ParentProductId == existingCombo.NhanhProductId &&
                           !x.IsDeleted);
        
        var child = await queryChild.ToListAsync();

        var comboProducts = new List<Repository.Entity.Product>
        {
            existingCombo
        };
        
        comboProducts.AddRange(child);
        
        var hasCoreInRequest = request.Items.Any(x => x.IsCore && x.IsActive);
        
        var hasExistingCore = await _dbContext.ComboSpecifications.AnyAsync(x =>
                x.SubId == existingCombo.Id &&
                x.IsCore &&
                x.IsActive);
        
        if (!hasCoreInRequest && !hasExistingCore)
        {
            throw new BadRequestException("CORE_MATERIAL_REQUIRED");
        }
        
        foreach (var product in comboProducts)
        {
            foreach (var item in request.Items)
            {
                var comboSpecification = new ComboSpecification()
                {
                    Id = Guid.NewGuid(),
                    SubId = product.Id,
                    MatId = item.MatId,
                    Quantity = item.Quantity,
                    IsCore = item.IsCore,
                    IsActive = item.IsActive,
                };

                _dbContext.Add(comboSpecification);
            }
        }
        
        await _dbContext.SaveChangesAsync();
        
        return "Lưu công thức thành công.";
    }

    public async Task<string> UpdateMaterialInCombo(Guid comboId, Request.MaterialComboRequest request)
    {
        var queryCombo = _dbContext.Products.Where(x => x.Id == comboId && x.ParentProductId == null && !x.IsDeleted);

        var existingCombo = await queryCombo.FirstOrDefaultAsync();

        if (existingCombo == null)
        {
            throw new NotFoundException("COMBO_NOT_FOUND");
        }

        if (request.Items == null || !request.Items.Any())
        {
            throw new BadRequestException("INVALID_INPUT");
        }
        
        var queryChild = _dbContext.Products
            .Where(x => x.ParentProductId == existingCombo.NhanhProductId &&
                        !x.IsDeleted);
        
        var child = await queryChild.ToListAsync();
        
        var comboIds = child.Select(x => x.Id).Append(comboId).ToList();

        var querySpecification = _dbContext.ComboSpecifications.Where(x => comboIds.Contains(x.SubId));
        
        var specifications = await querySpecification.ToListAsync();

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                throw new BadRequestException("INVALID_QUANTITY");
            }
            
            var parentSpecification = specifications.FirstOrDefault(x => x.SubId == comboId && x.MatId == item.MatId);

            if (parentSpecification == null)
            {
                throw new NotFoundException("MATERIAL_NOT_FOUND");
            }
            
            parentSpecification.Quantity = item.Quantity;
            parentSpecification.IsActive = item.IsActive;
            parentSpecification.IsCore = item.IsCore;

            foreach (var childItem in child)
            {
                var childSpecification = specifications.FirstOrDefault(x => x.SubId == childItem.Id && x.MatId == item.MatId);

                if (childSpecification == null)
                {
                    throw new NotFoundException("MATERIAL_NOT_IN_RECIPE");
                }
                
                childSpecification.Quantity = item.Quantity;
                childSpecification.IsCore = item.IsCore;
                childSpecification.IsActive = item.IsActive;
                childSpecification.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        foreach (var id in comboIds)
        {
            var hasCore = specifications.Any(x => x.SubId == id && x.IsActive && x.IsCore);

            if (!hasCore)
            {
                throw new BadRequestException("CORE_MATERIAL_REQUIRED");
            }
        }
        
        await _dbContext.SaveChangesAsync();
        
        return "Cập nhật vật liệu trong công thức thành công.";
    }

    public async Task<string> DeleteMaterialInCombo(Guid comboId, Request.DeleteMaterial request)
    {
        var queryCombo = _dbContext.Products.Where(x => x.Id == comboId && x.ParentProductId == null && !x.IsDeleted);

        var existingCombo = await queryCombo.FirstOrDefaultAsync();

        if (existingCombo == null)
        {
            throw new NotFoundException("COMBO_NOT_FOUND");
        }
        
        if (request.MaterialIds == null || !request.MaterialIds.Any())
        {
            throw new BadRequestException("INVALID_INPUT");
        }
        
        var queryChild = _dbContext.Products
            .Where(x => x.ParentProductId == existingCombo.NhanhProductId &&
                        !x.IsDeleted);
        
        var child = await queryChild.ToListAsync();
        
        var comboIds = child.Select(x => x.Id).Append(comboId).ToList();

        var querySpecification = _dbContext.ComboSpecifications.Where(x => comboIds.Contains(x.SubId));
        
        var specifications = await querySpecification.ToListAsync();
        
        foreach (var materialId in request.MaterialIds)
        {
            var parentSpecification = specifications
                .FirstOrDefault(x =>
                    x.SubId == comboId &&
                    x.MatId == materialId);

            if (parentSpecification == null)
            {
                throw new NotFoundException("MATERIAL_NOT_FOUND_IN_RECIPE");
            }
        }

        foreach (var id in comboIds)
        {
            var hasCoreAfterDelete = specifications.Any(x =>
                x.SubId == id &&
                x.IsCore &&
                !request.MaterialIds.Contains(x.MatId));

            if (!hasCoreAfterDelete)
            {
                throw new BadRequestException("CORE_MATERIAL_REQUIRED");
            }
        }
        
        var specificationsToDelete = specifications
            .Where(x => request.MaterialIds.Contains(x.MatId))
            .ToList();

        foreach (var specification in specificationsToDelete)
        {
            specification.IsDeleted = true;
        }
        
        await _dbContext.SaveChangesAsync();
        return "Xóa vật liệu khỏi công thức thành công";
    }

    public async Task<string> UpdateChildProductWithFactor(Request.UpdateMaterialChildRequest request)
    {
        if (request.Factor <= 0)
        {
            throw new BadRequestException("INVALID_FACTOR");
        }
        
        var queryChild = _dbContext.Products.Where(x => x.Id == request.ProductId && !x.IsDeleted);

        var childProduct = await queryChild.FirstOrDefaultAsync();

        if (childProduct == null)
        {
            throw new NotFoundException("PRODUCT_NOT_FOUND");
        }
        
        // Product không có cha => chính nó là combo cha
        // Không cho update factor
        if (childProduct.ParentProductId == null)
        {
            throw new BadRequestException("PARENT_PRODUCT_REQUIRED");
        }
        
        var queryParent = _dbContext.Products
            .Where(x => x.NhanhProductId == childProduct.ParentProductId && 
                        x.ParentProductId == null && !x.IsDeleted);

        var parentProduct = await queryParent.FirstOrDefaultAsync();

        if (parentProduct == null)
        {
            throw new NotFoundException("PARENT_PRODUCT_NOT_FOUND");
        }

        var queryParentSpecification = _dbContext.ComboSpecifications
            .Where(x => x.SubId == parentProduct.Id &&
                        !x.IsDeleted);

        var parentSpecification = await queryParentSpecification.ToListAsync();
        
        if (!parentSpecification.Any())
        {
            throw new NotFoundException("PARENT_COMBO_SPECIFICATION_NOT_FOUND");
        }

        var queryChildSpecification = _dbContext.ComboSpecifications
            .Where(x => x.SubId == childProduct.Id &&
                        !x.IsDeleted);

        var childSpecifications = await queryChildSpecification.ToListAsync();

        foreach (var item in parentSpecification)
        {
            var childSpecification = childSpecifications.FirstOrDefault(x => x.MatId == item.MatId);

            if (childSpecification == null)
            {
                throw new NotFoundException("MATERIAL_NOT_FOUND_IN_CHILD_RECIPE");
            }
            
            var quantity = Math.Floor(item.Quantity * request.Factor);
            
            if (quantity <= 0)
            {
                throw new BadRequestException("INVALID_QUANTITY");
            }
            
            childSpecification.Quantity = (int)quantity;
            childSpecification.UpdatedAt = DateTimeOffset.UtcNow;
        }
        
        childProduct.Factor = request.Factor;
        
        await _dbContext.SaveChangesAsync();

        return "Cập nhật hệ số và định lượng sản phẩm con thành công";
    }
}
