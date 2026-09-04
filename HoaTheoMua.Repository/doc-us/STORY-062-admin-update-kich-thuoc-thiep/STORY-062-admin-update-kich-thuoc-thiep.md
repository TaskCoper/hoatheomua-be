# STORY-062 — Admin update kích thước thiệp

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý cấu hình thiệp, tôi muốn cập nhật thông tin kích thước thiệp đã tồn tại để điều chỉnh chiều rộng, chiều cao, giá size và số lượng từ tối đa phù hợp với cấu hình hiện hành. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Reviewer** | Nguyễn Đức Bình |
| **Approver** | Chưa chỉ định |
| **Owner** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |
| **Cập nhật** | 04/09/2026 |

---

## Context

Kích thước thiệp được quản lý tại Core Database và được sử dụng trong quy trình tạo thiệp. Admin được phép cập nhật các thông tin:
* Chiều rộng, đơn vị cm.
* Chiều cao, đơn vị cm.
* Giá size.
* Số lượng từ tối đa.

**Quy tắc dữ liệu:**
- Chiều rộng và chiều cao phải lớn hơn `0` và được phép là số thập phân.
- Hai kích thước chỉ được xem là trùng khi có cùng chiều rộng và cùng chiều cao. Ví dụ `5x7` và `7x5` là hai kích thước khác nhau.
- Giá size phải không âm (`>= 0`).
- Số lượng từ tối đa phải lớn hơn `0` và được phép là số nguyên.

Admin có thể cập nhật kích thước đang Active hoặc Inactive. STORY này không cập nhật trạng thái Active/Inactive và không thay đổi isDelete.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý cấu hình thiệp.
- Kích thước thiệp tồn tại trong Core Database.
- Kích thước thiệp có thể đang Active hoặc Inactive.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin chọn thao tác “Cập nhật” tại một kích thước thiệp trong màn hình quản lý kích thước thiệp.

---

## Flow

### Main Flow: Admin cập nhật kích thước thiệp thành công

1. Admin truy cập chức năng quản lý kích thước thiệp.
2. Hệ thống kiểm tra quyền quản lý cấu hình thiệp của Admin.
3. Hệ thống hiển thị danh sách kích thước thiệp chưa bị xóa mềm.
4. Admin chọn thao tác “Cập nhật” tại một kích thước thiệp.
5. Hệ thống hiển thị form cập nhật với dữ liệu hiện tại gồm chiều rộng, chiều cao, giá size và số lượng từ tối đa.
6. Admin chỉnh sửa một hoặc nhiều thông tin.
7. Admin chọn “Lưu”.
8. Hệ thống kiểm tra dữ liệu đầu vào.
9. Hệ thống kiểm tra trùng kích thước theo cặp chiều rộng và chiều cao với các kích thước khác có `isDelete = false`.
10. Hệ thống cập nhật thông tin kích thước thiệp trong Core Database.
11. Hệ thống thông báo cập nhật kích thước thiệp thành công.
12. Hệ thống cập nhật lại danh sách kích thước thiệp với dữ liệu mới.

---

### Alternative Flows

#### ALT-01 — Admin hủy cập nhật
1. Admin chọn hủy hoặc đóng form mà không lưu.
2. Hệ thống không cập nhật dữ liệu trong Core Database.
3. Dữ liệu kích thước thiệp được giữ nguyên.
4. Hệ thống quay lại danh sách kích thước thiệp.

---

### Exception Flows

#### EXC-01 — Dữ liệu cập nhật không hợp lệ
1. Admin nhập một hoặc nhiều dữ liệu không hợp lệ (chiều rộng/chiều cao `<= 0`; hoặc giá size `< 0`; hoặc số lượng từ tối đa `<= 0`).
2. Hệ thống không cập nhật kích thước thiệp.
3. Hệ thống hiển thị lỗi tại trường tương ứng.
4. Admin có thể chỉnh sửa dữ liệu và thử lưu lại.

#### EXC-02 — Kích thước sau cập nhật bị trùng
1. Admin thay đổi chiều rộng hoặc chiều cao của kích thước thiệp.
2. Admin chọn “Lưu”.
3. Hệ thống phát hiện một kích thước khác (không tính chính nó) có cùng chiều rộng và cùng chiều cao, bất kể kích thước đó đang Active hay Inactive.
4. Hệ thống không cập nhật kích thước thiệp.
5. Hệ thống hiển thị thông báo kích thước đã tồn tại.

#### EXC-03 — Kích thước không tồn tại hoặc đã bị xóa mềm
1. Admin mở form cập nhật kích thước thiệp.
2. Trước khi Admin lưu, kích thước không còn tồn tại hoặc đã có `isDelete = true`.
3. Admin chọn “Lưu”.
4. Hệ thống kiểm tra lại dữ liệu từ Core Database.
5. Hệ thống không thực hiện cập nhật.
6. Hệ thống hiển thị thông báo kích thước không còn khả dụng.
7. Hệ thống tải lại danh sách kích thước thiệp hiện tại.

#### EXC-04 — Lưu cập nhật thất bại
1. Admin đã nhập dữ liệu hợp lệ.
2. Admin chọn “Lưu”.
3. Hệ thống không thể cập nhật dữ liệu vào Core Database.
4. Hệ thống không ghi nhận cập nhật thành công.
5. Dữ liệu hiện tại của kích thước thiệp không được lưu ở trạng thái dở dang.
6. Hệ thống hiển thị thông báo lỗi và cho phép Admin thử lại.

#### EXC-05 — Admin không có quyền cập nhật
1. Người dùng không phải Admin hoặc không có quyền quản lý cấu hình thiệp.
2. Người dùng thực hiện yêu cầu cập nhật kích thước thiệp.
3. Hệ thống từ chối thao tác.
4. Hệ thống không cập nhật dữ liệu.
5. Hệ thống hiển thị thông báo phù hợp.

---

## Acceptance Criteria

### AC-001 — Hiển thị dữ liệu hiện tại
- **Given:** Admin có quyền quản lý cấu hình thiệp và kích thước thiệp có `isDelete = false`.
- **When:** Admin chọn thao tác “Cập nhật”.
- **Then:** Hệ thống phải hiển thị form cập nhật với dữ liệu hiện tại của kích thước thiệp.
- **And:** Form phải hiển thị chiều rộng, chiều cao, giá size và số lượng từ tối đa.

### AC-002 - Cập nhật thành công
- **Given:** Admin đang cập nhật một kích thước thiệp chưa bị xóa mềm.
- **When:** Admin lưu dữ liệu cập nhật hợp lệ.
- **Then:** Hệ thống phải cập nhật dữ liệu kích thước thiệp trong Core Database.
- **And:** Hệ thống phải thông báo cập nhật thành công.
- **And:** Danh sách kích thước thiệp phải hiển thị dữ liệu mới.

### AC-003 - Validation chiều rộng và chiều cao
- **Given:** Admin đang cập nhật chiều rộng hoặc chiều cao.
- **When:** Hệ thống kiểm tra dữ liệu.
- **Then:** Chiều rộng và chiều cao phải lớn hơn 0.
- **And:** Chiều rộng và chiều cao được phép là số thập phân.

### AC-004 - Validation giá size
- **Given:** Admin đang cập nhật giá size.
- **When:** Hệ thống kiểm tra dữ liệu.
- **Then:** Giá size phải không âm.

### AC-005 — Validation số lượng từ tối đa
- **Given:** Admin đang cập nhật số lượng từ tối đa.
- **When:** Hệ thống kiểm tra dữ liệu.
- **Then:** Số lượng từ tối đa phải lớn hơn 0.
- **And:** Số lượng từ tối đa được phép là số nguyên.

### AC-006 - Không cho cập nhật thành kích thước trùng
- **Given:** Đã tồn tại một kích thước khác có `isDelete = false`.
- **When:** Admin lưu kích thước có cùng chiều rộng và cùng chiều cao với kích thước đó.
- **Then:** Hệ thống không được cập nhật kích thước thiệp.
- **And:** Hệ thống phải thông báo kích thước đã tồn tại.
- **And:** Trạng thái Active hoặc Inactive của kích thước đã tồn tại không ảnh hưởng đến việc kiểm tra trùng.

### AC-007 - Không coi chính record đang sửa là trùng
- **Given:** Admin đang cập nhật một kích thước thiệp hiện có.
- **When:** Admin giữ nguyên chiều rộng và chiều cao nhưng thay đổi giá size hoặc số lượng từ tối đa.
- **Then:** Hệ thống không được coi chính kích thước đang cập nhật là dữ liệu trùng.
- **And:** Hệ thống cho phép lưu nếu các dữ liệu còn lại hợp lệ.

### AC-008 - Không cập nhật kích thước đã bị xóa mềm
- **Given:** Kích thước thiệp đã có `isDelete = true` trước thời điểm lưu cập nhật.
- **When:** Admin thực hiện lưu cập nhật.
- **Then:** Hệ thống phải từ chối cập nhật kích thước đó.
- **And:** Hệ thống phải hiển thị thông báo kích thước không còn khả dụng.

### AC-009 - Hủy cập nhật
- **Given:** Admin đang ở form cập nhật kích thước thiệp.
- **When:** Admin chọn hủy hoặc đóng form mà không lưu.
- **Then:** Hệ thống không được cập nhật dữ liệu trong Core Database.
- **And:** Dữ liệu kích thước thiệp phải được giữ nguyên.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-219**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d5a69f90-d6bc-4f9e-91a3-bf5d792da217) | Định dạng dữ liệu kích thước thiệp | Quản lý cấu hình thiệp | Các trường dữ liệu khi cập nhật phải đáp ứng giới hạn hợp lệ. | Admin lưu cập nhật. | Hệ thống bắt buộc: Chiều rộng/cao > 0; Giá size >= 0; Số từ tối đa > 0. | N/A | Admin có quyền quản lý | STORY-062 | Draft | v0 | 2026-08-26 |
| [**BR-220**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ed4d16d9-e26f-4a16-9e68-4c9845be85c4) | Chống trùng kích thước khi cập nhật | Quản lý cấu hình thiệp | Không được phép lưu cập nhật trùng chiều rộng và chiều cao với kích thước đã tồn tại khác. | Admin lưu kích thước. | Hệ thống từ chối cập nhật nếu trùng cặp Rộng x Cao của record có isDelete = false. | Bỏ qua kiểm tra trùng cho chính ID của record đang cập nhật. | Admin có quyền quản lý | STORY-062 | Draft | v0 | 2026-08-26 |
| [**BR-221**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/00bd8c36-85db-4424-998e-19e9c3eb79ce) | Không cập nhật kích thước đã xóa mềm | Quản lý cấu hình thiệp | Cấu hình kích thước có isDelete = true không được phép cập nhật thông tin. | Hệ thống kiểm tra trước khi lưu vào DB. | Từ chối thao tác cập nhật và báo lỗi dữ liệu không khả dụng. | N/A | Admin có quyền quản lý | STORY-062 | Draft | v0 | 2026-08-26 |
| [**BR-222**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4f625d9a-7d7d-4242-aa77-b448784d6961) | Tính toàn vẹn khi cập nhật | Quản lý cấu hình thiệp | Quá trình lưu phải đảm bảo toàn vẹn dữ liệu, không ghi đè nửa chừng nếu xảy ra lỗi. | Xảy ra lỗi kết nối hoặc database trong lúc lưu. | Hệ thống không ghi trạng thái dở dang và báo lỗi cho Admin thử lại. | N/A | Admin có quyền quản lý | STORY-062 | Draft | v0 | 2026-08-26 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-219 | [Định dạng dữ liệu kích thước thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d5a69f90-d6bc-4f9e-91a3-bf5d792da217) |
| BR-220 | [Chống trùng kích thước khi cập nhật](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ed4d16d9-e26f-4a16-9e68-4c9845be85c4) |
| BR-221 | [Không cập nhật kích thước đã xóa mềm](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/00bd8c36-85db-4424-998e-19e9c3eb79ce) |
| BR-222 | [Tính toàn vẹn khi cập nhật](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4f625d9a-7d7d-4242-aa77-b448784d6961) |

### Dependencies
- [**STORY-054**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)
- [**STORY-055**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/03ba0112-6a33-45bb-9f13-084731dd97fc)
- [**STORY-056**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a0e996bf-4b34-4e88-aca1-63512326c6fb)

---

## Non-Functional Requirements

- Backend phải kiểm tra quyền Admin trước khi cập nhật dữ liệu.
- Backend phải kiểm tra lại trạng thái `isDelete` trước khi lưu cập nhật.
- Hệ thống không được ghi dữ liệu dở dang nếu cập nhật thất bại.
- Hệ thống không được hiển thị cập nhật thành công nếu Core Database chưa cập nhật thành công.
- Backend phải bảo đảm không tạo ra trạng thái trùng kích thước khi nhiều yêu cầu thêm/cập nhật kích thước xảy ra gần như đồng thời (race condition).

---

## Out of Scope

- Thêm kích thước thiệp mới.
- Xóa kích thước thiệp.
- Chuyển trạng thái Active/Inactive.
- Khôi phục kích thước đã xóa mềm.
- Cấu hình phụ phí viết tay.
- Cập nhật trực tiếp isDelete.
