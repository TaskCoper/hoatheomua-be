# Các US còn lại — Flow tổng quan dưới góc nhìn Backend

## 0. Phạm vi

File này tổng hợp các US còn lại trong `docs-us` và **không lặp lại** các US đã có trong `Task-US-DF.md`:

```text
US-034, US-040, US-041, US-042, US-043,
US-044, US-045, US-046, US-047, US-048
```

Các quyết định nghiệp vụ mới nhất được ưu tiên hơn nội dung cũ trong tài liệu:

- Không lưu draft tạo hoa hoặc Checkout.
- Reload/rời trang trước khi tạo Order thì dữ liệu FE đang nhập bị mất.
- Khách hàng phải thanh toán Order trước khi tạo thiệp AI mới.
- Giá thiệp đã được tính vào Order trước khi thanh toán.
- Hoa, thiệp và dữ liệu Order gửi sang Nhanh.vn thông qua file PDF tổng hợp.

---

# 1. Customer — Tạo hoa Custom AI

## US-030 — Nhập dữ liệu tạo hoa

**Mục đích:**

Customer chọn Combo nguồn, nhập tên và chọn Mockup để chuẩn bị input cho AI.

**Flow mới:**

Customer chọn **Tạo mẫu hoa** trên một Combo  
→ FE lấy thông tin Combo và tình trạng còn hàng  
→ Customer nhập tên  
→ FE lấy danh sách Mockup Active  
→ Customer chọn một Mockup  
→ FE giữ toàn bộ dữ liệu trong state.

> US-030 không còn tạo yêu cầu draft trong Database. Nếu reload hoặc thoát trang, dữ liệu chưa generate bị mất.

**API BE cần:**

- Lấy chi tiết Combo và kiểm tra khả năng bán.
- Lấy danh sách Mockup Active, chưa bị xóa mềm.
- Kiểm tra quota/trạng thái tạo AI nếu FE cần hiển thị trước khi generate.

---

## US-033 — Generate/Regenerate hoa bằng AI

**Mục đích:**

Nhận input từ US-030, gọi AI và lưu một kết quả hoa thành công.

**Flow:**

FE gửi `ProductId + MockupId + Name + dữ liệu tùy chỉnh`  
→ BE kiểm tra ownership, Combo, tồn kho, Mockup, quota và tiến trình AI đang chạy  
→ BE lấy dữ liệu Combo làm input đáng tin cậy  
→ BE gọi AI  
→ AI trả ảnh  
→ BE upload ảnh lên Cloudinary  
→ BE tạo một `generated_flowers`  
→ BE tạo một `client_histories`:

```text
type      = flower
base_id   = ProductId
output_id = GeneratedFlowerId
```

→ FE nhận kết quả hoa vừa tạo.

**Regenerate:**

Mỗi lần regenerate  
→ tạo một `generated_flowers` mới  
→ tạo một `client_histories` mới  
→ giữ nguyên `ProductId + Name` để US-040 group đúng một yêu cầu.

**API BE cần:**

- Generate hoa lần đầu.
- Regenerate từ một kết quả hoa thuộc Customer.
- Lấy quota và trạng thái AI hiện tại để ngăn chạy trùng cùng yêu cầu.

**Điểm quan trọng:**

- Chỉ kết quả AI thành công mới trở thành lịch sử chính thức.
- Không cần bảng request riêng và không lưu draft.
- `output_id` luôn chỉ đúng ảnh trong `generated_flowers`.
- `ProductId + Name` xác định nhóm generate/regenerate của US-040.

```text
US-030 — Chuẩn bị input trên FE
        │
        └── US-033 — Gọi AI và lưu kết quả
                    │
                    └── US-040 — Xem lại nhóm kết quả
```

---

# 2. Customer — Checkout, thanh toán và tạo thiệp

## Flow tổng thể

```text
US-038 — Nhập dữ liệu Checkout trên FE
        │
        └── US-039 — Tạo Order và thanh toán
                    │
                    ├── Thiệp Free/History: đã chọn trước thanh toán
                    │
                    └── Thiệp AI mới
                         │
                         └── US-035 — Generate sau khi Order đã Paid
                                      │
                                      └── US-036 — Regenerate thiệp
```

---

## US-038 — Chuẩn bị Checkout từ mẫu hoa

**Mục đích:**

Cho Customer nhập đủ dữ liệu để BE có thể báo giá và tạo Order.

**Flow:**

Customer chọn một Product hoặc `GeneratedFlowerId`  
→ FE mở Checkout  
→ BE kiểm tra nguồn hoa và Combo gốc còn hợp lệ  
→ Customer nhập địa chỉ, thông tin giao hàng, VAT và thông tin thiệp  
→ FE giữ dữ liệu tạm, chưa tạo Order và chưa lưu DB.

Customer có 3 lựa chọn thiệp:

1. **Thiệp Free:** chọn ngay tại Checkout.
2. **Thiệp từ History:** chọn ngay tại Checkout qua US-042.
3. **Thiệp AI mới:** nhập Size, hình thức, nội dung để tính giá; chỉ generate sau khi thanh toán.

**API BE cần:**

- Kiểm tra Product/GeneratedFlower được phép đặt hàng.
- Lấy Size, Template và cấu hình phụ phí đang Active.
- Tính phí giao hàng.
- Tính tạm tính hoa và thiệp.

> Không cần bảng `checkouts`. Reload/rời trang trước khi tạo Order thì state bị mất.

---

## US-039 — Tạo Order và thanh toán

**Mục đích:**

Chốt dữ liệu Checkout, tạo Order và thu tiền cho cả hoa lẫn thiệp.

**Flow:**

FE gửi toàn bộ Checkout  
→ BE kiểm tra lại dữ liệu, giá, tồn kho và phí ship  
→ BE tính tổng tiền cuối cùng  
→ BE tạo Order `PENDING` và snapshot dữ liệu đã tính  
→ Customer thanh toán  
→ Payment webhook xác thực giao dịch  
→ BE chuyển Order thành `PAID`  
→ BE tạo PDF chứa mô tả Order và các link ảnh hoa/thiệp hiện có  
→ BE tạo Order trên Nhanh.vn.

**Xử lý theo loại thiệp:**

- **Free/History:** Order đã biết thiệp được chọn ngay khi tạo.
- **AI mới:** Order lưu input và giá thiệp đã chốt, nhưng chưa có ảnh thiệp.

**API BE cần:**

- Báo giá Checkout.
- Tạo Order `PENDING` từ dữ liệu Checkout.
- Tạo yêu cầu thanh toán/thanh toán lại.
- Nhận webhook và xác nhận thanh toán.
- Lấy chi tiết Order để FE điều hướng đúng bước tiếp theo.

---

## US-035 — Tạo thiệp AI sau thanh toán

**Mục đích:**

Tạo ảnh thiệp cho Order đã thanh toán và đã chọn phương án **Thiệp AI mới**.

**Flow mới:**

Customer mở Order `PAID`  
→ FE lấy input thiệp đã chốt của Order  
→ Customer chọn Template và yêu cầu tạo  
→ BE kiểm tra Order thuộc Customer và đã `PAID`  
→ BE kiểm tra Size, Template, quota `10 lượt/ngày` và giới hạn `3 lượt/mẫu hoa`  
→ BE gọi AI  
→ lưu `generated_cards` và `client_histories`  
→ cập nhật Card đang chọn của Order  
→ tạo lại PDF  
→ gọi API update Order trên Nhanh.vn.

**API BE cần:**

- Lấy dữ liệu tạo thiệp của Order đã thanh toán.
- Generate thiệp AI cho Order đã thanh toán.
- Xác nhận một kết quả làm Card hiện tại của Order nếu nghiệp vụ vẫn cho chọn kết quả.

> Tên US cũ ghi “tại Checkout”, nhưng theo quyết định mới, AI chỉ được gọi **sau khi thanh toán thành công**.

---

## US-036 — Regenerate thiệp AI

**Mục đích:**

Tạo thêm một kết quả Card từ Card hiện tại nhưng không làm mất kết quả cũ.

**Flow:**

Customer chọn một Card đã tạo  
→ chọn Template mới  
→ FE gửi `GeneratedCardId + TemplateId mới`  
→ BE lấy lại toàn bộ input từ Card nguồn  
→ giữ nguyên người gửi, người nhận, lời chúc, Size, hình thức và ảnh đính kèm  
→ chỉ thay Template  
→ kiểm tra quota  
→ gọi AI  
→ tạo `generated_cards` mới và `client_histories` mới.

```text
base_id   = GeneratedCardId nguồn
output_id = GeneratedCardId mới
type      = card
```

Nếu Card mới được chọn cho Order  
→ BE cập nhật Card đang dùng  
→ tạo lại PDF  
→ update Order trên Nhanh.vn.

**API BE cần:**

- Regenerate từ một `GeneratedCardId`.
- Xác nhận Card mới cho Order nếu việc generate và chọn Card là hai thao tác riêng.

---

# 3. Admin — Quản lý Mockup

## Nhóm US: 050 → 051 → 052 → 053

```text
US-050 — Xem danh sách
        ├── US-051 — Thêm Mockup
        ├── US-052 — Active/Inactive
        └── US-053 — Xóa mềm
```

### US-050 — Xem danh sách Mockup

BE trả danh sách Active và Inactive, chưa xóa mềm, phân trang và mới nhất trước.

### US-051 — Thêm Mockup

BE validate tên tối đa 50 ký tự, ảnh PNG/JPG tối đa 10 MB  
→ upload ảnh  
→ tạo Mockup với trạng thái `Active`.

### US-052 — Chuyển trạng thái Mockup

- `Active`: Customer được chọn cho yêu cầu mới.
- `Inactive`: không xuất hiện cho yêu cầu mới, lịch sử cũ vẫn xem được.

### US-053 — Xóa Mockup

BE xóa mềm bằng `isDelete = true`; không xóa ảnh và dữ liệu lịch sử cũ.

**API BE của nhóm:**

- Lấy danh sách Mockup cho Admin.
- Thêm Mockup.
- Chuyển Active/Inactive.
- Xóa mềm Mockup.

**Ảnh hưởng tới Customer:**

US-030 chỉ được lấy Mockup `Active` và chưa bị xóa mềm.

---

# 4. Admin — Quản lý kích thước và giá Size của thiệp

## Nhóm US: 054 → 055 → 062 → 063 → 056

```text
US-054 — Xem danh sách
        ├── US-055 — Thêm Size
        ├── US-062 — Cập nhật Size
        ├── US-063 — Active/Inactive
        └── US-056 — Xóa mềm
```

### Vai trò chung

Mỗi Size cung cấp:

- Chiều rộng và chiều cao.
- Giá cơ bản.
- Số từ tối đa.
- Trạng thái Active/Inactive.

### US-062 — Cập nhật kích thước thiệp

Admin chọn một Size  
→ BE lấy dữ liệu hiện tại  
→ Admin sửa `Width`, `Height`, giá cơ bản hoặc số từ tối đa  
→ BE kiểm tra dữ liệu và kiểm tra trùng cặp `Width + Height`, loại trừ chính record đang sửa  
→ cập nhật Size nhưng giữ nguyên trạng thái Active/Inactive và trạng thái xóa.

Giá mới chỉ áp dụng cho lần báo giá tiếp theo; Card và Order đã snapshot giá không bị tính lại.

### US-063 — Chuyển trạng thái kích thước thiệp

Admin chọn Active/Inactive  
→ BE kiểm tra Size tồn tại và chưa bị xóa mềm  
→ cập nhật trạng thái.

- `Active`: được hiển thị và sử dụng cho báo giá mới.
- `Inactive`: không được dùng cho Checkout/Order mới; dữ liệu cũ vẫn giữ nguyên.

Nếu Customer đã chọn Size trên FE nhưng chưa tạo Order, BE phải kiểm tra lại khi báo giá/tạo Order. Size vừa bị Inactive thì request bị từ chối và FE phải chọn Size khác.

### Quy tắc BE chính

- Cặp `Width + Height` không được trùng với Size chưa xóa.
- Width/Height phải lớn hơn `0`.
- Giá Size không âm.
- Customer chỉ được chọn Size Active và chưa xóa mềm.
- Inactive/xóa mềm không làm thay đổi Card hoặc Order cũ.

### Công thức giá

```text
Gõ máy      = Giá Size
Calligraphy = Giá Size + Phụ phí theo số từ
```

**API BE của nhóm:**

- Lấy danh sách Size.
- Thêm Size.
- Cập nhật Size.
- Chuyển Active/Inactive.
- Xóa mềm Size.

**Ảnh hưởng tới Customer:**

US-038 dùng Size Active để báo giá; US-039 phải snapshot giá đã chốt để thay đổi cấu hình sau này không làm đổi Order cũ.

---

# 5. Admin — Quản lý Template thiệp

## Nhóm US: 057 → 058 → 065 → 059 → 060

```text
US-057 — Xem danh sách
        ├── US-058 — Thêm Template
        ├── US-065 — Cập nhật Template
        ├── US-059 — Active/Inactive
        └── US-060 — Xóa mềm
```

### US-057 — Xem danh sách

BE trả Template chưa bị xóa mềm, gồm tên, mô tả, ảnh Preview và trạng thái.

### US-058 — Thêm Template

BE validate tên, mô tả và ảnh PNG/JPG tối đa 10 MB  
→ upload ảnh  
→ tạo Template `Active`, `isDelete = false`.

### US-065 — Cập nhật Template

Admin chọn Template  
→ BE lấy dữ liệu hiện tại  
→ cho sửa tên, mô tả hoặc ảnh  
→ nếu có ảnh mới thì validate và upload  
→ cập nhật record nhưng giữ nguyên trạng thái Active/Inactive.

Thông tin mới chỉ dùng cho lần generate tiếp theo; snapshot của Card và Order cũ không bị thay đổi.

### US-059 — Chuyển trạng thái

Template Inactive không được dùng cho lần generate mới nhưng kết quả cũ vẫn giữ nguyên.

### US-060 — Xóa Template

BE xóa mềm; không xóa ảnh và không phá dữ liệu Card/Order cũ.

**API BE của nhóm:**

- Lấy danh sách Template.
- Thêm Template.
- Cập nhật Template.
- Chuyển Active/Inactive.
- Xóa mềm Template.

> **NOTICE:** File nguồn US-059 đang rỗng. Phần trên chỉ suy ra từ tên US và quy tắc của US-057; cần BA bổ sung AC/System Test trước khi coi là define hoàn chỉnh.

---

# 6. Admin — Quản lý phụ phí thiệp viết tay

## Nhóm US: 061 → 067 → 068 → 064 → 066

```text
US-061 — Xem danh sách
        ├── US-067 — Thêm khoảng phụ phí
        ├── US-068 — Cập nhật khoảng phụ phí
        ├── US-064 — Active/Inactive
        └── US-066 — Xóa mềm
```

### Cấu hình

```text
WordFrom + WordTo + ExtraPrice + Status
```

Ví dụ:

```text
1–10 từ  → 20.000đ
11–20 từ → 30.000đ
```

### US-061 — Xem danh sách cấu hình phụ phí

Admin mở màn hình cấu hình  
→ FE gọi API danh sách  
→ BE lấy các record `isDelete = false`  
→ trả cả Active và Inactive, gồm `WordFrom`, `WordTo`, `ExtraPrice` và trạng thái.

> API này phục vụ quản trị; Customer không gọi API Admin này để tính tiền.

### US-062 — Cập nhật kích thước thiệp

US này thuộc nhóm Size ở mục 4. BE cập nhật kích thước, giá cơ bản và số từ tối đa; không đổi trạng thái và không tính lại Order cũ.

### US-063 — Chuyển trạng thái kích thước thiệp

US này thuộc nhóm Size ở mục 4. Size Inactive không được dùng cho báo giá/Order mới nhưng dữ liệu lịch sử vẫn giữ nguyên.

### US-064 — Chuyển trạng thái cấu hình phụ phí

Admin yêu cầu đổi trạng thái  
→ BE kiểm tra record tồn tại và chưa bị xóa mềm  
→ nếu chuyển sang Active, kiểm tra khoảng từ không chồng lấn với cấu hình Active khác  
→ cập nhật trạng thái.

- Chuyển sang Inactive: không dùng để tính giá mới.
- Chuyển sang Active: được dùng cho lần báo giá tiếp theo.
- Không tính lại giá của Order đã snapshot.

### US-065 — Cập nhật Template thiệp

US này thuộc nhóm Template ở mục 5. BE cho sửa tên, mô tả hoặc ảnh Template; giữ nguyên trạng thái và không thay đổi Card đã generate.

Nếu Customer chỉ mới chọn Template trên FE mà Admin chuyển nó sang Inactive, khi Customer gọi generate thì BE trả `CARD_TEMPLATE_INACTIVE/409`. Nếu request đã qua validation và đã tạo snapshot thì AI vẫn tiếp tục bằng snapshot đó.

### US-066 — Xóa cấu hình phụ phí

Theo cấu trúc chung, hành vi dự kiến là:

Admin xác nhận xóa  
→ BE kiểm tra cấu hình  
→ cập nhật `isDelete = true`  
→ cấu hình không còn được dùng để tính giá mới  
→ Order cũ không bị ảnh hưởng.

> File nguồn US-066 đang rỗng nên đây chưa phải contract chính thức.

### US-067 — Thêm cấu hình phụ phí

Admin nhập `WordFrom`, `WordTo`, `ExtraPrice` và trạng thái  
→ BE kiểm tra các giá trị là số nguyên hợp lệ  
→ nếu tạo ở trạng thái Active, kiểm tra không chồng lấn với khoảng Active khác  
→ tạo record mới với `isDelete = false`.

Record Inactive vẫn được phép tạo nhưng chưa được sử dụng để tính giá.

### US-068 — Cập nhật cấu hình phụ phí

Admin chọn một cấu hình  
→ BE lấy dữ liệu hiện tại  
→ Admin sửa khoảng từ hoặc giá  
→ BE validate, loại trừ chính record đang cập nhật khi kiểm tra chồng lấn  
→ cập nhật dữ liệu và giữ nguyên trạng thái Active/Inactive.

Giá mới chỉ áp dụng cho lần báo giá tiếp theo.

### Cách cấu hình được dùng để tính tiền

```text
Đếm số từ trong Lời chúc
→ kiểm tra không vượt số từ tối đa của Size
→ tìm đúng một cấu hình Active, chưa xóa:
   WordFrom <= WordCount <= WordTo
→ Giá Calligraphy = Giá Size + ExtraPrice
```

Nếu không tìm thấy khoảng phù hợp, BE không được tự mặc định phụ phí bằng `0`; phải từ chối báo giá/tạo Order vì cấu hình giá không khả dụng.

### Quy tắc BE chính

- `WordFrom >= 0`.
- `WordTo > 0`.
- `WordFrom <= WordTo`.
- `ExtraPrice >= 0` và là số nguyên.
- Các khoảng Active, chưa xóa mềm không được chồng lấn.
- Khi bật một cấu hình từ Inactive sang Active, phải kiểm tra chồng lấn lại.
- Chỉ cấu hình Active, chưa xóa mềm được dùng để tính giá mới.
- Thay đổi cấu hình không tính lại giá Card/Order đã chốt.

**API BE của nhóm:**

- Lấy danh sách cấu hình phụ phí.
- Thêm cấu hình.
- Cập nhật cấu hình.
- Chuyển Active/Inactive.
- Xóa mềm cấu hình.

> **NOTICE:** File nguồn US-066 đang rỗng. Hành vi xóa mềm mới chỉ được suy ra từ các US phụ phí liền kề; cần BA bổ sung AC/System Test.

---

# 7. Quan hệ giữa các nhóm US

```text
ADMIN CONFIG
│
├── Mockup: US-050 → US-053
│        └── cung cấp Mockup cho US-030/US-033
│
├── Card Size: US-054, US-055, US-056, US-062, US-063
│        └── cung cấp Size + giá cơ bản cho US-038/US-039
│
├── Card Template: US-057 → US-060, US-065
│        └── cung cấp Template cho US-035/US-036
│
└── Calligraphy Fee: US-061, US-064, US-066 → US-068
         └── cung cấp phụ phí viết tay cho US-038/US-039

CUSTOMER FLOW

US-030 → US-033 → US-040/US-034
                       │
                       └── US-038 → US-039 → thanh toán
                                                │
                                                ├── Thiệp Free/History
                                                └── US-035 → US-036
```

---

# 8. Danh sách nhanh các US còn lại

| US | Chức năng BE |
|---|---|
| US-030 | Cung cấp Combo/Mockup để FE chuẩn bị input tạo hoa; không lưu draft |
| US-033 | Generate/regenerate hoa và lưu kết quả thành công vào History |
| US-035 | Generate thiệp AI cho Order đã thanh toán |
| US-036 | Regenerate Card, giữ input và chỉ đổi Template |
| US-038 | Validate và báo giá dữ liệu Checkout đang giữ trên FE |
| US-039 | Tạo Order, thanh toán, tạo PDF và đồng bộ Nhanh.vn |
| US-050 | Admin xem danh sách Mockup |
| US-051 | Admin thêm Mockup |
| US-052 | Admin bật/tắt Mockup |
| US-053 | Admin xóa mềm Mockup |
| US-054 | Admin xem danh sách Size thiệp |
| US-055 | Admin thêm Size thiệp |
| US-056 | Admin xóa mềm Size thiệp |
| US-057 | Admin xem danh sách Template thiệp |
| US-058 | Admin thêm Template thiệp |
| US-059 | Admin bật/tắt Template — tài liệu đang rỗng |
| US-060 | Admin xóa mềm Template thiệp |
| US-061 | Admin xem danh sách phụ phí viết tay |
| US-062 | Admin cập nhật Size thiệp |
| US-063 | Admin bật/tắt Size thiệp |
| US-064 | Admin bật/tắt phụ phí viết tay |
| US-065 | Admin cập nhật Template thiệp |
| US-066 | Admin xóa phụ phí viết tay — tài liệu đang rỗng |
| US-067 | Admin thêm phụ phí viết tay |
| US-068 | Admin cập nhật phụ phí viết tay |
