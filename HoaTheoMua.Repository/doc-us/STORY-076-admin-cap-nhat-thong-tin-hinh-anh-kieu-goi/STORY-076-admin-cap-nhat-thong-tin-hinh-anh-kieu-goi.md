# STORY-076 — Admin cập nhật thông tin hình ảnh kiểu gói

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý dữ liệu AI Custom, tôi muốn cập nhật thông tin hình ảnh kiểu gói, để chỉnh sửa ảnh preview, tên hoặc mô tả khi dữ liệu cấu hình thay đổi. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Võ Gia Huy |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Admin có thể cập nhật ảnh preview, tên và mô tả của một hình ảnh kiểu gói đã tồn tại.
---

## Conditions

### Preconditions

- Người dùng đã đăng nhập Website quản trị.
- Người dùng có quyền quản lý dữ liệu AI Custom.
- Hình ảnh kiểu gói cần cập nhật đang tồn tại.

### Trigger

> - Admin chọn thao tác "Cập nhật" hoặc "Sửa" tại một hình ảnh kiểu gói.


---

## Flow

### Main Flow

1. Admin chọn thao tác cập nhật tại một hình ảnh kiểu gói.
2. Backend kiểm tra quyền truy cập và bản ghi cần cập nhật.
3. Hệ thống mở form cập nhật với dữ liệu hiện tại.
4. Admin chỉnh sửa ảnh preview, tên hoặc mô tả.
5. Admin chọn lưu.
6. Backend kiểm tra dữ liệu cập nhật.
7. Nếu ảnh preview được thay đổi, backend kiểm tra ảnh có định dạng JPG hoặc PNG và dung lượng không vượt quá 10MB.
8. Backend cập nhật bản ghi hình ảnh kiểu gói.
9. Hệ thống giữ nguyên trạng thái Active/Inactive hiện tại của bản ghi.
10. Hệ thống thông báo cập nhật thành công.
11. Hệ thống hiển thị dữ liệu mới trong danh sách.

### Alternative Flows

#### ALT-01 — Admin không thay ảnh preview
- Admin chỉ chỉnh sửa tên hoặc mô tả.
- Hệ thống giữ ảnh preview hiện tại.
- Backend cập nhật các trường được thay đổi.

#### ALT-02 — Admin hủy thao tác cập nhật
- Admin chọn hủy hoặc đóng form.
- Hệ thống đóng form cập nhật.
- Hệ thống không thay đổi dữ liệu hình ảnh kiểu gói.

### Exception Flows

#### EXC-01 — Không tìm thấy bản ghi
- Backend không tìm thấy hình ảnh kiểu gói cần cập nhật.
- Hệ thống hiển thị thông báo dữ liệu không tồn tại hoặc đã bị xóa.
- Hệ thống không cập nhật dữ liệu.

#### EXC-02 — Dữ liệu cập nhật không hợp lệ
- Admin nhập thiếu tên hoặc dữ liệu vượt giới hạn cho phép.
- Hệ thống hiển thị lỗi tại trường chưa hợp lệ.
- Hệ thống không cập nhật dữ liệu.

#### EXC-03 — Ảnh preview mới không hợp lệ
- Admin thay ảnh preview bằng file không phải JPG hoặc PNG hoặc lớn hơn 10MB.
- Hệ thống hiển thị lỗi ảnh preview không hợp lệ.
- Hệ thống không cập nhật dữ liệu.

#### EXC-04 — Không có quyền cập nhật
- Backend từ chối request.
- Hệ thống không cập nhật dữ liệu hình ảnh kiểu gói.

---

## Acceptance Criteria

### AC-001
- **Given**: Admin có quyền và hình ảnh kiểu gói tồn tại.
- **When**: Admin chọn thao tác cập nhật.
- **Then**: Hệ thống mở form cập nhật với dữ liệu hiện tại.

### AC-002
- **Given**: Admin cập nhật tên hoặc mô tả hợp lệ.
- **When**: Admin chọn lưu.
- **Then**:
  - Hệ thống cập nhật dữ liệu hình ảnh kiểu gói.
  - Trạng thái Active/Inactive hiện tại không bị thay đổi.

### AC-003
- **Given**: Admin thay ảnh preview.
- **When**: Ảnh mới có định dạng JPG hoặc PNG và dung lượng không vượt quá 10MB.
- **Then**: Hệ thống cập nhật ảnh preview mới.

### AC-004
- **Given**: Admin thay ảnh preview.
- **When**: Ảnh mới không phải JPG hoặc PNG hoặc lớn hơn 10MB.
- **Then**:
  - Hệ thống không cập nhật dữ liệu.
  - Hệ thống hiển thị lỗi ảnh preview không hợp lệ.

### AC-005
- **Given**: Hình ảnh kiểu gói đã bị xóa hoặc không tồn tại.
- **When**: Admin gửi request cập nhật.
- **Then**:
  - Backend từ chối cập nhật.
  - Hệ thống hiển thị thông báo dữ liệu không tồn tại hoặc đã bị xóa.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Nguồn | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực | Ghi chú / Link logic |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| [BR-275](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1b63fe10-594e-470a-b8f7-ab07dc6d1d35) | Cập nhật thông tin hình ảnh kiểu gói | Quản lý dữ liệu AI Custom | Hình ảnh kiểu gói chỉ được cập nhật khi Admin có quyền quản lý dữ liệu AI Custom, bản ghi còn tồn tại và dữ liệu cập nhật hợp lệ. | Admin thực hiện thao tác cập nhật thông tin hình ảnh kiểu gói trong Website quản trị. | Backend phải kiểm tra các điều kiện sau trước khi cập nhật: - Người dùng có quyền quản lý dữ liệu AI Custom. - Hình ảnh kiểu gói cần cập nhật tồn tại và chưa bị xóa. - Tên hình ảnh kiểu gói là bắt buộc nếu được cập nhật. - Tên và mô tả phải đáp ứng giới hạn dữ liệu do hệ thống cấu hình. - Nếu thay ảnh preview, ảnh mới chỉ chấp nhận định dạng JPG hoặc PNG. - Nếu thay ảnh preview, dung lượng ảnh mới không vượt quá 10MB. Việc cập nhật thông tin không được tự động thay đổi trạng thái Active/Inactive hiện tại của bản ghi. | Không cho phép cập nhật nếu người dùng không có quyền, bản ghi không tồn tại, dữ liệu cập nhật không hợp lệ, ảnh preview mới sai định dạng hoặc ảnh preview mới vượt quá dung lượng cho phép. | Product discussion 2026-09-12; STORY-076 | Đức Bình | STORY-076 | Draft | v0 | 2026-09-12 | Việc đổi trạng thái Active/Inactive và việc xóa hình ảnh kiểu gói được xử lý theo các rule riêng. |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-275 | [BR-275](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1b63fe10-594e-470a-b8f7-ab07dc6d1d35) |

### Dependencies

- [STORY-074](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/322aae9b-a60d-43c2-ac07-2678379e48b8)

---

## Non-Functional Requirements

- Backend kiểm tra quyền và tính hợp lệ của dữ liệu trong mọi request cập nhật.
- UI không làm mất dữ liệu hiện tại nếu cập nhật thất bại.
- Thao tác cập nhật có loading và thông báo kết quả rõ ràng.

---

## Out of Scope

- Đổi trạng thái Active/Inactive.
- Xóa hình ảnh kiểu gói.
- Cập nhật dữ liệu của yêu cầu tạo mẫu hoa đã tạo trước đó.

---
