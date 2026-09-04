# STORY-046 — Admin tải xuống thiệp AI

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý AI Custom, tôi muốn tải xuống file ảnh thiệp AI của khách hàng, để phục vụ kiểm tra, đối soát hoặc hỗ trợ nghiệp vụ. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hồ Hoàng Nam |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Admin có thể thực hiện tải xuống từ **menu thao tác tại danh sách thiệp** hoặc từ **trang Chi tiết thiệp**.
- Chức năng sử dụng chính file ảnh thiệp đã generate thành công.
- Thao tác tải không gọi AI, không tạo ảnh mới, không tạo History record và không thay đổi quota của khách hàng.
- Story này chỉ áp dụng cho Admin có quyền tải thiệp AI; không thay thế quyền tải của khách hàng trong STORY-041.

---

## Conditions

### Preconditions
- Người dùng đã đăng nhập Website quản trị.
- Người dùng có quyền tải thiệp trong module AI Custom.
- History record của thiệp từng được tạo thành công.

### Trigger
> Admin chọn **"Tải ảnh thiệp xuống"** tại danh sách hoặc Chi tiết thiệp.

---

## Flow

### Main Flow: MF – Tải xuống thiệp

1. Admin chọn "Tải ảnh thiệp xuống".
2. Backend kiểm tra quyền tải của Admin.
3. Backend kiểm tra History record tồn tại và có file ảnh output được ghi nhận.
4. Backend kiểm tra file ảnh còn khả dụng trong storage.
5. Hệ thống lấy chính file ảnh thiệp đã generate thành công.
6. Hệ thống tạo tên file an toàn theo định dạng: `thiep_[ma-thiep]_[yyyyMMdd_HHmmss].png`
7. Backend bắt đầu cung cấp file PNG cho thiết bị của Admin.
8. Thao tác không gọi AI, không tạo file ảnh mới trong storage, không tạo History record và không thay đổi quota của khách hàng.

---

### Alternative Flows

#### ALT-01 — Tải lại
1. Admin được tải lại không giới hạn số lần nếu còn quyền truy cập và file còn khả dụng.
2. Mỗi lần tải đều được kiểm tra lại quyền và trạng thái file.

#### ALT-02 — Tải từ danh sách
1. Admin mở menu ba chấm của một item và chọn "Tải ảnh thiệp xuống".
2. Hệ thống thực hiện cùng quy trình kiểm tra như khi tải từ Chi tiết thiệp.

#### ALT-03 — Tải từ Chi tiết thiệp
1. Admin chọn nút "Tải ảnh thiệp xuống" tại trang Chi tiết thiệp.
2. File và tên file sử dụng mã thiệp, không sử dụng mã Order.

---

### Exception Flows

#### EXC-01 — Không tìm thấy thiệp
1. Backend kiểm tra mã thiệp hoặc History record được yêu cầu.
2. Thiệp không tồn tại, đã bị xóa logic hoặc không thể truy cập.
3. Backend không trả file.
4. Backend không trả storage URL hoặc metadata.
5. Hệ thống hiển thị trạng thái tài nguyên không tồn tại hoặc không thể truy cập.

#### EXC-02 — Không có quyền tải
1. Backend kiểm tra quyền tải thiệp AI của người dùng hiện tại.
2. Người dùng không có quyền tải thiệp AI.
3. Backend từ chối request.
4. Backend không trả file, storage URL, đường dẫn nội bộ hoặc metadata nhạy cảm.
5. Hệ thống hiển thị trạng thái không có quyền tải.

#### EXC-03 — Lỗi cung cấp file
1. Backend bắt đầu xử lý request tải nhưng gặp lỗi khi đọc file, stream file hoặc tạo response download.
2. Hệ thống không ghi nhận lần tải là thành công.
3. Hệ thống không trả file thiếu/hỏng như một kết quả thành công.
4. Hệ thống hiển thị: **"Tải thiệp thất bại. Vui lòng thử lại."**
5. Hệ thống cho phép Admin thử lại.
6. Khi Admin thử lại, backend kiểm tra lại quyền, History record và trạng thái file trước khi cung cấp file.

#### EXC-04 — File ảnh không còn khả dụng
1. History record của thiệp vẫn tồn tại.
2. Admin thực hiện tải xuống thiệp.
3. Backend phát hiện file ảnh bị mất, hỏng hoặc không thể truy cập.
4. Hệ thống không trả file hoặc storage URL.
5. Hệ thống giữ nguyên History record và metadata.
6. Hệ thống không gọi AI để tạo lại ảnh.
7. Hệ thống hiển thị: **"Ảnh không còn khả dụng."**

---

## Acceptance Criteria

### AC-001 – Tải đúng file
- **Given:** Admin có quyền và file thiệp còn khả dụng
- **When:** Admin chọn "Tải ảnh thiệp xuống"
- **Then:** hệ thống trả đúng file ảnh thiệp đã generate
- **And:** tên file sử dụng mã thiệp.

### AC-002 – Không gọi AI
- **Given:** thiệp đã có file output
- **When:** Admin tải hoặc tải lại
- **Then:** hệ thống không gọi AI
- **And:** không tạo ảnh hoặc History record mới
- **And:** không thay đổi quota khách hàng.

### AC-003 – Tải lại
- **Given:** file còn khả dụng và Admin còn quyền
- **When:** Admin tải nhiều lần
- **Then:** hệ thống tiếp tục cung cấp đúng file.

### AC-004 – Phân quyền
- **Given:** người dùng không có quyền tải thiệp
- **When:** người dùng gọi API bằng mã thiệp
- **Then:** backend từ chối
- **And:** không trả file hoặc storage URL.

### AC-005 – Tên file an toàn
- **Given:** mã thiệp chứa ký tự không an toàn
- **When:** hệ thống tạo tên file
- **Then:** ký tự không an toàn được loại bỏ hoặc thay thế
- **And:** tên file kết thúc bằng `.png`.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-140**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e6b1d971-1140-4a78-9173-e415327dce18) | File nguồn | Ảnh | Download luôn sử dụng file PNG chính thức của thiệp đã được lưu sau khi generate thành công, không gắn thêm logo, không tạo phiên bản mới trong storage và không kích hoạt AI. | Admin tải xuống thiệp AI. | Download luôn sử dụng file PNG chính thức của thiệp đã được lưu sau khi generate thành công, không gắn thêm logo, không tạo phiên bản mới trong storage và không kích hoạt AI. | N/A | Đức Bình | STORY-046 | Draft | v0 | 2026-08-19 |
| [**BR-141**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/51379482-5a1a-44c9-bc4f-64970e316a00) | Tên file | Ảnh | Tên file có định dạng `thiep_[ma-thiep]_[yyyyMMdd_HHmmss].png`; thời gian được xác định tại lúc bắt đầu tải theo múi giờ Asia/Ho_Chi_Minh. | Hệ thống tạo tên file tải xuống. | Tên file có định dạng `thiep_[ma-thiep]_[yyyyMMdd_HHmmss].png`; thời gian được xác định tại lúc bắt đầu tải theo múi giờ Asia/Ho_Chi_Minh. | N/A | Đức Bình | STORY-046 | Draft | v0 | 2026-08-19 |
| [**BR-142**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/37e14851-8725-4cc1-b578-e095dec06a4a) | Quyền tải | Ảnh | Chỉ Admin có quyền tải thiệp AI mới được backend cung cấp file. | Backend nhận request tải thiệp AI. | Chỉ Admin có quyền tải thiệp AI mới được backend cung cấp file. | N/A | Đức Bình | STORY-046 | Draft | v0 | 2026-08-19 |
| [**BR-143**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ecef43eb-67f5-4c95-9e5c-4867d94cf326) | Không tiêu hao quota | Ảnh | Tải và tải lại không làm thay đổi quota AI của khách hàng. | Admin tải hoặc tải lại thiệp AI. | Tải và tải lại không làm thay đổi quota AI của khách hàng. | N/A | Đức Bình | STORY-046 | Draft | v0 | 2026-08-19 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-140 | [File nguồn](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e6b1d971-1140-4a78-9173-e415327dce18) |
| BR-141 | [Tên file](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/51379482-5a1a-44c9-bc4f-64970e316a00) |
| BR-142 | [Quyền tải](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/37e14851-8725-4cc1-b578-e095dec06a4a) |
| BR-143 | [Không tiêu hao quota](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ecef43eb-67f5-4c95-9e5c-4867d94cf326) |

### Dependencies
- [**STORY-035**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [**STORY-036**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)
- [**STORY-043**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6e28060f-6d59-430e-81e6-1b4549ff1003)
- [**STORY-044**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1a84a816-6d73-4bbd-abf8-5fca7fd25cb3)

---

## Non-Functional Requirements

- Backend không expose permanent storage URL, thông tin xác thực hoặc đường dẫn nội bộ.
- API metadata không nhúng binary/Base64 của ảnh.
- Với file còn khả dụng, hệ thống bắt đầu phản hồi download **p95 ≤ 2 giây** trong điều kiện bình thường, không tính thời gian truyền toàn bộ file.
- Response phải khai báo đúng loại nội dung PNG.
- Backend phải kiểm tra quyền ở mỗi lần tải, kể cả khi Admin biết hoặc đoán được mã thiệp.
- Mỗi lần Admin tải xuống ảnh thiệp của khách hàng, hệ thống phải ghi **audit log** gồm tối thiểu: Admin thực hiện, thiệp được tải và thời điểm thực hiện.

---

## Out of Scope

- Generate, tạo lại hoặc chỉnh sửa thiệp.
- Tải xuống mẫu hoa Custom AI.
- Chỉnh sửa hoặc xóa History record.
- Thay đổi Order hoặc quota.
- Export Excel.
