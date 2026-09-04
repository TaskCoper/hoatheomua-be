# STORY-055 — Admin thêm kích thước thiệp

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **Phiên bản** | v0.1 |
| **User Story** | Là một Admin có quyền quản lý cấu hình thiệp, tôi muốn thêm kích thước thiệp mới để hệ thống có thêm lựa chọn size khi khách hàng tạo thiệp. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Reviewer** | Chưa chỉ định |
| **Approver** | Chưa chỉ định |
| **Owner** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Cập nhật** | 26/08/2026 |

---

## Context

Admin có thể thêm kích thước thiệp mới vào Core Database.

Kích thước thiệp mới cần có:
- **Kích thước** dạng rộng x cao, đơn vị cm x cm.
- **Giá size.**
- **Số lượng từ tối đa.**

Kích thước thiệp mới sau khi tạo thành công có trạng thái mặc định là **Active** và `isDelete = false`.

Phụ phí viết tay không được tạo trong US này.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý cấu hình thiệp.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin chọn chức năng **thêm kích thước thiệp**.

---

## Flow

### Main Flow

> ⚠️ *Main Flow chưa được định nghĩa chi tiết trong phiên bản hiện tại.*

---

## Non-Functional Requirements

- Backend phải kiểm tra quyền Admin trước khi lưu dữ liệu.
- Backend phải kiểm tra lại validation, không chỉ phụ thuộc Frontend.
- Hệ thống không được tạo dữ liệu dở dang nếu lưu thất bại.

---

## Out of Scope

- Cập nhật kích thước thiệp đã tồn tại.
- Xóa kích thước thiệp.
- Chuyển trạng thái kích thước thiệp.
- Cấu hình phụ phí viết tay.
