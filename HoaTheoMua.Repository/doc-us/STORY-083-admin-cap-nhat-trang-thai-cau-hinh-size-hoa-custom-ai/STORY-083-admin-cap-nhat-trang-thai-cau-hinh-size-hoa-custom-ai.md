# STORY-083 — Admin cập nhật trạng thái Active/Inactive của cấu hình Size Hoa Custom AI

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý dữ liệu AI Custom, tôi muốn cập nhật trạng thái Active/Inactive của cấu hình Size Hoa Custom AI, để kiểm soát Size nào được hiển thị cho khách hàng trong luồng tạo mẫu hoa. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Cấu hình Size có hai trạng thái Active và Inactive. Cấu hình Size mới tạo mặc định là Inactive. Admin có thể chuyển sang Active sau khi kiểm tra dữ liệu. Khi chuyển về Inactive, Size không còn được hiển thị cho khách hàng ở các luồng khởi tạo mẫu hoa mới.
---

## Conditions

### Preconditions

- Người dùng đã đăng nhập Website quản trị.
- Người dùng có quyền quản lý dữ liệu AI Custom.
- Cấu hình Size cần đổi trạng thái đang tồn tại và chưa bị xóa.

### Trigger

> - Admin chọn thao tác đổi trạng thái Active/Inactive tại một cấu hình Size.


---

## Flow

### Main Flow

1. Admin chọn thao tác đổi trạng thái tại một cấu hình Size.
2. Backend kiểm tra quyền thao tác.
3. Backend kiểm tra bản ghi cấu hình Size tồn tại.
4. Backend kiểm tra trạng thái hiện tại của bản ghi.
5. Backend cập nhật trạng thái sang Active hoặc Inactive theo thao tác của Admin.
6. Hệ thống thông báo cập nhật trạng thái thành công.
7. Hệ thống hiển thị trạng thái mới trên danh sách.
8. Nếu trạng thái mới là Active, cấu hình Size đủ điều kiện hiển thị cho khách hàng trong luồng chọn Size.
9. Nếu trạng thái mới là Inactive, cấu hình Size không được hiển thị cho khách hàng trong luồng chọn Size mới.

### Alternative Flows

#### ALT-01 — Admin hủy thao tác đổi trạng thái nếu UI có bước xác nhận
- Admin chọn hủy.
- Hệ thống không gửi request đổi trạng thái.
- Trạng thái cấu hình Size giữ nguyên.

### Exception Flows

#### EXC-01 — Không tìm thấy bản ghi
- Backend không tìm thấy cấu hình Size cần đổi trạng thái.
- Hệ thống hiển thị thông báo dữ liệu không tồn tại hoặc đã bị xóa.
- Hệ thống tải lại danh sách.

#### EXC-02 — Không có quyền đổi trạng thái
- Backend từ chối request.
- Hệ thống không thay đổi trạng thái cấu hình Size.

#### EXC-03 — Cập nhật trạng thái thất bại
- Backend không thể hoàn tất cập nhật trạng thái.
- Hệ thống hiển thị thông báo cập nhật thất bại.
- UI giữ hoặc khôi phục trạng thái trước đó.

---

## Acceptance Criteria

### AC-001
- **Given**: Admin có quyền và cấu hình Size tồn tại.
- **When**: Admin chuyển trạng thái từ Inactive sang Active.
- **Then**:
  - Hệ thống cập nhật trạng thái cấu hình Size thành Active.
  - Cấu hình Size đủ điều kiện hiển thị cho khách hàng trong luồng chọn Size mới.

### AC-002
- **Given**: Admin có quyền và cấu hình Size tồn tại.
- **When**: Admin chuyển trạng thái từ Active sang Inactive.
- **Then**:
  - Hệ thống cập nhật trạng thái cấu hình Size thành Inactive.
  - Cấu hình Size không được hiển thị cho khách hàng trong luồng chọn Size mới.

### AC-003
- **Given**: Cấu hình Size mới được tạo từ STORY-080.
- **When**: Hệ thống hiển thị bản ghi trong danh sách.
- **Then**: Trạng thái mặc định của bản ghi là Inactive.

### AC-004
- **Given**: Người dùng không có quyền đổi trạng thái cấu hình Size.
- **When**: Người dùng gửi request đổi trạng thái.
- **Then**:
  - Backend từ chối request.
  - Trạng thái dữ liệu không bị thay đổi.

### AC-005
- **Given**: Cấu hình Size không tồn tại hoặc đã bị xóa.
- **When**: Admin gửi request đổi trạng thái.
- **Then**:
  - Backend không cập nhật trạng thái.
  - Hệ thống hiển thị thông báo phù hợp.

---

## References

### Rules

- BR-273

### Dependencies

- [STORY-079](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c415b501-63cb-4689-ab77-19c3e6ea3e8e)
- [STORY-080](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/201f19e4-4ab9-408d-9aa5-e6ecc74c9199)

---

## Non-Functional Requirements

- Backend kiểm tra quyền trong mọi request đổi trạng thái.
- UI phản ánh đúng trạng thái cuối cùng từ backend.
- Danh sách khách hàng chỉ lấy cấu hình Size Active trong các luồng chọn Size mới.

---

## Out of Scope

- Tạo mới, cập nhật hoặc xóa cấu hình Size.
- Cập nhật dữ liệu đã snapshot trong yêu cầu tạo mẫu hoa cũ.
- Tự động thay đổi trạng thái của các Size khác.
