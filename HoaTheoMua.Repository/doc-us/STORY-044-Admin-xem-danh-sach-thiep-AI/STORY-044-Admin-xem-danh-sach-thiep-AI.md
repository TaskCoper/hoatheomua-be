# STORY-044 — Admin xem danh sách lịch sử thiệp AI

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
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Chức năng được truy cập tại **AI Custom → Thiệp** trên sidebar của Website quản trị.
- Mỗi History record có ảnh output hợp lệ được hiển thị thành **đúng 01 item** trong danh sách. 
- Một thiệp vẫn chỉ xuất hiện một lần dù được liên kết với nhiều Order.
- Danh sách phục vụ tra cứu nhanh. Admin có thể chọn ảnh Preview để mở Image Lightbox mà không cần rời khỏi danh sách.
- Danh sách thiệp không hiển thị giá. Giá và bill snapshot được xem tại Chi tiết Order tương ứng.

---

## Conditions

### Preconditions
- Người dùng đã đăng nhập Website quản trị.
- Người dùng có quyền xem dữ liệu AI Custom → Thiệp.

### Trigger
> Admin chọn **AI Custom → Thiệp** trên sidebar.

---

## Flow

### Main Flow: MF – Xem danh sách thiệp

1. Admin chọn AI Custom → Thiệp.
2. Backend kiểm tra quyền truy cập của Admin.
3. Hệ thống lấy danh sách các History record thiệp có ảnh output hợp lệ.
4. Hệ thống sắp xếp danh sách theo ngày tạo mới nhất trước.
5. Mỗi item trong bảng hiển thị:
   - Preview ảnh thiệp.
   - Mã thiệp.
   - Khách hàng.
   - Template.
   - Size.
   - Hình thức: Calligraphy hoặc Gõ máy.
   - Ngày tạo.
   - Thao tác.
6. Admin có thể tìm kiếm, lọc, thay đổi số item trên trang hoặc chuyển trang.
7. Admin chọn ảnh Preview để mở Image Lightbox.
8. Admin chọn mã thiệp hoặc “Xem chi tiết” trong menu thao tác để mở Chi tiết thiệp.

---

### Alternative Flows

#### ALT-01 — Tìm kiếm thiệp
1. Admin nhập tên khách hàng, mã Order hoặc mã thiệp vào ô tìm kiếm.
2. Hệ thống trả về các thiệp phù hợp mà Admin có quyền xem.
3. Điều kiện tìm kiếm được giữ nguyên khi Admin chuyển trang.

#### ALT-02 — Lọc danh sách
1. Admin mở bộ lọc.
2. Admin có thể lọc theo:
   - Size.
   - Hình thức: Calligraphy hoặc Gõ máy.
3. Hệ thống áp dụng đồng thời các điều kiện lọc đã chọn.
4. Admin có thể xóa từng điều kiện hoặc đặt lại toàn bộ bộ lọc.

#### ALT-03 — Thay đổi số item trên trang
1. Danh sách mặc định hiển thị 10 item trên một trang.
2. Admin chọn một trong các giá trị: 5, 10, 20, 30, 40 hoặc 50 item/trang.
3. Hệ thống tải lại danh sách theo số lượng đã chọn và đưa Admin về trang đầu tiên.

#### ALT-04 — Thiệp liên kết nhiều Order
1. Thiệp vẫn chỉ xuất hiện đúng 01 item trong danh sách.

#### ALT-05 — Xem nhanh ảnh
1. Admin chọn Preview tại danh sách.
2. Hệ thống mở Image Lightbox, hiển thị ảnh theo đúng tỷ lệ và cho phép đóng để quay lại vị trí trước đó.

---

### Exception Flows

#### EXC-01 — Không có dữ liệu
1. Hệ thống hiển thị empty state khi chưa có thiệp hoặc không có kết quả phù hợp với điều kiện tìm kiếm và bộ lọc.

#### EXC-02 — Không tải được dữ liệu
1. Hệ thống hiển thị error state, không hiển thị dữ liệu thiếu như kết quả hoàn chỉnh và cho phép Admin tải lại.

#### EXC-03 — Không có quyền truy cập
1. Backend từ chối request.
2. Hệ thống không trả ảnh, thông tin khách hàng, dữ liệu thiệp, Order liên kết hoặc metadata nhạy cảm.

#### EXC-04 — File ảnh thiệp không còn khả dụng
1. Hệ thống giữ History record và metadata của thiệp.
2. Tại danh sách, Preview được thay bằng placeholder.
3. Hệ thống không tự động gọi AI để tạo lại ảnh.

#### EXC-05 — Một ảnh tải lỗi tạm thời trong danh sách
1. Lỗi của một file chỉ làm Preview tương ứng hiển thị placeholder và không được làm hỏng toàn bộ danh sách.
2. Hệ thống cho phép Admin tải lại trang hoặc tải lại ảnh tương ứng.

---

## Acceptance Criteria

### AC-001 – Hiển thị danh sách thiệp hợp lệ
- **Given:** Admin có quyền xem AI Custom
- **When:** Admin mở AI Custom → Thiệp
- **Then:** hệ thống hiển thị mỗi History record thiệp thành đúng 01 item
- **And:** sắp xếp ngày tạo mới nhất trước.

### AC-002 – Thiệp liên kết nhiều Order
- **Given:** một thiệp liên kết với nhiều Order
- **When:** danh sách thiệp hiển thị
- **Then:** thiệp chỉ xuất hiện đúng 01 item.

### AC-003 – Tìm kiếm thiệp
- **Given:** danh sách có dữ liệu phù hợp
- **When:** Admin tìm theo tên khách hàng, mã Order hoặc mã thiệp
- **Then:** hệ thống chỉ hiển thị các thiệp thỏa điều kiện tìm kiếm.

### AC-004 – Lọc danh sách thiệp
- **Given:** Admin chọn Size và/hoặc hình thức
- **When:** áp dụng bộ lọc
- **Then:** hệ thống chỉ hiển thị các thiệp thỏa toàn bộ điều kiện đã chọn.

### AC-005 – Phân trang
- **Given:** Danh sách có nhiều hơn số item trên một trang
- **When:** Admin chọn 5, 10, 20, 30, 40 hoặc 50 item/trang
- **Then:** Hệ thống phân trang theo số lượng đã chọn
- **And:** Mặc định là 10 item/trang.

### AC-006 – Mở Image Lightbox từ Preview
- **Given:** File ảnh còn khả dụng
- **When:** Admin chọn Preview
- **Then:** Hệ thống mở Image Lightbox
- **And:** Hiển thị ảnh đúng tỷ lệ.

### AC-007 – Placeholder khi file hỏng
- **Given:** History record còn nhưng file ảnh mất, hỏng hoặc không thể truy cập
- **When:** Admin xem danh sách
- **Then:** danh sách hiển thị placeholder.

### AC-008 – Không hiển thị giá tại danh sách
- **Given:** một thiệp có thể liên kết với nhiều Order có giá khác nhau
- **When:** Admin xem danh sách
- **Then:** hệ thống không hiển thị giá thiệp hoặc bill snapshot
- **And:** Admin xem giá tại Chi tiết Order tương ứng.

### AC-009 – Phân quyền truy cập danh sách
- **Given:** người dùng không có quyền xem AI Custom
- **When:** người dùng gọi API danh sách
- **Then:** backend từ chối
- **And:** không trả ảnh, dữ liệu thiệp hoặc thông tin khách hàng.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-126**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8b3abc1f-d5cc-4ea4-9217-38dfe0c52c35) | Một item theo History record | AI Custom - Thiệp | Mỗi History record thiệp có ảnh output hợp lệ được hiển thị thành đúng 01 item, không phụ thuộc số Order liên kết. | Hệ thống hiển thị danh sách thiệp AI. | Hệ thống hiển thị mỗi History record thiệp có ảnh output hợp lệ thành đúng 01 item. | Không tạo nhiều item cho cùng một thiệp dù thiệp được liên kết với nhiều Order. | Đức Bình | STORY-044 | Draft | v0 | 2026-08-14 |
| [**BR-127**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5fc21993-cf12-4c58-bf64-db5e9b4f2763) | Thứ tự mặc định | AI Custom - Thiệp | Danh sách thiệp sắp xếp theo ngày tạo mới nhất trước. | Hệ thống hiển thị danh sách thiệp. | Hệ thống sắp xếp danh sách theo ngày tạo mới nhất trước. | Không áp dụng thứ tự cũ hơn trước theo mặc định. | Đức Bình | STORY-044 | Draft | v0 | 2026-08-14 |
| [**BR-128**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bb1e1878-9fac-44b1-9b14-1137cc43bc0b) | Dữ liệu danh sách | AI Custom - Thiệp | Danh sách chỉ hiển thị Preview, mã thiệp, khách hàng, Template, Size, hình thức, ngày tạo và thao tác. | Hệ thống hiển thị danh sách thiệp. | Hệ thống chỉ hiển thị các thông tin được quy định cho danh sách. | Không hiển thị dữ liệu chi tiết đầy đủ tại danh sách. | Đức Bình | STORY-044 | Draft | v0 | 2026-08-14 |
| [**BR-129**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3fda951f-01f8-40e1-8129-5e2ee16ea7f3) | Giá và bill | AI Custom - Thiệp | Danh sách và Chi tiết thiệp không hiển thị giá. Giá và bill snapshot thuộc từng Order và được xem tại Chi tiết Order. | Admin xem danh sách hoặc Chi tiết thiệp. | Hệ thống không hiển thị giá hoặc bill snapshot của thiệp. | Giá và bill snapshot được xem tại Chi tiết Order tương ứng. | Đức Bình | STORY-044 | Draft | v0 | 2026-08-14 |
| [**BR-130**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2c5fddec-4c32-49d6-abe8-477073969fd6) | File không khả dụng | AI Custom - Thiệp | Mất hoặc hỏng file không xóa History record, không đổi lần generate trước thành thất bại và không kích hoạt AI. | File ảnh thiệp không còn khả dụng. | Hệ thống giữ History record và không đổi lần generate trước thành thất bại. | Không kích hoạt AI để tạo lại ảnh. | Đức Bình | STORY-044 | Draft | v0 | 2026-08-14 |
| [**BR-131**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/270060ac-9a31-4c28-b059-3879c464bace) | Điều hướng AI Custom | AI Custom - Thiệp | Mục Thiệp được hiển thị bên dưới nhóm AI Custom trên sidebar và chỉ hiển thị cho tài khoản có quyền tương ứng. | Người dùng truy cập Website quản trị. | Hệ thống hiển thị mục Thiệp bên dưới nhóm AI Custom cho tài khoản có quyền tương ứng. | Không hiển thị mục Thiệp cho tài khoản không có quyền tương ứng. | Đức Bình | STORY-044 | Draft | v0 | 2026-08-14 |
| [**BR-132**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/556f0aa7-fe97-49c4-82fb-d64b637f4577) | Quyền truy cập | AI Custom - Phân quyền | Backend chỉ cung cấp danh sách, chi tiết, ảnh và thông tin khách hàng cho Admin có quyền quản lý AI Custom. | Backend nhận request danh sách, chi tiết, ảnh hoặc thông tin khách hàng. | Backend kiểm tra quyền quản lý AI Custom trước khi cung cấp dữ liệu, bao gồm họ tên, số điện thoại và email của khách hàng nếu có. | Không cung cấp ảnh, thông tin khách hàng hoặc metadata nhạy cảm cho người dùng không có quyền quản lý AI Custom. | Đức Bình | STORY-044 | Draft | v0 | 2026-08-14 |

---

## References

### Dependencies
- [**STORY-035**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

---

## Non-Functional Requirements

- API danh sách phải phân trang phía server.
- Danh sách **p95 ≤ 2 giây** với tối đa 50 item/trang trong điều kiện bình thường, không tính thời gian tải ảnh từ storage.
- API metadata không nhúng binary/Base64 và không expose đường dẫn storage nội bộ.
- Backend kiểm tra phân quyền trong mọi request, không chỉ dựa trên sidebar hoặc UI.
- UI có loading, empty và error state.
- Điều kiện tìm kiếm, bộ lọc và số item/trang được giữ khi Admin chuyển trang và quay lại từ trang chi tiết trong cùng phiên làm việc.
- Một file lỗi không làm hỏng toàn bộ danh sách.

---

## Out of Scope

- Tạo, tạo lại, chỉnh sửa hoặc xóa thiệp.
- Thay đổi dữ liệu khách hàng.
- Thay đổi Order hoặc bill.
- Export Excel.
- Tải xuống thiệp ngoài phạm vi STORY-046.
