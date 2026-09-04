# STORY-054 — Admin xem danh sách kích thước thiệp

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý cấu hình thiệp, tôi muốn xem danh sách kích thước thiệp để theo dõi kích thước, giá size, số lượng từ tối đa và trạng thái của các kích thước đang được hệ thống sử dụng. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Đã duyệt |

> **Feedback yêu cầu sửa gần nhất:**
> "Không thấy có AC cho EXC-01 và EXC-02." — *Nguyễn Đức Bình · 20:44 27/08/2026* (Đã được khắc phục trong phiên bản hiện tại bằng AC-010 và AC-011)

---

## Context

Kích thước thiệp được quản lý tại Core Database và được sử dụng trong quy trình tạo thiệp tại Checkout. Mỗi kích thước thiệp có:
- Kích thước dạng cm x cm, ví dụ 5x5, 7x7, 10x10, 14x14.
- Giá size.
- Số lượng từ tối đa.
- Trạng thái Active hoặc Inactive.

Thiệp Gõ máy sử dụng giá size. Thiệp Calligraphy sử dụng giá size cộng với phụ phí viết tay theo số lượng từ. Danh sách kích thước thiệp hỗ trợ phân trang và lọc theo trạng thái Active/Inactive để Admin dễ dàng theo dõi và quản lý dữ liệu khi số lượng kích thước tăng lên.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý cấu hình thiệp.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin truy cập chức năng quản lý kích thước thiệp.

---

## Flow

### Main Flow: Admin xem danh sách kích thước thiệp

1. Admin truy cập chức năng quản lý kích thước thiệp.
2. Hệ thống kiểm tra quyền quản lý cấu hình thiệp của Admin.
3. Hệ thống truy xuất danh sách kích thước thiệp chưa bị xóa mềm từ Core Database.
4. Hệ thống hiển thị danh sách kích thước thiệp dưới dạng bảng.
5. Mỗi dòng hiển thị kích thước, giá size, số lượng từ tối đa, trạng thái Active/Inactive và thao tác liên quan.
6. Hệ thống hỗ trợ phân trang khi số lượng kích thước thiệp vượt quá số lượng bản ghi hiển thị trên một trang.
7. Admin có thể chuyển giữa các trang để xem các kích thước thiệp còn lại.
8. Hệ thống cho phép Admin lọc danh sách theo trạng thái: Tất cả, Active hoặc Inactive.
9. Khi Admin chọn trạng thái lọc, hệ thống chỉ hiển thị các kích thước thiệp có isActive tương ứng và isDelete = false.
10. Nếu kích thước đang Active, hệ thống hiển thị thao tác chuyển sang Inactive.
11. Nếu kích thước đang Inactive, hệ thống hiển thị thao tác chuyển sang Active.
12. Hệ thống hiển thị “Thêm kích thước thiệp” nếu Admin có quyền quản lý cấu hình thiệp.

---

### Alternative Flows

#### ALT-01 — Danh sách kích thước thiệp rỗng
1. Hệ thống truy xuất danh sách kích thước thiệp từ Core Database thành công.
2. Không có kích thước thiệp nào có isDelete = false.
3. Hệ thống hiển thị trạng thái danh sách rỗng phù hợp.
4. Hệ thống không hiển thị thông báo lỗi tải dữ liệu.
5. Hệ thống vẫn hiển thị entry point “Thêm kích thước thiệp” cho Admin có quyền quản lý cấu hình thiệp.

#### ALT-02 — Lọc danh sách theo trạng thái
1. Admin đang xem danh sách kích thước thiệp.
2. Admin chọn bộ lọc trạng thái Active hoặc Inactive.
3. Hệ thống truy xuất các kích thước thiệp có isDelete = false và isActive tương ứng với trạng thái đã chọn.
4. Hệ thống hiển thị danh sách thỏa điều kiện lọc.
5. Nếu không có dữ liệu phù hợp, hệ thống hiển thị trạng thái không có kết quả phù hợp.
6. Admin có thể chọn “Tất cả” để quay lại danh sách không lọc theo trạng thái.

#### ALT-03 — Chuyển trang danh sách
1. Danh sách kích thước thiệp có nhiều hơn số lượng bản ghi được phép hiển thị trên một trang.
2. Hệ thống hiển thị điều khiển phân trang.
3. Admin chọn một trang khác.
4. Hệ thống tải và hiển thị đúng các kích thước thiệp thuộc trang được chọn.
5. Nếu đang áp dụng bộ lọc trạng thái, điều kiện lọc được giữ nguyên khi Admin chuyển trang.

---

### Exception Flows

#### EXC-01 — Admin không có quyền truy cập
1. Admin không có quyền quản lý cấu hình thiệp.
2. Hệ thống không hiển thị danh sách kích thước thiệp.
3. Hệ thống hiển thị thông báo phù hợp về việc Admin không có quyền truy cập chức năng này.

#### EXC-02 — Không thể tải dữ liệu
1. Hệ thống không thể truy xuất danh sách kích thước thiệp từ Core Database.
2. Hệ thống không hiển thị dữ liệu không đầy đủ như kết quả tải thành công.
3. Hệ thống hiển thị thông báo lỗi và cho phép Admin thử lại bằng nút “Thử lại”.

---

## Acceptance Criteria

### AC-001 — Hiển thị danh sách kích thước thiệp
- **Given:** Admin có quyền quản lý cấu hình thiệp.
- **When:** Admin truy cập chức năng quản lý kích thước thiệp.
- **Then:** hệ thống hiển thị danh sách kích thước thiệp chưa bị xóa mềm.

### AC-002 — Hiển thị thông tin kích thước thiệp
- **Given:** hệ thống có dữ liệu kích thước thiệp.
- **When:** danh sách kích thước thiệp được hiển thị.
- **Then:** mỗi dòng phải hiển thị kích thước, giá size, số lượng từ tối đa và trạng thái Active hoặc Inactive.

### AC-003 — Hiển thị thao tác theo trạng thái
- **Given:** kích thước thiệp chưa bị xóa mềm.
- **When:** danh sách kích thước thiệp được hiển thị.
- **Then:** nếu kích thước đang Active, hệ thống hiển thị thao tác chuyển sang Inactive.
- **And:** nếu kích thước đang Inactive, hệ thống hiển thị thao tác chuyển sang Active.

### AC-004 — Không hiển thị kích thước đã xóa mềm
- **Given:** kích thước thiệp đã có isDelete = true.
- **When:** Admin xem danh sách kích thước thiệp mặc định.
- **Then:** hệ thống không hiển thị kích thước đó trong danh sách mặc định.

### AC-005 — Hiển thị danh sách rỗng
- **Given:** Admin có quyền quản lý cấu hình thiệp và không có kích thước thiệp nào có isDelete = false.
- **When:** Admin truy cập danh sách kích thước thiệp.
- **Then:** Hệ thống phải hiển thị trạng thái danh sách rỗng phù hợp.
- **And:** Hệ thống không được hiển thị thông báo lỗi tải dữ liệu.
- **And:** Hệ thống vẫn hiển thị entry point “Thêm kích thước thiệp”.

### AC-006 - Phân trang danh sách kích thước thiệp
- **Given:** Danh sách kích thước thiệp có số lượng bản ghi vượt quá giới hạn hiển thị trên một trang.
- **When:** Admin xem danh sách kích thước thiệp.
- **Then:** Hệ thống hiển thị chức năng phân trang.
- **And:** Mỗi trang chỉ hiển thị các bản ghi thuộc trang tương ứng.
- **And:** Admin có thể chuyển giữa các trang để xem các bản ghi còn lại.

### AC-007 - Lọc kích thước thiệp theo trạng thái
- **Given:** Admin đang xem danh sách kích thước thiệp.
- **When:** Admin chọn trạng thái Active hoặc Inactive.
- **Then:** Hệ thống chỉ hiển thị các kích thước thiệp có isDelete = false và isActive tương ứng với trạng thái được chọn.
- **And:** Nếu Admin chọn “Tất cả”, hệ thống hiển thị tất cả kích thước thiệp có isDelete = false.

### AC-008 - Không có kết quả theo bộ lọc
- **Given:** Admin đã chọn bộ lọc trạng thái.
- **When:** không có kích thước thiệp nào thỏa điều kiện lọc.
- **Then:** Hệ thống hoàn tất tải dữ liệu.
- **And:** hệ thống hiển thị trạng thái không có kết quả phù hợp.
- **And:** hệ thống không hiển thị thông báo lỗi tải dữ liệu.

### AC-009 - Giữ bộ lọc khi chuyển trang
- **Given:** Admin đang áp dụng bộ lọc trạng thái và danh sách kết quả có nhiều trang.
- **When:** Admin chuyển sang trang khác.
- **Then:** hệ thống giữ nguyên trạng thái lọc đã chọn.
- **And:** chỉ hiển thị dữ liệu của trang tương ứng thỏa điều kiện lọc.

### AC-010 - Admin không có quyền truy cập
- **Given:** Admin không có quyền quản lý cấu hình thiệp.
- **When:** Admin truy cập chức năng quản lý kích thước thiệp.
- **Then:** hệ thống không hiển thị danh sách kích thước thiệp.
- **And:** Hệ thống hiển thị thông báo phù hợp về việc Admin không có quyền truy cập.

### AC-011 - Không thể tải dữ liệu
- **Given:** Admin có quyền quản lý cấu hình thiệp.
- **When:** hệ thống không thể truy xuất danh sách kích thước thiệp từ Core Database.
- **Then:** hệ thống hiển thị thông báo lỗi và không hiển thị dữ liệu không đầy đủ như kết quả tải thành công.
- **And:** hệ thống cho phép Admin thực hiện “Thử lại”.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-187**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2f1dbd3f-73c8-45de-a685-339c6ae46fbb) | Nguồn dữ liệu kích thước thiệp | Quản lý cấu hình thiệp | Danh sách kích thước thiệp trên màn hình quản lý của Admin phải được lấy từ Core Database. | Admin truy cập chức năng quản lý kích thước thiệp. | Hệ thống truy xuất danh sách kích thước thiệp từ Core Database và sử dụng dữ liệu này làm nguồn hiển thị chính thức. | Nếu không thể truy xuất, hệ thống không được hiển thị dữ liệu không đầy đủ. | Admin có quyền quản lý cấu hình thiệp | STORY-054 | Draft | v0 | 2026-08-26 |
| [**BR-188**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/18bfed12-4045-4071-a071-eba05b86e317) | Thông tin hiển thị của kích thước thiệp | Quản lý cấu hình thiệp | Mỗi kích thước thiệp trong danh sách quản lý phải cung cấp đủ thông tin để Admin nhận diện và theo dõi cấu hình. | Kích thước thiệp được hiển thị trong danh sách quản lý. | Hệ thống hiển thị kích thước, giá size, số lượng từ tối đa, trạng thái Active/Inactive và thao tác liên quan. | Kích thước thiệp đã bị xóa mềm không được hiển thị trong danh sách mặc định. | Admin có quyền quản lý cấu hình thiệp | STORY-054 | Draft | v0 | 2026-08-26 |
| [**BR-189**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/401a7585-ce52-421e-acb4-76444c24c57b) | Kích thước thiệp đã xóa mềm không hiển thị mặc định | Quản lý cấu hình thiệp | Kích thước thiệp đã bị xóa mềm không được hiển thị trong danh sách kích thước thiệp mặc định của Admin. | Admin truy cập danh sách kích thước thiệp. | Hệ thống chỉ hiển thị các kích thước thiệp có isDelete = false. | Việc xem hoặc khôi phục kích thước thiệp đã xóa, nếu có, thuộc chức năng riêng. | Admin có quyền quản lý cấu hình thiệp | STORY-054 | Draft | v0 | 2026-08-26 |
| [**BR-190**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ff7595ae-36b8-4655-a7a4-08d559685a93) | Giá size áp dụng cho thiệp | Cấu hình giá thiệp | Giá size là giá nền được sử dụng khi hệ thống tính giá thiệp. | Khách hàng tạo thiệp và chọn một kích thước thiệp hợp lệ. | Đối với Gõ máy, giá thiệp = giá size. Đối với Calligraphy, giá thiệp = giá size + phụ phí viết tay theo số lượng từ. | Nếu size không có giá hợp lệ, hệ thống không cho tiếp tục tạo thiệp. | Admin có quyền quản lý cấu hình thiệp | STORY-054 | Draft | v0 | 2026-08-26 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-187 | [Nguồn dữ liệu kích thước thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2f1dbd3f-73c8-45de-a685-339c6ae46fbb) |
| BR-188 | [Thông tin hiển thị của kích thước thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/18bfed12-4045-4071-a071-eba05b86e317) |
| BR-189 | [Kích thước thiệp đã xóa mềm không hiển thị mặc định](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/401a7585-ce52-421e-acb4-76444c24c57b) |
| BR-190 | [Giá size áp dụng cho thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ff7595ae-36b8-4655-a7a4-08d559685a93) |

### Dependencies
- [**STORY-035**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

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
