# STORY-058 — Admin thêm mẫu thiệp

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý mẫu thiệp, tôi muốn thêm mẫu thiệp mới để khách hàng có thêm template khi tạo thiệp. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Đã duyệt |

> **Feedback yêu cầu sửa gần nhất:**
> "Chưa thấy có rule có cho phép đặt trùng tên hay không. Chưa có EXC cho việc không có quyền. Khi bổ sung, cần bổ sung kèm AC. Chưa có Alt-flow cho flow Admin đang tạo giữa chừng thì huỷ." — *Nguyễn Đức Bình · 21:33 27/08/2026*
> (Đã được khắc phục trong phiên bản hiện tại bằng BR-251, EXC-04, AC-008, AC-007 và ALT-01)

---

## Context

Admin có thể thêm mẫu thiệp mới vào hệ thống. Mẫu thiệp mới phải có tên mẫu và ảnh template mẫu thiệp.
- Tên mẫu không được để trống hoặc chỉ chứa khoảng trắng. Tên mẫu không được vượt quá 100 ký tự.
- Mô tả là không bắt buộc; nếu nhập thì không được vượt quá 200 ký tự.

Thông tin mẫu thiệp được lưu vào Core Database. Ảnh template/ảnh Preview được lưu vào storage. Mẫu thiệp mới sau khi tạo thành công có trạng thái mặc định là Active và isDelete = false. Chấp nhận ảnh dạng PNG hoặc JPG.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý mẫu thiệp.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin chọn chức năng thêm mẫu thiệp.

---

## Flow

### Main Flow: Admin thêm mẫu thiệp thành công

1. Admin truy cập chức năng quản lý mẫu thiệp.
2. Admin chọn thêm mẫu thiệp.
3. Hệ thống hiển thị form thêm mẫu thiệp.
4. Admin nhập tên mẫu và upload ảnh template mẫu thiệp.
5. Admin nhập mô tả nếu có và chọn lưu.
6. Hệ thống kiểm tra dữ liệu đầu vào, độ dài tên mẫu/mô tả và file upload.
7. Hệ thống lưu thông tin mẫu thiệp vào Core Database.
8. Hệ thống lưu ảnh template/ảnh Preview vào storage.
9. Hệ thống thiết lập trạng thái mặc định là Active và isDelete = false.
10. Hệ thống thông báo thêm mẫu thiệp thành công và cập nhật danh sách mẫu thiệp.

---

### Alternative Flows

#### ALT-01 — Admin hủy thêm mẫu thiệp
1. Admin đang ở form thêm mẫu thiệp.
2. Admin chọn “Hủy”.
3. Hệ thống không tạo mẫu thiệp mới.
4. Hệ thống quay lại danh sách mẫu thiệp.

---

### Exception Flows

#### EXC-01 — Dữ liệu bắt buộc hoặc độ dài không hợp lệ
1. Admin không nhập tên mẫu, không upload ảnh template/ảnh Preview, nhập tên mẫu vượt quá 100 ký tự hoặc nhập mô tả vượt quá 200 ký tự.
2. Hệ thống không lưu mẫu thiệp.
3. Hệ thống hiển thị lỗi tại trường tương ứng.

#### EXC-02 — File ảnh không hợp lệ
1. Admin upload file không thuộc định dạng PNG, JPG hoặc vượt quá 10MB.
2. Hệ thống từ chối file và không lưu mẫu thiệp.
3. Hệ thống hiển thị lý do file không hợp lệ và cho phép Admin chọn file khác.

#### EXC-03 — Lưu dữ liệu thất bại
1. Admin đã nhập dữ liệu hợp lệ.
2. Hệ thống không thể lưu thông tin mẫu thiệp hoặc ảnh template/ảnh Preview.
3. Hệ thống không tạo mẫu thiệp thành công và hiển thị thông báo lỗi để Admin thử lại.

#### EXC-04 — Admin không có quyền truy cập
1. Admin không có quyền quản lý mẫu thiệp.
2. Hệ thống từ chối truy cập chức năng thêm mẫu thiệp.
3. Hệ thống không hiển thị form thêm mẫu thiệp và không tạo dữ liệu mới.
4. Hệ thống hiển thị thông báo phù hợp về việc Admin không có quyền truy cập.

---

## Acceptance Criteria

### AC-001 — Lưu mẫu thiệp thành công
- **Given:** Admin có quyền quản lý mẫu thiệp.
- **When:** Admin lưu mẫu thiệp mới với dữ liệu hợp lệ, tên mẫu không để trống/không chỉ chứa khoảng trắng/không vượt quá 100 ký tự, và mô tả có thể bỏ trống hoặc không vượt quá 200 ký tự nếu có nhập.
- **Then:** hệ thống lưu thông tin mẫu thiệp vào Core Database.
- **And:** lưu ảnh template/ảnh Preview vào storage.
- **And:** trạng thái mặc định là Active.
- **And:** isDelete = false.

### AC-002 — Báo lỗi dữ liệu không hợp lệ
- **Given:** Admin đang thêm mẫu thiệp.
- **When:** Admin không nhập tên mẫu, nhập tên mẫu chỉ chứa khoảng trắng, không upload ảnh template/ảnh Preview, nhập tên mẫu vượt quá 100 ký tự hoặc nhập mô tả vượt quá 200 ký tự.
- **Then:** hệ thống không cho lưu mẫu thiệp.
- **And:** hiển thị lỗi tại trường tương ứng.

### AC-003 — Từ chối file ảnh không hợp lệ
- **Given:** Admin upload ảnh template/ảnh Preview.
- **When:** file không thuộc định dạng PNG, JPG hoặc vượt quá 10MB.
- **Then:** hệ thống từ chối file và hiển thị lý do file không hợp lệ.

### AC-004 — Trạng thái mặc định
- **Given:** Admin thêm mẫu thiệp thành công.
- **When:** hệ thống tạo bản ghi mẫu thiệp mới.
- **Then:** mẫu thiệp mới phải có trạng thái Active và isDelete = false.

### AC-005 — Báo lỗi khi lưu thất bại
- **Given:** Admin đã nhập dữ liệu hợp lệ.
- **When:** hệ thống không thể lưu thông tin mẫu thiệp hoặc ảnh template/ảnh Preview.
- **Then:** Hệ thống không được thông báo thêm mẫu thiệp thành công.
- **And:** Hệ thống hiển thị thông báo lỗi để Admin thử lại.

### AC-006 — Đảm bảo tính nguyên tử
- **Given:** Hệ thống đang xử lý thêm mẫu thiệp.
- **When:** việc lưu Core Database hoặc lưu ảnh template/ảnh Preview thất bại.
- **Then:** Hệ thống không được tạo mẫu thiệp thành công.
- **And:** Hệ thống không được hiển thị dữ liệu không đầy đủ như kết quả tạo thành công.
- **And:** Admin có thể thực hiện lại thao tác sau khi lỗi được xử lý.

### AC-007 - Hủy thêm mẫu thiệp
- **Given:** Admin đang ở form thêm mẫu thiệp.
- **When:** Admin chọn “Hủy”.
- **Then:** hệ thống không tạo mẫu thiệp mới.
- **And:** hệ thống quay lại danh sách mẫu thiệp.

### AC-008 — Admin không có quyền truy cập
- **Given:** Admin không có quyền quản lý mẫu thiệp.
- **When:** Admin truy cập chức năng thêm mẫu thiệp.
- **Then:** hệ thống từ chối truy cập và không hiển thị form thêm mẫu thiệp.
- **And:** hệ thống không tạo dữ liệu mới.
- **And:** hệ thống hiển thị thông báo phù hợp.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-201**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/60c85a24-3b49-4aa8-87f8-a79e68b2f946) | Thông tin bắt buộc khi thêm mẫu thiệp | Quản lý mẫu thiệp | Mẫu thiệp mới phải có đủ thông tin bắt buộc và đáp ứng giới hạn độ dài trước khi được tạo. | Admin thêm mẫu thiệp mới. | Yêu cầu tên mẫu và ảnh template/ảnh Preview; tên mẫu không vượt quá 100 ký tự; mô tả nếu nhập không vượt quá 200 ký tự. | Nếu thiếu hoặc vượt giới hạn, hệ thống không lưu. | Admin có quyền quản lý mẫu thiệp | STORY-058, STORY-035 | Draft | v0 | 2026-08-26 |
| [**BR-202**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d6f9389-6669-4a70-a7b4-79e18f8ddb20) | Điều kiện file ảnh mẫu thiệp | Quản lý mẫu thiệp | Ảnh template/ảnh Preview mẫu thiệp phải đáp ứng điều kiện định dạng và dung lượng. | Admin upload ảnh template/ảnh Preview. | Chỉ chấp nhận PNG/JPG, tối đa 10 MB. | File không đúng định dạng/vượt dung lượng bị từ chối. | Admin có quyền quản lý mẫu thiệp | STORY-058, STORY-035 | Draft | v0 | 2026-08-26 |
| [**BR-203**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ef16666b-14ce-4677-be31-dc48d75f7038) | Trạng thái mặc định của mẫu thiệp mới | Quản lý mẫu thiệp | Mẫu thiệp mới sau khi tạo thành công phải có trạng thái mặc định thống nhất. | Admin thêm mẫu thiệp mới thành công. | Hệ thống thiết lập trạng thái mẫu thiệp là Active và isDelete = false. | Nếu lưu thất bại, không tạo dữ liệu dở dang. | Admin có quyền quản lý mẫu thiệp | STORY-058, STORY-035 | Draft | v0 | 2026-08-26 |
| [**BR-204**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b4a0f597-64b8-437f-802e-86d7f87089fd) | Tính nguyên tử khi thêm mẫu thiệp | Quản lý mẫu thiệp | Việc thêm mẫu thiệp chỉ thành công khi cả thông tin và ảnh liên quan được lưu thành công. | Admin lưu mẫu thiệp mới. | Lưu thông tin vào Core Database và ảnh vào storage. | Nếu 1 trong 2 bước thất bại, không hiển thị thêm thành công. | Admin có quyền quản lý mẫu thiệp | STORY-058, STORY-035 | Draft | v0 | 2026-08-26 |
| [**BR-251**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a46e2499-1305-449d-8091-97312508ab2f) | Cho phép trùng tên mẫu thiệp | Mẫu thiệp | Tên mẫu thiệp không yêu cầu duy nhất trong hệ thống. | Admin thêm mẫu thiệp mới. | Hệ thống cho phép tạo mẫu thiệp có tên trùng với mẫu thiệp đã tồn tại. | Miễn các dữ liệu còn lại hợp lệ. | Đức Bình | STORY-058 | Draft | v0 | 2026-08-28 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-201 | [Thông tin bắt buộc khi thêm mẫu thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/60c85a24-3b49-4aa8-87f8-a79e68b2f946) |
| BR-202 | [Điều kiện file ảnh mẫu thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d6f9389-6669-4a70-a7b4-79e18f8ddb20) |
| BR-203 | [Trạng thái mặc định của mẫu thiệp mới](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ef16666b-14ce-4677-be31-dc48d75f7038) |
| BR-204 | [Tính nguyên tử khi thêm mẫu thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b4a0f597-64b8-437f-802e-86d7f87089fd) |
| BR-251 | [Cho phép trùng tên mẫu thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a46e2499-1305-449d-8091-97312508ab2f) |

### Dependencies
- [**STORY-057**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)

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
