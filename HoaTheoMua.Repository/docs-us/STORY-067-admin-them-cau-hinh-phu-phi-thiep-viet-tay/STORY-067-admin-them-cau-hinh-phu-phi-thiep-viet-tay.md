# STORY-067 — Admin thêm cấu hình phụ phí thiệp viết tay

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **Phiên bản** | v0.1 |
| **User Story** | Là một Admin có quyền quản lý cấu hình giá thiệp, tôi muốn thêm cấu hình phụ phí thiệp viết tay theo khoảng số lượng từ để hệ thống có thể xác định đúng phụ phí cho thiệp Calligraphy. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Reviewer** | Nguyễn Đức Bình |
| **Approver** | Nguyễn Đức Bình |
| **Owner** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Cập nhật** | 28/08/2026 |

---

## Context

Phụ phí thiệp viết tay được xác định theo khoảng số lượng từ. Mỗi cấu hình phụ phí thiệp viết tay gồm:
- **Số lượng từ bắt đầu** — số nguyên không âm (cho phép `wordFrom = 0` để xử lý trường hợp khách không nhập lời chúc).
- **Số lượng từ kết thúc** — số nguyên lớn hơn 0.
- **Giá phụ phí** — số nguyên không âm theo đơn vị VNĐ (cho phép giá 0 khi khoảng số lượng từ không tính phụ phí).
- **Trạng thái** Active hoặc Inactive.

Ví dụ:
- Từ 1 đến 10 từ: phụ phí 20.000đ.
- Từ 11 đến 20 từ: phụ phí 30.000đ.

Số lượng từ bắt đầu phải nhỏ hơn hoặc bằng số lượng từ kết thúc. Số lượng từ bắt đầu/kết thúc và giá phụ phí không được là số thập phân.

Một cấu hình phụ phí đang Active không được có khoảng số lượng từ chồng lấn với một cấu hình Active khác chưa bị xóa mềm.

Cấu hình sau khi được tạo được lưu tại Core Database với `isDelete = false`. Chỉ cấu hình có trạng thái Active và `isDelete = false` mới được sử dụng khi hệ thống xác định phụ phí cho thiệp Calligraphy mới.

---

## Conditions

### Preconditions
- Admin đã đăng nhập.
- Tài khoản Admin đang hoạt động.
- Admin có quyền quản lý cấu hình giá thiệp.
- Admin đang ở chức năng quản lý cấu hình phụ phí thiệp viết tay.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Admin chọn **"Thêm cấu hình phụ phí thiệp viết tay"**.

---

## Flow

### Main Flow — Admin thêm cấu hình phụ phí thiệp viết tay thành công

1. Admin xem danh sách cấu hình phụ phí thiệp viết tay.
2. Admin chọn **"Thêm cấu hình phụ phí thiệp viết tay"**.
3. Hệ thống hiển thị form thêm mới.
4. Admin nhập số lượng từ bắt đầu.
5. Admin nhập số lượng từ kết thúc.
6. Admin nhập giá phụ phí.
7. Admin chọn trạng thái Active hoặc Inactive.
8. Admin chọn lưu.
9. Hệ thống kiểm tra dữ liệu bắt buộc và định dạng dữ liệu.
10. Hệ thống kiểm tra số lượng từ bắt đầu không lớn hơn số lượng từ kết thúc.
11. Hệ thống kiểm tra giá phụ phí hợp lệ và không âm.
12. Nếu trạng thái được chọn là Active, hệ thống kiểm tra khoảng số lượng từ không chồng lấn với các cấu hình Active có `isDelete = false`; nếu phát hiện chồng lấn, hệ thống xác định khoảng cấu hình Active đang gây xung đột để hiển thị cho Admin.
13. Hệ thống tạo cấu hình mới trong Core Database với `isDelete = false`.
14. Hệ thống thông báo thêm mới thành công.
15. Hệ thống tải lại danh sách và hiển thị cấu hình vừa tạo.

---

### Alternative Flows

#### ALT-01 — Thêm cấu hình ở trạng thái Inactive

1. Admin nhập thông tin cấu hình hợp lệ.
2. Admin chọn trạng thái **Inactive**.
3. Admin chọn lưu.
4. Hệ thống tạo cấu hình mới với trạng thái Inactive và `isDelete = false`.
5. Hệ thống thông báo thêm mới thành công.
6. Cấu hình được hiển thị trong danh sách quản lý nhưng không được sử dụng để xác định phụ phí cho thiệp Calligraphy mới.

#### ALT-02 — Admin hủy thêm mới

1. Admin đang ở form thêm cấu hình phụ phí thiệp viết tay.
2. Admin chọn hủy hoặc đóng form.
3. Hệ thống không tạo bản ghi mới.
4. Admin quay lại danh sách cấu hình phụ phí thiệp viết tay.

---

### Exception Flows

#### EXC-01 — Thiếu hoặc sai dữ liệu

1. Admin chọn lưu.
2. Hệ thống xác định một hoặc nhiều trường bắt buộc bị thiếu hoặc không hợp lệ (không phải số, là số thập phân, là số âm).
3. Hệ thống không tạo cấu hình.
4. Hệ thống hiển thị lỗi tại trường tương ứng.
5. Admin có thể chỉnh sửa và thử lại.

#### EXC-02 — Khoảng số lượng từ không hợp lệ

1. Admin nhập số lượng từ bắt đầu lớn hơn số lượng từ kết thúc.
2. Admin chọn lưu.
3. Hệ thống không tạo cấu hình.
4. Hệ thống thông báo khoảng số lượng từ không hợp lệ.
5. Admin có thể điều chỉnh dữ liệu.

#### EXC-03 — Khoảng số lượng từ Active bị chồng lấn

1. Admin tạo cấu hình ở trạng thái Active.
2. Khoảng số lượng từ mới chồng lấn với một cấu hình Active có `isDelete = false`.
3. Hệ thống không tạo cấu hình.
4. Hệ thống thông báo khoảng số lượng từ đang chồng lấn với cấu hình khác.
5. Admin có thể điều chỉnh khoảng hoặc chọn trạng thái Inactive.

#### EXC-04 — Giá phụ phí không hợp lệ

1. Admin nhập giá phụ phí không hợp lệ (không phải số, là số âm hoặc là số thập phân).
2. Admin chọn lưu.
3. Hệ thống không tạo cấu hình.
4. Hệ thống hiển thị thông báo phù hợp.

#### EXC-05 — Lỗi tạo cấu hình

1. Admin gửi dữ liệu hợp lệ.
2. Hệ thống gặp lỗi trong quá trình lưu.
3. Hệ thống không tạo bản ghi dở dang hoặc không đồng nhất.
4. Hệ thống thông báo thêm mới thất bại.
5. Admin có thể thử lại.

#### EXC-06 — Không có quyền thực hiện

1. Người dùng gửi yêu cầu thêm cấu hình phụ phí thiệp viết tay.
2. Hệ thống xác định người dùng không có quyền quản lý cấu hình giá thiệp.
3. Hệ thống từ chối yêu cầu.
4. Hệ thống không tạo dữ liệu mới.
5. Hệ thống hiển thị thông báo phù hợp.

---

## Acceptance Criteria

### AC-001 — Hiển thị form thêm cấu hình
- **Given:** Admin có quyền quản lý cấu hình giá thiệp.
- **When:** Admin chọn "Thêm cấu hình phụ phí thiệp viết tay".
- **Then:** Hệ thống hiển thị form nhập số lượng từ bắt đầu, số lượng từ kết thúc, giá phụ phí và trạng thái.

### AC-002 — Thêm cấu hình Active thành công
- **Given:** Admin nhập dữ liệu hợp lệ và khoảng số lượng từ không chồng lấn với cấu hình Active khác.
- **When:** Admin chọn trạng thái Active và lưu.
- **Then:** Hệ thống tạo cấu hình mới trong Core Database.
- **And:** Cấu hình có `isDelete = false`.
- **And:** Hệ thống thông báo thêm mới thành công.

### AC-003 — Thêm cấu hình Inactive thành công
- **Given:** Admin nhập dữ liệu hợp lệ.
- **When:** Admin chọn trạng thái Inactive và lưu.
- **Then:** Hệ thống tạo cấu hình mới ở trạng thái Inactive.
- **And:** Cấu hình có `isDelete = false`.
- **And:** Cấu hình không được sử dụng để xác định phụ phí cho thiệp Calligraphy mới.

### AC-004 — Khoảng số lượng từ không hợp lệ
- **Given:** Admin đang thêm cấu hình phụ phí thiệp viết tay.
- **When:** Số lượng từ bắt đầu lớn hơn số lượng từ kết thúc.
- **Then:** Hệ thống không được tạo cấu hình.
- **And:** Hệ thống hiển thị lỗi phù hợp.

### AC-005 — Khoảng số lượng từ Active bị chồng lấn
- **Given:** Đã tồn tại một cấu hình Active có `isDelete = false`.
- **When:** Admin tạo cấu hình Active mới có khoảng số lượng từ chồng lấn.
- **Then:** Hệ thống không được tạo cấu hình.
- **And:** Hệ thống phải thông báo khoảng số lượng từ bị chồng lấn.
- **And:** Hệ thống phải hiển thị khoảng cấu hình Active đang gây chồng lấn để Admin có thể xác định và điều chỉnh dữ liệu.

### AC-006 — Giá phụ phí không hợp lệ
- **Given:** Admin đang thêm cấu hình phụ phí thiệp viết tay.
- **When:** Giá phụ phí không hợp lệ hoặc nhỏ hơn 0.
- **Then:** Hệ thống không được tạo cấu hình.
- **And:** Hệ thống hiển thị lỗi phù hợp.

### AC-007 — Hủy thêm mới
- **Given:** Admin đang ở form thêm cấu hình phụ phí thiệp viết tay.
- **When:** Admin chọn hủy hoặc đóng form.
- **Then:** Hệ thống không tạo bản ghi mới.

### AC-008 — Lỗi tạo cấu hình
- **Given:** Admin gửi dữ liệu hợp lệ.
- **When:** Quá trình lưu thất bại.
- **Then:** Hệ thống không được tạo dữ liệu dở dang hoặc không đồng nhất.
- **And:** Hệ thống thông báo thêm mới thất bại.

### AC-009 — Không có quyền thực hiện
- **Given:** Người dùng không có quyền quản lý cấu hình giá thiệp.
- **When:** Người dùng gửi yêu cầu thêm cấu hình phụ phí thiệp viết tay.
- **Then:** Hệ thống phải từ chối yêu cầu.
- **And:** Không được tạo dữ liệu mới.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|
| **[BR-237](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d4e8c32e-f797-4896-be10-c634d810c297)** | Dữ liệu bắt buộc của cấu hình phụ phí thiệp viết tay | Cấu hình giá thiệp | Mỗi cấu hình phụ phí thiệp viết tay phải có đủ dữ liệu bắt buộc và đúng định dạng trước khi được tạo. | Admin tạo cấu hình mới. | Hệ thống yêu cầu số lượng từ bắt đầu, số lượng từ kết thúc, giá phụ phí và trạng thái; số lượng từ bắt đầu/kết thúc phải là số nguyên không âm; giá phụ phí phải là số nguyên không âm. | Dữ liệu thiếu, không phải số, là số âm hoặc là số thập phân thì không được tạo cấu hình. | Draft | v0 | 2026-08-27 |
| **[BR-238](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e8938677-2184-4f20-ba52-446fc7ae9829)** | Khoảng số lượng từ hợp lệ | Cấu hình giá thiệp | Khoảng số lượng từ của cấu hình phụ phí thiệp viết tay phải hợp lệ trước khi được lưu. | Hệ thống kiểm tra dữ liệu cấu hình. | `wordFrom` phải là số nguyên ≥ 0; `wordTo` phải là số nguyên > 0; không có phần thập phân; và `wordFrom ≤ wordTo`. | `wordFrom < 0`, `wordTo ≤ 0`, có số thập phân, ký tự không phải số hoặc `wordFrom > wordTo` thì không được lưu. | Draft | v0 | 2026-08-27 |
| **[BR-239](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/942296fb-57b0-429f-8ed5-72d919c9fdde)** | Không chồng lấn khoảng số lượng từ Active | Cấu hình giá thiệp | Hai cấu hình phụ phí Active và chưa bị xóa mềm không được có khoảng số lượng từ chồng lấn nhau. | Admin tạo cấu hình ở trạng thái Active hoặc cập nhật khoảng số lượng từ của một cấu hình Active. | Hệ thống kiểm tra khoảng mới với các cấu hình Active khác có `isDelete = false` trước khi lưu; khi cập nhật, phải loại trừ chính bản ghi đang được cập nhật. | Cấu hình Inactive không tham gia kiểm tra chồng lấn cho đến khi được chuyển sang Active. | Draft | v0 | 2026-08-27 |
| **[BR-240](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/76c98ec0-4c9f-45ef-beaa-283820ec9641)** | Giá phụ phí hợp lệ | Cấu hình giá thiệp | Giá phụ phí phải là số nguyên không âm theo đơn vị VNĐ. | Admin nhập giá phụ phí. | Hệ thống chỉ cho phép lưu khi giá phụ phí là số nguyên không âm, ≥ 0 và không có phần thập phân. | Giá thiếu, không phải số, là số âm hoặc là số thập phân thì không được lưu. | Draft | v0 | 2026-08-27 |
| **[BR-241](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/08772035-b783-438f-9129-917061951915)** | Quyền thêm cấu hình phụ phí thiệp viết tay | Phân quyền | Chỉ Admin có quyền quản lý cấu hình giá thiệp mới được thêm cấu hình phụ phí thiệp viết tay. | Người dùng gửi yêu cầu thêm cấu hình. | Hệ thống kiểm tra quyền trước khi tạo dữ liệu. | Người dùng không có quyền không được tạo cấu hình. | Draft | v0 | 2026-08-27 |

---

## References

### Business Rules
`BR-237` · `BR-238` · `BR-239` · `BR-240` · `BR-241`

### Dependencies
- **[STORY-061](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9029ff2d-0317-40d7-804a-bac81ebad922)** — Admin xem danh sách cấu hình phụ phí viết tay
- **[STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)** — Tạo thiệp cá nhân hóa bằng AI tại Checkout

---

## Non-Functional Requirements

- Backend phải kiểm tra quyền Admin trước khi tạo cấu hình.
- Backend phải validate dữ liệu độc lập với validation phía frontend.
- Kiểm tra chồng lấn phải dựa trên dữ liệu hiện tại trong Core Database.
- Mỗi yêu cầu thành công chỉ tạo một cấu hình phụ phí.
- Lỗi lưu không được tạo bản ghi dở dang hoặc không đồng nhất.
- Core Database là nguồn xác thực cuối cùng.

---

## Out of Scope

- Cập nhật cấu hình phụ phí thiệp viết tay.
- Xóa cấu hình phụ phí thiệp viết tay.
- Chuyển trạng thái Active/Inactive của cấu hình đã tồn tại.
- Tính lại giá của thiệp, Checkout hoặc Order đã tồn tại.
