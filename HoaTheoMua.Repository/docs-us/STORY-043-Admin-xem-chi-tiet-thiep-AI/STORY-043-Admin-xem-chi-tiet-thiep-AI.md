# STORY-043 — Admin xem chi tiết thiệp AI

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý AI Custom, tôi muốn xem thông tin chi tiết của một thiệp AI được khách hàng tạo, để kiểm tra ảnh kết quả, dữ liệu tạo thiệp, khách hàng sở hữu và các Order có liên quan. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |

---

## Context

Chức năng được mở từ **STORY-044** (Admin xem danh sách thiệp AI) khi Admin chọn mã thiệp hoặc **"Xem chi tiết"**, hoặc truy cập trực tiếp Chi tiết thiệp bằng mã thiệp hợp lệ.

- Trang Chi tiết thiệp hiển thị **đầy đủ thông tin** của History record thiệp tương ứng.
- Đối với thiệp **Calligraphy**: ảnh output không hiển thị Người gửi, Người nhận và Lời chúc; các nội dung này vẫn được lưu trong History record và **hiển thị riêng** tại Chi tiết thiệp để Staff/Admin sử dụng khi viết tay lên thiệp.
- Một thiệp có thể **chưa liên kết Order**, **liên kết một Order** hoặc **nhiều Order**.
- Chi tiết thiệp **không hiển thị giá**. Giá và bill snapshot được xem tại **Chi tiết Order** tương ứng.

---

## Conditions

### Preconditions
- Người dùng đã đăng nhập **Website quản trị**.
- Người dùng có quyền xem dữ liệu **AI Custom → Thiệp**.

### Trigger
> Admin chọn mã thiệp hoặc **"Xem chi tiết"** từ STORY-044, hoặc Admin mở trực tiếp Chi tiết thiệp bằng mã thiệp hợp lệ.

---

## Flow

### Main Flow

1. Admin yêu cầu mở **Chi tiết thiệp**.
2. Backend kiểm tra **quyền truy cập** và History record của thiệp.
3. Hệ thống hiển thị:
   - Ảnh kết quả thiệp.
   - Mã thiệp.
   - Thông tin khách hàng: họ tên, số điện thoại và email (nếu có).
   - Người gửi.
   - Người nhận.
   - Lời chúc.
   - Ảnh người dùng đính kèm (nếu có).
   - Template.
   - Size.
   - Hình thức.
   - Ngày tạo.
   - Trạng thái file.
   - Danh sách đầy đủ các Order liên kết.
4. Admin có thể chọn ảnh thiệp để mở **Image Lightbox**.
5. Nếu có Order liên kết, Admin có thể chọn mã Order để mở **Chi tiết Order**.
6. Nếu file ảnh còn khả dụng, hệ thống hiển thị thao tác **"Tải ảnh thiệp xuống"**.

---

### Alternative Flows

#### ALT-01 — Thiệp liên kết nhiều Order
- Trang Chi tiết thiệp hiển thị **toàn bộ Order** liên kết; mỗi Order giữ thông tin bill riêng tại Chi tiết Order.

#### ALT-02 — Xem nhanh ảnh
1. Admin chọn ảnh tại trang Chi tiết thiệp.
2. Hệ thống mở **Image Lightbox**, hiển thị ảnh theo đúng tỷ lệ.
3. Admin có thể đóng để quay lại vị trí trước đó.

---

### Exception Flows

#### EXC-01 — Không tải được dữ liệu chi tiết
1. Hệ thống hiển thị **error state**.
2. Không hiển thị dữ liệu thiếu như kết quả hoàn chỉnh.
3. Cho phép Admin **tải lại**.

#### EXC-02 — Không có quyền truy cập
1. Backend **từ chối request**.
2. Không trả ảnh, thông tin khách hàng, dữ liệu thiệp, Order liên kết hoặc metadata nhạy cảm.

#### EXC-03 — Không tìm thấy thiệp
1. Backend **không trả** dữ liệu chi tiết.
2. Hệ thống hiển thị trạng thái tài nguyên **không tồn tại hoặc không thể truy cập**.

#### EXC-04 — File ảnh thiệp không còn khả dụng
1. Hệ thống **giữ History record** và metadata của thiệp.
2. Trang chi tiết hiển thị trạng thái **"Không khả dụng"** và thông báo: **"Ảnh không còn khả dụng."**
3. Thao tác **"Tải ảnh thiệp xuống"** bị vô hiệu hóa.
4. Khi hover vào thao tác bị vô hiệu hóa, hệ thống hiển thị tooltip **"Ảnh không còn khả dụng."**
5. Hệ thống **không tự động** gọi AI để tạo lại ảnh.

---

## Acceptance Criteria

### AC-001 — Hiển thị nhiều Order liên kết
- **Given:** Một thiệp liên kết với nhiều Order.
- **When:** Admin mở Chi tiết thiệp.
- **Then:** Hệ thống hiển thị **đầy đủ** các Order liên kết.

### AC-002 — Mở Image Lightbox
- **Given:** File ảnh còn khả dụng.
- **When:** Admin chọn ảnh tại trang Chi tiết thiệp.
- **Then:** Hệ thống mở **Image Lightbox**.
- **And:** Hiển thị ảnh đúng tỷ lệ.

### AC-003 — Xem chi tiết thiệp thành công
- **Given:** Admin có quyền và thiệp tồn tại.
- **When:** Admin chọn mã thiệp hoặc **"Xem chi tiết"**.
- **Then:** Hệ thống hiển thị đầy đủ dữ liệu thiệp, khách hàng và Order liên kết.

### AC-004 — File ảnh không còn khả dụng
- **Given:** History record còn nhưng file ảnh mất, hỏng hoặc không thể truy cập.
- **When:** Admin xem chi tiết thiệp.
- **Then:** Trang chi tiết vẫn hiển thị metadata.
- **And:** Hiển thị **"Ảnh không còn khả dụng."**
- **And:** Thao tác tải xuống bị **vô hiệu hóa**.

### AC-005 — Không hiển thị giá/bill
- **Given:** Một thiệp có thể liên kết với nhiều Order có giá khác nhau.
- **When:** Admin xem Chi tiết thiệp.
- **Then:** Hệ thống **không hiển thị** giá thiệp hoặc bill snapshot.
- **And:** Admin xem giá tại **Chi tiết Order** tương ứng.

### AC-006 — Không có quyền truy cập
- **Given:** Người dùng không có quyền xem AI Custom.
- **When:** Người dùng gọi API Chi tiết thiệp.
- **Then:** Backend từ chối.
- **And:** Không trả ảnh, dữ liệu thiệp hoặc thông tin khách hàng.

### AC-007 — Hiển thị nội dung thiệp Calligraphy
- **Given:** Thiệp có Hình thức Calligraphy và History record có Người gửi, Người nhận, Lời chúc.
- **When:** Admin mở Chi tiết thiệp.
- **Then:** Ảnh output **không hiển thị** Người gửi, Người nhận và Lời chúc.
- **And:** Hệ thống vẫn **hiển thị riêng** Người gửi, Người nhận và Lời chúc trong thông tin chi tiết thiệp.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|
| **BR-121** | Dữ liệu chi tiết | AI Custom - Thiệp | Người gửi, Người nhận, Lời chúc, ảnh đính kèm, trạng thái file và danh sách Order liên kết chỉ hiển thị đầy đủ tại Chi tiết thiệp. | Admin mở Chi tiết thiệp. | Hệ thống hiển thị đầy đủ Người gửi, Người nhận, Lời chúc, ảnh đính kèm, trạng thái file và danh sách Order liên kết. | Các thông tin này không hiển thị đầy đủ tại danh sách. | Draft | v0 | 2026-08-15 |
| **BR-122** | Giá và bill | AI Custom - Thiệp | Danh sách và Chi tiết thiệp không hiển thị giá. Giá và bill snapshot thuộc từng Order và được xem tại Chi tiết Order. | Admin xem danh sách hoặc Chi tiết thiệp. | Hệ thống không hiển thị giá hoặc bill snapshot của thiệp. | Giá và bill snapshot được xem tại Chi tiết Order tương ứng. | Draft | v0 | 2026-08-15 |
| **BR-123** | File không khả dụng | AI Custom - Thiệp | Mất hoặc hỏng file không xóa History record, không đổi lần generate trước thành thất bại và không kích hoạt AI. | File ảnh thiệp không còn khả dụng. | Hệ thống giữ History record và không đổi lần generate trước thành thất bại. | Không kích hoạt AI để tạo lại ảnh. | Draft | v0 | 2026-08-15 |
| **BR-125** | Quyền truy cập | AI Custom - Thiệp | Backend chỉ cung cấp danh sách, chi tiết, ảnh và dữ liệu khách hàng cho Admin có quyền quản lý AI Custom. | Backend nhận request danh sách, chi tiết, ảnh hoặc dữ liệu khách hàng. | Backend kiểm tra quyền quản lý AI Custom trước khi cung cấp dữ liệu. | Không cung cấp dữ liệu cho người dùng không có quyền quản lý AI Custom. | Draft | v0 | 2026-08-15 |

---

## References

### Business Rules
`BR-121` · `BR-122` · `BR-123` · `BR-125`

### Dependencies
- **STORY-044** — Admin xem danh sách thiệp AI
- **STORY-046** — Admin tải xuống ảnh thiệp

---

## Non-Functional Requirements

- Chi tiết thiệp **p95 ≤ 2 giây** trong điều kiện bình thường, không tính thời gian tải ảnh.
- API metadata **không nhúng** binary/Base64 và không expose đường dẫn storage nội bộ.
- Backend kiểm tra **phân quyền** trong mọi request, không chỉ dựa trên sidebar hoặc UI.
- UI có đầy đủ: **loading**, **empty** và **error state**.

---

## Out of Scope

- Tạo, tạo lại, chỉnh sửa hoặc xóa thiệp.
- Thay đổi dữ liệu khách hàng.
- Thay đổi Order hoặc bill.
- Export Excel.
- Tải xuống thiệp ngoài phạm vi STORY-046.
