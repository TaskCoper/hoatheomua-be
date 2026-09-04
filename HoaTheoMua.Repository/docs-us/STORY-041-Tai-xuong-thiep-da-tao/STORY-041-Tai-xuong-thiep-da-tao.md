# STORY-041 — Tải xuống thiệp đã tạo

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng có thiệp AI đã được tạo ảnh thành công, tôi muốn tải file ảnh thiệp xuống thiết bị, để lưu trữ hoặc sử dụng bên ngoài hệ thống. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |

---

## Context

Khách hàng có thể truy cập màn hình **Chi tiết thiệp** từ:
- Lịch sử tạo thiệp.
- Chi tiết Order có liên kết với thiệp.

Chức năng tải xuống sử dụng **chính file ảnh kết quả** đã được tạo thành công trước đó.

**Thao tác tải xuống:**
- Không gọi lại dịch vụ AI.
- Không tạo ảnh mới.
- Không tạo History item mới.
- Không trừ quota AI.
- Không làm thay đổi thiệp hoặc Order.

**Một thiệp có thể:**
- Chưa được liên kết với Order.
- Được liên kết với một Order.
- Được liên kết với nhiều Order do được tái sử dụng.

### Quy tắc đặt tên file

| Ngữ cảnh tải xuống | Định dạng tên file |
|---|---|
| Từ History chung | `thiep_[ma-thiep]_[yyyyMMdd_HHmmss].png` |
| Từ Chi tiết Order cụ thể | `thiep_[ma-don-hang]_[yyyyMMdd_HHmmss].png` |

> Thành phần thời gian là **thời điểm khách hàng bắt đầu lần tải xuống hiện tại**, không phải thời điểm generate hoặc tạo Order.

---

## Conditions

### Preconditions
- Khách hàng đã đăng nhập.
- History record của thiệp tồn tại.
- History record thuộc khách hàng hiện tại.
- Thiệp đã có ít nhất một lần generate thành công và được ghi nhận trong History.
- Trạng thái khả dụng của file được backend kiểm tra tại thời điểm tải xuống.

### Trigger
> Khách hàng ấn chọn **"Tải xuống thiệp"** tại màn hình Chi tiết thiệp hoặc Chi tiết Order.

---

## Flow

### Main Flow — Tải xuống file ảnh thiệp

1. Khách hàng mở màn hình **Chi tiết thiệp** từ History hoặc từ một Order liên quan.
2. Hệ thống hiển thị thông tin thiệp và nút **"Tải xuống thiệp"**.
3. Khách hàng ấn chọn **"Tải xuống thiệp"**.
4. Backend kiểm tra History record của thiệp:
   - Tồn tại.
   - Thuộc khách hàng hiện tại.
   - Có file ảnh output được ghi nhận.
5. Nếu thao tác được thực hiện từ **Chi tiết Order**, backend kiểm tra Order:
   - Tồn tại.
   - Thuộc khách hàng hiện tại.
   - Có liên kết với thiệp đang được tải.
6. Backend kiểm tra file ảnh output **còn khả dụng** trong storage.
7. Hệ thống lấy chính file ảnh kết quả đã generate thành công.
8. Hệ thống xác định tên file theo **ngữ cảnh tải xuống**:
   - Nếu tải từ một Order cụ thể → sử dụng **mã Order**.
   - Nếu tải từ History chung → sử dụng **mã thiệp**.
9. Hệ thống trả file ảnh **PNG** cho khách hàng với tên file tương ứng.
10. Thiết bị của khách hàng bắt đầu quá trình tải file.
11. Hệ thống **không**:
    - Gọi lại AI / Tạo ảnh mới / Sao chép file ảnh.
    - Tạo History item mới / Trừ quota AI.
    - Thay đổi thiệp hoặc Order.

---

### Alternative Flows

#### ALT-01 — Tải lại cùng một thiệp
1. Khách hàng đã từng tải xuống một thiệp.
2. File ảnh của thiệp vẫn còn khả dụng.
3. Khách hàng ấn chọn **"Tải xuống thiệp"** lần nữa.
4. Backend thực hiện lại việc kiểm tra quyền sở hữu và trạng thái file.
5. Hệ thống trả lại đúng file ảnh kết quả đã lưu.
6. Hệ thống **không giới hạn** số lần tải xuống.
7. Việc tải lại **không trừ quota** và **không tạo** History item mới.

#### ALT-02 — Tải thiệp từ History chung
1. Khách hàng mở Chi tiết thiệp từ History chung.
2. Thao tác tải xuống không được thực hiện trong ngữ cảnh một Order cụ thể.
3. Khách hàng ấn chọn **"Tải xuống thiệp"**.
4. Hệ thống sử dụng **mã thiệp** để đặt tên file: `thiep_[ma-thiep]_[yyyyMMdd_HHmmss].png`
5. Việc thiệp đã liên kết với một hoặc nhiều Order **không làm thay đổi** cách đặt tên trong trường hợp này.

#### ALT-03 — Tải thiệp từ Chi tiết Order
1. Khách hàng đang xem một Order có liên kết với thiệp.
2. Khách hàng mở Chi tiết thiệp hoặc chọn tải thiệp trong ngữ cảnh Order đó.
3. Backend kiểm tra Order thuộc khách hàng hiện tại và có liên kết với thiệp.
4. Hệ thống sử dụng **mã của Order đang xem** để đặt tên file: `thiep_[ma-don-hang]_[yyyyMMdd_HHmmss].png`
5. Nếu thiệp được liên kết với nhiều Order, mã của các Order **khác không được sử dụng** trong lần tải này.

---

### Exception Flows

#### EXC-01 — File ảnh không còn khả dụng
1. History record của thiệp vẫn tồn tại.
2. Khách hàng mở Chi tiết thiệp hoặc thực hiện tải xuống.
3. Backend kiểm tra file ảnh trong storage.
4. Hệ thống phát hiện file đã bị mất, hỏng hoặc không thể truy cập.
5. Hệ thống giữ nguyên **History record và metadata** của thiệp.
6. Hệ thống **không xóa** hoặc chuyển lần generate trước đó thành thất bại.
7. Hệ thống **không trả** file hoặc đường dẫn storage.
8. Hệ thống **không tự động** gọi AI để tạo lại ảnh.
9. Hệ thống vô hiệu hóa chức năng tải xuống đối với file đó.
10. Hệ thống hiển thị thông báo: **"Ảnh không còn khả dụng."**

#### EXC-02 — Không tìm thấy thiệp
1. Khách hàng gửi yêu cầu tải xuống bằng mã thiệp hoặc URL.
2. Backend không tìm thấy History record tương ứng.
3. Hệ thống không thực hiện tải xuống, không gọi AI hoặc tạo file thay thế.
4. Hệ thống không trả thông tin về storage.
5. Hệ thống hiển thị thông báo: **"Không tìm thấy thiệp hoặc thiệp không còn tồn tại."**

#### EXC-03 — Khách hàng không có quyền tải thiệp
1. Khách hàng gửi yêu cầu tải xuống một thiệp.
2. Backend kiểm tra quyền sở hữu History record.
3. Hệ thống phát hiện thiệp **không thuộc** khách hàng hiện tại.
4. Backend từ chối request.
5. Hệ thống không trả: file ảnh, storage URL, đường dẫn nội bộ hoặc metadata nhạy cảm của thiệp.
6. Hệ thống **không tiết lộ** thiệp có thực sự tồn tại hay không.
7. Hệ thống hiển thị thông báo: **"Bạn không có quyền thực hiện thao tác này."**

#### EXC-04 — Order không hợp lệ với thiệp
1. Khách hàng thực hiện tải thiệp trong ngữ cảnh một Order cụ thể.
2. Backend kiểm tra Order và liên kết Thiệp–Order.
3. Hệ thống phát hiện một trong các trường hợp: Order không tồn tại / Order không thuộc khách hàng hiện tại / Order không có liên kết với thiệp được yêu cầu.
4. Backend từ chối tải file theo ngữ cảnh Order.
5. Hệ thống không sử dụng mã Order đó để đặt tên file.
6. Hệ thống không trả dữ liệu của Order hoặc thiệp.
7. Hệ thống hiển thị thông báo: **"Không thể tải thiệp từ đơn hàng này."**

#### EXC-05 — Lỗi trong quá trình cung cấp file
1. Thiệp tồn tại, thuộc khách hàng hiện tại và file được xác định còn khả dụng.
2. Khách hàng ấn chọn **"Tải xuống thiệp"**.
3. Hệ thống gặp lỗi khi đọc hoặc truyền file.
4. Hệ thống không trả file thiếu, hỏng hoặc không hoàn chỉnh như một kết quả thành công.
5. Hệ thống không gọi AI để tạo lại ảnh, không tạo History item mới, quota AI không thay đổi.
6. Hệ thống hiển thị thông báo: **"Tải thiệp thất bại. Vui lòng thử lại."**
7. Khách hàng có thể thử tải lại.

---

## Acceptance Criteria

### AC-001 — Tải thiệp từ History chung
- **Given:** Thiệp thuộc khách hàng hiện tại, file còn khả dụng và khách hàng đang xem thiệp từ History chung.
- **When:** Khách hàng ấn chọn **"Tải xuống thiệp"**.
- **Then:** Hệ thống phải trả đúng file ảnh kết quả đã lưu.
- **And:** Tên file phải có định dạng: `thiep_[ma-thiep]_[yyyyMMdd_HHmmss].png`

### AC-002 — Tải thiệp từ Chi tiết Order
- **Given:** Order thuộc khách hàng hiện tại, Order có liên kết với thiệp và file ảnh thiệp còn khả dụng.
- **When:** Khách hàng tải thiệp trong ngữ cảnh Order đó.
- **Then:** Hệ thống phải trả đúng file ảnh kết quả đã lưu.
- **And:** Tên file phải có định dạng: `thiep_[ma-don-hang]_[yyyyMMdd_HHmmss].png`

### AC-003 — Thiệp được liên kết với nhiều Order
- **Given:** Một thiệp được liên kết với nhiều Order.
- **When:** Khách hàng tải thiệp từ Chi tiết một Order cụ thể.
- **Then:** Hệ thống phải sử dụng **mã của Order đang xem** để đặt tên file.
- **And:** Không sử dụng mã của Order khác.

### AC-004 — Không gọi lại AI
- **Given:** Thiệp đã có file ảnh output.
- **When:** Khách hàng tải xuống thiệp.
- **Then:** Hệ thống không được gọi dịch vụ AI.
- **And:** Không được tạo ảnh mới.
- **And:** Không được tạo History item mới.
- **And:** Hệ thống không sao chép file ảnh trong storage.

### AC-005 — Không ảnh hưởng quota
- **Given:** Khách hàng còn hoặc đã hết quota AI.
- **When:** Khách hàng tải hoặc tải lại thiệp.
- **Then:** Quota AI **không được thay đổi**.
- **And:** Trạng thái quota không được ngăn khách hàng tải file hợp lệ.

### AC-006 — Tải lại không giới hạn
- **Given:** File ảnh còn khả dụng.
- **When:** Khách hàng tải lại cùng một thiệp nhiều lần.
- **Then:** Hệ thống phải tiếp tục cho phép tải xuống.
- **And:** Mỗi lần phải trả đúng file ảnh đã lưu.

### AC-007 — File không còn khả dụng
- **Given:** History record còn tồn tại nhưng file ảnh bị mất, hỏng hoặc không thể truy cập.
- **When:** Khách hàng mở Chi tiết thiệp hoặc thực hiện tải xuống.
- **Then:** Hệ thống phải giữ nguyên History record.
- **And:** Hiển thị: **"Ảnh không còn khả dụng."**
- **And:** Không trả file hoặc storage URL.
- **And:** Không tự động generate ảnh thay thế.

### AC-008 — Không tìm thấy thiệp
- **Given:** History record được yêu cầu không tồn tại.
- **When:** Khách hàng gửi request tải xuống.
- **Then:** Hệ thống không được trả file.
- **And:** Không được gọi AI.
- **And:** Phải hiển thị trạng thái tài nguyên không tồn tại hoặc không thể truy cập.

### AC-009 — Kiểm tra quyền sở hữu
- **Given:** Thiệp thuộc khách hàng khác.
- **When:** Khách hàng gọi API tải xuống bằng ID hoặc URL.
- **Then:** Backend phải từ chối request.
- **And:** Không được trả file, storage URL hoặc metadata nhạy cảm.

### AC-010 — Kiểm tra liên kết Thiệp–Order
- **Given:** Khách hàng tải thiệp trong ngữ cảnh một Order và Order không có liên kết với thiệp đó.
- **When:** Backend xử lý request tải xuống.
- **Then:** Hệ thống từ chối tải file theo ngữ cảnh Order.
- **And:** Không được sử dụng mã Order để đặt tên file.

### AC-011 — Lỗi cung cấp file
- **Given:** File được ghi nhận là còn khả dụng.
- **When:** Hệ thống gặp lỗi trong quá trình đọc hoặc truyền file.
- **Then:** Hệ thống phải thông báo tải thất bại.
- **And:** Cho phép khách hàng thử lại.
- **And:** Không làm thay đổi quota, History, thiệp hoặc Order.

### AC-012 — Tên file an toàn
- **Given:** Mã thiệp hoặc mã Order chứa ký tự không an toàn cho tên file.
- **When:** Hệ thống tạo tên file tải xuống.
- **Then:** Hệ thống phải loại bỏ hoặc thay thế ký tự không an toàn.
- **And:** Tên file cuối cùng phải có phần mở rộng `.png`.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|
| **BR-101** | File nguồn tải xuống | Tải xuống thiệp đã tạo | Chức năng tải xuống phải sử dụng đúng file ảnh kết quả chính thức định dạng PNG đã được lưu trước đó và không gắn thêm logo hoặc xử lý lại ảnh. | Khách hàng tải hoặc tải lại thiệp. | Hệ thống trả đúng file ảnh đã lưu. | Hệ thống trả đúng file PNG chính thức đã lưu. | Draft | v0 | 2026-08-14 |
| **BR-102** | Tên file trong ngữ cảnh Order | Tải xuống thiệp đã tạo | Khi thiệp được tải từ một Order cụ thể, tên file phải sử dụng mã của Order đó. | Khách hàng tải thiệp từ Chi tiết Order và Order có liên kết hợp lệ với thiệp. | Tên file có định dạng: `thiep_[ma-don-hang]_[yyyyMMdd_HHmmss].png` | Không sử dụng mã Order nếu Order không tồn tại, không thuộc khách hàng hiện tại hoặc không liên kết với thiệp. | Draft | v0 | 2026-08-14 |
| **BR-103** | Tên file ngoài ngữ cảnh Order | Tải xuống thiệp đã tạo | Khi thao tác tải xuống không gắn với một Order cụ thể, tên file phải sử dụng mã thiệp. | Khách hàng tải thiệp từ History chung. | Tên file có định dạng: `thiep_[ma-thiep]_[yyyyMMdd_HHmmss].png` | Việc thiệp đã liên kết với một hoặc nhiều Order không làm thay đổi quy tắc này. | Draft | v0 | 2026-08-14 |
| **BR-104** | Thời điểm trong tên file | Tải xuống thiệp đã tạo | Thành phần thời gian trong tên file là thời điểm khách hàng bắt đầu lần tải xuống hiện tại. | Hệ thống tạo tên file. | Thời gian được định dạng theo múi giờ hệ thống: `yyyyMMdd_HHmmss` | Không sử dụng thời điểm generate hoặc thời điểm tạo Order thay cho thời điểm tải xuống. | Draft | v0 | 2026-08-14 |
| **BR-105** | Không giới hạn số lần tải | Tải xuống thiệp đã tạo | Khách hàng được tải lại file ảnh hợp lệ không giới hạn số lần. | Khách hàng thực hiện tải lại. | Hệ thống tiếp tục cung cấp đúng file ảnh nếu file còn khả dụng và khách hàng có quyền truy cập. | Hệ thống từ chối nếu file không còn khả dụng hoặc khách hàng không có quyền. | Draft | v0 | 2026-08-14 |
| **BR-106** | Không tiêu hao quota AI | Tải xuống thiệp đã tạo | Xem, tải và tải lại thiệp không phải là thao tác generate. | Khách hàng xem hoặc tải thiệp. | Hệ thống không được khấu trừ quota AI. | Không có. | Draft | v0 | 2026-08-14 |
| **BR-107** | Bảo toàn History khi mất file | Tải xuống thiệp đã tạo | Trạng thái tồn tại của History record độc lập với trạng thái khả dụng của file ảnh. | File ảnh bị mất, hỏng hoặc không thể truy cập. | Hệ thống giữ nguyên History record và metadata đã lưu. | Hệ thống không đổi lần generate trước đó thành thất bại và không tự động gọi AI để tạo lại ảnh. | Draft | v0 | 2026-08-14 |
| **BR-108** | Quyền sở hữu | Tải xuống thiệp đã tạo | Khách hàng chỉ được tải thiệp thuộc History của chính mình. | Backend tiếp nhận request tải xuống. | Backend phải kiểm tra quyền sở hữu trước khi cung cấp file. | Nếu thiệp không thuộc khách hàng hiện tại, backend từ chối request và không trả file, storage URL hoặc metadata nhạy cảm. | Draft | v0 | 2026-08-14 |
| **BR-109** | Liên kết Thiệp–Order | Tải xuống thiệp đã tạo | Mã Order chỉ được sử dụng trong tên file khi Order có liên kết hợp lệ với thiệp. | Khách hàng tải thiệp từ Chi tiết Order. | Backend kiểm tra quyền sở hữu Order và record liên kết Thiệp–Order. | Nếu liên kết không tồn tại hoặc không hợp lệ, hệ thống không cung cấp file trong ngữ cảnh Order đó. | Draft | v0 | 2026-08-14 |

---

## References

### Business Rules
`BR-101` · `BR-102` · `BR-103` · `BR-104` · `BR-105` · `BR-106` · `BR-107` · `BR-108` · `BR-109`

### Dependencies
- **STORY-035** — Tạo thiệp cá nhân hóa bằng AI tại Checkout
- **STORY-036** — Tạo lại thiệp cá nhân hóa bằng AI
- **STORY-039** — Hoàn tất Checkout, tạo và thanh toán Order

---

## Non-Functional Requirements

- Backend **không được expose** đường dẫn nội bộ, thông tin xác thực hoặc permanent storage URL.
- API metadata **không được nhúng** binary hoặc Base64 của file ảnh.
- File phải được cung cấp qua **response download** hoặc **URL tạm thời** có thời hạn ngắn.
- **Ownership** phải được kiểm tra tại backend, không chỉ dựa vào trạng thái hiển thị của frontend.
- Với file còn khả dụng, hệ thống bắt đầu phản hồi download ở mức **p95 ≤ 2 giây** trong điều kiện vận hành bình thường, không tính thời gian truyền toàn bộ file đến thiết bị.
- Chức năng tải xuống **không được kích hoạt** AI hoặc xử lý ảnh không cần thiết.
- Tên file phải **loại bỏ hoặc thay thế** các ký tự không an toàn từ mã Order hoặc mã thiệp.
- Response phải khai báo đúng **loại nội dung** của file PNG.
- Lỗi của một file **không được làm ảnh hưởng** đến khả năng xem hoặc tải các thiệp khác.
- Backend phải **chống truy cập trái phép** bằng cách thay đổi ID thiệp hoặc ID Order trên request.

---

## Out of Scope

- Generate hoặc tạo lại thiệp.
- Chỉnh sửa nội dung hoặc hình ảnh thiệp.
- Xóa thiệp hoặc History record.
- Chia sẻ thiệp cho tài khoản khác.
- Khôi phục file ảnh đã mất hoặc hỏng.
- Chuyển đổi ảnh sang định dạng khác PNG.
- Tải xuống mẫu hoa Custom AI.
- Quản lý hoặc thay đổi Order.
- Tạo liên kết Thiệp–Order.
