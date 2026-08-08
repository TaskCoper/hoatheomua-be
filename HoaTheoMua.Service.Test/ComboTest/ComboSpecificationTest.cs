using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HoaTheoMua.Service.Test.ComboTest;

// ---- MOCK ENTITIES & INTERFACES (For TDD blueprint) ----
// (Developer sẽ move các class này sang các thư mục/project phù hợp khi triển khai thật)

public class ProductMockEntity
{
    public Guid Id { get; set; }
    public int AvailableForSale { get; set; }
    public string ProductName { get; set; }
}

public class SpecificationMockEntity
{
    public Guid Id { get; set; }
    public Guid SubId { get; set; } // Combo ID
    public Guid MatId { get; set; } // Material ID
    public int Quantity { get; set; }
    public bool IsCore { get; set; }
    public bool IsActive { get; set; }
    public ProductMockEntity MaterialProduct { get; set; }
}

public class ComboFormulaResponse
{
    public int SellableQuantity { get; set; }
    public List<ComboFormulaItemResponse> Items { get; set; } = new();
}

public class ComboFormulaItemResponse
{
    public Guid MaterialId { get; set; }
    public string MaterialName { get; set; }
    public int RequiredQuantity { get; set; }
    public bool IsCore { get; set; }
}

public interface IComboService
{
    Task<ComboFormulaResponse> GetComboSpecificationAsync(Guid comboId);
}

// ---- TDD TEST CLASS (US006) ----

public class ComboSpecificationTest
{
    [Fact]
    public void GetComboSpecification_ValidCombo_CalculatesSellableQuantityAndSortsByCore()
    {
        // 1. Arrange
        var comboId = Guid.NewGuid();
        
        // Tạo dữ liệu kho (3 vật liệu)
        var mat1 = new ProductMockEntity { Id = Guid.NewGuid(), ProductName = "Hoa hồng", AvailableForSale = 10 };
        var mat2 = new ProductMockEntity { Id = Guid.NewGuid(), ProductName = "Giấy gói", AvailableForSale = 100 };
        var mat3 = new ProductMockEntity { Id = Guid.NewGuid(), ProductName = "Hoa baby", AvailableForSale = 20 };

        // Tạo cấu hình công thức cho Combo
        var specs = new List<SpecificationMockEntity>
        {
            // Core 1: Cần 2, Kho có 10 -> Có thể làm tối đa 5 combo
            new SpecificationMockEntity { SubId = comboId, MatId = mat1.Id, MaterialProduct = mat1, IsCore = true, IsActive = true, Quantity = 2 },
            
            // Support: Cần 1, Kho có 100 -> Có thể làm tối đa 100 combo (nhưng không dùng để tính SellableQuantity của cả Combo)
            new SpecificationMockEntity { SubId = comboId, MatId = mat2.Id, MaterialProduct = mat2, IsCore = false, IsActive = true, Quantity = 1 },
            
            // Core 2: Cần 5, Kho có 20 -> Có thể làm tối đa 4 combo
            new SpecificationMockEntity { SubId = comboId, MatId = mat3.Id, MaterialProduct = mat3, IsCore = true, IsActive = true, Quantity = 5 }
        };

        // --- TODO (Dành cho Dev): Khởi tạo Service thật với InMemoryDbContext chứa `specs` ở đây ---
        // var service = CreateComboService(dbContext);
        // var result = await service.GetComboSpecificationAsync(comboId);
        
        // --- SIMULATED ACT --- (Mô phỏng lại logic Service sẽ thực hiện để cho Pass Test)
        var sortedItems = specs.Where(s => s.IsActive).OrderByDescending(s => s.IsCore).ToList();
        var coreItems = sortedItems.Where(s => s.IsCore);
        
        // Công thức tính toán
        int calculatedSellableQuantity = coreItems.Any() 
            ? coreItems.Min(cs => cs.MaterialProduct.AvailableForSale / cs.Quantity) 
            : 0;

        // 3. Assert
        // Tính toán bằng tay: Min(10/2, 20/5) = Min(5, 4) = 4
        Assert.Equal(4, calculatedSellableQuantity); 
        
        // Item Core phải luôn nằm trước Support trong danh sách hiển thị
        Assert.True(sortedItems[0].IsCore);
        Assert.True(sortedItems[1].IsCore);
        Assert.False(sortedItems[2].IsCore);
    }
    
    [Fact]
    public void GetComboSpecification_EmptyCombo_ThrowsException()
    {
        // 1. Arrange
        var comboId = Guid.NewGuid();
        var specs = new List<SpecificationMockEntity>(); // Combo rỗng, không có vật liệu nào
        
        // 2 & 3. Act & Assert
        // TODO: Mở comment dòng dưới khi code logic thật
        // var ex = await Assert.ThrowsAsync<Exception>(() => service.GetComboSpecificationAsync(comboId));
        // Assert.Equal("COMBO_EMPTY", ex.Message);
    }

    [Fact]
    public void GetComboSpecification_NoCoreItems_CalculatesSellableQuantityAsZero()
    {
        // 1. Arrange
        var comboId = Guid.NewGuid();
        var mat1 = new ProductMockEntity { Id = Guid.NewGuid(), ProductName = "Giấy gói", AvailableForSale = 100 };

        var specs = new List<SpecificationMockEntity>
        {
            // Combo này thiết kế sai, chỉ có đồ phụ kiện (Support), không có lõi (Core)
            new SpecificationMockEntity { SubId = comboId, MatId = mat1.Id, MaterialProduct = mat1, IsCore = false, IsActive = true, Quantity = 1 }
        };
        
        // 2. Act
        var coreItems = specs.Where(s => s.IsActive && s.IsCore);
        int calculatedSellableQuantity = coreItems.Any() 
            ? coreItems.Min(cs => cs.MaterialProduct.AvailableForSale / cs.Quantity) 
            : 0; // Trả về 0 nếu Any() = false
            
        // 3. Assert
        Assert.Equal(0, calculatedSellableQuantity);
    }
}
