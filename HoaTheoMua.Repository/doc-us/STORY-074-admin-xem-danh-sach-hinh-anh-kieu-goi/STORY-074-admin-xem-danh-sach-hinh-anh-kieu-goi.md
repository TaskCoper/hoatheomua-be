# STORY-074 — Admin xem danh sách hình ảnh kiểu gói

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý dữ liệu AI Custom, tôi muốn xem danh sách hình ảnh kiểu gói, để kiểm tra và quản lý các kiểu gói có thể dùng trong luồng tạo mẫu hoa. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Võ Gia Huy |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Hình ảnh kiểu gói là dữ liệu cấu hình do cửa hàng quản lý để mô tả cách bó/gói hoa trong luồng tạo mẫu hoa AI.
---

## Conditions

### Preconditions

- Người dùng đã đăng nhập Website quản trị.
- Người dùng có quyền quản lý dữ liệu AI Custom.

### Trigger

> - Admin mở màn hình quản lý hình ảnh kiểu gói.


---

## Flow

### Main Flow

1. Admin mở menu quản lý hình ảnh kiểu gói.
2. Backend kiểm tra quyền truy cập.
3. Hệ thống lấy danh sách hình ảnh kiểu gói.
4. Hệ thống hiển thị danh sách theo dạng bảng hoặc lưới.
5. Mỗi bản ghi hiển thị ảnh preview, tên, mô tả, trạng thái và thao tác.
6. Hệ thống hiển thị trạng thái Active hoặc Inactive của từng bản ghi.
7. Admin có thể lọc danh sách theo trạng thái Tất cả, Active hoặc Inactive.
8. Hệ thống cập nhật danh sách theo trạng thái đã chọn.
9. Admin có thể chọn thao tác thêm mới, cập nhật, xóa hoặc đổi trạng thái.

### Alternative Flows

#### ALT-01 — Không có dữ liệu hình ảnh kiểu gói
- Hệ thống hiển thị empty state.
- Hệ thống vẫn hiển thị thao tác thêm mới nếu Admin có quyền.

#### ALT-02 — Ảnh preview không tải được
- Hệ thống hiển thị placeholder ảnh không khả dụng.
- Hệ thống vẫn hiển thị tên, mô tả, trạng thái và thao tác của bản ghi.

#### ALT-03 — Lọc danh sách theo trạng thái
- Admin chọn bộ lọc trạng thái Tất cả, Active hoặc Inactive.
- Hệ thống lấy và hiển thị danh sách hình ảnh kiểu gói theo trạng thái đã chọn.
- Nếu không có dữ liệu phù hợp với bộ lọc, hệ thống hiển thị empty state theo bộ lọc hiện tại.

### Exception Flows

#### EXC-01 — Không có quyền truy cập
- Backend từ chối request.
- Hệ thống không hiển thị danh sách hình ảnh kiểu gói.

#### EXC-02 — Không tải được danh sách
- Hệ thống hiển thị error state.
- Hệ thống cho phép Admin tải lại danh sách.

---

## Acceptance Criteria

### AC-001
- **Given**: Admin có quyền quản lý dữ liệu AI Custom.
- **When**: Admin mở màn hình quản lý hình ảnh kiểu gói.
- **Then**:
  - Hệ thống hiển thị danh sách hình ảnh kiểu gói.
  - Mỗi bản ghi có ảnh preview, tên, mô tả, trạng thái và thao tác.

### AC-002
- **Given**: Hình ảnh kiểu gói có trạng thái Active.
- **When**: Danh sách được hiển thị.
- **Then**: Hệ thống hiển thị trạng thái Active của bản ghi.

### AC-003
- **Given**: Hình ảnh kiểu gói có trạng thái Inactive.
- **When**: Danh sách được hiển thị.
- **Then**: Hệ thống hiển thị trạng thái Inactive của bản ghi.

### AC-004
- **Given**: Chưa có hình ảnh kiểu gói nào.
- **When**: Admin mở danh sách.
- **Then**:
  - Hệ thống hiển thị empty state.
  - Hệ thống vẫn cho phép thêm mới nếu Admin có quyền.

### AC-005
- **Given**: Người dùng không có quyền quản lý dữ liệu AI Custom.
- **When**: Người dùng mở màn hình quản lý hình ảnh kiểu gói.
- **Then**:
  - Backend từ chối request.
  - Hệ thống không hiển thị dữ liệu quản trị.

### AC-006
- **Given**: Danh sách có hình ảnh kiểu gói ở nhiều trạng thái.
- **When**: Admin chọn bộ lọc trạng thái Active.
- **Then**: Hệ thống chỉ hiển thị các hình ảnh kiểu gói có trạng thái Active.

### AC-007
- **Given**: Danh sách có hình ảnh kiểu gói ở nhiều trạng thái.
- **When**: Admin chọn bộ lọc trạng thái Inactive.
- **Then**: Hệ thống chỉ hiển thị các hình ảnh kiểu gói có trạng thái Inactive.

---

## Non-Functional Requirements

- Danh sách tải trong p95 ≤ 2 giây trong điều kiện bình thường.
- Backend kiểm tra phân quyền trong mọi request.
- UI có loading, empty và error state.

---

## Out of Scope

- Quản lý giấy gói.
- Quản lý ruy băng.
- Khách hàng chọn hình ảnh kiểu gói trong luồng tạo mẫu hoa.
