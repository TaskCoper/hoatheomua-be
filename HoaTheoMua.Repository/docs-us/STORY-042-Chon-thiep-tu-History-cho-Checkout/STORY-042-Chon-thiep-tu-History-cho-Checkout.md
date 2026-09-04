# STORY-042 — Chọn thiệp từ History cho Checkout

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã đăng nhập và đang thực hiện Checkout, tôi muốn chọn lại một thiệp đã tạo trong History, để sử dụng thiệp đó cho Checkout mà không phải gọi AI tạo lại. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |

---

## Context

Chức năng được mở từ Checkout thông qua lựa chọn **"Chọn thiệp từ lịch sử"**.

- Hệ thống chỉ hiển thị các thiệp có History record thuộc khách hàng hiện tại và **từng có ảnh output hợp lệ**.
- Một Checkout có tối đa **01 thiệp đang chọn**. Chọn một thiệp khác sẽ **thay thế** thiệp hiện tại nhưng **không xóa** bất kỳ History record nào.
- Thiệp đã từng liên kết với Order **vẫn được phép** chọn cho Checkout mới.
- Việc chọn lại **không gọi AI**, **không sao chép ảnh**, **không tạo History record** và **không trừ quota**.
- Việc tạo Order, tính bill cuối và tạo liên kết Thiệp–Order thuộc **STORY-039**.

---

## Conditions

### Preconditions
- Khách hàng đã đăng nhập.
- Checkout tồn tại, thuộc khách hàng hiện tại và **chưa hoàn tất**.

### Trigger
> Khách hàng chọn **"Chọn thiệp từ lịch sử"** hoặc thao tác bỏ thiệp hiện tại tại Checkout.

---

## Flow

### Main Flow — Chọn thiệp từ History

1. Khách hàng mở chức năng **"Chọn thiệp từ lịch sử"** tại Checkout.
2. Backend kiểm tra Checkout tồn tại, thuộc khách hàng hiện tại và chưa hoàn tất.
3. Hệ thống lấy danh sách thiệp có History record thuộc khách hàng và có ảnh output hợp lệ.
4. Hệ thống hiển thị danh sách thiệp theo **thời điểm generate gần nhất, mới nhất trước**.
5. Khách hàng chọn một thiệp.
6. Backend kiểm tra **quyền sở hữu** và **khả dụng của file ảnh**.
7. Hệ thống gắn thiệp đã chọn làm **thiệp đã được xác nhận và đang chọn** của Checkout.
8. Nếu Checkout đã có thiệp khác, hệ thống **gỡ thiệp cũ** khỏi Checkout nhưng vẫn **giữ thiệp đó trong History**.
9. Hệ thống không gọi AI, không sao chép ảnh, không tạo History record và không trừ quota.
10. Order Summary hiển thị **thiệp đang chọn** và giá tạm tính hiện hành.

---

### Alternative Flows

#### ALT-01 — Không chọn thiệp
1. Khách hàng đóng danh sách History và tiếp tục Checkout **mà không thay đổi** thiệp đang chọn.
2. Nếu Checkout chưa có thiệp, khách hàng có thể tiếp tục không kèm thiệp.

#### ALT-02 — Thay đổi thiệp đang chọn
1. Khách hàng chọn một thiệp khác từ History.
2. Thiệp mới **thay thế** thiệp hiện tại của Checkout.
3. Các thiệp còn lại vẫn được **giữ trong History**.

#### ALT-03 — Chọn lại thiệp đã từng dùng cho Order
1. Hệ thống vẫn cho phép chọn thiệp nếu thiệp thuộc khách hàng và file ảnh còn khả dụng.
2. Các Order và liên kết trước đó **không bị thay đổi**.

#### ALT-04 — Template cũ không còn khả dụng
1. Thiệp đã tạo thành công vẫn được phép chọn lại nếu **file ảnh còn khả dụng**.
2. Việc Template nguồn ngừng khả dụng chỉ **chặn generate mới** bằng Template đó.

#### ALT-05 — Bỏ thiệp khỏi Checkout
1. Khách hàng bỏ chọn thiệp hiện tại.
2. Hệ thống **gỡ thiệp khỏi Checkout** nhưng vẫn giữ History record.
3. Hệ thống cho phép khách hàng tiếp tục Checkout **không có thiệp**.

---

### Exception Flows

#### EXC-01 — Không có thiệp trong History
1. Khách hàng mở chức năng **"Chọn thiệp từ lịch sử"** tại Checkout.
2. Hệ thống kiểm tra danh sách History của khách hàng hiện tại.
3. Hệ thống không tìm thấy thiệp phù hợp trong History.
4. Hệ thống hiển thị **empty state**.
5. Hệ thống cho phép khách hàng quay lại tạo thiệp mới hoặc tiếp tục Checkout không có thiệp.

#### EXC-02 — Checkout không hợp lệ
1. Khách hàng thực hiện thao tác chọn thiệp từ History.
2. Backend kiểm tra Checkout.
3. Backend phát hiện Checkout không tồn tại, không thuộc khách hàng hiện tại hoặc đã hoàn tất.
4. Backend từ chối thao tác.
5. Hệ thống **không thay đổi** thiệp đang chọn.

#### EXC-03 — Không có quyền với thiệp
1. Khách hàng gửi request chọn thiệp.
2. Backend kiểm tra quyền sở hữu của History record.
3. Backend phát hiện History record **không thuộc** khách hàng hiện tại.
4. Backend từ chối request.
5. Hệ thống không gắn thiệp vào Checkout.
6. Hệ thống không trả ảnh hoặc metadata nhạy cảm.

#### EXC-04 — File ảnh không còn khả dụng
1. Khách hàng chọn một thiệp từ History.
2. Backend kiểm tra trạng thái khả dụng của file ảnh.
3. Hệ thống phát hiện file ảnh **không còn khả dụng**.
4. Hệ thống **không cho chọn** thiệp.
5. Hệ thống giữ History record và metadata.
6. Hệ thống hiển thị: **"Ảnh không còn khả dụng."**

#### EXC-05 — Không tải được danh sách History
1. Khách hàng mở chức năng **"Chọn thiệp từ lịch sử"** tại Checkout.
2. Hệ thống thực hiện tải danh sách History.
3. Hệ thống không tải được danh sách History.
4. Hệ thống hiển thị **error state**.
5. Hệ thống **giữ nguyên** thiệp đang chọn trong Checkout.
6. Hệ thống cho phép khách hàng tải lại.

#### EXC-06 — Request chọn thiệp bị gửi trùng
1. Khách hàng gửi request chọn thiệp nhiều lần hoặc request bị gửi lại.
2. Backend xử lý theo **cơ chế idempotent**.
3. Hệ thống không tạo nhiều liên kết tạm.
4. Hệ thống không làm thay đổi dữ liệu History và quota.

---

## Acceptance Criteria

### AC-001 — Hiển thị thiệp thuộc khách hàng
- **Given:** Checkout hợp lệ.
- **When:** Khách hàng mở chức năng chọn thiệp từ History.
- **Then:** Hệ thống chỉ hiển thị các thiệp **thuộc khách hàng hiện tại** có ảnh output hợp lệ.

### AC-002 — Chọn thiệp thành công
- **Given:** Thiệp thuộc khách hàng và file ảnh còn khả dụng.
- **When:** Khách hàng chọn thiệp.
- **Then:** Hệ thống gắn thiệp làm **thiệp đã được xác nhận và đang chọn** của Checkout.
- **And:** Order Summary hiển thị thiệp đã chọn.

### AC-003 — Một thiệp trong Checkout
- **Given:** Checkout đã có thiệp A.
- **When:** Khách hàng chọn thiệp B.
- **Then:** B thay thế A trong Checkout.
- **And:** A vẫn được **giữ trong History**.

### AC-004 — Không gọi AI và không ảnh hưởng quota
- **Given:** Thiệp đã có History record hợp lệ.
- **When:** Khách hàng chọn thiệp cho Checkout.
- **Then:** Hệ thống **không gọi AI**.
- **And:** Không sao chép ảnh.
- **And:** Không tạo History record mới.
- **And:** Không thay đổi quota.

### AC-005 — Tái sử dụng thiệp đã từng đặt hàng
- **Given:** Thiệp đã từng liên kết với một hoặc nhiều Order.
- **When:** Khách hàng chọn thiệp cho Checkout mới.
- **Then:** Hệ thống vẫn cho phép chọn nếu thiệp thuộc khách hàng và file còn khả dụng.
- **And:** Các Order và liên kết trước đó **không bị thay đổi**.

### AC-006 — Template nguồn không còn khả dụng
- **Given:** Template nguồn của thiệp đã ngừng khả dụng.
- **When:** Khách hàng chọn lại thiệp có file ảnh hợp lệ.
- **Then:** Hệ thống **vẫn cho phép** gắn thiệp vào Checkout.

### AC-007 — File ảnh không còn khả dụng
- **Given:** History record còn tồn tại nhưng file ảnh mất hoặc hỏng.
- **When:** Khách hàng chọn thiệp.
- **Then:** Hệ thống **không gắn** thiệp vào Checkout.
- **And:** Hiển thị **"Ảnh không còn khả dụng."**

### AC-008 — Ownership
- **Given:** Thiệp thuộc khách hàng khác.
- **When:** User gửi request chọn thiệp bằng ID hoặc URL.
- **Then:** Backend từ chối.
- **And:** Không gắn thiệp vào Checkout.
- **And:** Không trả ảnh hoặc metadata nhạy cảm.

### AC-009 — Checkout không hợp lệ
- **Given:** Checkout không tồn tại, không thuộc khách hàng hoặc đã hoàn tất.
- **When:** Khách hàng chọn thiệp.
- **Then:** Backend từ chối.
- **And:** Không thay đổi thiệp đang chọn.

### AC-010 — Bỏ thiệp khỏi Checkout
- **Given:** Checkout đang có một thiệp được chọn.
- **When:** Khách hàng chọn bỏ thiệp.
- **Then:** Hệ thống **gỡ thiệp khỏi Checkout**.
- **And:** Vẫn giữ thiệp trong History.
- **And:** Cho phép tiếp tục Checkout không có thiệp.

### AC-011 — Không tải được danh sách History
- **Given:** Checkout hợp lệ và khách hàng mở chức năng chọn thiệp từ History.
- **When:** Hệ thống không tải được danh sách History.
- **Then:** Hệ thống hiển thị **error state**.
- **And:** Giữ nguyên thiệp đang chọn trong Checkout.
- **And:** Cho phép khách hàng tải lại.

### AC-012 — Request chọn thiệp bị gửi trùng
- **Given:** Khách hàng chọn một thiệp hợp lệ cho Checkout.
- **When:** Request chọn thiệp bị gửi lặp lại.
- **Then:** Backend xử lý **idempotent**.
- **And:** Không tạo nhiều liên kết tạm cho cùng một thiệp trong Checkout.
- **And:** Không thay đổi History hoặc quota.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|
| **BR-110** | Một thiệp trong Checkout | Chọn thiệp từ History cho Checkout | Mỗi Checkout có tối đa 01 thiệp đang chọn. Thiệp được chọn sau thay thế thiệp đang chọn trước đó. | Khách hàng chọn thiệp từ History cho Checkout. | Hệ thống gắn thiệp được chọn làm thiệp hiện tại của Checkout. | Không cho phép một Checkout có nhiều hơn 01 thiệp đang chọn. | Draft | v0 | 2026-08-14 |
| **BR-111** | Quyền sở hữu | Chọn thiệp từ History cho Checkout | Khách hàng chỉ được xem và chọn History record thuộc tài khoản của mình. | Khách hàng xem hoặc chọn thiệp từ History. | Backend kiểm tra quyền sở hữu của History record. | Không cho phép xem hoặc chọn History record không thuộc tài khoản khách hàng hiện tại. | Draft | v0 | 2026-08-14 |
| **BR-112** | Tái sử dụng thiệp | Chọn thiệp từ History cho Checkout | Thiệp đã từng liên kết với Order vẫn được phép chọn cho Checkout mới nếu file ảnh còn khả dụng. | Khách hàng chọn lại thiệp đã từng liên kết với Order. | Hệ thống vẫn cho phép chọn thiệp cho Checkout mới nếu file ảnh còn khả dụng. | Không cho phép chọn nếu file ảnh không còn khả dụng. | Draft | v0 | 2026-08-14 |
| **BR-113** | Không tạo dữ liệu AI mới | Chọn thiệp từ History cho Checkout | Chọn thiệp từ History không gọi AI, không sao chép ảnh, không tạo History record và không tiêu hao quota. | Khách hàng chọn thiệp từ History cho Checkout. | Hệ thống gắn thiệp vào Checkout mà không tạo dữ liệu AI mới. | Không gọi AI, không sao chép ảnh, không tạo History record và không tiêu hao quota. | Draft | v0 | 2026-08-14 |
| **BR-114** | Liên kết tạm với Checkout | Chọn thiệp từ History cho Checkout | Việc chọn thiệp chỉ thay đổi thiệp hiện tại của Checkout. Liên kết Thiệp–Order chỉ được tạo khi Order được tạo thành công theo STORY-039. | Khách hàng chọn thiệp từ History cho Checkout. | Hệ thống chỉ cập nhật thiệp hiện tại của Checkout. | Không tạo liên kết Thiệp–Order trước khi Order được tạo thành công theo STORY-039. | Draft | v0 | 2026-08-14 |
| **BR-115** | Template ngừng khả dụng | Chọn thiệp từ History cho Checkout | Template nguồn ngừng khả dụng không làm mất quyền sử dụng lại thiệp đã tạo thành công, nếu file ảnh còn khả dụng. | Khách hàng chọn lại thiệp có Template nguồn đã ngừng khả dụng. | Hệ thống vẫn cho phép sử dụng lại thiệp đã tạo thành công nếu file ảnh còn khả dụng. | Template nguồn ngừng khả dụng chỉ chặn generate mới bằng Template đó. | Draft | v0 | 2026-08-14 |
| **BR-116** | Giá tạm tính | Chọn thiệp từ History cho Checkout | Order Summary sử dụng giá thiệp hiện hành để hiển thị tạm tính. Giá cuối được kiểm tra lại khi hoàn tất Checkout theo STORY-039. | Order Summary hiển thị thiệp đang chọn. | Hệ thống sử dụng giá thiệp hiện hành để hiển thị tạm tính. | Giá cuối được kiểm tra lại khi hoàn tất Checkout theo STORY-039. | Draft | v0 | 2026-08-14 |
| **BR-117** | Bảo toàn History | Chọn thiệp từ History cho Checkout | Chọn, thay thế hoặc bỏ thiệp khỏi Checkout không xóa hoặc thay đổi History record. | Khách hàng chọn, thay thế hoặc bỏ thiệp khỏi Checkout. | Hệ thống giữ nguyên History record. | Không xóa hoặc thay đổi History record khi thao tác với thiệp trong Checkout. | Draft | v0 | 2026-08-14 |

---

## References

### Business Rules
`BR-110` · `BR-111` · `BR-112` · `BR-113` · `BR-114` · `BR-115` · `BR-116` · `BR-117`

### Dependencies
- **STORY-038** — Khởi tạo Checkout từ mẫu hoa
- **STORY-045**

---

## Non-Functional Requirements

- Danh sách History được **phân trang phía server** và sắp xếp mới nhất trước.
- Tải danh sách History **p95 ≤ 1.5 giây** với page size cấu hình, không tính thời gian tải ảnh từ storage.
- Backend kiểm tra **ownership** của Checkout và History record trong mọi request.
- API danh sách **không trả binary ảnh** hoặc expose đường dẫn storage nội bộ.
- UI có đầy đủ: **loading**, **empty** và **error state**.
- Một file ảnh lỗi **không được làm hỏng** toàn bộ danh sách.

---

## Out of Scope

- Generate hoặc tạo lại thiệp bằng AI.
- Hoàn tất Checkout, tạo Order và thanh toán.
- Tạo liên kết Thiệp–Order và tính bill cuối.
- Tải xuống, chia sẻ, chỉnh sửa hoặc xóa thiệp.
