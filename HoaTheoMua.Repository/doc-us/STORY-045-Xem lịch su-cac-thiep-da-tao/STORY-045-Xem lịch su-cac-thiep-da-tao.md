# STORY-045 — Khách hàng xem lịch sử các thiệp đã tạo

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã sử dụng chức năng tạo thiệp AI, tôi muốn xem lại tất cả thiệp có ảnh đã tạo, để xem thông tin chi tiết và truy cập các thao tác liên quan đến thiệp đã tạo. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Chức năng được truy cập từ mục **“Thiệp thiết kế”** trên sidebar. History hiển thị dạng bảng.

| Ảnh | Mã thiệp | Thông tin thiệp | Thời điểm tạo | Thao tác |
| --- | --- | --- | --- | --- |
| Preview | CARD-00125 | Cổ điển · 10×15 · Viết tay | 17/08/2026 10:30 | Xem chi tiết |

**Một thiệp có thể:**
- Chưa từng liên kết với Order.
- Đang liên kết với một Order.
- Được tái sử dụng và có nhiều record liên kết Thiệp–Order.

- Mỗi History item đại diện cho đúng một lần generate có ảnh output. 
- Việc chọn lại hoặc tải xuống không tạo History item mới. 
- Generate không có ảnh output không xuất hiện trong History.

---

## Conditions

### Preconditions
- Khách hàng đã đăng nhập.

### Trigger
> Khách hàng mở **“Thiệp thiết kế”**.

---

## Flow

### Main Flow: Xem danh sách

1. Khách hàng mở “Thiệp thiết kế”.
2. Backend lấy các History record có ảnh output thuộc khách hàng hiện tại.
3. Hệ thống sắp xếp theo thời điểm generate, mới nhất trước.
4. Hệ thống hiển thị mỗi thiệp với: Ảnh preview, Mã thiệp, Thông tin thiệp, Thời điểm tạo, Thao tác.
5. Nếu thiệp có liên kết Order, hệ thống có thể hiển thị các mã Order liên quan.
6. Nếu chưa liên kết Order, hệ thống vẫn hiển thị bình thường.
7. Khách hàng chọn một thiệp để xem chi tiết.
8. Màn chi tiết hiển thị: Ảnh, Metadata còn khả dụng, Các Order liên quan, Chức năng tải xuống và Chức năng tạo lại thiệp.

---

### Alternative Flows

#### ALT-01 — Thiệp chưa thuộc Order
1. Khách hàng mở danh sách hoặc chi tiết một thiệp History.
2. Hệ thống xác định thiệp chưa liên kết với Order nào.
3. Hệ thống vẫn hiển thị thiệp trong History.
4. Hệ thống hiển thị: Các thông tin metadata, Trạng thái chưa liên kết đơn hàng (FE vẫn hiển thị trường nhưng để trống).

#### ALT-02 — Thiệp liên kết nhiều Order
1. Khách hàng mở chi tiết một thiệp History.
2. Hệ thống xác định thiệp đã được sử dụng cho nhiều Order.
3. Hệ thống hiển thị danh sách các mã Order mà thiệp đã được sử dụng.
4. Hệ thống vẫn chỉ hiển thị một History item của ảnh gốc.
5. Hệ thống không nhân bản History item theo số lượng Order liên kết.
6. Khách hàng có thể ấn vô được mã Order để điều hướng qua trang lịch sử của order đó.

---

### Exception Flows

#### EXC-01 — Không có History
1. Khách hàng mở “Thiệp thiết kế”.
2. Backend không tìm thấy History record có ảnh output thuộc khách hàng hiện tại.
3. Hệ thống không hiển thị danh sách thiệp.
4. Hệ thống hiển thị empty state.
5. Hệ thống hướng dẫn khách hàng tạo thiệp.

#### EXC-02 — Không tải được danh sách
1. Khách hàng mở “Thiệp thiết kế”.
2. Hệ thống không tải được danh sách History.
3. Hệ thống không hiển thị dữ liệu History không đầy đủ.
4. Hệ thống hiển thị error state.
5. Hệ thống cho phép khách hàng tải lại danh sách.

#### EXC-03 — Thiệp không thuộc khách hàng
1. Khách hàng truy cập trực tiếp vào một thiệp bằng ID hoặc URL.
2. Backend kiểm tra quyền sở hữu của thiệp.
3. Backend xác định thiệp không thuộc khách hàng hiện tại.
4. Backend từ chối truy cập.
5. Hệ thống không trả:
   - Ảnh thiệp.
   - Metadata của thiệp.

#### EXC-04 — File không còn khả dụng
1. Khách hàng mở một History record đã tồn tại.
2. Hệ thống xác định file ảnh của thiệp không còn khả dụng.
3. Hệ thống vẫn giữ History record.
4. Hệ thống hiển thị các metadata còn lại.
5. Hệ thống hiển thị thông báo: **“Ảnh không còn khả dụng.”**
6. Chức năng tải xuống bị vô hiệu hóa.

---

## Acceptance Criteria

### AC-001 – Hiển thị danh sách thiệp
- **Given:** Khách hàng có các thiệp generate có ảnh
- **When:** Mở “Thiệp thiết kế”
- **Then:** Hệ thống hiển thị danh sách chung của khách hàng
- **And:** Sắp xếp mới nhất trước.

### AC-002 – Điều kiện tạo History item
- **Given:** Một lần generate kết thúc
- **When:** Không có ảnh output
- **Then:** Không tạo History item.

### AC-003 – Thiệp chưa liên kết Order
- **Given:** một thiệp có ảnh nhưng chưa liên kết Order
- **When:** Xem History
- **Then:** Thiệp vẫn xuất hiện
- **And:** Có thể xem, tải hoặc Tạo lại.

### AC-004 – Thiệp liên kết nhiều Order
- **Given:** Một thiệp đã được dùng cho Order A và Order B
- **When:** Xem History
- **Then:** Chỉ có một History item ảnh gốc
- **And:** Có hai record liên kết Order tương ứng.

### AC-005 – Màn hình chi tiết thiệp
- **Given:** Khách hàng chọn một thiệp của mình
- **When:** Mở chi tiết
- **Then:** Hệ thống hiển thị ảnh và metadata
- **And:** Hiển thị các Order liên quan nếu có.

### AC-006 – File không còn khả dụng
- **Given:** History record tồn tại nhưng file không còn khả dụng
- **When:** Mở chi tiết
- **Then:** Record vẫn tồn tại
- **And:** Hiển thị “Ảnh không còn khả dụng.”

### AC-007 – Kiểm tra Ownership
- **Given:** Thiệp thuộc khách hàng khác
- **When:** User truy cập trực tiếp
- **Then:** Backend từ chối
- **And:** Không trả ảnh hoặc metadata.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-133**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c6e43277-f57a-495b-8f0e-59929386c30e) | Điều kiện tạo History | Xem lịch sử thiệp | Chỉ lần generate có ảnh output mới tạo đúng 01 History item. | Một lần generate kết thúc. | Nếu có ảnh output, hệ thống tạo đúng 01 History item. | Generate không có ảnh output không tạo History item. | Đức Bình | STORY-045 | Draft | v0 | 2026-08-14 |
| [**BR-134**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/14e5617a-93d9-4b7f-b79a-81ffa9997305) | Phạm vi History | Xem lịch sử thiệp | History thuộc khách hàng và hiển thị dưới dạng danh sách chung, không phụ thuộc thiệp đã có Order hay chưa. | Khách hàng mở “Thiệp thiết kế”. | Hệ thống hiển thị các History item thuộc khách hàng hiện tại dưới dạng danh sách chung. | Thiệp chưa liên kết Order vẫn được hiển thị bình thường. | Đức Bình | STORY-045 | Draft | v0 | 2026-08-14 |
| [**BR-135**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/77859db4-5755-4ef7-ad89-18093ffa450b) | Liên kết Order | Xem lịch sử thiệp | History record ảnh gốc độc lập với record liên kết Thiệp–Order. Một thiệp có thể có 0, 1 hoặc nhiều liên kết Order. | Thiệp được liên kết với một hoặc nhiều Order. | Hệ thống giữ một History record ảnh gốc và các record liên kết Thiệp–Order tương ứng. | Không nhân bản History item theo số lượng Order liên kết. | Đức Bình | STORY-045 | Draft | v0 | 2026-08-14 |
| [**BR-136**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f0f4ef94-8cc7-4e23-8d2d-2b4a932417fc) | Tái sử dụng | Xem lịch sử thiệp | Tái sử dụng thiệp chỉ tạo thêm liên kết Thiệp–Order khi Order mới được tạo; không sao chép ảnh, không tạo History và không trừ quota. | Khách hàng chọn lại một thiệp History để sử dụng cho Checkout mới và Order mới được tạo. | Hệ thống tạo thêm liên kết Thiệp–Order cho Order mới. | Không sao chép ảnh, không chạy AI, không tạo History item mới và không trừ quota. | Đức Bình | STORY-045 | Draft | v0 | 2026-08-14 |
| [**BR-137**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/562e4749-d01e-4244-94b8-5bd3db56ec78) | Thứ tự | Xem lịch sử thiệp | Danh sách sắp xếp theo thời điểm generate, mới nhất trước. | Hệ thống hiển thị danh sách History. | Các History item được sắp xếp theo thời điểm generate, mới nhất trước. | Không có. | Đức Bình | STORY-045 | Draft | v0 | 2026-08-14 |
| [**BR-138**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a36b64f9-2e28-4fa1-8336-01082bd8a7c5) | Bảo toàn record | Xem lịch sử thiệp | Order đổi trạng thái hoặc file ảnh mất/hỏng không làm xóa History record. | Order liên quan hoàn tất, bị hủy hoặc refund, hoặc file ảnh của History item mất/hỏng. | History record vẫn được giữ lại. | Nếu file ảnh không còn khả dụng, hệ thống chỉ hiển thị metadata còn lại và thông báo “Ảnh không còn khả dụng.” | Đức Bình | STORY-045 | Draft | v0 | 2026-08-14 |
| [**BR-139**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0c1312b1-89a3-4eaf-ab39-ef084d66e00b) | Quyền sở hữu | Xem lịch sử thiệp | Khách hàng chỉ được xem và sử dụng thiệp History thuộc tài khoản của mình. | Khách hàng xem, truy cập trực tiếp hoặc sử dụng một thiệp History. | Backend kiểm tra quyền sở hữu và chỉ cho phép thao tác nếu thiệp thuộc khách hàng hiện tại. | Nếu thiệp thuộc khách hàng khác, backend từ chối và không trả ảnh hoặc metadata. | Đức Bình | STORY-045 | Draft | v0 | 2026-08-14 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-133 | [Điều kiện tạo History](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c6e43277-f57a-495b-8f0e-59929386c30e) |
| BR-134 | [Phạm vi History](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/14e5617a-93d9-4b7f-b79a-81ffa9997305) |
| BR-135 | [Liên kết Order](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/77859db4-5755-4ef7-ad89-18093ffa450b) |
| BR-136 | [Tái sử dụng](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f0f4ef94-8cc7-4e23-8d2d-2b4a932417fc) |
| BR-137 | [Thứ tự](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/562e4749-d01e-4244-94b8-5bd3db56ec78) |
| BR-138 | [Bảo toàn record](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a36b64f9-2e28-4fa1-8336-01082bd8a7c5) |
| BR-139 | [Quyền sở hữu](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0c1312b1-89a3-4eaf-ab39-ef084d66e00b) |

---

## Non-Functional Requirements

- API danh sách phải phân trang và không trả binary ảnh trong payload metadata.
- Mục tiêu **p95 ≤ 2 giây** với page size được cấu hình, không tính tải ảnh từ storage.
- Ownership phải kiểm tra tại backend.
- File không khả dụng không được làm hỏng toàn bộ danh sách.
- UI có loading, empty và error state rõ ràng.

---

## Out of Scope

- Generate/tạo lại thiệp.
- Chỉnh sửa hoặc xóa History.
- Thanh toán và quản lý Order.
- Chia sẻ thiệp.
