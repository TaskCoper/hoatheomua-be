# STORY-038 — Khách hàng khởi tạo Checkout từ mẫu hoa

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã đăng nhập, tôi muốn mở Checkout từ mẫu hoa đã chọn, để nhập thông tin giao hàng, lựa chọn thiệp và chuẩn bị đặt hàng. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Danh Nguyen |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Checkout được mở khi khách hàng chọn "Đặt hàng ngay" từ một mẫu hoa thông thường hoặc một kết quả Custom AI hợp lệ.
---

## Conditions

### Preconditions

- Khách hàng đã đăng nhập.
- Mẫu hoa tồn tại, thuộc phạm vi được phép sử dụng.
- Sản phẩm/Combo nguồn còn đủ điều kiện đặt hàng.

### Trigger

> - Khách hàng chọn "Đặt hàng ngay".


---

## Flow

### Main Flow

1. Khách hàng chọn "Đặt hàng ngay" từ mẫu hoa hợp lệ.
2. Backend kiểm tra mẫu hoa, quyền truy cập và khả năng sử dụng.
3. Hệ thống kiểm tra Combo nguồn từ hệ thống bên thứ ba.
4. Nếu Core còn hàng, hệ thống tạo phiên Checkout tạm.
5. Hệ thống gắn đúng 01 mẫu hoa nguồn vào Checkout.
6. Hệ thống hiển thị Step 1 và các thông tin cần nhập.
7. Khách hàng chọn địa chỉ đã lưu hoặc nhập địa chỉ giao hàng mới.
8. Khách hàng có thể nhập thông tin xuất hóa đơn VAT.
9. Khách hàng có thể cấu hình thiệp mới tại Checkout hoặc chọn thiệp đã có từ History theo STORY-042.
10. Hệ thống không hiển thị hoặc xử lý thao tác Tạo lại thiệp từ History trong Checkout.
11. Hệ thống hiển thị tạm tính; phí giao hàng được xác định khi hệ thống gọi API của Ahamove tại thời điểm gọi.
12. Chưa tạo Order cho đến khi khách hàng bấm "Hoàn tất".

### Alternative Flows

#### ALT-01 — Không có thiệp
- Khách hàng tiếp tục và hoàn tất Checkout bình thường.
- Bill không có giá thiệp.

#### ALT-02 — Support hết hàng
- Khách hàng tiếp tục Checkout từ mẫu hoa đã chọn.
- Hệ thống kiểm tra tồn kho các thành phần của Combo.
- Hệ thống xác định Core còn đủ hàng nhưng một hoặc nhiều Support hết hàng.
- Hệ thống vẫn cho phép khách hàng tiếp tục Checkout.
- Hệ thống ghi nhận các thành phần Support cần thay thế.
- Sau khi Order được tạo, Staff Hoa Theo Mùa chủ động liên hệ trực tiếp với khách hàng.

#### ALT-03 — Thoát Checkout
- Khách hàng đóng tab, rời khỏi luồng Checkout sang trang khác, đăng xuất hoặc trình duyệt bị đóng/crash trước khi hoàn tất.
- Hệ thống hủy Checkout và dữ liệu đã nhập trong Checkout bị mất.
- Refresh trang không làm mất Checkout hiện tại.
- Không tạo Order/PENDING.
- Kết quả AI đã có trong History không bị xóa và quota đã dùng không được hoàn.

#### ALT-04 — Đặt lại mẫu hoa Custom AI đã sử dụng
- Khách hàng mở lịch sử mẫu hoa Custom AI.
- Khách hàng chọn một mẫu đã từng được dùng để tạo Order.
- Khách hàng ấn chọn "Đặt hàng ngay".
- Backend kiểm tra mẫu hoa thuộc khách hàng hiện tại.
- Backend kiểm tra Combo nguồn vẫn tồn tại.
- Hệ thống kiểm tra tồn kho hiện tại của các thành phần trong Combo.
- Nếu Combo nguồn còn hợp lệ và Core còn đủ hàng, hệ thống khởi tạo Checkout mới từ mẫu đó.
- Các Order và liên kết lịch sử trước đó vẫn được giữ nguyên.

### Exception Flows

#### EXC-01 — Thành phần Core không còn đủ hàng
- Hệ thống kiểm tra tồn kho Core tại thời điểm khách hàng mở Checkout hoặc bấm "Hoàn tất".
- Nếu Core không đủ hàng tại thời điểm mở Checkout, hệ thống không khởi tạo Checkout.
- Nếu Checkout đã được khởi tạo nhưng Core không đủ hàng tại thời điểm bấm "Hoàn tất", hệ thống giữ Checkout hiện tại nhưng không cho hoàn tất.
- Hệ thống không tạo Order.
- Hệ thống hiển thị thông báo Combo hiện không còn đủ thành phần chính để đặt hàng.

#### EXC-02 — Combo nguồn không còn khả dụng
- Khách hàng chọn "Đặt hàng ngay" từ một mẫu hoa Custom AI đã tạo.
- Backend kiểm tra Combo nguồn của mẫu hoa.
- Hệ thống phát hiện Combo nguồn không tồn tại, đã bị vô hiệu hóa hoặc không còn được phép đặt hàng.
- Hệ thống không khởi tạo Checkout.
- Hệ thống không tạo Order hoặc liên kết Mẫu hoa-Order mới.

#### EXC-03 — Khách hàng không có quyền truy cập
- Khách hàng gửi request khởi tạo, xem hoặc hoàn tất Checkout.
- Backend kiểm tra quyền sở hữu của mẫu hoa AI và Checkout.
- Backend phát hiện khách hàng không có quyền thực hiện thao tác.
- Backend từ chối request.
- Backend không trả dữ liệu của khách hàng khác.

#### EXC-04 — Thông tin giao hàng bắt buộc không hợp lệ
- Khách hàng chưa nhập hoặc chưa chọn hợp lệ thông tin giao hàng bắt buộc.
- Khách hàng ấn chọn "Đặt hàng".
- Hệ thống kiểm tra tính đầy đủ và hợp lệ của từng trường.
- Hệ thống không hoàn tất Checkout và không tạo Order nếu có dữ liệu không hợp lệ.
- Hệ thống giữ lại các thông tin hợp lệ mà khách hàng đã nhập.

#### EXC-05 — Không lấy được phí giao hàng
- Hệ thống gọi Ahamove nhưng không nhận được phí hợp lệ.
- Hệ thống giữ Checkout.
- Hệ thống không tạo Order.
- Hệ thống yêu cầu khách hàng thử lại hoặc kiểm tra địa chỉ giao hàng.

---

## Acceptance Criteria

### AC-001
- **Given**: Mẫu hoa hợp lệ.
- **When**: Khách hàng chọn "Đặt hàng ngay".
- **Then**:
  - Hệ thống mở Checkout.
  - Chưa tạo Order.
  - Chưa gán trạng thái PENDING.

### AC-002
- **Given**: Checkout được mở.
- **When**: Hệ thống khởi tạo Checkout.
- **Then**: Checkout phải tham chiếu đúng 01 mẫu hoa nguồn.

### AC-003
- **Given**: Khách hàng chưa chọn hoặc nhập địa chỉ giao hàng hợp lệ.
- **When**: Khách hàng tiếp tục từ Step 1.
- **Then**: Hệ thống không cho chuyển sang bước tiếp theo.

### AC-004
- **Given**: Checkout không có thiệp.
- **When**: Khách hàng hoàn tất hợp lệ.
- **Then**: Hệ thống vẫn cho phép tạo Order.

### AC-005
- **Given**: Core hết hàng.
- **When**: Khách hàng chọn "Đặt hàng ngay".
- **Then**: Hệ thống không khởi tạo Checkout.

### AC-006
- **Given**: Core còn hàng nhưng support hết.
- **When**: Khách hàng tiếp tục Checkout.
- **Then**:
  - Hệ thống vẫn cho đặt hàng.
  - FE thông báo cửa hàng sẽ thay support tương ứng.

### AC-007
- **Given**: Khách hàng đang có Checkout chưa hoàn tất.
- **When**: Khách hàng đóng tab, rời khỏi luồng Checkout, đăng xuất hoặc trình duyệt bị đóng/crash.
- **Then**:
  - Checkout bị hủy và không thể khôi phục.
  - Không tạo Order/PENDING.

### AC-008
- **Given**: Mẫu hoa Custom AI thuộc khách hàng và đã từng được dùng để tạo Order.
- **When**: Khách hàng chọn "Đặt hàng ngay" từ lịch sử mẫu hoa.
- **Then**:
  - Backend phải kiểm tra Combo nguồn và tồn kho hiện tại.
  - Nếu Combo nguồn còn khả dụng và Core còn đủ hàng, hệ thống cho phép khởi tạo Checkout mới.
  - Nếu Combo nguồn không còn khả dụng hoặc Core hết hàng, hệ thống không khởi tạo Checkout.

### AC-009
- **Given**: Checkout đã tồn tại và Core hết hàng.
- **When**: Khách hàng bấm "Hoàn tất".
- **Then**: Hệ thống giữ Checkout nhưng không tạo Order.

### AC-010
- **Given**: Checkout hợp lệ đang tồn tại.
- **When**: Khách hàng refresh trang Checkout.
- **Then**: Checkout hiện tại vẫn được giữ.

### AC-011
- **Given**: Khách hàng đang ở Checkout.
- **When**: Hệ thống hiển thị các thao tác liên quan đến thiệp History.
- **Then**:
  - Hệ thống không hiển thị hoặc xử lý thao tác Tạo lại thiệp từ History.
  - Khách hàng chỉ có thể chọn thiệp History hợp lệ theo STORY-042 hoặc cấu hình thiệp mới theo STORY-035.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Nguồn | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực | Ghi chú / Link logic |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| [BR-081](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d007e95d-bc22-44e1-8cf3-0ec1bd33c827) | Checkout tạm | Khởi tạo Checkout từ mẫu hoa | Checkout chỉ là phiên chuẩn bị đặt hàng. | Khách hàng mở Checkout từ một mẫu hoa hợp lệ. | Hệ thống tạo phiên Checkout tạm để khách hàng nhập thông tin giao hàng, lựa chọn thiệp và chuẩn bị đặt hàng. | Order chỉ được tạo khi khách hàng bấm “Hoàn tất” và toàn bộ kiểm tra cuối hợp lệ. | Google Sheet rule source | Đức Bình | STORY-038 | Draft | v0 | 2026-08-14 | — |
| [BR-082](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/39b4985d-a875-4939-9bd7-6bae7f6cf788) | Một mẫu hoa | Khởi tạo Checkout từ mẫu hoa | Mỗi Checkout bắt buộc tham chiếu đúng 01 mẫu hoa thông thường hoặc Custom AI. | Hệ thống khởi tạo Checkout. | Hệ thống gắn đúng 01 mẫu hoa nguồn vào Checkout. | Không cho phép một Checkout không có mẫu hoa nguồn hoặc tham chiếu nhiều mẫu hoa nguồn. | Google Sheet rule source | Đức Bình | STORY-038 | Draft | v0 | 2026-08-14 | — |
| [BR-083](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7184a4b9-e6d0-49f0-a2b1-ef990f8dfd1e) | Tái sử dụng mẫu hoa Custom AI | Khởi tạo Checkout từ mẫu hoa | Mẫu hoa Custom AI được phép sử dụng để tạo nhiều Order nếu Combo nguồn vẫn còn đủ điều kiện đặt hàng. | Khách hàng chọn “Đặt hàng ngay” từ một mẫu đã từng được sử dụng. | Backend kiểm tra Combo nguồn và tồn kho hiện tại. Nếu Combo hợp lệ và Core còn đủ hàng, hệ thống khởi tạo Checkout mới. | Không cho phép mở Checkout nếu Combo nguồn không còn khả dụng hoặc Core không còn đủ hàng. Các Order và liên kết lịch sử trước đó không bị thay đổi. | Google Sheet rule source | Đức Bình | STORY-038 | Draft | v0 | 2026-08-14 | — |
| [BR-084](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ecc9b927-b6fb-4f23-b272-cc92ffc588f2) | Tồn kho Combo | Khởi tạo Checkout từ mẫu hoa | Tồn kho Combo quyết định khả năng tiếp tục đặt hàng. | Khách hàng mở Checkout hoặc hoàn tất Checkout từ một mẫu hoa. | Core hết hàng làm Combo hết hàng và chặn đặt. Support hết không chặn đặt nếu core còn; cửa hàng chịu trách nhiệm thay thế support tương ứng. | Hệ thống không tự động thay thế Support khi Staff và khách hàng chưa thống nhất. | Google Sheet rule source | Đức Bình | STORY-038 | Draft | v0 | 2026-08-14 | — |
| [BR-085](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/67c4719b-e909-49c4-b778-a83cf804c03d) | Thiệp | Khởi tạo Checkout từ mẫu hoa | Thiệp không bắt buộc trong Checkout. Một Checkout chỉ có tối đa 01 thiệp đã được khách hàng xác nhận và đang chọn tại một thời điểm. | Khách hàng thao tác trong Checkout. | Hệ thống cho phép Checkout có tối đa 01 thiệp đã được xác nhận và đang chọn hoặc không có thiệp. | Không cho phép một Checkout có nhiều hơn 01 thiệp đang chọn. | Google Sheet rule source | Đức Bình | STORY-038 | Draft | v0 | 2026-08-14 | — |
| [BR-086](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6a8a89fa-c0a9-4569-a72e-e6c34e7fb4cb) | Thoát Checkout | Khởi tạo Checkout từ mẫu hoa | Checkout chỉ bị hủy khi khách hàng rời khỏi phiên thao tác, không bị hủy khi refresh trang. | Khách hàng đóng tab, rời khỏi luồng Checkout, đăng xuất hoặc trình duyệt bị đóng/crash trước khi bấm “Hoàn tất”. | Hệ thống hủy Checkout, không tạo Order/PENDING và không cho phép khôi phục Checkout đó. | Refresh trang không làm mất Checkout hiện tại. History AI không bị xóa và quota đã dùng không được hoàn. | Google Sheet rule source | Đức Bình | STORY-038 | Draft | v0 | 2026-08-14 | — |
| [BR-044](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b4df6090-28ff-4fe4-a528-9123a7f5ad8e) | Phạm vi Tạo lại thiệp AI | Tạo ảnh | Tạo lại thiệp AI chỉ được phép thực hiện trong bước tạo Thiệp Custom sau khi Order đã thanh toán thành công; không được Tạo lại thiệp từ History. | Khách hàng chọn Tạo lại thiệp hoặc backend nhận request Tạo lại thiệp. | Backend chỉ chấp nhận request Tạo lại khi request thuộc một Order Thiệp Custom của khách hàng hiện tại, Order đã Payment SUCCESS và đang ở bước tạo Thiệp Custom sau thanh toán. Mỗi request Tạo lại hợp lệ khởi tạo một AI Job mới và được tính vào quota tạo thiệp AI. Hệ thống không hiển thị thao tác Tạo lại trên danh sách History hoặc Chi tiết thiệp đã tạo. Backend phải từ chối mọi request Tạo lại thiệp từ History. Khi request bị từ chối, hệ thống không tạo AI Job, không gọi AI service, không tạo ảnh mới, không tạo History record và không trừ quota. | Không áp dụng cho thao tác chọn thiệp đã có từ History cho Checkout, vì thao tác đó không gọi AI và không tạo History record mới. | Product discussion 2026-09-11; STORY-036; STORY-039; STORY-045 | Đức Bình | STORY-038; STORY-036 | Draft | v0 | 2026-09-11 | Rule này phân biệt Tạo lại hợp lệ sau thanh toán thành công với Tạo lại không hợp lệ từ History. |
| [BR-153](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/78f3d74b-fc51-4150-9bf3-85741fcdf683) | Tính phí giao hàng | Tính phí giao hàng | Phí giao hàng được lấy từ Ahamove theo địa chỉ giao hàng hiện tại. | Hệ thống cần hiển thị tạm tính hoặc khách hàng bấm “Hoàn tất”. | Hệ thống gọi Ahamove để lấy phí ship hiện hành; khi “Hoàn tất” phải gọi lại và dùng kết quả cuối cùng để tạo Order. | Nếu Ahamove timeout, lỗi hoặc khu vực không được hỗ trợ, hệ thống không cho hoàn tất Checkout và hiển thị thông báo phù hợp. | Google Sheet rule source | Đức Bình | STORY-038 | Draft | v0 | 2026-08-17 | — |
| [BR-156](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3bb381ff-ff91-4892-9309-ceed135f603c) | Giới hạn 03 lần tạo thiệp theo mẫu hoa | AI | Trong một lần đặt hàng từ một mẫu hoa nguồn, khách hàng được thực hiện tối đa 03 lần generate thiệp AI cho mẫu hoa đó. | Khách hàng thực hiện thao tác tạo thiệp hoặc Tạo lại thiệp hợp lệ làm phát sinh một AI Job tạo thiệp mới trong bước tạo Thiệp Custom sau khi Order đã thanh toán thành công. | Backend phải đồng thời kiểm tra mẫu hoa hiện tại chưa sử dụng hết 03 lượt generate thiệp và khách hàng chưa sử dụng hết 10 lượt generate thiệp trong ngày. Nếu cả hai điều kiện đều thỏa, hệ thống cho phép tạo AI Job và ghi nhận 01 lượt vào cả hai giới hạn. | Nếu AI Job thất bại sau toàn bộ retry và không tạo được ảnh output hợp lệ, hệ thống hoàn lại lượt đã ghi nhận cho cả giới hạn theo mẫu hoa và quota ngày. Thao tác Xác nhận thiệp, chọn thiệp đã có từ History, tải xuống thiệp và request Tạo lại thiệp từ History không được tính là lượt generate. Request Tạo lại thiệp từ History phải bị từ chối theo BR-044. | Product discussion 2026-09-11; STORY-036; STORY-038; STORY-039 | Đức Bình | STORY-038; STORY-036 | Draft | v0 | 2026-09-11 | Tạo lại hợp lệ sau thanh toán thành công vẫn nằm trong giới hạn 03 lần generate thiệp theo mẫu hoa. |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-081 | [BR-081](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d007e95d-bc22-44e1-8cf3-0ec1bd33c827) |
| BR-082 | [BR-082](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/39b4985d-a875-4939-9bd7-6bae7f6cf788) |
| BR-083 | [BR-083](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7184a4b9-e6d0-49f0-a2b1-ef990f8dfd1e) |
| BR-084 | [BR-084](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ecc9b927-b6fb-4f23-b272-cc92ffc588f2) |
| BR-085 | [BR-085](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/67c4719b-e909-49c4-b778-a83cf804c03d) |
| BR-086 | [BR-086](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6a8a89fa-c0a9-4569-a72e-e6c34e7fb4cb) |
| BR-044 | [BR-044](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b4df6090-28ff-4fe4-a528-9123a7f5ad8e) |
| BR-153 | [BR-153](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/78f3d74b-fc51-4150-9bf3-85741fcdf683) |
| BR-156 | [BR-156](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3bb381ff-ff91-4892-9309-ceed135f603c) |

### Dependencies

- [STORY-030](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [STORY-033](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9)
- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [STORY-036](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)
- [STORY-042](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)

---

## Non-Functional Requirements

- Backend kiểm tra ownership và trạng thái tài nguyên.
- Kiểm tra giá và tồn kho cuối phải thực hiện lại khi bấm "Hoàn tất".
- Khi mở Checkout, hệ thống chỉ kiểm tra tồn kho hiện tại và chưa reserve tồn.
- API Checkout trong điều kiện bình thường đạt p95 ≤ 2 giây, không tính dịch vụ bên thứ ba bị chậm.

---

## Out of Scope

- Thanh toán Order.
- Quản lý tồn kho phía cửa hàng.
- Admin thay thế support.
- Generate mẫu hoa AI và thiệp AI.

---
