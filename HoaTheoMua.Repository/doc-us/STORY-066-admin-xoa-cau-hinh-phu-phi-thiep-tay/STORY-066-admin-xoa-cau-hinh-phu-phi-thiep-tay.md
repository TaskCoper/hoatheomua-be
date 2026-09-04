# STORY-066 — Admin xóa cấu hình phụ phí thiệp viết tay

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý cấu hình giá thiệp, tôi muốn xóa cấu hình phụ phí thiệp viết tay không còn sử dụng để hệ thống không tiếp tục áp dụng cấu hình đó cho các thiệp Calligraphy mới. |
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
| **Cập nhật** | 27/08/2026 |

---

## Context

Phụ phí thiệp viết tay được xác định theo khoảng số lượng từ. Mỗi cấu hình phụ phí thiệp viết tay gồm:
- Số lượng từ bắt đầu.
- Số lượng từ kết thúc.
- Giá phụ phí.
- Trạng thái Active hoặc Inactive.

*Ví dụ:*
- Từ 1 đến 10 từ: phụ phí 20.000đ.
- Từ 11 đến 20 từ: phụ phí 30.000đ.

Thao tác xóa sử dụng cơ chế **xóa mềm** bằng cách cập nhật `isDelete = true`. Cấu hình đã bị xóa mềm không còn hiển thị trong danh sách quản lý mặc định và không được sử dụng khi hệ thống xác định phụ phí cho các thiệp Calligraphy mới.

Việc xóa mềm không xóa vật lý dữ liệu khỏi Core Database và không làm thay đổi giá hoặc dữ liệu của thiệp, Checkout hoặc Order đã tồn tại trước thời điểm xóa.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang thao tác trên một cấu hình phụ phí thiệp viết tay được hiển thị trong danh sách quản lý.

### Trigger
> Admin chọn thao tác xóa tại một cấu hình phụ phí thiệp viết tay.

---

## Flow

### Main Flow: Admin xóa cấu hình phụ phí thiệp viết tay

1. Admin xem danh sách cấu hình phụ phí thiệp viết tay.
2. Admin chọn thao tác xóa tại một cấu hình.
3. Hệ thống hiển thị popup xác nhận xóa, trong đó hiển thị khoảng số lượng từ, giá phụ phí của cấu hình và cảnh báo rằng sau khi xóa, khoảng này sẽ không còn cấu hình phụ phí tương ứng cho thiệp Calligraphy mới.
4. Admin xác nhận thao tác.
5. Hệ thống xác định cấu hình cần xóa.
6. Hệ thống kiểm tra cấu hình còn tồn tại và chưa bị xóa mềm.
7. Hệ thống cập nhật `isDelete = true` trong Core Database.
8. Hệ thống thông báo xóa thành công.
9. Hệ thống tải lại danh sách cấu hình phụ phí thiệp viết tay.
10. Cấu hình vừa xóa không còn hiển thị trong danh sách mặc định.

---

### Alternative Flows

#### ALT-01 — Admin hủy popup xác nhận
1. Admin chọn thao tác xóa một cấu hình phụ phí thiệp viết tay.
2. Hệ thống hiển thị popup xác nhận xóa kèm thông tin cấu hình và cảnh báo ảnh hưởng sau khi xóa.
3. Admin chọn hủy hoặc đóng popup.
4. Hệ thống không cập nhật `isDelete`.
5. Cấu hình giữ nguyên dữ liệu và tiếp tục hiển thị trong danh sách quản lý.

---

### Exception Flows

#### EXC-01 — Cấu hình không tồn tại hoặc đã bị xóa mềm
1. Admin xác nhận xóa một cấu hình phụ phí thiệp viết tay.
2. Hệ thống không tìm thấy cấu hình tương ứng trong Core Database hoặc cấu hình đã có `isDelete = true`.
3. Hệ thống không thực hiện cập nhật.
4. Hệ thống thông báo dữ liệu không còn tồn tại hoặc đã thay đổi.
5. Hệ thống tải lại danh sách cấu hình phụ phí thiệp viết tay.

#### EXC-02 — Lỗi khi xóa cấu hình
1. Admin xác nhận xóa cấu hình phụ phí thiệp viết tay.
2. Hệ thống gặp lỗi trong quá trình cập nhật.
3. Hệ thống không thay đổi `isDelete`.
4. Cấu hình giữ nguyên dữ liệu trước khi thao tác.
5. Hệ thống thông báo xóa thất bại.
6. Admin có thể thử lại.

#### EXC-03 — Không có quyền thực hiện
1. Người dùng gửi yêu cầu xóa cấu hình phụ phí thiệp viết tay.
2. Hệ thống xác định người dùng không có quyền quản lý cấu hình giá thiệp.
3. Hệ thống từ chối yêu cầu.
4. Hệ thống không thay đổi dữ liệu.
5. Hệ thống hiển thị thông báo phù hợp.

---

## Acceptance Criteria

### AC-001 — Hiển thị popup xóa
- **Given:** Admin có quyền quản lý cấu hình giá thiệp và cấu hình chưa bị xóa mềm.
- **When:** Admin chọn thao tác xóa.
- **Then:** Hệ thống hiển thị popup xác nhận trước khi thực hiện xóa.
- **And:** popup phải hiển thị khoảng số lượng từ và giá phụ phí của cấu hình đang xóa.
- **And:** popup phải cảnh báo rằng sau khi xóa, các thiệp Calligraphy thuộc khoảng số lượng từ đó sẽ không còn cấu hình phụ phí tương ứng.

### AC-002 — Xóa cấu hình thành công
- **Given:** Popup xác nhận xóa đang hiển thị.
- **When:** Admin xác nhận.
- **Then:** Hệ thống cập nhật `isDelete = true` cho cấu hình trong Core Database.
- **And:** Hệ thống thông báo xóa thành công.
- **And:** Cấu hình không còn hiển thị trong danh sách mặc định.

### AC-003 — Chặn cấu hình đã xóa mềm
- **Given:** Cấu hình phụ phí thiệp viết tay đã có `isDelete = true`.
- **When:** Hệ thống xác định phụ phí theo số lượng từ của một thiệp Calligraphy mới.
- **Then:** Hệ thống không được sử dụng cấu hình đó.

### AC-004 — Hủy thao tác xóa
- **Given:** Admin đang ở popup xác nhận xóa cấu hình phụ phí thiệp viết tay.
- **When:** Admin chọn hủy hoặc đóng popup.
- **Then:** Hệ thống không được cập nhật `isDelete`.
- **And:** Cấu hình giữ nguyên dữ liệu.

### AC-005 — Cấu hình không tồn tại
- **Given:** Admin đã xác nhận xóa một cấu hình phụ phí thiệp viết tay.
- **When:** Cấu hình không còn tồn tại trong Core Database hoặc đã có `isDelete = true`.
- **Then:** Hệ thống không được thực hiện cập nhật.
- **And:** Hệ thống hiển thị thông báo dữ liệu không còn tồn tại hoặc đã thay đổi.
- **And:** Hệ thống tải lại danh sách.

### AC-006 — Lỗi cập nhật isDelete
- **Given:** Admin đã xác nhận xóa cấu hình phụ phí thiệp viết tay.
- **When:** Quá trình cập nhật `isDelete` thất bại.
- **Then:** Cấu hình phải giữ nguyên trạng thái trước khi thao tác.
- **And:** Hệ thống thông báo xóa thất bại.
- **And:** Admin có thể thử lại.

### AC-007 — Không có quyền xóa
- **Given:** Người dùng không có quyền quản lý cấu hình giá thiệp.
- **When:** Người dùng gửi yêu cầu xóa cấu hình phụ phí thiệp viết tay.
- **Then:** Hệ thống phải từ chối yêu cầu.
- **And:** Không được thay đổi dữ liệu cấu hình.

### AC-008 — Không làm thay đổi lịch sử
- **Given:** Cấu hình phụ phí thiệp viết tay đã từng được sử dụng để xác định phụ phí trước đó.
- **When:** Admin xóa mềm cấu hình.
- **Then:** Hệ thống không được tính lại hoặc thay đổi giá của thiệp, Checkout hoặc Order đã tồn tại.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-247**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7c0d412a-add2-48a2-876c-d385faabb75c) | Cấu hình đã xóa mềm không được áp dụng | Cấu hình giá thiệp | Cấu hình đã bị xóa mềm không được sử dụng để tính phụ phí cho thiệp Calligraphy mới. | Hệ thống xác định phụ phí cho thiệp mới. | Chỉ sử dụng cấu hình có isDelete = false. | Cấu hình đã xóa mềm vẫn được lưu để phục vụ lịch sử. | Admin có quyền quản lý cấu hình giá thiệp | STORY-066, STORY-035 | Draft | v0 | 2026-08-27 |
| [**BR-248**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/10c7fcf4-0616-45f7-b2df-dc75c7aaf952) | Xóa cấu hình không làm thay đổi dữ liệu đã tồn tại | Quản lý cấu hình giá thiệp | Xóa mềm một cấu hình không làm thay đổi dữ liệu nghiệp vụ đã phát sinh trước thời điểm xóa. | Admin xóa một cấu hình đã từng được áp dụng. | Hệ thống chỉ cập nhật isDelete, giữ nguyên giá của thiệp/Checkout/Order cũ. | Các lần xác định phụ phí mới không dùng cấu hình đã xóa. | Admin có quyền quản lý cấu hình giá thiệp | STORY-066, STORY-035 | Draft | v0 | 2026-08-27 |
| [**BR-249**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1cc6e9f-ad07-4a0c-9b75-131c95f5047c) | Quyền xóa cấu hình phụ phí thiệp viết tay | Phân quyền | Chỉ Admin có quyền quản lý cấu hình giá thiệp mới được xóa cấu hình. | Người dùng gửi yêu cầu xóa cấu hình. | Hệ thống kiểm tra quyền trước khi cập nhật isDelete. | Người dùng không có quyền không được thay đổi dữ liệu. | Admin có quyền quản lý cấu hình giá thiệp | STORY-066 | Draft | v0 | 2026-08-27 |
| [**BR-246**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6cf316e0-7684-449e-971a-b08de3a917bf) | Xóa mềm cấu hình phụ phí thiệp viết tay | Quản lý cấu hình giá thiệp | Xóa một cấu hình phải sử dụng cơ chế xóa mềm thay vì xóa vật lý. | Admin xác nhận xóa một cấu hình chưa bị xóa mềm. | Hệ thống cập nhật isDelete = true trong Core Database. | Cấu hình không tồn tại/đã xóa thì không cập nhật lại. | Admin có quyền quản lý cấu hình giá thiệp | STORY-066, STORY-061 | Draft | v0 | 2026-08-27 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-247 | [Cấu hình đã xóa mềm không được áp dụng cho thiệp mới](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7c0d412a-add2-48a2-876c-d385faabb75c) |
| BR-248 | [Xóa cấu hình không làm thay đổi dữ liệu đã tồn tại](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/10c7fcf4-0616-45f7-b2df-dc75c7aaf952) |
| BR-249 | [Quyền xóa cấu hình phụ phí thiệp viết tay](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a1cc6e9f-ad07-4a0c-9b75-131c95f5047c) |
| BR-246 | [Xóa mềm cấu hình phụ phí thiệp viết tay](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6cf316e0-7684-449e-971a-b08de3a917bf) |

### Dependencies
- [**STORY-061**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [**STORY-035**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

---

## Non-Functional Requirements

- Backend phải kiểm tra quyền Admin trước khi xóa.
- Backend phải kiểm tra lại cấu hình còn tồn tại và chưa bị xóa mềm trước khi cập nhật.
- Mỗi yêu cầu xóa chỉ áp dụng cho đúng cấu hình được chỉ định.
- Thao tác xóa phải sử dụng xóa mềm; không xóa vật lý bản ghi khỏi Core Database.
- Lỗi cập nhật không được lưu trạng thái dở dang hoặc không đồng nhất.
- Core Database là nguồn xác thực cuối cùng của `isDelete`.

---

## Out of Scope

- Xóa vật lý cấu hình khỏi Core Database.
- Khôi phục cấu hình đã xóa mềm.
- Thêm cấu hình phụ phí thiệp viết tay.
- Cập nhật khoảng số lượng từ hoặc giá phụ phí.
- Chuyển trạng thái Active/Inactive.
- Tính lại giá của thiệp, Checkout hoặc Order đã tồn tại.
