# STORY-063 — Admin chuyển trạng thái kích thước thiệp

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **Phiên bản** | v0.1 |
| **User Story** | Là một Admin có quyền quản lý cấu hình thiệp, tôi muốn chuyển trạng thái kích thước thiệp giữa Active và Inactive để kiểm soát kích thước nào được phép sử dụng khi khách hàng tạo thiệp. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Reviewer** | Nguyễn Đức Bình |
| **Approver** | Nguyễn Đức Bình |
| **Owner** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Cập nhật** | 27/08/2026 |

---

## Context

Kích thước thiệp có trạng thái quản lý là Active hoặc Inactive.

- Kích thước thiệp **Active** và chưa bị xóa mềm được phép sử dụng trong quy trình tạo thiệp mới.
- Kích thước thiệp **Inactive** không được sử dụng khi khách hàng tạo thiệp mới nhưng vẫn được hiển thị trong màn hình quản trị.

Chuyển trạng thái kích thước thiệp không xóa kích thước, không thay đổi giá size, số lượng từ tối đa và không làm mất dữ liệu thiệp, Checkout hoặc Order đã từng sử dụng kích thước đó.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý cấu hình thiệp.
- Admin đang thao tác trên một kích thước thiệp được hiển thị trong danh sách quản lý.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin chọn thao tác **chuyển trạng thái** tại một kích thước thiệp trong danh sách.

---

## Flow

### Main Flow — Chuyển kích thước thiệp sang Inactive

1. Admin xem danh sách kích thước thiệp.
2. Hệ thống hiển thị một kích thước thiệp đang ở trạng thái **Active**.
3. Admin chọn thao tác chuyển sang **Inactive** tại kích thước thiệp đang Active.
4. Hệ thống hiển thị popup xác nhận chuyển kích thước thiệp sang Inactive.
5. Admin xác nhận thao tác.
6. Hệ thống xác định kích thước thiệp cần thay đổi trạng thái.
7. Hệ thống kiểm tra kích thước thiệp còn tồn tại và chưa bị xóa mềm.
8. Hệ thống cập nhật trạng thái kích thước thiệp từ Active sang **Inactive**.
9. Hệ thống lưu trạng thái mới vào Core Database.
10. Hệ thống thông báo cập nhật trạng thái thành công.
11. Danh sách kích thước thiệp hiển thị trạng thái mới là **Inactive**.
12. Thao tác của kích thước thiệp được thay đổi thành chuyển sang **Active**.

---

### Alternative Flows

#### ALT-01 — Chuyển kích thước thiệp sang Active

1. Admin xem danh sách kích thước thiệp.
2. Hệ thống hiển thị một kích thước thiệp đang ở trạng thái **Inactive**.
3. Admin chọn thao tác chuyển sang **Active**.
4. Hệ thống hiển thị popup xác nhận chuyển kích thước thiệp sang Active.
5. Admin xác nhận thao tác.
6. Hệ thống xác định kích thước thiệp cần thay đổi trạng thái.
7. Hệ thống kiểm tra kích thước thiệp còn tồn tại và chưa bị xóa mềm.
8. Hệ thống cập nhật trạng thái kích thước thiệp từ Inactive sang **Active**.
9. Hệ thống lưu trạng thái mới vào Core Database.
10. Hệ thống thông báo cập nhật trạng thái thành công.
11. Danh sách kích thước thiệp hiển thị trạng thái mới là **Active**.
12. Thao tác của kích thước thiệp được thay đổi thành chuyển sang **Inactive**.

#### ALT-02 — Admin hủy popup xác nhận

1. Admin chọn thao tác chuyển sang Active hoặc chuyển sang Inactive.
2. Hệ thống hiển thị popup xác nhận.
3. Admin chọn hủy hoặc đóng popup.
4. Hệ thống không cập nhật trạng thái kích thước thiệp.
5. Kích thước thiệp giữ nguyên trạng thái trước khi thao tác.
6. Danh sách kích thước thiệp không thay đổi trạng thái của kích thước đó.

---

### Exception Flows

#### EXC-01 — Kích thước thiệp không tồn tại hoặc đã bị xóa mềm

1. Admin xác nhận thao tác chuyển trạng thái một kích thước thiệp.
2. Hệ thống không tìm thấy kích thước thiệp tương ứng trong Core Database hoặc kích thước thiệp đã bị xóa mềm.
3. Hệ thống không thực hiện cập nhật trạng thái.
4. Hệ thống thông báo kích thước thiệp không còn tồn tại hoặc dữ liệu đã thay đổi.
5. Hệ thống tải lại danh sách kích thước thiệp.

#### EXC-02 — Lỗi cập nhật trạng thái

1. Admin xác nhận thao tác chuyển trạng thái kích thước thiệp.
2. Hệ thống gặp lỗi trong quá trình cập nhật.
3. Hệ thống không thay đổi trạng thái kích thước thiệp.
4. Kích thước thiệp giữ nguyên trạng thái trước khi thao tác.
5. Hệ thống thông báo cập nhật trạng thái thất bại.
6. Admin có thể thử lại.

#### EXC-03 — Không có quyền thực hiện

1. Người dùng gửi yêu cầu chuyển trạng thái kích thước thiệp.
2. Hệ thống xác định người dùng không có quyền quản lý cấu hình thiệp.
3. Hệ thống từ chối yêu cầu.
4. Hệ thống không thay đổi trạng thái kích thước thiệp.
5. Hệ thống hiển thị thông báo phù hợp.

---

## Acceptance Criteria

### AC-001 — Hiển thị popup xác nhận chuyển sang Inactive
- **Given:** Admin có quyền quản lý cấu hình thiệp và kích thước thiệp đang Active.
- **When:** Admin chọn chuyển sang Inactive.
- **Then:** Hệ thống hiển thị popup xác nhận.

### AC-002 — Chuyển sang Inactive thành công
- **Given:** Popup xác nhận chuyển sang Inactive đang hiển thị.
- **When:** Admin xác nhận.
- **Then:** Hệ thống cập nhật trạng thái kích thước thiệp thành **Inactive** trong Core Database.
- **And:** Hệ thống thông báo cập nhật thành công.
- **And:** Danh sách hiển thị trạng thái mới là **Inactive**.

### AC-003 — Hiển thị popup xác nhận chuyển sang Active
- **Given:** Admin có quyền quản lý cấu hình thiệp và kích thước thiệp đang Inactive.
- **When:** Admin chọn chuyển sang Active.
- **Then:** Hệ thống hiển thị popup xác nhận.

### AC-004 — Chuyển sang Active thành công
- **Given:** Popup xác nhận chuyển sang Active đang hiển thị.
- **When:** Admin xác nhận.
- **Then:** Hệ thống cập nhật trạng thái kích thước thiệp thành **Active** trong Core Database.
- **And:** Hệ thống thông báo cập nhật thành công.
- **And:** Danh sách hiển thị trạng thái mới là **Active**.

### AC-005 — Hủy popup xác nhận
- **Given:** Admin đang ở popup xác nhận chuyển trạng thái kích thước thiệp.
- **When:** Admin chọn hủy hoặc đóng popup.
- **Then:** Hệ thống không được cập nhật trạng thái kích thước thiệp.
- **And:** Kích thước thiệp phải giữ nguyên trạng thái trước khi thao tác.

### AC-006 — Lỗi cập nhật trạng thái
- **Given:** Admin đã xác nhận thay đổi trạng thái kích thước thiệp.
- **When:** Quá trình cập nhật thất bại.
- **Then:** Kích thước thiệp phải giữ nguyên trạng thái trước khi thao tác.
- **And:** Hệ thống phải thông báo cập nhật trạng thái thất bại.
- **And:** Admin có thể thử lại.

### AC-007 — Kích thước thiệp không tồn tại hoặc đã bị xóa mềm
- **Given:** Admin đã xác nhận thay đổi trạng thái một kích thước thiệp.
- **When:** Kích thước thiệp không còn tồn tại trong Core Database hoặc đã bị xóa mềm.
- **Then:** Hệ thống không được cập nhật trạng thái.
- **And:** Hiển thị thông báo dữ liệu không còn tồn tại hoặc đã thay đổi.
- **And:** Hệ thống tải lại danh sách kích thước thiệp.

### AC-008 — Không có quyền thực hiện
- **Given:** Người dùng không có quyền quản lý cấu hình thiệp.
- **When:** Người dùng gửi yêu cầu chuyển trạng thái kích thước thiệp.
- **Then:** Hệ thống phải từ chối yêu cầu.
- **And:** Không được thay đổi trạng thái kích thước thiệp.
- **And:** Hệ thống phải hiển thị thông báo phù hợp.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|
| **[BR-223](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/07f5b569-3136-44b8-9a6b-4f06f536c384)** | Trạng thái quản lý của kích thước thiệp | Quản lý cấu hình thiệp | Mỗi kích thước thiệp chỉ được có một trạng thái quản lý tại một thời điểm. | Kích thước thiệp tồn tại trong hệ thống. | Kích thước thiệp phải ở một trong hai trạng thái Active hoặc Inactive. | Không cho phép một kích thước thiệp đồng thời ở cả trạng thái Active và Inactive. | Draft | v0 | 2026-08-27 |
| **[BR-224](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b43fa5ea-94a1-46d9-9114-038e90875cd1)** | Ảnh hưởng của kích thước thiệp Active | Quản lý cấu hình thiệp | Kích thước thiệp Active được phép sử dụng khi khách hàng tạo thiệp mới nếu chưa bị xóa mềm. | Khách hàng tải hoặc sử dụng danh sách kích thước thiệp khả dụng trong quy trình tạo thiệp. | Hệ thống chỉ xem kích thước thiệp có `isActive = true` và `isDelete = false` là kích thước khả dụng. | Kích thước Active nhưng `isDelete = true` không được sử dụng khi tạo thiệp mới. | Draft | v0 | 2026-08-27 |
| **[BR-225](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/98255c98-7f84-4115-be4a-69fcb75f6c1c)** | Ảnh hưởng của kích thước thiệp Inactive | Quản lý cấu hình thiệp | Kích thước thiệp Inactive không được sử dụng khi khách hàng tạo thiệp mới. | Khách hàng tải hoặc sử dụng danh sách kích thước thiệp khả dụng trong quy trình tạo thiệp. | Hệ thống loại bỏ kích thước thiệp Inactive khỏi danh sách kích thước khách hàng có thể chọn khi tạo thiệp mới. | Kích thước thiệp Inactive vẫn được giữ trong Core Database và vẫn hiển thị trong màn hình quản trị nếu chưa bị xóa mềm. | Draft | v0 | 2026-08-27 |
| **[BR-226](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6d68f5ff-d27c-4993-a2de-066cc2204208)** | Chuyển trạng thái kích thước thiệp không làm thay đổi dữ liệu cấu hình | Quản lý cấu hình thiệp | Chuyển trạng thái kích thước thiệp không được làm thay đổi dữ liệu cấu hình khác hoặc dữ liệu lịch sử liên quan. | Admin chuyển trạng thái kích thước thiệp giữa Active và Inactive. | Hệ thống chỉ cập nhật trạng thái kích thước thiệp trong Core Database. | Hệ thống không được xóa kích thước, không thay đổi giá size, số lượng từ tối đa và không làm mất dữ liệu thiệp, Checkout hoặc Order đã từng sử dụng kích thước đó. | Draft | v0 | 2026-08-27 |
| **[BR-227](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2411f028-caea-431a-abbc-eefe9a7a578e)** | Quyền chuyển trạng thái kích thước thiệp | Phân quyền | Chỉ Admin có quyền quản lý cấu hình thiệp mới được chuyển trạng thái kích thước thiệp. | Người dùng yêu cầu chuyển trạng thái kích thước thiệp. | Hệ thống kiểm tra quyền trước khi cập nhật trạng thái. | Người dùng không có quyền quản lý cấu hình thiệp không được cập nhật trạng thái kích thước thiệp. | Draft | v0 | 2026-08-27 |

---

## References

### Business Rules
`BR-223` · `BR-224` · `BR-225` · `BR-226` · `BR-227`

### Dependencies
- **[STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)** — Tạo thiệp cá nhân hóa bằng AI tại Checkout
- **[STORY-054](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)** — Admin xem danh sách kích thước thiệp

---

## Non-Functional Requirements

- Backend phải kiểm tra quyền Admin trước khi cập nhật trạng thái.
- Backend phải kiểm tra lại kích thước thiệp còn tồn tại và chưa bị xóa mềm trước khi cập nhật.
- Mỗi yêu cầu chỉ được áp dụng cho đúng kích thước thiệp được chỉ định.
- Lỗi cập nhật không được lưu trạng thái dở dang hoặc không đồng nhất.
- Trạng thái trong Core Database là nguồn xác thực cuối cùng.

---

## Out of Scope

- Thêm kích thước thiệp.
- Xóa kích thước thiệp.
- Cập nhật kích thước.
- Cập nhật giá size.
- Cập nhật số lượng từ tối đa.
- Thay đổi kích thước của các thiệp, Checkout hoặc Order đã tồn tại.
