using System.Data;
using System.Data.Common;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Entity;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
            throw new BadRequestException(includeMaterialId ? $"Số lượng vật liệu sau khi tính toán không hợp lệ: {matId}" : "Số lượng vật liệu sau khi tính toán không hợp lệ");
    }

    private static async Task SafeRollbackAsync(IDbContextTransaction transaction)
    {
        try
        {
            await transaction.RollbackAsync();
        }
        catch (DbException)
        {
            // Transaction có thể đã bị DB hủy sẵn (ví dụ nạn nhân của deadlock).
            // Nuốt lỗi ở đây để không che mất exception gốc; `await using` sẽ dispose.
        }
    }
    
    private sealed record CalculatedChildSpec(
        Guid MatId,
        decimal? Quantity,
        bool IsCore,
        bool IsActive,
        decimal? AvailableForSale);
    
    //Nếu combo ở trạng thái mở bán thì ko đc thêm, ko được chỉnh sửa
    public async Task<string> AddMaterialInCombo(Guid comboId, Request.MaterialComboRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
            throw new BadRequestException("Danh sách vật liệu không được để trống");

        if (request.Items.Any(x => x.MatId == Guid.Empty))
            throw new BadRequestException("Mã vật liệu không được để trống");

        if (request.Items.Any(x => x.Quantity <= 0))
            throw new BadRequestException("Số lượng vật liệu phải lớn hơn 0");

        var duplicateMaterialId = request.Items
            .GroupBy(x => x.MatId)
            .FirstOrDefault(x => x.Count() > 1)
            ?.Key;

        if (duplicateMaterialId.HasValue)
            throw new ConflictException("Danh sách vật liệu bị trùng lặp");

        var items = request.Items;
        var matIds = items.Select(x => x.MatId).ToList();

        var combo = await _dbContext.Products
                        .AsNoTracking()
                        .Where(x => x.Id == comboId && x.ParentProductId == null && !x.IsDeleted)
                        .Select(x => new { x.Id, x.NhanhProductId, x.ProductType, x.IsActive })
                        .FirstOrDefaultAsync()
                    ?? throw new NotFoundException("Không tìm thấy combo");

        if (combo.IsActive)
        {
            throw new ConflictException("Combo đang ở trạng thái mở bán, không thể thêm vật liệu");
        }

        if (string.IsNullOrWhiteSpace(combo.NhanhProductId))
            throw new BadRequestException("Combo chưa đồng bộ");

        var requestMaterials = await _dbContext.Products
            .AsNoTracking()
            .Where(x => matIds.Contains(x.Id) && !x.IsDeleted)
            .Select(x => new AddMaterialInfo(x.Id, x.ProductLabel, x.Unit))
            .ToListAsync();

        if (requestMaterials.Count != matIds.Count || requestMaterials.Any(x =>
                !HasLabel(x.ProductLabel, ComponentLabel) &&
                !HasLabel(x.ProductLabel, AuxiliaryLabel)))
            throw new NotFoundException("Vật liệu không tồn tại hoặc không hợp lệ");

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
            throw new BadRequestException("Công thức phải có ít nhất một vật liệu chính (core) đang hoạt động");

        var materialMap = requestMaterials.ToDictionary(x => x.Id);

        var syncableChildren = await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.ParentProductId == combo.NhanhProductId && !x.IsDeleted)
            .Select(x => new { x.Id, x.Factor }) 
            .ToListAsync();

        // Combo không size (ProductType = "Combo") không có combo con:
        // chỉ lưu công thức ở combo cha, không nhân Factor và không tạo dòng cho child.
        var normalizedProductType = combo.ProductType?.Trim().ToUpperInvariant();
        var isComboProductType = normalizedProductType == "COMBO";
        var isNoSizeCombo =
            isComboProductType &&
            syncableChildren.Count == 0;

        if (!isNoSizeCombo && syncableChildren.Count == 0)
            throw new BadRequestException("Combo phải có ít nhất một biến thể (combo con)");

        foreach (var child in syncableChildren)
        {
            if (child.Factor is not null && child.Factor <= 0)
                throw new BadRequestException($"Hệ số của combo con không hợp lệ: {child.Id}");
        }

        var targetIds = syncableChildren
            .Select(x => x.Id)
            .Append(comboId)
            .ToList();

        var hasActiveSpec = await _dbContext.ComboSpecifications
            .AsNoTracking()
            .AnyAsync(x => targetIds.Contains(x.SubId)
                           && matIds.Contains(x.MatId)
                           && !x.IsDeleted);

        if (hasActiveSpec)
            throw new ConflictException("Vật liệu đã có trong công thức của combo");

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
        }

        var now = DateTimeOffset.UtcNow;
        var toInsert = new List<ComboSpecification>();

        foreach (var calculated in calculatedSpecs)
        {
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
            throw new BadRequestException("Danh sách vật liệu không được để trống");

        if (request.Items.Any(x => x.MatId == Guid.Empty))
            throw new BadRequestException("Mã vật liệu không được để trống");

        if (request.Items.Any(x => x.Quantity <= 0))
            throw new BadRequestException("Số lượng vật liệu phải lớn hơn 0");

        var duplicateMaterialId = request.Items
            .GroupBy(x => x.MatId)
            .FirstOrDefault(x => x.Count() > 1)
            ?.Key;

        if (duplicateMaterialId.HasValue)
            throw new ConflictException("Danh sách vật liệu bị trùng lặp");

        var items = request.Items.ToList();
        var matIds = items.Select(x => x.MatId).ToList();

        // 2. Load combo cha
        var combo = await _dbContext.Products
                        .AsNoTracking()
                        .Where(x => x.Id == comboId && x.ParentProductId == null && !x.IsDeleted)
                        .Select(x => new { x.Id, x.NhanhProductId, x.IsActive })
                        .FirstOrDefaultAsync()
                    ?? throw new NotFoundException("Không tìm thấy combo");

        if (combo.IsActive)
        {
            throw new ConflictException("Combo đang ở trạng thái mở bán, không thể chỉnh sửa");
        }

        if (string.IsNullOrWhiteSpace(combo.NhanhProductId))
            throw new BadRequestException("Combo chưa đồng bộ");

        // 3. Validate material
        var materials = await _dbContext.Products
            .AsNoTracking()
            .Where(x => matIds.Contains(x.Id) && !x.IsDeleted)
            .Select(x => new MaterialInfo(x.Id, x.ProductLabel, x.Unit))
            .ToListAsync();

        if (materials.Count != matIds.Count || materials.Any(x =>
                !HasLabel(x.ProductLabel, ComponentLabel) &&
                !HasLabel(x.ProductLabel, AuxiliaryLabel)))
            throw new NotFoundException("Vật liệu không tồn tại hoặc không hợp lệ");

        var materialMap = materials.ToDictionary(x => x.Id);

        // 4. Load combo con
        var syncableChildren = await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.ParentProductId == combo.NhanhProductId && !x.IsDeleted)
            .Select(x => new { x.Id, x.Factor })
            .ToListAsync();
        //co the dung any() throw ra cung duoc

        foreach (var child in syncableChildren)
        {
            if (child.Factor is not null && child.Factor <= 0)
                throw new BadRequestException($"Hệ số của combo con không hợp lệ: {child.Id}");
        }

        var targetIds = syncableChildren
            .Select(x => x.Id)
            .Append(comboId)
            .ToList();

        // 5. Load specification hiện tại
        // Lưu ý: KHÔNG check "already exists" ở đây như Add — Update yêu cầu spec
        // phải đã tồn tại từ trước (được xác nhận ở bước 6), nên check đó luôn throw nhầm.
        var existingSpecs = await _dbContext.ComboSpecifications
            .Where(x => targetIds.Contains(x.SubId) && matIds.Contains(x.MatId) && !x.IsDeleted)
            .ToListAsync();
        
        var specMap = existingSpecs
            .GroupBy(x => (x.SubId, x.MatId))
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
                    .First());

        // 6. Material phải tồn tại trong recipe cha
        foreach (var item in items)
        {
            if (!specMap.TryGetValue((comboId, item.MatId), out _))
                throw new NotFoundException($"Vật liệu không có trong công thức hiện tại của combo: {item.MatId}");
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
            throw new BadRequestException("Công thức phải có ít nhất một vật liệu chính (core) đang hoạt động");

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

        // 9. Update hoặc insert specification — không hồi sinh dòng đã xóa.
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

                specification.UpdatedAt = now;
                continue;
            }

            // Chưa có dòng active -> tạo dòng MỚI, kể cả khi từng có dòng đã xóa.
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
        // ---- 1. Validate request -----------------------------------------------
        if (request?.Materials is null || request.Materials.Count == 0)
            throw new BadRequestException("Danh sách vật liệu cần xóa không được để trống");

        if (request.Materials.Any(x => x == Guid.Empty))
            throw new BadRequestException("Mã vật liệu không được để trống");

        var matIds = request.Materials.ToHashSet();

        // ---- 2. Mở transaction Serializable ------------------------------------
        // Serializable chặn tình huống hai Quản trị viên cùng lúc xóa hai dòng CORE
        // khác nhau: cả hai đều đọc thấy "vẫn còn CORE khác" rồi cùng xóa, làm công
        // thức mất sạch CORE. Check ở tầng ứng dụng không chặn được việc này.
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            // ---- 3. Load combo cha ---------------------------------------------
            var combo = await _dbContext.Products
                            .AsNoTracking()
                            .Where(x => x.Id == comboId
                                        && x.ParentProductId == null
                                        && !x.IsDeleted)
                            .Select(x => new { x.NhanhProductId, x.IsActive })
                            .FirstOrDefaultAsync()
                        ?? throw new NotFoundException("Không tìm thấy combo");
            var nhanhProductId = combo.NhanhProductId;

            if (combo.IsActive)
            {
                throw new ConflictException("Combo đang mở bán, không thể xóa");
            }

            if (string.IsNullOrWhiteSpace(nhanhProductId))
                throw new BadRequestException("Combo chưa đồng bộ");

            // ---- 4. Load dòng công thức còn hiệu lực của combo cha ---------------
            // IsDeleted = true coi như không tồn tại, không đọc tới.
            var parentSpecs = await _dbContext.ComboSpecifications
                .AsNoTracking()
                .Where(x => x.SubId == comboId
                            && !x.IsDeleted
                            && (matIds.Contains(x.MatId) || (x.IsCore && x.IsActive)))
                .Select(x => new { x.MatId, x.IsCore, x.IsActive })
                .ToListAsync();

            var parentMatIds = parentSpecs.Select(x => x.MatId).ToHashSet();

            // Vật liệu được yêu cầu nhưng không còn dòng hiệu lực nào trong công thức.
            // Nguyên nhân phổ biến nhất là trang đang mở đã cũ, hoặc người khác vừa xóa
            // (EXC-02 / AC-011). Frontend nhận mã này thì làm mới công thức.
            var missing = matIds.Where(id => !parentMatIds.Contains(id)).ToList();

            if (missing.Count > 0)
                throw new ConflictException($"Công thức đã bị thay đổi, vui lòng tải lại trang: {string.Join(", ", missing)}");

            // ---- 5. Kiểm tra ràng buộc CORE ------------------------------------
            var activeCoreMatIds = parentSpecs
                .Where(x => x.IsCore && x.IsActive)
                .Select(x => x.MatId)
                .ToHashSet();

            // 5a. Công thức vốn đã không còn dòng CORE active nào -> dữ liệu ở trạng
            //     thái không hợp lệ từ trước, không phải lỗi của thao tác hiện tại.
            //     Tách mã lỗi để người dùng không hiểu nhầm là dòng họ đang xóa có vấn đề.
            if (activeCoreMatIds.Count == 0)
                throw new BadRequestException("Công thức hiện tại không còn vật liệu chính (core) đang hoạt động");

            // 5b. Chặn cả lô nếu lô xóa làm mất dòng CORE cuối cùng (EXC-03 / AC-008).
            //     Không xóa một phần: người dùng chọn một lô là chọn một ý định.
            if (activeCoreMatIds.All(id => matIds.Contains(id)))
                throw new BadRequestException("Không thể xóa vì công thức phải chứa ít nhất một vật liệu core");

            // ---- 6. Xóa mềm ở combo cha và toàn bộ combo con --------------------
            var now = DateTimeOffset.UtcNow;

            var specificationsToDelete = await _dbContext.ComboSpecifications
                .Where(x => !x.IsDeleted
                            && matIds.Contains(x.MatId)
                            && (x.SubId == comboId
                                || _dbContext.Products.Any(product =>
                                    product.Id == x.SubId
                                    && product.ParentProductId == nhanhProductId
                                    && !product.IsDeleted)))
                .ToListAsync();

            foreach (var specification in specificationsToDelete)
            {
                specification.IsDeleted = true;
                specification.UpdatedAt = now;
                // specification.DeletedAt = now;
                // TODO: specification.DeletedBy = _currentUser.Id khi có sẵn accessor
            }

            var affected = specificationsToDelete.Count;

            if (affected > 0)
                await _dbContext.SaveChangesAsync();

            if (affected == 0)
                throw new ConflictException("Công thức đã bị thay đổi bởi người khác, vui lòng thử lại");

            await transaction.CommitAsync();

            return "Xóa vật liệu khỏi công thức thành công";
        }
        catch (DbUpdateException)
        {
            // Ghi thất bại ở tầng EF
            await SafeRollbackAsync(transaction);
            throw new ConflictException("Cập nhật công thức thất bại, vui lòng thử lại");
        }
        catch (DbException)
        {
            // Deadlock, timeout, mất kết nối. Với Serializable, deadlock là cơ chế
            // bình thường khi có tranh chấp chứ không phải trường hợp hiếm.
            await SafeRollbackAsync(transaction);
            throw new ConflictException("Cập nhật công thức thất bại, vui lòng thử lại");
        }
        // Các exception nghiệp vụ (BadRequest/NotFound/Conflict) không bị bắt ở đây;
        // chúng thoát ra ngoài và transaction được rollback khi `await using` dispose.
    }

    public async Task<string> UpdateChildProductWithFactor(Request.UpdateMaterialChildRequest request)
    {
        // ---- 1. Validate request -----------------------------------------------
        if (request is null)
            throw new BadRequestException("Dữ liệu yêu cầu không được để trống");

        if (request.Factor is not null && request.Factor <= 0)
            throw new BadRequestException("Hệ số phải lớn hơn 0");

        // ---- 2. Load combo con và combo cha ------------------------------------
        var childProduct = await _dbContext.Products
                               .FirstOrDefaultAsync(x => x.Id == request.ProductId && !x.IsDeleted)
                           ?? throw new NotFoundException("Không tìm thấy sản phẩm");

        if (childProduct.IsActive)
        {
            throw new ConflictException("Sản phẩm đang mở bán, không thể cập nhật hệ số");
        }

        if (childProduct.ParentProductId == null)
            throw new BadRequestException("Sản phẩm không phải là combo con (không có sản phẩm cha)");

        var parentProduct = await _dbContext.Products
                                .AsNoTracking()
                                .Where(x => x.NhanhProductId == childProduct.ParentProductId
                                            && x.ParentProductId == null
                                            && !x.IsDeleted)
                                .Select(x => new { x.Id, x.IsActive })
                                .FirstOrDefaultAsync()
                            ?? throw new NotFoundException("Không tìm thấy sản phẩm cha");

        // if (parentProduct.IsActive)
        // {
        //     throw new ConflictException("Combo đang mở bán, không thể cập nhật hệ số");
        // }

        // ---- 3. Load công thức cha (chỉ dòng còn hiệu lực) ----------------------
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
            throw new NotFoundException("Không tìm thấy công thức combo cha");

        // ---- 4. Tính định lượng con, gom hết lỗi rồi mới báo --------------------
        var calculatedSpecifications = new List<CalculatedChildSpec>();
        var invalidParentQuantities = new List<string>();
        var invalidChildQuantities = new List<string>();

        foreach (var item in parentSpecifications)
        {
            var materialName = string.IsNullOrWhiteSpace(item.ProductName)
                ? item.MatId.ToString()
                : item.ProductName;

            // Định lượng cha lẽ ra luôn > 0 (Add/Update đã ép). Rơi vào đây nghĩa là
            // dữ liệu cha đang lệch, KHÔNG phải người dùng nhập sai hệ số.
            if (item.Quantity is null || item.Quantity <= 0)
            {
                invalidParentQuantities.Add(materialName);
                continue;
            }

            // Chưa có hệ số -> định lượng con để trống, không phải 0.
            decimal? calculated = request.Factor is null
                ? null
                : CalculateChildQuantity(
                    item.Quantity.Value,
                    request.Factor.Value,
                    item.ProductLabel,
                    item.Unit);

            // EXC-03 / AC-010: định lượng con rơi xuống 0 sau khi làm tròn.
            if (calculated is not null && calculated <= 0)
            {
                invalidChildQuantities.Add(materialName);
                continue;
            }

            calculatedSpecifications.Add(new CalculatedChildSpec(
                item.MatId, calculated, item.IsCore, item.IsActive, item.AvailableForSale));
        }

        if (invalidParentQuantities.Count > 0)
            throw new BadRequestException(
                $"Công thức của combo cha có số lượng không hợp lệ: {string.Join(", ", invalidParentQuantities)}");

        if (invalidChildQuantities.Count > 0)
            throw new BadRequestException(
                $"Số lượng vật liệu sau khi nhân hệ số bị giảm về 0, vui lòng kiểm tra lại hệ số: {string.Join(", ", invalidChildQuantities)}");

        // ---- 5. Kiểm tra CORE — TRƯỚC khi sửa bất kỳ entity nào -----------------
        // Nguồn là công thức cha, nên đổi hệ số không thể làm mất CORE. Câu này chỉ
        // fail khi công thức cha vốn đã hỏng -> dùng mã riêng để không khiến người
        // dùng tưởng lỗi nằm ở hệ số họ vừa nhập.
        var coreSpecifications = calculatedSpecifications
            .Where(x => x.IsCore && x.IsActive)
            .ToList();

        if (coreSpecifications.Count == 0)
            throw new BadRequestException("Công thức của combo cha không còn vật liệu chính (core) đang hoạt động");

        // ---- 6. Load công thức con đang hiệu lực -------------------------------
        // Dòng IsDeleted = true coi như không tồn tại: không đọc, không hồi sinh.
        var childSpecifications = await _dbContext.ComboSpecifications
            .Where(x => x.SubId == childProduct.Id && !x.IsDeleted)
            .ToListAsync();

        var parentMatIds = calculatedSpecifications.Select(x => x.MatId).ToHashSet();

        // Giữ một dòng cho mỗi vật liệu. Sau khi có unique index lọc theo IsDeleted
        // thì mỗi cặp (SubId, MatId) chỉ còn tối đa một dòng hiệu lực và GroupBy này
        // thành thừa — giữ lại cho tới khi migration chạy xong và dữ liệu trùng cũ
        // đã được dọn.
        var childSpecificationMap = childSpecifications
            .Where(x => parentMatIds.Contains(x.MatId))
            .GroupBy(x => x.MatId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).First());

        // Dòng cần dọn: vật liệu không còn ở công thức cha, hoặc dòng trùng dư ra.
        var staleSpecifications = childSpecifications
            .Where(x => !parentMatIds.Contains(x.MatId)
                        || !ReferenceEquals(x, childSpecificationMap[x.MatId]))
            .ToList();

        // ---- 7. Ghi công thức con ----------------------------------------------
        var now = DateTimeOffset.UtcNow;
        var toInsert = new List<ComboSpecification>();

        foreach (var item in calculatedSpecifications)
        {
            if (childSpecificationMap.TryGetValue(item.MatId, out var childSpecification))
            {
                // Dòng đang hiệu lực -> cập nhật tại chỗ.
                childSpecification.Quantity = item.Quantity;
                childSpecification.IsCore = item.IsCore;
                childSpecification.IsActive = item.IsActive;
                childSpecification.UpdatedAt = now;
                continue;
            }

            // Chưa có dòng hiệu lực -> tạo dòng MỚI, kể cả khi từng có dòng đã xóa.
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

        foreach (var stale in staleSpecifications)
        {
            stale.IsDeleted = true;
            stale.UpdatedAt = now;
            // stale.DeletedAt = now; // CẦN MIGRATION: cột DeletedAt
        }

        // ---- 8. Cập nhật hệ số --------------------------------------------------
        // TREO — cần chốt cách biểu diễn "chưa xác định" trước khi bật lại:
        // gán 0 cho trường hợp chưa có hệ số là SAI, vì 0 nghĩa là hết hàng còn chưa
        // có hệ số nghĩa là chưa cấu hình. Hai thứ này người vận hành xử lý khác nhau.
        //
        // childProduct.AvailableForSale = request.Factor is null
        //     ? null
        //     : coreSpecifications.Min(x =>
        //         Math.Floor((x.AvailableForSale ?? 0m) / x.Quantity!.Value));

        childProduct.Factor = request.Factor;
        childProduct.UpdatedAt = now;

        if (toInsert.Count > 0)
            _dbContext.ComboSpecifications.AddRange(toInsert);

        await _dbContext.SaveChangesAsync();

        return "Cập nhật hệ số và định lượng sản phẩm con thành công";
    }

   public async Task<string> PublicActiveCombo(Guid productId)
    {
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var product = await _dbContext.Products
                              .Where(x => x.Id == productId && !x.IsDeleted)
                              .FirstOrDefaultAsync()
                          ?? throw new NotFoundException("Không tìm thấy sản phẩm");
            
            if (product.IsActive)
            {
                await transaction.CommitAsync();
                return "Sản phẩm đã được mở bán trước đó.";
            }

            product.IsActive = true;
            product.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return "Mở bán thành công.";
        }
        catch (DbUpdateException)
        {
            await SafeRollbackAsync(transaction);
            throw new ConflictException("Mở bán thất bại, vui lòng thử lại");
        }
        catch (DbException)
        {
            await SafeRollbackAsync(transaction);
            throw new ConflictException("Mở bán thất bại, vui lòng thử lại");
        }
    }
 
    public async Task<string> UnPublicActiveCombo(Guid productId)
    {
        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            var product = await _dbContext.Products
                              .Where(x => x.Id == productId && !x.IsDeleted)
                              .FirstOrDefaultAsync()
                          ?? throw new NotFoundException("Không tìm thấy sản phẩm");
            if (!product.IsActive)
            {
                await transaction.CommitAsync();
                return "Sản phẩm đang ở trạng thái chưa mở bán.";
            }

            product.IsActive = false;
            product.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return "Ngừng mở bán thành công.";
        }
        catch (DbUpdateException)
        {
            await SafeRollbackAsync(transaction);
            throw new ConflictException("Ngừng mở bán thất bại, vui lòng thử lại");
        }
        catch (DbException)
        {
            await SafeRollbackAsync(transaction);
            throw new ConflictException("Ngừng mở bán thất bại, vui lòng thử lại");
        }
    }
}
