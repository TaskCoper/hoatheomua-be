# STORY-077 — Admin xóa hình ảnh kiểu gói không còn sử dụng

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý dữ liệu AI Custom, tôi muốn xóa hình ảnh kiểu gói không còn sử dụng, để danh sách cấu hình chỉ giữ các dữ liệu còn cần quản lý. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Võ Gia Huy |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Admin có thể xóa hình ảnh kiểu gói từ danh sách quản trị.
---

## Conditions

### Preconditions

- Người dùng đã đăng nhập Website quản trị.
- Người dùng có quyền quản lý dữ liệu AI Custom.
- Hình ảnh kiểu gói cần xóa đang tồn tại.

### Trigger

> - Admin chọn thao tác "Xóa" tại một hình ảnh kiểu gói.


---

## Flow

### Main Flow

1. Admin chọn thao tác "Xóa" tại một hình ảnh kiểu gói.
2. Hệ thống hiển thị hộp thoại xác nhận xóa.
3. Admin xác nhận xóa.
4. Backend kiểm tra quyền thao tác.
5. Backend kiểm tra bản ghi hình ảnh kiểu gói tồn tại.
6. Backend xóa hình ảnh kiểu gói theo cơ chế lưu trữ hiện hành.
7. Hệ thống thông báo xóa thành công.
8. Hệ thống cập nhật lại danh sách hình ảnh kiểu gói.

### Alternative Flows

#### ALT-01 — Admin hủy xác nhận xóa
- Admin chọn hủy hoặc đóng hộp thoại xác nhận.
- Hệ thống đóng hộp thoại xác nhận.
- Hệ thống không xóa hình ảnh kiểu gói.
- Danh sách giữ nguyên dữ liệu trước đó.

### Exception Flows

#### EXC-01 — Không tìm thấy bản ghi cần xóa
- Backend không tìm thấy hình ảnh kiểu gói.
- Hệ thống hiển thị thông báo dữ liệu không tồn tại hoặc đã bị xóa.
- Hệ thống tải lại danh sách.

#### EXC-02 — Không có quyền xóa
- Backend từ chối request.
- Hệ thống không xóa hình ảnh kiểu gói.

#### EXC-03 — Xóa thất bại
- Backend không thể hoàn tất thao tác xóa.
- Hệ thống hiển thị thông báo xóa thất bại.
- Dữ liệu hình ảnh kiểu gói không bị hiển thị như đã xóa nếu backend chưa xác nhận thành công.

---

## Acceptance Criteria

### AC-001
- **Given**: Admin có quyền và hình ảnh kiểu gói tồn tại.
- **When**: Admin chọn "Xóa".
- **Then**: Hệ thống hiển thị hộp thoại xác nhận xóa.

### AC-002
- **Given**: Hộp thoại xác nhận xóa đang hiển thị.
- **When**: Admin xác nhận xóa.
- **Then**:
  - Backend thực hiện xóa hình ảnh kiểu gói.
  - Hệ thống cập nhật lại danh sách sau khi xóa thành công.

### AC-003
- **Given**: Hộp thoại xác nhận xóa đang hiển thị.
- **When**: Admin hủy thao tác.
- **Then**:
  - Hệ thống không xóa hình ảnh kiểu gói.
  - Danh sách giữ nguyên dữ liệu trước đó.

### AC-004
- **Given**: Người dùng không có quyền xóa hình ảnh kiểu gói.
- **When**: Người dùng gửi request xóa.
- **Then**:
  - Backend từ chối request.
  - Dữ liệu không bị xóa.

### AC-005
- **Given**: Hình ảnh kiểu gói không tồn tại hoặc đã bị xóa.
- **When**: Admin gửi request xóa.
- **Then**:
  - Backend không thực hiện xóa lại.
  - Hệ thống hiển thị thông báo phù hợp.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Nguồn | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực | Ghi chú / Link logic |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| [BR-276](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6452f701-1e2b-4cd4-a80e-3c015aebe7bf) | Xóa hình ảnh kiểu gói | Quản lý dữ liệu AI Custom | Hình ảnh kiểu gói chỉ được xóa khi Admin có quyền quản lý dữ liệu AI Custom, bản ghi còn tồn tại và Admin đã xác nhận thao tác xóa. | Admin thực hiện thao tác xóa hình ảnh kiểu gói trong Website quản trị. | Hệ thống phải hiển thị bước xác nhận trước khi gửi request xóa. Sau khi Admin xác nhận, backend phải kiểm tra quyền thao tác và trạng thái tồn tại của bản ghi trước khi xóa theo cơ chế lưu trữ hiện hành. Sau khi xóa thành công, hệ thống phải cập nhật lại danh sách hình ảnh kiểu gói. | Không cho phép xóa nếu Admin chưa xác nhận, người dùng không có quyền, bản ghi không tồn tại hoặc bản ghi đã bị xóa trước đó. Nếu backend trả lỗi, UI không được hiển thị bản ghi như đã xóa thành công. | Product discussion 2026-09-12; STORY-077 | Đức Bình | STORY-077 | Draft | v0 | 2026-09-12 | Việc xóa hình ảnh kiểu gói không làm thay đổi dữ liệu đã được snapshot trong các yêu cầu tạo mẫu hoa cũ. |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-276 | [BR-276](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6452f701-1e2b-4cd4-a80e-3c015aebe7bf) |

### Dependencies

- [STORY-074](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/322aae9b-a60d-43c2-ac07-2678379e48b8)

---

## Non-Functional Requirements

- Xóa phải có xác nhận rõ ràng trước khi gửi request.
- Backend kiểm tra quyền trong mọi request xóa.
- UI không cập nhật trạng thái đã xóa nếu backend trả lỗi.

---

## Out of Scope

- Khôi phục hình ảnh kiểu gói đã xóa.
- Xóa hàng loạt.
- Xóa dữ liệu yêu cầu tạo mẫu hoa đã tham chiếu hình ảnh kiểu gói trước đó.

---
