# STORY-043 — Admin xem chi tiết thiệp AI

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý AI Custom, tôi muốn xem thông tin chi tiết của một thiệp AI được khách hàng tạo, để kiểm tra ảnh kết quả, dữ liệu tạo thiệp, khách hàng sở hữu và các Order có liên quan. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Võ Gia Huy |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Chức năng được mở từ STORY-044 khi Admin chọn mã thiệp hoặc "Xem chi tiết", hoặc truy cập trực tiếp Chi tiết thiệp bằng mã thiệp hợp lệ.
---

## Conditions

### Preconditions

- Người dùng đã đăng nhập Website quản trị.
- Người dùng có quyền xem dữ liệu AI Custom - Thiệp.

### Trigger

> - Admin chọn mã thiệp hoặc "Xem chi tiết" từ STORY-044. Hoặc Admin mở trực tiếp Chi tiết thiệp bằng mã thiệp hợp lệ.


---

## Flow

### Main Flow

1. Admin yêu cầu mở Chi tiết thiệp.
2. Backend kiểm tra quyền truy cập và History record của thiệp.
3. Hệ thống hiển thị ảnh kết quả thiệp.
4. Hệ thống hiển thị Mã thiệp.
5. Hệ thống hiển thị thông tin khách hàng gồm họ tên, số điện thoại và email nếu có.
6. Hệ thống hiển thị Nội dung thiệp/Lời chúc.
7. Hệ thống hiển thị ảnh người dùng đính kèm nếu có.
8. Hệ thống hiển thị Template, Size và Hình thức.
9. Hệ thống hiển thị ngày tạo, trạng thái file và danh sách đầy đủ các Order liên kết.
10. Admin có thể chọn ảnh thiệp để mở Image Lightbox.
11. Nếu có Order liên kết, Admin có thể chọn mã Order để mở Chi tiết Order.
12. Nếu file ảnh còn khả dụng, hệ thống hiển thị thao tác "Tải ảnh thiệp xuống".

### Alternative Flows

#### ALT-01 — Thiệp liên kết nhiều Order
- Trang Chi tiết thiệp hiển thị toàn bộ Order liên kết.
- Mỗi Order giữ thông tin bill riêng tại Chi tiết Order.

#### ALT-02 — Xem nhanh ảnh
- Admin chọn ảnh tại trang Chi tiết thiệp.
- Hệ thống mở Image Lightbox.
- Hệ thống hiển thị ảnh theo đúng tỷ lệ.
- Admin đóng Image Lightbox để quay lại vị trí trước đó.

### Exception Flows

#### EXC-01 — Không tải được dữ liệu chi tiết
- Hệ thống hiển thị error state.
- Hệ thống không hiển thị dữ liệu thiếu như kết quả hoàn chỉnh.
- Hệ thống cho phép Admin tải lại.

#### EXC-02 — Không có quyền truy cập
- Backend từ chối request.
- Backend không trả ảnh, thông tin khách hàng, dữ liệu thiệp, Order liên kết hoặc metadata nhạy cảm.

#### EXC-03 — Không tìm thấy thiệp
- Backend không trả dữ liệu chi tiết.
- Hệ thống hiển thị trạng thái tài nguyên không tồn tại hoặc không thể truy cập.

#### EXC-04 — File ảnh thiệp không còn khả dụng
- Hệ thống giữ History record và metadata của thiệp.
- Trang chi tiết hiển thị trạng thái "Không khả dụng".
- Trang chi tiết hiển thị thông báo "Ảnh không còn khả dụng."
- Thao tác "Tải ảnh thiệp xuống" bị vô hiệu hóa.
- Khi hover vào thao tác bị vô hiệu hóa, hệ thống hiển thị tooltip "Ảnh không còn khả dụng."
- Hệ thống không tự động gọi AI để tạo lại ảnh.

---

## Acceptance Criteria

### AC-001
- **Given**: Một thiệp liên kết với nhiều Order.
- **When**: Admin mở Chi tiết thiệp.
- **Then**: Hệ thống hiển thị đầy đủ các Order liên kết.

### AC-002
- **Given**: File ảnh còn khả dụng.
- **When**: Admin chọn ảnh tại trang Chi tiết thiệp.
- **Then**:
  - Hệ thống mở Image Lightbox.
  - Hiển thị ảnh đúng tỷ lệ.

### AC-003
- **Given**: Admin có quyền và thiệp tồn tại.
- **When**: Admin chọn mã thiệp hoặc "Xem chi tiết".
- **Then**:
  - Hệ thống hiển thị đầy đủ dữ liệu thiệp, khách hàng và Order liên kết.
  - Hệ thống không hiển thị Người gửi hoặc Người nhận.

### AC-004
- **Given**: History record còn nhưng file ảnh mất, hỏng hoặc không thể truy cập.
- **When**: Admin xem chi tiết thiệp.
- **Then**:
  - Trang chi tiết vẫn hiển thị metadata.
  - Hiển thị "Ảnh không còn khả dụng."
  - Thao tác tải xuống bị vô hiệu hóa.

### AC-005
- **Given**: Một thiệp có thể liên kết với nhiều Order có giá khác nhau.
- **When**: Admin xem Chi tiết thiệp.
- **Then**:
  - Hệ thống không hiển thị giá thiệp hoặc bill snapshot.
  - Admin xem giá tại Chi tiết Order tương ứng.

### AC-006
- **Given**: Người dùng không có quyền xem AI Custom.
- **When**: Người dùng gọi API Chi tiết thiệp.
- **Then**:
  - Backend từ chối.
  - Không trả ảnh, dữ liệu thiệp hoặc thông tin khách hàng.

### AC-007
- **Given**: Thiệp có Hình thức Calligraphy và History record có Nội dung thiệp/Lời chúc.
- **When**: Admin mở Chi tiết thiệp.
- **Then**:
  - Ảnh output không bắt buộc hiển thị Nội dung thiệp/Lời chúc.
  - Hệ thống vẫn hiển thị riêng Nội dung thiệp/Lời chúc trong thông tin chi tiết thiệp.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Nguồn | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực | Ghi chú / Link logic |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| [BR-121](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f6e87379-c166-460c-ab30-43cac75150a8) | Dữ liệu chi tiết | AI Custom - Thiệp | Nội dung thiệp/Lời chúc, ảnh đính kèm, trạng thái file và danh sách Order liên kết chỉ hiển thị đầy đủ tại Chi tiết thiệp. | Admin mở Chi tiết thiệp. | Hệ thống hiển thị đầy đủ Nội dung thiệp/Lời chúc, ảnh đính kèm nếu có, trạng thái file và danh sách Order liên kết. Hệ thống không hiển thị Người gửi hoặc Người nhận trong Chi tiết thiệp AI. | Các thông tin này không hiển thị đầy đủ tại danh sách. | STORY-043 | Hoàng Thị Khánh Linh | STORY-043 | Draft | v0 | 2026-09-11 | Rule này cập nhật dữ liệu chi tiết theo luồng thiệp AI mới đã bỏ Người gửi và Người nhận. |
| [BR-122](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d387e4a2-b452-469c-b3b4-61db7d654416) | Giá và bill | AI Custom - Thiệp | Danh sách và Chi tiết thiệp không hiển thị giá. Giá và bill snapshot thuộc từng Order và được xem tại Chi tiết Order. | Admin xem danh sách hoặc Chi tiết thiệp. | Hệ thống không hiển thị giá hoặc bill snapshot của thiệp. | Giá và bill snapshot được xem tại Chi tiết Order tương ứng. | STORY-043 | Hoàng Thị Khánh Linh | STORY-043 | Draft | v0 | 2026-08-15 | — |
| [BR-123](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/058623b9-bafc-420d-b0fe-5b7f8776a1f8) | File không khả dụng | AI Custom - Thiệp | Mất hoặc hỏng file không xóa History record, không đổi lần generate trước thành thất bại và không kích hoạt AI. | File ảnh thiệp không còn khả dụng. | Hệ thống giữ History record và không đổi lần generate trước thành thất bại. | Không kích hoạt AI để tạo lại ảnh. | STORY-043 | Hoàng Thị Khánh Linh | STORY-043 | Draft | v0 | 2026-08-15 | — |
| [BR-125](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1dade9bd-16ac-4c40-86be-56420f2ae609) | Quyền truy cập | AI Custom - Thiệp | Backend chỉ cung cấp danh sách, chi tiết, ảnh và dữ liệu khách hàng cho Admin có quyền quản lý AI Custom. | Backend nhận request danh sách, chi tiết, ảnh hoặc dữ liệu khách hàng. | Backend kiểm tra quyền quản lý AI Custom trước khi cung cấp dữ liệu. | Không cung cấp dữ liệu cho người dùng không có quyền quản lý AI Custom. | STORY-043 | Hoàng Thị Khánh Linh | STORY-043 | Draft | v0 | 2026-08-15 | — |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-121 | [BR-121](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f6e87379-c166-460c-ab30-43cac75150a8) |
| BR-122 | [BR-122](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d387e4a2-b452-469c-b3b4-61db7d654416) |
| BR-123 | [BR-123](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/058623b9-bafc-420d-b0fe-5b7f8776a1f8) |
| BR-125 | [BR-125](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1dade9bd-16ac-4c40-86be-56420f2ae609) |

### Dependencies

- [STORY-044](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)

---

## Non-Functional Requirements

- Chi tiết thiệp p95 ≤ 2 giây trong điều kiện bình thường, không tính thời gian tải ảnh.
- API metadata không nhúng binary/Base64 và không expose đường dẫn storage nội bộ.
- Backend kiểm tra phân quyền trong mọi request, không chỉ dựa trên sidebar hoặc UI.
- UI có loading, empty và error state.

---

## Out of Scope

- Tạo, tạo lại, chỉnh sửa hoặc xóa thiệp.
- Thay đổi dữ liệu khách hàng.
- Thay đổi Order hoặc bill.
- Export Excel.
- Tải xuống thiệp ngoài phạm vi STORY-046.

---
