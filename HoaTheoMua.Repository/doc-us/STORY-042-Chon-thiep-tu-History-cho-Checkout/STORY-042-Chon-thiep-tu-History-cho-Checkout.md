# STORY-042 — Khách hàng chọn thiệp từ History cho Checkout

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
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Chức năng được mở từ Checkout thông qua lựa chọn **“Chọn thiệp từ lịch sử”**.
- Hệ thống chỉ hiển thị các thiệp có History record thuộc khách hàng hiện tại, có ảnh output hợp lệ và được tạo từ đúng mẫu hoa đang được sử dụng trong Checkout hiện tại.
- Thiệp thuộc mẫu hoa khác không được hiển thị và không được phép chọn cho Checkout hiện tại.
- Một Checkout có tối đa 01 thiệp đang chọn.
- Chọn một thiệp khác sẽ thay thế thiệp hiện tại nhưng không xóa bất kỳ History record nào.
- Thiệp đã từng liên kết với Order vẫn được phép chọn lại nếu thiệp thuộc đúng mẫu hoa của Checkout hiện tại và file ảnh còn khả dụng.
- Việc chọn lại không gọi AI, không sao chép ảnh, không tạo History record và không trừ quota.
- Việc tạo Order, tính bill cuối và tạo liên kết Thiệp–Order thuộc STORY-039.

---

## Conditions

### Preconditions
- Khách hàng đã đăng nhập.
- Checkout tồn tại, thuộc khách hàng hiện tại và chưa hoàn tất.
- Checkout có mẫu hoa hiện tại để xác định phạm vi History được phép hiển thị và lựa chọn.

### Trigger
> Khách hàng chọn **“Chọn thiệp từ lịch sử”** hoặc thao tác bỏ thiệp hiện tại tại Checkout.

---

## Flow

### Main Flow: MF – Chọn thiệp từ History

1. Khách hàng mở chức năng “Chọn thiệp từ lịch sử” tại Checkout.
2. Backend kiểm tra Checkout tồn tại, thuộc khách hàng hiện tại và chưa hoàn tất.
3. Hệ thống lấy danh sách thiệp có History record thuộc khách hàng hiện tại, có ảnh output hợp lệ và được tạo từ đúng mẫu hoa đang được sử dụng trong Checkout hiện tại.
4. Hệ thống hiển thị danh sách thiệp theo thời điểm generate gần nhất, mới nhất trước.
5. Khách hàng chọn một thiệp.
6. Backend kiểm tra quyền sở hữu History record, quan hệ giữa thiệp và mẫu hoa hiện tại của Checkout, và trạng thái khả dụng của file ảnh.
7. Hệ thống gắn thiệp đã chọn làm thiệp đã được xác nhận và đang chọn của Checkout.
8. Nếu Checkout đã có thiệp khác, hệ thống gỡ thiệp cũ khỏi Checkout nhưng vẫn giữ thiệp đó trong History.
9. Hệ thống không gọi AI, không sao chép ảnh, không tạo History record và không trừ quota.
10. Order Summary hiển thị thiệp đang chọn và giá tạm tính hiện hành.

---

### Alternative Flows

#### ALT-01 — Không chọn thiệp
1. Khách hàng đóng danh sách History và tiếp tục Checkout mà không thay đổi thiệp đang chọn.
2. Nếu Checkout chưa có thiệp, khách hàng có thể tiếp tục không kèm thiệp.

#### ALT-02 — Thay đổi thiệp đang chọn
1. Khách hàng chọn một thiệp khác từ History.
2. Thiệp mới thay thế thiệp hiện tại của Checkout.
3. Các thiệp còn lại vẫn được giữ trong History.

#### ALT-03 — Chọn lại thiệp đã từng dùng cho Order
1. Khách hàng chọn một thiệp đã từng liên kết với một hoặc nhiều Order.
2. Hệ thống vẫn cho phép chọn nếu: thiệp thuộc khách hàng hiện tại; thiệp được tạo từ đúng mẫu hoa đang được sử dụng trong Checkout hiện tại; file ảnh còn khả dụng.
3. Các Order và liên kết trước đó không bị thay đổi.

#### ALT-04 — Template cũ không còn khả dụng
1. Thiệp đã tạo thành công vẫn được phép chọn lại nếu thuộc đúng mẫu hoa của Checkout hiện tại và file ảnh còn khả dụng.
2. Việc Template nguồn ngừng khả dụng chỉ chặn generate mới bằng Template đó.

#### ALT-05 — Bỏ thiệp khỏi Checkout
1. Khách hàng bỏ chọn thiệp hiện tại.
2. Hệ thống gỡ thiệp khỏi Checkout nhưng vẫn giữ History record.
3. Hệ thống cho phép khách hàng tiếp tục Checkout không có thiệp.

---

### Exception Flows

#### EXC-01 — Không có thiệp trong History
1. Khách hàng mở chức năng “Chọn thiệp từ lịch sử” tại Checkout.
2. Hệ thống kiểm tra History của khách hàng hiện tại theo mẫu hoa đang được sử dụng trong Checkout.
3. Hệ thống không tìm thấy thiệp có ảnh output hợp lệ được tạo từ đúng mẫu hoa hiện tại.
4. Hệ thống hiển thị empty state.
5. Hệ thống cho phép khách hàng quay lại tạo thiệp mới hoặc tiếp tục Checkout không có thiệp.

#### EXC-02 — Checkout không hợp lệ
1. Khách hàng thực hiện thao tác chọn thiệp từ History.
2. Backend kiểm tra Checkout.
3. Backend phát hiện Checkout không tồn tại, không thuộc khách hàng hiện tại hoặc đã hoàn tất.
4. Backend từ chối thao tác.
5. Hệ thống không thay đổi thiệp đang chọn.

#### EXC-03 — Thiệp không hợp lệ với Checkout hiện tại
1. Khách hàng gửi request chọn thiệp.
2. Backend kiểm tra quyền sở hữu History record và quan hệ với mẫu hoa hiện tại của Checkout.
3. Backend phát hiện một trong các trường hợp: History record không thuộc khách hàng hiện tại; hoặc thiệp được tạo từ mẫu hoa khác với mẫu hoa đang được sử dụng trong Checkout hiện tại.
4. Backend từ chối request.
5. Hệ thống không gắn thiệp vào Checkout.
6. Hệ thống không trả ảnh hoặc metadata nhạy cảm.

#### EXC-04 — File ảnh không còn khả dụng
1. Khách hàng chọn một thiệp từ History.
2. Backend kiểm tra trạng thái khả dụng của file ảnh.
3. Hệ thống phát hiện file ảnh không còn khả dụng.
4. Hệ thống không cho chọn thiệp.
5. Hệ thống giữ History record và metadata.
6. Hệ thống hiển thị: **“Ảnh không còn khả dụng.”**

#### EXC-05 — Không tải được danh sách History
1. Khách hàng mở chức năng “Chọn thiệp từ lịch sử” tại Checkout.
2. Hệ thống thực hiện tải danh sách History.
3. Hệ thống không tải được danh sách History.
4. Hệ thống hiển thị error state.
5. Hệ thống giữ nguyên thiệp đang chọn trong Checkout.
6. Hệ thống cho phép khách hàng tải lại.

#### EXC-06 — Request chọn thiệp bị gửi trùng
1. Khách hàng gửi request chọn thiệp nhiều lần hoặc request bị gửi lại.
2. Backend xử lý theo cơ chế idempotent.
3. Hệ thống không tạo nhiều liên kết tạm.
4. Hệ thống không làm thay đổi dữ liệu History và quota.

---

## Acceptance Criteria

### AC-001 – Hiển thị thiệp thuộc khách hàng
- **Given:** Checkout hợp lệ, thuộc khách hàng hiện tại và có mẫu hoa đang được sử dụng.
- **When:** Khách hàng mở chức năng chọn thiệp từ History.
- **Then:** Hệ thống chỉ hiển thị các thiệp thuộc khách hàng hiện tại, có ảnh output hợp lệ và được tạo từ đúng mẫu hoa của Checkout hiện tại.

### AC-002 – Chọn thiệp thành công
- **Given:** Thiệp thuộc khách hàng hiện tại, được tạo từ đúng mẫu hoa của Checkout và file ảnh còn khả dụng.
- **When:** khách hàng chọn thiệp
- **Then:** Hệ thống gắn thiệp làm thiệp đã được xác nhận và đang chọn của Checkout.
- **And:** Order Summary hiển thị thiệp đã chọn.

### AC-003 – Một thiệp trong Checkout
- **Given:** Checkout đã có thiệp A
- **When:** khách hàng chọn thiệp B
- **Then:** B thay thế A trong Checkout
- **And:** A vẫn được giữ trong History.

### AC-004 – Không gọi AI và không ảnh hưởng quota
- **Given:** thiệp đã có History record hợp lệ
- **When:** khách hàng chọn thiệp cho Checkout
- **Then:** hệ thống không gọi AI
- **And:** không sao chép ảnh
- **And:** không tạo History record mới
- **And:** không thay đổi quota.

### AC-005 – Tái sử dụng thiệp đã từng đặt hàng
- **Given:** Thiệp đã từng liên kết với một hoặc nhiều Order, thuộc khách hàng hiện tại, được tạo từ đúng mẫu hoa của Checkout và file ảnh còn khả dụng.
- **When:** khách hàng chọn thiệp cho Checkout mới
- **Then:** hệ thống vẫn cho phép chọn nếu thiệp thuộc khách hàng và file còn khả dụng
- **And:** các Order và liên kết trước đó không bị thay đổi.

### AC-006 – Template nguồn không còn khả dụng
- **Given:** Template nguồn của thiệp đã ngừng khả dụng nhưng thiệp thuộc đúng mẫu hoa của Checkout và file ảnh còn hợp lệ.
- **When:** Khách hàng chọn lại thiệp.
- **Then:** Hệ thống vẫn cho phép gắn thiệp vào Checkout.

### AC-007 – File ảnh không còn khả dụng
- **Given:** History record còn tồn tại nhưng file ảnh mất hoặc hỏng
- **When:** khách hàng chọn thiệp
- **Then:** hệ thống không gắn thiệp vào Checkout
- **And:** hiển thị “Ảnh không còn khả dụng.”

### AC-008 – Ownership
- **Given:** Thiệp không thuộc khách hàng hiện tại hoặc được tạo từ mẫu hoa khác với mẫu hoa của Checkout.
- **When:** Khách hàng gửi request chọn thiệp bằng ID hoặc URL trực tiếp.
- **Then:** backend từ chối
- **And:** Hệ thống không gắn thiệp vào Checkout.
- **And:** Hệ thống không trả ảnh hoặc metadata nhạy cảm.

### AC-009 – Checkout không hợp lệ
- **Given:** Checkout không tồn tại, không thuộc khách hàng hoặc đã hoàn tất
- **When:** khách hàng chọn thiệp
- **Then:** backend từ chối
- **And:** không thay đổi thiệp đang chọn.

### AC-010 – Bỏ thiệp khỏi Checkout
- **Given:** Checkout đang có một thiệp được chọn
- **When:** khách hàng chọn bỏ thiệp
- **Then:** hệ thống gỡ thiệp khỏi Checkout
- **And:** vẫn giữ thiệp trong History
- **And:** cho phép tiếp tục Checkout không có thiệp.

### AC-011 – Không tải được danh sách History
- **Given:** Checkout hợp lệ và khách hàng mở chức năng chọn thiệp từ History.
- **When:** Hệ thống không tải được danh sách History.
- **Then:** Hệ thống hiển thị error state.
- **And:** Giữ nguyên thiệp đang chọn trong Checkout.
- **And:** Cho phép khách hàng tải lại.

### AC-012 – Request chọn thiệp bị gửi trùng
- **Given:** Khách hàng chọn một thiệp hợp lệ cho Checkout.
- **When:** Request chọn thiệp bị gửi lặp lại.
- **Then:** Backend xử lý idempotent.
- **And:** Không tạo nhiều liên kết tạm cho cùng một thiệp trong Checkout.
- **And:** Không thay đổi History hoặc quota.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-110**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7673ca62-beb9-4542-9d77-483c2ecbd865) | Một thiệp trong Checkout | Chọn thiệp từ History cho Checkout | Khách hàng chỉ được xem và chọn History record thuộc tài khoản của mình và được tạo từ đúng mẫu hoa đang được sử dụng trong Checkout hiện tại. | Khách hàng xem hoặc chọn thiệp từ History. | Backend kiểm tra quyền sở hữu History record và quan hệ giữa thiệp với mẫu hoa của Checkout hiện tại. | Không cho phép xem hoặc chọn History record không thuộc khách hàng hiện tại hoặc được tạo từ mẫu hoa khác. | Đức Bình | STORY-042 | Draft | v0 | 2026-08-14 |
| [**BR-111**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/de42c1af-ff3c-46d2-a597-b5c33311de4e) | Quyền sở hữu | Chọn thiệp từ History cho Checkout | Khách hàng chỉ được xem và chọn History record thuộc tài khoản của mình. | Khách hàng xem hoặc chọn thiệp từ History. | Backend kiểm tra quyền sở hữu của History record. | Không cho phép xem hoặc chọn History record không thuộc tài khoản khách hàng hiện tại. | Đức Bình | STORY-042 | Draft | v0 | 2026-08-14 |
| [**BR-112**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e3417531-fad6-4974-8c70-2c26555684f5) | Tái sử dụng thiệp | Chọn thiệp từ History cho Checkout | Thiệp đã từng liên kết với Order vẫn được phép chọn cho Checkout mới nếu thiệp được tạo từ đúng mẫu hoa của Checkout hiện tại và file ảnh còn khả dụng. | Khách hàng chọn lại thiệp đã từng liên kết với Order. | Hệ thống cho phép chọn nếu thiệp thuộc khách hàng hiện tại, đúng mẫu hoa và file ảnh còn khả dụng. | Không cho phép chọn nếu thiệp thuộc mẫu hoa khác hoặc file ảnh không còn khả dụng. | Đức Bình | STORY-042 | Draft | v0 | 2026-08-14 |
| [**BR-113**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/415b72c7-5124-4ab3-8155-50c7c0e6e500) | Không tạo dữ liệu AI mới | Chọn thiệp từ History cho Checkout | Chọn thiệp từ History không gọi AI, không sao chép ảnh, không tạo History record và không tiêu hao quota. | Khách hàng chọn thiệp từ History cho Checkout. | Hệ thống gắn thiệp vào Checkout mà không tạo dữ liệu AI mới. | Không gọi AI, không sao chép ảnh, không tạo History record và không tiêu hao quota. | Đức Bình | STORY-042 | Draft | v0 | 2026-08-14 |
| [**BR-114**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ed7d5cf8-f07a-4ff2-ac26-5d50bf6791da) | Liên kết tạm với Checkout | Chọn thiệp từ History cho Checkout | Việc chọn thiệp chỉ thay đổi thiệp hiện tại của Checkout. Liên kết Thiệp–Order chỉ được tạo khi Order được tạo thành công theo STORY-039. | Khách hàng chọn thiệp từ History cho Checkout. | Hệ thống chỉ cập nhật thiệp hiện tại của Checkout. | Không tạo liên kết Thiệp–Order trước khi Order được tạo thành công theo STORY-039. | Đức Bình | STORY-042 | Draft | v0 | 2026-08-14 |
| [**BR-115**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f4478f91-dd86-4d5a-a825-ea423e0480d8) | Template ngừng khả dụng | Chọn thiệp từ History cho Checkout | Template nguồn ngừng khả dụng không làm mất quyền sử dụng lại thiệp đã tạo thành công nếu thiệp thuộc đúng mẫu hoa của Checkout hiện tại và file ảnh còn khả dụng. | Khách hàng chọn lại thiệp có Template nguồn đã ngừng khả dụng. | Hệ thống vẫn cho phép sử dụng lại thiệp đã tạo thành công nếu file ảnh còn khả dụng. | Template nguồn ngừng khả dụng chỉ chặn generate mới bằng Template đó. | Đức Bình | STORY-042 | Draft | v0 | 2026-08-14 |
| [**BR-116**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/97ef5916-4ca6-460f-a02d-a28b26d6823b) | Giá tạm tính | Chọn thiệp từ History cho Checkout | Order Summary sử dụng giá thiệp hiện hành để hiển thị tạm tính. Giá cuối được kiểm tra lại khi hoàn tất Checkout theo STORY-039. | Order Summary hiển thị thiệp đang chọn. | Hệ thống sử dụng giá thiệp hiện hành để hiển thị tạm tính. | Giá cuối được kiểm tra lại khi hoàn tất Checkout theo STORY-039. | Đức Bình | STORY-042 | Draft | v0 | 2026-08-14 |
| [**BR-117**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8f6b23f3-7321-4b92-a7e6-1ccedf83e959) | Bảo toàn History | Chọn thiệp từ History cho Checkout | Chọn, thay thế hoặc bỏ thiệp khỏi Checkout không xóa hoặc thay đổi History record. | Khách hàng chọn, thay thế hoặc bỏ thiệp khỏi Checkout. | Hệ thống giữ nguyên History record. | Không xóa hoặc thay đổi History record khi thao tác với thiệp trong Checkout. | Đức Bình | STORY-042 | Draft | v0 | 2026-08-14 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-110 | [Một thiệp trong Checkout](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7673ca62-beb9-4542-9d77-483c2ecbd865) |
| BR-111 | [Quyền sở hữu](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/de42c1af-ff3c-46d2-a597-b5c33311de4e) |
| BR-112 | [Tái sử dụng thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e3417531-fad6-4974-8c70-2c26555684f5) |
| BR-113 | [Không tạo dữ liệu AI mới](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/415b72c7-5124-4ab3-8155-50c7c0e6e500) |
| BR-114 | [Liên kết tạm với Checkout](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ed7d5cf8-f07a-4ff2-ac26-5d50bf6791da) |
| BR-115 | [Template ngừng khả dụng](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/f4478f91-dd86-4d5a-a825-ea423e0480d8) |
| BR-116 | [Giá tạm tính](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/97ef5916-4ca6-460f-a02d-a28b26d6823b) |
| BR-117 | [Bảo toàn History](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8f6b23f3-7321-4b92-a7e6-1ccedf83e959) |

### Dependencies
- [**STORY-038**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)
- [**STORY-045**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)

---

## Non-Functional Requirements

- Danh sách History được phân trang phía server và sắp xếp mới nhất trước.
- Tải danh sách History **p95 ≤ 1,5 giây** với page size cấu hình, không tính thời gian tải ảnh từ storage.
- Backend kiểm tra ownership của Checkout và History record trong mọi request.
- API danh sách không trả binary ảnh hoặc expose đường dẫn storage nội bộ.
- UI có loading, empty và error state.
- Một file ảnh lỗi không được làm hỏng toàn bộ danh sách.

---

## Out of Scope

- Generate hoặc tạo lại thiệp bằng AI.
- Hoàn tất Checkout, tạo Order và thanh toán.
- Tạo liên kết Thiệp–Order và tính bill cuối.
- Tải xuống, chia sẻ, chỉnh sửa hoặc xóa thiệp.
