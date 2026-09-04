# STORY-039 — Khách hàng hoàn tất Checkout, tạo và thanh toán Order

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã đăng nhập, tôi muốn hoàn tất Checkout và thanh toán Order, để xác nhận đơn hàng và tiếp tục các bước xử lý tương ứng với lựa chọn thiệp của mình. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Checkout khi vào bước này có thể thuộc 4 trường hợp:
1. Không có thiệp.
2. Có Thiệp Miễn phí.
3. Có cấu hình Thiệp Custom mới từ STORY-035.
4. Có thiệp đã tạo được chọn lại từ History theo STORY-042.

Khi khách hàng hoàn tất Checkout, hệ thống kiểm tra lại dữ liệu Checkout, tính tổng tiền cuối, tạo Order và thực hiện thanh toán. 
- Thiệp Custom mới chưa được generate tại Checkout. Chỉ sau khi Order được thanh toán thành công, khách hàng mới được tiếp tục sang bước tạo Thiệp Custom. 
- Thiệp được chọn từ History không generate lại; sau khi Order được tạo thành công, hệ thống liên kết thiệp đã chọn với Order. 
- Payment không thành công không được làm phát sinh AI Job tạo Thiệp Custom.

---

## Conditions

### Preconditions
- Khách hàng đã đăng nhập.
- Checkout tồn tại và thuộc khách hàng hiện tại.
- Checkout chưa được hoàn tất.
- Bó hoa trong Checkout còn hợp lệ.
- Nếu Checkout có thiệp thì thông tin thiệp tương ứng đã hợp lệ.
- Checkout có đủ thông tin cần thiết để tính tổng tiền cuối.

### Trigger
> Khách hàng chọn **"Hoàn tất"** hoặc thao tác tương ứng để xác nhận Checkout và tiến hành thanh toán.

---

## Flow

### Main Flow: MF-01 – Tạo Order

1. Khách hàng chọn hoàn tất Checkout.
2. Backend kiểm tra Checkout tồn tại, thuộc khách hàng hiện tại và chưa được hoàn tất.
3. Backend kiểm tra lại trạng thái hợp lệ của bó hoa và dữ liệu cần thiết trong Checkout.
4. Nếu Checkout có thiệp, backend xác định lựa chọn thiệp hiện tại: Thiệp Miễn phí, Thiệp Custom mới được cấu hình, hoặc Thiệp được chọn từ History.
5. Hệ thống tính lại tổng tiền cuối của Checkout theo dữ liệu hợp lệ hiện tại.
6. Hệ thống tạo Order từ Checkout và lưu snapshot các thông tin cần thiết tại thời điểm tạo Order.
7. Hệ thống khởi tạo giao dịch thanh toán cho Order.
8. Khách hàng thực hiện thanh toán.
9. Hệ thống nhận và xác minh kết quả thanh toán.
10. Nếu thanh toán thành công, hệ thống cập nhật trạng thái thanh toán của Order.
11. Hệ thống xử lý bước tiếp theo theo lựa chọn thiệp của Order.
12. Khách hàng được điều hướng đến bước tiếp theo phù hợp.

---

### Alternative Flows

#### ALT-01 — Order không có thiệp
1. Khách hàng hoàn tất Checkout không có thiệp.
2. Hệ thống tạo Order và tính bill không bao gồm giá thiệp.
3. Sau khi thanh toán thành công, Order tiếp tục quy trình xử lý bình thường.
4. Không tạo AI Job hoặc liên kết thiệp.

#### ALT-02 — Order có Thiệp Miễn phí
1. Khách hàng hoàn tất Checkout với Thiệp Miễn phí.
2. Hệ thống lưu thông tin Thiệp Miễn phí vào Order theo cấu hình đã xác nhận.
3. Hệ thống áp dụng giá theo rule Thiệp Miễn phí.
4. Sau khi thanh toán thành công, Order tiếp tục xử lý theo nghiệp vụ Thiệp Miễn phí.

#### ALT-03 — Order có Thiệp Custom mới
1. Khách hàng hoàn tất Checkout với cấu hình Thiệp Custom mới.
2. Hệ thống snapshot cấu hình Thiệp Custom và giá áp dụng vào Order.
3. Hệ thống không tạo AI Job trước hoặc trong quá trình thanh toán.
4. Sau khi Payment SUCCESS, hệ thống xác định Order đủ điều kiện tạo Thiệp Custom.
5. Hệ thống hiển thị thao tác: “Thanh toán thành công. Tiếp tục tạo thiệp.”
6. Khách hàng được điều hướng sang chức năng tạo Thiệp Custom.

#### ALT-04 — Order dùng thiệp từ History
1. Khách hàng đã chọn thiệp từ History phù hợp với mẫu hoa của Checkout.
2. Hệ thống không gọi AI và không tạo History record mới.
3. Khi Order được tạo thành công, hệ thống tạo liên kết giữa thiệp đã chọn và Order.
4. Các Order đã từng sử dụng thiệp đó không bị thay đổi.
5. Việc tái sử dụng không làm thay đổi History record.

#### ALT-05 — Payment đang chờ xử lý
1. Nếu gateway trả trạng thái PENDING:
2. Order giữ trạng thái thanh toán tương ứng.
3. Không cho phép tạo Thiệp Custom mới.
4. Không tạo AI Job.
5. Hệ thống cho khách hàng xem trạng thái thanh toán hoặc tiếp tục theo cơ chế kiểm tra payment hiện hành.

#### ALT-06 — Thanh toán lại
1. Không tạo thêm Order mới chỉ vì retry payment.
2. Nếu payment sau đó SUCCESS, hệ thống tiếp tục flow theo loại thiệp đã snapshot trong Order.

---

### Exception Flows

#### EXC-01 — Checkout không hợp lệ
1. Backend phát hiện Checkout không tồn tại, không thuộc khách hàng hiện tại hoặc đã được hoàn tất.
2. Hệ thống từ chối tạo Order.
3. Không tạo Payment.
4. Không thay đổi dữ liệu Checkout/Order khác.

#### EXC-02 — Dữ liệu Checkout thay đổi hoặc không còn hợp lệ
1. Backend phát hiện dữ liệu không còn hợp lệ khi hoàn tất Checkout.
2. Hệ thống không tạo Order.
3. Hệ thống yêu cầu khách hàng cập nhật Checkout.

#### EXC-03 — Không tính được giá cuối
1. Hệ thống không hoàn tất Checkout.
2. Không tạo Order với giá không xác định.
3. Hiển thị lỗi phù hợp.

#### EXC-04 — Lỗi tạo Order
1. Hệ thống không để lại Order/Bill/liên kết thiệp ở trạng thái dở dang hoặc không đồng nhất.
2. Khách hàng có thể thử lại khi lỗi được xử lý.

#### EXC-05 — Payment FAILED
1. Order đã được tạo nhưng thanh toán thất bại.
2. Hệ thống cập nhật trạng thái Payment FAILED.
3. Không tạo Thiệp Custom, không tạo AI Job, không tạo History từ cấu hình Custom mới.
4. Khách hàng được phép xử lý thanh toán lại theo rule hiện hành nếu Order còn hợp lệ.

#### EXC-06 — Payment không xác minh được
1. Gateway trả về nhưng backend không xác minh được payment thành công.
2. Hệ thống không được coi Order đã thanh toán.
3. Hệ thống xử lý Order theo trạng thái payment xác thực cuối cùng.

#### EXC-07 — Request hoàn tất Checkout bị gửi trùng
1. Backend xử lý idempotent.
2. Cùng một Checkout không được tạo nhiều Order do request lặp.
3. Không tạo nhiều giao dịch/link thiệp ngoài nghiệp vụ cho phép.

---

## Acceptance Criteria

### AC-001 — Tạo Order thành công
- **Given:** Checkout hợp lệ, thuộc khách hàng hiện tại và đủ điều kiện hoàn tất.
- **When:** Khách hàng xác nhận hoàn tất Checkout.
- **Then:** Hệ thống tạo đúng một Order từ Checkout.
- **And:** Order lưu các thông tin cần thiết tại thời điểm tạo.

### AC-002 — Tính tổng tiền cuối
- **Given:** Checkout có dữ liệu hợp lệ.
- **When:** Hệ thống hoàn tất Checkout.
- **Then:** Hệ thống tính tổng tiền cuối theo các thành phần áp dụng trong Checkout.
- **And:** Giá được lưu cho Order không bị thay đổi bởi việc Admin cập nhật cấu hình giá sau đó.

### AC-003 — Không thiệp
- **Given:** Checkout không có thiệp.
- **When:** Order được tạo và thanh toán thành công.
- **Then:** Hệ thống hoàn tất flow Order mà không tạo hoặc liên kết thiệp.

### AC-004 — Thiệp Custom chưa được tạo trước thanh toán
- **Given:** Checkout chứa cấu hình Thiệp Custom mới.
- **When:** Order chưa có Payment SUCCESS.
- **Then:** Hệ thống không cho phép tạo Thiệp Custom.
- **And:** Không tạo AI Job, ảnh thiệp hoặc History record.

### AC-005 — Payment SUCCESS với Thiệp Custom
- **Given:** Order chứa cấu hình Thiệp Custom mới.
- **When:** Payment của Order được xác minh thành công.
- **Then:** Hệ thống cho phép khách hàng tiếp tục bước tạo Thiệp Custom.
- **And:** Hệ thống không coi cấu hình tại Checkout là một thiệp đã generate.

### AC-006 — Chọn thiệp từ History
- **Given:** Checkout đang sử dụng một thiệp hợp lệ được chọn từ History.
- **When:** Order được tạo thành công.
- **Then:** Hệ thống liên kết thiệp đã chọn với Order.
- **And:** Không gọi AI hoặc tạo History record mới.

### AC-007 — Payment FAILED
- **Given:** Order đã được tạo.
- **When:** Payment được xác định là FAILED.
- **Then:** Không tạo AI Job hoặc History record từ cấu hình Custom.

### AC-008 — Payment PENDING
- **Given:** Order đã được tạo.
- **When:** Payment chưa được xác minh thành công.
- **Then:** Hệ thống không xử lý Order như đã thanh toán.

### AC-009 — Request hoàn tất Checkout gửi trùng
- **Given:** Checkout đã tạo thành công một Order từ request hoàn tất hợp lệ.
- **When:** Request hoàn tất Checkout bị gửi lại.
- **Then:** Backend không tạo thêm Order cho cùng Checkout.
- **And:** Hệ thống không tạo trùng Payment hoặc liên kết thiệp ngoài nghiệp vụ cho phép.

### AC-010 — Lỗi tạo Order
- **Given:** Checkout hợp lệ và đủ điều kiện hoàn tất.
- **When:** Hệ thống gặp lỗi trong quá trình tạo Order hoặc dữ liệu liên quan.
- **Then:** Hệ thống không để lại Order, Bill hoặc liên kết thiệp ở trạng thái dở dang hoặc không đồng nhất.
- **And:** Checkout không được đánh dấu hoàn tất nếu Order chưa được tạo thành công.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-087**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f0773ccf-da70-4dd1-8328-bc7d68d35824) | Thời điểm tạo Order | Hoàn tất Checkout | Order chỉ được tạo sau khi khách hàng bấm “Hoàn tất” và backend xác nhận toàn bộ điều kiện cuối. | Khách hàng bấm “Hoàn tất”. | Backend xác nhận toàn bộ điều kiện cuối trước khi tạo Order. | Không tạo Order nếu điều kiện cuối không hợp lệ. | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 |
| [**BR-088**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3b534fb0-c991-4ff7-89ad-eb4bd02921a8) | Trạng thái ban đầu | Hoàn tất Checkout | Order mới được tạo có trạng thái PENDING cho đến khi có Payment được xác minh thành công hoặc Order bị hủy theo rule hết hạn. | Order mới được tạo. | Hệ thống gán trạng thái PENDING cho Order. | Trạng thái PENDING vẫn được giữ khi khách hàng chưa bấm thanh toán hoặc lần thanh toán đầu thất bại. | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 |
| [**BR-089**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35efbcf7-42f4-4d17-b1e3-a419221bb001) | Thời hạn PENDING và giải phóng tồn kho | Hoàn tất Checkout | Order ở trạng thái PENDING được duy trì tối đa 24 giờ. Hệ thống giữ tồn kho thực tế. | Order đã ở trạng thái PENDING đủ 24 giờ và chưa thanh toán. | Chuyển Order sang CANCELLED, giải phóng tồn kho. Order lưu trạng thái “Đã hủy”. | Giải phóng tồn kho thực hiện 1 lần. Mẫu hoa Custom AI không bị xóa. Order CANCELLED không được tiếp tục thanh toán. | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 |
| [**BR-090**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f3d3d0c2-21ae-42b0-864c-18027a6ebf6e) | Thử lại thanh toán | Hoàn tất Checkout | Khách hàng được thử thanh toán không giới hạn số lần khi Order còn PENDING và chưa hết hạn. | Khách hàng thử thanh toán lại. | Hệ thống cho phép thử thanh toán không giới hạn số lần. | Chỉ áp dụng khi Order còn PENDING và chưa hết hạn. | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 |
| [**BR-091**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7bc0baa5-01b5-4fe1-98c6-6b525be8e406) | Tự động hủy | Hoàn tất Checkout | Sau 24 giờ chưa thanh toán thành công, hệ thống tự động chuyển Order sang CANCELLED và chặn thanh toán. | Order chưa thanh toán thành công sau 24 giờ. | Hệ thống tự động chuyển Order sang CANCELLED. | Order CANCELLED bị chặn thanh toán. | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 |
| [**BR-092**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/92b589a3-f87b-4d15-a4e5-165a2be241a8) | Chốt bill | Hoàn tất Checkout | Backend tính giá sản phẩm, giá thiệp nếu có và phí giao hàng tại thời điểm hoàn tất Checkout. | Khách hàng hoàn tất Checkout. | Backend tính giá sản phẩm, giá thiệp nếu có và phí giao hàng. | Bill được chốt tại thời điểm hoàn tất Checkout. | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 |
| [**BR-093**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/31338ae7-cd10-434c-a2f6-f671ea33a5e6) | Tồn kho | Hoàn tất Checkout | Core hết chặn tạo Order. Support hết không chặn nếu Core còn và cửa hàng thay thế tương ứng. | Backend kiểm tra tồn kho trước khi tạo Order. | Core hết chặn tạo Order. Support hết không chặn nếu Core còn. | Không tạo Order khi Core hết. | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 |
| [**BR-094**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/807a720b-e138-442e-b91b-0f701766fd32) | Thiệp sau khi tạo Order | Hoàn tất Checkout | Khách hàng không được đổi hoặc gỡ thiệp sau khi Order đã được tạo. | Order đã được tạo. | Hệ thống không cho khách hàng đổi hoặc gỡ thiệp. | Không áp dụng trước thời điểm Order được tạo. | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-087 | [Thời điểm tạo Order](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f0773ccf-da70-4dd1-8328-bc7d68d35824) |
| BR-088 | [Trạng thái ban đầu](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3b534fb0-c991-4ff7-89ad-eb4bd02921a8) |
| BR-089 | [Thời hạn PENDING và giải phóng tồn kho](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35efbcf7-42f4-4d17-b1e3-a419221bb001) |
| BR-090 | [Thử lại thanh toán](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f3d3d0c2-21ae-42b0-864c-18027a6ebf6e) |
| BR-091 | [Tự động hủy](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7bc0baa5-01b5-4fe1-98c6-6b525be8e406) |
| BR-092 | [Chốt bill](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/92b589a3-f87b-4d15-a4e5-165a2be241a8) |
| BR-093 | [Tồn kho](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/31338ae7-cd10-434c-a2f6-f671ea33a5e6) |
| BR-094 | [Thiệp sau khi tạo Order](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/807a720b-e138-442e-b91b-0f701766fd32) |

### Dependencies
- [**STORY-038**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)

---

## Non-Functional Requirements

- Tạo Order và các liên kết liên quan phải **nhất quán**; không để Order hoặc link bị tạo trùng do double click/retry.
- Backend kiểm tra **trạng thái Order** trước mọi lần thanh toán.
- Job tự hủy phải **idempotent**.
- Không hiển thị lỗi cổng thanh toán hoặc stack trace nội bộ.
- API tạo Order mục tiêu **p95 ≤ 3 giây**, không tính thời gian hệ thống bên thứ ba.
- Event/giao dịch thanh toán phải **truy vết được** theo Order.

---

## Out of Scope

- Admin xử lý fulfillment.
- Chi tiết tích hợp payment provider.
- Chính sách refund tự động.
- Quản trị tồn kho.