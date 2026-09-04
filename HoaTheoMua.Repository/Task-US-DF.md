# AI Custom — Customer & Admin

## 1. Customer — Xem và tải hoa AI

### US-040 — Xem lịch sử hoa AI

**Flow:**

Customer mở **Lịch sử hoa**  
→ FE gọi API lấy danh sách nhóm  
→ BE group theo `UserId + ProductId + Name`  
→ Mỗi nhóm hiển thị:

- Ảnh đại diện
    
- Tổng số ảnh
    

→ Customer mở một nhóm  
→ BE trả về **toàn bộ ảnh trong nhóm**, sắp xếp **mới nhất trước**.

> Đây là màn hình gốc để Customer tìm lại các hoa AI đã tạo.

---

### US-034 — Tải ảnh hoa AI

**Flow:**

Customer chọn một ảnh trong **US-040**  
→ FE gửi `GeneratedFlowerId`  
→ BE kiểm tra:

- Ownership
    
- File ảnh tồn tại
    

→ BE trả về **signed URL**  
→ FE tải ảnh trực tiếp từ **Cloudinary**.

**Quan hệ:**

```text
US-040 — Xem lịch sử hoa
        │
        └── US-034 — Tải một ảnh hoa cụ thể
```

---

# 2. Customer — Xem, chọn và tải thiệp

### US-045 — Xem lịch sử thiệp

**Flow:**

Customer mở **Lịch sử thiệp**  
→ FE gọi API  
→ BE lấy `client_histories` với `type = card` của User  
→ Join `output_id` với `generated_cards`  
→ Trả về toàn bộ thiệp, sắp xếp **mới nhất trước**.

> Đây là màn hình để Customer xem toàn bộ thiệp của mình, **không phụ thuộc Checkout**.

---

### US-042 — Chọn thiệp History tại Checkout

**Flow:**

Checkout đang có `ComboId`  
→ Customer chọn **"Thiệp từ lịch sử"**  
→ FE gửi `ComboId`  
→ BE truy vấn các Card bắt nguồn từ Combo theo các quan hệ:

```text
Combo
 ├── Card
 └── Flower
      └── Card
           └── Card regenerate
```

→ BE trả về các Card phù hợp  
→ Customer chọn một Card  
→ FE lưu `GeneratedCardId` vào **state của Checkout**.

**Lưu ý:**

- US-042 **chỉ phục vụ việc chọn thiệp**.
    
- Không gọi AI.
    
- Không tạo History mới.
    

---

### US-041 — Tải ảnh thiệp

**Flow:**

Customer chọn một `GeneratedCard`  
→ FE gửi `GeneratedCardId`  
→ BE kiểm tra:

- Ownership
    
- File ảnh tồn tại
    

→ BE trả về **signed URL**  
→ FE tải ảnh trực tiếp từ **Cloudinary**.

**Quan hệ:**

```text
US-045 — Xem toàn bộ lịch sử thiệp
        │
        └── US-041 — Tải một thiệp


US-042 — Lấy thiệp History theo Combo
        │
        └── Customer chọn thiệp để sử dụng tại Checkout
```

---

# 3. Admin — Quản lý thiệp AI

### US-044 — Danh sách thiệp AI

**Flow:**

Admin mở **AI Custom → Thiệp**  
→ FE gọi API danh sách  
→ BE lấy tất cả `client_histories` với `type = card`  
→ Join với `generated_cards`  
→ Trả về Card của **tất cả khách hàng**.

**Admin có thể:**

- Tìm theo khách hàng
    
- Tìm theo mã Card
    
- Tìm theo mã Order
    
- Lọc theo Size
    
- Lọc theo hình thức
    
- Phân trang
    
- Chọn một Card để xem chi tiết
    

---

### US-043 — Chi tiết thiệp AI

**Flow:**

Admin chọn `GeneratedCardId` từ **US-044**  
→ FE gọi API chi tiết  
→ BE lấy:

- `GeneratedCard`
    
- Card History
    
- Thông tin khách hàng
    
- Template
    
- Nội dung thiệp
    
- Các Order đã sử dụng Card
    

→ Trả về thông tin chi tiết.

**Lưu ý:**

> Không hiển thị giá tại màn hình Card. Giá được lưu và quản lý trong **Order**.

---

### US-046 — Admin tải ảnh thiệp

**Flow:**

Admin đang xem **Danh sách** hoặc **Chi tiết**  
→ Chọn tải ảnh  
→ FE gửi `GeneratedCardId`  
→ BE kiểm tra:

- Quyền Admin
    
- File ảnh tồn tại
    

→ BE trả về **signed URL**  
→ FE tải ảnh trực tiếp từ **Cloudinary**.

**Quan hệ:**

```text
US-044 — Danh sách thiệp
        │
        └── US-043 — Chi tiết thiệp
                │
                └── US-046 — Tải ảnh thiệp
```

---

# 4. Admin — Quản lý hoa Custom AI

### US-047 — Danh sách hoa Custom AI

**Flow:**

Admin mở **AI Custom → Hoa**  
→ FE gọi API danh sách  
→ BE lấy tất cả `client_histories` với `type = flower`  
→ Join `output_id` với `generated_flowers`  
→ Trả về kết quả hoa của **tất cả khách hàng**  
→ Thực hiện:

- Sắp xếp
    
- Tìm kiếm
    
- Lọc
    
- Phân trang
    

> Mỗi item tương ứng với **một kết quả hoa AI cụ thể**.

---

### US-048 — Chi tiết hoa Custom AI

**Flow:**

Admin chọn `GeneratedFlowerId` từ **US-047**  
→ FE gọi API chi tiết  
→ BE lấy:

- `GeneratedFlower`
    
- Flower History
    
- Thông tin khách hàng
    
- Combo nguồn
    
- Mockup
    
- Input đã sử dụng
    

→ Trả về thông tin chi tiết của hoa AI.

**Quan hệ:**

```text
US-047 — Danh sách hoa
        │
        └── US-048 — Chi tiết một hoa
```

---

# 5. Thứ tự tổng thể

## Customer — Hoa

```text
US-040 — Xem lịch sử hoa
        │
        └── US-034 — Tải ảnh hoa AI
```

## Customer — Thiệp

```text
US-045 — Xem lịch sử thiệp
        │
        └── US-041 — Tải ảnh thiệp


US-042 — Chọn thiệp History tại Checkout
        │
        └── Chọn thiệp để sử dụng tại Checkout
```

## Admin — Thiệp

```text
US-044 — Danh sách thiệp AI
        │
        └── US-043 — Chi tiết thiệp AI
                │
                └── US-046 — Admin tải ảnh thiệp
```

## Admin — Hoa

```text
US-047 — Danh sách hoa Custom AI
        │
        └── US-048 — Chi tiết hoa Custom AI
```

---

# 6. Tổng quan toàn bộ luồng

```text
                         AI CUSTOM
                            │
              ┌─────────────┴─────────────┐
              │                           │
           CUSTOMER                     ADMIN
              │                           │
       ┌──────┴──────┐             ┌──────┴──────┐
       │             │             │             │
      HOA           THIỆP         HOA           THIỆP
       │             │             │             │
    US-040         US-045       US-047         US-044
       │             │             │             │
    US-034         US-041       US-048         US-043
                     │                           │
                   US-042                     US-046
                     │
                 CHECKOUT
```