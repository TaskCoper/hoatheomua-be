# STORY-039 — Khách hàng hoàn tất Checkout và thanh toán Order

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã đăng nhập, tôi muốn hoàn tất Checkout và thanh toán Order, để xác nhận đơn hàng và tiếp tục các bước xử lý tương ứng với lựa chọn thiệp của mình. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Danh Nguyen |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Checkout khi vào STORY-039 có thể thuộc một trong các trường hợp: không có thiệp, có Thiệp Miễn phí, có cấu hình Thiệp Custom mới từ STORY-035 hoặc có thiệp đã tạo được chọn lại từ History theo STORY-042.
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

> - Khách hàng chọn "Hoàn tất" hoặc thao tác tương ứng để xác nhận Checkout và tiến hành thanh toán.


---

## Flow

### Main Flow

1. Khách hàng chọn hoàn tất Checkout.
2. Backend kiểm tra Checkout tồn tại, thuộc khách hàng hiện tại và chưa được hoàn tất.
3. Backend kiểm tra lại trạng thái hợp lệ của bó hoa và dữ liệu cần thiết trong Checkout.
4. Nếu Checkout có thiệp, backend xác định lựa chọn thiệp hiện tại là Thiệp Miễn phí, Thiệp Custom mới được cấu hình hoặc thiệp được chọn từ History.
5. Hệ thống tính lại tổng tiền cuối của Checkout theo dữ liệu hợp lệ hiện tại.
6. Nếu Checkout có Thiệp Custom mới, hệ thống lưu snapshot Nội dung thiệp, Size, Hình thức, Template, giá áp dụng và số lượng từ đã thanh toán.
7. Hệ thống tạo Order từ Checkout và lưu snapshot các thông tin cần thiết tại thời điểm tạo Order.
8. Hệ thống khởi tạo giao dịch thanh toán cho Order.
9. Khách hàng thực hiện thanh toán.
10. Hệ thống nhận và xác minh kết quả thanh toán.
11. Nếu thanh toán thành công, hệ thống cập nhật trạng thái thanh toán của Order.
12. Hệ thống xử lý bước tiếp theo theo lựa chọn thiệp của Order.
13. Khách hàng được điều hướng đến bước tiếp theo phù hợp.

### Alternative Flows

#### ALT-01 — Order không có thiệp
- Khách hàng hoàn tất Checkout không có thiệp.
- Hệ thống tạo Order và tính bill không bao gồm giá thiệp.
- Sau khi thanh toán thành công, Order tiếp tục quy trình xử lý bình thường.
- Không tạo AI Job hoặc liên kết thiệp.

#### ALT-02 — Order có Thiệp Miễn phí
- Khách hàng hoàn tất Checkout với Thiệp Miễn phí.
- Hệ thống lưu thông tin Thiệp Miễn phí vào Order theo cấu hình đã xác nhận.
- Hệ thống áp dụng giá theo rule Thiệp Miễn phí.
- Sau khi thanh toán thành công, Order tiếp tục xử lý theo nghiệp vụ Thiệp Miễn phí.

#### ALT-03 — Order có Thiệp Custom mới
- Khách hàng hoàn tất Checkout với cấu hình Thiệp Custom mới.
- Hệ thống snapshot cấu hình Thiệp Custom, giá áp dụng và số lượng từ đã thanh toán vào Order.
- Hệ thống không tạo AI Job trước hoặc trong quá trình thanh toán.
- Sau khi Payment SUCCESS, hệ thống xác định Order đủ điều kiện tạo Thiệp Custom.
- Hệ thống hiển thị thao tác "Thanh toán thành công. Tiếp tục tạo thiệp."
- Khách hàng được điều hướng sang chức năng tạo Thiệp Custom.
- Nếu khách hàng chỉnh sửa Nội dung thiệp trước khi tạo ảnh AI, hệ thống chỉ cho phép chỉnh trong phạm vi số lượng từ đã thanh toán.

#### ALT-04 — Order dùng thiệp từ History
- Khách hàng đã chọn thiệp từ History phù hợp với mẫu hoa của Checkout.
- Hệ thống không gọi AI và không tạo History record mới.
- Khi Order được tạo thành công, hệ thống tạo liên kết giữa thiệp đã chọn và Order.
- Các Order đã từng sử dụng thiệp đó không bị thay đổi.
- Việc tái sử dụng không làm thay đổi History record.

#### ALT-05 — Payment đang chờ xử lý
- Gateway trả trạng thái PENDING.
- Order giữ trạng thái thanh toán tương ứng.
- Không cho phép tạo Thiệp Custom mới.
- Không tạo AI Job.
- Hệ thống cho khách hàng xem trạng thái thanh toán hoặc tiếp tục theo cơ chế kiểm tra payment hiện hành.

#### ALT-06 — Thanh toán lại
- Khách hàng thực hiện lại thanh toán cho cùng Order hợp lệ.
- Hệ thống không tạo thêm Order mới chỉ vì retry payment.
- Nếu payment sau đó SUCCESS, hệ thống tiếp tục flow theo loại thiệp đã snapshot trong Order.

### Exception Flows

#### EXC-01 — Checkout không hợp lệ
- Backend phát hiện Checkout không tồn tại, không thuộc khách hàng hiện tại hoặc đã được hoàn tất.
- Hệ thống từ chối tạo Order.
- Không tạo Payment.
- Không thay đổi dữ liệu Checkout/Order khác.

#### EXC-02 — Dữ liệu Checkout thay đổi hoặc không còn hợp lệ
- Backend phát hiện dữ liệu không còn hợp lệ khi hoàn tất Checkout.
- Hệ thống không tạo Order.
- Hệ thống yêu cầu khách hàng cập nhật Checkout.

#### EXC-03 — Không tính được giá cuối
- Hệ thống không hoàn tất Checkout.
- Không tạo Order với giá không xác định.
- Hiển thị lỗi phù hợp.

#### EXC-04 — Lỗi tạo Order
- Hệ thống gặp lỗi trong quá trình tạo Order hoặc dữ liệu liên quan.
- Hệ thống không để lại Order, Bill hoặc liên kết thiệp ở trạng thái dở dang hoặc không đồng nhất.
- Khách hàng có thể thử lại khi lỗi được xử lý.

#### EXC-05 — Payment FAILED
- Order đã được tạo nhưng thanh toán thất bại.
- Hệ thống cập nhật trạng thái Payment FAILED.
- Không tạo Thiệp Custom.
- Không tạo AI Job.
- Không tạo History từ cấu hình Custom mới.
- Khách hàng được phép xử lý thanh toán lại theo rule hiện hành nếu Order còn hợp lệ.

#### EXC-06 — Payment không xác minh được
- Gateway trả về nhưng backend không xác minh được payment thành công.
- Hệ thống không được coi Order đã thanh toán.
- Hệ thống xử lý Order theo trạng thái payment xác thực cuối cùng.

#### EXC-07 — Request hoàn tất Checkout bị gửi trùng
- Backend xử lý idempotent.
- Cùng một Checkout không được tạo nhiều Order do request lặp.
- Không tạo nhiều giao dịch/link thiệp ngoài nghiệp vụ cho phép.

---

## Acceptance Criteria

### AC-001
- **Given**: Checkout hợp lệ, thuộc khách hàng hiện tại và đủ điều kiện hoàn tất.
- **When**: Khách hàng xác nhận hoàn tất Checkout.
- **Then**:
  - Hệ thống tạo đúng một Order từ Checkout.
  - Order lưu các thông tin cần thiết tại thời điểm tạo.

### AC-002
- **Given**: Checkout có dữ liệu hợp lệ.
- **When**: Hệ thống hoàn tất Checkout.
- **Then**:
  - Hệ thống tính tổng tiền cuối theo các thành phần áp dụng trong Checkout.
  - Giá được lưu cho Order không bị thay đổi bởi việc Admin cập nhật cấu hình giá sau đó.

### AC-003
- **Given**: Checkout không có thiệp.
- **When**: Order được tạo và thanh toán thành công.
- **Then**: Hệ thống hoàn tất flow Order mà không tạo hoặc liên kết thiệp.

### AC-004
- **Given**: Checkout chứa cấu hình Thiệp Custom mới.
- **When**: Order chưa có Payment SUCCESS.
- **Then**:
  - Hệ thống không cho phép tạo Thiệp Custom.
  - Không tạo AI Job, ảnh thiệp hoặc History record.

### AC-005
- **Given**: Order chứa cấu hình Thiệp Custom mới.
- **When**: Payment của Order được xác minh thành công.
- **Then**:
  - Hệ thống cho phép khách hàng tiếp tục bước tạo Thiệp Custom.
  - Hệ thống không coi cấu hình tại Checkout là một thiệp đã generate.
  - Nếu khách hàng chỉnh sửa Nội dung thiệp trước khi tạo AI, nội dung sau chỉnh sửa không được vượt quá số lượng từ đã thanh toán.

### AC-006
- **Given**: Checkout đang sử dụng một thiệp hợp lệ được chọn từ History.
- **When**: Order được tạo thành công.
- **Then**:
  - Hệ thống liên kết thiệp đã chọn với Order.
  - Không gọi AI hoặc tạo History record mới.

### AC-007
- **Given**: Order đã được tạo.
- **When**: Payment được xác định là FAILED.
- **Then**: Không tạo AI Job hoặc History record từ cấu hình Custom.

### AC-008
- **Given**: Order đã được tạo.
- **When**: Payment chưa được xác minh thành công.
- **Then**: Hệ thống không xử lý Order như đã thanh toán.

### AC-009
- **Given**: Checkout đã tạo thành công một Order từ request hoàn tất hợp lệ.
- **When**: Request hoàn tất Checkout bị gửi lại.
- **Then**:
  - Backend không tạo thêm Order cho cùng Checkout.
  - Hệ thống không tạo trùng Payment hoặc liên kết thiệp ngoài nghiệp vụ cho phép.

### AC-010
- **Given**: Checkout hợp lệ và đủ điều kiện hoàn tất.
- **When**: Hệ thống gặp lỗi trong quá trình tạo Order hoặc dữ liệu liên quan.
- **Then**:
  - Hệ thống không để lại Order, Bill hoặc liên kết thiệp ở trạng thái dở dang hoặc không đồng nhất.
  - Checkout không được đánh dấu hoàn tất nếu Order chưa được tạo thành công.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Nguồn | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực | Ghi chú / Link logic |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| [BR-087](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f0773ccf-da70-4dd1-8328-bc7d68d35824) | Thời điểm tạo Order | Hoàn tất Checkout, tạo và thanh toán Order | Order chỉ được tạo sau khi khách hàng bấm “Hoàn tất” và backend xác nhận toàn bộ điều kiện cuối. | Khách hàng bấm “Hoàn tất”. | Backend xác nhận toàn bộ điều kiện cuối trước khi tạo Order. | Không tạo Order nếu điều kiện cuối không hợp lệ. | Google Sheet rule source | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 | — |
| [BR-088](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3b534fb0-c991-4ff7-89ad-eb4bd02921a8) | Trạng thái ban đầu | Hoàn tất Checkout, tạo và thanh toán Order | Order mới được tạo có trạng thái PENDING cho đến khi có Payment được xác minh thành công hoặc Order bị hủy theo rule hết hạn. | Order mới được tạo. | Hệ thống gán trạng thái PENDING cho Order. | Trạng thái PENDING vẫn được giữ khi khách hàng chưa bấm thanh toán hoặc lần thanh toán đầu thất bại. | Google Sheet rule source | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 | — |
| [BR-089](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35efbcf7-42f4-4d17-b1e3-a419221bb001) | Thời hạn PENDING và giải phóng tồn kho | Hoàn tất Checkout, tạo và thanh toán Order | Order ở trạng thái PENDING được duy trì tối đa 24 giờ kể từ thời điểm tạo. Trong thời gian này, hệ thống giữ số lượng tồn kho thực tế của các thành phần đã được ghi nhận cho Order. Thành phần Support không còn đủ hàng được ghi nhận là cần thay thế và không được reserve vượt quá tồn kho thực tế. Việc xác định thành phần Support thay thế được thực hiện sau khi Staff liên hệ và thống nhất với khách hàng, không phải tại thời điểm tạo Order. | Order đã ở trạng thái PENDING đủ 24 giờ và chưa có giao dịch thanh toán thành công. | Hệ thống chuyển Order sang trạng thái CANCELLED. Hệ thống giải phóng số lượng tồn kho các thành phần của Combo đã được giữ cho Order. Số lượng được giải phóng trở lại thành tồn kho khả dụng. Order vẫn được lưu và hiển thị trong lịch sử đơn hàng với trạng thái “Đã hủy”. Mẫu hoa Custom AI vẫn được giữ và tiếp tục liên kết với Order đã hủy. | Việc giải phóng tồn kho chỉ được thực hiện đúng một lần cho mỗi Order. Chỉ giải phóng đúng số lượng đã được giữ cho Order; không làm thay đổi phần tồn kho khác. Không giải phóng lại nếu tồn kho của Order đã được giải phóng trước đó. Mẫu hoa Custom AI không bị xóa hoặc chuyển về trạng thái Bản nháp. Order đã chuyển sang CANCELLED không được tiếp tục thanh toán. Thanh toán lại trước khi hết hạn không làm thay đổi mốc hết hạn ban đầu. Order PENDING hoặc CANCELLED do hết hạn không được phép tạo Thiệp Custom mới. | Google Sheet rule source | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 | — |
| [BR-090](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f3d3d0c2-21ae-42b0-864c-18027a6ebf6e) | Thử lại thanh toán | Hoàn tất Checkout, tạo và thanh toán Order | Khách hàng được thử thanh toán không giới hạn số lần khi Order còn PENDING và chưa hết hạn. | Khách hàng thử thanh toán lại. | Hệ thống cho phép thử thanh toán không giới hạn số lần. | Chỉ áp dụng khi Order còn PENDING và chưa hết hạn. | Google Sheet rule source | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 | — |
| [BR-091](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7bc0baa5-01b5-4fe1-98c6-6b525be8e406) | Tự động hủy | Hoàn tất Checkout, tạo và thanh toán Order | Sau 24 giờ chưa thanh toán thành công, hệ thống tự động chuyển Order sang CANCELLED và chặn thanh toán. | Order chưa thanh toán thành công sau 24 giờ. | Hệ thống tự động chuyển Order sang CANCELLED. | Order CANCELLED bị chặn thanh toán. | Google Sheet rule source | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 | — |
| [BR-092](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/92b589a3-f87b-4d15-a4e5-165a2be241a8) | Chốt bill | Hoàn tất Checkout, tạo và thanh toán Order | Backend tính giá sản phẩm, giá thiệp nếu có và phí giao hàng tại thời điểm hoàn tất Checkout. | Khách hàng hoàn tất Checkout. | Backend tính giá sản phẩm, giá thiệp nếu có và phí giao hàng. | Bill được chốt tại thời điểm hoàn tất Checkout. | Google Sheet rule source | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 | — |
| [BR-093](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/31338ae7-cd10-434c-a2f6-f671ea33a5e6) | Tồn kho | Hoàn tất Checkout, tạo và thanh toán Order | Core hết chặn tạo Order. Support hết không chặn nếu core còn và cửa hàng thay thế tương ứng. | Backend kiểm tra tồn kho trước khi tạo Order. | Core hết chặn tạo Order. Support hết không chặn nếu core còn và cửa hàng thay thế tương ứng. | Không tạo Order khi Core hết. | Google Sheet rule source | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 | — |
| [BR-094](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/807a720b-e138-442e-b91b-0f701766fd32) | Thiệp sau khi tạo Order | Hoàn tất Checkout, tạo và thanh toán Order | Khách hàng không được đổi hoặc gỡ thiệp sau khi Order đã được tạo. | Order đã được tạo. | Hệ thống không cho khách hàng đổi hoặc gỡ thiệp. | Không áp dụng trước thời điểm Order được tạo. | Google Sheet rule source | Đức Bình | STORY-039 | Draft | v0 | 2026-08-14 | — |
| [BR-272](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/767acec8-626b-48e7-9352-622cf0a55a61) | Giới hạn chỉnh sửa nội dung thiệp theo số lượng từ đã thanh toán | Tạo thiệp | Sau khi Order đã thanh toán thành công, nếu khách hàng chỉnh sửa Nội dung thiệp trước khi tạo ảnh AI, nội dung sau chỉnh sửa không được vượt quá số lượng từ đã được thanh toán trong Order. | Khách hàng chỉnh sửa Nội dung thiệp của Thiệp Custom sau khi Order đã thanh toán thành công và trước khi gửi yêu cầu tạo ảnh AI, bao gồm cả thao tác Tạo lại hợp lệ trong bước sau thanh toán. | Hệ thống đếm số lượng từ của Nội dung thiệp sau chỉnh sửa theo quy tắc đếm từ hiện hành. Hệ thống so sánh số lượng từ sau chỉnh sửa với số lượng từ đã được dùng để tính tiền và lưu trong snapshot Order. Nếu số lượng từ sau chỉnh sửa nhỏ hơn hoặc bằng số lượng từ đã thanh toán, hệ thống cho phép tiếp tục tạo thiệp AI. Nếu số lượng từ sau chỉnh sửa vượt quá số lượng từ đã thanh toán, hệ thống không cho phép tiếp tục tạo thiệp AI và yêu cầu khách hàng rút gọn nội dung hoặc cập nhật thanh toán theo nghiệp vụ được quy định riêng. | Không áp dụng rule này cho thao tác chọn thiệp đã tạo từ History cho Checkout, vì thao tác đó không mở biểu mẫu chỉnh sửa nội dung và không gọi AI. | Product discussion 2026-09-11; STORY-035; STORY-039 | Đức Bình | STORY-035; STORY-039 | Draft | v0 | 2026-09-11 | Rule này không tự định nghĩa quy trình thu thêm tiền khi khách hàng muốn tăng số lượng từ sau thanh toán; quy trình đó nằm ngoài phạm vi STORY-035 và STORY-039 nếu chưa được chốt riêng. Chức năng Tạo lại thiệp từ History không còn được hỗ trợ; Tạo lại hợp lệ chỉ diễn ra trong bước tạo Thiệp Custom sau khi Order đã thanh toán thành công. |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-087 | [BR-087](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f0773ccf-da70-4dd1-8328-bc7d68d35824) |
| BR-088 | [BR-088](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3b534fb0-c991-4ff7-89ad-eb4bd02921a8) |
| BR-089 | [BR-089](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35efbcf7-42f4-4d17-b1e3-a419221bb001) |
| BR-090 | [BR-090](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f3d3d0c2-21ae-42b0-864c-18027a6ebf6e) |
| BR-091 | [BR-091](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7bc0baa5-01b5-4fe1-98c6-6b525be8e406) |
| BR-092 | [BR-092](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/92b589a3-f87b-4d15-a4e5-165a2be241a8) |
| BR-093 | [BR-093](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/31338ae7-cd10-434c-a2f6-f671ea33a5e6) |
| BR-094 | [BR-094](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/807a720b-e138-442e-b91b-0f701766fd32) |
| BR-272 | [BR-272](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/767acec8-626b-48e7-9352-622cf0a55a61) |

### Dependencies

- [STORY-035](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)
- [STORY-038](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)

---

## Non-Functional Requirements

- Tạo Order và các liên kết liên quan phải nhất quán; không để Order hoặc link bị tạo trùng do double click/retry.
- Backend kiểm tra trạng thái Order trước mọi lần thanh toán.
- Job tự hủy phải idempotent.
- Không hiển thị lỗi cổng thanh toán hoặc stack trace nội bộ.
- API tạo Order mục tiêu p95 ≤ 3 giây, không tính thời gian hệ thống bên thứ ba.
- Event/giao dịch thanh toán phải truy vết được theo Order.

---

## Out of Scope

- Admin xử lý fulfillment.
- Chi tiết tích hợp payment provider.
- Chính sách refund tự động.
- Quản trị tồn kho.

---
