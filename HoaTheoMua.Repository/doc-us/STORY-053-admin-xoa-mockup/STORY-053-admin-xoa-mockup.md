# STORY-053 — Admin xóa Mockup

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý Mockup, tôi muốn xóa Mockup để loại bỏ Mockup không còn sử dụng khỏi danh sách quản lý và không cho khách hàng tiếp tục chọn trong quy trình tạo mẫu hoa mới. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Đã duyệt |

---

## Context

Mockup được quản lý tại Core Database và được sử dụng trong quy trình khởi tạo mẫu hoa.
Admin có thể xóa Mockup từ màn hình Quản lý Mockup. Việc xóa Mockup trong US này là xóa mềm, không xóa vật lý dữ liệu Mockup hoặc ảnh Preview khỏi hệ thống. Khi Admin xác nhận xóa, hệ thống cập nhật trường `isDelete = true` trong Core Database. 

Mockup đã bị xóa mềm:
- Không hiển thị trong danh sách Mockup mặc định của Admin.
- Không hiển thị cho khách hàng trong quy trình tạo mẫu hoa mới.
- Không được sử dụng cho yêu cầu tạo mẫu hoa mới hoặc generate lại.
- Vẫn giữ dữ liệu, ảnh Preview và liên kết với các yêu cầu đã tồn tại để phục vụ xem lịch sử.
- Nếu khách hàng muốn tạo mẫu hoa mới, phải thực hiện lại quy trình tạo mới và chọn Mockup hiện còn khả dụng.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý Mockup.
- Mockup tồn tại trong Core Database.
- Mockup chưa bị xóa mềm.
- Hệ thống đang hoạt động bình thường.
- Mockup có thể ở trạng thái Hoạt động hoặc Không hoạt động; trạng thái này không ảnh hưởng đến thao tác xóa mềm.

### Trigger
> Admin chọn thao tác “Xóa” tại một Mockup trong màn hình Quản lý Mockup.

---

## Flow

### Main Flow: Admin xóa mềm Mockup thành công

1. Admin truy cập chức năng “Quản lý Mockup”.
2. Hệ thống kiểm tra quyền truy cập chức năng quản lý Mockup của Admin.
3. Hệ thống hiển thị danh sách Mockup chưa bị xóa mềm.
4. Admin chọn thao tác “Xóa” tại một Mockup.
5. Hệ thống hiển thị popup xác nhận với nội dung: “Bạn chắc chắn muốn xóa Mockup này không?”.
6. Admin chọn “Yes”.
7. Hệ thống cập nhật trường `isDelete = true` của Mockup trong Core Database.
8. Hệ thống thông báo xóa Mockup thành công.
9. Hệ thống cập nhật lại danh sách Mockup và không còn hiển thị Mockup vừa bị xóa trong danh sách mặc định.

---

### Alternative Flows

#### ALT-01 — Admin hủy xóa Mockup
1. Tại popup xác nhận xóa Mockup, Admin chọn hủy hoặc đóng popup.
2. Hệ thống đóng popup xác nhận.
3. Hệ thống không cập nhật trường `isDelete`.
4. Hệ thống giữ nguyên trạng thái và dữ liệu của Mockup.
5. Hệ thống tiếp tục hiển thị danh sách Mockup như trước đó.

---

### Exception Flows

#### EXC-01 — Admin không có quyền xóa Mockup
1. Người dùng không phải Admin hoặc không có quyền quản lý Mockup.
2. Hệ thống từ chối thao tác xóa.
3. Không cập nhật `isDelete`.

#### EXC-02 — Mockup không tồn tại hoặc đã bị xóa mềm
1. Admin chọn xóa một Mockup không còn tồn tại hoặc đã bị xóa mềm trước đó.
2. Hệ thống không thực hiện cập nhật xóa.
3. Hệ thống hiển thị thông báo phù hợp.
4. Hệ thống tải lại danh sách Mockup hiện tại.

#### EXC-03 — Cập nhật xóa mềm thất bại
1. Admin đã xác nhận xóa Mockup.
2. Hệ thống không thể cập nhật trường `isDelete = true` trong Core Database.
3. Hệ thống không ghi nhận Mockup là đã xóa.
4. Hệ thống hiển thị thông báo lỗi và cho phép Admin thử lại.
5. Mockup vẫn hiển thị trong danh sách Mockup nếu dữ liệu chưa được cập nhật thành công.

#### EXC-04 — Mất kết nối hoặc hệ thống lỗi khi xóa Mockup
1. Admin đã xác nhận xóa Mockup.
2. Hệ thống mất kết nối hoặc xảy ra lỗi hệ thống trong quá trình xử lý.
3. Hệ thống không hiển thị kết quả xóa thành công khi chưa xác nhận cập nhật thành công từ Core Database.
4. Hệ thống hiển thị thông báo lỗi và cho phép Admin thử lại bằng nút “Thử lại”.

#### EXC-05 — Mockup bị xóa trong lúc khách hàng đang sử dụng
1. Khách hàng đã chọn Mockup khi Mockup chưa bị xóa.
2. Trước khi khách hàng hoàn thành yêu cầu, Admin xóa mềm Mockup.
3. Khách hàng chọn “Hoàn thành”.
4. Hệ thống kiểm tra lại Mockup từ Core Database.
5. Hệ thống xác định `isDelete = true`.
6. Hệ thống không tạo yêu cầu với Mockup đó.
7. Hệ thống thông báo Mockup không còn khả dụng.
8. Khách hàng phải chọn Mockup khác còn khả dụng.

---

## Acceptance Criteria

### AC-001 — Hiển thị thao tác xóa Mockup
- **Given:** Admin có quyền quản lý Mockup.
- **When:** hệ thống hiển thị danh sách Mockup.
- **Then:** hệ thống phải hiển thị thao tác “Xóa” tại Mockup.
- **And:** Mockup chưa bị xóa mềm.

### AC-002 — Hiển thị popup xác nhận xóa
- **Given:** Admin có quyền quản lý Mockup.
- **When:** Admin chọn thao tác “Xóa”.
- **Then:** hệ thống phải hiển thị popup xác nhận.
- **And:** Mockup chưa bị xóa mềm.
- **And:** nội dung popup là “Bạn chắc chắn muốn xóa Mockup này không?”.
- **And:** popup phải có nút “Yes” để xác nhận xóa.

### AC-003 — Hủy xóa Mockup
- **Given:** hệ thống đang hiển thị popup xác nhận xóa Mockup.
- **When:** Admin chọn hủy hoặc đóng popup.
- **Then:** hệ thống phải đóng popup xác nhận.
- **And:** không cập nhật trường isDelete.
- **And:** Mockup vẫn hiển thị trong danh sách Mockup.

### AC-004 — Xóa mềm Mockup thành công
- **Given:** Admin có quyền quản lý Mockup.
- **When:** Admin chọn “Yes”.
- **Then:** hệ thống phải cập nhật trường isDelete = true của Mockup trong Core Database.
- **And:** Mockup chưa bị xóa mềm.
- **And:** hệ thống đang hiển thị popup xác nhận xóa Mockup.
- **And:** thông báo xóa Mockup thành công.
- **And:** Mockup không còn hiển thị trong danh sách Mockup mặc định.

### AC-005 — Không xóa vật lý dữ liệu Mockup
- **Given:** Admin đã xóa mềm Mockup thành công.
- **When:** hệ thống lưu thay đổi.
- **Then:** hệ thống không được xóa vật lý dữ liệu Mockup khỏi Core Database.
- **And:** không được xóa vật lý ảnh Preview của Mockup khỏi storage.

### AC-006 — Mockup đã xóa mềm không hiển thị cho khách hàng
- **Given:** Mockup đã có isDelete = true.
- **When:** khách hàng truy cập quy trình tạo mẫu hoa mới.
- **Then:** hệ thống không được hiển thị Mockup đó cho khách hàng lựa chọn.

### AC-007 — Giữ lịch sử Mockup đã được sử dụng
- **Given:** Mockup đã từng được sử dụng trong một yêu cầu tạo mẫu hoa đã tồn tại.
- **When:** Admin xóa mềm Mockup.
- **Then:** hệ thống không được làm mất liên kết giữa Mockup và các yêu cầu tạo mẫu hoa đã tồn tại.
- **And:** Dữ liệu Mockup và ảnh Preview vẫn được giữ để phục vụ xem lịch sử.
- **And:** Mockup đã bị xóa mềm không được sử dụng để generate lại hoặc tạo yêu cầu mới.
- **And:** Nếu khách hàng muốn tạo mẫu hoa mới, khách hàng phải thực hiện lại quy trình tạo mới và chọn Mockup hiện còn khả dụng.

### AC-008 — Không cho xóa Mockup đã bị xóa mềm
- **Given:** Mockup đã có isDelete = true.
- **When:** Admin hoặc hệ thống gửi yêu cầu xóa lại Mockup đó.
- **Then:** hệ thống không thực hiện cập nhật xóa lại.
- **And:** hiển thị thông báo phù hợp.

### AC-009 — Không có quyền xóa Mockup
- **Given:** Người dùng không phải Admin hoặc không có quyền quản lý Mockup.
- **When:** người dùng thực hiện thao tác xóa Mockup.
- **Then:** Hệ thống phải từ chối thao tác xóa.
- **And:** Hệ thống hiển thị thông báo phù hợp về việc người dùng không có quyền thực hiện chức năng này.
- **And:** Hệ thống không được cập nhật isDelete.
- **And:** Dữ liệu và trạng thái hiện tại của Mockup phải được giữ nguyên.

### AC-010 — Cập nhật xóa mềm thất bại
- **Given:** Admin đã xác nhận xóa Mockup.
- **When:** hệ thống không thể cập nhật trường isDelete = true trong Core Database.
- **Then:** hệ thống phải hiển thị thông báo lỗi.
- **And:** Mockup không được ghi nhận là đã xóa nếu cập nhật chưa thành công.
- **And:** hệ thống cho phép Admin thử lại.

### AC-011 — Danh sách được cập nhật sau khi xóa thành công
- **Given:** Admin đã xóa mềm Mockup thành công.
- **When:** hệ thống quay lại danh sách Mockup.
- **Then:** danh sách Mockup mặc định phải được cập nhật.
- **And:** Mockup vừa xóa không còn hiển thị trong danh sách mặc định.

### AC-012 — Mockup bị xóa trước khi khách hàng hoàn thành
- **Given:** Khách hàng đã chọn một Mockup chưa bị xóa mềm và Mockup đó đã được Admin xóa mềm trước khi khách hàng hoàn thành yêu cầu.
- **When:** Khách hàng chọn “Hoàn thành”.
- **Then:** Hệ thống phải kiểm tra lại isDelete của Mockup từ Core Database.
- **And:** Nếu isDelete = true, hệ thống không được tạo yêu cầu với Mockup đó.
- **And:** Hệ thống phải thông báo Mockup không còn khả dụng.
- **And:** Hệ thống phải yêu cầu khách hàng chọn một Mockup khác còn khả dụng.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Nguồn | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-176**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3cfc0039-7193-4c21-a52c-f2c326892a3f) | Xóa Mockup là xóa mềm | Quản lý Mockup | Xóa Mockup trong hệ thống quản trị phải được thực hiện bằng cơ chế xóa mềm. | Admin xác nhận xóa Mockup. | Hệ thống cập nhật trường isDelete = true của Mockup trong Core Database. | Hệ thống không được xóa vật lý dữ liệu Mockup hoặc ảnh Preview khi thực hiện chức năng xóa Mockup trong US này. | Core Database | Admin có quyền quản lý Mockup | STORY-053 | Draft | v0 | 2026-08-26 |
| [**BR-181**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7940d811-5cc4-4324-9950-2db88693fb66) | Mockup đã xóa không hiển thị trong danh sách mặc định | Quản lý Mockup | Mockup đã bị xóa mềm không được hiển thị trong danh sách Mockup mặc định của Admin. | Admin truy cập màn hình Quản lý Mockup. | Hệ thống chỉ hiển thị các Mockup chưa bị xóa mềm trong danh sách mặc định. | Việc xem hoặc khôi phục Mockup đã xóa, nếu có, thuộc chức năng riêng và không thuộc phạm vi US này. | Core Database | Admin có quyền quản lý Mockup | STORY-053 | Draft | v0 | 2026-08-26 |
| [**BR-182**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/62e46d0d-d5b3-4af9-9ce5-565ae25fa60c) | Mockup đã xóa không hiển thị cho khách hàng | Quản lý Mockup | Mockup đã bị xóa mềm không được hiển thị cho khách hàng trong quy trình tạo mẫu hoa mới. | Khách hàng truy cập bước chọn Mockup trong quy trình tạo mẫu hoa mới. | Hệ thống loại trừ các Mockup có isDelete = true khỏi danh sách Mockup có thể chọn. | Mockup đã xóa mềm vẫn có thể được dùng để hiển thị lịch sử hoặc dữ liệu của các yêu cầu tạo mẫu hoa đã tồn tại. | Core Database | Admin có quyền quản lý Mockup | STORY-053 | Draft | v0 | 2026-08-26 |
| [**BR-183**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942b4bdb-cd38-4332-802f-c0cb8b540541) | Giữ dữ liệu lịch sử của Mockup đã xóa | Quản lý Mockup | Việc xóa mềm Mockup không được làm mất dữ liệu lịch sử liên quan đến Mockup đó. | Mockup đã từng được sử dụng trong yêu cầu tạo mẫu hoa trước đó. | Hệ thống giữ lại dữ liệu Mockup, ảnh Preview và liên kết với các yêu cầu tạo mẫu hoa đã tồn tại. | US này không định nghĩa chức năng khôi phục Mockup đã xóa. | Core Database | Admin có quyền quản lý Mockup | STORY-053 | Draft | v0 | 2026-08-26 |
| [**BR-184**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/547bdd7d-ff22-4cb8-81d0-87fb3ff45c3e) | Xác nhận trước khi xóa Mockup | Quản lý Mockup | Admin phải xác nhận trước khi hệ thống thực hiện xóa mềm Mockup. | Admin chọn thao tác “Xóa” tại một Mockup. | Hệ thống hiển thị popup xác nhận với nội dung “Bạn chắc chắn muốn xóa Mockup này không?” và chỉ thực hiện xóa khi Admin chọn “Yes”. | Nếu Admin hủy hoặc đóng popup, hệ thống không được cập nhật trường isDelete. | Core Database | Admin có quyền quản lý Mockup | STORY-053 | Draft | v0 | 2026-08-26 |
| [**BR-186**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/90ac1fc2-0c4f-4374-b1e0-befa3f7317b4) | Tính nhất quán khi xóa mềm Mockup | Quản lý Mockup | Trạng thái xóa mềm của Mockup phải nhất quán giữa dữ liệu lưu trữ và danh sách hiển thị. | Hệ thống cập nhật isDelete = true cho Mockup. | Sau khi cập nhật thành công, danh sách Mockup mặc định phải loại bỏ Mockup đó khỏi kết quả hiển thị. | Nếu cập nhật Core Database thất bại, hệ thống không được hiển thị kết quả xóa thành công. | Core Database | Admin có quyền quản lý Mockup | STORY-053 | Draft | v0 | 2026-08-26 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-176 | [Xóa Mockup là xóa mềm](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3cfc0039-7193-4c21-a52c-f2c326892a3f) |
| BR-181 | [Mockup đã xóa không hiển thị trong danh sách mặc định](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7940d811-5cc4-4324-9950-2db88693fb66) |
| BR-182 | [Mockup đã xóa không hiển thị cho khách hàng](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/62e46d0d-d5b3-4af9-9ce5-565ae25fa60c) |
| BR-183 | [Giữ dữ liệu lịch sử của Mockup đã xóa](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942b4bdb-cd38-4332-802f-c0cb8b540541) |
| BR-184 | [Xác nhận trước khi xóa Mockup](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/547bdd7d-ff22-4cb8-81d0-87fb3ff45c3e) |
| BR-186 | [Tính nhất quán khi xóa mềm Mockup](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/90ac1fc2-0c4f-4374-b1e0-befa3f7317b4) |

### Dependencies
- [**STORY-052**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ff78292a-2763-4e9c-9348-227909eb8288)
- [**STORY-050**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/478ba2d5-04a9-4971-9800-1cd0ac446516)
- [**STORY-030**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)

---

## Non-Functional Requirements

- Hệ thống phải kiểm tra quyền Admin trước khi thực hiện cập nhật isDelete.
- Thao tác xóa mềm phải được xử lý nhất quán, không hiển thị thành công nếu Core Database chưa cập nhật thành công.
- Hệ thống không được xóa vật lý ảnh Preview hoặc dữ liệu Mockup khi thực hiện xóa mềm.
- Danh sách Mockup phải được cập nhật sau khi xóa thành công.
- Thông báo lỗi phải rõ ràng để Admin biết thao tác chưa thành công và có thể thử lại.

---

## Out of Scope

- Xóa vật lý Mockup khỏi Core Database.
- Xóa vật lý ảnh Preview khỏi storage.
- Khôi phục Mockup đã xóa.
- Xem danh sách Mockup đã xóa.
- Chỉnh sửa thông tin Mockup.
- Chuyển trạng thái Hoạt động/Không hoạt động.
- Thay đổi hoặc cập nhật các yêu cầu tạo mẫu hoa đã tồn tại.
