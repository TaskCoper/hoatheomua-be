# STORY-064 — Admin chuyển trạng thái cấu hình phụ phí thiệp viết tay

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **Phiên bản** | v0.1 |
| **User Story** | Là một Admin có quyền quản lý cấu hình giá thiệp, tôi muốn chuyển trạng thái cấu hình phụ phí viết tay giữa Active và Inactive để kiểm soát cấu hình nào được sử dụng khi hệ thống tính phụ phí cho thiệp Calligraphy. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Reviewer** | Nguyễn Đức Bình |
| **Approver** | Nguyễn Đức Bình |
| **Owner** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Cập nhật** | 28/08/2026 |

---

## Context

Cấu hình phụ phí viết tay có trạng thái quản lý là Active hoặc Inactive.

- Cấu hình **Active** và chưa bị xóa mềm được phép sử dụng khi hệ thống xác định phụ phí viết tay cho thiệp Calligraphy.
- Cấu hình **Inactive** không được sử dụng để xác định phụ phí cho thiệp Calligraphy mới nhưng vẫn được hiển thị trong màn hình quản trị.

Chuyển trạng thái cấu hình phụ phí viết tay không xóa cấu hình, không thay đổi khoảng số lượng từ, giá phụ phí và không làm thay đổi dữ liệu thiệp, Checkout hoặc Order đã tồn tại.

Khi chuyển cấu hình từ Inactive sang Active, hệ thống phải kiểm tra khoảng số lượng từ không chồng lấn với các cấu hình Active khác có `isDelete = false`.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang thao tác trên một cấu hình phụ phí viết tay được hiển thị trong danh sách quản lý.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin chọn thao tác **chuyển trạng thái** tại một cấu hình phụ phí viết tay trong danh sách.

---

## Flow

### Main Flow — Chuyển cấu hình phụ phí viết tay sang Inactive

1. Admin xem danh sách cấu hình phụ phí viết tay.
2. Hệ thống hiển thị một cấu hình đang ở trạng thái **Active**.
3. Admin chọn thao tác chuyển sang **Inactive** tại cấu hình đang Active.
4. Hệ thống hiển thị popup xác nhận chuyển cấu hình sang Inactive.
5. Admin xác nhận thao tác.
6. Hệ thống xác định cấu hình phụ phí viết tay cần thay đổi trạng thái.
7. Hệ thống kiểm tra cấu hình còn tồn tại và chưa bị xóa mềm.
8. Hệ thống cập nhật trạng thái cấu hình từ Active sang **Inactive**.
9. Hệ thống lưu trạng thái mới vào Core Database.
10. Hệ thống thông báo cập nhật trạng thái thành công.
11. Danh sách cấu hình phụ phí viết tay hiển thị trạng thái mới là **Inactive**.
12. Thao tác của cấu hình được thay đổi thành chuyển sang **Active**.

---

### Alternative Flows

#### ALT-01 — Chuyển cấu hình phụ phí viết tay sang Active

1. Admin xem danh sách cấu hình phụ phí viết tay.
2. Hệ thống hiển thị một cấu hình đang ở trạng thái **Inactive**.
3. Admin chọn thao tác chuyển sang **Active** tại cấu hình đang Inactive.
4. Hệ thống hiển thị popup xác nhận chuyển cấu hình sang Active.
5. Admin xác nhận thao tác.
6. Hệ thống xác định cấu hình phụ phí viết tay cần thay đổi trạng thái.
7. Hệ thống kiểm tra cấu hình còn tồn tại và chưa bị xóa mềm.
8. Hệ thống kiểm tra khoảng số lượng từ của cấu hình không chồng lấn với các cấu hình Active khác có `isDelete = false`.
9. Hệ thống cập nhật trạng thái cấu hình từ Inactive sang **Active**.
10. Hệ thống lưu trạng thái mới vào Core Database.
11. Hệ thống thông báo cập nhật trạng thái thành công.
12. Danh sách cấu hình phụ phí viết tay hiển thị trạng thái mới là **Active**.
13. Thao tác của cấu hình được thay đổi thành chuyển sang **Inactive**.

#### ALT-02 — Admin hủy popup xác nhận

1. Admin chọn thao tác chuyển sang Active hoặc chuyển sang Inactive.
2. Hệ thống hiển thị popup xác nhận.
3. Admin chọn hủy hoặc đóng popup.
4. Hệ thống không cập nhật trạng thái cấu hình phụ phí viết tay.
5. Cấu hình giữ nguyên trạng thái trước khi thao tác.
6. Danh sách không thay đổi trạng thái của cấu hình đó.

---

### Exception Flows

#### EXC-01 — Cấu hình không tồn tại hoặc đã bị xóa mềm

1. Admin xác nhận thao tác chuyển trạng thái một cấu hình phụ phí viết tay.
2. Hệ thống không tìm thấy cấu hình tương ứng trong Core Database hoặc cấu hình đã bị xóa mềm.
3. Hệ thống không thực hiện cập nhật trạng thái.
4. Hệ thống thông báo cấu hình không còn tồn tại hoặc dữ liệu đã thay đổi.
5. Hệ thống tải lại danh sách cấu hình phụ phí viết tay.

#### EXC-02 — Lỗi cập nhật trạng thái

1. Admin xác nhận thao tác chuyển trạng thái cấu hình phụ phí viết tay.
2. Hệ thống gặp lỗi trong quá trình cập nhật.
3. Hệ thống không thay đổi trạng thái cấu hình.
4. Cấu hình giữ nguyên trạng thái trước khi thao tác.
5. Hệ thống thông báo cập nhật trạng thái thất bại.
6. Admin có thể thử lại.

#### EXC-03 — Không có quyền thực hiện

1. Người dùng gửi yêu cầu chuyển trạng thái cấu hình phụ phí viết tay.
2. Hệ thống xác định người dùng không có quyền quản lý cấu hình giá thiệp.
3. Hệ thống từ chối yêu cầu.
4. Hệ thống không thay đổi trạng thái cấu hình.
5. Hệ thống hiển thị thông báo phù hợp.

#### EXC-04 — Khoảng số lượng từ bị chồng lấn khi chuyển sang Active

1. Admin xác nhận chuyển cấu hình từ Inactive sang Active.
2. Khoảng số lượng từ của cấu hình chồng lấn với một cấu hình Active khác có `isDelete = false`.
3. Hệ thống không cập nhật trạng thái.
4. Hệ thống thông báo khoảng số lượng từ đang chồng lấn với cấu hình khác.

---

## Acceptance Criteria

### AC-001 — Hiển thị popup xác nhận chuyển sang Inactive
- **Given:** Admin có quyền quản lý cấu hình giá thiệp và cấu hình phụ phí viết tay đang Active.
- **When:** Admin chọn chuyển sang Inactive.
- **Then:** Hệ thống hiển thị popup xác nhận.

### AC-002 — Chuyển sang Inactive thành công
- **Given:** Popup xác nhận chuyển sang Inactive đang hiển thị.
- **When:** Admin xác nhận.
- **Then:** Hệ thống cập nhật trạng thái cấu hình phụ phí viết tay thành **Inactive** trong Core Database.
- **And:** Hệ thống thông báo cập nhật thành công.
- **And:** Danh sách hiển thị trạng thái mới là **Inactive**.

### AC-003 — Hiển thị popup xác nhận chuyển sang Active
- **Given:** Admin có quyền quản lý cấu hình giá thiệp và cấu hình phụ phí viết tay đang Inactive.
- **When:** Admin chọn chuyển sang Active.
- **Then:** Hệ thống hiển thị popup xác nhận.

### AC-004 — Chuyển sang Active thành công
- **Given:** Popup xác nhận chuyển sang Active đang hiển thị.
- **When:** Admin xác nhận.
- **Then:** Hệ thống cập nhật trạng thái cấu hình phụ phí viết tay thành **Active** trong Core Database.
- **And:** Hệ thống thông báo cập nhật thành công.
- **And:** Danh sách hiển thị trạng thái mới là **Active**.

### AC-005 — Hủy popup xác nhận
- **Given:** Admin đang ở popup xác nhận chuyển trạng thái cấu hình phụ phí viết tay.
- **When:** Admin chọn hủy hoặc đóng popup.
- **Then:** Hệ thống không được cập nhật trạng thái cấu hình.
- **And:** Cấu hình phải giữ nguyên trạng thái trước khi thao tác.

### AC-006 — Lỗi cập nhật trạng thái
- **Given:** Admin đã xác nhận thay đổi trạng thái cấu hình phụ phí viết tay.
- **When:** Quá trình cập nhật thất bại.
- **Then:** Cấu hình phải giữ nguyên trạng thái trước khi thao tác.
- **And:** Hệ thống phải thông báo cập nhật trạng thái thất bại.
- **And:** Admin có thể thử lại.

### AC-007 — Cấu hình không tồn tại hoặc đã bị xóa mềm
- **Given:** Admin đã xác nhận thay đổi trạng thái một cấu hình phụ phí viết tay.
- **When:** Cấu hình không còn tồn tại trong Core Database hoặc đã bị xóa mềm.
- **Then:** Hệ thống không được cập nhật trạng thái.
- **And:** Hệ thống hiển thị thông báo dữ liệu không còn tồn tại hoặc đã thay đổi.
- **And:** Hệ thống tải lại danh sách cấu hình phụ phí viết tay.

### AC-008 — Không có quyền thực hiện
- **Given:** Người dùng không có quyền quản lý cấu hình giá thiệp.
- **When:** Người dùng gửi yêu cầu chuyển trạng thái cấu hình phụ phí viết tay.
- **Then:** Hệ thống phải từ chối yêu cầu.
- **And:** Không được thay đổi trạng thái cấu hình.
- **And:** Hệ thống phải hiển thị thông báo phù hợp.

### AC-009 — Không cho Active khi khoảng số lượng từ chồng lấn
- **Given:** Cấu hình phụ phí viết tay đang Inactive.
- **When:** Admin chuyển sang Active và khoảng số lượng từ chồng lấn với cấu hình Active khác có `isDelete = false`.
- **Then:** Hệ thống không được chuyển cấu hình sang Active.
- **And:** Hệ thống hiển thị thông báo khoảng số lượng từ đang chồng lấn với cấu hình khác.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|
| **[BR-218](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2ee2a41e-61c8-445b-8c67-b63d24ebcd61)** | Hiển thị trạng thái cấu hình phụ phí viết tay | Dữ liệu thiệp | Mỗi cấu hình chưa bị xóa mềm phải có trạng thái Active hoặc Inactive. | Danh sách cấu hình được hiển thị. | Hệ thống hiển thị trạng thái hiện tại và thao tác chuyển trạng thái tương ứng. | N/A | Draft | v0 | 2026-08-26 |
| **[BR-228](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/65ec4904-0028-4e84-8372-0867d48c6d87)** | Ảnh hưởng của cấu hình phụ phí viết tay Active | Cấu hình giá thiệp | Cấu hình phụ phí viết tay Active được phép sử dụng để xác định phụ phí cho thiệp Calligraphy nếu chưa bị xóa mềm. | Hệ thống xác định phụ phí viết tay cho một thiệp Calligraphy mới. | Hệ thống chỉ được sử dụng cấu hình có trạng thái Active và `isDelete = false` phù hợp với số lượng từ của thiệp. | Cấu hình Active nhưng `isDelete = true` không được sử dụng để xác định phụ phí. | Draft | v0 | 2026-08-27 |
| **[BR-229](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bf136312-8a7a-434d-aedc-9278cc6797b2)** | Ảnh hưởng của cấu hình phụ phí viết tay Inactive | Cấu hình giá thiệp | Cấu hình phụ phí viết tay Inactive không được sử dụng để xác định phụ phí cho thiệp Calligraphy mới. | Hệ thống xác định phụ phí viết tay cho một thiệp Calligraphy mới. | Hệ thống không sử dụng cấu hình có trạng thái Inactive để xác định phụ phí. | Cấu hình Inactive vẫn được giữ trong Core Database và có thể hiển thị trong màn hình quản trị. | Draft | v0 | 2026-08-27 |
| **[BR-230](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8bf3ccbf-1d1f-4e06-a4bf-d24711481906)** | Chuyển trạng thái cấu hình phụ phí viết tay không làm thay đổi dữ liệu cấu hình | Quản lý cấu hình giá thiệp | Chuyển trạng thái cấu hình phụ phí viết tay không được làm mất hoặc thay đổi các dữ liệu cấu hình khác. | Admin chuyển trạng thái cấu hình giữa Active và Inactive. | Hệ thống chỉ cập nhật trạng thái của cấu hình phụ phí viết tay trong Core Database. | Hệ thống không được tự động thay đổi số lượng từ bắt đầu, số lượng từ kết thúc, giá phụ phí hoặc làm thay đổi dữ liệu thiệp, Checkout, Order đã tồn tại. | Draft | v0 | 2026-08-27 |
| **[BR-231](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bd4b2186-6257-48c2-9bea-a36b84ebdea1)** | Quyền chuyển trạng thái cấu hình phụ phí viết tay | Phân quyền | Chỉ Admin có quyền quản lý cấu hình giá thiệp mới được chuyển trạng thái cấu hình phụ phí viết tay. | Người dùng gửi yêu cầu chuyển trạng thái cấu hình phụ phí viết tay. | Hệ thống phải kiểm tra quyền trước khi cập nhật trạng thái. | Người dùng không có quyền không được cập nhật trạng thái cấu hình phụ phí viết tay. | Draft | v0 | 2026-08-27 |
| **[BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)** | Không chồng lấn khoảng số lượng từ Active | Cấu hình giá thiệp | Hai cấu hình phụ phí Active và chưa bị xóa mềm không được có khoảng số lượng từ chồng lấn nhau. | Admin tạo cấu hình ở trạng thái Active hoặc cập nhật khoảng số lượng từ của một cấu hình Active. | Hệ thống kiểm tra khoảng mới với các cấu hình Active khác có `isDelete = false` trước khi lưu; khi cập nhật, phải loại trừ chính bản ghi đang được cập nhật. | Cấu hình Inactive không tham gia kiểm tra chồng lấn cho đến khi được chuyển sang Active. | Draft | v0 | 2026-08-27 |

---

## References

### Business Rules
`BR-218` · `BR-228` · `BR-229` · `BR-230` · `BR-231` · `BR-239`

### Dependencies
- **[STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)** — Tạo thiệp cá nhân hóa bằng AI tại Checkout
- **[STORY-061](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)** — Admin xem danh sách cấu hình phụ phí viết tay

---

## Non-Functional Requirements

- Backend phải kiểm tra quyền Admin trước khi cập nhật trạng thái.
- Backend phải kiểm tra lại cấu hình phụ phí viết tay còn tồn tại và chưa bị xóa mềm trước khi cập nhật.
- Mỗi yêu cầu chỉ được áp dụng cho đúng cấu hình phụ phí viết tay được chỉ định.
- Lỗi cập nhật không được lưu trạng thái dở dang hoặc không đồng nhất.
- Trạng thái trong Core Database là nguồn xác thực cuối cùng.

---

## Out of Scope

- Thêm cấu hình phụ phí viết tay.
- Cập nhật khoảng số lượng từ.
- Cập nhật giá phụ phí.
- Xóa cấu hình phụ phí viết tay.
- Thay đổi cấu hình đã được áp dụng cho thiệp, Checkout hoặc Order đã tồn tại.
- Tính giá cuối cùng của thiệp, Checkout hoặc Order.
