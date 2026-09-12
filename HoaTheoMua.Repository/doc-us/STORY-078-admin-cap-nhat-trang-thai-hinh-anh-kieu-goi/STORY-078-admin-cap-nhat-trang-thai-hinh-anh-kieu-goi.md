# STORY-078 — Admin cập nhật trạng thái Active/Inactive của hình ảnh kiểu gói

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý dữ liệu AI Custom, tôi muốn cập nhật trạng thái Active/Inactive của hình ảnh kiểu gói, để kiểm soát dữ liệu nào được hiển thị cho khách hàng trong luồng tạo mẫu hoa. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Võ Gia Huy |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Hình ảnh kiểu gói có hai trạng thái Active và Inactive.
---

## Conditions

### Preconditions

- Người dùng đã đăng nhập Website quản trị.
- Người dùng có quyền quản lý dữ liệu AI Custom.
- Hình ảnh kiểu gói cần đổi trạng thái đang tồn tại.

### Trigger

> - Admin chọn thao tác đổi trạng thái Active/Inactive tại một hình ảnh kiểu gói.


---

## Flow

### Main Flow

1. Admin chọn thao tác đổi trạng thái tại một hình ảnh kiểu gói.
2. Backend kiểm tra quyền thao tác.
3. Backend kiểm tra bản ghi hình ảnh kiểu gói tồn tại.
4. Backend kiểm tra trạng thái hiện tại của bản ghi.
5. Backend cập nhật trạng thái sang Active hoặc Inactive theo thao tác của Admin.
6. Hệ thống thông báo cập nhật trạng thái thành công.
7. Hệ thống hiển thị trạng thái mới trên danh sách.
8. Nếu trạng thái mới là Active, hình ảnh kiểu gói đủ điều kiện hiển thị cho khách hàng trong luồng chọn kiểu gói.
9. Nếu trạng thái mới là Inactive, hình ảnh kiểu gói không được hiển thị cho khách hàng trong luồng chọn kiểu gói mới.

### Alternative Flows

#### ALT-01 — Admin hủy thao tác đổi trạng thái nếu UI có bước xác nhận
- Admin chọn hủy.
- Hệ thống không gửi request đổi trạng thái.
- Trạng thái hình ảnh kiểu gói giữ nguyên.

### Exception Flows

#### EXC-01 — Không tìm thấy bản ghi
- Backend không tìm thấy hình ảnh kiểu gói cần đổi trạng thái.
- Hệ thống hiển thị thông báo dữ liệu không tồn tại hoặc đã bị xóa.
- Hệ thống tải lại danh sách.

#### EXC-02 — Không có quyền đổi trạng thái
- Backend từ chối request.
- Hệ thống không thay đổi trạng thái hình ảnh kiểu gói.

#### EXC-03 — Cập nhật trạng thái thất bại
- Backend không thể hoàn tất cập nhật trạng thái.
- Hệ thống hiển thị thông báo cập nhật thất bại.
- UI giữ hoặc khôi phục trạng thái trước đó.

---

## Acceptance Criteria

### AC-001
- **Given**: Admin có quyền và hình ảnh kiểu gói tồn tại.
- **When**: Admin chuyển trạng thái từ Inactive sang Active.
- **Then**:
  - Hệ thống cập nhật trạng thái hình ảnh kiểu gói thành Active.
  - Hình ảnh kiểu gói đủ điều kiện hiển thị cho khách hàng trong luồng chọn kiểu gói mới.

### AC-002
- **Given**: Admin có quyền và hình ảnh kiểu gói tồn tại.
- **When**: Admin chuyển trạng thái từ Active sang Inactive.
- **Then**:
  - Hệ thống cập nhật trạng thái hình ảnh kiểu gói thành Inactive.
  - Hình ảnh kiểu gói không được hiển thị cho khách hàng trong luồng chọn kiểu gói mới.

### AC-003
- **Given**: Hình ảnh kiểu gói mới được tạo từ STORY-075.
- **When**: Hệ thống hiển thị bản ghi trong danh sách.
- **Then**: Trạng thái mặc định của bản ghi là Inactive.

### AC-004
- **Given**: Người dùng không có quyền đổi trạng thái hình ảnh kiểu gói.
- **When**: Người dùng gửi request đổi trạng thái.
- **Then**:
  - Backend từ chối request.
  - Trạng thái dữ liệu không bị thay đổi.

### AC-005
- **Given**: Hình ảnh kiểu gói không tồn tại hoặc đã bị xóa.
- **When**: Admin gửi request đổi trạng thái.
- **Then**:
  - Backend không cập nhật trạng thái.
  - Hệ thống hiển thị thông báo phù hợp.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Nguồn | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực | Ghi chú / Link logic |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| [BR-274](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bb4c5ee0-0ef8-4e72-be5b-306f7fdc4317) | Thêm mới hình ảnh kiểu gói | Quản lý dữ liệu AI Custom | Hình ảnh kiểu gói mới chỉ được tạo khi Admin có quyền quản lý dữ liệu AI Custom và cung cấp đầy đủ dữ liệu hợp lệ. | Admin thực hiện thao tác thêm mới hình ảnh kiểu gói trong Website quản trị. | Backend phải kiểm tra các điều kiện sau trước khi tạo bản ghi: - Người dùng có quyền quản lý dữ liệu AI Custom. - Ảnh preview là bắt buộc. - Ảnh preview chỉ chấp nhận định dạng JPG hoặc PNG. - Dung lượng ảnh preview không vượt quá 10MB. - Tên hình ảnh kiểu gói là bắt buộc. - Tên và mô tả phải đáp ứng giới hạn dữ liệu do hệ thống cấu hình. Khi tạo mới thành công, hệ thống phải gán trạng thái mặc định của hình ảnh kiểu gói là Inactive. | Không cho phép tạo mới hình ảnh kiểu gói nếu người dùng không có quyền, thiếu ảnh preview, thiếu tên, ảnh preview sai định dạng hoặc ảnh preview vượt quá dung lượng cho phép. | Product discussion 2026-09-12; STORY-075 | Đức Bình | STORY-075; STORY-078 | Draft | v0 | 2026-09-12 | Trạng thái Inactive mặc định giúp cửa hàng kiểm tra dữ liệu trước khi cho khách hàng sử dụng trong luồng tạo mẫu hoa AI. Việc chuyển sang Active được xử lý theo story đổi trạng thái riêng. |
| [BR-277](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1b3e3896-e7e4-46ce-8549-4ea310736f72) | Đổi trạng thái Active/Inactive của hình ảnh kiểu gói | Quản lý dữ liệu AI Custom | Admin có quyền quản lý dữ liệu AI Custom được phép đổi trạng thái hình ảnh kiểu gói giữa Active và Inactive để kiểm soát dữ liệu hiển thị cho khách hàng. | Admin thực hiện thao tác đổi trạng thái Active/Inactive của một hình ảnh kiểu gói trong Website quản trị. | Backend phải kiểm tra người dùng có quyền thao tác và hình ảnh kiểu gói còn tồn tại trước khi cập nhật trạng thái. Nếu trạng thái mới là Active, hình ảnh kiểu gói đủ điều kiện hiển thị cho khách hàng trong luồng chọn kiểu gói mới. Nếu trạng thái mới là Inactive, hình ảnh kiểu gói không được hiển thị cho khách hàng trong luồng chọn kiểu gói mới. | Không cho phép đổi trạng thái nếu người dùng không có quyền, bản ghi không tồn tại hoặc đã bị xóa. Nếu cập nhật trạng thái thất bại, UI phải giữ hoặc khôi phục trạng thái trước đó. | Product discussion 2026-09-12; STORY-078 | Đức Bình | STORY-078 | Draft | v0 | 2026-09-12 | Hình ảnh kiểu gói mới tạo từ STORY-075 có trạng thái mặc định là Inactive theo BR-274. |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-274 | [BR-274](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bb4c5ee0-0ef8-4e72-be5b-306f7fdc4317) |
| BR-277 | [BR-277](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1b3e3896-e7e4-46ce-8549-4ea310736f72) |

### Dependencies

- [STORY-074](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/322aae9b-a60d-43c2-ac07-2678379e48b8)
- [STORY-075](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aff65069-4327-4a9f-a72e-511b99dfb73a)

---

## Non-Functional Requirements

- Backend kiểm tra quyền trong mọi request đổi trạng thái.
- UI phản ánh đúng trạng thái cuối cùng từ backend.
- Danh sách khách hàng chỉ lấy dữ liệu Active trong các luồng chọn kiểu gói mới.

---

## Out of Scope

- Tạo mới, cập nhật hoặc xóa hình ảnh kiểu gói.
- Cấu hình thứ tự hiển thị cho khách hàng.
- Thay đổi dữ liệu đã snapshot trong yêu cầu tạo mẫu hoa cũ.

---
