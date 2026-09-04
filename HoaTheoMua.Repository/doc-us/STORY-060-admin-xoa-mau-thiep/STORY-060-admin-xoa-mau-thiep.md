# STORY-060 — Admin xóa mẫu thiệp

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý mẫu thiệp, tôi muốn xóa mẫu thiệp không còn sử dụng để khách hàng không tiếp tục chọn mẫu đó khi tạo thiệp mới. |
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

Xóa mẫu thiệp là **xóa mềm** bằng cách cập nhật `isDelete = true`. Hệ thống không xóa cứng mẫu thiệp khỏi Core Database và không xóa cứng ảnh template/ảnh Preview khỏi storage. 

Mẫu thiệp đã xóa mềm không hiển thị trong danh sách mặc định và không hiển thị cho khách hàng khi tạo thiệp mới. Việc xóa mềm không làm mất lịch sử thiệp, Checkout hoặc Order đã từng sử dụng mẫu thiệp đó. 

`isDelete` là trạng thái kỹ thuật dùng cho thao tác xóa mềm và không hiển thị như một cột trạng thái trong danh sách mẫu thiệp.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý mẫu thiệp.
- Mẫu thiệp tồn tại trong Core Database.
- Mẫu thiệp chưa bị xóa mềm.
- Hệ thống đang hoạt động bình thường.
- Mẫu thiệp có thể ở trạng thái Active hoặc Inactive; trạng thái này không ảnh hưởng đến thao tác xóa mềm.

### Trigger
> Admin chọn thao tác “Xóa” tại một mẫu thiệp trong màn hình quản lý mẫu thiệp.

---

## Flow

### Main Flow: Admin xóa mềm mẫu thiệp thành công

1. Admin truy cập chức năng quản lý mẫu thiệp.
2. Hệ thống kiểm tra quyền quản lý mẫu thiệp của Admin.
3. Hệ thống hiển thị danh sách mẫu thiệp chưa bị xóa mềm.
4. Admin chọn thao tác “Xóa” tại một mẫu thiệp.
5. Hệ thống hiển thị popup xác nhận với nội dung: “Bạn chắc chắn muốn xóa mẫu thiệp này không?”.
6. Admin chọn “Yes”.
7. Hệ thống cập nhật trường `isDelete = true` của mẫu thiệp trong Core Database.
8. Hệ thống thông báo xóa mẫu thiệp thành công.
9. Hệ thống cập nhật lại danh sách mặc định và không còn hiển thị mẫu thiệp vừa bị xóa.

---

### Alternative Flows

#### ALT-01 — Admin hủy xóa mẫu thiệp
1. Tại popup xác nhận xóa mẫu thiệp, Admin chọn hủy hoặc đóng popup.
2. Hệ thống đóng popup xác nhận.
3. Hệ thống không cập nhật trường `isDelete`.
4. Hệ thống giữ nguyên trạng thái và dữ liệu của mẫu thiệp.
5. Hệ thống tiếp tục hiển thị danh sách mẫu thiệp như trước đó.

---

### Exception Flows

#### EXC-01 — Admin không có quyền xóa mẫu thiệp
1. Người dùng không phải Admin hoặc không có quyền quản lý mẫu thiệp.
2. Hệ thống từ chối thao tác xóa.
3. Không cập nhật `isDelete`.
4. Hệ thống hiển thị thông báo phù hợp.
5. Hệ thống giữ nguyên dữ liệu mẫu thiệp.

#### EXC-02 — Mẫu thiệp không tồn tại hoặc đã bị xóa mềm
1. Admin chọn xóa một mẫu thiệp không còn tồn tại hoặc đã bị xóa mềm trước đó.
2. Hệ thống không thực hiện cập nhật xóa.
3. Hệ thống hiển thị thông báo phù hợp.
4. Hệ thống tải lại danh sách mẫu thiệp hiện tại.

#### EXC-03 — Cập nhật xóa mềm thất bại
1. Admin đã xác nhận xóa mẫu thiệp.
2. Hệ thống không thể cập nhật trường `isDelete = true` trong Core Database.
3. Hệ thống không ghi nhận mẫu thiệp là đã xóa.
4. Hệ thống hiển thị thông báo lỗi và cho phép Admin thử lại.
5. Mẫu thiệp vẫn hiển thị trong danh sách mẫu thiệp nếu dữ liệu chưa được cập nhật thành công.

#### EXC-04 — Mất kết nối hoặc hệ thống lỗi khi xóa mẫu thiệp
1. Admin đã xác nhận xóa mẫu thiệp.
2. Hệ thống mất kết nối hoặc xảy ra lỗi hệ thống trong quá trình xử lý.
3. Hệ thống không hiển thị kết quả xóa thành công khi chưa xác nhận cập nhật thành công từ Core Database.
4. Hệ thống hiển thị thông báo lỗi và cho phép Admin thử lại bằng nút “Thử lại”.

#### EXC-05 — Mẫu thiệp bị xóa trong lúc khách hàng đang sử dụng
1. Khách hàng đã chọn mẫu thiệp khi mẫu thiệp chưa bị xóa.
2. Trước khi khách hàng hoàn thành tạo thiệp, Admin xóa mềm mẫu thiệp.
3. Khách hàng chọn hoàn thành tạo thiệp.
4. Hệ thống kiểm tra lại `isDelete` và trạng thái mẫu thiệp từ Core Database.
5. Hệ thống xác định mẫu thiệp đã bị xóa mềm.
6. Hệ thống không tạo thiệp mới với mẫu thiệp đó.
7. Hệ thống thông báo mẫu thiệp đã chọn không còn khả dụng.
8. Hệ thống yêu cầu khách hàng chọn mẫu thiệp khác.

---

## Acceptance Criteria

### AC-001 — Nút thao tác Xóa
- **Given:** Admin có quyền quản lý mẫu thiệp.
- **When:** hệ thống hiển thị danh sách mẫu thiệp.
- **Then:** hệ thống phải hiển thị thao tác “Xóa” tại mẫu thiệp chưa bị xóa mềm.
- **And:** Mẫu thiệp chưa bị xóa mềm.

### AC-002 — Popup xác nhận xóa
- **Given:** Admin có quyền quản lý mẫu thiệp.
- **When:** Admin chọn thao tác “Xóa”.
- **Then:** hệ thống phải hiển thị popup xác nhận.
- **And:** Mẫu thiệp chưa bị xóa mềm.
- **And:** nội dung popup là “Bạn chắc chắn muốn xóa mẫu thiệp này không?”.
- **And:** popup phải có nút “Yes” để xác nhận xóa.

### AC-003 — Hủy xóa mẫu thiệp
- **Given:** hệ thống đang hiển thị popup xác nhận xóa mẫu thiệp.
- **When:** Admin chọn hủy hoặc đóng popup.
- **Then:** hệ thống phải đóng popup xác nhận.
- **And:** không cập nhật trường isDelete.
- **And:** Mẫu thiệp vẫn hiển thị trong danh sách mẫu thiệp.

### AC-004 — Xóa thành công
- **Given:** Admin có quyền quản lý mẫu thiệp, popup xác nhận đang hiển thị, mẫu thiệp có isDelete = false.
- **When:** Admin chọn “Yes”.
- **Then:** hệ thống phải cập nhật trường isDelete = true của mẫu thiệp trong Core Database.
- **And:** Thông báo thành công.
- **And:** Mẫu thiệp không còn hiển thị trong danh sách mặc định.

### AC-005 — Bảo toàn dữ liệu vật lý
- **Given:** Admin đã xóa mềm mẫu thiệp thành công.
- **When:** hệ thống lưu thay đổi.
- **Then:** hệ thống không được xóa vật lý dữ liệu mẫu thiệp khỏi Core Database.
- **And:** không được xóa vật lý ảnh template/ảnh Preview của mẫu thiệp khỏi storage.

### AC-006 — Ẩn với khách hàng
- **Given:** Mẫu thiệp đã có isDelete = true.
- **When:** khách hàng truy cập quy trình tạo thiệp mới.
- **Then:** hệ thống không được hiển thị mẫu thiệp đó cho khách hàng lựa chọn.

### AC-007 — Bảo toàn liên kết lịch sử
- **Given:** Mẫu thiệp đã từng được sử dụng trong thiệp, Checkout hoặc Order đã tồn tại.
- **When:** Admin xóa mềm mẫu thiệp.
- **Then:** hệ thống không được làm mất liên kết giữa mẫu thiệp và dữ liệu lịch sử đã tồn tại.
- **And:** Dữ liệu mẫu thiệp và ảnh template/ảnh Preview vẫn được giữ để phục vụ xem lịch sử.
- **And:** Mẫu thiệp đã bị xóa mềm không được sử dụng để tạo thiệp mới.
- **And:** Nếu khách hàng muốn tạo thiệp mới, khách hàng phải chọn mẫu thiệp còn khả dụng khác.

### AC-008 — Tránh xóa đúp
- **Given:** Mẫu thiệp đã có isDelete = true.
- **When:** Admin hoặc hệ thống gửi yêu cầu xóa lại mẫu thiệp đó.
- **Then:** hệ thống không thực hiện cập nhật xóa lại.
- **And:** hiển thị thông báo phù hợp.

### AC-009 — Lỗi quyền truy cập
- **Given:** Người dùng không phải Admin hoặc không có quyền quản lý mẫu thiệp.
- **When:** người dùng thực hiện thao tác xóa mẫu thiệp.
- **Then:** hệ thống phải từ chối yêu cầu.
- **And:** không được cập nhật trường isDelete.
- **And:** hệ thống phải hiển thị thông báo phù hợp.
- **And:** Dữ liệu và trạng thái hiện tại của mẫu thiệp phải được giữ nguyên.

### AC-010 — Lỗi cập nhật Core Database
- **Given:** Admin đã xác nhận xóa mẫu thiệp.
- **When:** hệ thống không thể cập nhật trường isDelete = true trong Core Database.
- **Then:** hệ thống phải hiển thị thông báo lỗi.
- **And:** Mẫu thiệp không được ghi nhận là đã xóa nếu cập nhật chưa thành công.
- **And:** hệ thống cho phép Admin thử lại.

### AC-011 — Cập nhật danh sách hiển thị
- **Given:** Admin đã xóa mềm mẫu thiệp thành công.
- **When:** hệ thống quay lại danh sách mẫu thiệp.
- **Then:** danh sách mẫu thiệp mặc định phải được cập nhật.
- **And:** Mẫu thiệp vừa xóa không còn hiển thị trong danh sách mặc định.

### AC-012 — Khách hàng đang thao tác
- **Given:** Khách hàng đã chọn một mẫu thiệp chưa bị xóa mềm và mẫu thiệp đó đã được Admin xóa mềm trước khi khách hàng hoàn thành tạo thiệp.
- **When:** Khách hàng chọn hoàn thành tạo thiệp.
- **Then:** Hệ thống phải kiểm tra lại isDelete của mẫu thiệp từ Core Database.
- **And:** Nếu isDelete = true, hệ thống không được tạo thiệp mới với mẫu thiệp đó.
- **And:** Hệ thống phải thông báo mẫu thiệp không còn khả dụng.
- **And:** Hệ thống phải yêu cầu khách hàng chọn một mẫu thiệp khác còn khả dụng.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-210**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/af13b78a-1beb-4447-b9bd-aea31e9a680c) | Xóa mẫu thiệp là xóa mềm | Quản lý mẫu thiệp | Xóa mẫu thiệp trong hệ thống quản trị là xóa mềm. | Admin xác nhận xóa mẫu thiệp. | Hệ thống cập nhật isDelete = true trong Core Database. | Hệ thống không được xóa cứng mẫu thiệp khỏi Core Database. | Admin có quyền quản lý mẫu thiệp | STORY-060, STORY-035 | Draft | v0 | 2026-08-26 |
| [**BR-211**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/790d2764-23c2-47df-9553-5c69b117c987) | Mẫu thiệp đã xóa không hiển thị cho khách hàng | Quản lý mẫu thiệp | Mẫu thiệp đã xóa mềm không được hiển thị cho khách hàng khi tạo thiệp mới. | Khách hàng tải danh sách mẫu thiệp khả dụng. | Hệ thống chỉ trả về mẫu thiệp có isDelete = false. | Mẫu thiệp có isDelete = true không được hiển thị dù đang ở trạng thái Active. | Admin có quyền quản lý mẫu thiệp | STORY-060, STORY-035 | Draft | v0 | 2026-08-26 |
| [**BR-212**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/153d644a-1458-4f7e-afa0-359ae837765c) | Mẫu thiệp đã xóa không hiển thị trong danh sách mặc định | Quản lý mẫu thiệp | Danh sách mẫu thiệp mặc định của Admin không hiển thị mẫu thiệp đã xóa mềm. | Admin xem danh sách mẫu thiệp mặc định. | Hệ thống chỉ hiển thị mẫu thiệp có isDelete = false. | Mẫu thiệp có isDelete = true chỉ được hiển thị nếu có chức năng xem dữ liệu đã xóa riêng. | Admin có quyền quản lý mẫu thiệp | STORY-060, STORY-035 | Draft | v0 | 2026-08-26 |
| [**BR-213**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d4205572-ff8e-4108-8dcc-709f86faed53) | Xóa mềm mẫu thiệp không ảnh hưởng lịch sử | Quản lý mẫu thiệp | Xóa mềm mẫu thiệp không được làm mất dữ liệu lịch sử đã sử dụng mẫu thiệp đó. | Mẫu thiệp đã từng được dùng trong thiệp, Checkout hoặc Order. | Hệ thống giữ nguyên dữ liệu lịch sử liên quan. | Hệ thống không được xóa hoặc làm sai lệch lịch sử thiệp, Checkout, Order đã phát sinh trước đó. | Admin có quyền quản lý mẫu thiệp | STORY-060, STORY-035 | Draft | v0 | 2026-08-26 |
| [**BR-214**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bcf5f504-3483-402c-a3f3-d9216fca6d06) | Không xóa cứng ảnh mẫu thiệp khi xóa mềm | Quản lý mẫu thiệp | Ảnh template/ảnh Preview của mẫu thiệp không được xóa cứng khỏi storage khi Admin xóa mềm. | Admin xóa mềm mẫu thiệp. | Hệ thống giữ lại ảnh trong storage để phục vụ lịch sử. | Chỉ được xóa file khỏi storage nếu có chức năng xóa cứng riêng. | Admin có quyền quản lý mẫu thiệp | STORY-060, STORY-035 | Draft | v0 | 2026-08-26 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-210 | [Xóa mẫu thiệp là xóa mềm](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/af13b78a-1beb-4447-b9bd-aea31e9a680c) |
| BR-211 | [Mẫu thiệp đã xóa không hiển thị cho khách hàng](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/790d2764-23c2-47df-9553-5c69b117c987) |
| BR-212 | [Mẫu thiệp đã xóa không hiển thị trong danh sách mặc định](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/153d644a-1458-4f7e-afa0-359ae837765c) |
| BR-213 | [Xóa mềm mẫu thiệp không ảnh hưởng lịch sử](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d4205572-ff8e-4108-8dcc-709f86faed53) |
| BR-214 | [Không xóa cứng ảnh mẫu thiệp khi xóa mềm](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bcf5f504-3483-402c-a3f3-d9216fca6d06) |

### Dependencies
- [**STORY-057**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [**STORY-035**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

---

## Non-Functional Requirements

- Hệ thống phải kiểm tra quyền Admin trước khi thực hiện cập nhật isDelete.
- Thao tác xóa mềm phải được xử lý nhất quán, không hiển thị thành công nếu Core Database chưa cập nhật thành công.
- Hệ thống không được xóa vật lý ảnh template/ảnh Preview hoặc dữ liệu mẫu thiệp khi thực hiện xóa mềm.
- Danh sách mẫu thiệp phải được cập nhật sau khi xóa thành công.
- Thông báo lỗi phải rõ ràng để Admin biết thao tác chưa thành công và có thể thử lại.

---

## Out of Scope

- Xóa vật lý mẫu thiệp khỏi Core Database.
- Xóa vật lý ảnh template/ảnh Preview khỏi storage.
- Khôi phục mẫu thiệp đã xóa.
- Xem danh sách mẫu thiệp đã xóa.
- Chỉnh sửa thông tin mẫu thiệp.
- Chuyển trạng thái Active/Inactive.
- Thay đổi hoặc cập nhật các thiệp, Checkout hoặc Order đã tồn tại.
