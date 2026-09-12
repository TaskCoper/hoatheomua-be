# STORY-080 — Admin thêm mới cấu hình Size Hoa Custom AI

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý dữ liệu AI Custom, tôi muốn thêm mới cấu hình Size Hoa Custom AI, để tạo Size và số lượng Combo tương ứng cho luồng tạo mẫu hoa. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Võ Gia Huy |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Admin thêm mới cấu hình Size bằng cách nhập Size và số lượng Combo. Size là trường text ngắn, ví dụ S, M, L, XL. Số lượng Combo là số nguyên dương cho biết số Combo tối đa hoặc số Combo cần dùng trong cấu hình của Size đó.
---

## Conditions

### Preconditions

- Người dùng đã đăng nhập Website quản trị.
- Người dùng có quyền quản lý dữ liệu AI Custom.
- Admin đang ở màn hình quản lý cấu hình Size Hoa Custom AI.

### Trigger

> - Admin chọn thao tác "Thêm mới".


---

## Flow

### Main Flow

1. Admin chọn "Thêm mới".
2. Hệ thống mở form thêm mới cấu hình Size.
3. Admin nhập Size.
4. Admin nhập số lượng Combo.
5. Admin chọn lưu.
6. Backend kiểm tra quyền thao tác.
7. Backend kiểm tra Size không rỗng và không vượt quá giới hạn độ dài.
8. Backend kiểm tra số lượng Combo là số nguyên dương.
9. Backend kiểm tra chưa tồn tại cấu hình Size chưa bị xóa có cùng Size.
10. Hệ thống tạo bản ghi cấu hình Size.
11. Hệ thống gán trạng thái mặc định là Inactive.
12. Hệ thống thông báo tạo mới thành công.
13. Hệ thống hiển thị bản ghi mới trong danh sách.

### Alternative Flows

#### ALT-01 — Admin hủy thao tác thêm mới
- Admin chọn hủy hoặc đóng form.
- Hệ thống đóng form thêm mới.
- Hệ thống không tạo bản ghi cấu hình Size.

### Exception Flows

#### EXC-01 — Thiếu thông tin bắt buộc
- Admin để trống Size hoặc số lượng Combo.
- Hệ thống hiển thị lỗi tại trường chưa hợp lệ.
- Hệ thống không cho lưu bản ghi.

#### EXC-02 — Số lượng Combo không hợp lệ
- Admin nhập số lượng Combo không phải số nguyên dương.
- Hệ thống hiển thị lỗi tại trường số lượng Combo.
- Hệ thống không cho lưu bản ghi.

#### EXC-03 — Size bị trùng
- Admin nhập Size đã tồn tại trong danh sách chưa bị xóa.
- Backend từ chối tạo mới.
- Hệ thống hiển thị thông báo Size đã tồn tại.

#### EXC-04 — Không có quyền thêm mới
- Backend từ chối request.
- Hệ thống không tạo bản ghi cấu hình Size.

---

## Acceptance Criteria

### AC-001
- **Given**: Admin có quyền quản lý dữ liệu AI Custom.
- **When**: Admin chọn "Thêm mới".
- **Then**: Hệ thống mở form thêm mới cấu hình Size.

### AC-002
- **Given**: Admin nhập Size hợp lệ và số lượng Combo là số nguyên dương.
- **When**: Admin chọn lưu.
- **Then**:
  - Hệ thống tạo bản ghi cấu hình Size.
  - Trạng thái mặc định của bản ghi là Inactive.

### AC-003
- **Given**: Admin để trống Size hoặc số lượng Combo.
- **When**: Admin chọn lưu.
- **Then**:
  - Hệ thống hiển thị lỗi tại trường bắt buộc.
  - Không tạo bản ghi mới.

### AC-004
- **Given**: Admin nhập số lượng Combo nhỏ hơn 1 hoặc không phải số nguyên.
- **When**: Admin chọn lưu.
- **Then**:
  - Hệ thống không cho lưu.
  - Hệ thống hiển thị lỗi số lượng Combo không hợp lệ.

### AC-005
- **Given**: Đã tồn tại cấu hình Size chưa bị xóa có cùng Size.
- **When**: Admin tạo mới cấu hình Size với Size đó.
- **Then**:
  - Backend từ chối tạo mới.
  - Hệ thống hiển thị thông báo Size đã tồn tại.

---

## References

### Rules

- BR-273

### Dependencies

- [STORY-079](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c415b501-63cb-4689-ab77-19c3e6ea3e8e)

---

## Non-Functional Requirements

- Backend kiểm tra phân quyền và tính hợp lệ của dữ liệu trong mọi request tạo mới.
- UI hiển thị trạng thái đang lưu khi gửi request.
- Dữ liệu Size nên được chuẩn hóa khoảng trắng trước khi kiểm tra trùng.

---

## Out of Scope

- Tự động chuyển Active sau khi tạo mới.
- Cấu hình giá bán theo Size.
- Cấu hình danh sách Combo cụ thể cho từng Size.
