# STORY-038 — Khách hàng khởi tạo Checkout từ mẫu hoa

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã đăng nhập, tôi muốn mở Checkout từ mẫu hoa đã chọn, để nhập thông tin giao hàng, lựa chọn thiệp và chuẩn bị đặt hàng. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Checkout được mở khi khách hàng chọn **"Đặt hàng ngay"** từ một mẫu hoa thông thường hoặc một kết quả Custom AI hợp lệ.

- Checkout là **phiên tạm** trong thời gian khách hàng thao tác và không được lưu lâu dài.
- Một Checkout bắt buộc xuất phát từ đúng **01 mẫu hoa**.
- Thiệp là **tùy chọn**. Đối với mẫu hoa nguồn của Checkout, khách hàng được thực hiện tối đa **03 lượt** generate thiệp AI theo BR-156, nhưng tại một thời điểm Checkout chỉ có tối đa **01 thiệp** đã được xác nhận và đang chọn.

---

## Conditions

### Preconditions
- Khách hàng đã đăng nhập.
- Mẫu hoa tồn tại, thuộc phạm vi được phép sử dụng.
- Sản phẩm/Combo nguồn còn đủ điều kiện đặt hàng.

### Trigger
> Khách hàng chọn **"Đặt hàng ngay"**.

---

## Flow

### Main Flow — Mở Checkout

1. Khách hàng chọn **"Đặt hàng ngay"** từ mẫu hoa hợp lệ.
2. Backend kiểm tra mẫu hoa, quyền truy cập và khả năng sử dụng.
3. Hệ thống kiểm tra Combo nguồn từ hệ thống bên thứ ba.
4. Nếu Core còn hàng, hệ thống tạo phiên **Checkout tạm**.
5. Hệ thống gắn đúng **01 mẫu hoa nguồn** vào Checkout.
6. Hệ thống hiển thị Step 1 và các thông tin cần nhập.
7. Khách hàng chọn địa chỉ đã lưu hoặc nhập địa chỉ giao hàng mới; đây là thông tin **bắt buộc**.
8. Khách hàng có thể nhập thông tin xuất hóa đơn VAT.
9. Khách hàng có thể tạo thiệp AI, Tạo lại thiệp AI hoặc chọn thiệp đã có từ History; trong Checkout chỉ có tối đa **01 thiệp** đã được xác nhận và đang chọn, hoặc khách hàng có thể bỏ qua thiệp.
10. Hệ thống hiển thị tạm tính; phí giao hàng được xác định khi hệ thống gọi API của **Ahamove** để tính phí ship tại thời điểm gọi.
11. **Chưa tạo Order** cho đến khi khách hàng bấm **"Hoàn tất"**.

---

### Alternative Flows

#### ALT-01 — Không có thiệp
1. Khách hàng tiếp tục và hoàn tất Checkout bình thường; bill không có giá thiệp.

#### ALT-02 — Support hết hàng
1. Khách hàng tiếp tục Checkout từ mẫu hoa đã chọn.
2. Hệ thống kiểm tra tồn kho các thành phần của Combo.
3. Hệ thống xác định Core còn đủ hàng nhưng một hoặc nhiều Support hết hàng.
4. Hệ thống **vẫn cho phép** khách hàng tiếp tục Checkout.
5. Hệ thống ghi nhận các thành phần Support cần thay thế.
6. Sau khi Order được tạo, Staff Hoa Theo Mùa chủ động liên hệ trực tiếp với khách hàng.
7. Staff thông báo tình trạng Support và thống nhất thành phần thay thế với khách hàng.
8. Hệ thống **không tự động thay thế** Support khi Staff và khách hàng chưa thống nhất.

#### ALT-03 — Thoát Checkout
1. Nếu khách hàng đóng tab, rời khỏi luồng Checkout sang trang khác, đăng xuất hoặc trình duyệt bị đóng/crash trước khi hoàn tất, hệ thống **hủy Checkout** và dữ liệu đã nhập trong Checkout bị mất.
2. **Refresh trang** không làm mất Checkout hiện tại.
3. Không tạo Order/PENDING.
4. Kết quả AI đã có trong History **không bị xóa** và quota đã dùng **không được hoàn**.

#### ALT-04 — Đặt lại mẫu hoa Custom AI đã sử dụng
1. Khách hàng mở lịch sử mẫu hoa Custom AI.
2. Khách hàng chọn một mẫu đã từng được dùng để tạo Order.
3. Khách hàng ấn chọn **"Đặt hàng ngay"**.
4. Backend kiểm tra mẫu hoa thuộc khách hàng hiện tại.
5. Backend kiểm tra Combo nguồn vẫn tồn tại.
6. Hệ thống kiểm tra tồn kho hiện tại của các thành phần trong Combo.
7. Nếu Combo nguồn còn hợp lệ và Core còn đủ hàng, hệ thống **khởi tạo Checkout mới** từ mẫu đó.
8. Nếu Support hết nhưng Core còn đủ, hệ thống vẫn cho phép Checkout và ghi nhận Support cần thay thế.
9. Các Order và liên kết lịch sử trước đó **vẫn được giữ nguyên**.
10. Khi Checkout mới được hoàn tất, hệ thống tạo thêm liên kết Mẫu hoa–Order mới.

---

### Exception Flows

#### EXC-01 — Thành phần Core không còn đủ hàng
1. Hệ thống kiểm tra tồn kho Core tại thời điểm khách hàng mở Checkout hoặc bấm **"Hoàn tất"**.
2. Nếu Core không đủ hàng tại thời điểm **mở Checkout**: hệ thống **không khởi tạo** Checkout.
3. Nếu Checkout đã được khởi tạo nhưng Core không đủ hàng tại thời điểm bấm **"Hoàn tất"**: hệ thống giữ Checkout hiện tại nhưng **không cho hoàn tất**.
4. Hệ thống không tạo Order.
5. Hệ thống hiển thị thông báo: **"Combo hiện không còn đủ thành phần chính để đặt hàng. Vui lòng chọn mẫu hoa khác."**

#### EXC-02 — Combo nguồn không còn khả dụng
1. Khách hàng chọn **"Đặt hàng ngay"** từ một mẫu hoa Custom AI đã tạo.
2. Backend kiểm tra Combo nguồn của mẫu hoa.
3. Hệ thống phát hiện Combo nguồn không tồn tại, đã bị vô hiệu hóa hoặc không còn được phép đặt hàng.
4. Hệ thống **không khởi tạo** Checkout.
5. Hệ thống không tạo Order hoặc liên kết Mẫu hoa–Order mới.
6. Mẫu hoa và các Order trước đó vẫn được **giữ nguyên** trong lịch sử.
7. Hệ thống hiển thị: **"Combo của mẫu hoa này hiện không còn khả dụng. Vui lòng chọn mẫu hoa khác."**

#### EXC-03 — Khách hàng không có quyền truy cập
1. Khách hàng gửi request khởi tạo, xem hoặc hoàn tất Checkout.
2. Backend kiểm tra quyền sở hữu của mẫu hoa AI và Checkout.
3. Hệ thống phát hiện một trong các trường hợp sau:
   - Mẫu hoa AI không thuộc khách hàng hiện tại.
   - Checkout không thuộc khách hàng hiện tại.
   - Checkout không được khởi tạo từ mẫu hoa AI đang được yêu cầu.
4. Backend từ chối request.
5. Hệ thống không tạo hoặc cập nhật Checkout.
6. Hệ thống không tạo Order.
7. Backend không trả về ảnh, thông tin mẫu hoa, dữ liệu Checkout hoặc thông tin nhạy cảm của khách hàng khác.
8. Hệ thống hiển thị thông báo chung: **"Bạn không có quyền thực hiện thao tác này."**

#### EXC-04 — Thông প্রান্ত giao hàng bắt buộc không hợp lệ
*(Ghi chú: Header "Thông tin giao hàng bắt buộc không hợp lệ")*

1. Khách hàng đang ở bước 1: thông tin cá nhân.
2. Khách hàng chưa nhập hoặc chưa chọn hợp lệ một trong các trường bắt buộc sau:
   - Số điện thoại người nhận.
   - Tỉnh/Thành phố.
   - Phường/Xã.
   - Địa chỉ chi tiết gồm số nhà, tên đường hoặc thông tin cần thiết để xác định điểm giao hàng.
3. Khách hàng ấn chọn **"Đặt hàng"**.
4. Hệ thống kiểm tra tính đầy đủ và hợp lệ của từng trường.
5. Nếu có ít nhất một trường không hợp lệ, hệ thống:
   - Không hoàn tất Checkout.
   - Không tạo Order.
   - Giữ lại các thông tin hợp lệ mà khách hàng đã nhập.
   - Đánh dấu và hiển thị lỗi tại từng trường không hợp lệ.
   - Đưa khách hàng đến trường không hợp lệ đầu tiên.
6. Khách hàng bổ sung hoặc điều chỉnh thông tin và có thể tiếp tục đặt hàng.

#### EXC-05 — Không lấy được phí giao hàng
1. Hệ thống gọi Ahamove nhưng không nhận được phí hợp lệ.
2. Hệ thống giữ Checkout, không tạo Order và yêu cầu khách hàng thử lại hoặc kiểm tra địa chỉ giao hàng.

---

## Acceptance Criteria

### AC-001 — Checkout là phiên tạm
- **Given:** Mẫu hoa hợp lệ
- **When:** Khách hàng chọn **"Đặt hàng ngay"**
- **Then:** Hệ thống mở Checkout
- **And:** Chưa tạo Order
- **And:** Chưa gán trạng thái PENDING.

### AC-002 — Một mẫu hoa
- **Given:** Checkout được mở
- **When:** Hệ thống khởi tạo Checkout
- **Then:** Checkout phải tham chiếu đúng **01 mẫu hoa nguồn**.

### AC-003 — Địa chỉ bắt buộc
- **Given:** Khách hàng chưa chọn hoặc nhập địa chỉ giao hàng hợp lệ
- **When:** Khách hàng tiếp tục từ Step 1
- **Then:** Hệ thống không cho chuyển sang bước tiếp theo

### AC-004 — Thiệp tùy chọn
- **Given:** Checkout không có thiệp
- **When:** Khách hàng hoàn tất hợp lệ
- **Then:** Hệ thống vẫn cho phép tạo Order.

### AC-005 — Core hết khi mở Checkout
- **Given:** Core hết hàng
- **When:** Khách hàng chọn **"Đặt hàng ngay"**
- **Then:** Không khởi tạo Checkout.

### AC-006 — Support hết
- **Given:** Core còn hàng nhưng support hết
- **When:** Khách hàng tiếp tục Checkout
- **Then:** Hệ thống vẫn cho đặt hàng
- **And:** FE thông báo cửa hàng sẽ thay support tương ứng.

### AC-007 — Thoát trang
- **Given:** Khách hàng đang có Checkout chưa hoàn tất.
- **When:** Khách hàng đóng tab, rời khỏi luồng Checkout, đăng xuất hoặc trình duyệt bị đóng/crash.
- **Then:** Checkout bị hủy và không thể khôi phục.
- **And:** Không tạo Order/PENDING.

### AC-008 — Đặt lại mẫu hoa Custom AI
- **Given:** Mẫu hoa Custom AI thuộc khách hàng và đã từng được dùng để tạo Order.
- **When:** Khách hàng chọn **"Đặt hàng ngay"** từ lịch sử mẫu hoa.
- **Then:** Backend phải kiểm tra Combo nguồn và tồn kho hiện tại.
- **And:** Nếu Combo nguồn còn khả dụng và Core còn đủ hàng, hệ thống cho phép khởi tạo Checkout mới.
- **And:** Nếu Combo nguồn không còn khả dụng hoặc Core hết hàng, hệ thống không khởi tạo Checkout.
- **And:** Các Order và liên kết lịch sử trước đó vẫn được giữ nguyên.

### AC-009 — Core hết khi hoàn tất Checkout
- **Given:** Checkout đã tồn tại và Core hết hàng
- **When:** Khách hàng bấm **"Hoàn tất"**
- **Then:** Giữ Checkout nhưng không tạo Order.

### AC-010 — Refresh Checkout
- **Given:** Checkout hợp lệ đang tồn tại.
- **When:** Khách hàng refresh trang Checkout.
- **Then:** Checkout hiện tại vẫn được giữ.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-081**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d007e95d-bc22-44e1-8cf3-0ec1bd33c827) | Checkout tạm | Khởi tạo Checkout từ mẫu hoa | Checkout chỉ là phiên chuẩn bị đặt hàng. | Khách hàng mở Checkout từ một mẫu hoa hợp lệ. | Hệ thống tạo phiên Checkout tạm để khách hàng nhập thông tin giao hàng, lựa chọn thiệp và chuẩn bị đặt hàng. | Order chỉ được tạo khi khách hàng bấm "Hoàn tất" và toàn bộ kiểm tra cuối hợp lệ. | Đức Bình | STORY-038 | Draft | v0 | 2026-08-14 |
| [**BR-082**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/39b4985d-a875-4939-9bd7-6bae7f6cf788) | Một mẫu hoa | Khởi tạo Checkout từ mẫu hoa | Mỗi Checkout bắt buộc tham chiếu đúng 01 mẫu hoa thông thường hoặc Custom AI. | Hệ thống khởi tạo Checkout. | Hệ thống gắn đúng 01 mẫu hoa nguồn vào Checkout. | Không cho phép một Checkout không có mẫu hoa nguồn hoặc tham chiếu nhiều mẫu hoa nguồn. | Đức Bình | STORY-038 | Draft | v0 | 2026-08-14 |
| [**BR-083**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7184a4b9-e6d0-49f0-a2b1-ef990f8dfd1e) | Tái sử dụng mẫu hoa Custom AI | Khởi tạo Checkout từ mẫu hoa | Mẫu hoa Custom AI được phép sử dụng để tạo nhiều Order nếu Combo nguồn vẫn còn đủ điều kiện đặt hàng. | Khách hàng chọn "Đặt hàng ngay" từ một mẫu đã từng được sử dụng. | Backend kiểm tra Combo nguồn và tồn kho hiện tại. Nếu Combo hợp lệ và Core còn đủ hàng, hệ thống khởi tạo Checkout mới. | Không cho phép mở Checkout nếu Combo nguồn không còn khả dụng hoặc Core không còn đủ hàng. Các Order và liên kết lịch sử trước đó không bị thay đổi. | Đức Bình | STORY-038 | Draft | v0 | 2026-08-14 |
| [**BR-084**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ecc9b927-b6fb-4f23-b272-cc92ffc588f2) | Tồn kho Combo | Khởi tạo Checkout từ mẫu hoa | Tồn kho Combo quyết định khả năng tiếp tục đặt hàng. | Khách hàng mở Checkout hoặc hoàn tất Checkout từ một mẫu hoa. | Core hết hàng làm Combo hết hàng và chặn đặt. Support hết không chặn đặt nếu core còn; cửa hàng chịu trách nhiệm thay thế support tương ứng. | Hệ thống không tự động thay thế Support khi Staff và khách hàng chưa thống nhất. | Đức Bình | STORY-038 | Draft | v0 | 2026-08-14 |
| [**BR-085**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/67c4719b-e909-49c4-b778-a83cf804c03d) | Thiệp | Khởi tạo Checkout từ mẫu hoa | Thiệp không bắt buộc trong Checkout. Một Checkout chỉ có tối đa 01 thiệp đã được khách hàng xác nhận và đang chọn tại một thời điểm. | Khách hàng thao tác trong Checkout. | Hệ thống cho phép Checkout có tối đa 01 thiệp đã được xác nhận và đang chọn hoặc không có thiệp. | Không cho phép một Checkout có nhiều hơn 01 thiệp đang chọn. | Đức Bình | STORY-038 | Draft | v0 | 2026-08-14 |
| [**BR-086**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6a8a89fa-c0a9-4569-a72e-e6c34e7fb4cb) | Thoát Checkout | Khởi tạo Checkout từ mẫu hoa | Checkout chỉ bị hủy khi khách hàng rời khỏi phiên thao tác, không bị hủy khi refresh trang. | Khách hàng đóng tab, rời khỏi luồng Checkout, đăng xuất hoặc trình duyệt bị đóng/crash trước khi bấm "Hoàn tất". | Hệ thống hủy Checkout, không tạo Order/PENDING và không cho phép khôi phục Checkout đó. | Refresh trang không làm mất Checkout hiện tại. History AI không bị xóa và quota đã dùng không được hoàn. | Đức Bình | STORY-038 | Draft | v0 | 2026-08-14 |
| [**BR-153**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/78f3d74b-fc51-4150-9bf3-85741fcdf683) | Tính phí giao hàng | Tính phí giao hàng | Phí giao hàng được lấy từ Ahamove theo địa chỉ giao hàng hiện tại. | Hệ thống cần hiển thị tạm tính hoặc khách hàng bấm "Hoàn tất". | Hệ thống gọi Ahamove để lấy phí ship hiện hành; khi "Hoàn tất" phải gọi lại và dùng kết quả cuối cùng để tạo Order. | Nếu Ahamove timeout, lỗi hoặc khu vực không được hỗ trợ, hệ thống không cho hoàn tất Checkout và hiển thị thông báo phù hợp. | Đức Bình | STORY-038 | Draft | v0 | 2026-08-17 |
| [**BR-156**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3bb381ff-ff91-4892-9309-ceed135f603c) | Giới hạn 03 lần tạo thiệp theo mẫu hoa | AI | Trong một lần đặt hàng từ một mẫu hoa nguồn, khách hàng được thực hiện tối đa 03 lần generate thiệp AI cho mẫu hoa đó. | Khách hàng thực hiện Tạo thiệp hoặc Tạo lại làm phát sinh một AI Job tạo thiệp mới. | Backend phải đồng thời kiểm tra: Mẫu hoa hiện tại chưa sử dụng hết 03 lượt generate thiệp. Khách hàng chưa sử dụng hết 10 lượt generate thiệp trong ngày. Nếu cả hai điều kiện đều thỏa, hệ thống cho phép tạo AI Job và ghi nhận 01 lượt vào cả hai giới hạn. | Nếu AI Job thất bại sau toàn bộ retry và không tạo được ảnh output hợp lệ, hệ thống hoàn lại lượt đã ghi nhận cho cả giới hạn theo mẫu hoa và quota ngày. Các thao tác Xác nhận thiệp, chọn thiệp đã có từ History và tải xuống thiệp không được tính là lượt generate. | Đức Bình | STORY-038 | Draft | v0 | 2026-08-19 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-081 | [Checkout tạm](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d007e95d-bc22-44e1-8cf3-0ec1bd33c827) |
| BR-082 | [Một mẫu hoa](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/39b4985d-a875-4939-9bd7-6bae7f6cf788) |
| BR-083 | [Tái sử dụng mẫu hoa Custom AI](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7184a4b9-e6d0-49f0-a2b1-ef990f8dfd1e) |
| BR-084 | [Tồn kho Combo](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ecc9b927-b6fb-4f23-b272-cc92ffc588f2) |
| BR-085 | [Thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/67c4719b-e909-49c4-b778-a83cf804c03d) |
| BR-086 | [Thoát Checkout](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/6a8a89fa-c0a9-4569-a72e-e6c34e7fb4cb) |
| BR-153 | [Tính phí giao hàng](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/78f3d74b-fc51-4150-9bf3-85741fcdf683) |
| BR-156 | [Giới hạn 03 lần tạo thiệp theo mẫu hoa](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3bb381ff-ff91-4892-9309-ceed135f603c) |

### Dependencies
- [**STORY-030**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [**STORY-033**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9)

---

## Non-Functional Requirements

- Backend kiểm tra **ownership** và trạng thái tài nguyên.
- Kiểm tra giá và tồn kho cuối phải **thực hiện lại** khi bấm **"Hoàn tất"**.
- Khi mở Checkout, hệ thống chỉ kiểm tra tồn kho hiện tại và **chưa reserve** tồn. Khi khách bấm **"Hoàn tất"**, hệ thống phải kiểm tra lại và **reserve tồn kho atomically** cùng quá trình tạo Order. Nếu reserve thất bại, không tạo Order.
- API Checkout trong điều kiện bình thường đạt **p95 ≤ 2 giây**, không tính dịch vụ bên thứ ba bị chậm.

---

## Out of Scope

- Thanh toán Order.
- Quản lý tồn kho phía cửa hàng.
- Admin thay thế Support.
- Generate mẫu hoa AI và thiệp AI.