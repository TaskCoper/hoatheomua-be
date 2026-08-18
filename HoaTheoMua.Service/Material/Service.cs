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
    private sealed record ParentMaterialInfo(Guid MatId, decimal Quantity, bool IsCore, bool IsActive, string? ProductLabel, string? Unit);

    private static bool HasLabel(string? value, string expected) =>
        string.Equals(value?.Trim(), expected, StringComparison.OrdinalIgnoreCase);

    private static bool KeepsDecimal(string? unit) =>
        HasLabel(unit, "bó") || HasLabel(unit, "chậu");

    private static decimal CalculateParentQuantity(decimal requestedQuantity, string? productLabel, string? unit) =>
        HasLabel(productLabel, ComponentLabel) && HasLabel(unit, "cành")
            ? Math.Floor(requestedQuantity)
            : requestedQuantity;

    private static decimal CalculateChildQuantity(decimal parentQuantity, decimal factor, string? productLabel, string? unit)
    {
        if (HasLabel(productLabel, AuxiliaryLabel))
            return parentQuantity;

        var calculated = parentQuantity * factor;
        return KeepsDecimal(unit) ? calculated : Math.Floor(calculated);
    }

    private static void ValidateChildQuantity(decimal quantity, Guid matId, bool includeMaterialId = true)
    {
        if (quantity <= 0)
            throw new BadRequestException(includeMaterialId ? $"INVALID_QUANTITY:{matId}" : "INVALID_QUANTITY");
    }
    
    public async Task<string> AddMaterialInCombo(Guid comboId, Request.MaterialComboRequest request)
    {
        // ══════════ 1. Validate input ══════════
        if (request.Items is null || request.Items.Count == 0)
            throw new BadRequestException("EMPTY_ITEMS");

        if (request.Items.Any(x => x.Quantity <= 0))
            throw new BadRequestException("INVALID_QUANTITY");

        var items = request.Items
            .GroupBy(x => x.MatId)
            .Select(g => g.Last())
            .ToList();

        var matIds = items.Select(x => x.MatId).ToList();

        // ══════════ 2. Load combo cha ══════════
        var combo = await _dbContext.Products
                        .AsNoTracking()
                        .Where(x => x.Id == comboId && x.ParentProductId == null && !x.IsDeleted)
                        .Select(x => new { x.Id, x.NhanhProductId })
                        .FirstOrDefaultAsync()
                    ?? throw new NotFoundException("COMBO_NOT_FOUND");

        if (string.IsNullOrEmpty(combo.NhanhProductId))
            throw new BadRequestException("COMBO_NOT_SYNCED");

        // ══════════ 3. Validate material — 1 query ══════════
        var materials = await _dbContext.Products
            .AsNoTracking()
            .Where(x => matIds.Contains(x.Id) && !x.IsDeleted)
            .Select(x => new MaterialInfo(x.Id, x.ProductLabel, x.Unit))
            .ToListAsync();

        if (materials.Count != matIds.Count || materials.Any(x =>
                !HasLabel(x.ProductLabel, ComponentLabel) && !HasLabel(x.ProductLabel, AuxiliaryLabel)))
            throw new NotFoundException("MATERIAL_NOT_FOUND");

        var materialMap = materials.ToDictionary(x => x.Id);

        // ══════════ 4. Load child — Factor null = dùng quantity giống cha ══════════
        var syncableChildren = await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.ParentProductId == combo.NhanhProductId && !x.IsDeleted)
            .Select(x => new { x.Id, Factor = x.Factor ?? 1m })
            .ToListAsync();

        var factorMap = syncableChildren.ToDictionary(x => x.Id, x => x.Factor);
        var targetIds = syncableChildren.Select(x => x.Id).Append(comboId).ToList();

        // ══════════ 5. Load spec — cả row đã soft-delete, để revive ══════════
        var allSpecs = await _dbContext.ComboSpecifications
            .Where(x => targetIds.Contains(x.SubId))
            .ToListAsync();

        // Duplicate-safe: ưu tiên row chưa xoá + mới nhất
        var specMap = allSpecs
            .GroupBy(x => (x.SubId, x.MatId))
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.IsDeleted)
                    .ThenByDescending(x => x.UpdatedAt ?? DateTimeOffset.MinValue)
                    .First());

        var staleDuplicates = allSpecs
            .Where(x => !x.IsDeleted && !ReferenceEquals(x, specMap[(x.SubId, x.MatId)]))
            .ToList();

        // ══════════ 6. VALIDATE — chưa đụng vào entity ══════════
        var itemMap = items.ToDictionary(x => x.MatId);
        
        var finalParentState = specMap
            .Where(kv => kv.Key.SubId == comboId && !kv.Value.IsDeleted)
            .Select(kv =>
            {
                var s = kv.Value;
                return itemMap.TryGetValue(s.MatId, out var req)
                    ? new { s.MatId, req.Quantity, req.IsCore, req.IsActive }
                    : new { s.MatId, s.Quantity, s.IsCore, s.IsActive };
            })
            .ToList();

        // Material mới hoàn toàn (chưa có ở cha, hoặc đang soft-delete)
        var existingParentMatIds = finalParentState.Select(x => x.MatId).ToHashSet();

        finalParentState.AddRange(items
            .Where(x => !existingParentMatIds.Contains(x.MatId))
            .Select(x => new { x.MatId, x.Quantity, x.IsCore, x.IsActive }));

        if (!finalParentState.Any(x => x.IsCore && x.IsActive))
            throw new BadRequestException("CORE_MATERIAL_REQUIRED");

        // Factor không được làm quantity về 0 / tràn int — check trên TOÀN CẢNH công thức cha
        foreach (var child in syncableChildren)
        {
            if (child.Factor <= 0 || child.Factor > 1000m)
                throw new BadRequestException($"INVALID_FACTOR:{child.Id}");

            foreach (var state in finalParentState)
            {
                var material = materialMap[state.MatId];
                var parentQuantity = CalculateParentQuantity(state.Quantity, material.ProductLabel, material.Unit);
                ValidateChildQuantity(parentQuantity, state.MatId);

                var quantity = CalculateChildQuantity(parentQuantity, child.Factor, material.ProductLabel, material.Unit);
                ValidateChildQuantity(quantity, state.MatId);
            }
        }

        // ══════════ 7. MUTATE — từ đây không throw nữa ══════════
        var now = DateTimeOffset.UtcNow;
        var toInsert = new List<ComboSpecification>();

        foreach (var subId in targetIds)
        {
            var factor = subId == comboId ? 1m : factorMap[subId];

            foreach (var item in items)
            {
                var material = materialMap[item.MatId];
                var parentQuantity = CalculateParentQuantity(item.Quantity, material.ProductLabel, material.Unit);
                var quantity = subId == comboId
                    ? parentQuantity
                    : CalculateChildQuantity(parentQuantity, factor, material.ProductLabel, material.Unit);

                if (specMap.TryGetValue((subId, item.MatId), out var spec))
                {
                    var changed = spec.Quantity != quantity
                                  || spec.IsCore != item.IsCore
                                  || spec.IsActive != item.IsActive
                                  || spec.IsDeleted;

                    if (changed)
                    {
                        spec.Quantity = quantity;
                        spec.IsCore = item.IsCore;
                        spec.IsActive = item.IsActive;
                        spec.IsDeleted = false; // revive nếu từng bị xoá mềm
                        spec.UpdatedAt = now;
                    }
                }
                else
                {
                    toInsert.Add(new ComboSpecification
                    {
                        Id = Guid.NewGuid(),
                        SubId = subId,
                        MatId = item.MatId,
                        Quantity = quantity,
                        IsCore = item.IsCore,
                        IsActive = item.IsActive,
                        IsDeleted = false,
                        CreatedAt = now,
                        // UpdatedAt = now,
                    });
                }
            }
        }

        foreach (var dup in staleDuplicates)
        {
            dup.IsDeleted = true;
            dup.UpdatedAt = now;
        }

        if (toInsert.Count > 0)
            _dbContext.ComboSpecifications.AddRange(toInsert);

        await _dbContext.SaveChangesAsync();
        return "Lưu công thức thành công.";
    }

    public async Task<string> UpdateMaterialInCombo(Guid comboId, Request.MaterialComboRequest request)
    {
        // ══════════ 1. Validate input ══════════

        if (request.Items is null || request.Items.Count == 0)
            throw new BadRequestException("INVALID_INPUT");

        if (request.Items.Any(x => x.Quantity <= 0))
            throw new BadRequestException("INVALID_QUANTITY");

        var items = request.Items
            .GroupBy(x => x.MatId)
            .Select(g => g.Last())
            .ToList();

        // ══════════ 2. Load combo cha ══════════

        var combo = await _dbContext.Products
                        .AsNoTracking()
                        .Where(x => x.Id == comboId && x.ParentProductId == null && !x.IsDeleted)
                        .Select(x => new { x.Id, x.NhanhProductId })
                        .FirstOrDefaultAsync()
                    ?? throw new NotFoundException("COMBO_NOT_FOUND");

        if (string.IsNullOrEmpty(combo.NhanhProductId))
            throw new BadRequestException("COMBO_NOT_SYNCED");

        // ══════════ 3. Load child — Factor null = dùng quantity giống cha ══════════
        var syncableChildren = await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.ParentProductId == combo.NhanhProductId && !x.IsDeleted)
            .Select(x => new { x.Id, Factor = x.Factor ?? 1m })
            .ToListAsync();

        var factorMap = syncableChildren.ToDictionary(x => x.Id, x => x.Factor);
        var targetIds = syncableChildren.Select(x => x.Id).Append(comboId).ToList();

        // ══════════ 4. Load spec — cả row đã soft-delete, để revive ══════════
        var allSpecs = await _dbContext.ComboSpecifications
            .Where(x => targetIds.Contains(x.SubId))
            .ToListAsync();

        var specMap = allSpecs
            .GroupBy(x => (x.SubId, x.MatId))
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.IsDeleted)
                    .ThenByDescending(x => x.UpdatedAt ?? DateTimeOffset.MinValue)
                    .First());

        var staleDuplicates = allSpecs
            .Where(x => !x.IsDeleted && !ReferenceEquals(x, specMap[(x.SubId, x.MatId)]))
            .ToList();

        // ══════════ 5. VALIDATE — chưa đụng vào entity ══════════

        // Update: material BẮT BUỘC đã có (và đang active) trong công thức cha
        foreach (var item in items)
        {
            if (!specMap.TryGetValue((comboId, item.MatId), out var parentSpec) || parentSpec.IsDeleted)
                throw new NotFoundException($"MATERIAL_NOT_FOUND:{item.MatId}");
        }

        var itemMap = items.ToDictionary(x => x.MatId);
        
        var finalParentState = specMap
            .Where(kv => kv.Key.SubId == comboId && !kv.Value.IsDeleted)
            .Select(kv =>
            {
                var s = kv.Value;
                return itemMap.TryGetValue(s.MatId, out var req)
                    ? new { s.MatId, req.Quantity, req.IsCore, req.IsActive }
                    : new { s.MatId, s.Quantity, s.IsCore, s.IsActive };
            })
            .ToList();

        if (!finalParentState.Any(x => x.IsCore && x.IsActive))
            throw new BadRequestException("CORE_MATERIAL_REQUIRED");
        var materialProfiles = await _dbContext.Products
            .AsNoTracking()
            .Where(x => finalParentState.Select(s => s.MatId).Contains(x.Id) && !x.IsDeleted)
            .Select(x => new MaterialInfo(x.Id, x.ProductLabel, x.Unit))
            .ToListAsync();
        var materialMap = materialProfiles.ToDictionary(x => x.Id);

        foreach (var child in syncableChildren)
        {
            if (child.Factor <= 0 || child.Factor > 1000m)
                throw new BadRequestException($"INVALID_FACTOR:{child.Id}");

            foreach (var state in finalParentState)
            {
                var material = materialMap[state.MatId];
                var parentQuantity = CalculateParentQuantity(state.Quantity, material.ProductLabel, material.Unit);
                ValidateChildQuantity(parentQuantity, state.MatId);

                var quantity = CalculateChildQuantity(parentQuantity, child.Factor, material.ProductLabel, material.Unit);
                ValidateChildQuantity(quantity, state.MatId);
            }
        }

        // ══════════ 6. MUTATE — từ đây không throw nữa ══════════

        var now = DateTimeOffset.UtcNow;
        var toInsert = new List<ComboSpecification>();

        foreach (var subId in targetIds)
        {
            var factor = subId == comboId ? 1m : factorMap[subId];
            
            foreach (var state in finalParentState)
            {
                var material = materialMap[state.MatId];
                var parentQuantity = CalculateParentQuantity(state.Quantity, material.ProductLabel, material.Unit);
                var quantity = subId == comboId
                    ? parentQuantity
                    : CalculateChildQuantity(parentQuantity, factor, material.ProductLabel, material.Unit);

                if (specMap.TryGetValue((subId, state.MatId), out var spec))
                {
                    spec.Quantity = quantity;
                    spec.IsCore = state.IsCore;
                    spec.IsActive = state.IsActive;
                    spec.IsDeleted = false;
                    spec.UpdatedAt = now;
                }
                else
                {
                    toInsert.Add(new ComboSpecification
                    {
                        Id = Guid.NewGuid(),
                        SubId = subId,
                        MatId = state.MatId,
                        Quantity = quantity,
                        IsCore = state.IsCore,
                        IsActive = state.IsActive,
                        IsDeleted = false,
                        CreatedAt = now, //này insert vào
                        // UpdatedAt = now,
                    });
                }
            }
        }

        foreach (var dup in staleDuplicates)
        {
            dup.IsDeleted = true;
            dup.UpdatedAt = now;
        }

        if (toInsert.Count > 0)
            _dbContext.ComboSpecifications.AddRange(toInsert);

        await _dbContext.SaveChangesAsync();
        return "Cập nhật vật liệu trong công thức thành công.";
    }

    public async Task<string> DeleteMaterialInCombo(Guid comboId, Request.DeleteMaterial request)
    {
        // ══════════ 1. Validate input ══════════

        if (request.Materials is null || request.Materials.Count == 0)
            throw new BadRequestException("INVALID_INPUT");

        var matIds = request.Materials.ToHashSet();

        // ══════════ 2. Load combo cha ══════════

        var combo = await _dbContext.Products
                        .AsNoTracking()
                        .Where(x => x.Id == comboId && x.ParentProductId == null && !x.IsDeleted)
                        .Select(x => new { x.Id, x.NhanhProductId })
                        .FirstOrDefaultAsync()
                    ?? throw new NotFoundException("COMBO_NOT_FOUND");

        if (string.IsNullOrEmpty(combo.NhanhProductId))
            throw new BadRequestException("COMBO_NOT_SYNCED");

        // ══════════ 3. Load child ids ══════════

        var childIds = await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.ParentProductId == combo.NhanhProductId && !x.IsDeleted)
            .Select(x => x.Id)
            .ToListAsync();

        var targetIds = childIds.Append(comboId).ToList();

        // ══════════ 4. VALIDATE — chỉ load spec của CHA (no-tracking, projection nhẹ) ══════════

        var parentSpecs = await _dbContext.ComboSpecifications
            .AsNoTracking()
            .Where(x => x.SubId == comboId && !x.IsDeleted)
            .Select(x => new { x.MatId, x.IsCore, x.IsActive })
            .ToListAsync();

        var parentMatIds = parentSpecs.Select(x => x.MatId).ToHashSet();

        if (!matIds.IsSubsetOf(parentMatIds))
            throw new NotFoundException(
                $"MATERIAL_NOT_FOUND_IN_RECIPE:{string.Join(",", matIds.Except(parentMatIds))}");

        var hasCoreAfterDelete = parentSpecs.Any(x =>
            x.IsCore && x.IsActive && !matIds.Contains(x.MatId));

        if (!hasCoreAfterDelete)
            throw new BadRequestException("CORE_MATERIAL_REQUIRED");

        // ══════════ 5. MUTATE — chỉ load ĐÚNG những row cần update (có tracking) ══════════

        var specsToDelete = await _dbContext.ComboSpecifications
            .Where(x => targetIds.Contains(x.SubId)
                        && matIds.Contains(x.MatId)
                        && !x.IsDeleted)
            .ToListAsync();

        var now = DateTimeOffset.UtcNow;

        foreach (var spec in specsToDelete)
        {
            spec.IsDeleted = true;
            spec.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync();

        return "Xóa vật liệu khỏi công thức thành công";
    }

    public async Task<string> UpdateChildProductWithFactor(Request.UpdateMaterialChildRequest request)
    {
        if (request.Factor <= 0 || request.Factor > 1000m)
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

        // ══════════ Load song song — 2 query độc lập ══════════

        var parentSpecTask = _dbContext.ComboSpecifications
            .AsNoTracking()
            .Where(x => x.SubId == parentProduct.Id && !x.IsDeleted)
            .Select(x => new ParentMaterialInfo(
                x.MatId,
                x.Quantity,
                x.IsCore,
                x.IsActive,
                x.Mat.ProductLabel,
                x.Mat.Unit))
            .ToListAsync();

        // childSpecifications sẽ được lọc lại theo parentMatIds sau khi parentSpecTask xong,
        // nên tạm thời chưa gọi query này song song theo matId — xem note bên dưới.

        var parentSpecification = await parentSpecTask;

        if (parentSpecification.Count == 0)
            throw new NotFoundException("PARENT_COMBO_SPECIFICATION_NOT_FOUND");

        var parentMatIds = parentSpecification.Select(x => x.MatId).ToHashSet();

        foreach (var item in parentSpecification)
        {
            var quantity = CalculateChildQuantity(item.Quantity, request.Factor, item.ProductLabel, item.Unit);
            ValidateChildQuantity(quantity, item.MatId);
        }

        // Chỉ load spec của child liên quan tới MatId có trong công thức cha
        var childSpecifications = await _dbContext.ComboSpecifications
            .Where(x => x.SubId == childProduct.Id && parentMatIds.Contains(x.MatId))
            .ToListAsync();

        var childSpecificationMap = childSpecifications
            .GroupBy(x => x.MatId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(x => x.IsDeleted)
                    .ThenByDescending(x => x.UpdatedAt ?? DateTimeOffset.MinValue)
                    .First());

        var staleDuplicates = childSpecifications
            .Where(x => !x.IsDeleted && !ReferenceEquals(x, childSpecificationMap[x.MatId]))
            .ToList();

        var now = DateTimeOffset.UtcNow;
        var toInsert = new List<ComboSpecification>();

        foreach (var item in parentSpecification)
        {
            var quantity = CalculateChildQuantity(item.Quantity, request.Factor, item.ProductLabel, item.Unit);

            if (childSpecificationMap.TryGetValue(item.MatId, out var childSpecification))
            {
                childSpecification.Quantity = quantity;
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
                    Quantity = quantity,
                    IsCore = item.IsCore,
                    IsActive = item.IsActive,
                    IsDeleted = false,
                    CreatedAt = now,
                    // UpdatedAt = now
                });
            }
        }

        foreach (var dup in staleDuplicates)
        {
            dup.IsDeleted = true;
            dup.UpdatedAt = now;
        }

        childProduct.Factor = request.Factor;

        if (toInsert.Count > 0)
            _dbContext.ComboSpecifications.AddRange(toInsert);

        await _dbContext.SaveChangesAsync();

        return "Cập nhật hệ số và định lượng sản phẩm con thành công";
    }
}

