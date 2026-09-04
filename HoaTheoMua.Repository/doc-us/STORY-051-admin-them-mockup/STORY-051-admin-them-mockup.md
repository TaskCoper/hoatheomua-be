# STORY-051 — Admin thêm Mockup

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý Mockup, tôi muốn thêm Mockup mới để bổ sung Mockup cho hệ thống và cho phép khách hàng sử dụng trong quy trình khởi tạo mẫu hoa. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Đã duyệt |

> **Feedback yêu cầu sửa gần nhất:**
> "Chưa thấy có kiểm chứng rule: mô tả tối đa 200 ký tự. Chưa có rule chống trùng tên Mockup. EXC-03 chưa có AC cover." — *Nguyễn Đức Bình · 14:12 28/08/2026* (Đã được khắc phục trong phiên bản hiện tại)

---

## Context

Mockup được quản lý tại Core Database và được sử dụng trong quy trình khởi tạo mẫu hoa.
Admin có thể thêm Mockup mới từ màn hình Quản lý Mockup. Thông tin Mockup khi thêm mới gồm:
- Tên Mockup là thông tin bắt buộc, không được chỉ chứa khoảng trắng và có độ dài tối đa 50 ký tự.
- Mô tả không bắt buộc, có độ dài tối đa 200 ký tự.
- Ảnh Mockup.

Trong đó Tên Mockup và ảnh Mockup là thông tin bắt buộc; mô tả là thông tin hỗ trợ nhận diện Mockup.
Ảnh Mockup là hình ảnh được sử dụng để Preview trong màn hình quản trị và hiển thị cho khách hàng khi lựa chọn Mockup.
Ảnh hợp lệ phải là PNG hoặc JPG, dung lượng tối đa 10MB. Hệ thống không áp dụng ràng buộc kích thước ảnh tối thiểu/tối đa hoặc tỷ lệ ảnh.
Mockup được tạo thành công có trạng thái ban đầu là Active.
Một lần thực hiện thao tác “Thêm” thành công chỉ được tạo một Mockup. Việc Admin nhấn “Thêm” nhiều lần liên tiếp không được tạo duplicate ngoài ý muốn.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý Mockup.
- Admin đang ở màn hình Quản lý Mockup.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin chọn chức năng “Thêm Mockup”.

---

## Flow

### Main Flow: Admin thêm Mockup

1. Admin chọn chức năng “Thêm Mockup”.
2. Hệ thống hiển thị form thêm Mockup.
3. Hệ thống hiển thị các trường: Tên Mockup, mô tả và ảnh Mockup.
4. Admin nhập Tên Mockup.
5. Admin nhập mô tả nếu cần.
6. Admin chọn ảnh Mockup từ thiết bị.
7. Hệ thống kiểm tra file ảnh.
8. Nếu file ảnh hợp lệ, hệ thống hiển thị Preview của ảnh đã chọn.
9. Admin kiểm tra thông tin và chọn “Thêm”.
10. Hệ thống kiểm tra tính hợp lệ của dữ liệu.
11. Hệ thống tạo Mockup mới trong Core Database với trạng thái ban đầu là Active.
12. Hệ thống thông báo thêm Mockup thành công.
13. Hệ thống chuyển Admin về danh sách Mockup.
14. Mockup vừa tạo được hiển thị trong danh sách với trạng thái Active.

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
2. Admin chọn “Hủy” hoặc quay lại.
3. Hệ thống không tạo Mockup mới.
4. Hệ thống chuyển Admin về danh sách Mockup.

---

### Exception Flows

#### EXC-01 — Thiếu hoặc không hợp lệ thông tin bắt buộc
1. Admin chưa nhập Tên Mockup, Tên Mockup chỉ chứa khoảng trắng, Tên Mockup vượt quá 50 ký tự hoặc chưa chọn ảnh Mockup hợp lệ.
2. Admin chọn “Thêm Mockup”.
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
2. Admin chọn “Thêm Mockup”.
3. Hệ thống gặp lỗi trong quá trình tạo Mockup.
4. Hệ thống không tạo Mockup ở trạng thái dữ liệu dở dang.
5. Hệ thống thông báo thêm Mockup thất bại.
6. Hệ thống không giữ lại dữ liệu Admin đã nhập.
7. Hệ thống chuyển Admin về danh sách Mockup.
8. Admin có thể chọn lại “Thêm Mockup” để thực hiện lại thao tác.

#### EXC-05 — Không có quyền thực hiện
1. Người dùng thực hiện yêu cầu thêm Mockup.
2. Hệ thống xác định người dùng không có quyền quản lý Mockup.
3. Hệ thống từ chối yêu cầu.
4. Hệ thống không tạo Mockup mới.
5. Hệ thống hiển thị thông báo phù hợp.

#### EXC-06 — Tên Mockup đã tồn tại
1. Tên Mockup sau khi chuẩn hóa khoảng trắng và không phân biệt chữ hoa/chữ thường đã tồn tại ở một Mockup chưa bị xóa mềm.
2. Admin chọn “Thêm”.
3. Hệ thống kiểm tra trùng tên Mockup.
4. Hệ thống không tạo Mockup mới.
5. Hệ thống hiển thị lỗi tên Mockup đã tồn tại.
6. Admin có thể nhập tên khác và thực hiện lại.

---

## Acceptance Criteria

### AC-001 — Hiển thị form thêm Mockup
- **Given:** Admin đã đăng nhập và có quyền quản lý Mockup.
- **When:** Admin chọn “Thêm Mockup”.
- **Then:** Hệ thống phải hiển thị form thêm Mockup.

### AC-002 — Các trường thông tin
- **Given:** Admin đang ở form thêm Mockup.
- **When:** Form được hiển thị.
- **Then:** Hệ thống phải cung cấp các trường: Tên Mockup, mô tả và ảnh Mockup.

### AC-003 — Validation thông tin bắt buộc
- **Given:** Admin đang thêm Mockup.
- **When:** Admin thực hiện thêm Mockup.
- **Then:** Tên Mockup phải được nhập, không được chỉ chứa khoảng trắng và không được vượt quá 50 ký tự.
- **And:** Ảnh Mockup hợp lệ phải được cung cấp.
- **And:** Hệ thống không được tạo Mockup nếu thiếu hoặc không hợp lệ một trong các thông tin bắt buộc.

### AC-004 — File ảnh hợp lệ
- **Given:** Admin đang ở form thêm Mockup.
- **When:** Admin chọn file ảnh PNG hoặc JPG có dung lượng không vượt quá 10MB.
- **Then:** Hệ thống phải ghi nhận file là ảnh Mockup hợp lệ.
- **And:** Hệ thống phải hiển thị Preview của ảnh đã chọn trước khi Admin hoàn tất thao tác thêm.

### AC-005 — Thay đổi ảnh
- **Given:** Admin đã chọn một ảnh Mockup.
- **When:** Admin chọn một ảnh hợp lệ khác.
- **Then:** Hệ thống phải sử dụng ảnh mới làm ảnh được chọn.
- **And:** Preview phải được cập nhật theo ảnh mới.

### AC-006 — File ảnh không hợp lệ
- **Given:** Admin đang thêm Mockup.
- **When:** Admin chọn file không phải PNG/JPG hoặc file vượt quá 10MB.
- **Then:** Hệ thống không được ghi nhận file đó là ảnh Mockup.
- **And:** Phải hiển thị thông báo phù hợp.

### AC-007 — Thêm Mockup thành công
- **Given:** Admin đã cung cấp đầy đủ dữ liệu hợp lệ.
- **When:** Admin chọn “Thêm”.
- **Then:** Hệ thống phải tạo một Mockup mới trong Core Database.
- **And:** Phải gán trạng thái ban đầu Active cho Mockup mới.
- **And:** Phải thông báo thêm Mockup thành công.
- **And:** Mockup mới phải xuất hiện trong danh sách quản lý Mockup.

### AC-008 — Hủy thao tác thêm
- **Given:** Admin đang ở form thêm Mockup.
- **When:** Admin chọn “Hủy” hoặc quay lại trước khi hoàn tất.
- **Then:** Hệ thống không được tạo Mockup mới.
- **And:** Admin được chuyển về danh sách Mockup.

### AC-009 — Lỗi khi lưu Mockup
- **Given:** Admin thực hiện thêm Mockup.
- **When:** Quá trình tạo Mockup thất bại.
- **Then:** Hệ thống không được tạo bản ghi Mockup dở dang.
- **And:** Phải thông báo thao tác thất bại.
- **And:** hệ thống không giữ lại dữ liệu đã nhập trên form.
- **And:** hệ thống phải chuyển Admin về danh sách Mockup.
- **And:** Admin có thể chọn lại “Thêm Mockup” để thực hiện lại.

### AC-010 — Chống spam request (Duplicate)
- **Given:** Admin gửi yêu cầu thêm Mockup.
- **When:** Admin nhấn “Thêm” nhiều lần liên tiếp hoặc request bị gửi lặp.
- **Then:** Một thao tác thêm thành công chỉ được tạo một Mockup.
- **And:** Hệ thống không được tạo duplicate ngoài ý muốn.

### AC-011 — Phân quyền
- **Given:** Người dùng không có quyền quản lý Mockup.
- **When:** Người dùng gửi yêu cầu thêm Mockup.
- **Then:** Hệ thống phải từ chối yêu cầu.
- **And:** Không được tạo Mockup mới.
- **And:** Hệ thống phải hiển thị thông báo phù hợp.

### AC-012 — Kiểm tra độ dài mô tả Mockup
- **Given:** Admin đang ở form thêm Mockup.
- **When:** Admin nhập mô tả vượt quá 200 ký tự và chọn “Thêm”.
- **Then:** hệ thống không tạo Mockup mới.
- **And:** hệ thống hiển thị lỗi mô tả không được vượt quá 200 ký tự.

### AC-013 — Không cho thêm Mockup trùng tên
- **Given:** đã tồn tại một Mockup chưa bị xóa mềm có cùng tên sau khi chuẩn hóa khoảng trắng và không phân biệt chữ hoa/chữ thường.
- **When:** Admin gửi yêu cầu thêm Mockup với tên đó.
- **Then:** hệ thống không tạo Mockup mới.
- **And:** hệ thống hiển thị lỗi tên Mockup đã tồn tại.

### AC-014 — Lỗi tải ảnh Mockup
- **Given:** Admin chọn một file ảnh hợp lệ.
- **When:** hệ thống gặp lỗi trong quá trình tải ảnh.
- **Then:** hệ thống không ghi nhận ảnh tải lỗi là ảnh Mockup hợp lệ.
- **And:** hệ thống hiển thị thông báo tải ảnh thất bại.
- **And:** Admin có thể tải lại hoặc chọn ảnh khác.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-163**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0fb3fa93-c3d3-4afb-9249-362689d34b0e) | Quyền truy cập danh sách Mockup | Phân quyền | Chỉ Admin có quyền quản lý Mockup mới được truy cập danh sách quản lý Mockup. | Người dùng yêu cầu truy cập chức năng “Quản lý Mockup”. | Hệ thống kiểm tra quyền của tài khoản trước khi trả dữ liệu quản lý Mockup. | N/A | N/A | STORY-050 | Draft | v0 | 2026-08-25 |
| [**BR-164**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6ec7d242-0830-4a48-ad03-8da0c7f0bb6b) | Nguồn lưu dữ liệu Mockup mới | Quản lý Mockup | Mockup mới do Admin tạo phải được lưu vào Core Database. | Admin thực hiện thêm Mockup với dữ liệu hợp lệ. | Hệ thống tạo bản ghi Mockup mới trong Core Database và sử dụng bản ghi này làm dữ liệu quản lý chính thức. | N/A | N/A | STORY-051 | Draft | v0 | 2026-08-25 |
| [**BR-165**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5fd84240-a8c1-4eef-a8c4-52efa2affada) | Thông tin bắt buộc khi thêm Mockup | Quản lý Mockup | Khi thêm Mockup, Admin phải cung cấp Tên Mockup và ảnh Mockup hợp lệ. | Admin thực hiện thao tác thêm Mockup. | Hệ thống chỉ cho phép tạo Mockup khi Tên Mockup được nhập hợp lệ. Mô tả nếu có không quá 200 ký tự. | N/A | N/A | STORY-051 | Draft | v0 | 2026-08-25 |
| [**BR-166**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1023a5d4-a494-4851-b61a-fbd33f50d39f) | Điều kiện file ảnh Mockup | Quản lý Mockup | Ảnh Mockup được tải lên phải là file ảnh thuộc định dạng được hệ thống hỗ trợ và không vượt quá dung lượng cho phép. | Admin chọn ảnh Mockup từ thiết bị. | Hệ thống chỉ chấp nhận file PNG hoặc JPG, dung lượng tối đa 10MB. | N/A | N/A | STORY-051 | Draft | v0 | 2026-08-25 |
| [**BR-167**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e77482c0-7002-402a-ad0b-ac5a5d6f80fc) | Trạng thái mặc định của Mockup mới | Quản lý Mockup | Mockup được tạo thành công phải có trạng thái ban đầu là Active. | Hệ thống tạo thành công Mockup mới. | Hệ thống gán trạng thái Active cho Mockup mới. | N/A | N/A | STORY-051 | Draft | v0 | 2026-08-25 |
| [**BR-168**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fe545913-29b2-4f35-b757-5be36b844f5f) | Tính nguyên tử khi thêm Mockup | Quản lý Mockup | Một thao tác thêm Mockup thành công chỉ được tạo một Mockup và không được để lại dữ liệu dở dang khi thất bại. | Admin gửi yêu cầu thêm Mockup. | Hệ thống xử lý thao tác thêm Mockup theo hướng nguyên tử. | N/A | N/A | STORY-051 | Draft | v0 | 2026-08-25 |
| [**BR-253**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0a457a4f-2f02-467d-901c-d23b4d487dea) | Tên Mockup không được trùng | Mockup | Tên Mockup phải là duy nhất trong phạm vi các Mockup chưa bị xóa mềm. | Admin thực hiện thêm Mockup mới. | Hệ thống kiểm tra tên Mockup sau khi chuẩn hóa. | N/A | Đức Bình | STORY-051 | Draft | v0 | 2026-08-28 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-163 | [Quyền truy cập danh sách Mockup](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0fb3fa93-c3d3-4afb-9249-362689d34b0e) |
| BR-164 | [Nguồn lưu dữ liệu Mockup mới](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6ec7d242-0830-4a48-ad03-8da0c7f0bb6b) |
| BR-165 | [Thông tin bắt buộc khi thêm Mockup](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5fd84240-a8c1-4eef-a8c4-52efa2affada) |
| BR-166 | [Điều kiện file ảnh Mockup](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1023a5d4-a494-4851-b61a-fbd33f50d39f) |
| BR-167 | [Trạng thái mặc định của Mockup mới](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e77482c0-7002-402a-ad0b-ac5a5d6f80fc) |
| BR-168 | [Tính nguyên tử khi thêm Mockup](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fe545913-29b2-4f35-b757-5be36b844f5f) |
| BR-253 | [Tên Mockup không được trùng](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0a457a4f-2f02-467d-901c-d23b4d487dea) |

---

## Non-Functional Requirements

- Backend phải kiểm tra quyền Admin trước khi cho phép tạo Mockup.
- Một lần thực hiện thao tác “Thêm” thành công chỉ được tạo một Mockup.
- Việc Admin nhấn “Thêm” nhiều lần liên tiếp không được tạo duplicate ngoài ý muốn.
- Nếu quá trình tạo thất bại, không được tồn tại Mockup ở trạng thái dữ liệu lưu dở dang.

---

## Out of Scope

- Chỉnh sửa Mockup.
- Xóa Mockup.
- Thực hiện Active/Inactive Mockup sau khi Mockup đã được tạo.
- Xem trang Chi tiết Mockup.
- Thêm nhiều Mockup trong cùng một thao tác.
