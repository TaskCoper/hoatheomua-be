# STORY-052 — Admin chuyển trạng thái Mockup

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý Mockup, tôi muốn chuyển trạng thái Mockup giữa Hoạt động và Không hoạt động để kiểm soát Mockup nào được phép hiển thị cho khách hàng trong quy trình khởi tạo mẫu hoa. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Đã duyệt |

> **Feedback yêu cầu sửa gần nhất:**
> "Chưa có cảnh báo/chặn khi Inactive Mockup Hoạt động cuối cùng." — *Nguyễn Đức Bình · 14:16 28/08/2026* (Đã được khắc phục trong phiên bản hiện tại bằng EXC-05 và AC-012)

---

## Context

Mockup được quản lý tại Core Database và có một trong hai trạng thái:
- **Hoạt động:** Mockup đang khả dụng và có thể được hiển thị cho khách hàng trong bước chọn Mockup nếu thỏa các điều kiện hiển thị khác của hệ thống.
- **Không hoạt động:** Mockup không còn khả dụng cho các yêu cầu tạo mẫu hoa mới và không được hiển thị cho khách hàng lựa chọn.

Admin có thể thay đổi trạng thái Mockup trực tiếp từ màn hình Quản lý Mockup. Khi Admin chọn thao tác “Chuyển sang Hoạt động” hoặc “Chuyển sang Không hoạt động”, hệ thống phải hiển thị popup xác nhận trước khi cập nhật trạng thái.
Việc chuyển Mockup sang Không hoạt động không xóa Mockup khỏi hệ thống và không làm mất liên kết với các yêu cầu tạo mẫu hoa đã sử dụng Mockup đó trước đó.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý Mockup.
- Mockup cần thao tác tồn tại trong Core Database.
- Admin đang ở màn hình Quản lý Mockup.

### Trigger
> Admin chọn thao tác “Chuyển sang Hoạt động” hoặc “Chuyển sang Không hoạt động” tại một Mockup trong danh sách.

---

## Flow

### Main Flow: Chuyển Mockup sang Không hoạt động

1. Admin xem danh sách Mockup.
2. Hệ thống hiển thị một Mockup đang ở trạng thái Hoạt động.
3. Hệ thống hiển thị thao tác “Chuyển sang Không hoạt động” cho Mockup đó.
4. Admin chọn thao tác “Chuyển sang Không hoạt động”.
5. Hệ thống hiển thị popup xác nhận chuyển Mockup sang Không hoạt động.
6. Admin xác nhận thao tác.
7. Hệ thống xác định Mockup cần thay đổi trạng thái.
8. Hệ thống kiểm tra quyền của Admin.
9. Hệ thống kiểm tra Mockup này có phải Mockup Hoạt động cuối cùng chưa bị xóa mềm hay không.
10. Hệ thống cập nhật trạng thái Mockup từ Hoạt động sang Không hoạt động.
11. Hệ thống lưu trạng thái mới vào Core Database.
12. Hệ thống thông báo cập nhật trạng thái thành công.
13. Danh sách Mockup hiển thị trạng thái mới là Không hoạt động.
14. Thao tác của Mockup được thay đổi thành “Chuyển sang Hoạt động”.
15. Mockup không còn được hiển thị cho khách hàng trong các yêu cầu tạo mẫu hoa mới.

---

### Alternative Flows

#### ALT-01 — Chuyển Mockup sang Hoạt động
1. Admin xem danh sách Mockup.
2. Hệ thống hiển thị một Mockup đang ở trạng thái Không hoạt động.
3. Hệ thống hiển thị thao tác “Chuyển sang Hoạt động” cho Mockup đó.
4. Admin chọn thao tác “Chuyển sang Hoạt động”.
5. Hệ thống hiển thị popup xác nhận chuyển Mockup sang Hoạt động.
6. Admin xác nhận thao tác.
7. Hệ thống xác định Mockup cần thay đổi trạng thái.
8. Hệ thống kiểm tra quyền của Admin.
9. Hệ thống cập nhật trạng thái Mockup từ Không hoạt động sang Hoạt động.
10. Hệ thống lưu trạng thái mới vào Core Database.
11. Hệ thống thông báo cập nhật trạng thái thành công.
12. Danh sách Mockup hiển thị trạng thái mới là Hoạt động.
13. Thao tác của Mockup được thay đổi thành “Chuyển sang Không hoạt động”.
14. Mockup trở lại trạng thái khả dụng để có thể được hiển thị cho khách hàng lựa chọn nếu thỏa các điều kiện hiển thị khác của hệ thống.

#### ALT-02 — Admin hủy popup xác nhận
1. Admin chọn thao tác “Chuyển sang Hoạt động” hoặc “Chuyển sang Không hoạt động”.
2. Hệ thống hiển thị popup xác nhận.
3. Admin chọn hủy hoặc đóng popup.
4. Hệ thống không cập nhật trạng thái Mockup.
5. Mockup giữ nguyên trạng thái trước khi thao tác.
6. Danh sách Mockup không thay đổi trạng thái của Mockup đó.

---

### Exception Flows

#### EXC-01 — Mockup không còn tồn tại
1. Admin xác nhận thao tác chuyển trạng thái một Mockup.
2. Hệ thống không tìm thấy Mockup tương ứng trong Core Database.
3. Hệ thống không thực hiện cập nhật trạng thái.
4. Hệ thống thông báo Mockup không còn tồn tại hoặc dữ liệu đã thay đổi.
5. Hệ thống tải lại danh sách Mockup.

#### EXC-02 — Lỗi cập nhật trạng thái
1. Admin xác nhận thao tác chuyển trạng thái Mockup.
2. Hệ thống gặp lỗi trong quá trình cập nhật.
3. Hệ thống không thay đổi trạng thái Mockup.
4. Mockup giữ nguyên trạng thái trước khi thao tác.
5. Hệ thống thông báo cập nhật trạng thái thất bại.
6. Admin có thể thử lại.

#### EXC-03 — Không có quyền thực hiện
1. Người dùng gửi yêu cầu chuyển trạng thái Mockup.
2. Hệ thống xác định người dùng không có quyền quản lý Mockup.
3. Hệ thống từ chối yêu cầu.
4. Hệ thống không thay đổi trạng thái Mockup.
5. Hệ thống hiển thị thông báo phù hợp.

#### EXC-04 — Mockup bị chuyển sang Không hoạt động trong lúc khách hàng đang chọn
1. Khách hàng đã chọn một Mockup khi Mockup còn Hoạt động.
2. Trước khi khách hàng hoàn thành yêu cầu tạo mẫu hoa, Admin chuyển Mockup đó sang Không hoạt động.
3. Khách hàng chọn “Hoàn thành”.
4. Hệ thống kiểm tra lại trạng thái Mockup từ Core Database.
5. Hệ thống xác định Mockup đang Không hoạt động.
6. Hệ thống không tạo yêu cầu tạo mẫu hoa với Mockup đó.
7. Hệ thống thông báo Mockup đã chọn không còn khả dụng.
8. Hệ thống yêu cầu khách hàng chọn Mockup khác.

#### EXC-05 — Không cho chuyển Mockup Hoạt động cuối cùng sang Không hoạt động
1. Admin chọn thao tác “Chuyển sang Không hoạt động” tại một Mockup đang Hoạt động.
2. Admin xác nhận popup.
3. Hệ thống kiểm tra số lượng Mockup đang Hoạt động và chưa bị xóa mềm trong Core Database.
4. Hệ thống xác định đây là Mockup Hoạt động cuối cùng.
5. Hệ thống không cập nhật trạng thái Mockup.
6. Mockup giữ nguyên trạng thái Hoạt động.
7. Hệ thống thông báo cần có ít nhất 01 Mockup Hoạt động để khách hàng có thể tạo mẫu hoa.

---

## Acceptance Criteria

### AC-001
- **Given:** Admin có quyền quản lý Mockup.
- **When:** Admin chọn thao tác “Chuyển sang Không hoạt động”.
- **Then:** Hệ thống phải hiển thị popup xác nhận trước khi cập nhật trạng thái.
- **And:** Mockup đang ở trạng thái Hoạt động.

### AC-002
- **Given:** Admin có quyền quản lý Mockup.
- **When:** Hệ thống xử lý yêu cầu.
- **Then:** Hệ thống phải cập nhật trạng thái Mockup thành Không hoạt động.
- **And:** Mockup đang ở trạng thái Hoạt động.
- **And:** Admin đã xác nhận popup chuyển sang Không hoạt động.
- **And:** Lưu trạng thái mới vào Core Database.
- **And:** Thông báo cập nhật thành công.
- **And:** Danh sách Mockup phải hiển thị trạng thái mới là Không hoạt động.

### AC-003
- **Given:** Mockup đang ở trạng thái Không hoạt động.
- **When:** Khách hàng truy cập bước chọn Mockup trong quy trình khởi tạo mẫu hoa.
- **Then:** Hệ thống không được hiển thị Mockup đó trong danh sách Mockup khả dụng.

### AC-004
- **Given:** Admin có quyền quản lý Mockup.
- **When:** Admin chọn thao tác “Chuyển sang Hoạt động”.
- **Then:** Hệ thống phải hiển thị popup xác nhận trước khi cập nhật trạng thái.
- **And:** Mockup đang ở trạng thái Không hoạt động.

### AC-005
- **Given:** Admin có quyền quản lý Mockup.
- **When:** Hệ thống xử lý yêu cầu.
- **Then:** Hệ thống phải cập nhật trạng thái Mockup thành Hoạt động.
- **And:** Mockup đang ở trạng thái Không hoạt động.
- **And:** Admin đã xác nhận popup chuyển sang Hoạt động.
- **And:** Lưu trạng thái mới vào Core Database.
- **And:** Thông báo cập nhật thành công.
- **And:** Danh sách Mockup phải hiển thị trạng thái mới là Hoạt động.

### AC-006
- **Given:** Mockup đang ở trạng thái Hoạt động.
- **When:** Khách hàng truy cập bước chọn Mockup.
- **Then:** Mockup được xem là Mockup khả dụng để hệ thống có thể đưa vào danh sách hiển thị nếu thỏa các điều kiện hiển thị khác của hệ thống.

### AC-007
- **Given:** Admin đang ở popup xác nhận chuyển trạng thái Mockup.
- **When:** Admin chọn hủy hoặc đóng popup.
- **Then:** Hệ thống không được cập nhật trạng thái Mockup.
- **And:** Mockup phải giữ nguyên trạng thái trước khi thao tác.

### AC-008
- **Given:** Khách hàng đã chọn một Mockup khi Mockup còn Hoạt động.
- **When:** Khách hàng chọn “Hoàn thành”.
- **Then:** Hệ thống phải kiểm tra lại trạng thái Mockup từ Core Database.
- **And:** Mockup được chuyển sang Không hoạt động trước khi khách hàng hoàn thành yêu cầu.
- **And:** Không được tạo yêu cầu với Mockup đã Không hoạt động.
- **And:** Phải yêu cầu khách hàng chọn Mockup khác.

### AC-009
- **Given:** Admin xác nhận thay đổi trạng thái Mockup.
- **When:** Quá trình cập nhật thất bại.
- **Then:** Mockup phải giữ nguyên trạng thái trước khi thao tác.
- **And:** Hệ thống phải thông báo cập nhật thất bại.
- **And:** Admin có thể thử lại.

### AC-010
- **Given:** Admin xác nhận thay đổi trạng thái một Mockup.
- **When:** Mockup không còn tồn tại trong Core Database.
- **Then:** Hệ thống không được cập nhật trạng thái.
- **And:** Phải thông báo dữ liệu không còn tồn tại hoặc đã thay đổi.
- **And:** Danh sách Mockup phải được tải lại.

### AC-011
- **Given:** Người dùng không có quyền quản lý Mockup.
- **When:** Người dùng gửi yêu cầu chuyển trạng thái Mockup.
- **Then:** Hệ thống phải từ chối yêu cầu.
- **And:** Không được thay đổi trạng thái Mockup.
- **And:** Hệ thống phải hiển thị thông báo phù hợp.

### AC-012 — Không cho chuyển Mockup Hoạt động cuối cùng sang Không hoạt động
- **Given:** hệ thống chỉ còn 01 Mockup đang Hoạt động và chưa bị xóa mềm.
- **When:** Admin xác nhận chuyển Mockup đó sang Không hoạt động.
- **Then:** hệ thống không cập nhật trạng thái Mockup.
- **And:** Mockup giữ nguyên trạng thái Hoạt động.
- **And:** hệ thống hiển thị thông báo cần có ít nhất 01 Mockup Hoạt động để khách hàng có thể tạo mẫu hoa.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-169**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9723aab3-235c-4aed-9784-74ad851d8b5c) | Trạng thái quản lý của Mockup | Quản lý Mockup | Mockup chỉ được có một trong hai trạng thái quản lý tại một thời điểm. | Mockup tồn tại trong hệ thống. | Mockup phải ở một trong hai trạng thái: Hoạt động hoặc Không hoạt động. Hệ thống hiển thị và xử lý đúng trạng thái hiện tại của Mockup. | Không cho phép một Mockup đồng thời ở cả trạng thái Hoạt động và Không hoạt động. | BA / Product Owner | STORY-052 | Draft | v0 | 2026-08-25 |
| [**BR-170**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8f83e5e2-0f8b-45fe-98ba-f0b3b252ae7c) | Ảnh hưởng của Mockup Không hoạt động với khách hàng | Quản lý Mockup | Mockup Không hoạt động không hiển thị cho khách hàng và không được dùng cho yêu cầu tạo mẫu hoa mới. | Khách hàng chọn Mockup hoặc hoàn thành yêu cầu tạo mẫu hoa có Mockup. | Hệ thống chỉ hiển thị và sử dụng Mockup Hoạt động. Mockup Không hoạt động phải bị loại. | Không áp dụng cho các yêu cầu tạo mẫu hoa đã tạo thành công trước khi Mockup chuyển sang Không hoạt động. | BA / Product Owner | STORY-052 | Draft | v0 | 2026-08-25 |
| [**BR-171**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c9a21221-3060-4a0e-bedf-e43b09f97205) | Ảnh hưởng của Mockup Hoạt động với khách hàng | Quản lý Mockup | Mockup Hoạt động được xem là Mockup khả dụng để hiển thị cho khách hàng. | Khách hàng chọn Mockup trong quy trình khởi tạo mẫu hoa. | Hệ thống có thể hiển thị Mockup Hoạt động nếu thỏa các điều kiện hiển thị khác. | Nếu không thỏa điều kiện khác thì không bắt buộc hiển thị dù đang Hoạt động. | BA / Product Owner | STORY-052 | Draft | v0 | 2026-08-25 |
| [**BR-172**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bcee63b0-301a-4869-a10d-ae488271c802) | Chuyển sang Không hoạt động không xóa Mockup | Quản lý Mockup | Chuyển Mockup sang Không hoạt động không xóa Mockup và không làm mất liên kết với các yêu cầu đã sử dụng Mockup. | Admin chuyển Mockup từ Hoạt động sang Không hoạt động. | Hệ thống cập nhật trạng thái, giữ nguyên bản ghi và các liên kết đã tồn tại. | Không áp dụng cho chức năng xóa Mockup. | BA / Product Owner | STORY-052 | Draft | v0 | 2026-08-25 |
| [**BR-173**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d40cd52b-de2d-4cd8-9c5e-2951b3d94df7) | Quyền thay đổi trạng thái Mockup | Quản lý Mockup | Chỉ Admin có quyền quản lý Mockup mới được chuyển trạng thái Mockup. | Người dùng gửi yêu cầu chuyển trạng thái Mockup. | Hệ thống kiểm tra quyền. Nếu có quyền quản lý Mockup thì xử lý, ngược lại từ chối. | Người dùng không có quyền quản lý Mockup không được thay đổi trạng thái Mockup trong bất kỳ trường hợp nào. | BA / Product Owner | STORY-052 | Draft | v0 | 2026-08-25 |
| [**BR-174**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0a76650e-2524-49a9-ab5f-b3df68cc03e2) | Tính nhất quán khi cập nhật trạng thái Mockup | Quản lý Mockup | Cập nhật thất bại phải giữ nguyên trạng thái trước đó, không lưu dữ liệu dở dang. | Hệ thống xử lý yêu cầu chuyển trạng thái Mockup. | Nếu thất bại, trạng thái trước đó giữ nguyên. | Không cho phép lưu một phần dữ liệu khiến hiển thị và Core Database không đồng nhất. | BA / Product Owner | STORY-052 | Draft | v0 | 2026-08-25 |
| [**BR-175**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6f062971-445e-448d-89f1-339b5baa2064) | Xác nhận trước khi thay đổi trạng thái Mockup | Quản lý Mockup | Hệ thống phải hiển thị popup xác nhận trước khi chuyển trạng thái Mockup. | Admin chọn Chuyển sang Hoạt động hoặc Không hoạt động. | Hệ thống hiển thị popup xác nhận. Chỉ khi xác nhận mới cập nhật. | Nếu hủy hoặc đóng popup, không cập nhật trạng thái. | BA / Product Owner | STORY-052 | Draft | v0 | 2026-08-25 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-169 | [Trạng thái quản lý của Mockup](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9723aab3-235c-4aed-9784-74ad851d8b5c) |
| BR-170 | [Ảnh hưởng của Mockup Không hoạt động với khách hàng](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8f83e5e2-0f8b-45fe-98ba-f0b3b252ae7c) |
| BR-171 | [Ảnh hưởng của Mockup Hoạt động với khách hàng](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c9a21221-3060-4a0e-bedf-e43b09f97205) |
| BR-172 | [Chuyển sang Không hoạt động không xóa Mockup](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bcee63b0-301a-4869-a10d-ae488271c802) |
| BR-173 | [Quyền thay đổi trạng thái Mockup](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d40cd52b-de2d-4cd8-9c5e-2951b3d94df7) |
| BR-174 | [Tính nhất quán khi cập nhật trạng thái Mockup](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0a76650e-2524-49a9-ab5f-b3df68cc03e2) |
| BR-175 | [Xác nhận trước khi thay đổi trạng thái Mockup](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6f062971-445e-448d-89f1-339b5baa2064) |

### Dependencies
- [**STORY-030**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [**STORY-050**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/478ba2d5-04a9-4971-9800-1cd0ac446516)
- [**STORY-051**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d9b0448d-1fbd-4f73-b772-e5abb947449c)

---

## Non-Functional Requirements

- Backend kiểm tra quyền Admin trước khi cập nhật trạng thái.
- Mỗi yêu cầu chỉ áp dụng đúng Mockup được chỉ định.
- Lỗi cập nhật không được lưu trạng thái dở dang hoặc không đồng nhất.
- Trạng thái trong Core Database là nguồn xác thực cuối cùng.

---

## Out of Scope

- Thêm Mockup.
- Chỉnh sửa thông diễn hoặc ảnh Mockup.
- Xóa Mockup.
- Thay thế Mockup trong các yêu cầu tạo mẫu hoa đã tồn tại.
