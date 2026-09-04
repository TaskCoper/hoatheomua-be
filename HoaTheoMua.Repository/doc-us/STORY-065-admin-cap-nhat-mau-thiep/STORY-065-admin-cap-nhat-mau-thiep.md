# STORY-065 — Admin cập nhật mẫu thiệp

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý mẫu thiệp, tôi muốn cập nhật thông tin mẫu thiệp để chỉnh sửa tên mẫu, mô tả hoặc ảnh template/ảnh Preview khi thông tin mẫu thiệp cần thay đổi. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Reviewer** | Nguyễn Đức Bình |
| **Approver** | Nguyễn Đức Bình |
| **Owner** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Đã duyệt |
| **Cập nhật** | 28/08/2026 |

---

## Context

Mẫu thiệp được quản lý tại Core Database và được sử dụng trong quy trình tạo thiệp tại Checkout. Admin có thể cập nhật tên mẫu, mô tả và ảnh mẫu thiệp của mẫu thiệp chưa bị xóa mềm. 

Ảnh template và ảnh Preview là cùng một ảnh của mẫu thiệp. Ảnh này được sử dụng làm template trong quy trình tạo thiệp và đồng thời được hiển thị dưới dạng Preview tại màn hình quản lý. Việc cập nhật thông tin mẫu thiệp không làm thay đổi trạng thái Active/Inactive của mẫu thiệp. Thông tin sau khi cập nhật được sử dụng cho các lần tạo thiệp mới. Việc cập nhật mẫu thiệp không được làm thay đổi nội dung hoặc ảnh của các thiệp, Checkout hoặc Order đã tồn tại.

**Quy tắc dữ liệu:**
- **Tên mẫu:** Bắt buộc và không vượt quá 20 từ.
- **Mô tả:** Không bắt buộc và không vượt quá 200 ký tự.
- **Ảnh mẫu thiệp:** Nếu có ảnh mới, ảnh phải thuộc định dạng PNG hoặc JPG và có dung lượng không vượt quá 10 MB.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý mẫu thiệp.
- Admin đang thao tác trên một mẫu thiệp được hiển thị trong danh sách quản lý.

### Trigger
> Admin chọn thao tác cập nhật tại một mẫu thiệp trong danh sách quản lý.

---

## Flow

### Main Flow: Admin cập nhật mẫu thiệp thành công

1. Admin xem danh sách mẫu thiệp.
2. Admin chọn thao tác cập nhật tại một mẫu thiệp.
3. Hệ thống xác định mẫu thiệp được chọn.
4. Hệ thống tải thông tin hiện tại của mẫu thiệp từ Core Database.
5. Hệ thống hiển thị form cập nhật với dữ liệu hiện tại gồm tên mẫu, mô tả nếu có và ảnh mẫu thiệp.
6. Khi chưa có dữ liệu nào được thay đổi, nút “Lưu” ở trạng thái disable.
7. Admin chỉnh sửa một hoặc nhiều thông tin.
8. Khi có ít nhất một thông tin thay đổi, hệ thống enable nút “Lưu”.
9. Admin chọn “Lưu”.
10. Hệ thống kiểm tra dữ liệu.
11. Hệ thống kiểm tra mẫu thiệp còn tồn tại và chưa bị xóa mềm.
12. Nếu dữ liệu hợp lệ, hệ thống cập nhật thông tin mẫu thiệp trong Core Database.
13. Nếu Admin thay ảnh, hệ thống cập nhật tham chiếu ảnh mới của mẫu thiệp.
14. Nếu Admin không thay ảnh, hệ thống giữ nguyên ảnh hiện tại.
15. Hệ thống giữ nguyên trạng thái Active/Inactive hiện tại của mẫu thiệp.
16. Hệ thống thông báo cập nhật thành công.
17. Danh sách mẫu thiệp hiển thị thông tin mới.

---

### Alternative Flows

#### ALT-01 — Không thay đổi ảnh
1. Admin mở form cập nhật mẫu thiệp.
2. Admin thay đổi thông tin (tên mẫu/mô tả) nhưng không chọn ảnh mới.
3. Admin chọn lưu.
4. Hệ thống cập nhật các thông tin đã thay đổi.
5. Hệ thống giữ nguyên ảnh template/ảnh Preview hiện tại.
6. Hệ thống thông báo cập nhật thành công.

#### ALT-02 — Thay đổi ảnh
1. Admin mở form cập nhật mẫu thiệp.
2. Admin chọn ảnh mới.
3. Hệ thống kiểm tra ảnh theo điều kiện file được hỗ trợ (PNG/JPG, <=10MB).
4. Admin chọn lưu.
5. Hệ thống lưu ảnh mới và cập nhật tham chiếu ảnh của mẫu thiệp.
6. Hệ thống thông báo cập nhật thành công.
7. Danh sách hiển thị ảnh Preview mới.

#### ALT-03 — Admin hủy cập nhật
1. Admin đang ở form cập nhật mẫu thiệp.
2. Admin chọn hủy hoặc đóng form.
3. Hệ thống không cập nhật dữ liệu mẫu thiệp.
4. Mẫu thiệp giữ nguyên thông tin trước khi thao tác.

#### ALT-04 — Không có dữ liệu thay đổi
1. Admin mở form cập nhật mẫu thiệp.
2. Admin chưa thay đổi tên mẫu, mô tả hoặc ảnh.
3. Hệ thống giữ nút “Lưu” ở trạng thái disable.
4. Admin không thể gửi yêu cầu cập nhật khi chưa có dữ liệu thay đổi.

---

### Exception Flows

#### EXC-01 — Dữ liệu cập nhật không hợp lệ
1. Admin nhập dữ liệu và chọn lưu.
2. Hệ thống xác định một hoặc nhiều trường không hợp lệ.
3. Hệ thống không cập nhật mẫu thiệp.
4. Hệ thống hiển thị lỗi tại trường tương ứng.
5. Admin có thể chỉnh sửa dữ liệu và thử lại.

#### EXC-02 — Ảnh không hợp lệ
1. Admin chọn ảnh template/ảnh Preview mới.
2. Hệ thống xác định file không đáp ứng điều kiện ảnh được hỗ trợ.
3. Hệ thống không sử dụng ảnh đó để cập nhật mẫu thiệp.
4. Hệ thống hiển thị thông báo phù hợp.
5. Admin có thể chọn ảnh khác.

#### EXC-03 — Mẫu thiệp không tồn tại hoặc đã bị xóa mềm
1. Admin chọn lưu thay đổi.
2. Hệ thống không tìm thấy mẫu thiệp tương ứng trong Core Database hoặc mẫu thiệp đã bị xóa mềm.
3. Hệ thống không cập nhật dữ liệu.
4. Hệ thống thông báo mẫu thiệp không còn tồn tại hoặc dữ liệu đã thay đổi.
5. Hệ thống tải lại danh sách mẫu thiệp.

#### EXC-04 — Lỗi cập nhật
1. Admin chọn lưu dữ liệu hợp lệ.
2. Hệ thống gặp lỗi trong quá trình cập nhật.
3. Hệ thống không lưu dữ liệu cập nhật dở dang.
4. Mẫu thiệp giữ nguyên dữ liệu hợp lệ trước lần cập nhật.
5. Hệ thống thông báo cập nhật thất bại.
6. Admin có thể thử lại.

#### EXC-05 — Không có quyền thực hiện
1. Người dùng gửi yêu cầu cập nhật mẫu thiệp.
2. Hệ thống xác định người dùng không có quyền quản lý mẫu thiệp.
3. Hệ thống từ chối yêu cầu.
4. Hệ thống không thay đổi dữ liệu mẫu thiệp.
5. Hệ thống hiển thị thông báo phù hợp.

---

## Acceptance Criteria

### AC-001 — Hiển thị dữ liệu hiện tại
- **Given:** Admin có quyền quản lý mẫu thiệp và chọn cập nhật một mẫu thiệp.
- **When:** form cập nhật được mở.
- **Then:** hệ thống hiển thị tên mẫu, mô tả nếu có và ảnh mẫu thiệp hiện tại.

### AC-002 — Cập nhật thông tin thành công
- **Given:** Admin đang cập nhật một mẫu thiệp và dữ liệu nhập hợp lệ.
- **When:** Admin chọn lưu thay đổi.
- **Then:** hệ thống cập nhật thông tin mẫu thiệp trong Core Database.
- **And:** hệ thống thông báo cập nhật thành công.
- **And:** danh sách mẫu thiệp hiển thị thông tin mới.

### AC-003 — Giữ nguyên ảnh hiện tại
- **Given:** Admin đang cập nhật mẫu thiệp và mẫu thiệp đã có ảnh.
- **When:** Admin lưu thay đổi mà không chọn ảnh mới.
- **Then:** hệ thống giữ nguyên ảnh template/ảnh Preview hiện tại.

### AC-004 — Thay ảnh mới thành công
- **Given:** Admin chọn một ảnh mới hợp lệ.
- **When:** Admin lưu thay đổi.
- **Then:** hệ thống cập nhật tham chiếu ảnh mới cho mẫu thiệp.
- **And:** danh sách hiển thị ảnh Preview mới.

### AC-005 — Không đổi trạng thái
- **Given:** mẫu thiệp đang ở trạng thái Active hoặc Inactive.
- **When:** Admin cập nhật thông tin mẫu thiệp thành công.
- **Then:** trạng thái Active/Inactive của mẫu thiệp phải giữ nguyên.

### AC-006 — Hủy thao tác
- **Given:** Admin đang ở form cập nhật mẫu thiệp.
- **When:** Admin chọn hủy hoặc đóng form.
- **Then:** hệ thống không cập nhật dữ liệu mẫu thiệp.
- **And:** mẫu thiệp giữ nguyên thông tin trước khi thao tác.

### AC-007 — Validation tên mẫu
- **Given:** Admin đang cập nhật mẫu thiệp.
- **When:** Admin nhập tên mẫu để trống hoặc vượt quá 20 từ.
- **Then:** không cho phép cập nhật mẫu thiệp.
- **And:** hệ thống hiển thị lỗi tại trường tên mẫu.

### AC-008 — Validation ảnh
- **Given:** Admin chọn ảnh mới.
- **When:** ảnh không thuộc định dạng PNG hoặc JPG hoặc có dung lượng vượt quá 10 MB.
- **Then:** hệ thống không sử dụng ảnh đó để cập nhật mẫu thiệp.
- **And:** hệ thống hiển thị thông báo phù hợp.

### AC-009 — Lỗi dữ liệu không tồn tại/đã bị xóa mềm
- **Given:** Admin đang cập nhật một mẫu thiệp.
- **When:** Admin lưu nhưng mẫu thiệp không còn tồn tại trong Core Database hoặc đã bị xóa mềm.
- **Then:** hệ thống không được cập nhật dữ liệu.
- **And:** hệ thống hiển thị thông báo dữ liệu không còn tồn tại hoặc đã thay đổi.
- **And:** hệ thống tải lại danh sách mẫu thiệp.

### AC-010 — Lỗi cập nhật
- **Given:** Admin gửi dữ liệu cập nhật hợp lệ.
- **When:** quá trình cập nhật thất bại (mất kết nối, lỗi DB,...).
- **Then:** hệ thống không được lưu dữ liệu dở dang hoặc không đồng nhất.
- **And:** hệ thống thông báo cập nhật thất bại.
- **And:** Admin có thể thử lại.

### AC-011 — Kiểm tra quyền Admin
- **Given:** người dùng không có quyền quản lý mẫu thiệp.
- **When:** người dùng gửi yêu cầu cập nhật mẫu thiệp.
- **Then:** hệ thống phải từ chối yêu cầu.
- **And:** không được thay đổi dữ liệu mẫu thiệp.

### AC-012 — Validation mô tả
- **Given:** Admin đang cập nhật mẫu thiệp.
- **When:** Admin nhập mô tả vượt quá 200 ký tự.
- **Then:** hệ thống không cho phép cập nhật mẫu thiệp.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-232**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ab34cf24-bed6-4353-84ca-b0dcdef83794) | Quyền cập nhật mẫu thiệp | Phân quyền | Chỉ Admin có quyền quản lý mẫu thiệp mới được cập nhật thông tin mẫu thiệp. | Người dùng gửi yêu cầu cập nhật mẫu thiệp. | Hệ thống kiểm tra quyền trước khi thực hiện cập nhật. | Người dùng không có quyền không được thay đổi dữ liệu. | Admin có quyền quản lý mẫu thiệp | STORY-065 | Draft | v0 | 2026-08-27 |
| [**BR-233**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fdb72537-b0de-4c2a-9673-aa6b52979140) | Thông tin được phép cập nhật của mẫu thiệp | Quản lý mẫu thiệp | Admin được phép cập nhật các thông tin quản lý của mẫu thiệp gồm tên mẫu, mô tả và ảnh. | Admin cập nhật một mẫu thiệp chưa bị xóa mềm. | Hệ thống chỉ cập nhật các thông tin được thay đổi và lưu vào Core Database. | Việc chuyển trạng thái/xóa mẫu thiệp thuộc chức năng riêng. | Admin có quyền quản lý mẫu thiệp | STORY-065 | Draft | v0 | 2026-08-27 |
| [**BR-234**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d5e941e4-e526-4813-a674-e9c763d05111) | Cập nhật mẫu thiệp không thay đổi trạng thái | Quản lý mẫu thiệp | Cập nhật thông tin mẫu thiệp không được tự động thay đổi trạng thái Active/Inactive. | Admin cập nhật thông tin mẫu thiệp. | Hệ thống giữ nguyên trạng thái hiện tại của mẫu thiệp sau khi cập nhật thành công. | Việc thay đổi trạng thái chỉ thực hiện qua chức năng riêng. | Admin có quyền quản lý mẫu thiệp | STORY-059, STORY-065 | Draft | v0 | 2026-08-27 |
| [**BR-235**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ec4cbcf6-2f41-4ef9-bc11-f0a2425a21a9) | Ảnh mẫu thiệp khi cập nhật | Quản lý mẫu thiệp | Ảnh template/ảnh Preview hiện tại phải được giữ nguyên nếu Admin không cung cấp ảnh mới hợp lệ. | Admin cập nhật mẫu thiệp. | Cập nhật ảnh mới nếu có; nếu không, giữ nguyên ảnh hiện tại. | Ảnh không hợp lệ không được sử dụng. | Admin có quyền quản lý mẫu thiệp | STORY-057, STORY-065 | Draft | v0 | 2026-08-27 |
| [**BR-236**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df42784d-c617-4436-9d8f-7f8d00515a4c) | Cập nhật mẫu thiệp không làm thay đổi dữ liệu đã tồn tại | Quản lý mẫu thiệp | Cập nhật mẫu thiệp chỉ áp dụng cho dữ liệu mẫu được sử dụng cho các lần tạo thiệp mới. | Admin cập nhật mẫu thiệp thành công. | Hệ thống lưu thông tin mới mà không thay đổi dữ liệu thiệp/Checkout/Order đã tồn tại. | Dữ liệu lịch sử giữ nguyên tại thời điểm phát sinh. | Admin có quyền quản lý mẫu thiệp | STORY-035, STORY-065 | Draft | v0 | 2026-08-27 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-232 | [Quyền cập nhật mẫu thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ab34cf24-bed6-4353-84ca-b0dcdef83794) |
| BR-233 | [Thông tin được phép cập nhật của mẫu thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fdb72537-b0de-4c2a-9673-aa6b52979140) |
| BR-234 | [Cập nhật mẫu thiệp không thay đổi trạng thái](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d5e941e4-e526-4813-a674-e9c763d05111) |
| BR-235 | [Ảnh mẫu thiệp khi cập nhật](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ec4cbcf6-2f41-4ef9-bc11-f0a2425a21a9) |
| BR-236 | [Cập nhật mẫu thiệp không làm thay đổi dữ liệu đã tồn tại](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/df42784d-c617-4436-9d8f-7f8d00515a4c) |

### Dependencies
- [**STORY-057**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [**STORY-035**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

---

## Non-Functional Requirements

- Backend phải kiểm tra quyền Admin trước khi cập nhật mẫu thiệp.
- Backend phải kiểm tra lại mẫu thiệp còn tồn tại và chưa bị xóa mềm trước khi cập nhật.
- Mỗi yêu cầu cập nhật chỉ được áp dụng cho đúng mẫu thiệp được chỉ định.
- Lỗi cập nhật không được tạo dữ liệu dở dang hoặc không đồng nhất.
- Ảnh Preview không được expose đường dẫn storage nội bộ.
- Core Database là nguồn xác thực cuối cùng của thông tin mẫu thiệp.

---

## Out of Scope

- Thêm mẫu thiệp.
- Xóa mẫu thiệp.
- Chuyển trạng thái Active/Inactive.
- Tạo thiệp cho khách hàng.
- Thay đổi nội dung hoặc ảnh của thiệp, Checkout hoặc Order đã tồn tại.
