# STORY-044 — Admin xem danh sách thiệp AI

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý AI Custom, tôi muốn xem danh sách các thiệp AI được khách hàng tạo, để kiểm tra ảnh kết quả, dữ liệu tạo thiệp, khách hàng sở hữu và các Order có liên quan. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |

---

## Context

Chức năng được truy cập tại **AI Custom → Thiệp** trên sidebar của Website quản trị.

- Mỗi History record có ảnh output hợp lệ được hiển thị thành **đúng 01 item** trong danh sách. Một thiệp vẫn chỉ xuất hiện **một lần** dù được liên kết với nhiều Order.
- Danh sách phục vụ **tra cứu nhanh**. Admin có thể chọn ảnh Preview để mở **Image Lightbox** mà không cần rời khỏi danh sách.
- Danh sách thiệp **không hiển thị giá**. Giá và bill snapshot được xem tại **Chi tiết Order** tương ứng.

---

## Conditions

### Preconditions
- Người dùng đã đăng nhập **Website quản trị**.
- Người dùng có quyền xem dữ liệu **AI Custom → Thiệp**.

### Trigger
> Admin chọn **AI Custom → Thiệp** trên sidebar.

---

## Flow

### Main Flow — Xem danh sách thiệp

1. Admin chọn **AI Custom → Thiệp**.
2. Backend kiểm tra **quyền truy cập** của Admin.
3. Hệ thống lấy danh sách các History record thiệp có ảnh output hợp lệ.
4. Hệ thống sắp xếp danh sách theo **ngày tạo mới nhất trước**.
5. Mỗi item trong bảng hiển thị:
   - Preview ảnh thiệp.
   - Mã thiệp.
   - Khách hàng.
   - Template.
   - Size.
   - Hình thức: **Calligraphy** hoặc **Gõ máy**.
   - Ngày tạo.
   - Thao tác.
6. Admin có thể **tìm kiếm**, **lọc**, **thay đổi số item trên trang** hoặc **chuyển trang**.
7. Admin chọn ảnh Preview để mở **Image Lightbox**.
8. Admin chọn mã thiệp hoặc **"Xem chi tiết"** trong menu thao tác để mở **Chi tiết thiệp**.

---

### Alternative Flows

#### ALT-01 — Tìm kiếm thiệp
1. Admin nhập **tên khách hàng**, **mã Order** hoặc **mã thiệp** vào ô tìm kiếm.
2. Hệ thống trả về các thiệp phù hợp mà Admin có quyền xem.
3. Điều kiện tìm kiếm được **giữ nguyên** khi Admin chuyển trang.

#### ALT-02 — Lọc danh sách
1. Admin mở bộ lọc.
2. Admin có thể lọc theo:
   - **Size**.
   - **Hình thức**: Calligraphy hoặc Gõ máy.
3. Hệ thống áp dụng **đồng thời** các điều kiện lọc đã chọn.
4. Admin có thể xóa từng điều kiện hoặc đặt lại **toàn bộ bộ lọc**.

#### ALT-03 — Thay đổi số item trên trang
1. Danh sách mặc định hiển thị **10 item** trên một trang.
2. Admin chọn một trong các giá trị: **5, 10, 20, 30, 40 hoặc 50** item/trang.
3. Hệ thống tải lại danh sách theo số lượng đã chọn và đưa Admin về **trang đầu tiên**.

#### ALT-04 — Thiệp liên kết nhiều Order
- Thiệp vẫn chỉ xuất hiện **đúng 01 item** trong danh sách.

#### ALT-05 — Xem nhanh ảnh
1. Admin chọn Preview tại danh sách.
2. Hệ thống mở **Image Lightbox**, hiển thị ảnh theo đúng tỷ lệ.
3. Admin có thể đóng để quay lại **vị trí trước đó**.

---

### Exception Flows

#### EXC-01 — Không có dữ liệu
- Hệ thống hiển thị **empty state** khi chưa có thiệp hoặc không có kết quả phù hợp với điều kiện tìm kiếm và bộ lọc.

#### EXC-02 — Không tải được dữ liệu
- Hệ thống hiển thị **error state**, không hiển thị dữ liệu thiếu như kết quả hoàn chỉnh và cho phép Admin **tải lại**.

#### EXC-03 — Không có quyền truy cập
- Backend **từ chối request**, không trả ảnh, thông tin khách hàng, dữ liệu thiệp, Order liên kết hoặc metadata nhạy cảm.

#### EXC-04 — File ảnh thiệp không còn khả dụng
1. Hệ thống **giữ History record** và metadata của thiệp.
2. Tại danh sách, Preview được thay bằng **placeholder**.
3. Hệ thống **không tự động** gọi AI để tạo lại ảnh.

#### EXC-05 — Một ảnh tải lỗi tạm thời trong danh sách
1. Lỗi của một file chỉ làm Preview tương ứng hiển thị **placeholder** và **không làm hỏng** toàn bộ danh sách.
2. Hệ thống cho phép Admin **tải lại trang** hoặc **tải lại ảnh** tương ứng.

---

## Acceptance Criteria

### AC-001 — Hiển thị danh sách thiệp
- **Given:** Admin có quyền xem AI Custom.
- **When:** Admin mở AI Custom → Thiệp.
- **Then:** Hệ thống hiển thị mỗi History record thiệp thành **đúng 01 item**.
- **And:** Sắp xếp ngày tạo **mới nhất trước**.

### AC-002 — Thiệp liên kết nhiều Order chỉ xuất hiện một lần
- **Given:** Một thiệp liên kết với nhiều Order.
- **When:** Danh sách thiệp hiển thị.
- **Then:** Thiệp chỉ xuất hiện **đúng 01 item**.

### AC-003 — Tìm kiếm
- **Given:** Danh sách có dữ liệu phù hợp.
- **When:** Admin tìm theo tên khách hàng, mã Order hoặc mã thiệp.
- **Then:** Hệ thống chỉ hiển thị các thiệp thỏa **điều kiện tìm kiếm**.

### AC-004 — Lọc danh sách
- **Given:** Admin chọn Size và/hoặc hình thức.
- **When:** Áp dụng bộ lọc.
- **Then:** Hệ thống chỉ hiển thị các thiệp thỏa **toàn bộ điều kiện** đã chọn.

### AC-005 — Phân trang
- **Given:** Danh sách có nhiều hơn số item trên một trang.
- **When:** Admin chọn 5, 10, 20, 30, 40 hoặc 50 item/trang.
- **Then:** Hệ thống phân trang theo số lượng đã chọn.
- **And:** Mặc định là **10 item/trang**.

### AC-006 — Image Lightbox
- **Given:** File ảnh còn khả dụng.
- **When:** Admin chọn Preview.
- **Then:** Hệ thống mở **Image Lightbox**.
- **And:** Hiển thị ảnh đúng tỷ lệ.

### AC-007 — File ảnh không còn khả dụng
- **Given:** History record còn nhưng file ảnh mất, hỏng hoặc không thể truy cập.
- **When:** Admin xem danh sách.
- **Then:** Danh sách hiển thị **placeholder**.

### AC-008 — Không hiển thị giá/bill
- **Given:** Một thiệp có thể liên kết với nhiều Order có giá khác nhau.
- **When:** Admin xem danh sách.
- **Then:** Hệ thống **không hiển thị** giá thiệp hoặc bill snapshot.
- **And:** Admin xem giá tại **Chi tiết Order** tương ứng.

### AC-009 — Không có quyền truy cập
- **Given:** Người dùng không có quyền xem AI Custom.
- **When:** Người dùng gọi API danh sách.
- **Then:** Backend từ chối.
- **And:** Không trả ảnh, dữ liệu thiệp hoặc thông tin khách hàng.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|
| **BR-126** | Một item theo History record | AI Custom - Thiệp | Mỗi History record thiệp có ảnh output hợp lệ được hiển thị thành đúng 01 item, không phụ thuộc số Order liên kết. | Hệ thống hiển thị danh sách thiệp AI. | Hệ thống hiển thị mỗi History record thiệp có ảnh output hợp lệ thành đúng 01 item. | Không tạo nhiều item cho cùng một thiệp dù thiệp được liên kết với nhiều Order. | Draft | v0 | 2026-08-14 |
| **BR-127** | Thứ tự mặc định | AI Custom - Thiệp | Danh sách thiệp sắp xếp theo ngày tạo mới nhất trước. | Hệ thống hiển thị danh sách thiệp. | Hệ thống sắp xếp danh sách theo ngày tạo mới nhất trước. | Không áp dụng thứ tự cũ hơn trước theo mặc định. | Draft | v0 | 2026-08-14 |
| **BR-128** | Dữ liệu danh sách | AI Custom - Thiệp | Danh sách chỉ hiển thị Preview, mã thiệp, khách hàng, Template, Size, hình thức, ngày tạo và thao tác. | Hệ thống hiển thị danh sách thiệp. | Hệ thống chỉ hiển thị các thông tin được quy định cho danh sách. | Không hiển thị dữ liệu chi tiết đầy đủ tại danh sách. | Draft | v0 | 2026-08-14 |
| **BR-129** | Giá và bill | AI Custom - Thiệp | Danh sách và Chi tiết thiệp không hiển thị giá. Giá và bill snapshot thuộc từng Order và được xem tại Chi tiết Order. | Admin xem danh sách hoặc Chi tiết thiệp. | Hệ thống không hiển thị giá hoặc bill snapshot của thiệp. | Giá và bill snapshot được xem tại Chi tiết Order tương ứng. | Draft | v0 | 2026-08-14 |
| **BR-130** | File không khả dụng | AI Custom - Thiệp | Mất hoặc hỏng file không xóa History record, không đổi lần generate trước thành thất bại và không kích hoạt AI. | File ảnh thiệp không còn khả dụng. | Hệ thống giữ History record và không đổi lần generate trước thành thất bại. | Không kích hoạt AI để tạo lại ảnh. | Draft | v0 | 2026-08-14 |
| **BR-131** | Điều hướng AI Custom | AI Custom - Thiệp | Mục Thiệp được hiển thị bên dưới nhóm AI Custom trên sidebar và chỉ hiển thị cho tài khoản có quyền tương ứng. | Người dùng truy cập Website quản trị. | Hệ thống hiển thị mục Thiệp bên dưới nhóm AI Custom cho tài khoản có quyền tương ứng. | Không hiển thị mục Thiệp cho tài khoản không có quyền tương ứng. | Draft | v0 | 2026-08-14 |
| **BR-132** | Quyền truy cập | AI Custom - Phân quyền | Backend chỉ cung cấp danh sách, chi tiết, ảnh và thông tin khách hàng cho Admin có quyền quản lý AI Custom. | Backend nhận request danh sách, chi tiết, ảnh hoặc thông tin khách hàng. | Backend kiểm tra quyền quản lý AI Custom trước khi cung cấp dữ liệu, bao gồm họ tên, số điện thoại và email của khách hàng nếu có. | Không cung cấp ảnh, thông tin khách hàng hoặc metadata nhạy cảm cho người dùng không có quyền quản lý AI Custom. | Draft | v0 | 2026-08-14 |

---

## References

### Business Rules
`BR-126` · `BR-127` · `BR-128` · `BR-129` · `BR-130` · `BR-131` · `BR-132`

### Dependencies
- **STORY-035** — Tạo thiệp cá nhân hóa bằng AI tại Checkout

---

## Non-Functional Requirements

- API danh sách phải **phân trang phía server**.
- Danh sách **p95 ≤ 2 giây** với tối đa 50 item/trang trong điều kiện bình thường, không tính thời gian tải ảnh từ storage.
- API metadata **không nhúng** binary/Base64 và không expose đường dẫn storage nội bộ.
- Backend kiểm tra **phân quyền** trong mọi request, không chỉ dựa trên sidebar hoặc UI.
- UI có đầy đủ: **loading**, **empty** và **error state**.
- Điều kiện **tìm kiếm, bộ lọc** và **số item/trang** được giữ khi Admin chuyển trang và quay lại từ trang chi tiết trong cùng phiên làm việc.
- Một file lỗi **không làm hỏng** toàn bộ danh sách.

---

## Out of Scope

- Tạo, tạo lại, chỉnh sửa hoặc xóa thiệp.
- Thay đổi dữ liệu khách hàng.
- Thay đổi Order hoặc bill.
- Export Excel.
- Tải xuống thiệp ngoài phạm vi STORY-046.
