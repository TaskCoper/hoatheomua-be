# STORY-045 — Khách hàng xem History thiệp AI đã tạo

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã sử dụng chức năng tạo thiệp AI, tôi muốn xem lại tất cả thiệp có ảnh đã tạo, để xem thông tin chi tiết và truy cập các thao tác còn được phép liên quan đến thiệp đã tạo. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Danh Nguyen |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Chức năng được truy cập từ mục "Thiệp thiết kế" trên sidebar.
---

## Conditions

### Preconditions

- Khách hàng đã đăng nhập.

### Trigger

> - Khách hàng mở "Thiệp thiết kế".


---

## Flow

### Main Flow

1. Khách hàng mở "Thiệp thiết kế".
2. Backend lấy các History record có ảnh output thuộc khách hàng hiện tại.
3. Hệ thống sắp xếp theo thời điểm generate, mới nhất trước.
4. Hệ thống hiển thị mỗi thiệp với ảnh preview, mã thiệp, thông tin thiệp, thời điểm tạo và thao tác còn được phép.
5. Hệ thống không hiển thị thao tác "Tạo lại" trong danh sách thiệp.
6. Nếu thiệp có liên kết Order, hệ thống có thể hiển thị các mã Order liên quan.
7. Nếu chưa liên kết Order, hệ thống vẫn hiển thị bình thường.
8. Khách hàng chọn một thiệp để xem chi tiết.
9. Màn chi tiết hiển thị ảnh, metadata còn khả dụng, các Order liên quan và chức năng tải xuống nếu file ảnh còn khả dụng.
10. Màn chi tiết không hiển thị chức năng "Tạo lại".

### Alternative Flows

#### ALT-01 — Thiệp chưa thuộc Order
- Khách hàng mở danh sách hoặc chi tiết một thiệp History.
- Hệ thống xác định thiệp chưa liên kết với Order nào.
- Hệ thống vẫn hiển thị thiệp trong History.
- Hệ thống hiển thị các thông tin metadata và trạng thái chưa liên kết đơn hàng.

#### ALT-02 — Thiệp liên kết nhiều Order
- Khách hàng mở chi tiết một thiệp History.
- Hệ thống xác định thiệp đã được sử dụng cho nhiều Order.
- Hệ thống hiển thị danh sách các mã Order mà thiệp đã được sử dụng.
- Hệ thống vẫn chỉ hiển thị một History item của ảnh gốc.
- Hệ thống không nhân bản History item theo số lượng Order liên kết.
- Khách hàng có thể chọn mã Order để điều hướng qua trang lịch sử của Order đó.

### Exception Flows

#### EXC-01 — Không có History
- Khách hàng mở "Thiệp thiết kế".
- Backend không tìm thấy History record có ảnh output thuộc khách hàng hiện tại.
- Hệ thống không hiển thị danh sách thiệp.
- Hệ thống hiển thị empty state.
- Hệ thống hướng dẫn khách hàng tạo thiệp.

#### EXC-02 — Không tải được danh sách
- Khách hàng mở "Thiệp thiết kế".
- Hệ thống không tải được danh sách History.
- Hệ thống không hiển thị dữ liệu History không đầy đủ.
- Hệ thống hiển thị error state.
- Hệ thống cho phép khách hàng tải lại danh sách.

#### EXC-03 — Thiệp không thuộc khách hàng
- Khách hàng truy cập trực tiếp vào một thiệp bằng ID hoặc URL.
- Backend kiểm tra quyền sở hữu của thiệp.
- Backend xác định thiệp không thuộc khách hàng hiện tại.
- Backend từ chối truy cập.
- Backend không trả ảnh thiệp hoặc metadata của thiệp.

#### EXC-04 — File không còn khả dụng
- Khách hàng mở một History record đã tồn tại.
- Hệ thống xác định file ảnh của thiệp không còn khả dụng.
- Hệ thống vẫn giữ History record.
- Hệ thống hiển thị các metadata còn lại.
- Hệ thống hiển thị thông báo "Ảnh không còn khả dụng."
- Chức năng tải xuống bị vô hiệu hóa.

#### EXC-05 — Request Tạo lại từ History
- Client gửi request Tạo lại thiệp từ History.
- Backend xác định chức năng Tạo lại thiệp từ History không còn được hỗ trợ.
- Backend từ chối request.
- Hệ thống không tạo AI Job, không gọi AI service, không tạo ảnh mới, không tạo History item và không trừ quota.

---

## Acceptance Criteria

### AC-001
- **Given**: Khách hàng có các thiệp generate có ảnh.
- **When**: Mở "Thiệp thiết kế".
- **Then**:
  - Hệ thống hiển thị danh sách chung của khách hàng.
  - Sắp xếp mới nhất trước.

### AC-002
- **Given**: Một lần generate kết thúc.
- **When**: Không có ảnh output.
- **Then**: Không tạo History item.

### AC-003
- **Given**: Một thiệp có ảnh nhưng chưa liên kết Order.
- **When**: Xem History.
- **Then**:
  - Thiệp vẫn xuất hiện.
  - Có thể xem hoặc tải xuống nếu file còn khả dụng.

### AC-004
- **Given**: Một thiệp đã được dùng cho Order A và Order B.
- **When**: Xem History.
- **Then**:
  - Chỉ có một History item ảnh gốc.
  - Có hai record liên kết Order tương ứng.

### AC-005
- **Given**: Khách hàng chọn một thiệp của mình.
- **When**: Mở chi tiết.
- **Then**:
  - Hệ thống hiển thị ảnh và metadata.
  - Hiển thị các Order liên quan nếu có.
  - Không hiển thị thao tác "Tạo lại".

### AC-006
- **Given**: History record tồn tại nhưng file không còn khả dụng.
- **When**: Mở chi tiết.
- **Then**:
  - Record vẫn tồn tại.
  - Hiển thị "Ảnh không còn khả dụng."

### AC-007
- **Given**: Thiệp thuộc khách hàng khác.
- **When**: User truy cập trực tiếp.
- **Then**:
  - Backend từ chối.
  - Không trả ảnh hoặc metadata.

### AC-008
- **Given**: Backend nhận request Tạo lại thiệp từ History.
- **When**: Request được xử lý.
- **Then**:
  - Backend từ chối request.
  - Không tạo AI Job, không gọi AI service, không tạo ảnh mới, không tạo History item và không trừ quota.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Nguồn | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực | Ghi chú / Link logic |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| [BR-133](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c6e43277-f57a-495b-8f0e-59929386c30e) | Điều kiện tạo History | Xem lịch sử thiệp | Chỉ lần generate có ảnh output mới tạo đúng 01 History item. | Một lần generate kết thúc. | Nếu có ảnh output, hệ thống tạo đúng 01 History item. | Generate không có ảnh output không tạo History item. | Google Sheet rule source | Đức Bình | STORY-045 | Draft | v0 | 2026-08-14 | — |
| [BR-134](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/14e5617a-93d9-4b7f-b79a-81ffa9997305) | Phạm vi History | Xem lịch sử thiệp | History thuộc khách hàng và hiển thị dưới dạng danh sách chung, không phụ thuộc thiệp đã có Order hay chưa. | Khách hàng mở “Thiệp thiết kế”. | Hệ thống hiển thị các History item thuộc khách hàng hiện tại dưới dạng danh sách chung. | Thiệp chưa liên kết Order vẫn được hiển thị bình thường. | Google Sheet rule source | Đức Bình | STORY-045 | Draft | v0 | 2026-08-14 | — |
| [BR-135](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/77859db4-5755-4ef7-ad89-18093ffa450b) | Liên kết Order | Xem lịch sử thiệp | History record ảnh gốc độc lập với record liên kết Thiệp–Order. Một thiệp có thể có 0, 1 hoặc nhiều liên kết Order. | Thiệp được liên kết với một hoặc nhiều Order. | Hệ thống giữ một History record ảnh gốc và các record liên kết Thiệp–Order tương ứng. | Không nhân bản History item theo số lượng Order liên kết. | Google Sheet rule source | Đức Bình | STORY-045 | Draft | v0 | 2026-08-14 | — |
| [BR-136](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f0f4ef94-8cc7-4e23-8d2d-2b4a932417fc) | Tái sử dụng | Xem lịch sử thiệp | Tái sử dụng thiệp chỉ tạo thêm liên kết Thiệp–Order khi Order mới được tạo; không sao chép ảnh, không tạo History và không trừ quota. | Khách hàng chọn lại một thiệp History để sử dụng cho Checkout mới và Order mới được tạo. | Hệ thống tạo thêm liên kết Thiệp–Order cho Order mới. | Không sao chép ảnh, không chạy AI, không tạo History item mới và không trừ quota. | Google Sheet rule source | Đức Bình | STORY-045 | Draft | v0 | 2026-08-14 | — |
| [BR-137](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/562e4749-d01e-4244-94b8-5bd3db56ec78) | Thứ tự | Xem lịch sử thiệp | Danh sách sắp xếp theo thời điểm generate, mới nhất trước. | Hệ thống hiển thị danh sách History. | Các History item được sắp xếp theo thời điểm generate, mới nhất trước. | Không có. | Google Sheet rule source | Đức Bình | STORY-045 | Draft | v0 | 2026-08-14 | — |
| [BR-138](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a36b64f9-2e28-4fa1-8336-01082bd8a7c5) | Bảo toàn record | Xem lịch sử thiệp | Order đổi trạng thái hoặc file ảnh mất/hỏng không làm xóa History record. | Order liên quan hoàn tất, bị hủy hoặc refund, hoặc file ảnh của History item mất/hỏng. | History record vẫn được giữ lại. | Nếu file ảnh không còn khả dụng, hệ thống chỉ hiển thị metadata còn lại và thông báo “Ảnh không còn khả dụng.” | Google Sheet rule source | Đức Bình | STORY-045 | Draft | v0 | 2026-08-14 | — |
| [BR-139](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0c1312b1-89a3-4eaf-ab39-ef084d66e00b) | Quyền sở hữu | Xem lịch sử thiệp | Khách hàng chỉ được xem và sử dụng thiệp History thuộc tài khoản của mình. | Khách hàng xem, truy cập trực tiếp hoặc sử dụng một thiệp History. | Backend kiểm tra quyền sở hữu và chỉ cho phép thao tác nếu thiệp thuộc khách hàng hiện tại. | Nếu thiệp thuộc khách hàng khác, backend từ chối và không trả ảnh hoặc metadata. | Google Sheet rule source | Đức Bình | STORY-045 | Draft | v0 | 2026-08-14 | — |
| [BR-044](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b4df6090-28ff-4fe4-a528-9123a7f5ad8e) | Phạm vi Tạo lại thiệp AI | Tạo ảnh | Tạo lại thiệp AI chỉ được phép thực hiện trong bước tạo Thiệp Custom sau khi Order đã thanh toán thành công; không được Tạo lại thiệp từ History. | Khách hàng chọn Tạo lại thiệp hoặc backend nhận request Tạo lại thiệp. | Backend chỉ chấp nhận request Tạo lại khi request thuộc một Order Thiệp Custom của khách hàng hiện tại, Order đã Payment SUCCESS và đang ở bước tạo Thiệp Custom sau thanh toán. Mỗi request Tạo lại hợp lệ khởi tạo một AI Job mới và được tính vào quota tạo thiệp AI. Hệ thống không hiển thị thao tác Tạo lại trên danh sách History hoặc Chi tiết thiệp đã tạo. Backend phải từ chối mọi request Tạo lại thiệp từ History. Khi request bị từ chối, hệ thống không tạo AI Job, không gọi AI service, không tạo ảnh mới, không tạo History record và không trừ quota. | Không áp dụng cho thao tác chọn thiệp đã có từ History cho Checkout, vì thao tác đó không gọi AI và không tạo History record mới. | Product discussion 2026-09-11; STORY-036; STORY-039; STORY-045 | Đức Bình | STORY-036; STORY-045 | Draft | v0 | 2026-09-11 | Rule này phân biệt Tạo lại hợp lệ sau thanh toán thành công với Tạo lại không hợp lệ từ History. |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-133 | [BR-133](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c6e43277-f57a-495b-8f0e-59929386c30e) |
| BR-134 | [BR-134](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/14e5617a-93d9-4b7f-b79a-81ffa9997305) |
| BR-135 | [BR-135](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/77859db4-5755-4ef7-ad89-18093ffa450b) |
| BR-136 | [BR-136](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f0f4ef94-8cc7-4e23-8d2d-2b4a932417fc) |
| BR-137 | [BR-137](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/562e4749-d01e-4244-94b8-5bd3db56ec78) |
| BR-138 | [BR-138](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a36b64f9-2e28-4fa1-8336-01082bd8a7c5) |
| BR-139 | [BR-139](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0c1312b1-89a3-4eaf-ab39-ef084d66e00b) |
| BR-044 | [BR-044](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b4df6090-28ff-4fe4-a528-9123a7f5ad8e) |

### Dependencies

- [STORY-036](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)

---

## Non-Functional Requirements

- API danh sách phải phân trang và không trả binary ảnh trong payload metadata.
- Mục tiêu p95 ≤ 2 giây với page size được cấu hình, không tính tải ảnh từ storage.
- Ownership phải kiểm tra tại backend.
- File không khả dụng không được làm hỏng toàn bộ danh sách.
- UI có loading, empty và error state rõ ràng.

---

## Out of Scope

- Generate/tạo mới thiệp.
- Tạo lại thiệp từ History.
- Chỉnh sửa hoặc xóa History.
- Thanh toán và quản lý Order.
- Chia sẻ thiệp.

---
