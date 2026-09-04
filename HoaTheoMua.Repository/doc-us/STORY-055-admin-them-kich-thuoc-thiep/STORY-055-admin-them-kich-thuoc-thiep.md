# STORY-055 — Admin thêm kích thước thiệp

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý cấu hình thiệp, tôi muốn thêm kích thước thiệp mới để hệ thống có thêm lựa chọn size khi khách hàng tạo thiệp. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Đã duyệt |

> **Feedback yêu cầu sửa gần nhất:**
> "Nên có thêm EXC về không có quyền truy cập. Chưa có Alt-flow về admin huỷ thêm mới." — *Nguyễn Đức Bình · 20:49 27/08/2026* (Đã được khắc phục trong phiên bản hiện tại bằng EXC-04 và ALT-01)

---

## Context

Admin có thể thêm kích thước thiệp mới vào Core Database. Kích thước thiệp mới cần có:
- **Kích thước** dạng rộng x cao, đơn vị cm x cm. Chiều rộng và chiều cao phải lớn hơn 0 và được phép là số thập phân. Hai kích thước chỉ được xem là trùng khi có cùng chiều rộng và cùng chiều cao; ví dụ 5x7 và 7x5 là hai kích thước khác nhau.
- **Giá size**.
- **Số lượng từ tối đa**.

Kích thước thiệp mới sau khi tạo thành công có trạng thái mặc định là Active và isDelete = false.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý cấu hình thiệp.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin chọn chức năng thêm kích thước thiệp.

---

## Flow

### Main Flow: Admin thêm kích thước thiệp thành công

1. Admin truy cập chức năng quản lý kích thước thiệp.
2. Admin chọn thêm kích thước thiệp.
3. Hệ thống hiển thị form thêm kích thước thiệp.
4. Admin nhập kích thước, giá size và số lượng từ tối đa.
5. Admin chọn lưu.
6. Hệ thống kiểm tra dữ liệu đầu vào.
7. Hệ thống lưu kích thước thiệp mới vào Core Database.
8. Hệ thống thiết lập trạng thái mặc định là Active và isDelete = false.
9. Hệ thống thông báo thêm kích thước thiệp thành công.
10. Hệ thống cập nhật danh sách kích thước thiệp.

---

### Alternative Flows

#### ALT-01 — Admin hủy thêm kích thước thiệp
1. Admin đang ở form thêm kích thước thiệp.
2. Admin chọn “Hủy”.
3. Hệ thống không tạo kích thước thiệp mới.
4. Hệ thống quay lại danh sách kích thước thiệp.

---

### Exception Flows

#### EXC-01 — Dữ liệu bắt buộc không hợp lệ
1. Admin nhập thiếu hoặc nhập không hợp lệ kích thước, giá size hoặc số lượng từ tối đa.
2. Hệ thống không lưu kích thước thiệp mới.
3. Hệ thống hiển thị lỗi tại trường tương ứng.

#### EXC-02 — Kích thước thiệp bị trùng
1. Admin nhập kích thước có cùng chiều rộng và cùng chiều cao với kích thước đã tồn tại có isDelete = false, bất kể kích thước đó đang Active hay Inactive.
2. Hệ thống không tạo kích thước thiệp mới.
3. Hệ thống hiển thị thông báo kích thước đã tồn tại.

#### EXC-03 — Lưu dữ liệu thất bại
1. Admin đã nhập dữ liệu hợp lệ.
2. Hệ thống không thể lưu dữ liệu vào Core Database.
3. Hệ thống không tạo kích thước thiệp mới.
4. Hệ thống hiển thị thông báo lỗi và cho phép Admin thử lại.

#### EXC-04 — Admin không có quyền truy cập
1. Admin không có quyền quản lý cấu hình thiệp.
2. Hệ thống từ chối truy cập chức năng thêm kích thước thiệp.
3. Hệ thống không hiển thị form thêm kích thước thiệp và không tạo dữ liệu mới.
4. Hệ thống hiển thị thông báo phù hợp về việc Admin không có quyền truy cập.

---

## Acceptance Criteria

### AC-001 — Lưu kích thước thiệp thành công
- **Given:** Admin có quyền quản lý cấu hình thiệp.
- **When:** Admin lưu kích thước thiệp mới.
- **Then:** hệ thống lưu kích thước thiệp vào Core Database.
- **And:** dữ liệu kích thước thiệp hợp lệ.
- **And:** trạng thái mặc định là Active.
- **And:** isDelete = false.

### AC-002 — Báo lỗi khi thiếu dữ liệu
- **Given:** Admin đang thêm kích thước thiệp.
- **When:** Admin nhập thiếu kích thước, giá size hoặc số lượng từ tối đa.
- **Then:** hệ thống không cho lưu.
- **And:** hiển thị lỗi tại trường tương ứng.

### AC-003 — Giá size không âm
- **Given:** Admin nhập giá size.
- **When:** hệ thống kiểm tra dữ liệu.
- **Then:** giá size phải không âm.

### AC-004 — Số lượng từ lớn hơn 0
- **Given:** Admin nhập số lượng từ tối đa.
- **When:** hệ thống kiểm tra dữ liệu.
- **Then:** số lượng từ tối đa phải lớn hơn 0

### AC-005 - Validation kích thước
- **Given:** Admin đang thêm kích thước thiệp mới.
- **When:** Admin nhập chiều rộng và chiều cao của kích thước thiệp.
- **Then:** Chiều rộng và chiều cao phải lớn hơn 0.
- **And:** Chiều rộng và chiều cao được phép là số thập phân.

### AC-006 — Không cho tạo kích thước trùng
- **Given:** Đã tồn tại kích thước thiệp có isDelete = false.
- **When:** Admin lưu kích thước mới có cùng chiều rộng và cùng chiều cao với kích thước đó.
- **Then:** Hệ thống không được tạo kích thước thiệp mới.
- **And:** Hệ thống phải thông báo kích thước đã tồn tại.
- **And:** Trạng thái Active hoặc Inactive của kích thước đã tồn tại không ảnh hưởng đến việc kiểm tra trùng.

### AC-007 - Hủy thêm kích thước thiệp
- **Given:** Admin đang ở form thêm kích thước thiệp.
- **When:** Admin chọn “Hủy”.
- **Then:** hệ thống không tạo kích thước thiệp mới.
- **And:** hệ thống quay lại danh sách kích thước thiệp.

### AC-008 - Admin không có quyền truy cập
- **Given:** Admin không có quyền quản lý cấu hình thiệp.
- **When:** Admin truy cập chức năng thêm kích thước thiệp.
- **Then:** hệ thống từ chối truy cập và không hiển thị form thêm kích thước thiệp.
- **And:** hệ thống không tạo dữ liệu mới.
- **And:** hệ thống hiển thị thông báo phù hợp.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-191**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c7ea3513-2bd6-4872-ad0c-da72c7929bac) | Thông tin bắt buộc khi thêm kích thước thiệp | Quản lý cấu hình thiệp | Kích thước thiệp mới phải có đủ thông tin bắt buộc trước khi được lưu vào hệ thống. | Admin thêm kích thước thiệp mới. | Hệ thống yêu cầu Admin nhập kích thước, giá size và số lượng từ tối đa. | Nếu thiếu hoặc không hợp lệ, hệ thống không tạo kích thước thiệp mới. | Admin có quyền quản lý cấu hình thiệp | STORY-055 | Draft | v0 | 2026-08-26 |
| [**BR-192**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6adb9749-5127-4c2b-a39e-e2ade6d4f533) | Trạng thái mặc định của kích thước thiệp mới | Quản lý cấu hình thiệp | Kích thước thiệp mới được tạo thành công phải có trạng thái mặc định là Active. | Admin thêm kích thước thiệp mới thành công. | Hệ thống lưu kích thước thiệp với trạng thái Active và isDelete = false. | Nếu thao tác lưu thất bại, hệ thống không tạo bản ghi dở dang. | Admin có quyền quản lý cấu hình thiệp | STORY-055 | Draft | v0 | 2026-08-26 |
| [**BR-193**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/222f8a1f-c5b1-4bf8-a54e-fadfc0efeb2f) | Không cho tạo trùng kích thước thiệp còn hiệu lực | Quản lý cấu hình thiệp | Hệ thống không cho phép tạo kích thước thiệp mới có cùng chiều rộng và cùng chiều cao với kích thước đã tồn tại có isDelete = false. | Admin nhập kích thước thiệp mới. | Hệ thống kiểm tra kích thước đã tồn tại trước khi lưu. Nếu tồn tại, hệ thống báo lỗi. | Nếu kích thước trùng nhưng đã bị xóa (isDelete = true), việc thêm mới được phép (nếu không có rule khác ngăn chặn). | Admin có quyền quản lý cấu hình thiệp | STORY-055 | Draft | v0 | 2026-08-26 |
| [**BR-250**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e15e3e5b-b45c-4d9a-9f1b-58fa81aee57b) | Điều kiện hợp lệ của dữ liệu kích thước thiệp | Kích thước thiệp | Các giá trị của cấu hình kích thước thiệp phải đáp ứng điều kiện dữ liệu hợp lệ trước khi được lưu. | Admin thêm kích thước thiệp mới. | Chiều rộng/cao > 0 và được phép là số thập phân. Giá size >= 0. Số lượng từ tối đa > 0 và nguyên. | Nếu giá trị không hợp lệ, hệ thống báo lỗi tại trường tương ứng. | Đức Bình | STORY-055 | Draft | v0 | 2026-08-27 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-191 | [Thông tin bắt buộc khi thêm kích thước thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c7ea3513-2bd6-4872-ad0c-da72c7929bac) |
| BR-192 | [Trạng thái mặc định của kích thước thiệp mới](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6adb9749-5127-4c2b-a39e-e2ade6d4f533) |
| BR-193 | [Không cho tạo trùng kích thước thiệp còn hiệu lực](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/222f8a1f-c5b1-4bf8-a54e-fadfc0efeb2f) |
| BR-250 | [Điều kiện hợp lệ của dữ liệu kích thước thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e15e3e5b-b45c-4d9a-9f1b-58fa81aee57b) |

### Dependencies
- [**STORY-054**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)

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
