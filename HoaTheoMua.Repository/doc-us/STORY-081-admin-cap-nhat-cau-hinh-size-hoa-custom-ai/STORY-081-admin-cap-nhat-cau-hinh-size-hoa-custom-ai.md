# STORY-081 — Admin cập nhật cấu hình Size Hoa Custom AI

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý dữ liệu AI Custom, tôi muốn cập nhật cấu hình Size Hoa Custom AI, để chỉnh sửa Size hoặc số lượng Combo khi cấu hình cửa hàng thay đổi. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Võ Gia Huy |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Admin có thể cập nhật Size và số lượng Combo của một cấu hình Size đã tồn tại. Việc cập nhật thông tin không tự động thay đổi trạng thái Active/Inactive hiện tại của bản ghi.
---

## Conditions

### Preconditions

- Người dùng đã đăng nhập Website quản trị.
- Người dùng có quyền quản lý dữ liệu AI Custom.
- Cấu hình Size cần cập nhật đang tồn tại và chưa bị xóa.

### Trigger

> - Admin chọn thao tác "Cập nhật" hoặc "Sửa" tại một cấu hình Size.


---

## Flow

### Main Flow

1. Admin chọn thao tác cập nhật tại một cấu hình Size.
2. Backend kiểm tra quyền truy cập và bản ghi cần cập nhật.
3. Hệ thống mở form cập nhật với dữ liệu hiện tại.
4. Admin chỉnh sửa Size hoặc số lượng Combo.
5. Admin chọn lưu.
6. Backend kiểm tra dữ liệu cập nhật.
7. Backend kiểm tra Size không rỗng và không trùng với cấu hình Size khác chưa bị xóa.
8. Backend kiểm tra số lượng Combo là số nguyên dương.
9. Backend cập nhật bản ghi cấu hình Size.
10. Hệ thống giữ nguyên trạng thái Active/Inactive hiện tại của bản ghi.
11. Hệ thống thông báo cập nhật thành công.
12. Hệ thống hiển thị dữ liệu mới trong danh sách.

### Alternative Flows

#### ALT-01 — Admin hủy thao tác cập nhật
- Admin chọn hủy hoặc đóng form.
- Hệ thống đóng form cập nhật.
- Hệ thống không thay đổi dữ liệu cấu hình Size.

### Exception Flows

#### EXC-01 — Không tìm thấy bản ghi
- Backend không tìm thấy cấu hình Size cần cập nhật.
- Hệ thống hiển thị thông báo dữ liệu không tồn tại hoặc đã bị xóa.
- Hệ thống không cập nhật dữ liệu.

#### EXC-02 — Dữ liệu cập nhật không hợp lệ
- Admin nhập thiếu Size hoặc số lượng Combo không hợp lệ.
- Hệ thống hiển thị lỗi tại trường chưa hợp lệ.
- Hệ thống không cập nhật dữ liệu.

#### EXC-03 — Size bị trùng với bản ghi khác
- Admin nhập Size đã tồn tại ở cấu hình Size khác chưa bị xóa.
- Backend từ chối cập nhật.
- Hệ thống hiển thị thông báo Size đã tồn tại.

#### EXC-04 — Không có quyền cập nhật
- Backend từ chối request.
- Hệ thống không cập nhật dữ liệu cấu hình Size.

---

## Acceptance Criteria

### AC-001
- **Given**: Admin có quyền và cấu hình Size tồn tại.
- **When**: Admin chọn thao tác cập nhật.
- **Then**: Hệ thống mở form cập nhật với dữ liệu hiện tại.

### AC-002
- **Given**: Admin cập nhật Size hoặc số lượng Combo hợp lệ.
- **When**: Admin chọn lưu.
- **Then**:
  - Hệ thống cập nhật dữ liệu cấu hình Size.
  - Trạng thái Active/Inactive hiện tại không bị thay đổi.

### AC-003
- **Given**: Admin nhập số lượng Combo nhỏ hơn 1 hoặc không phải số nguyên.
- **When**: Admin chọn lưu.
- **Then**:
  - Hệ thống không cập nhật dữ liệu.
  - Hệ thống hiển thị lỗi số lượng Combo không hợp lệ.

### AC-004
- **Given**: Admin nhập Size đã tồn tại ở bản ghi khác chưa bị xóa.
- **When**: Admin chọn lưu.
- **Then**:
  - Backend từ chối cập nhật.
  - Hệ thống hiển thị thông báo Size đã tồn tại.

### AC-005
- **Given**: Cấu hình Size đã bị xóa hoặc không tồn tại.
- **When**: Admin gửi request cập nhật.
- **Then**:
  - Backend từ chối cập nhật.
  - Hệ thống hiển thị thông báo phù hợp.

---

## References

### Rules

- BR-273

### Dependencies

- [STORY-079](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c415b501-63cb-4689-ab77-19c3e6ea3e8e)

---

## Non-Functional Requirements

- Backend kiểm tra quyền và tính hợp lệ của dữ liệu trong mọi request cập nhật.
- UI không làm mất dữ liệu hiện tại nếu cập nhật thất bại.
- Thao tác cập nhật có loading và thông báo kết quả rõ ràng.

---

## Out of Scope

- Đổi trạng thái Active/Inactive.
- Xóa cấu hình Size.
- Cập nhật dữ liệu đã snapshot trong yêu cầu tạo mẫu hoa đã tạo trước đó.
