# STORY-068 — Admin cập nhật cấu hình phụ phí thiệp viết tay

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý cấu hình giá thiệp, tôi muốn cập nhật khoảng số lượng từ hoặc giá phụ phí của cấu hình phụ phí thiệp viết tay để điều chỉnh mức phụ phí áp dụng cho các thiệp Calligraphy mới. |
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
| **Cập nhật** | 27/08/2026 |

---

## Context

Mỗi cấu hình phụ phí thiệp viết tay xác định một mức giá phụ phí tương ứng với một khoảng số lượng từ. Admin có thể cập nhật:
- Số lượng từ bắt đầu.
- Số lượng từ kết thúc.
- Giá phụ phí.

**Quy tắc dữ liệu:**
- Số lượng từ bắt đầu phải nhỏ hơn hoặc bằng số lượng từ kết thúc.
- Nếu cấu hình đang Active, khoảng số lượng từ sau khi cập nhật không được chồng lấn với các cấu hình Active khác có `isDelete = false`. (Khi kiểm tra chồng lấn, phải loại trừ chính cấu hình đang được cập nhật khỏi tập so sánh).

Việc cập nhật thông tin không tự động thay đổi trạng thái Active hoặc Inactive của cấu hình.
Thông tin mới chỉ được sử dụng cho các lần xác định phụ phí mới. Việc cập nhật cấu hình không được tính lại hoặc làm thay đổi giá của thiệp, Checkout hoặc Order đã tồn tại trước thời điểm cập nhật.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang thao tác trên một cấu hình phụ phí thiệp viết tay được hiển thị trong danh sách quản lý.

### Trigger
> Admin chọn thao tác cập nhật tại một cấu hình phụ phí thiệp viết tay.

---

## Flow

### Main Flow: Admin cập nhật cấu hình phụ phí thiệp viết tay thành công

1. Admin xem danh sách cấu hình phụ phí thiệp viết tay.
2. Admin chọn thao tác cập nhật tại một cấu hình.
3. Hệ thống xác định cấu hình được chọn.
4. Hệ thống tải dữ liệu hiện tại từ Core Database.
5. Hệ thống hiển thị form với số lượng từ bắt đầu, số lượng từ kết thúc và giá phụ phí hiện tại.
6. Admin thay đổi một hoặc nhiều thông tin.
7. Admin chọn lưu.
8. Hệ thống kiểm tra dữ liệu nhập.
9. Hệ thống kiểm tra cấu hình còn tồn tại và chưa bị xóa mềm.
10. Hệ thống kiểm tra số lượng từ bắt đầu không lớn hơn số lượng từ kết thúc.
11. Hệ thống kiểm tra giá phụ phí hợp lệ và không âm.
12. Nếu cấu hình đang Active, hệ thống kiểm tra khoảng sau cập nhật không chồng lấn với các cấu hình Active khác có `isDelete = false`, không tính chính cấu hình đang được cập nhật.
13. Hệ thống cập nhật thông tin cấu hình trong Core Database.
14. Hệ thống giữ nguyên trạng thái Active/Inactive hiện tại.
15. Hệ thống thông báo cập nhật thành công.
16. Danh sách hiển thị thông tin mới.

---

### Alternative Flows

#### ALT-01 — Chỉ cập nhật giá phụ phí
1. Admin mở form cập nhật.
2. Admin chỉ thay đổi giá phụ phí.
3. Admin chọn lưu.
4. Hệ thống kiểm tra giá mới hợp lệ.
5. Hệ thống cập nhật giá phụ phí.
6. Hệ thống giữ nguyên khoảng số lượng từ và trạng thái.
7. Hệ thống thông báo cập nhật thành công.

#### ALT-02 — Cập nhật cấu hình Inactive
1. Admin cập nhật một cấu hình đang Inactive.
2. Admin thay đổi khoảng số lượng từ hoặc giá phụ phí.
3. Admin chọn lưu.
4. Hệ thống kiểm tra dữ liệu hợp lệ.
5. Hệ thống cập nhật thông tin.
6. Hệ thống giữ nguyên trạng thái Inactive.
7. Cấu hình không được sử dụng để xác định phụ phí cho thiệp Calligraphy mới cho đến khi được chuyển sang Active.

#### ALT-03 — Admin hủy cập nhật
1. Admin đang ở form cập nhật.
2. Admin chọn hủy hoặc đóng form.
3. Hệ thống không cập nhật dữ liệu.
4. Cấu hình giữ nguyên thông tin trước khi thao tác.

---

### Exception Flows

#### EXC-01 — Dữ liệu cập nhật không hợp lệ
1. Admin chọn lưu.
2. Hệ thống xác định dữ liệu thiếu hoặc không hợp lệ.
3. Hệ thống không cập nhật cấu hình.
4. Hệ thống hiển thị lỗi tại trường tương ứng.
5. Admin có thể chỉnh sửa và thử lại.

#### EXC-02 — Khoảng số lượng từ không hợp lệ
1. Admin cập nhật số lượng từ bắt đầu lớn hơn số lượng từ kết thúc.
2. Admin chọn lưu.
3. Hệ thống không cập nhật dữ liệu.
4. Hệ thống thông báo khoảng số lượng từ không hợp lệ.

#### EXC-03 — Khoảng số lượng từ Active bị chồng lấn
1. Admin cập nhật một cấu hình đang Active.
2. Khoảng mới chồng lấn với một cấu hình Active khác có `isDelete = false`.
3. Hệ thống không cập nhật dữ liệu.
4. Hệ thống hiển thị khoảng cấu hình Active đang gây chồng lấn để Admin có thể nhận biết và điều chỉnh dữ liệu.
5. Admin có thể điều chỉnh và thử lại.

#### EXC-04 — Giá phụ phí không hợp lệ
1. Admin nhập giá phụ phí không hợp lệ hoặc nhỏ hơn 0.
2. Admin chọn lưu.
3. Hệ thống không cập nhật dữ liệu.
4. Hệ thống hiển thị thông báo phù hợp.

#### EXC-05 — Cấu hình không tồn tại hoặc đã bị xóa mềm
1. Admin chọn lưu thay đổi.
2. Hệ thống không tìm thấy cấu hình trong Core Database hoặc cấu hình đã có `isDelete = true`.
3. Hệ thống không cập nhật dữ liệu.
4. Hệ thống thông báo dữ liệu không còn tồn tại hoặc đã thay đổi.
5. Hệ thống tải lại danh sách.

#### EXC-06 — Lỗi cập nhật
1. Admin gửi dữ liệu hợp lệ.
2. Hệ thống gặp lỗi trong quá trình cập nhật.
3. Hệ thống không lưu dữ liệu dở dang hoặc không đồng nhất.
4. Cấu hình giữ nguyên dữ liệu trước lần cập nhật.
5. Hệ thống thông báo cập nhật thất bại.
6. Admin có thể thử lại.

#### EXC-07 — Không có quyền thực hiện
1. Người dùng gửi yêu cầu cập nhật cấu hình phụ phí thiệp viết tay.
2. Hệ thống xác định người dùng không có quyền quản lý cấu hình giá thiệp.
3. Hệ thống từ chối yêu cầu.
4. Hệ thống không thay đổi dữ liệu.
5. Hệ thống hiển thị thông báo phù hợp.

---

## Acceptance Criteria

### AC-001 — Hiển thị dữ liệu hiện tại
- **Given:** Admin có quyền quản lý cấu hình giá thiệp và chọn cập nhật một cấu hình phụ phí thiệp viết tay.
- **When:** Form cập nhật được mở.
- **Then:** Hệ thống hiển thị số lượng từ bắt đầu, số lượng từ kết thúc và giá phụ phí hiện tại.

### AC-002 — Cập nhật thành công
- **Given:** Admin nhập dữ liệu hợp lệ.
- **When:** Admin chọn lưu.
- **Then:** Hệ thống cập nhật cấu hình trong Core Database.
- **And:** Hệ thống thông báo cập nhật thành công.
- **And:** Danh sách hiển thị thông tin mới.

### AC-003 — Không đổi trạng thái khi cập nhật
- **Given:** Cấu hình đang Active hoặc Inactive.
- **When:** Admin cập nhật thông tin thành công.
- **Then:** Hệ thống phải giữ nguyên trạng thái Active/Inactive hiện tại.

### AC-004 — Validation khoảng số lượng từ
- **Given:** Admin đang cập nhật cấu hình phụ phí thiệp viết tay.
- **When:** Số lượng từ bắt đầu lớn hơn số lượng từ kết thúc.
- **Then:** Hệ thống không được cập nhật dữ liệu.
- **And:** Hệ thống hiển thị lỗi phù hợp.

### AC-005 — Lỗi chồng lấn khoảng số lượng từ Active
- **Given:** Cấu hình đang Active.
- **When:** Admin cập nhật khoảng số lượng từ thành một khoảng chồng lấn với cấu hình Active khác có `isDelete = false`.
- **Then:** Hệ thống không được cập nhật dữ liệu.
- **And:** Hệ thống thông báo khoảng số lượng từ bị chồng lấn.
- **And:** Hệ thống phải hiển thị khoảng cấu hình Active đang gây chồng lấn để Admin có thể xác định và điều chỉnh dữ liệu.

### AC-006 — Loại trừ cấu hình hiện tại khi check chồng lấn
- **Given:** Admin cập nhật khoảng số lượng từ của một cấu hình Active.
- **When:** Hệ thống kiểm tra chồng lấn.
- **Then:** Hệ thống phải loại chính cấu hình đang được cập nhật khỏi tập dữ liệu dùng để kiểm tra chồng lấn.

### AC-007 — Validation giá phụ phí
- **Given:** Admin đang cập nhật cấu hình phụ phí thiệp viết tay.
- **When:** Giá phụ phí không hợp lệ hoặc nhỏ hơn 0.
- **Then:** Hệ thống không được cập nhật dữ liệu.
- **And:** Hệ thống hiển thị lỗi phù hợp.

### AC-008 — Hủy cập nhật
- **Given:** Admin đang ở form cập nhật.
- **When:** Admin chọn hủy hoặc đóng form.
- **Then:** Hệ thống không cập nhật dữ liệu.
- **And:** Cấu hình giữ nguyên thông tin trước khi thao tác.

### AC-009 — Lỗi dữ liệu không tồn tại/đã bị xóa
- **Given:** Admin đang cập nhật một cấu hình phụ phí thiệp viết tay.
- **When:** Cấu hình không còn tồn tại trong Core Database hoặc đã có `isDelete = true`.
- **Then:** Hệ thống không được cập nhật dữ liệu.
- **And:** Hệ thống hiển thị thông báo dữ liệu không còn tồn tại hoặc đã thay đổi.
- **And:** Hệ thống tải lại danh sách.

### AC-010 — Lỗi hệ thống khi cập nhật
- **Given:** Admin gửi dữ liệu hợp lệ.
- **When:** Quá trình cập nhật thất bại.
- **Then:** Hệ thống không được lưu dữ liệu dở dang hoặc không đồng nhất.
- **And:** Cấu hình giữ nguyên dữ liệu trước lần cập nhật.
- **And:** Hệ thống thông báo cập nhật thất bại.

### AC-011 — Không thay đổi dữ liệu lịch sử
- **Given:** Cấu hình đã từng được sử dụng để xác định phụ phí trước đó.
- **When:** Admin cập nhật cấu hình thành công.
- **Then:** Hệ thống không được tính lại hoặc thay đổi giá của thiệp, Checkout hoặc Order đã tồn tại.

### AC-012 — Kiểm tra quyền Admin
- **Given:** Người dùng không có quyền quản lý cấu hình giá thiệp.
- **When:** Người dùng gửi yêu cầu cập nhật cấu hình phụ phí thiệp viết tay.
- **Then:** Hệ thống phải từ chối yêu cầu.
- **And:** Không được thay đổi dữ liệu cấu hình.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-242**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/58ea7f6a-ef08-48df-abc7-0beed7a7a858) | Thông tin được phép cập nhật của cấu hình | Cấu hình giá thiệp | Admin được phép cập nhật số lượng từ bắt đầu, kết thúc và giá. | Admin cập nhật một cấu hình chưa bị xóa mềm. | Hệ thống cập nhật các thông tin hợp lệ trong Core Database. | Chuyển trạng thái và xóa thuộc chức năng riêng. | Admin có quyền quản lý cấu hình giá thiệp | STORY-068 | Draft | v0 | 2026-08-27 |
| [**BR-238**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e8938677-2184-4f20-ba52-446fc7ae9829) | Khoảng số lượng từ hợp lệ | Cấu hình giá thiệp | Khoảng số lượng từ của cấu hình phải hợp lệ trước khi được lưu. | Hệ thống kiểm tra dữ liệu cấu hình. | wordFrom >= 0; wordTo > 0; wordFrom <= wordTo; phải là số nguyên. | Giá trị âm, có số thập phân, không phải số hoặc wordFrom > wordTo thì không lưu. | Admin có quyền quản lý cấu hình giá thiệp | STORY-067, STORY-068 | Draft | v0 | 2026-08-27 |
| [**BR-239**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde) | Không chồng lấn khoảng số lượng từ Active | Cấu hình giá thiệp | Hai cấu hình phụ phí Active và chưa bị xóa mềm không được có khoảng chồng lấn. | Admin tạo hoặc cập nhật khoảng số lượng từ của một cấu hình Active. | Kiểm tra khoảng mới với các cấu hình Active có isDelete = false; loại trừ chính bản ghi đang cập nhật. | Cấu hình Inactive không tham gia kiểm tra chồng lấn. | Admin có quyền quản lý cấu hình giá thiệp | STORY-067, STORY-064, STORY-068 | Draft | v0 | 2026-08-27 |
| [**BR-240**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/76c98ec0-4c9f-45ef-beaa-283820ec9641) | Giá phụ phí hợp lệ | Cấu hình giá thiệp | Giá phụ phí phải là số nguyên không âm theo đơn vị VNĐ. | Admin nhập giá phụ phí. | Chỉ cho phép lưu khi giá phụ phí là số nguyên >= 0 và không có phần thập phân. | Thiếu, không phải số, số âm hoặc số thập phân thì không được lưu. | Admin có quyền quản lý cấu hình giá thiệp | STORY-067, STORY-068 | Draft | v0 | 2026-08-27 |
| [**BR-243**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f206f518-8279-40df-a4de-e8454c136af2) | Cập nhật cấu hình không thay đổi trạng thái | Quản lý cấu hình giá thiệp | Cập nhật khoảng số lượng từ/giá không được tự động thay đổi trạng thái. | Admin cập nhật cấu hình phụ phí thiệp viết tay. | Hệ thống giữ nguyên trạng thái hiện tại sau khi cập nhật thành công. | Việc chuyển trạng thái thuộc chức năng riêng. | Admin có quyền quản lý cấu hình giá thiệp | STORY-068, STORY-064 | Draft | v0 | 2026-08-27 |
| [**BR-244**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3a150e28-1a62-4b82-a8f3-c838238d674a) | Cập nhật cấu hình không làm thay đổi dữ liệu đã tồn tại | Quản lý cấu hình giá thiệp | Thay đổi cấu hình phụ phí chỉ áp dụng cho các lần xác định phụ phí mới. | Admin cập nhật cấu hình thành công. | Sử dụng dữ liệu mới cho nghiệp vụ sau cập nhật; giữ nguyên dữ liệu thiệp/Checkout/Order cũ. | Không được tự động tính lại dữ liệu lịch sử. | Admin có quyền quản lý cấu hình giá thiệp | STORY-068, STORY-035 | Draft | v0 | 2026-08-27 |
| [**BR-245**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/08765b97-4257-488b-a705-9383ae56056e) | Quyền cập nhật cấu hình phụ phí thiệp viết tay | Phân quyền | Chỉ Admin có quyền quản lý cấu hình giá thiệp mới được cập nhật cấu hình. | Người dùng gửi yêu cầu cập nhật cấu hình. | Hệ thống kiểm tra quyền trước khi cập nhật dữ liệu. | Người dùng không có quyền không được thay đổi cấu hình. | Admin có quyền quản lý cấu hình giá thiệp | STORY-068 | Draft | v0 | 2026-08-27 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-242 | [Thông tin được phép cập nhật của cấu hình phụ phí thiệp viết tay](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/58ea7f6a-ef08-48df-abc7-0beed7a7a858) |
| BR-238 | [Khoảng số lượng từ hợp lệ](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e8938677-2184-4f20-ba52-446fc7ae9829) |
| BR-239 | [Không chồng lấn khoảng số lượng từ Active](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde) |
| BR-240 | [Giá phụ phí hợp lệ](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/76c98ec0-4c9f-45ef-beaa-283820ec9641) |
| BR-243 | [Cập nhật cấu hình không thay đổi trạng thái](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f206f518-8279-40df-a4de-e8454c136af2) |
| BR-244 | [Cập nhật cấu hình không làm thay đổi dữ liệu đã tồn tại](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3a150e28-1a62-4b82-a8f3-c838238d674a) |
| BR-245 | [Quyền cập nhật cấu hình phụ phí thiệp viết tay](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/08765b97-4257-488b-a705-9383ae56056e) |

### Dependencies
- [**STORY-061**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)
- [**STORY-064**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4d819b85-3da1-4375-a27e-5f6ed558f428)
- [**STORY-035**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

---

## Non-Functional Requirements

- Backend phải kiểm tra quyền Admin trước khi cập nhật.
- Backend phải kiểm tra lại cấu hình còn tồn tại và chưa bị xóa mềm.
- Backend phải validate dữ liệu độc lập với frontend.
- Kiểm tra chồng lấn phải dựa trên dữ liệu hiện tại trong Core Database.
- Khi kiểm tra chồng lấn, phải loại bản ghi đang được cập nhật khỏi tập so sánh.
- Mỗi yêu cầu chỉ cập nhật đúng cấu hình được chỉ định.
- Lỗi cập nhật không được lưu dữ liệu dở dang hoặc không đồng nhất.
- Core Database là nguồn xác thực cuối cùng.

---

## Out of Scope

- Thêm cấu hình phụ phí thiệp viết tay.
- Xóa cấu hình phụ phí thiệp viết tay.
- Chuyển trạng thái Active/Inactive.
- Tính lại giá của thiệp, Checkout hoặc Order đã tồn tại.
