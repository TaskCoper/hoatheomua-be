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

    private const string ComponentLabel = "Thành phần";
    private const string AuxiliaryLabel = "Phụ liệu";

    private sealed record MaterialInfo(Guid Id, string? ProductLabel, string? Unit);
    private sealed record AddMaterialInfo(Guid Id, string? ProductLabel, string? Unit);
    
    private static bool HasLabel(string? value, string expected) =>
        string.Equals(value?.Trim(), expected, StringComparison.OrdinalIgnoreCase);

    private static bool KeepsDecimal(string? unit) =>
        HasLabel(unit, "bó") || HasLabel(unit, "giỏ") || HasLabel(unit, "chậu");

    private static decimal CalculateParentQuantity(decimal requestedQuantity, string? productLabel, string? unit) =>
        HasLabel(productLabel, ComponentLabel) && HasLabel(unit, "cành")
            ? Math.Floor(requestedQuantity)
            : requestedQuantity;

    private static decimal CalculateChildQuantity(decimal parentQuantity, decimal? factor, string? productLabel,
        string? unit)
    {
        // Phụ liệu giữ nguyên định lượng của combo cha.
        if (HasLabel(productLabel, AuxiliaryLabel))
            return parentQuantity;

        // Combo con chưa có Factor -> giữ nguyên định lượng của combo cha.
        if (factor is null)
            return parentQuantity;

        var calculated = parentQuantity * factor.Value;
        return KeepsDecimal(unit) ? calculated : Math.Floor(calculated);
    }

    private static void ValidateChildQuantity(decimal quantity, Guid matId, bool includeMaterialId = true)
    {
        if (quantity <= 0)
            throw new BadRequestException(includeMaterialId ? $"INVALID_QUANTITY:{matId}" : "INVALID_QUANTITY");
    }

    public async Task<string> AddMaterialInCombo(Guid comboId, Request.MaterialComboRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
            throw new BadRequestException("EMPTY_ITEMS");

        if (request.Items.Any(x => x.MatId == Guid.Empty))
            throw new BadRequestException("INVALID_MATERIAL_ID");

        if (request.Items.Any(x => x.Quantity <= 0))
            throw new BadRequestException("INVALID_QUANTITY");

        var duplicateMaterialId = request.Items
            .GroupBy(x => x.MatId)
            .FirstOrDefault(x => x.Count() > 1)
            ?.Key;

        if (duplicateMaterialId.HasValue)
            throw new ConflictException("MATERIAL_ALREADY_EXISTS");

        var items = request.Items;
        var matIds = items.Select(x => x.MatId).ToList();

        var combo = await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.Id == comboId && x.ParentProductId == null && !x.IsDeleted)
            .Select(x => new { x.Id, x.NhanhProductId, x.ProductType })
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("COMBO_NOT_FOUND");

        if (string.IsNullOrWhiteSpace(combo.NhanhProductId))
            throw new BadRequestException("COMBO_NOT_SYNCED");

        var requestMaterials = await _dbContext.Products
            .AsNoTracking()
            .Where(x => matIds.Contains(x.Id) && !x.IsDeleted)
            .Select(x => new AddMaterialInfo(x.Id, x.ProductLabel, x.Unit))
            .ToListAsync();

        if (requestMaterials.Count != matIds.Count || requestMaterials.Any(x =>
                !HasLabel(x.ProductLabel, ComponentLabel) &&
                !HasLabel(x.ProductLabel, AuxiliaryLabel)))
            throw new NotFoundException("MATERIAL_NOT_FOUND");

        // Sau khi thêm, công thức cha phải còn ít nhất 1 material core + active
        // (tính cả material đang add mới lẫn material core-active đã có sẵn không nằm trong request)
        //Note: context: Khi mà combo A (có các biến thể S, L) có có cách Hoa Lan, Hoa Hồng
        //con factor = null thi van luu thang cha, va con nhung thang con, thang nao co factor == null thi luu null
        //tuong ung voi cac vat lieu trong combo
        var existingParentSpecs = await _dbContext.ComboSpecifications
            .AsNoTracking()
            .Where(x => x.SubId == comboId && !x.IsDeleted && !matIds.Contains(x.MatId))
            .Select(x => new { x.IsCore, x.IsActive })
            .ToListAsync();

        var hasCoreActive =
            items.Any(x => x.IsCore && x.IsActive) ||
            existingParentSpecs.Any(x => x.IsCore && x.IsActive);

        if (!hasCoreActive)
            throw new BadRequestException("CORE_MATERIAL_REQUIRED");

        var materialMap = requestMaterials.ToDictionary(x => x.Id);

        var syncableChildren = await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.ParentProductId == combo.NhanhProductId && !x.IsDeleted)
            .Select(x => new { x.Id, x.Factor })   // decimal?, Factor null -> không mặc định 1m
            .ToListAsync();

        // Combo không size (ProductType = "Combo") không có combo con:
        // chỉ lưu công thức ở combo cha, không nhân Factor và không tạo dòng cho child.
        var normalizedProductType = combo.ProductType?.Trim().ToUpperInvariant();
        var isComboProductType = normalizedProductType == "COMBO";
        var isNoSizeCombo =
            isComboProductType &&
            syncableChildren.Count == 0;

        if (!isNoSizeCombo && syncableChildren.Count == 0)
            throw new BadRequestException("COMBO_CHILDREN_REQUIRED");

        foreach (var child in syncableChildren)
        {
            if (child.Factor is not null && child.Factor <= 0)
                throw new BadRequestException($"INVALID_FACTOR:{child.Id}");
        }

        var targetIds = syncableChildren
            .Select(x => x.Id)
            .Append(comboId)
            .ToList();

        var existingSpecs = await _dbContext.ComboSpecifications
            .Where(x => targetIds.Contains(x.SubId) && matIds.Contains(x.MatId))
            .ToListAsync();

        if (existingSpecs.Any(x => !x.IsDeleted))
            throw new ConflictException("MATERIAL_ALREADY_EXISTS");

        var existingSpecMap = existingSpecs
            .GroupBy(x => (x.SubId, x.MatId))
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(y => y.UpdatedAt ?? y.CreatedAt).First());

        var calculatedSpecs = new List<(Guid SubId, Guid MatId, decimal? Quantity, bool IsCore, bool IsActive)>();

        foreach (var item in items)
        {
            var material = materialMap[item.MatId];
            var parentQuantity = CalculateParentQuantity(item.Quantity, material.ProductLabel, material.Unit);
            //
            ValidateChildQuantity(parentQuantity, item.MatId);

            // Combo cha: giữ nguyên số lượng đã tính, không nhân Factor
            calculatedSpecs.Add((comboId, item.MatId, parentQuantity, item.IsCore, item.IsActive));

            if (isNoSizeCombo)
                continue;

            foreach (var child in syncableChildren)
            {
                decimal? childQuantity = child.Factor is null
                    ? null
                    : CalculateChildQuantity(
                        parentQuantity,
                        child.Factor,
                        material.ProductLabel,
                        material.Unit);

                if (childQuantity is not null)
                    ValidateChildQuantity(childQuantity.Value, item.MatId);

                calculatedSpecs.Add((child.Id, item.MatId, childQuantity, item.IsCore, item.IsActive));
            }
            //
        }

        var now = DateTimeOffset.UtcNow;
        var toInsert = new List<ComboSpecification>();

        foreach (var calculated in calculatedSpecs)
        {
            if (existingSpecMap.TryGetValue((calculated.SubId, calculated.MatId), out var deletedSpec))
            {
                deletedSpec.Quantity = calculated.Quantity;
                deletedSpec.IsCore = calculated.IsCore;
                deletedSpec.IsActive = calculated.IsActive;
                deletedSpec.IsDeleted = false;
                deletedSpec.UpdatedAt = now;
                continue;
            }

            toInsert.Add(new ComboSpecification
            {
                Id = Guid.NewGuid(),
                SubId = calculated.SubId,
                MatId = calculated.MatId,
                Quantity = calculated.Quantity,
                IsCore = calculated.IsCore,
                IsActive = calculated.IsActive,
                IsDeleted = false,
                CreatedAt = now,
            });
        }

        if (toInsert.Count > 0)
            _dbContext.ComboSpecifications.AddRange(toInsert);

        await _dbContext.SaveChangesAsync();
        return "Lưu công thức thành công.";
    }

    public async Task<string> UpdateMaterialInCombo(Guid comboId, Request.MaterialComboRequest request)
    {
        // 1. Validate request
        if (request is null || request.Items is null || request.Items.Count == 0)
            throw new BadRequestException("INVALID_INPUT");

        if (request.Items.Any(x => x.MatId == Guid.Empty))
            throw new BadRequestException("INVALID_MATERIAL_ID");

        if (request.Items.Any(x => x.Quantity <= 0))
            throw new BadRequestException("INVALID_QUANTITY");

        var duplicateMaterialId = request.Items
            .GroupBy(x => x.MatId)
            .FirstOrDefault(x => x.Count() > 1)
            ?.Key;

        if (duplicateMaterialId.HasValue)
            throw new ConflictException("MATERIAL_ALREADY_EXISTS");

        var items = request.Items.ToList();
        var matIds = items.Select(x => x.MatId).ToList();

        // 2. Load combo cha
        var combo = await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.Id == comboId && x.ParentProductId == null && !x.IsDeleted)
            .Select(x => new { x.Id, x.NhanhProductId })
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("COMBO_NOT_FOUND");

        if (string.IsNullOrWhiteSpace(combo.NhanhProductId))
            throw new BadRequestException("COMBO_NOT_SYNCED");

        // 3. Validate material
        var materials = await _dbContext.Products
            .AsNoTracking()
            .Where(x => matIds.Contains(x.Id) && !x.IsDeleted)
            .Select(x => new MaterialInfo(x.Id, x.ProductLabel, x.Unit))
            .ToListAsync();

        if (materials.Count != matIds.Count || materials.Any(x =>
                !HasLabel(x.ProductLabel, ComponentLabel) &&
                !HasLabel(x.ProductLabel, AuxiliaryLabel)))
            throw new NotFoundException("MATERIAL_NOT_FOUND");

        var materialMap = materials.ToDictionary(x => x.Id);

        // 4. Load combo con
        var syncableChildren = await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.ParentProductId == combo.NhanhProductId && !x.IsDeleted)
            .Select(x => new { x.Id, x.Factor })   // decimal?
            .ToListAsync();

        foreach (var child in syncableChildren)
        {
            if (child.Factor is not null && child.Factor <= 0)
                throw new BadRequestException($"INVALID_FACTOR:{child.Id}");
        }

        var targetIds = syncableChildren
            .Select(x => x.Id)
            .Append(comboId)
            .ToList();

        // 5. Load specification hiện tại
        // Lưu ý: KHÔNG check "already exists" ở đây như Add — Update yêu cầu spec
        // phải đã tồn tại từ trước (được xác nhận ở bước 6), nên check đó luôn throw nhầm.
        var existingSpecs = await _dbContext.ComboSpecifications
            .Where(x => targetIds.Contains(x.SubId) && matIds.Contains(x.MatId))
            .ToListAsync();

        var specMap = existingSpecs
            .GroupBy(x => (x.SubId, x.MatId))
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(x => x.IsDeleted)
                    .ThenByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                    .First());

        // 6. Material phải tồn tại trong recipe cha
        foreach (var item in items)
        {
            if (!specMap.TryGetValue((comboId, item.MatId), out var parentSpec) || parentSpec.IsDeleted)
                throw new NotFoundException($"MATERIAL_NOT_FOUND:{item.MatId}");
        }

        // 7. Kiểm tra sau update vẫn còn ít nhất một core active
        var currentParentSpecs = await _dbContext.ComboSpecifications
            .AsNoTracking()
            .Where(x => x.SubId == comboId && !x.IsDeleted)
            .Select(x => new { x.MatId, x.IsCore, x.IsActive })
            .ToListAsync();

        var requestMap = items.ToDictionary(x => x.MatId);

        var hasCoreActiveAfterUpdate = currentParentSpecs.Any(spec =>
        {
            if (requestMap.TryGetValue(spec.MatId, out var requestedItem))
                return requestedItem.IsCore && requestedItem.IsActive;
            return spec.IsCore && spec.IsActive;
        });

        if (!hasCoreActiveAfterUpdate)
            throw new BadRequestException("CORE_MATERIAL_REQUIRED");

        // 8. Calculate quantity parent và child
        var calculatedSpecs = new List<(Guid SubId, Guid MatId, decimal? Quantity, bool IsCore, bool IsActive)>();

        foreach (var item in items)
        {
            var material = materialMap[item.MatId];

            var parentQuantity = CalculateParentQuantity(item.Quantity, material.ProductLabel, material.Unit);
            ValidateChildQuantity(parentQuantity, item.MatId);

            calculatedSpecs.Add((comboId, item.MatId, parentQuantity, item.IsCore, item.IsActive));

            foreach (var child in syncableChildren)
            {
                decimal? childQuantity = child.Factor is null
                    ? null
                    : CalculateChildQuantity(
                        parentQuantity,
                        child.Factor,
                        material.ProductLabel,
                        material.Unit);

                if (childQuantity is not null)
                    ValidateChildQuantity(childQuantity.Value, item.MatId);

                calculatedSpecs.Add((child.Id, item.MatId, childQuantity, item.IsCore, item.IsActive));
            }
        }

        // 9. Update hoặc insert specification
        var now = DateTimeOffset.UtcNow;
        var toInsert = new List<ComboSpecification>();

        foreach (var calculated in calculatedSpecs)
        {
            if (specMap.TryGetValue((calculated.SubId, calculated.MatId), out var specification))
            {
                specification.Quantity = calculated.Quantity;
                specification.IsCore = calculated.IsCore;

                if (calculated.SubId == comboId)
                {
                    specification.IsActive = calculated.IsActive;
                }
                else if (specMap.TryGetValue((comboId, calculated.MatId), out var parentSpec))
                {
                    specification.IsActive = parentSpec.IsActive;
                }

                specification.IsDeleted = false;
                specification.UpdatedAt = now;
                continue;
            }

            var isActive = calculated.SubId == comboId
                ? calculated.IsActive
                : specMap[(comboId, calculated.MatId)].IsActive;

            toInsert.Add(new ComboSpecification
            {
                Id = Guid.NewGuid(),
                SubId = calculated.SubId,
                MatId = calculated.MatId,
                Quantity = calculated.Quantity,
                IsCore = calculated.IsCore,
                IsActive = isActive,
                IsDeleted = false,
                CreatedAt = now
            });
        }

        if (toInsert.Count > 0)
            _dbContext.ComboSpecifications.AddRange(toInsert);

        // 10. Save
        await _dbContext.SaveChangesAsync();

        return "Cập nhật vật liệu trong công thức thành công.";
    }

    public async Task<string> DeleteMaterialInCombo(Guid comboId, Request.DeleteMaterial request)
    {
        if (request.Materials is null || request.Materials.Count == 0)
            throw new BadRequestException("INVALID_INPUT");

        var matIds = request.Materials.ToHashSet();

        var isRelational = _dbContext.Database.IsRelational();

        // Serializable prevents concurrent requests from deleting different CORE rows
        // after validating against the same recipe state.
        await using var transaction = isRelational
            ? await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
            : null;

        var combo = await _dbContext.Products
                        .AsNoTracking()
                        .Where(x => x.Id == comboId && x.ParentProductId == null && !x.IsDeleted)
                        .Select(x => new { x.NhanhProductId })
                        .FirstOrDefaultAsync()
                    ?? throw new NotFoundException("COMBO_NOT_FOUND");

        var nhanhProductId = combo.NhanhProductId;

        if (string.IsNullOrWhiteSpace(nhanhProductId))
            throw new BadRequestException("COMBO_NOT_SYNCED");

        var parentSpecs = await _dbContext.ComboSpecifications
            .AsNoTracking()
            .Where(x => x.SubId == comboId
                        && !x.IsDeleted
                        && (matIds.Contains(x.MatId) || (x.IsCore && x.IsActive)))
            .Select(x => new { x.MatId, x.IsCore, x.IsActive })
            .ToListAsync();

        var parentMatIds = parentSpecs.Select(x => x.MatId).ToHashSet();
        if (!matIds.IsSubsetOf(parentMatIds))
            throw new NotFoundException("MATERIAL_NOT_FOUND_IN_RECIPE");

        var hasCoreAfterDelete = parentSpecs.Any(x =>
            x.IsCore && x.IsActive && !matIds.Contains(x.MatId));

        if (!hasCoreAfterDelete)
            throw new BadRequestException("CORE_MATERIAL_REQUIRED");

        var now = DateTimeOffset.UtcNow;

        if (isRelational)
        {
            await _dbContext.ComboSpecifications
                .Where(x => !x.IsDeleted
                            && matIds.Contains(x.MatId)
                            && (x.SubId == comboId || _dbContext.Products.Any(product =>
                                product.Id == x.SubId
                                && product.ParentProductId == nhanhProductId
                                && !product.IsDeleted)))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.IsDeleted, true)
                    .SetProperty(x => x.UpdatedAt, now));

            await transaction!.CommitAsync();
        }
        else
        {
            // ExecuteUpdate is not available for the EF InMemory provider used by service tests.
            var specsToDelete = await _dbContext.ComboSpecifications
                .Where(x =>
                    !x.IsDeleted &&
                    matIds.Contains(x.MatId) &&
                    (
                        x.SubId == comboId ||
                        _dbContext.Products.Any(product =>
                            product.Id == x.SubId &&
                            product.ParentProductId == combo.NhanhProductId &&
                            !product.IsDeleted)
                    ))
                .ToListAsync();

            foreach (var spec in specsToDelete)
            {
                spec.IsDeleted = true;
                spec.UpdatedAt = now;
            }

            await _dbContext.SaveChangesAsync();
        }

        return "Xóa vật liệu khỏi công thức thành công";
    }

    public async Task<string> UpdateChildProductWithFactor(Request.UpdateMaterialChildRequest request)
    {
        if (request is null || request.Factor is not null && request.Factor <= 0)
            throw new BadRequestException("INVALID_FACTOR");

        var childProduct = await _dbContext.Products
                               .FirstOrDefaultAsync(x => x.Id == request.ProductId && !x.IsDeleted)
                           ?? throw new NotFoundException("PRODUCT_NOT_FOUND");

        if (childProduct.ParentProductId == null)
            throw new BadRequestException("PARENT_PRODUCT_REQUIRED");

        var parentProduct = await _dbContext.Products
                                .AsNoTracking()
                                .Where(x => x.NhanhProductId == childProduct.ParentProductId &&
                                            x.ParentProductId == null &&
                                            !x.IsDeleted)
                                .Select(x => new { x.Id })
                                .FirstOrDefaultAsync()
                            ?? throw new NotFoundException("PARENT_PRODUCT_NOT_FOUND");

        var parentSpecifications = await _dbContext.ComboSpecifications
            .AsNoTracking()
            .Where(x => x.SubId == parentProduct.Id && !x.IsDeleted)
            .Select(x => new
            {
                x.MatId,
                x.Quantity,
                x.IsCore,
                x.IsActive,
                x.Mat.ProductName,
                x.Mat.ProductLabel,
                x.Mat.Unit,
                x.Mat.AvailableForSale
            })
            .ToListAsync();

        if (parentSpecifications.Count == 0)
            throw new NotFoundException("PARENT_COMBO_SPECIFICATION_NOT_FOUND");

        var calculatedSpecifications = parentSpecifications
            .Select(item =>
            {
                if (item.Quantity is null || item.Quantity <= 0)
                {
                    var materialName = string.IsNullOrWhiteSpace(item.ProductName)
                        ? item.MatId.ToString()
                        : item.ProductName;
                    throw new BadRequestException($"INVALID_QUANTITY:{materialName}");
                }

                decimal? calculated = request.Factor is null
                    ? null
                    : CalculateChildQuantity(
                        item.Quantity.Value,
                        request.Factor.Value,
                        item.ProductLabel,
                        item.Unit);

                if (calculated is not null && calculated <= 0)
                {
                    var materialName = string.IsNullOrWhiteSpace(item.ProductName)
                        ? item.MatId.ToString()
                        : item.ProductName;
                    throw new BadRequestException($"INVALID_QUANTITY:{materialName}");
                }

                return new
                {
                    item.MatId,
                    Quantity = calculated,
                    item.IsCore,
                    item.IsActive,
                    item.AvailableForSale
                };
            })
            .ToList();

        var parentMatIds = calculatedSpecifications.Select(x => x.MatId).ToHashSet();

        // Load toàn bộ công thức con để có thể loại bỏ những dòng không còn ở công thức cha.
        var childSpecifications = await _dbContext.ComboSpecifications
            .Where(x => x.SubId == childProduct.Id)
            .ToListAsync();

        var childSpecificationMap = childSpecifications
            .Where(x => parentMatIds.Contains(x.MatId))
            .GroupBy(x => x.MatId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.IsDeleted)
                    .ThenByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                    .First());

        var staleDuplicates = childSpecifications
            .Where(x => !x.IsDeleted &&
                        (!parentMatIds.Contains(x.MatId) ||
                         !ReferenceEquals(x, childSpecificationMap[x.MatId])))
            .ToList();

        var now = DateTimeOffset.UtcNow;
        var toInsert = new List<ComboSpecification>();

        foreach (var item in calculatedSpecifications)
        {
            if (childSpecificationMap.TryGetValue(item.MatId, out var childSpecification))
            {
                childSpecification.Quantity = item.Quantity;
                childSpecification.IsCore = item.IsCore;
                childSpecification.IsActive = item.IsActive;
                childSpecification.IsDeleted = false;
                childSpecification.UpdatedAt = now;
            }
            else
            {
                toInsert.Add(new ComboSpecification
                {
                    Id = Guid.NewGuid(),
                    SubId = childProduct.Id,
                    MatId = item.MatId,
                    Quantity = item.Quantity,
                    IsCore = item.IsCore,
                    IsActive = item.IsActive,
                    IsDeleted = false,
                    CreatedAt = now
                });
            }
        }

        foreach (var dup in staleDuplicates)
        {
            dup.IsDeleted = true;
            dup.UpdatedAt = now;
        }

        var coreSpecifications = calculatedSpecifications
            .Where(x => x.IsCore && x.IsActive)
            .ToList();

        if (!coreSpecifications.Any())
            throw new BadRequestException("CORE_MATERIAL_REQUIRED");
        //
        // childProduct.AvailableForSale = request.Factor is null
        //     ? 0m
        //     : coreSpecifications.Min(x =>
        //         Math.Floor((x.AvailableForSale ?? 0m) / x.Quantity!.Value));
        childProduct.Factor = request.Factor;
        childProduct.UpdatedAt = now;

        if (toInsert.Count > 0)
            _dbContext.ComboSpecifications.AddRange(toInsert);

        await _dbContext.SaveChangesAsync();

        return "Cập nhật hệ số và định lượng sản phẩm con thành công";
    }
}
