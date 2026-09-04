# STORY-050 — Admin xem danh sách Mockup

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **Phiên bản** | v0.1 |
| **User Story** | Là một Admin có quyền quản lý Mockup, tôi muốn xem danh sách Mockup để theo dõi thông tin và trạng thái của các Mockup đang được quản lý trong hệ thống. |
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

Mockup được quản lý tại Core Database và được sử dụng trong quy trình khởi tạo mẫu hoa. Admin cần có chức năng xem toàn bộ Mockup đang được quản lý trong hệ thống, bao gồm Mockup ở trạng thái Active và Inactive.

Danh sách Mockup được hiển thị dưới dạng bảng, gồm:
- Tên Mockup
- Mô tả
- Ảnh Preview
- Trạng thái
- Thao tác

Danh sách Mockup có phân trang. Mặc định hiển thị 10 dòng/trang và Admin có thể chọn số hàng mỗi trang gồm 5, 10, 20, 30, 40 hoặc 50.

Danh sách được sắp xếp mặc định theo thời gian tạo giảm dần, Mockup được tạo gần nhất hiển thị trước. Story này không cung cấp chức năng sort riêng cho danh sách Mockup.

Mô tả Mockup được giới hạn vùng hiển thị trên bảng danh sách. Nếu mô tả vượt quá vùng hiển thị, hệ thống hiển thị tối đa 2 dòng và rút gọn phần nội dung vượt quá bằng dấu "…". Khi Admin di chuột vào phần mô tả bị rút gọn, hệ thống hiển thị đầy đủ nội dung bằng Tooltip.

Admin có thể chọn ảnh Preview để xem hình ảnh Mockup với kích thước lớn hơn thông qua Image Lightbox.

Story này chỉ chịu trách nhiệm hiển thị danh sách Mockup và các entry point/thao tác tương ứng gồm **"Thêm Mockup"**, **"Active"** và **"Inactive"**. Hành vi sau khi Admin chọn "Thêm Mockup", "Active" hoặc "Inactive" được xử lý tại các User Story riêng. Story này không cung cấp màn hình Chi tiết Mockup riêng.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý Mockup.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin truy cập chức năng **"Quản lý Mockup"**.

---

## Flow

### Main Flow

1. Admin truy cập chức năng **"Quản lý Mockup"**.
2. Hệ thống kiểm tra quyền truy cập chức năng quản lý Mockup của Admin.
3. Hệ thống truy xuất danh sách Mockup từ Core Database.
4. Hệ thống hiển thị danh sách Mockup dưới dạng bảng, có phân trang.
5. Mỗi dòng tương ứng với một Mockup và hiển thị: Tên Mockup, Mô tả, Ảnh Preview, Trạng thái Active hoặc Inactive, Thao tác tương ứng.
6. Nếu Mockup đang ở trạng thái Active, hệ thống hiển thị thao tác **Inactive**.
7. Nếu Mockup đang ở trạng thái Inactive, hệ thống hiển thị thao tác **Active**.
8. Hệ thống hiển thị chức năng **"Thêm Mockup"**.

---

### Alternative Flows

#### ALT-01 — Xem Preview Mockup bằng Image Lightbox

1. Admin đang xem danh sách Mockup.
2. Admin chọn ảnh Preview của một Mockup.
3. Hệ thống hiển thị Image Lightbox với hình ảnh Mockup ở kích thước lớn hơn.
4. Admin đóng Image Lightbox.
5. Hệ thống đóng Image Lightbox và Admin tiếp tục thao tác tại danh sách Mockup.

#### ALT-02 — Danh sách Mockup rỗng

1. Admin truy cập chức năng **"Quản lý Mockup"**.
2. Hệ thống truy xuất danh sách Mockup từ Core Database.
3. Không có Mockup nào trong hệ thống.
4. Hệ thống hiển thị trạng thái danh sách rỗng.
5. Hệ thống vẫn hiển thị chức năng **"Thêm Mockup"**.

#### ALT-03 — Thay đổi số hàng mỗi trang

1. Admin đang xem danh sách Mockup có phân trang.
2. Admin chọn số hàng mỗi trang là một trong các giá trị: 5, 10, 20, 30, 40 hoặc 50.
3. Hệ thống cập nhật số Mockup hiển thị trên mỗi trang theo lựa chọn của Admin.
4. Hệ thống đưa danh sách về trang đầu tiên.
5. Admin có thể chuyển trang để xem các Mockup còn lại.

---

### Exception Flows

#### EXC-01 — Không tải được danh sách Mockup

1. Admin truy cập chức năng **"Quản lý Mockup"**.
2. Hệ thống gặp lỗi khi truy xuất danh sách Mockup.
3. Hệ thống không hiển thị dữ liệu Mockup không đầy đủ hoặc sai lệch.
4. Hệ thống hiển thị thông báo không thể tải danh sách Mockup và cung cấp nút **"Thử lại"**.
5. Khi Admin chọn **"Thử lại"**, hệ thống thực hiện lại việc truy xuất danh sách Mockup.

#### EXC-02 — Không tải được ảnh Preview

1. Danh sách Mockup được tải thành công.
2. Hệ thống không thể tải ảnh Preview của một Mockup.
3. Hệ thống hiển thị trạng thái ảnh không khả dụng tại vị trí Preview của Mockup đó.
4. Khi Admin chọn vào vị trí Preview không khả dụng, hệ thống hiển thị thông báo hình ảnh không thể tải.
5. Các thông tin còn lại của Mockup vẫn được hiển thị.
6. Các Mockup khác trong danh sách không bị ảnh hưởng.

---

## Acceptance Criteria

### AC-001 — Hiển thị danh sách Mockup
- **Given:** Admin đã đăng nhập và có quyền quản lý Mockup.
- **When:** Admin truy cập chức năng "Quản lý Mockup".
- **Then:** Hệ thống phải truy xuất danh sách Mockup từ Core Database.
- **And:** Hệ thống phải hiển thị danh sách Mockup dưới dạng bảng.
- **And:** Hệ thống phải hiển thị cả Mockup Active và Mockup Inactive.
- **And:** Danh sách phải được sắp xếp theo thời gian tạo giảm dần, Mockup được tạo gần nhất hiển thị trước.

### AC-002 — Hiển thị thông tin Mockup
- **Given:** Danh sách Mockup được tải thành công.
- **When:** Hệ thống hiển thị bảng danh sách Mockup.
- **Then:** Mỗi Mockup phải được hiển thị trên một dòng riêng.
- **And:** Mỗi dòng phải hiển thị: Tên Mockup, Mô tả, Ảnh Preview, Trạng thái Active hoặc Inactive, Thao tác tương ứng.

### AC-003 — Hiển thị mô tả Mockup
- **Given:** Mockup có mô tả.
- **When:** Mô tả được hiển thị trong bảng danh sách Mockup.
- **Then:** Hệ thống phải giới hạn vùng hiển thị mô tả tối đa 2 dòng.
- **And:** Nếu mô tả vượt quá vùng hiển thị, hệ thống phải rút gọn phần nội dung vượt quá bằng dấu "…".
- **And:** Khi Admin di chuột vào phần mô tả bị rút gọn, hệ thống phải hiển thị đầy đủ nội dung bằng Tooltip.

### AC-004 — Hiển thị thao tác theo trạng thái Mockup
- **Given:** Mockup đang ở trạng thái Active hoặc Inactive.
- **When:** Mockup được hiển thị trong danh sách.
- **Then:** Mockup Active phải hiển thị thao tác **Inactive**.
- **And:** Mockup Inactive phải hiển thị thao tác **Active**.
- **And:** Việc thực hiện Active/Inactive không thuộc phạm vi User Story này.

### AC-005 — Xem Preview Mockup
- **Given:** Admin đang xem danh sách Mockup.
- **When:** Admin chọn ảnh Preview của một Mockup.
- **Then:** Hệ thống phải hiển thị Image Lightbox với hình ảnh Mockup ở kích thước lớn hơn.
- **And:** Admin phải có thể đóng Image Lightbox để tiếp tục thao tác tại danh sách.

### AC-006 — Phân trang danh sách Mockup
- **Given:** Danh sách Mockup có nhiều hơn số hàng hiển thị trên một trang.
- **When:** Admin xem danh sách Mockup.
- **Then:** Hệ thống mặc định hiển thị 10 dòng/trang.
- **And:** Admin có thể chọn số hàng mỗi trang gồm 5, 10, 20, 30, 40 hoặc 50.
- **And:** Admin có thể chuyển trang để xem các Mockup còn lại.
- **And:** Khi Admin thay đổi số hàng mỗi trang, hệ thống phải đưa danh sách về trang đầu tiên.

### AC-007 — Danh sách Mockup rỗng
- **Given:** Core Database chưa có Mockup.
- **When:** Admin truy cập chức năng "Quản lý Mockup".
- **Then:** Hệ thống phải hiển thị trạng thái danh sách rỗng.
- **And:** Không được hiển thị dữ liệu Mockup không tồn tại.
- **And:** Hệ thống vẫn phải hiển thị chức năng **"Thêm Mockup"**.

### AC-008 — Lỗi tải danh sách Mockup
- **Given:** Admin đang truy cập chức năng "Quản lý Mockup".
- **When:** Hệ thống không thể truy xuất danh sách Mockup từ Core Database.
- **Then:** Hệ thống phải hiển thị thông báo lỗi phù hợp.
- **And:** Không được hiển thị dữ liệu Mockup sai hoặc không đầy đủ.
- **And:** Hệ thống phải cung cấp nút **"Thử lại"**.
- **And:** Khi Admin chọn "Thử lại", hệ thống phải thực hiện lại việc truy xuất danh sách Mockup từ Core Database và cập nhật kết quả hiển thị theo kết quả truy xuất mới.

### AC-009 — Lỗi tải ảnh Preview
- **Given:** Danh sách Mockup được tải thành công.
- **When:** Ảnh Preview của một Mockup không thể tải.
- **Then:** Hệ thống phải hiển thị trạng thái ảnh không khả dụng tại Mockup tương ứng.
- **And:** Khi Admin chọn vào vị trí Preview không khả dụng, hệ thống phải hiển thị thông báo cho biết hình ảnh không thể tải.
- **And:** Vẫn phải hiển thị các thông tin còn lại của Mockup đó.
- **And:** Các Mockup khác vẫn phải được hiển thị bình thường.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|
| **[BR-163](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0fb3fa93-c3d3-4afb-9249-362689d34b0e)** | Quyền truy cập danh sách Mockup | Phân quyền | Chỉ Admin có quyền quản lý Mockup mới được truy cập danh sách quản lý Mockup. | Người dùng yêu cầu truy cập chức năng "Quản lý Mockup". | Hệ thống kiểm tra quyền của tài khoản trước khi trả dữ liệu quản lý Mockup. | Người dùng không có quyền quản lý Mockup không được truy cập hoặc nhận dữ liệu quản trị Mockup. | Draft | v0 | 2026-08-25 |
| **[BR-162](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/63059c03-5bd0-4090-ab0b-d3a496b53c80)** | Trạng thái Mockup | Quản lý Mockup | Mỗi Mockup chỉ được có một trạng thái quản lý tại một thời điểm. | Mockup tồn tại trong hệ thống. | Mockup phải ở một trong hai trạng thái: Active hoặc Inactive. Hệ thống hiển thị đúng trạng thái hiện tại của Mockup trên danh sách quản lý. | Không cho phép một Mockup đồng thời ở cả trạng thái Active và Inactive. | Draft | v0 | 2026-08-25 |
| **[BR-161](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc74bb24-b792-470e-b108-13d36a16d3f6)** | Hiển thị mô tả Mockup | Quản lý Mockup | Mô tả Mockup phải được giới hạn vùng hiển thị trên bảng danh sách. | Mô tả Mockup vượt quá vùng hiển thị. | Hệ thống hiển thị tối đa 2 dòng mô tả và rút gọn phần nội dung vượt quá bằng dấu "…". Khi Admin di chuột vào phần mô tả bị rút gọn, hệ thống hiển thị đầy đủ nội dung bằng Tooltip. | Nếu mô tả nằm trong giới hạn hiển thị thì hệ thống hiển thị đầy đủ và không bắt buộc hiển thị Tooltip. | Draft | v0 | 2026-08-25 |
| **[BR-160](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/534ed567-f3d3-4073-a358-37e29d6cac57)** | Thông tin hiển thị của Mockup | Quản lý Mockup | Mỗi Mockup trong danh sách quản lý phải cung cấp đủ thông tin để Admin nhận diện và theo dõi trạng thái. | Mockup được hiển thị trong danh sách quản lý. | Hệ thống hiển thị: Tên Mockup, mô tả, ảnh Preview, trạng thái Active hoặc Inactive và thao tác tương ứng. | Nếu ảnh Preview không khả dụng, các thông tin còn lại của Mockup vẫn được hiển thị. | Draft | v0 | 2026-08-25 |
| **[BR-159](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/396bf048-2243-42eb-a6c7-f364581c3ca8)** | Phạm vi Mockup hiển thị cho Admin | Quản lý Mockup | Admin được phép xem cả Mockup đang Active và Mockup đang Inactive. | Admin có quyền quản lý Mockup và truy cập danh sách Mockup. | Hệ thống hiển thị các Mockup trong hệ thống không phụ thuộc vào trạng thái Active hoặc Inactive. | Quy tắc chỉ hiển thị Mockup còn khả dụng cho khách hàng không áp dụng cho màn hình quản trị. | Draft | v0 | 2026-08-25 |
| **[BR-158](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/dbbaa80e-2369-4095-9495-387a1277c0f4)** | Nguồn dữ liệu danh sách Mockup | Quản lý Mockup | Danh sách Mockup trên màn hình quản lý của Admin phải được lấy từ Core Database. | Admin truy cập chức năng "Quản lý Mockup". | Hệ thống truy xuất danh sách Mockup từ Core Database và sử dụng dữ liệu này làm nguồn hiển thị chính thức. | Nếu không thể truy xuất dữ liệu từ Core Database, hệ thống không được hiển thị dữ liệu không đầy đủ như một kết quả tải thành công. | Draft | v0 | 2026-08-25 |

---

## References

### Business Rules
`BR-158` · `BR-159` · `BR-160` · `BR-161` · `BR-162` · `BR-163`

---

## Non-Functional Requirements

- Backend phải kiểm tra quyền truy cập trước khi trả dữ liệu quản lý Mockup.
- Lỗi tải ảnh của một Mockup không được làm lỗi toàn bộ danh sách.
- Hệ thống không được hiển thị dữ liệu Mockup không đầy đủ như một kết quả tải thành công khi việc truy xuất danh sách thất bại.

---

## Out of Scope

- Xem trang Chi tiết Mockup.
- Thêm Mockup.
- Thực hiện Active/Inactive Mockup — được xử lý tại User Story riêng.
- Chỉnh sửa Mockup.
