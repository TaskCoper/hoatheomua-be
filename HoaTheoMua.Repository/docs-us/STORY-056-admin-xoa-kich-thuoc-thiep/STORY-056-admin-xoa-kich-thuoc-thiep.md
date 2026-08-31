# STORY-056 — Admin xóa kích thước thiệp

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **Phiên bản** | v0.1 |
| **User Story** | Là một Admin có quyền quản lý cấu hình thiệp, tôi muốn xóa kích thước thiệp không còn sử dụng để khách hàng không tiếp tục chọn size đó khi tạo thiệp mới. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Reviewer** | Nguyễn Đức Bình |
| **Approver** | Nguyễn Đức Bình |
| **Owner** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Cập nhật** | 26/08/2026 |

---

## Context

Xóa kích thước thiệp là **xóa mềm** bằng cách cập nhật `isDelete = true`. Hệ thống không xóa cứng kích thước thiệp khỏi Core Database.

Việc xóa mềm không làm mất lịch sử thiệp, Checkout hoặc Order đã từng sử dụng kích thước đó.

Kích thước thiệp đã xóa mềm:
- Không hiển thị trong danh sách mặc định.
- Không hiển thị cho khách hàng khi tạo thiệp mới.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại trong Core Database.
- Kích thước thiệp chưa bị xóa mềm.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin chọn thao tác **xóa** tại một kích thước thiệp.

---

## Flow

### Main Flow — Admin xóa mềm kích thước thiệp thành công

1. Admin truy cập chức năng quản lý kích thước thiệp.
2. Hệ thống hiển thị danh sách kích thước thiệp chưa bị xóa mềm.
3. Admin chọn thao tác **"Xóa"** tại một kích thước thiệp.
4. Hệ thống hiển thị popup xác nhận: **"Bạn chắc chắn muốn xóa kích thước thiệp này không?"**
5. Admin chọn **"Yes"**.
6. Hệ thống cập nhật `isDelete = true` trong Core Database.
7. Hệ thống thông báo xóa kích thước thiệp thành công.
8. Hệ thống cập nhật lại danh sách mặc định và không còn hiển thị kích thước vừa xóa.

---

### Alternative Flows

#### ALT-01 — Admin hủy xóa

1. Tại popup xác nhận, Admin hủy hoặc đóng popup.
2. Hệ thống đóng popup.
3. Hệ thống không cập nhật `isDelete`.
4. Kích thước thiệp vẫn hiển thị như trước đó.

---

### Exception Flows

#### EXC-01 — Không có quyền xóa

1. Admin không có quyền quản lý cấu hình thiệp.
2. Hệ thống không cho phép xóa.
3. Hệ thống hiển thị thông báo phù hợp.
4. Hệ thống giữ nguyên dữ liệu kích thước thiệp.

#### EXC-02 — Kích thước không tồn tại hoặc đã bị xóa mềm

1. Admin xóa một kích thước không tồn tại hoặc đã bị xóa mềm trước đó.
2. Hệ thống không cập nhật dữ liệu.
3. Hệ thống hiển thị thông báo phù hợp.
4. Hệ thống tải lại danh sách hiện tại.

#### EXC-03 — Cập nhật xóa mềm thất bại

1. Admin đã xác nhận xóa.
2. Hệ thống không thể cập nhật `isDelete = true`.
3. Hệ thống không ghi nhận xóa thành công.
4. Hệ thống hiển thị thông báo lỗi và cho phép Admin thử lại.

---

## Acceptance Criteria

### AC-001 — Hiển thị popup xác nhận
- **Given:** Admin có quyền quản lý cấu hình thiệp và kích thước thiệp chưa bị xóa mềm.
- **When:** Admin chọn xóa kích thước thiệp.
- **Then:** Hệ thống hiển thị popup xác nhận "Bạn chắc chắn muốn xóa kích thước thiệp này không?".
- **And:** Popup có nút "Yes".

### AC-002 — Xóa mềm thành công
- **Given:** Hệ thống đang hiển thị popup xác nhận xóa.
- **When:** Admin chọn "Yes".
- **Then:** Hệ thống cập nhật `isDelete = true` trong Core Database.
- **And:** Kích thước thiệp không còn hiển thị trong danh sách mặc định.

### AC-003 — Không xóa cứng dữ liệu
- **Given:** Kích thước thiệp đã được xóa mềm.
- **When:** Hệ thống lưu thay đổi.
- **Then:** Hệ thống không được xóa cứng kích thước thiệp khỏi Core Database.

### AC-004 — Không ảnh hưởng lịch sử
- **Given:** Kích thước thiệp đã từng được dùng trong thiệp, Checkout hoặc Order.
- **When:** Admin xóa mềm kích thước thiệp.
- **Then:** Hệ thống không được làm mất dữ liệu lịch sử liên quan.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|
| **[BR-194](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7e4c47c5-65fe-48c8-a800-96c7a50c64e6)** | Xóa kích thước thiệp là xóa mềm | Quản lý cấu hình thiệp | Xóa kích thước thiệp trong hệ thống quản trị phải được thực hiện bằng cơ chế xóa mềm. | Admin xác nhận xóa kích thước thiệp. | Hệ thống cập nhật isDelete = true của kích thước thiệp trong Core Database. | Hệ thống không được xóa cứng dữ liệu kích thước thiệp khỏi Core Database khi thực hiện chức năng này. | Draft | v0 | 2026-08-26 |
| **[BR-195](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7d9328de-3fbc-46ab-bd3b-bef2b2c09ab2)** | Kích thước đã xóa mềm không hiển thị cho khách hàng | Quản lý cấu hình thiệp | Kích thước thiệp đã bị xóa mềm không được hiển thị cho khách hàng trong quy trình tạo thiệp mới. | Khách hàng truy cập bước chọn kích thước thiệp khi tạo thiệp. | Hệ thống loại trừ các kích thước thiệp có isDelete = true khỏi danh sách kích thước có thể chọn. | Kích thước đã xóa mềm vẫn có thể được dùng để hiển thị dữ liệu lịch sử của thiệp, Checkout hoặc Order đã tồn tại. | Draft | v0 | 2026-08-26 |
| **[BR-196](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/673fbaef-59fb-4a3a-985e-0dda27ffb300)** | Xóa mềm kích thước thiệp không ảnh hưởng lịch sử | Quản lý cấu hình thiệp | Việc xóa mềm kích thước thiệp không được làm mất dữ liệu lịch sử liên quan đến kích thước đó. | Kích thước thiệp đã từng được sử dụng trong thiệp, Checkout hoặc Order. | Hệ thống giữ lại dữ liệu kích thước thiệp và liên kết với các dữ liệu lịch sử đã tồn tại. | US này không định nghĩa chức năng khôi phục kích thước thiệp đã xóa. | Draft | v0 | 2026-08-26 |

---

## References

### Business Rules
`BR-194` · `BR-195` · `BR-196`

---

## Non-Functional Requirements

- Backend phải kiểm tra quyền Admin trước khi cập nhật `isDelete`.
- Hệ thống không được hiển thị xóa thành công nếu Core Database chưa cập nhật thành công.

---

## Out of Scope

- Xóa cứng kích thước thiệp.
- Khôi phục kích thước thiệp đã xóa.
- Cập nhật giá size.
- Chuyển trạng thái kích thước thiệp.
