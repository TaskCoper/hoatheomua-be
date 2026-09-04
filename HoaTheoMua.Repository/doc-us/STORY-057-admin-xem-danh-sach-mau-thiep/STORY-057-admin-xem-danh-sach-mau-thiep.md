# STORY-057 — Admin xem danh sách mẫu thiệp

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý mẫu thiệp, tôi muốn xem danh sách mẫu thiệp để theo dõi các mẫu thiệp đang được hệ thống sử dụng. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Đã duyệt |

> **Feedback yêu cầu sửa gần nhất:**
> "US về xem danh sách mẫu thiệp mà alt-01 và các ac đều liên quan đến kích thước thiệp?" — *Nguyễn Đức Bình · 21:16 27/08/2026* (Đã được khắc phục trong phiên bản hiện tại, ALT-01 và toàn bộ AC đã chuẩn hóa thành "mẫu thiệp")

---

## Context

Mẫu thiệp được quản lý tại Core Database và được sử dụng trong quy trình tạo thiệp tại Checkout. Mỗi mẫu thiệp có:
- Tên mẫu.
- Ảnh template/ảnh Preview.
- Mô tả nếu có.
- Trạng thái Active hoặc Inactive.

Danh sách mặc định chỉ hiển thị mẫu thiệp chưa bị xóa mềm. `isDelete` là trạng thái kỹ thuật dùng cho thao tác xóa mềm và lọc dữ liệu, không hiển thị như một cột trạng thái trong danh sách.

Mẫu thiệp Inactive hoặc đã xóa mềm không được hiển thị cho khách hàng khi tạo thiệp mới.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý mẫu thiệp.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin truy cập chức năng quản lý mẫu thiệp.

---

## Flow

### Main Flow: Admin xem danh sách mẫu thiệp

1. Admin truy cập chức năng quản lý mẫu thiệp.
2. Hệ thống kiểm tra quyền quản lý mẫu thiệp của Admin.
3. Hệ thống truy xuất danh sách mẫu thiệp chưa bị xóa mềm từ Core Database.
4. Hệ thống hiển thị danh sách mẫu thiệp dưới dạng bảng.
5. Mỗi dòng hiển thị tên mẫu, ảnh Preview, mô tả nếu có, trạng thái và thao tác liên quan.
6. Admin có thể chọn ảnh Preview để xem ảnh lớn hơn bằng Image Lightbox.
7. Nếu mẫu thiệp đang Active, hệ thống hiển thị thao tác chuyển sang Inactive.
8. Nếu mẫu thiệp đang Inactive, hệ thống hiển thị thao tác chuyển sang Active.
9. Hệ thống hiển thị thêm mẫu thiệp nếu Admin có quyền phù hợp.

---

### Alternative Flows

#### ALT-01 — Danh sách mẫu thiệp rỗng
1. Hệ thống truy xuất danh sách mẫu thiệp từ Core Database thành công.
2. Không có mẫu thiệp nào có isDelete = false.
3. Hệ thống hiển thị trạng thái danh sách rỗng phù hợp.
4. Hệ thống không hiển thị thông báo lỗi tải dữ liệu.
5. Hệ thống vẫn hiển thị entry point “Thêm mẫu thiệp” cho Admin có quyền quản lý mẫu thiệp.

---

### Exception Flows

#### EXC-01 — Admin không có quyền truy cập
1. Admin không có quyền quản lý mẫu thiệp.
2. Hệ thống không hiển thị danh sách mẫu thiệp.
3. Hệ thống hiển thị thông báo phù hợp.

#### EXC-02 — Không thể tải dữ liệu
1. Hệ thống không thể truy xuất danh sách mẫu thiệp từ Core Database.
2. Hệ thống không hiển thị dữ liệu không đầy đủ như kết quả tải thành công.
3. Hệ thống hiển thị thông báo lỗi và cho phép Admin thử lại bằng nút “Thử lại”.

---

## Acceptance Criteria

### AC-001 — Hiển thị danh sách mẫu thiệp
- **Given:** Admin có quyền quản lý mẫu thiệp.
- **When:** Admin truy cập chức năng quản lý mẫu thiệp.
- **Then:** hệ thống hiển thị danh sách mẫu thiệp có isDelete = false.

### AC-002 — Hiển thị thông tin mẫu thiệp
- **Given:** hệ thống có dữ liệu mẫu thiệp chưa bị xóa mềm.
- **When:** danh sách mẫu thiệp được hiển thị.
- **Then:** mỗi dòng hiển thị tên mẫu, ảnh Preview, mô tả nếu có và trạng thái Active hoặc Inactive.

### AC-003 — Hiển thị thao tác theo trạng thái
- **Given:** mẫu thiệp có isDelete = false.
- **When:** danh sách mẫu thiệp được hiển thị.
- **Then:** nếu mẫu thiệp đang Active, hệ thống hiển thị thao tác chuyển sang Inactive.
- **And:** nếu mẫu thiệp đang Inactive, hệ thống hiển thị thao tác chuyển sang Active.

### AC-004 — Không hiển thị mẫu thiệp đã xóa mềm
- **Given:** mẫu thiệp có isDelete = true.
- **When:** Admin xem danh sách mẫu thiệp mặc định.
- **Then:** hệ thống không hiển thị mẫu thiệp đó trong danh sách.

### AC-005 — Hiển thị danh sách rỗng
- **Given:** Admin có quyền quản lý mẫu thiệp và không có mẫu thiệp nào có isDelete = false.
- **When:** Admin truy cập danh sách mẫu thiệp.
- **Then:** hệ thống hiển thị trạng thái danh sách rỗng phù hợp.
- **And:** hệ thống không hiển thị thông báo lỗi tải dữ liệu.
- **And:** hệ thống vẫn hiển thị entry point “Thêm mẫu thiệp”.

### AC-006 — Admin không có quyền truy cập
- **Given:** Admin không có quyền quản lý mẫu thiệp.
- **When:** Admin truy cập chức năng quản lý mẫu thiệp.
- **Then:** hệ thống không hiển thị danh sách mẫu thiệp.
- **And:** hệ thống hiển thị thông báo phù hợp.

### AC-007 — Không thể tải dữ liệu
- **Given:** Admin có quyền quản lý mẫu thiệp.
- **When:** hệ thống không thể truy xuất danh sách mẫu thiệp từ Core Database.
- **Then:** hệ thống không hiển thị dữ liệu không đầy đủ như kết quả tải thành công.
- **And:** hệ thống hiển thị thông báo lỗi.
- **And:** Admin có thể thực hiện “Thử lại”.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-197**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/46a5830f-c580-4564-8fc2-5a2a3576d1b9) | Nguồn dữ liệu danh sách mẫu thiệp | Quản lý mẫu thiệp | Danh sách mẫu thiệp trên màn hình quản lý của Admin phải được lấy từ Core Database. | Admin truy cập chức năng quản lý mẫu thiệp. | Hệ thống truy xuất danh sách mẫu thiệp từ Core Database và sử dụng dữ liệu này làm nguồn hiển thị chính thức. | Nếu không thể truy xuất, hệ thống không hiển thị dữ liệu không đầy đủ. | Admin có quyền quản lý mẫu thiệp | STORY-057 | Draft | v0 | 2026-08-26 |
| [**BR-198**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ed30ed6e-4368-4ac7-a0b0-b04f2ebb889b) | Thông tin hiển thị của mẫu thiệp | Quản lý mẫu thiệp | Mỗi mẫu thiệp trong danh sách quản lý phải cung cấp đủ thông tin để Admin nhận diện và theo dõi trạng thái. | Mẫu thiệp được hiển thị trong danh sách quản lý. | Hệ thống hiển thị tên mẫu, ảnh Preview, mô tả nếu có, trạng thái Active hoặc Inactive và thao tác tương ứng. | Nếu ảnh Preview không khả dụng, các thông tin còn lại của mẫu thiệp vẫn được hiển thị. | Admin có quyền quản lý mẫu thiệp | STORY-057 | Draft | v0 | 2026-08-26 |
| [**BR-199**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a733299c-59cf-4aa9-8f6e-8f90e358b7bc) | Mẫu thiệp Inactive hoặc đã xóa mềm không hiển thị cho khách hàng | Quản lý mẫu thiệp | Khách hàng chỉ được chọn mẫu thiệp đang Active và chưa bị xóa mềm khi tạo thiệp mới. | Khách hàng truy cập chức năng tạo thiệp và hệ thống tải danh sách mẫu thiệp khả dụng. | Hệ thống chỉ hiển thị mẫu thiệp có trạng thái Active và isDelete = false. | Mẫu thiệp Inactive hoặc đã xóa mềm vẫn có thể được giữ để phục vụ quản trị và lịch sử. | Admin có quyền quản lý mẫu thiệp | STORY-057, STORY-035 | Draft | v0 | 2026-08-26 |
| [**BR-200**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8a06ed0c-7ae5-436d-b08d-c6398eb88148) | Xem ảnh Preview mẫu thiệp bằng Image Lightbox | Quản lý mẫu thiệp | Admin phải có thể xem ảnh Preview mẫu thiệp với kích thước lớn hơn. | Admin chọn ảnh Preview của mẫu thiệp trong danh sách quản lý. | Hệ thống hiển thị ảnh bằng Image Lightbox. | Nếu ảnh Preview không khả dụng, hệ thống hiển thị thông báo thay vì mở ảnh lỗi. | Admin có quyền quản lý mẫu thiệp | STORY-057, STORY-035 | Draft | v0 | 2026-08-26 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-197 | [Nguồn dữ liệu danh sách mẫu thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/46a5830f-c580-4564-8fc2-5a2a3576d1b9) |
| BR-198 | [Thông tin hiển thị của mẫu thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ed30ed6e-4368-4ac7-a0b0-b04f2ebb889b) |
| BR-199 | [Mẫu thiệp Inactive hoặc đã xóa mềm không hiển thị cho khách hàng](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a733299c-59cf-4aa9-8f6e-8f90e358b7bc) |
| BR-200 | [Xem ảnh Preview mẫu thiệp bằng Image Lightbox](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8a06ed0c-7ae5-436d-b08d-c6398eb88148) |

### Dependencies
- [**STORY-035**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

---

## Non-Functional Requirements

- Danh sách phải lấy dữ liệu từ Core Database.
- Backend phải kiểm tra quyền Admin trước khi trả dữ liệu.
- Ảnh Preview không được expose đường dẫn storage nội bộ.

---

## Out of Scope

- Thêm mẫu thiệp.
- Xóa mẫu thiệp.
- Chuyển trạng thái mẫu thiệp.
- Cập nhật thông tin mẫu thiệp.
