# STORY-051 — Admin thêm Mockup

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **Phiên bản** | v0.1 |
| **User Story** | Là một Admin có quyền quản lý Mockup, tôi muốn thêm Mockup mới để bổ sung Mockup cho hệ thống và cho phép khách hàng sử dụng trong quy trình khởi tạo mẫu hoa. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Reviewer** | Nguyễn Đức Bình |
| **Approver** | Nguyễn Đức Bình |
| **Owner** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Cập nhật** | 25/08/2026 |

---

## Context

Mockup được quản lý tại Core Database và được sử dụng trong quy trình khởi tạo mẫu hoa. Admin có thể thêm Mockup mới từ màn hình Quản lý Mockup.

Thông tin Mockup khi thêm mới gồm:
- **Tên Mockup** — bắt buộc, không được chỉ chứa khoảng trắng, độ dài tối đa 50 ký tự.
- **Mô tả** — thông tin hỗ trợ nhận diện Mockup (không bắt buộc).
- **Ảnh Mockup** — bắt buộc; là hình ảnh được sử dụng để Preview trong màn hình quản trị và hiển thị cho khách hàng khi lựa chọn Mockup.

Ảnh hợp lệ phải là PNG hoặc JPG, dung lượng tối đa 10MB. Hệ thống không áp dụng ràng buộc kích thước ảnh tối thiểu/tối đa hoặc tỷ lệ ảnh.

Mockup được tạo thành công có trạng thái ban đầu là **Active**. Một lần thực hiện thao tác "Thêm" thành công chỉ được tạo một Mockup. Việc Admin nhấn "Thêm" nhiều lần liên tiếp không được tạo duplicate ngoài ý muốn.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý Mockup.
- Admin đang ở màn hình Quản lý Mockup.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin chọn chức năng **"Thêm Mockup"**.

---

## Flow

### Main Flow

1. Admin chọn chức năng **"Thêm Mockup"**.
2. Hệ thống hiển thị form thêm Mockup.
3. Hệ thống hiển thị các trường: Tên Mockup, mô tả và ảnh Mockup.
4. Admin nhập Tên Mockup.
5. Admin nhập mô tả nếu cần.
6. Admin chọn ảnh Mockup từ thiết bị.
7. Hệ thống kiểm tra file ảnh.
8. Nếu file ảnh hợp lệ, hệ thống hiển thị Preview của ảnh đã chọn.
9. Admin kiểm tra thông tin và chọn **"Thêm"**.
10. Hệ thống kiểm tra tính hợp lệ của dữ liệu.
11. Hệ thống tạo Mockup mới trong Core Database với trạng thái ban đầu là **Active**.
12. Hệ thống thông báo thêm Mockup thành công.
13. Hệ thống chuyển Admin về danh sách Mockup.
14. Mockup vừa tạo được hiển thị trong danh sách với trạng thái **Active**.

---

### Alternative Flows

#### ALT-01 — Thay đổi ảnh trước khi thêm

1. Admin đã chọn một ảnh Mockup.
2. Hệ thống đang hiển thị Preview của ảnh đó.
3. Admin chọn ảnh hợp lệ khác.
4. Hệ thống thay thế ảnh đã chọn trước đó bằng ảnh mới.
5. Hệ thống cập nhật Preview theo ảnh mới.
6. Admin tiếp tục nhập thông tin và thực hiện thêm Mockup.

#### ALT-02 — Hủy thêm Mockup

1. Admin đang ở form thêm Mockup.
2. Admin chọn **"Hủy"** hoặc quay lại.
3. Hệ thống không tạo Mockup mới.
4. Hệ thống chuyển Admin về danh sách Mockup.

---

### Exception Flows

#### EXC-01 — Thiếu hoặc không hợp lệ thông tin bắt buộc

1. Admin chưa nhập Tên Mockup, Tên Mockup chỉ chứa khoảng trắng, Tên Mockup vượt quá 50 ký tự hoặc chưa chọn ảnh Mockup hợp lệ.
2. Admin chọn **"Thêm Mockup"**.
3. Hệ thống kiểm tra dữ liệu.
4. Hệ thống không tạo Mockup.
5. Hệ thống hiển thị lỗi tại thông tin chưa hợp lệ.
6. Admin bổ sung thông tin và có thể thực hiện lại.

#### EXC-02 — Ảnh Mockup không hợp lệ

1. Admin chọn ảnh Mockup từ thiết bị.
2. Hệ thống kiểm tra file ảnh.
3. Hệ thống xác định file không phải PNG/JPG hoặc vượt quá 10MB.
4. Hệ thống không sử dụng file đó làm ảnh Mockup.
5. Hệ thống hiển thị thông báo phù hợp.
6. Admin có thể chọn ảnh khác.

#### EXC-03 — Lỗi tải ảnh Mockup

1. Admin chọn một file ảnh hợp lệ.
2. Hệ thống gặp lỗi trong quá trình tải ảnh.
3. Hệ thống không ghi nhận ảnh tải lỗi là ảnh Mockup hợp lệ.
4. Hệ thống thông báo tải ảnh thất bại.
5. Admin có thể thực hiện tải lại hoặc chọn ảnh khác.

#### EXC-04 — Lỗi khi lưu Mockup

1. Admin đã cung cấp đầy đủ dữ liệu hợp lệ.
2. Admin chọn **"Thêm Mockup"**.
3. Hệ thống gặp lỗi trong quá trình tạo Mockup.
4. Hệ thống không tạo Mockup ở trạng thái dữ liệu dở dang.
5. Hệ thống thông báo thêm Mockup thất bại.
6. Hệ thống không giữ lại dữ liệu Admin đã nhập.
7. Hệ thống chuyển Admin về danh sách Mockup.
8. Admin có thể chọn lại **"Thêm Mockup"** để thực hiện lại thao tác.

#### EXC-05 — Không có quyền thực hiện

1. Người dùng thực hiện yêu cầu thêm Mockup.
2. Hệ thống xác định người dùng không có quyền quản lý Mockup.
3. Hệ thống từ chối yêu cầu.
4. Hệ thống không tạo Mockup mới.
5. Hệ thống hiển thị thông báo phù hợp.

---

## Acceptance Criteria

### AC-001 — Hiển thị form thêm Mockup
- **Given:** Admin đã đăng nhập và có quyền quản lý Mockup.
- **When:** Admin chọn "Thêm Mockup".
- **Then:** Hệ thống phải hiển thị form thêm Mockup.

### AC-002 — Hiển thị các trường thông tin
- **Given:** Admin đang ở form thêm Mockup.
- **When:** Form được hiển thị.
- **Then:** Hệ thống phải cung cấp các trường: Tên Mockup, mô tả và ảnh Mockup.

### AC-003 — Validate thông tin bắt buộc
- **Given:** Admin đang thêm Mockup.
- **When:** Admin thực hiện thêm Mockup.
- **Then:** Tên Mockup phải được nhập, không được chỉ chứa khoảng trắng và không được vượt quá 50 ký tự.
- **And:** Ảnh Mockup hợp lệ phải được cung cấp.
- **And:** Hệ thống không được tạo Mockup nếu thiếu hoặc không hợp lệ một trong các thông tin bắt buộc.

### AC-004 — Chấp nhận ảnh hợp lệ và hiển thị Preview
- **Given:** Admin đang ở form thêm Mockup.
- **When:** Admin chọn file ảnh PNG hoặc JPG có dung lượng không vượt quá 10MB.
- **Then:** Hệ thống phải ghi nhận file là ảnh Mockup hợp lệ.
- **And:** Hệ thống phải hiển thị Preview của ảnh đã chọn trước khi Admin hoàn tất thao tác thêm.

### AC-005 — Thay đổi ảnh đã chọn
- **Given:** Admin đã chọn một ảnh Mockup.
- **When:** Admin chọn một ảnh hợp lệ khác.
- **Then:** Hệ thống phải sử dụng ảnh mới làm ảnh được chọn.
- **And:** Preview phải được cập nhật theo ảnh mới.

### AC-006 — Từ chối ảnh không hợp lệ
- **Given:** Admin đang thêm Mockup.
- **When:** Admin chọn file không phải PNG/JPG hoặc file vượt quá 10MB.
- **Then:** Hệ thống không được ghi nhận file đó là ảnh Mockup.
- **And:** Phải hiển thị thông báo phù hợp.

### AC-007 — Tạo Mockup thành công
- **Given:** Admin đã cung cấp đầy đủ dữ liệu hợp lệ.
- **When:** Admin chọn "Thêm".
- **Then:** Hệ thống phải tạo một Mockup mới trong Core Database.
- **And:** Phải gán trạng thái ban đầu **Active** cho Mockup mới.
- **And:** Phải thông báo thêm Mockup thành công.
- **And:** Mockup mới phải xuất hiện trong danh sách quản lý Mockup.

### AC-008 — Hủy thêm Mockup
- **Given:** Admin đang ở form thêm Mockup.
- **When:** Admin chọn "Hủy" hoặc quay lại trước khi hoàn tất.
- **Then:** Hệ thống không được tạo Mockup mới.
- **And:** Admin được chuyển về danh sách Mockup.

### AC-009 — Xử lý khi tạo Mockup thất bại
- **Given:** Admin thực hiện thêm Mockup.
- **When:** Quá trình tạo Mockup thất bại.
- **Then:** Hệ thống không được tạo bản ghi Mockup dở dang.
- **And:** Phải thông báo thao tác thất bại.
- **And:** Hệ thống không giữ lại dữ liệu đã nhập trên form.
- **And:** Hệ thống phải chuyển Admin về danh sách Mockup.
- **And:** Admin có thể chọn lại "Thêm Mockup" để thực hiện lại.

### AC-010 — Chống duplicate
- **Given:** Admin gửi yêu cầu thêm Mockup.
- **When:** Admin nhấn "Thêm" nhiều lần liên tiếp hoặc request bị gửi lặp.
- **Then:** Một thao tác thêm thành công chỉ được tạo một Mockup.
- **And:** Hệ thống không được tạo duplicate ngoài ý muốn.

### AC-011 — Kiểm tra quyền
- **Given:** Người dùng không có quyền quản lý Mockup.
- **When:** Người dùng gửi yêu cầu thêm Mockup.
- **Then:** Hệ thống phải từ chối yêu cầu.
- **And:** Không được tạo Mockup mới.
- **And:** Hệ thống phải hiển thị thông báo phù hợp.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|
| **[BR-166](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1023a5d4-a494-4851-b61a-fbd33f50d39f)** | Điều kiện file ảnh Mockup | Quản lý Mockup | Ảnh Mockup được tải lên phải là file ảnh thuộc định dạng được hệ thống hỗ trợ và không vượt quá dung lượng cho phép. | Admin chọn ảnh Mockup từ thiết bị. | Hệ thống chỉ chấp nhận file PNG hoặc JPG, dung lượng tối đa 10MB. Hệ thống không áp dụng ràng buộc kích thước ảnh tối thiểu/tối đa hoặc tỷ lệ ảnh. | Nếu file không đúng định dạng hoặc vượt quá 10MB, hệ thống không ghi nhận file đó là ảnh Mockup hợp lệ. | Draft | v0 | 2026-08-25 |
| **[BR-168](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fe545913-29b2-4f35-b757-5be36b844f5f)** | Tính nguyên tử khi thêm Mockup | Quản lý Mockup | Một thao tác thêm Mockup thành công chỉ được tạo một Mockup và không được để lại dữ liệu dở dang khi thất bại. | Admin gửi yêu cầu thêm Mockup. | Hệ thống xử lý thao tác thêm Mockup theo hướng nguyên tử: thành công thì tạo đúng một Mockup; thất bại thì không tạo bản ghi Mockup dở dang. Việc Admin nhấn "Thêm" nhiều lần liên tiếp không được tạo duplicate ngoài ý muốn. | Nếu hệ thống không xác định được kết quả tạo Mockup, trạng thái dữ liệu phải được xử lý để không phát sinh bản ghi trùng hoặc dở dang. | Draft | v0 | 2026-08-25 |
| **[BR-167](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e77482c0-7002-402a-ad0b-ac5a5d6f80fc)** | Trạng thái mặc định của Mockup mới | Quản lý Mockup | Mockup được tạo thành công phải có trạng thái ban đầu là Active. | Hệ thống tạo thành công Mockup mới. | Hệ thống gán trạng thái Active cho Mockup mới và hiển thị trạng thái này trong danh sách quản lý Mockup. | Không tạo Mockup mới ở trạng thái không xác định hoặc đồng thời nhiều trạng thái. | Draft | v0 | 2026-08-25 |
| **[BR-165](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5fd84240-a8c1-4eef-a8c4-52efa2affada)** | Thông tin bắt buộc khi thêm Mockup | Quản lý Mockup | Khi thêm Mockup, Admin phải cung cấp Tên Mockup và ảnh Mockup hợp lệ. | Admin thực hiện thao tác thêm Mockup. | Hệ thống chỉ cho phép tạo Mockup khi Tên Mockup được nhập, không chỉ chứa khoảng trắng, có độ dài tối đa 50 ký tự và ảnh Mockup hợp lệ đã được cung cấp. | Nếu thiếu Tên Mockup hoặc ảnh Mockup hợp lệ, hệ thống không được tạo Mockup mới. | Draft | v0 | 2026-08-25 |
| **[BR-164](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6ec7d242-0830-4a48-ad03-8da0c7f0bb6b)** | Nguồn lưu dữ liệu Mockup mới | Quản lý Mockup | Mockup mới do Admin tạo phải được lưu vào Core Database. | Admin thực hiện thêm Mockup với dữ liệu hợp lệ. | Hệ thống tạo bản ghi Mockup mới trong Core Database và sử dụng bản ghi này làm dữ liệu quản lý chính thức. | Nếu không thể lưu vào Core Database, hệ thống không được xem thao tác thêm Mockup là thành công. | Draft | v0 | 2026-08-25 |
| **[BR-163](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0fb3fa93-c3d3-4afb-9249-362689d34b0e)** | Quyền truy cập danh sách Mockup | Phân quyền | Chỉ Admin có quyền quản lý Mockup mới được truy cập danh sách quản lý Mockup. | Người dùng yêu cầu truy cập chức năng "Quản lý Mockup". | Hệ thống kiểm tra quyền của tài khoản trước khi trả dữ liệu quản lý Mockup. | Người dùng không có quyền quản lý Mockup không được truy cập hoặc nhận dữ liệu quản trị Mockup. | Draft | v0 | 2026-08-25 |

---

## References

### Business Rules
`BR-163` · `BR-164` · `BR-165` · `BR-166` · `BR-167` · `BR-168`

---

## Non-Functional Requirements

- Backend phải kiểm tra quyền Admin trước khi cho phép tạo Mockup.
- Một lần thực hiện thao tác "Thêm" thành công chỉ được tạo một Mockup.
- Việc Admin nhấn "Thêm" nhiều lần liên tiếp không được tạo duplicate ngoài ý muốn.
- Nếu quá trình tạo thất bại, không được tồn tại Mockup ở trạng thái dữ liệu lưu dở dang.

---

## Out of Scope

- Chỉnh sửa Mockup.
- Xóa Mockup.
- Thực hiện Active/Inactive Mockup sau khi Mockup đã được tạo.
- Xem trang Chi tiết Mockup.
- Thêm nhiều Mockup trong cùng một thao tác.
