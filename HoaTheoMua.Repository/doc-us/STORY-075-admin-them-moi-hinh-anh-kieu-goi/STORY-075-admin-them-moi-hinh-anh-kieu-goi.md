# STORY-075 — Admin thêm mới hình ảnh kiểu gói

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý dữ liệu AI Custom, tôi muốn thêm mới hình ảnh kiểu gói, để tạo dữ liệu kiểu gói dùng cho luồng tạo mẫu hoa AI. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Võ Gia Huy |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Admin thêm mới hình ảnh kiểu gói bằng cách nhập tên, mô tả và tải lên ảnh preview.
---

## Conditions

### Preconditions

- Người dùng đã đăng nhập Website quản trị.
- Người dùng có quyền quản lý dữ liệu AI Custom.
- Admin đang ở màn hình quản lý hình ảnh kiểu gói.

### Trigger

> - Admin chọn thao tác "Thêm mới".


---

## Flow

### Main Flow

1. Admin chọn "Thêm mới".
2. Hệ thống mở form thêm mới hình ảnh kiểu gói.
3. Admin tải lên ảnh preview.
4. Admin nhập tên hình ảnh kiểu gói.
5. Admin nhập mô tả.
6. Admin chọn lưu.
7. Backend kiểm tra quyền thao tác.
8. Backend kiểm tra ảnh preview có định dạng JPG hoặc PNG và dung lượng không vượt quá 10MB.
9. Backend kiểm tra tên và mô tả hợp lệ.
10. Hệ thống tạo bản ghi hình ảnh kiểu gói.
11. Hệ thống gán trạng thái mặc định là Inactive.
12. Hệ thống thông báo tạo mới thành công.
13. Hệ thống hiển thị bản ghi mới trong danh sách.

### Alternative Flows

#### ALT-01 — Admin hủy thao tác thêm mới
- Admin chọn hủy hoặc đóng form.
- Hệ thống đóng form thêm mới.
- Hệ thống không tạo bản ghi hình ảnh kiểu gói.

### Exception Flows

#### EXC-01 — Thiếu thông tin bắt buộc
- Admin để trống ảnh preview hoặc tên.
- Hệ thống hiển thị lỗi tại trường chưa hợp lệ.
- Hệ thống không cho lưu bản ghi.

#### EXC-02 — Ảnh preview sai định dạng
- Admin tải lên file không phải JPG hoặc PNG.
- Hệ thống hiển thị lỗi định dạng ảnh không hợp lệ.
- Hệ thống không cho lưu bản ghi.

#### EXC-03 — Ảnh preview vượt quá dung lượng cho phép
- Admin tải lên ảnh preview lớn hơn 10MB.
- Hệ thống hiển thị lỗi dung lượng ảnh không hợp lệ.
- Hệ thống không cho lưu bản ghi.

#### EXC-04 — Không có quyền thêm mới
- Backend từ chối request.
- Hệ thống không tạo bản ghi hình ảnh kiểu gói.

---

## Acceptance Criteria

### AC-001
- **Given**: Admin có quyền quản lý dữ liệu AI Custom.
- **When**: Admin chọn "Thêm mới".
- **Then**: Hệ thống mở form thêm mới hình ảnh kiểu gói.

### AC-002
- **Given**: Admin nhập đầy đủ dữ liệu hợp lệ.
- **When**: Admin chọn lưu.
- **Then**:
  - Hệ thống tạo bản ghi hình ảnh kiểu gói.
  - Trạng thái mặc định của bản ghi là Inactive.

### AC-003
- **Given**: Admin tải lên ảnh preview.
- **When**: File ảnh có định dạng khác JPG hoặc PNG.
- **Then**:
  - Hệ thống không cho lưu.
  - Hệ thống hiển thị lỗi định dạng ảnh không hợp lệ.

### AC-004
- **Given**: Admin tải lên ảnh preview.
- **When**: File ảnh lớn hơn 10MB.
- **Then**:
  - Hệ thống không cho lưu.
  - Hệ thống hiển thị lỗi dung lượng ảnh không hợp lệ.

### AC-005
- **Given**: Admin chưa nhập ảnh preview hoặc tên.
- **When**: Admin chọn lưu.
- **Then**:
  - Hệ thống hiển thị lỗi tại trường bắt buộc.
  - Không tạo bản ghi mới.

### AC-006
- **Given**: Người dùng không có quyền thêm mới hình ảnh kiểu gói.
- **When**: Người dùng gửi request thêm mới.
- **Then**:
  - Backend từ chối request.
  - Không tạo dữ liệu mới.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Nguồn | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực | Ghi chú / Link logic |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| [BR-274](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bb4c5ee0-0ef8-4e72-be5b-306f7fdc4317) | Thêm mới hình ảnh kiểu gói | Quản lý dữ liệu AI Custom | Hình ảnh kiểu gói mới chỉ được tạo khi Admin có quyền quản lý dữ liệu AI Custom và cung cấp đầy đủ dữ liệu hợp lệ. | Admin thực hiện thao tác thêm mới hình ảnh kiểu gói trong Website quản trị. | Backend phải kiểm tra các điều kiện sau trước khi tạo bản ghi: - Người dùng có quyền quản lý dữ liệu AI Custom. - Ảnh preview là bắt buộc. - Ảnh preview chỉ chấp nhận định dạng JPG hoặc PNG. - Dung lượng ảnh preview không vượt quá 10MB. - Tên hình ảnh kiểu gói là bắt buộc. - Tên và mô tả phải đáp ứng giới hạn dữ liệu do hệ thống cấu hình. Khi tạo mới thành công, hệ thống phải gán trạng thái mặc định của hình ảnh kiểu gói là Inactive. | Không cho phép tạo mới hình ảnh kiểu gói nếu người dùng không có quyền, thiếu ảnh preview, thiếu tên, ảnh preview sai định dạng hoặc ảnh preview vượt quá dung lượng cho phép. | Product discussion 2026-09-12; STORY-075 | Đức Bình | STORY-075 | Draft | v0 | 2026-09-12 | Trạng thái Inactive mặc định giúp cửa hàng kiểm tra dữ liệu trước khi cho khách hàng sử dụng trong luồng tạo mẫu hoa AI. Việc chuyển sang Active được xử lý theo story đổi trạng thái riêng. |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-274 | [BR-274](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bb4c5ee0-0ef8-4e72-be5b-306f7fdc4317) |

### Dependencies

- [STORY-074](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/322aae9b-a60d-43c2-ac07-2678379e48b8)

---

## Non-Functional Requirements

- Backend kiểm tra định dạng và dung lượng file, không chỉ dựa vào UI.
- File upload không lưu binary/Base64 trong payload metadata.
- UI hiển thị tiến trình hoặc trạng thái đang lưu khi gửi request.

---

## Out of Scope

- Tự động chuyển Active sau khi tạo mới.
- Cắt ảnh, chỉnh ảnh hoặc tạo ảnh bằng AI.
- Gắn hình ảnh kiểu gói vào yêu cầu tạo mẫu hoa.

---
