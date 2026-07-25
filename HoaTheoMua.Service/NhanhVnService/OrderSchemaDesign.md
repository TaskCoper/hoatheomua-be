# Thiết kế Schema cho OrderNhanhVn và OrderDetailsNhanhVn

Dựa trên payload `orderAdd` từ Nhanh.vn webhook, chúng ta sẽ tách dữ liệu đơn hàng thành hai bảng để đảm bảo tính chuẩn hóa và dễ dàng truy vấn.

## 1. Bảng `OrderNhanhVn` (Thông tin đơn hàng chính)

Bảng này sẽ chứa thông tin chung về đơn hàng.

```csharp
public class OrderNhanhVn : BaseEntity<long>, IAuditableEntity
{
    // Id (Primary Key, từ BaseEntity): ID tự động hoặc có thể map với OrderId của Nhanh.vn nếu muốn
    public long NhanhVnOrderId { get; set; } // data.info.id
    public int BusinessId { get; set; } // businessId
    public int DepotId { get; set; } // data.info.depotId
    public int Status { get; set; } // data.info.status
    public string Description { get; set; } // data.info.description
    public string PrivateDescription { get; set; } // data.info.privateDescription
    public string TrackingUrl { get; set; } // data.info.trackingUrl

    // Shipping Address
    public string CustomerName { get; set; } // data.shippingAddress.name
    public string CustomerMobile { get; set; } // data.shippingAddress.mobile
    public string ShippingAddress { get; set; } // data.shippingAddress.address
    public int ShippingCityId { get; set; } // data.shippingAddress.cityId
    public int ShippingDistrictId { get; set; } // data.shippingAddress.districtId
    public int ShippingWardId { get; set; } // data.shippingAddress.wardId

    // Carrier
    public int CarrierId { get; set; } // data.carrier.id
    public string CarrierName { get; set; } // data.carrier.name
    public DateTime DeliveryDate { get; set; } // data.carrier.deliveryDate (cần parse từ string sang DateTime)
    public int TotalWeight { get; set; } // data.carrier.weight
    public decimal ShipFee { get; set; } // data.carrier.shipFee
    public decimal CodFee { get; set; } // data.carrier.codFee

    // Payment
    public int BusinessPaymentStatus { get; set; } // data.payment.businessPaymentStatus
    public string PaymentDiscountType { get; set; } // data.payment.discount.type
    public decimal PaymentDiscountAmount { get; set; } // data.payment.discount.amount
    public decimal TotalAmount { get; set; } // Tổng tiền của đơn hàng, có thể tính toán từ products hoặc lấy từ payload nếu có trường rõ ràng

    // IAuditableEntity fields (CreatedAt, UpdatedAt, IsDeleted)
    public DateTime CreatedAt { get; set; } // data.info.createdAt (cần chuyển từ Unix timestamp)
    public DateTime UpdatedAt { get; set; } // data.info.updatedAt (cần chuyển từ Unix timestamp)
    public bool IsDeleted { get; set; }

    // Navigation property for Order Details
    public ICollection<OrderDetailsNhanhVn> OrderDetails { get; set; }
}
```

## 2. Bảng `OrderDetailsNhanhVn` (Chi tiết sản phẩm trong đơn hàng)

Bảng này sẽ chứa thông tin của từng sản phẩm trong một đơn hàng, với khóa ngoại liên kết tới bảng `OrderNhanhVn`.

```csharp
public class OrderDetailsNhanhVn : BaseEntity<long>, IAuditableEntity
{
    // Id (Primary Key, từ BaseEntity): ID tự động
    public long OrderNhanhVnId { get; set; } // Foreign Key tới OrderNhanhVn.Id

    public long NhanhVnProductId { get; set; } // data.products[i].id
    public string ProductCode { get; set; } // data.products[i].code
    public string ProductName { get; set; } // data.products[i].name
    public string ProductAvatar { get; set; } // data.products[i].avatar
    public int ProductWeight { get; set; } // data.products[i].weight
    public int Quantity { get; set; } // data.products[i].quantity
    public decimal Price { get; set; } // data.products[i].price
    public decimal OriginalPrice { get; set; } // data.products[i].originalPrice
    public decimal Discount { get; set; } // data.products[i].discount
    public string UnitName { get; set; } // data.products[i].unit.name
    public decimal VatValue { get; set; } // data.products[i].vat.value
    public decimal VatAmount { get; set; } // data.products[i].vat.amount

    // IAuditableEntity fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Mặc định UtcNow
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Mặc định UtcNow
    public bool IsDeleted { get; set; } = false;

    // Navigation property back to OrderNhanhVn
    public OrderNhanhVn OrderNhanhVn { get; set; }
}
```

## 3. Mối quan hệ giữa hai bảng

*   **Một-nhiều (One-to-Many):** Một `OrderNhanhVn` có thể có nhiều `OrderDetailsNhanhVn`.
*   Khóa ngoại `OrderDetailsNhanhVn.OrderNhanhVnId` sẽ tham chiếu đến `OrderNhanhVn.Id`.

## 4. Hướng dẫn triển khai chi tiết

Để triển khai schema này vào dự án hiện có, bạn cần thực hiện các bước sau:

### Bước 1: Tạo các Entity Class (`.cs`)

1.  **Tạo thư mục:** Trong `HoaTheoMua.Repository/Entity/`, tạo một thư mục mới ví dụ `NhanhVnOrder/`.
2.  **Tạo file `OrderNhanhVn.cs`:** Copy nội dung class `OrderNhanhVn` ở trên vào file này. Đảm bảo namespace là `HoaTheoMua.Repository.Entity.NhanhVnOrder`.
3.  **Tạo file `OrderDetailsNhanhVn.cs`:** Copy nội dung class `OrderDetailsNhanhVn` ở trên vào file này. Đảm bảo namespace là `HoaTheoMua.Repository.Entity.NhanhVnOrder`.

### Bước 2: Tạo Configuration Class (`.cs`)

Theo convention của dự án (`Configurations/`), bạn cần tạo các lớp cấu hình cho EF Core.

1.  **Tạo thư mục:** Trong `HoaTheoMua.Repository/Configurations/`, tạo một thư mục mới ví dụ `NhanhVnOrderConfiguration/`.
2.  **Tạo file `OrderNhanhVnConfiguration.cs`:**

    ```csharp
    using HoaTheoMua.Repository.Entity.NhanhVnOrder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    namespace HoaTheoMua.Repository.Configurations.NhanhVnOrderConfiguration
    {
        public class OrderNhanhVnConfiguration : IEntityTypeConfiguration<OrderNhanhVn>
        {
            public void Configure(EntityTypeBuilder<OrderNhanhVn> builder)
            {
                builder.ToTable("OrderNhanhVns"); // Đặt tên bảng

                builder.HasKey(o => o.Id); // Khóa chính

                builder.Property(o => o.NhanhVnOrderId)
                       .IsRequired();

                // Cấu hình các thuộc tính khác (max length, is required, default values, v.v.)
                builder.Property(o => o.CustomerName).HasMaxLength(250);
                builder.Property(o => o.CustomerMobile).HasMaxLength(50);
                builder.Property(o => o.ShippingAddress).HasMaxLength(500);
                // ...

                // Cấu hình mối quan hệ One-to-Many
                builder.HasMany(o => o.OrderDetails)
                       .WithOne(od => od.OrderNhanhVn)
                       .HasForeignKey(od => od.OrderNhanhVnId)
                       .OnDelete(DeleteBehavior.Cascade); // Ví dụ: xóa OrderNhanhVn thì xóa luôn OrderDetails

                // Cấu hình cho Soft Delete
                builder.HasQueryFilter(o => !o.IsDeleted);
            }
        }
    }
    ```

3.  **Tạo file `OrderDetailsNhanhVnConfiguration.cs`:**

    ```csharp
    using HoaTheoMua.Repository.Entity.NhanhVnOrder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    namespace HoaTheoMua.Repository.Configurations.NhanhVnOrderConfiguration
    {
        public class OrderDetailsNhanhVnConfiguration : IEntityTypeConfiguration<OrderDetailsNhanhVn>
        {
            public void Configure(EntityTypeBuilder<OrderDetailsNhanhVn> builder)
            {
                builder.ToTable("OrderDetailsNhanhVns"); // Đặt tên bảng

                builder.HasKey(od => od.Id); // Khóa chính

                builder.Property(od => od.NhanhVnProductId)
                       .IsRequired();
                builder.Property(od => od.Quantity)
                       .IsRequired();
                builder.Property(od => od.Price)
                       .HasColumnType("decimal(18,2)") // Ví dụ: Cấu hình kiểu dữ liệu cho Decimal
                       .IsRequired();

                // Cấu hình các thuộc tính khác
                builder.Property(od => od.ProductCode).HasMaxLength(100);
                builder.Property(od => od.ProductName).HasMaxLength(500);
                // ...

                // Cấu hình cho Soft Delete
                builder.HasQueryFilter(od => !od.IsDeleted);
            }
        }
    }
    ```

### Bước 3: Cập nhật `AppDbContext` (`HoaTheoMua.Repository/AppDbContext.cs`)

Thêm các `DbSet` mới vào `AppDbContext`:

```csharp
using HoaTheoMua.Repository.Entity.NhanhVnOrder; // Thêm using này

public class AppDbContext : DbContext
{
    // ... các DbSet hiện có ...

    public DbSet<OrderNhanhVn> OrderNhanhVns { get; set; }
    public DbSet<OrderDetailsNhanhVn> OrderDetailsNhanhVns { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        // ...
    }
}
```

### Bước 4: Tạo Migration

Chạy lệnh EF Core migration từ thư mục gốc của repo:

```bash
dotnet ef migrations add AddNhanhVnOrderTables --project HoaTheoMua.Repository --startup-project HoaTheoMua.API
```

Sau đó, áp dụng migration vào cơ sở dữ liệu (thường là tự động khi ứng dụng khởi động, như đã nêu trong `CLAUDE.md`, hoặc thủ công nếu cần):

```bash
dotnet ef database update --project HoaTheoMua.Repository --startup-project HoaTheoMua.API
```

### Bước 5: Cập nhật logic xử lý Webhook `WebhookNhanhVnOrderUpdate`

Trong phương thức `WebhookNhanhVnOrderUpdate` (có lẽ trong `NhanhVnService.Service.cs` hoặc một controller tương tự), bạn cần parse payload và lưu vào hai bảng mới.

1.  **Parse Payload:**
    Đảm bảo bạn có DTOs để deserialize JSON payload thành các đối tượng C#.

    ```csharp
    // Ví dụ DTOs (cần tạo các class tương ứng)
    public class NhanhVnWebhookPayload
    {
        public string Event { get; set; }
        public int BusinessId { get; set; }
        public NhanhVnOrderData Data { get; set; }
    }

    public class NhanhVnOrderData
    {
        public ShippingAddress ShippingAddress { get; set; }
        public Info Info { get; set; }
        public List<Product> Products { get; set; }
        public Carrier Carrier { get; set; }
        public Channel Channel { get; set; }
        public Payment Payment { get; set; }
    }
    // ... và các DTOs cho ShippingAddress, Info, Product, Carrier, v.v.
    ```

2.  **Logic lưu trữ:**

    ```csharp
    // Giả sử bạn đã deserialize payload thành object `webhookPayload`
    var data = webhookPayload.Data;

    // 1. Tạo và lưu OrderNhanhVn
    var orderNhanhVn = new OrderNhanhVn
    {
        NhanhVnOrderId = data.Info.Id,
        BusinessId = webhookPayload.BusinessId,
        DepotId = data.Info.DepotId,
        Status = data.Info.Status,
        Description = data.Info.Description,
        PrivateDescription = data.Info.PrivateDescription,
        TrackingUrl = data.Info.TrackingUrl,
        CustomerName = data.ShippingAddress.Name,
        CustomerMobile = data.ShippingAddress.Mobile,
        ShippingAddress = data.ShippingAddress.Address,
        ShippingCityId = data.ShippingAddress.CityId,
        ShippingDistrictId = data.ShippingAddress.DistrictId,
        ShippingWardId = data.ShippingAddress.WardId,
        CarrierId = data.Carrier.Id,
        CarrierName = data.Carrier.Name,
        DeliveryDate = DateTime.Parse(data.Carrier.DeliveryDate), // Cần xử lý lỗi nếu parse thất bại
        TotalWeight = data.Carrier.Weight,
        ShipFee = data.Carrier.ShipFee,
        CodFee = data.Carrier.CodFee,
        BusinessPaymentStatus = data.Payment.BusinessPaymentStatus,
        PaymentDiscountType = data.Payment.Discount.Type,
        PaymentDiscountAmount = data.Payment.Discount.Amount,
        // TotalAmount: Cần tính toán hoặc lấy từ payload nếu có
        CreatedAt = DateTimeOffset.FromUnixTimeSeconds(data.Info.CreatedAt).UtcDateTime,
        UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(data.Info.UpdatedAt).UtcDateTime,
        IsDeleted = false
    };

    _dbContext.OrderNhanhVns.Add(orderNhanhVn);
    await _dbContext.SaveChangesAsync(); // Lưu để có OrderNhanhVn.Id

    // 2. Tạo và lưu OrderDetailsNhanhVn cho từng sản phẩm
    var orderDetailsList = new List<OrderDetailsNhanhVn>();
    foreach (var product in data.Products)
    {
        var orderDetail = new OrderDetailsNhanhVn
        {
            OrderNhanhVnId = orderNhanhVn.Id, // Sử dụng ID của đơn hàng vừa tạo
            NhanhVnProductId = product.Id,
            ProductCode = product.Code,
            ProductName = product.Name,
            ProductAvatar = product.Avatar,
            ProductWeight = product.Weight,
            Quantity = product.Quantity,
            Price = product.Price,
            OriginalPrice = product.OriginalPrice,
            Discount = product.Discount,
            UnitName = product.Unit.Name,
            VatValue = product.Vat.Value,
            VatAmount = product.Vat.Amount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        orderDetailsList.Add(orderDetail);
    }

    _dbContext.OrderDetailsNhanhVns.AddRange(orderDetailsList);
    await _dbContext.SaveChangesAsync();
    ```

**Lưu ý quan trọng:**

*   **Xử lý thời gian:** Các trường `createdAt` và `updatedAt` trong payload là Unix timestamp. Bạn cần chuyển đổi chúng sang `DateTime` hoặc `DateTimeOffset` của C#.
*   **`DeliveryDate`:** Trường `deliveryDate` là chuỗi ngày tháng, cần `DateTime.Parse` hoặc `DateTime.TryParse` để chuyển đổi.
*   **DTOs:** Bạn sẽ cần tạo các lớp DTO (Data Transfer Objects) tương ứng với cấu trúc JSON của webhook payload để dễ dàng deserialize.
*   **`TotalAmount`:** Nếu payload không có trường `TotalAmount` rõ ràng, bạn sẽ cần tính tổng từ các sản phẩm trong `OrderDetailsNhanhVn` sau khi đã lưu, hoặc trong quá trình ánh xạ.
*   **Soft Delete:** Nhớ áp dụng `.Where(x => !x.IsDeleted)` khi truy vấn dữ liệu từ cả hai bảng.
*   **Error Handling:** Cần thêm các khối `try-catch` và xử lý lỗi phù hợp, ví dụ khi parse ngày tháng hoặc khi lưu vào database.
*   **Ghi log:** Ghi log chi tiết quá trình xử lý webhook để dễ dàng debug khi có lỗi.
