# STORY-054 — Admin xem danh sách kích thước thiệp

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **Phiên bản** | v0.1 |
| **User Story** | Là một Admin có quyền quản lý cấu hình thiệp, tôi muốn xem danh sách kích thước thiệp để theo dõi kích thước, giá size, số lượng từ tối đa và trạng thái của các kích thước đang được hệ thống sử dụng. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Reviewer** | Nguyễn Đức Bình |
| **Approver** | Nguyễn Đức Bình |
| **Owner** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Cập nhật** | 26/08/2026 |

---

## Context

Kích thước thiệp được quản lý tại Core Database và được sử dụng trong quy trình tạo thiệp tại Checkout.

Mỗi kích thước thiệp có:
- **Kích thước** dạng cm x cm, ví dụ 5x5, 7x7, 10x10, 14x14.
- **Giá size.**
- **Số lượng từ tối đa.**
- **Trạng thái** Active hoặc Inactive.
- **Trạng thái xóa mềm** `isDelete`.

Danh sách mặc định chỉ hiển thị kích thước thiệp chưa bị xóa mềm.

### Quy tắc tính giá thiệp

| Hình thức | Công thức giá |
|---|---|
| **Gõ máy** | Giá thiệp = Giá size |
| **Calligraphy** | Giá thiệp = Giá size + Phụ phí viết tay theo số lượng từ |

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý cấu hình thiệp.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin truy cập chức năng **quản lý kích thước thiệp**.

---

## Flow

### Main Flow — Admin xem danh sách kích thước thiệp

1. Admin truy cập chức năng quản lý kích thước thiệp.
2. Hệ thống kiểm tra quyền quản lý cấu hình thiệp của Admin.
3. Hệ thống truy xuất danh sách kích thước thiệp chưa bị xóa mềm từ Core Database.
4. Hệ thống hiển thị danh sách kích thước thiệp dưới dạng bảng.
5. Mỗi dòng hiển thị: Kích thước, Giá size, Số lượng từ tối đa, Trạng thái Active/Inactive và Thao tác liên quan.
6. Nếu kích thước đang Active, hệ thống hiển thị thao tác chuyển sang **Inactive**.
7. Nếu kích thước đang Inactive, hệ thống hiển thị thao tác chuyển sang **Active**.
8. Hệ thống hiển thị entry point **thêm kích thước thiệp** nếu Admin có quyền phù hợp.

---

### Exception Flows

#### EXC-01 — Admin không có quyền truy cập

1. Admin không có quyền quản lý cấu hình thiệp.
2. Hệ thống không hiển thị danh sách kích thước thiệp.
3. Hệ thống hiển thị thông báo phù hợp về việc Admin không có quyền truy cập chức năng này.

#### EXC-02 — Không thể tải dữ liệu

1. Hệ thống không thể truy xuất danh sách kích thước thiệp từ Core Database.
2. Hệ thống không hiển thị dữ liệu không đầy đủ như kết quả tải thành công.
3. Hệ thống hiển thị thông báo lỗi và cho phép Admin thử lại bằng nút **"Thử lại"**.

---

## Acceptance Criteria

### AC-001 — Hiển thị danh sách kích thước thiệp
- **Given:** Admin có quyền quản lý cấu hình thiệp.
- **When:** Admin truy cập chức năng quản lý kích thước thiệp.
- **Then:** Hệ thống hiển thị danh sách kích thước thiệp chưa bị xóa mềm.

### AC-002 — Hiển thị thông tin kích thước thiệp
- **Given:** Hệ thống có dữ liệu kích thước thiệp.
- **When:** Danh sách kích thước thiệp được hiển thị.
- **Then:** Mỗi dòng phải hiển thị: Kích thước, Giá size, Số lượng từ tối đa và Trạng thái Active hoặc Inactive.

### AC-003 — Hiển thị thao tác theo trạng thái
- **Given:** Kích thước thiệp chưa bị xóa mềm.
- **When:** Danh sách kích thước thiệp được hiển thị.
- **Then:** Nếu kích thước đang Active, hệ thống hiển thị thao tác chuyển sang Inactive.
- **And:** Nếu kích thước đang Inactive, hệ thống hiển thị thao tác chuyển sang Active.

### AC-004 — Không hiển thị kích thước đã xóa mềm
- **Given:** Kích thước thiệp đã có `isDelete = true`.
- **When:** Admin xem danh sách kích thước thiệp mặc định.
- **Then:** Hệ thống không hiển thị kích thước đó trong danh sách mặc định.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|
| **[BR-187](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2f1dbd3f-73c8-45de-a685-339c6ae46fbb)** | Nguồn dữ liệu kích thước thiệp | Quản lý cấu hình thiệp | Danh sách kích thước thiệp trên màn hình quản lý của Admin phải được lấy từ Core Database. | Admin truy cập chức năng quản lý kích thước thiệp. | Hệ thống truy xuất danh sách kích thước thiệp từ Core Database và sử dụng dữ liệu này làm nguồn hiển thị chính thức. | Nếu không thể truy xuất dữ liệu từ Core Database, hệ thống không được hiển thị dữ liệu không đầy đủ như một kết quả tải thành công. | Draft | v0 | 2026-08-26 |
| **[BR-190](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ff7595ae-36b8-4655-a7a4-08d559685a93)** | Giá size áp dụng cho thiệp | Cấu hình giá thiệp | Giá size là giá nền được sử dụng khi hệ thống tính giá thiệp. | Khách hàng tạo thiệp và chọn một kích thước thiệp hợp lệ. | Đối với hình thức Gõ máy, giá thiệp bằng giá size. Đối với hình thức Calligraphy, giá thiệp bằng giá size cộng với phụ phí viết tay theo số lượng từ. | Nếu size không có giá hợp lệ, hệ thống không cho tiếp tục tạo thiệp và thông báo dữ liệu giá hiện không khả dụng. | Draft | v0 | 2026-08-26 |
| **[BR-189](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/401a7585-ce52-421e-acb4-76444c24c57b)** | Kích thước thiệp đã xóa mềm không hiển thị mặc định | Quản lý cấu hình thiệp | Kích thước thiệp đã bị xóa mềm không được hiển thị trong danh sách kích thước thiệp mặc định của Admin. | Admin truy cập danh sách kích thước thiệp. | Hệ thống chỉ hiển thị các kích thước thiệp có isDelete = false. | Việc xem hoặc khôi phục kích thước thiệp đã xóa, nếu có, thuộc chức năng riêng và không thuộc phạm vi US này. | Draft | v0 | 2026-08-26 |
| **[BR-188](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/18bfed12-4045-4071-a071-eba05b86e317)** | Thông tin hiển thị của kích thước thiệp | Quản lý cấu hình thiệp | Mỗi kích thước thiệp trong danh sách quản lý phải cung cấp đủ thông tin để Admin nhận diện và theo dõi cấu hình. | Kích thước thiệp được hiển thị trong danh sách quản lý. | Hệ thống hiển thị kích thước, giá size, số lượng từ tối đa, trạng thái Active/Inactive và thao tác liên quan. | Kích thước thiệp đã bị xóa mềm không được hiển thị trong danh sách mặc định. | Draft | v0 | 2026-08-26 |

---

## References

### Business Rules
`BR-187` · `BR-188` · `BR-189` · `BR-190`

### Dependencies
- **STORY-035** — Tạo thiệp tại Checkout

---

## Non-Functional Requirements

- Danh sách phải lấy dữ liệu từ Core Database.
- Backend phải kiểm tra quyền Admin trước khi trả dữ liệu.
- Thông báo lỗi không được chứa stack trace hoặc thông tin kỹ thuật nhạy cảm.

---

## Out of Scope

- Thêm kích thước thiệp.
- Xóa kích thước thiệp.
- Chuyển trạng thái kích thước thiệp.
- Cập nhật giá size.
- Cập nhật số lượng từ tối đa.
