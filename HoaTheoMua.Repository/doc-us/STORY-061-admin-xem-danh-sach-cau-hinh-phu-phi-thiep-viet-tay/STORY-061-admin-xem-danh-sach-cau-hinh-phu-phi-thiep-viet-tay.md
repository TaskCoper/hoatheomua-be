# STORY-061 — Admin xem danh sách cấu hình phụ phí thiệp viết tay

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý cấu hình giá thiệp, tôi muốn xem danh sách cấu hình phụ phí viết tay theo khoảng số lượng từ để theo dõi mức phụ phí và trạng thái của các cấu hình đang được hệ thống sử dụng. |
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

> **Feedback yêu cầu sửa gần nhất:**
> "Thay toàn bộ Context hiện tại bằng: ???? EXC - 01, 02 chưa có AC nào cover" — *Nguyễn Đức Bình · 22:00 27/08/2026*
> (Đã được khắc phục trong phiên bản hiện tại bằng cách bổ sung đầy đủ Context và bổ sung `AC-006`, `AC-007` để xử lý các luồng Exception)

---

## Context

Phụ phí viết tay được quản lý tại Core Database và được sử dụng khi tính giá thiệp Calligraphy.
Mỗi cấu hình phụ phí viết tay gồm:
- Số lượng từ bắt đầu.
- Số lượng từ kết thúc.
- Giá phụ phí.
- Trạng thái Active hoặc Inactive.

*Ví dụ: từ 1 đến 10 từ có phụ phí 20.000đ; từ 11 đến 20 từ có phụ phí 30.000đ.*

Danh sách mặc định chỉ hiển thị các cấu hình chưa bị xóa mềm. Chỉ cấu hình Active và chưa bị xóa mềm mới được sử dụng khi hệ thống xác định phụ phí cho thiệp Calligraphy.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý cấu hình giá thiệp.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin truy cập chức năng quản lý cấu hình phụ phí viết tay.

---

## Flow

### Main Flow: Admin xem danh sách cấu hình phụ phí viết tay

1. Admin truy cập chức năng cấu hình giá viết tay.
2. Hệ thống kiểm tra quyền quản lý cấu hình giá thiệp của Admin.
3. Hệ thống truy xuất các cấu hình phụ phí viết tay có `isDelete = false` từ Core Database.
4. Hệ thống hiển thị danh sách cấu hình dưới dạng bảng.
5. Mỗi dòng hiển thị số lượng từ bắt đầu, số lượng từ kết thúc, giá phụ phí và trạng thái Active/Inactive.
6. Nếu cấu hình đang Active, hệ thống hiển thị thao tác chuyển sang Inactive.
7. Nếu cấu hình đang Inactive, hệ thống hiển thị thao tác chuyển sang Active.
8. Hệ thống hiển thị các entry point quản lý liên quan như cập nhật và xóa cấu hình.
9. Hệ thống hiển thị entry point “Thêm cấu hình phụ phí viết tay” cho Admin có quyền quản lý cấu hình giá thiệp.

---

### Alternative Flows

#### ALT-01 — Danh sách cấu hình phụ phí viết tay rỗng
1. Hệ thống truy xuất dữ liệu từ Core Database thành công.
2. Không có cấu hình phụ phí viết tay nào có `isDelete = false`.
3. Hệ thống hiển thị trạng thái danh sách rỗng phù hợp.
4. Hệ thống không hiển thị thông báo lỗi tải dữ liệu.
5. Hệ thống vẫn hiển thị entry point “Thêm cấu hình phụ phí viết tay”.

---

### Exception Flows

#### EXC-01 — Admin không có quyền truy cập
1. Admin không có quyền quản lý cấu hình giá thiệp.
2. Hệ thống không hiển thị danh sách cấu hình phụ phí viết tay.
3. Hệ thống hiển thị thông báo phù hợp về việc Admin không có quyền truy cập chức năng này.

#### EXC-02 — Không thể tải dữ liệu
1. Hệ thống không thể truy xuất danh sách cấu hình phụ phí viết tay từ Core Database.
2. Hệ thống không hiển thị dữ liệu không đầy đủ như một kết quả tải thành công.
3. Hệ thống hiển thị thông báo lỗi và cho phép Admin thử lại bằng nút “Thử lại”.

---

## Acceptance Criteria

### AC-001 - Hiển thị danh sách cấu hình phụ phí viết tay
- **Given:** Admin có quyền quản lý cấu hình giá thiệp.
- **When:** Admin truy cập chức năng quản lý cấu hình phụ phí viết tay.
- **Then:** Hệ thống phải hiển thị danh sách các cấu hình có `isDelete = false`.

### AC-002 - Hiển thị thông tin cấu hình
- **Given:** Hệ thống có dữ liệu cấu hình phụ phí viết tay chưa bị xóa mềm.
- **When:** Danh sách cấu hình được hiển thị.
- **Then:** Mỗi dòng phải hiển thị số lượng từ bắt đầu, số lượng từ kết thúc, giá phụ phí và trạng thái Active/Inactive.

### AC-003 - Hiển thị thao tác theo trạng thái
- **Given:** Cấu hình phụ phí viết tay chưa bị xóa mềm.
- **When:** Danh sách cấu hình được hiển thị.
- **Then:** Nếu cấu hình đang Active, hệ thống hiển thị thao tác chuyển sang Inactive.
- **And:** Nếu cấu hình đang Inactive, hệ thống hiển thị thao tác chuyển sang Active.

### AC-004 - Không hiển thị cấu hình đã xóa mềm
- **Given:** Cấu hình phụ phí viết tay có `isDelete = true`.
- **When:** Admin xem danh sách cấu hình phụ phí viết tay mặc định.
- **Then:** Hệ thống không được hiển thị cấu hình đó trong danh sách mặc định.

### AC-005 - Danh sách rỗng
- **Given:** Admin có quyền quản lý cấu hình giá thiệp và không có cấu hình nào có `isDelete = false`.
- **When:** Admin truy cập danh sách cấu hình phụ phí viết tay.
- **Then:** Hệ thống phải hiển thị trạng thái danh sách rỗng phù hợp.
- **And:** Hệ thống không được hiển thị thông báo lỗi tải dữ liệu.
- **And:** Hệ thống vẫn hiển thị entry point “Thêm cấu hình phụ phí viết tay”.

### AC-006 - Admin không có quyền truy cập
- **Given:** Admin không có quyền quản lý cấu hình giá thiệp.
- **When:** Admin truy cập chức năng quản lý cấu hình phụ phí viết tay.
- **Then:** hệ thống không hiển thị danh sách cấu hình phụ phí viết tay.
- **And:** hệ thống hiển thị thông báo phù hợp về việc Admin không có quyền truy cập.

### AC-007 - Không thể tải dữ liệu
- **Given:** Admin có quyền quản lý cấu hình giá thiệp.
- **When:** hệ thống không thể truy xuất danh sách cấu hình phụ phí viết tay từ Core Database.
- **Then:** hệ thống không hiển thị dữ liệu không đầy đủ như kết quả tải thành công.
- **And:** hệ thống hiển thị thông báo lỗi.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-215**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e9552bc4-83eb-49de-8a63-347e4d50d597) | Nguồn dữ liệu danh sách cấu hình | Quản lý cấu hình giá thiệp | Danh sách cấu hình phụ phí viết tay phải lấy dữ liệu từ Core Database. | Admin xem danh sách. | Hệ thống truy xuất và hiển thị dữ liệu từ Core Database. | N/A | Admin có quyền quản lý cấu hình | STORY-061 | Draft | v0 | 2026-08-26 |
| [**BR-216**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c7d4c09e-f10f-4f86-bb2d-ff5b49a5a901) | Thông tin hiển thị của cấu hình | Quản lý cấu hình giá thiệp | Mỗi dòng phải cung cấp đủ số lượng từ, giá, và trạng thái. | Hiển thị danh sách cấu hình. | Hiển thị các thông tin: số lượng từ, giá phụ phí và trạng thái. | N/A | Admin có quyền quản lý cấu hình | STORY-061 | Draft | v0 | 2026-08-26 |
| [**BR-217**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d2270398-9b56-45fd-a8ca-e6b48a67c557) | Cấu hình đã xóa mềm không hiển thị | Quản lý cấu hình giá thiệp | Cấu hình có isDelete = true không được hiển thị trong danh sách mặc định. | Admin xem danh sách cấu hình. | Hệ thống chỉ trả các cấu hình có isDelete = false. | N/A | Admin có quyền quản lý cấu hình | STORY-061 | Draft | v0 | 2026-08-26 |
| [**BR-218**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2ee2a41e-61c8-445b-8c67-b63d24ebcd61) | Điều kiện áp dụng cấu hình phụ phí | Quản lý cấu hình giá thiệp | Chỉ cấu hình Active và chưa bị xóa mềm mới được tính phụ phí. | Hệ thống tính giá thiệp Calligraphy. | Áp dụng cấu hình tương ứng với điều kiện Active và isDelete = false. | N/A | Admin có quyền quản lý cấu hình | STORY-061 | Draft | v0 | 2026-08-26 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-215 | [Nguồn dữ liệu danh sách cấu hình phụ phí viết tay](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e9552bc4-83eb-49de-8a63-347e4d50d597) |
| BR-216 | [Thông tin hiển thị của cấu hình phụ phí](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c7d4c09e-f10f-4f86-bb2d-ff5b49a5a901) |
| BR-217 | [Cấu hình phụ phí đã xóa mềm không hiển thị](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d2270398-9b56-45fd-a8ca-e6b48a67c557) |
| BR-218 | [Điều kiện áp dụng cấu hình phụ phí](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2ee2a41e-61c8-445b-8c67-b63d24ebcd61) |

### Dependencies
- [**STORY-035**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [**STORY-054**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/699cfd59-a837-47c5-aa1b-dec64300c63e)

---

## Non-Functional Requirements

- Danh sách cấu hình phụ phí viết tay phải lấy dữ liệu từ Core Database.
- Backend phải kiểm tra quyền Admin trước khi trả dữ liệu.
- Hệ thống chỉ trả các cấu hình có isDelete = false cho danh sách mặc định.
- Thông báo lỗi không được chứa stack trace hoặc thông tin kỹ thuật nhạy cảm.

---

## Out of Scope

- Thêm cấu hình phụ phí viết tay.
- Cập nhật khoảng số lượng từ hoặc giá phụ phí.
- Chuyển trạng thái Active/Inactive.
- Xóa cấu hình phụ phí viết tay.
- Tính giá cuối cùng của thiệp, Checkout hoặc Order.
