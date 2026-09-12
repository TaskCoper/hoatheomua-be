# STORY-079 — Admin xem danh sách cấu hình Size cho Hoa Custom AI

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý dữ liệu AI Custom, tôi muốn xem danh sách cấu hình Size cho Hoa Custom AI, để quản lý các Size và số lượng Combo được phép dùng trong luồng tạo mẫu hoa. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Cấu hình Size là dữ liệu do cửa hàng quản lý, gồm tên Size và số lượng Combo tương ứng. Ví dụ Size có thể là S, M, L, XL hoặc các tên Size khác theo nhu cầu cửa hàng.
---

## Conditions

### Preconditions

- Người dùng đã đăng nhập Website quản trị.
- Người dùng có quyền quản lý dữ liệu AI Custom.

### Trigger

> - Admin mở màn hình quản lý cấu hình Size Hoa Custom AI.


---

## Flow

### Main Flow

1. Admin mở menu quản lý cấu hình Size Hoa Custom AI.
2. Backend kiểm tra quyền truy cập.
3. Hệ thống lấy danh sách cấu hình Size chưa bị xóa theo cơ chế lưu trữ hiện hành.
4. Hệ thống hiển thị danh sách theo dạng bảng.
5. Mỗi bản ghi hiển thị Size, số lượng Combo, trạng thái Active/Inactive và thao tác.
6. Admin có thể tìm kiếm theo Size.
7. Admin có thể lọc danh sách theo trạng thái Tất cả, Active hoặc Inactive.
8. Admin có thể chọn thao tác thêm mới, cập nhật, xóa hoặc đổi trạng thái.

### Alternative Flows

#### ALT-01 — Không có cấu hình Size
- Hệ thống hiển thị empty state.
- Hệ thống vẫn hiển thị thao tác thêm mới nếu Admin có quyền.

#### ALT-02 — Lọc danh sách theo trạng thái không có kết quả
- Admin chọn bộ lọc trạng thái.
- Hệ thống không tìm thấy bản ghi phù hợp.
- Hệ thống hiển thị empty state theo bộ lọc hiện tại.

### Exception Flows

#### EXC-01 — Không có quyền truy cập
- Backend từ chối request.
- Hệ thống không hiển thị dữ liệu quản trị.

#### EXC-02 — Không tải được danh sách
- Hệ thống hiển thị error state.
- Hệ thống cho phép Admin tải lại danh sách.

---

## Acceptance Criteria

### AC-001
- **Given**: Admin có quyền quản lý dữ liệu AI Custom.
- **When**: Admin mở màn hình quản lý cấu hình Size Hoa Custom AI.
- **Then**:
  - Hệ thống hiển thị danh sách cấu hình Size.
  - Mỗi bản ghi hiển thị Size, số lượng Combo, trạng thái và thao tác.

### AC-002
- **Given**: Danh sách có cấu hình Size ở nhiều trạng thái.
- **When**: Admin chọn bộ lọc Active.
- **Then**: Hệ thống chỉ hiển thị các cấu hình Size có trạng thái Active.

### AC-003
- **Given**: Danh sách có cấu hình Size ở nhiều trạng thái.
- **When**: Admin chọn bộ lọc Inactive.
- **Then**: Hệ thống chỉ hiển thị các cấu hình Size có trạng thái Inactive.

### AC-004
- **Given**: Chưa có cấu hình Size nào.
- **When**: Admin mở danh sách.
- **Then**:
  - Hệ thống hiển thị empty state.
  - Hệ thống vẫn cho phép thêm mới nếu Admin có quyền.

### AC-005
- **Given**: Người dùng không có quyền quản lý dữ liệu AI Custom.
- **When**: Người dùng mở màn hình quản lý cấu hình Size Hoa Custom AI.
- **Then**:
  - Backend từ chối request.
  - Hệ thống không hiển thị dữ liệu quản trị.

---

## References

### Rules

- BR-273

### Dependencies

- [STORY-030](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)

---

## Non-Functional Requirements

- Danh sách tải trong p95 <= 2 giây trong điều kiện bình thường.
- Backend kiểm tra phân quyền trong mọi request.
- UI có loading, empty và error state.

---

## Out of Scope

- Khách hàng chọn Size trong luồng khởi tạo mẫu hoa.
- Tự động tính giá theo Size.
- Cấu hình danh sách Combo cụ thể được phép kết hợp cho từng Size.
