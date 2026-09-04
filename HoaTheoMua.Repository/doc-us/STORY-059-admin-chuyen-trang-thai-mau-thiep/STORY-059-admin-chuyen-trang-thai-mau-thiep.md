# STORY-059 — Admin chuyển trạng thái mẫu thiệp

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý mẫu thiệp, tôi muốn chuyển trạng thái mẫu thiệp giữa Active và Inactive để kiểm soát mẫu thiệp nào được hiển thị cho khách hàng khi tạo thiệp. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Đã duyệt |

> **Feedback yêu cầu sửa gần nhất:**
> "Trường hợp khách hàng đang dùng mẫu thiệp thì admin đổi trạng thái sẽ thế nào? Có cảnh báo nếu chuyển trạng thái cho mẫu thiệp cuối cùng từ Active sang Inactive không?" — *Nguyễn Đức Bình · 21:54 27/08/2026*
> (Đã được khắc phục trong phiên bản hiện tại bằng BR-252, ALT-03, AC-010 cho cảnh báo mẫu thiệp cuối cùng, và AC-009 cho dữ liệu đang sử dụng)

---

## Context

Mẫu thiệp có trạng thái quản lý là Active hoặc Inactive. Mẫu thiệp Active được phép hiển thị cho khách hàng khi tạo thiệp mới nếu chưa bị xóa mềm. Mẫu thiệp Inactive không được hiển thị cho khách hàng khi tạo thiệp mới, nhưng vẫn có thể hiển thị trong màn hình quản trị.

Chuyển trạng thái mẫu thiệp không xóa mẫu thiệp, không xóa ảnh template/ảnh Preview và không làm mất lịch sử thiệp, Checkout hoặc Order đã từng sử dụng mẫu thiệp đó.

Khi Admin chuyển mẫu thiệp Active cuối cùng sang Inactive, hệ thống phải cảnh báo rằng khách hàng sẽ không còn mẫu thiệp Active để lựa chọn; Admin vẫn có thể xác nhận tiếp tục hoặc hủy thao tác.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý mẫu thiệp.
- Admin đang thao tác trên một mẫu thiệp được hiển thị trong danh sách quản lý.

### Trigger
> Admin chọn thao tác chuyển trạng thái tại một mẫu thiệp trong danh sách.

---

## Flow

### Main Flow: Chuyển mẫu thiệp sang Inactive

1. Admin xem danh sách mẫu thiệp.
2. Hệ thống hiển thị một mẫu thiệp đang ở trạng thái Active.
3. Admin chọn thao tác chuyển sang Inactive tại mẫu thiệp đang Active.
4. Hệ thống hiển thị popup xác nhận chuyển mẫu thiệp sang Inactive.
5. Admin xác nhận thao tác.
6. Hệ thống xác định mẫu thiệp cần thay đổi trạng thái.
7. Hệ thống cập nhật trạng thái mẫu thiệp từ Active sang Inactive.
8. Hệ thống lưu trạng thái mới vào Core Database.
9. Hệ thống thông báo cập nhật trạng thái thành công.
10. Danh sách mẫu thiệp hiển thị trạng thái mới là Inactive.
11. Thao tác của mẫu thiệp được thay đổi thành chuyển sang Active.

---

### Alternative Flows

#### ALT-01 — Chuyển mẫu thiệp sang Active
1. Admin xem danh sách mẫu thiệp.
2. Hệ thống hiển thị một mẫu thiệp đang ở trạng thái Inactive.
3. Hệ thống hiển thị thao tác chuyển sang Active cho mẫu thiệp đó.
4. Admin chọn thao tác chuyển sang Active.
5. Hệ thống hiển thị popup xác nhận chuyển mẫu thiệp sang Active.
6. Admin xác nhận thao tác.
7. Hệ thống xác định mẫu thiệp cần thay đổi trạng thái.
8. Hệ thống cập nhật trạng thái mẫu thiệp từ Inactive sang Active.
9. Hệ thống lưu trạng thái mới vào Core Database.
10. Hệ thống thông báo cập nhật trạng thái thành công.
11. Danh sách mẫu thiệp hiển thị trạng thái mới là Active.
12. Thao tác của mẫu thiệp được thay đổi thành chuyển sang Inactive.

#### ALT-02 — Admin hủy popup xác nhận
1. Admin chọn thao tác chuyển sang Active hoặc chuyển sang Inactive.
2. Hệ thống hiển thị popup xác nhận.
3. Admin chọn hủy hoặc đóng popup.
4. Hệ thống không cập nhật trạng thái mẫu thiệp.
5. Mẫu thiệp giữ nguyên trạng thái trước khi thao tác.
6. Danh sách mẫu thiệp không thay đổi trạng thái của mẫu thiệp đó.

#### ALT-03 — Chuyển mẫu thiệp Active cuối cùng sang Inactive
1. Admin chọn chuyển một mẫu thiệp đang Active sang Inactive.
2. Hệ thống xác định đây là mẫu thiệp Active cuối cùng có isDelete = false.
3. Hệ thống hiển thị cảnh báo rằng nếu tiếp tục, khách hàng sẽ không còn mẫu thiệp Active để lựa chọn khi tạo thiệp mới.
4. Admin có thể: Chọn “Hủy” để giữ nguyên trạng thái mẫu thiệp; hoặc Xác nhận tiếp tục để chuyển mẫu thiệp sang Inactive.
5. Nếu Admin xác nhận tiếp tục, hệ thống thực hiện chuyển trạng thái theo Main Flow.

---

### Exception Flows

#### EXC-01 — Mẫu thiệp không tồn tại hoặc đã bị xóa mềm
1. Admin xác nhận thao tác chuyển trạng thái một mẫu thiệp.
2. Hệ thống không tìm thấy mẫu thiệp tương ứng trong Core Database hoặc mẫu thiệp đã bị xóa mềm.
3. Hệ thống không thực hiện cập nhật trạng thái.
4. Hệ thống thông báo mẫu thiệp không còn tồn tại hoặc dữ liệu đã thay đổi.
5. Hệ thống tải lại danh sách mẫu thiệp.

#### EXC-02 — Lỗi cập nhật trạng thái
1. Admin xác nhận thao tác chuyển trạng thái mẫu thiệp.
2. Hệ thống gặp lỗi trong quá trình cập nhật.
3. Hệ thống không thay đổi trạng thái mẫu thiệp.
4. Mẫu thiệp giữ nguyên trạng thái trước khi thao tác.
5. Hệ thống thông báo cập nhật trạng thái thất bại.
6. Admin có thể thử lại.

#### EXC-03 — Không có quyền thực hiện
1. Người dùng gửi yêu cầu chuyển trạng thái mẫu thiệp.
2. Hệ thống xác định người dùng không có quyền quản lý mẫu thiệp.
3. Hệ thống từ chối yêu cầu.
4. Hệ thống không thay đổi trạng thái mẫu thiệp.
5. Hệ thống hiển thị thông báo phù hợp.

---

## Acceptance Criteria

### AC-001 — Hiển thị popup Inactive
- **Given:** Admin có quyền quản lý mẫu thiệp và mẫu thiệp đang Active.
- **When:** Admin chọn chuyển sang Inactive.
- **Then:** hệ thống hiển thị popup xác nhận.

### AC-002 - Danh sách hiển thị trạng thái mới là Inactive
- **Given:** popup xác nhận chuyển sang Inactive đang hiển thị.
- **When:** Admin xác nhận.
- **Then:** hệ thống cập nhật trạng thái thành Inactive trong Core Database.
- **And:** thông báo thành công.
- **And:** danh sách hiển thị trạng thái mới là Inactive.

### AC-003 — Hiển thị popup Active
- **Given:** Admin có quyền quản lý mẫu thiệp và mẫu thiệp đang Inactive.
- **When:** Admin chọn chuyển sang Active.
- **Then:** hệ thống hiển thị popup xác nhận.

### AC-004 - Danh sách hiển thị trạng thái mới là Active
- **Given:** popup xác nhận chuyển sang Active đang hiển thị.
- **When:** Admin xác nhận.
- **Then:** hệ thống cập nhật trạng thái thành Active trong Core Database.
- **And:** thông báo thành công.
- **And:** danh sách hiển thị trạng thái mới là Active.

### AC-005 — Hủy chuyển trạng thái
- **Given:** Admin đang ở popup xác nhận chuyển trạng thái mẫu thiệp.
- **When:** Admin chọn hủy hoặc đóng popup.
- **Then:** Hệ thống không được cập nhật trạng thái mẫu thiệp.
- **And:** Mẫu thiệp phải giữ nguyên trạng thái trước khi thao tác.

### AC-006 — Xử lý lỗi cập nhật
- **Given:** Admin xác nhận thay đổi trạng thái mẫu thiệp.
- **When:** Quá trình cập nhật thất bại.
- **Then:** Mẫu thiệp phải giữ nguyên trạng thái trước khi thao tác.
- **And:** Hệ thống phải thông báo cập nhật trạng thái thất bại.
- **And:** Admin có thể thử lại.

### AC-007 — Mẫu thiệp không tồn tại
- **Given:** Admin đã xác nhận thay đổi trạng thái một mẫu thiệp.
- **When:** Mẫu thiệp không còn tồn tại trong Core Database hoặc đã bị xóa mềm.
- **Then:** Hệ thống không được cập nhật trạng thái.
- **And:** Hiển thị thông báo dữ liệu không còn tồn tại hoặc đã thay đổi.
- **And:** Tải lại danh sách mẫu thiệp.

### AC-008 — Lỗi phân quyền
- **Given:** Người dùng không có quyền quản lý mẫu thiệp.
- **When:** Người dùng gửi yêu cầu chuyển trạng thái mẫu thiệp.
- **Then:** Hệ thống phải từ chối yêu cầu.
- **And:** Không được thay đổi trạng thái mẫu thiệp.
- **And:** Hệ thống phải hiển thị thông báo phù hợp.

### AC-009 - Không ảnh hưởng dữ liệu đã sử dụng mẫu thiệp
- **Given:** mẫu thiệp đã được sử dụng để tạo thiệp hoặc đã liên kết với Checkout, History hoặc Order.
- **When:** Admin chuyển mẫu thiệp đó từ Active sang Inactive.
- **Then:** hệ thống không được xóa hoặc thay đổi các dữ liệu đã sử dụng mẫu thiệp trước thời điểm chuyển trạng thái.
- **And:** mẫu thiệp không còn được sử dụng cho các yêu cầu tạo thiệp mới.

### AC-010 - Cảnh báo khi chuyển mẫu thiệp Active cuối cùng
- **Given:** hệ thống chỉ còn một mẫu thiệp Active có isDelete = false.
- **When:** Admin chọn chuyển mẫu thiệp đó sang Inactive.
- **Then:** hệ thống phải cảnh báo rằng khách hàng sẽ không còn mẫu thiệp Active để lựa chọn khi tạo thiệp mới.
- **And:** Admin có thể hủy hoặc xác nhận tiếp tục thao tác.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-205**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c19549f9-7b47-4209-926d-008a0a00afb0) | Trạng thái quản lý của mẫu thiệp | Quản lý mẫu thiệp | Mỗi mẫu thiệp chỉ được có một trạng thái quản lý tại một thời điểm. | Mẫu thiệp tồn tại trong hệ thống. | Mẫu thiệp phải ở một trong hai trạng thái Active hoặc Inactive. | Không cho phép một mẫu thiệp đồng thời ở cả trạng thái Active và Inactive. | Admin có quyền quản lý mẫu thiệp | STORY-059, STORY-035 | Draft | v0 | 2026-08-26 |
| [**BR-206**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8a696b79-cdf0-4cec-89c3-b897955dfd13) | Ảnh hưởng của mẫu thiệp Active với khách hàng | Quản lý mẫu thiệp | Mẫu thiệp Active được phép hiển thị cho khách hàng khi tạo thiệp mới nếu chưa bị xóa mềm. | Khách hàng tải danh sách mẫu thiệp khả dụng. | Hệ thống có thể hiển thị mẫu thiệp Active có isDelete = false. | Mẫu thiệp Active nhưng isDelete = true không được hiển thị. | Admin có quyền quản lý mẫu thiệp | STORY-059, STORY-035 | Draft | v0 | 2026-08-26 |
| [**BR-207**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6bf44083-df90-4065-b07a-f9dcf537adff) | Ảnh hưởng của mẫu thiệp Inactive với khách hàng | Quản lý mẫu thiệp | Mẫu thiệp Inactive không được hiển thị cho khách hàng khi tạo thiệp mới. | Khách hàng tải danh sách mẫu thiệp khả dụng. | Hệ thống loại bỏ mẫu thiệp Inactive khỏi danh sách khách hàng có thể chọn. | Mẫu thiệp Inactive vẫn được giữ trong Core Database và hiển thị ở quản trị. | Admin có quyền quản lý mẫu thiệp | STORY-059, STORY-035 | Draft | v0 | 2026-08-26 |
| [**BR-208**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5dc2e025-0f1f-4848-b240-83574727fa54) | Chuyển trạng thái mẫu thiệp không xóa dữ liệu | Quản lý mẫu thiệp | Chuyển trạng thái mẫu thiệp không được làm mất dữ liệu mẫu thiệp hoặc dữ liệu lịch sử liên quan. | Admin chuyển trạng thái mẫu thiệp. | Hệ thống chỉ cập nhật trạng thái mẫu thiệp trong Core Database, không xóa ảnh hay làm mất lịch sử thiệp/Order. | N/A | Admin có quyền quản lý mẫu thiệp | STORY-059, STORY-035 | Draft | v0 | 2026-08-26 |
| [**BR-209**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ceaf3267-aaf7-4dea-a4be-649351f01f57) | Quyền chuyển trạng thái mẫu thiệp | Phân quyền | Chỉ Admin có quyền quản lý mẫu thiệp mới được chuyển trạng thái mẫu thiệp. | Người dùng yêu cầu chuyển trạng thái mẫu thiệp. | Hệ thống kiểm tra quyền trước khi cập nhật. | Người không có quyền bị từ chối cập nhật. | Admin có quyền quản lý mẫu thiệp | STORY-059, STORY-035 | Draft | v0 | 2026-08-26 |
| [**BR-252**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/84d54964-ed81-45f4-a2c0-4260614f8207) | Cảnh báo khi chuyển mẫu thiệp Active cuối cùng | Quản lý mẫu thiệp | Hệ thống phải cảnh báo khi Admin chuyển mẫu thiệp Active cuối cùng sang Inactive. | Admin chuyển Inactive một mẫu thiệp Active cuối cùng có isDelete = false. | Hệ thống hiển thị cảnh báo, Admin có thể hủy hoặc xác nhận tiếp tục. | Nếu còn mẫu thiệp Active khác, hệ thống thực hiện chuyển trạng thái thông thường không cần cảnh báo này. | Đức Bình | STORY-059 | Draft | v0 | 2026-08-28 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-205 | [Trạng thái quản lý của mẫu thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c19549f9-7b47-4209-926d-008a0a00afb0) |
| BR-206 | [Ảnh hưởng của mẫu thiệp Active với khách hàng](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8a696b79-cdf0-4cec-89c3-b897955dfd13) |
| BR-207 | [Ảnh hưởng của mẫu thiệp Inactive với khách hàng](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6bf44083-df90-4065-b07a-f9dcf537adff) |
| BR-208 | [Chuyển trạng thái mẫu thiệp không xóa dữ liệu](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5dc2e025-0f1f-4848-b240-83574727fa54) |
| BR-209 | [Quyền chuyển trạng thái mẫu thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ceaf3267-aaf7-4dea-a4be-649351f01f57) |
| BR-252 | [Cảnh báo khi chuyển mẫu thiệp Active cuối cùng](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/84d54964-ed81-45f4-a2c0-4260614f8207) |

### Dependencies
- [**STORY-057**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35e0ce5f-145f-456e-be67-f8a2b8d950e5)
- [**STORY-035**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

---

## Non-Functional Requirements

- Backend phải kiểm tra quyền Admin trước khi cập nhật trạng thái.
- Mỗi yêu cầu chỉ áp dụng đúng mẫu thiệp được chỉ định.
- Lỗi cập nhật không được lưu trạng thái dở dang hoặc không đồng nhất.
- Trạng thái trong Core Database là nguồn xác thực cuối cùng.

---

## Out of Scope

- Thêm mẫu thiệp.
- Chỉnh sửa thông tin hoặc ảnh template/ảnh Preview của mẫu thiệp.
- Xóa mẫu thiệp.
- Thay thế mẫu thiệp trong các thiệp, Checkout hoặc Order đã tồn tại.
