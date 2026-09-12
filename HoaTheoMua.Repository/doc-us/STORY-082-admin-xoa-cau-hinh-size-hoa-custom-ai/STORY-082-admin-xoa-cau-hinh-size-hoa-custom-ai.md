# STORY-082 — Admin xóa cấu hình Size Hoa Custom AI không còn sử dụng

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý dữ liệu AI Custom, tôi muốn xóa cấu hình Size Hoa Custom AI không còn sử dụng, để danh sách cấu hình chỉ giữ các Size còn cần quản lý. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Võ Gia Huy |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Admin có thể xóa cấu hình Size từ danh sách quản trị. Trước khi xóa, hệ thống bắt buộc hiển thị hộp thoại xác nhận. Chỉ khi Admin xác nhận, hệ thống mới thực hiện xóa theo cơ chế lưu trữ hiện hành.
---

## Conditions

### Preconditions

- Người dùng đã đăng nhập Website quản trị.
- Người dùng có quyền quản lý dữ liệu AI Custom.
- Cấu hình Size cần xóa đang tồn tại và chưa bị xóa.

### Trigger

> - Admin chọn thao tác "Xóa" tại một cấu hình Size.


---

## Flow

### Main Flow

1. Admin chọn thao tác "Xóa" tại một cấu hình Size.
2. Hệ thống hiển thị hộp thoại xác nhận xóa.
3. Admin xác nhận xóa.
4. Backend kiểm tra quyền thao tác.
5. Backend kiểm tra bản ghi cấu hình Size tồn tại.
6. Backend xóa cấu hình Size theo cơ chế lưu trữ hiện hành.
7. Hệ thống thông báo xóa thành công.
8. Hệ thống cập nhật lại danh sách cấu hình Size.

### Alternative Flows

#### ALT-01 — Admin hủy xác nhận xóa
- Admin chọn hủy hoặc đóng hộp thoại xác nhận.
- Hệ thống đóng hộp thoại xác nhận.
- Hệ thống không xóa cấu hình Size.
- Danh sách giữ nguyên dữ liệu trước đó.

### Exception Flows

#### EXC-01 — Không tìm thấy bản ghi cần xóa
- Backend không tìm thấy cấu hình Size.
- Hệ thống hiển thị thông báo dữ liệu không tồn tại hoặc đã bị xóa.
- Hệ thống tải lại danh sách.

#### EXC-02 — Không có quyền xóa
- Backend từ chối request.
- Hệ thống không xóa cấu hình Size.

#### EXC-03 — Xóa thất bại
- Backend không thể hoàn tất thao tác xóa.
- Hệ thống hiển thị thông báo xóa thất bại.
- UI không hiển thị bản ghi như đã xóa nếu backend chưa xác nhận thành công.

---

## Acceptance Criteria

### AC-001
- **Given**: Admin có quyền và cấu hình Size tồn tại.
- **When**: Admin chọn "Xóa".
- **Then**: Hệ thống hiển thị hộp thoại xác nhận xóa.

### AC-002
- **Given**: Hộp thoại xác nhận xóa đang hiển thị.
- **When**: Admin xác nhận xóa.
- **Then**:
  - Backend thực hiện xóa cấu hình Size.
  - Hệ thống cập nhật lại danh sách sau khi xóa thành công.

### AC-003
- **Given**: Hộp thoại xác nhận xóa đang hiển thị.
- **When**: Admin hủy thao tác.
- **Then**:
  - Hệ thống không xóa cấu hình Size.
  - Danh sách giữ nguyên dữ liệu trước đó.

### AC-004
- **Given**: Người dùng không có quyền xóa cấu hình Size.
- **When**: Người dùng gửi request xóa.
- **Then**:
  - Backend từ chối request.
  - Dữ liệu không bị xóa.

### AC-005
- **Given**: Cấu hình Size không tồn tại hoặc đã bị xóa.
- **When**: Admin gửi request xóa.
- **Then**:
  - Backend không thực hiện xóa lại.
  - Hệ thống hiển thị thông báo phù hợp.

---

## References

### Rules

- BR-273

### Dependencies

- [STORY-079](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c415b501-63cb-4689-ab77-19c3e6ea3e8e)

---

## Non-Functional Requirements

- Xóa phải có xác nhận rõ ràng trước khi gửi request.
- Backend kiểm tra quyền trong mọi request xóa.
- UI không cập nhật trạng thái đã xóa nếu backend trả lỗi.

---

## Out of Scope

- Khôi phục cấu hình Size đã xóa.
- Xóa hàng loạt.
- Xóa dữ liệu yêu cầu tạo mẫu hoa đã snapshot Size trước đó.
