# STORY-058 — Admin thêm mẫu thiệp

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **Phiên bản** | v0.1 |
| **User Story** | Là một Admin có quyền quản lý mẫu thiệp, tôi muốn thêm mẫu thiệp mới để khách hàng có thêm template khi tạo thiệp. |
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

Admin có thể thêm mẫu thiệp mới vào hệ thống. Mẫu thiệp mới phải có tên mẫu và ảnh template mẫu thiệp. Thông tin mẫu thiệp được lưu vào Core Database, ảnh template/ảnh Preview được lưu vào storage.

Thông tin mẫu thiệp khi thêm mới gồm:
- **Tên mẫu** — bắt buộc, không được để trống hoặc chỉ chứa khoảng trắng, độ dài tối đa 100 ký tự.
- **Mô tả** — không bắt buộc; nếu nhập thì không được vượt quá 200 ký tự.
- **Ảnh template / Ảnh Preview** — bắt buộc; chấp nhận định dạng PNG hoặc JPG, dung lượng tối đa 10MB.

Mẫu thiệp mới sau khi tạo thành công có trạng thái mặc định là **Active** và `isDelete = false`.

Tên mẫu thiệp không yêu cầu duy nhất trong hệ thống — hệ thống cho phép tạo mẫu thiệp có tên trùng với mẫu thiệp đã tồn tại, miễn các dữ liệu còn lại hợp lệ.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý mẫu thiệp.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin chọn chức năng **thêm mẫu thiệp**.

---

## Flow

### Main Flow — Admin thêm mẫu thiệp thành công

1. Admin truy cập chức năng quản lý mẫu thiệp.
2. Admin chọn thêm mẫu thiệp.
3. Hệ thống hiển thị form thêm mẫu thiệp.
4. Admin nhập tên mẫu và upload ảnh template mẫu thiệp.
5. Admin nhập mô tả nếu có và chọn lưu.
6. Hệ thống kiểm tra dữ liệu đầu vào: độ dài tên mẫu, độ dài mô tả và file upload.
7. Hệ thống lưu thông tin mẫu thiệp vào Core Database.
8. Hệ thống lưu ảnh template/ảnh Preview vào storage.
9. Hệ thống thiết lập trạng thái mặc định là **Active** và `isDelete = false`.
10. Hệ thống thông báo thêm mẫu thiệp thành công và cập nhật danh sách mẫu thiệp.

---

### Alternative Flows

#### ALT-01 — Admin hủy thêm mẫu thiệp

1. Admin đang ở form thêm mẫu thiệp.
2. Admin chọn **"Hủy"**.
3. Hệ thống không tạo mẫu thiệp mới.
4. Hệ thống quay lại danh sách mẫu thiệp.

---

### Exception Flows

#### EXC-01 — Dữ liệu bắt buộc hoặc độ dài không hợp lệ

1. Admin không nhập tên mẫu, nhập tên mẫu chỉ chứa khoảng trắng, không upload ảnh template/ảnh Preview, nhập tên mẫu vượt quá 100 ký tự hoặc nhập mô tả vượt quá 200 ký tự.
2. Hệ thống không lưu mẫu thiệp.
3. Hệ thống hiển thị lỗi tại trường tương ứng.
4. Admin bổ sung hoặc sửa thông tin và có thể thực hiện lại.

#### EXC-02 — File ảnh không hợp lệ

1. Admin upload file không thuộc định dạng PNG, JPG hoặc vượt quá 10MB.
2. Hệ thống từ chối file và không lưu mẫu thiệp.
3. Hệ thống hiển thị lý do file không hợp lệ.
4. Admin có thể chọn file khác.

#### EXC-03 — Lưu dữ liệu thất bại

1. Admin đã nhập dữ liệu hợp lệ.
2. Hệ thống không thể lưu thông tin mẫu thiệp vào Core Database hoặc lưu ảnh template/ảnh Preview vào storage.
3. Hệ thống không tạo mẫu thiệp thành công và không hiển thị dữ liệu không đầy đủ như kết quả tạo thành công.
4. Hệ thống hiển thị thông báo lỗi để Admin thử lại.

#### EXC-04 — Admin không có quyền truy cập

1. Admin không có quyền quản lý mẫu thiệp.
2. Hệ thống từ chối truy cập chức năng thêm mẫu thiệp.
3. Hệ thống không hiển thị form thêm mẫu thiệp và không tạo dữ liệu mới.
4. Hệ thống hiển thị thông báo phù hợp về việc Admin không có quyền truy cập.

---

## Acceptance Criteria

### AC-001 — Thêm mẫu thiệp thành công
- **Given:** Admin có quyền quản lý mẫu thiệp.
- **When:** Admin lưu mẫu thiệp mới với dữ liệu hợp lệ — tên mẫu không để trống, không chỉ chứa khoảng trắng, không vượt quá 100 ký tự, và mô tả có thể bỏ trống hoặc không vượt quá 200 ký tự nếu có nhập.
- **Then:** Hệ thống lưu thông tin mẫu thiệp vào Core Database.
- **And:** Lưu ảnh template/ảnh Preview vào storage.
- **And:** Trạng thái mặc định là **Active**.
- **And:** `isDelete = false`.

### AC-002 — Validate dữ liệu bắt buộc và độ dài
- **Given:** Admin đang thêm mẫu thiệp.
- **When:** Admin không nhập tên mẫu, nhập tên mẫu chỉ chứa khoảng trắng, không upload ảnh template/ảnh Preview, nhập tên mẫu vượt quá 100 ký tự hoặc nhập mô tả vượt quá 200 ký tự.
- **Then:** Hệ thống không cho lưu mẫu thiệp.
- **And:** Hiển thị lỗi tại trường tương ứng.

### AC-003 — Validate file ảnh
- **Given:** Admin upload ảnh template/ảnh Preview.
- **When:** File không thuộc định dạng PNG, JPG hoặc vượt quá 10MB.
- **Then:** Hệ thống từ chối file và hiển thị lý do file không hợp lệ.

### AC-004 — Trạng thái mặc định mẫu thiệp mới
- **Given:** Admin thêm mẫu thiệp thành công.
- **When:** Hệ thống tạo bản ghi mẫu thiệp mới.
- **Then:** Mẫu thiệp mới phải có trạng thái **Active** và `isDelete = false`.

### AC-005 — Xử lý lỗi lưu dữ liệu — không thông báo thành công
- **Given:** Admin đã nhập dữ liệu hợp lệ.
- **When:** Hệ thống không thể lưu thông tin mẫu thiệp hoặc ảnh template/ảnh Preview.
- **Then:** Hệ thống không được thông báo thêm mẫu thiệp thành công.
- **And:** Hệ thống hiển thị thông báo lỗi để Admin thử lại.

### AC-006 — Tính nguyên tử khi thêm mẫu thiệp
- **Given:** Hệ thống đang xử lý thêm mẫu thiệp.
- **When:** Việc lưu Core Database hoặc lưu ảnh template/ảnh Preview thất bại.
- **Then:** Hệ thống không được tạo mẫu thiệp thành công.
- **And:** Hệ thống không được hiển thị dữ liệu không đầy đủ như kết quả tạo thành công.
- **And:** Admin có thể thực hiện lại thao tác sau khi lỗi được xử lý.

### AC-007 — Hủy thêm mẫu thiệp
- **Given:** Admin đang ở form thêm mẫu thiệp.
- **When:** Admin chọn **"Hủy"**.
- **Then:** Hệ thống không tạo mẫu thiệp mới.
- **And:** Hệ thống quay lại danh sách mẫu thiệp.

### AC-008 — Admin không có quyền truy cập
- **Given:** Admin không có quyền quản lý mẫu thiệp.
- **When:** Admin truy cập chức năng thêm mẫu thiệp.
- **Then:** Hệ thống từ chối truy cập và không hiển thị form thêm mẫu thiệp.
- **And:** Hệ thống không tạo dữ liệu mới.
- **And:** Hệ thống hiển thị thông báo phù hợp.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|
| **[BR-201](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/60c85a24-3b49-4aa8-87f8-a79e68b2f946)** | Thông tin bắt buộc khi thêm mẫu thiệp | Quản lý mẫu thiệp | Mẫu thiệp mới phải có đủ thông tin bắt buộc và đáp ứng giới hạn độ dài trước khi được tạo. | Admin thêm mẫu thiệp mới. | Hệ thống yêu cầu tên mẫu và ảnh template/ảnh Preview mẫu thiệp; tên mẫu không được vượt quá 100 ký tự; mô tả là không bắt buộc nhưng nếu nhập thì không được vượt quá 200 ký tự. | Nếu thiếu tên mẫu, thiếu ảnh template/ảnh Preview, tên mẫu vượt quá 100 ký tự hoặc mô tả vượt quá 200 ký tự, hệ thống không được lưu mẫu thiệp mới. | Draft | v0 | 2026-08-26 |
| **[BR-202](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d6f9389-6669-4a70-a7b4-79e18f8ddb20)** | Điều kiện file ảnh mẫu thiệp | Quản lý mẫu thiệp | Ảnh template/ảnh Preview mẫu thiệp phải đáp ứng điều kiện định dạng và dung lượng. | Admin upload ảnh template/ảnh Preview khi thêm mẫu thiệp. | Hệ thống chỉ chấp nhận file PNG hoặc JPG, dung lượng tối đa 10MB. | File không đúng định dạng hoặc vượt quá dung lượng không được chấp nhận. | Draft | v0 | 2026-08-26 |
| **[BR-203](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ef16666b-14ce-4677-be31-dc48d75f7038)** | Trạng thái mặc định của mẫu thiệp mới | Quản lý mẫu thiệp | Mẫu thiệp mới sau khi tạo thành công phải có trạng thái mặc định thống nhất. | Admin thêm mẫu thiệp mới thành công. | Hệ thống thiết lập trạng thái mẫu thiệp là Active và `isDelete = false`. | Nếu quá trình lưu thất bại, hệ thống không được tạo dữ liệu mẫu thiệp dở dang. | Draft | v0 | 2026-08-26 |
| **[BR-204](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b4a0f597-64b8-437f-802e-86d7f87089fd)** | Tính nguyên tử khi thêm mẫu thiệp | Quản lý mẫu thiệp | Việc thêm mẫu thiệp chỉ được xem là thành công khi cả thông tin mẫu thiệp và ảnh liên quan đều được lưu thành công. | Admin lưu mẫu thiệp mới. | Hệ thống lưu thông tin mẫu thiệp vào Core Database và lưu ảnh template/ảnh Preview vào storage. | Nếu một trong hai bước lưu thất bại, hệ thống không được hiển thị thêm thành công. | Draft | v0 | 2026-08-26 |
| **[BR-251](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a46e2499-1305-449d-8091-97312508ab2f)** | Cho phép trùng tên mẫu thiệp | Quản lý mẫu thiệp | Tên mẫu thiệp không yêu cầu duy nhất trong hệ thống. | Admin thêm mẫu thiệp mới. | Hệ thống cho phép tạo mẫu thiệp có tên trùng với mẫu thiệp đã tồn tại, miễn các dữ liệu còn lại hợp lệ. | N/A | Draft | v0 | 2026-08-28 |

---

## References

### Business Rules
`BR-201` · `BR-202` · `BR-203` · `BR-204` · `BR-251`

### Dependencies
- **[STORY-057](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)** — Admin xem danh sách mẫu thiệp

---

## Non-Functional Requirements

- Backend phải kiểm tra quyền Admin trước khi lưu dữ liệu.
- Backend phải kiểm tra lại định dạng và dung lượng file.
- Nếu lưu Core Database hoặc storage thất bại, hệ thống không được hiển thị thêm thành công.
- Backend phải kiểm tra tên mẫu không vượt quá 100 ký tự và mô tả không vượt quá 200 ký tự nếu có nhập.

---

## Out of Scope

- Cập nhật mẫu thiệp đã tồn tại.
- Xóa mẫu thiệp.
- Chuyển trạng thái mẫu thiệp.
- Xóa cứng ảnh template/ảnh Preview khỏi storage.
