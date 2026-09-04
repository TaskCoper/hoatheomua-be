# STORY-064 — Admin chuyển trạng thái cấu hình phụ phí thiệp viết tay

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý cấu hình giá thiệp, tôi muốn chuyển trạng thái cấu hình phụ phí viết tay giữa Active và Inactive để kiểm soát cấu hình nào được sử dụng khi hệ thống tính phụ phí cho thiệp Calligraphy. |
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

Cấu hình phụ phí viết tay có trạng thái quản lý là Active hoặc Inactive. Cấu hình Active và chưa bị xóa mềm được phép sử dụng khi hệ thống xác định phụ phí viết tay cho thiệp Calligraphy. Cấu hình Inactive không được sử dụng để xác định phụ phí cho thiệp Calligraphy mới nhưng vẫn được hiển thị trong màn hình quản trị.

Chuyển trạng thái cấu hình phụ phí viết tay không xóa cấu hình, không thay đổi khoảng số lượng từ, giá phụ phí và không làm thay đổi dữ liệu thiệp, Checkout hoặc Order đã tồn tại.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang thao tác trên một cấu hình phụ phí viết tay được hiển thị trong danh sách quản lý.

### Trigger
> Admin chọn thao tác chuyển trạng thái tại một cấu hình phụ phí viết tay trong danh sách.

---

## Flow

### Main Flow: Chuyển cấu hình phụ phí viết tay sang Inactive

1. Admin xem danh sách cấu hình phụ phí viết tay.
2. Hệ thống hiển thị một cấu hình đang ở trạng thái Active.
3. Admin chọn thao tác chuyển sang Inactive tại cấu hình đang Active.
4. Hệ thống hiển thị popup xác nhận chuyển cấu hình sang Inactive.
5. Admin xác nhận thao tác.
6. Hệ thống xác định cấu hình phụ phí viết tay cần thay đổi trạng thái.
7. Hệ thống kiểm tra cấu hình còn tồn tại và chưa bị xóa mềm.
8. Hệ thống cập nhật trạng thái cấu hình từ Active sang Inactive.
9. Hệ thống lưu trạng thái mới vào Core Database.
10. Hệ thống thông báo cập nhật trạng thái thành công.
11. Danh sách cấu hình phụ phí viết tay hiển thị trạng thái mới là Inactive.
12. Thao tác của cấu hình được thay đổi thành chuyển sang Active.

---

### Alternative Flows

#### ALT-01 — Chuyển cấu hình phụ phí viết tay sang Active
1. Admin xem danh sách cấu hình phụ phí viết tay.
2. Hệ thống hiển thị một cấu hình đang ở trạng thái Inactive.
3. Admin chọn thao tác chuyển sang Active tại cấu hình đang Inactive.
4. Hệ thống hiển thị popup xác nhận chuyển cấu hình sang Active.
5. Admin xác nhận thao tác.
6. Hệ thống xác định cấu hình phụ phí viết tay cần thay đổi trạng thái.
7. Hệ thống kiểm tra cấu hình còn tồn tại và chưa bị xóa mềm.
8. Hệ thống kiểm tra khoảng số lượng từ của cấu hình không chồng lấn với các cấu hình Active khác có `isDelete = false`.
9. Hệ thống cập nhật trạng thái cấu hình từ Inactive sang Active.
10. Hệ thống lưu trạng thái mới vào Core Database.
11. Hệ thống thông báo cập nhật trạng thái thành công.
12. Danh sách cấu hình phụ phí viết tay hiển thị trạng thái mới là Active.
13. Thao tác của cấu hình được thay đổi thành chuyển sang Inactive.

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

### AC-001 — Hiển thị popup Inactive
- **Given:** Admin có quyền quản lý cấu hình giá thiệp và cấu hình phụ phí viết tay đang Active.
- **When:** Admin chọn chuyển sang Inactive.
- **Then:** hệ thống hiển thị popup xác nhận.

### AC-002 — Xác nhận Inactive
- **Given:** popup xác nhận chuyển sang Inactive đang hiển thị.
- **When:** Admin xác nhận.
- **Then:** hệ thống cập nhật trạng thái cấu hình phụ phí viết tay thành Inactive trong Core Database.
- **And:** hệ thống thông báo cập nhật thành công.
- **And:** danh sách hiển thị trạng thái mới là Inactive.

### AC-003 — Hiển thị popup Active
- **Given:** Admin có quyền quản lý cấu hình giá thiệp và cấu hình phụ phí viết tay đang Inactive.
- **When:** Admin chọn chuyển sang Active.
- **Then:** hệ thống hiển thị popup xác nhận.

### AC-004 — Xác nhận Active
- **Given:** popup xác nhận chuyển sang Active đang hiển thị.
- **When:** Admin xác nhận.
- **Then:** hệ thống cập nhật trạng thái cấu hình phụ phí viết tay thành Active trong Core Database.
- **And:** hệ thống thông báo cập nhật thành công.
- **And:** danh sách hiển thị trạng thái mới là Active.

### AC-005 — Hủy thay đổi trạng thái
- **Given:** Admin đang ở popup xác nhận chuyển trạng thái cấu hình phụ phí viết tay.
- **When:** Admin chọn hủy hoặc đóng popup.
- **Then:** Hệ thống không được cập nhật trạng thái cấu hình.
- **And:** cấu hình phải giữ nguyên trạng thái trước khi thao tác.

### AC-006 — Xử lý lỗi cập nhật
- **Given:** Admin đã xác nhận thay đổi trạng thái cấu hình phụ phí viết tay.
- **When:** Quá trình cập nhật thất bại.
- **Then:** cấu hình phải giữ nguyên trạng thái trước khi thao tác.
- **And:** Hệ thống phải thông báo cập nhật trạng thái thất bại.
- **And:** Admin có thể thử lại.

### AC-007 — Cấu hình không tồn tại
- **Given:** Admin đã xác nhận thay đổi trạng thái một cấu hình phụ phí viết tay.
- **When:** cấu hình không còn tồn tại trong Core Database hoặc đã bị xóa mềm.
- **Then:** Hệ thống không được cập nhật trạng thái.
- **And:** hệ thống hiển thị thông báo dữ liệu không còn tồn tại hoặc đã thay đổi.
- **And:** hệ thống tải lại danh sách cấu hình phụ phí viết tay.

### AC-008 — Kiểm tra quyền
- **Given:** người dùng không có quyền quản lý cấu hình giá thiệp.
- **When:** người dùng gửi yêu cầu chuyển trạng thái cấu hình phụ phí viết tay.
- **Then:** Hệ thống phải từ chối yêu cầu.
- **And:** không được thay đổi trạng thái cấu hình.
- **And:** Hệ thống phải hiển thị thông báo phù hợp.

### AC-009 — Không cho Active khi khoảng số lượng từ chồng lấn
- **Given:** cấu hình phụ phí viết tay đang Inactive.
- **When:** Admin chuyển sang Active và khoảng số lượng từ chồng lấn với cấu hình Active khác có `isDelete = false`.
- **Then:** hệ thống không được chuyển cấu hình sang Active.
- **And:** hệ thống hiển thị thông báo khoảng số lượng từ đang chồng lấn với cấu hình khác.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-218**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2ee2a41e-61c8-445b-8c67-b63d24ebcd61) | Hiển thị trạng thái cấu hình phụ phí viết tay | Dữ liệu thiệp | Mỗi cấu hình chưa bị xóa mềm phải có trạng thái Active hoặc Inactive. | Danh sách cấu hình được hiển thị. | Hệ thống hiển thị trạng thái hiện tại và thao tác chuyển trạng thái tương ứng. | N/A | Admin có quyền quản lý cấu hình giá thiệp | STORY-035 | Draft | v0 | 2026-08-26 |
| [**BR-228**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/65ec4904-0028-4e84-8372-0867d48c6d87) | Ảnh hưởng của cấu hình phụ phí viết tay Active | Cấu hình giá thiệp | Cấu hình Active được phép sử dụng để xác định phụ phí cho thiệp Calligraphy nếu chưa bị xóa mềm. | Hệ thống xác định phụ phí viết tay cho thiệp Calligraphy mới. | Hệ thống chỉ sử dụng cấu hình có trạng thái Active và isDelete = false phù hợp với số lượng từ của thiệp. | Cấu hình Active nhưng isDelete = true không được sử dụng. | Admin có quyền quản lý cấu hình giá thiệp | STORY-061, STORY-035 | Draft | v0 | 2026-08-27 |
| [**BR-229**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bf136312-8a7a-434d-aedc-9278cc6797b2) | Ảnh hưởng của cấu hình phụ phí viết tay Inactive | Cấu hình giá thiệp | Cấu hình Inactive không được sử dụng để xác định phụ phí cho thiệp Calligraphy mới. | Hệ thống xác định phụ phí viết tay cho thiệp Calligraphy mới. | Hệ thống không sử dụng cấu hình có trạng thái Inactive để xác định phụ phí. | Cấu hình Inactive vẫn giữ trong Core Database và hiển thị quản trị. | Admin có quyền quản lý cấu hình giá thiệp | STORY-061, STORY-035 | Draft | v0 | 2026-08-27 |
| [**BR-230**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8bf3ccbf-1d1f-4e06-a4bf-d24711481906) | Chuyển trạng thái cấu hình không thay đổi dữ liệu | Quản lý cấu hình giá thiệp | Chuyển trạng thái cấu hình phụ phí viết tay không được làm mất hoặc thay đổi các dữ liệu cấu hình khác. | Admin chuyển trạng thái cấu hình giữa Active và Inactive. | Hệ thống chỉ cập nhật trạng thái của cấu hình trong Core Database, không tự động thay đổi khoảng số từ/giá/Order. | N/A | Admin có quyền quản lý cấu hình giá thiệp | STORY-061, STORY-035 | Draft | v0 | 2026-08-27 |
| [**BR-231**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bd4b2186-6257-48c2-9bea-a36b84ebdea1) | Quyền chuyển trạng thái cấu hình phụ phí viết tay | Phân quyền | Chỉ Admin có quyền quản lý cấu hình giá thiệp mới được chuyển trạng thái cấu hình phụ phí viết tay. | Người dùng gửi yêu cầu chuyển trạng thái. | Hệ thống phải kiểm tra quyền trước khi cập nhật. | Người dùng không có quyền bị từ chối cập nhật. | Admin có quyền quản lý cấu hình giá thiệp | STORY-061, STORY-035 | Draft | v0 | 2026-08-27 |
| [**BR-239**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde) | Không chồng lấn khoảng số lượng từ Active | Cấu hình giá thiệp | Hai cấu hình phụ phí Active và chưa bị xóa mềm không được có khoảng số lượng từ chồng lấn nhau. | Admin tạo cấu hình Active hoặc cập nhật khoảng của một cấu hình Active. | Hệ thống kiểm tra khoảng mới với các cấu hình Active khác có isDelete = false trước khi lưu. | Cấu hình Inactive không tham gia kiểm tra chồng lấn cho đến khi Active. | Admin có quyền quản lý cấu hình giá thiệp | STORY-067, STORY-064, STORY-068 | Draft | v0 | 2026-08-27 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-218 | [Hiển thị trạng thái cấu hình phụ phí viết tay](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2ee2a41e-61c8-445b-8c67-b63d24ebcd61) |
| BR-228 | [Ảnh hưởng của cấu hình phụ phí viết tay Active](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/65ec4904-0028-4e84-8372-0867d48c6d87) |
| BR-229 | [Ảnh hưởng của cấu hình phụ phí viết tay Inactive](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bf136312-8a7a-434d-aedc-9278cc6797b2) |
| BR-230 | [Chuyển trạng thái cấu hình phụ phí viết tay không làm thay đổi dữ liệu cấu hình](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8bf3ccbf-1d1f-4e06-a4bf-d24711481906) |
| BR-231 | [Quyền chuyển trạng thái cấu hình phụ phí viết tay](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bd4b2186-6257-48c2-9bea-a36b84ebdea1) |
| BR-239 | [Không chồng lấn khoảng số lượng từ Active](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde) |

### Dependencies
- [**STORY-035**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [**STORY-061**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)

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
