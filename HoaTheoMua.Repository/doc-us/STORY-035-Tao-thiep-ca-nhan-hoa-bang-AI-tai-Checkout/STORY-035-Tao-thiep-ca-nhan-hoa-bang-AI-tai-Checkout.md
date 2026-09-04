# STORY-035 — Khách hàng cấu hình thiệp tại Checkout

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã đăng nhập, tôi muốn cấu hình thiệp tại Checkout, để lựa chọn loại thiệp và cung cấp thông tin cần thiết cho đơn hàng trước khi thanh toán. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |
| **Cập nhật** | 03/09/2026 |
| **Author** | Hoàng Thị Khánh Linh |
| **Reviewer** | Nguyễn Đức Bình |
| **Approver** | Chưa chỉ định |
| **Owner** | Hoàng Thị Khánh Linh |

---

## Context

Tại Checkout, khách hàng có thể không chọn thiệp hoặc chọn thêm thiệp cho bó hoa. Thiệp gồm hai loại: **Thiệp Miễn phí** và **Thiệp Custom**.

- Checkout chỉ thu thập và lưu thông tin yêu cầu thiệp để tính giá và tạo Order.
- Đối với Thiệp Custom, hệ thống chỉ cho phép thực hiện bước tạo thiệp sau khi Order đã được thanh toán thành công.
- Việc hoàn tất Checkout, tạo Order, tính bill cuối và tạo liên kết Thiệp-Order thuộc phạm vi khác.

### Input Data

**Thông tin bắt buộc:**

| Trường | Giới hạn |
|---|---|
| Người gửi | Tối đa 20 từ |
| Người nhận | Tối đa 20 từ |
| Lời chúc | Tối đa 100 từ |
| Template | Bắt buộc chọn |
| Size | Bắt buộc chọn |
| Loại thiệp | Thiệp Miễn phí hoặc Thiệp Custom |
| Hình thức Thiệp Custom | In hoặc Viết tay |

**Ảnh đính kèm (không bắt buộc):**

| Thuộc tính | Quy định |
|---|---|
| Số lượng | Tối đa 01 ảnh |
| Định dạng | PNG, JPG |
| Dung lượng | Tối đa 10 MB |

### Quy tắc đếm từ

- Một từ là một chuỗi ký tự liên tục được phân tách bởi khoảng trắng, tab hoặc ký tự xuống dòng.
- Khoảng trắng ở đầu và cuối nội dung không được tính.
- Nhiều khoảng trắng, tab hoặc ký tự xuống dòng liên tiếp được tính là **một dấu phân tách**.
- Dấu câu đi liền với một từ **không được tính** thành từ riêng.
- Nội dung chỉ gồm khoảng trắng được tính là **0 từ**.

> Frontend chỉ cho phép gửi yêu cầu khi dữ liệu hợp lệ. **Backend phải kiểm tra lại** toàn bộ dữ liệu, quyền sở hữu và điều kiện xử lý.

---

## Conditions

### Preconditions
- Khách hàng đã đăng nhập.
- Checkout hợp lệ và thuộc khách hàng hiện tại.
- Bó hoa của Checkout còn hợp lệ.
- Các Size và Mẫu thiệp được hiển thị phải đang khả dụng.

### Trigger
> Khách hàng chọn thêm thiệp hoặc cấu hình thiệp tại Checkout.

---

## Flow

### Main Flow

1. Khách hàng mở chức năng cấu hình thiệp tại Checkout.
2. Hệ thống hiển thị tùy chọn **Có thiệp / Không có thiệp**.
3. Nếu chọn **Có thiệp**, hệ thống cho khách hàng chọn: **Thiệp Miễn phí** hoặc **Thiệp Custom**.
4. Khách hàng nhập: Người gửi, Người nhận, Nội dung.
5. Nếu chọn **Thiệp Custom**: chọn hình thức **In** hoặc **Viết tay**; chọn Size; chọn Mẫu thiệp; có thể đính kèm ảnh.
6. Hệ thống validate dữ liệu.
7. Hệ thống tính giá thiệp tương ứng.
8. Hệ thống lưu thông tin cấu hình thiệp vào Checkout.
9. Không tạo ảnh thiệp và không gọi AI tại bước này.
10. Khách hàng tiếp tục thanh toán.

---

### Alternative Flows

#### ALT-01 — Không có ảnh đính kèm
1. Khách hàng vẫn được lưu cấu hình Thiệp Custom nếu các dữ liệu bắt buộc còn lại hợp lệ.

#### ALT-02 — Không chọn thiệp
1. Khách hàng chọn **Không có thiệp** tại Checkout.
2. Hệ thống không hiển thị yêu cầu nhập Người gửi, Người nhận, Nội dung, Size, Mẫu thiệp hoặc Hình thức.
3. Hệ thống không tính thêm giá thiệp.
4. Checkout được tiếp tục với thông tin bó hoa hiện tại.
5. Khách hàng tiếp tục sang bước thanh toán.

#### ALT-03 — Chọn Thiệp Miễn phí
1. Khách hàng chọn **Có thiệp**.
2. Khách hàng chọn loại **Thiệp Miễn phí**.
3. Hệ thống hiển thị các trường thông tin áp dụng cho Thiệp Miễn phí.
4. Khách hàng nhập Người gửi, Người nhận và Nội dung.
5. Hệ thống kiểm tra giới hạn nội dung theo cấu hình Thiệp Miễn phí.
6. Nếu dữ liệu hợp lệ, hệ thống lưu thông tin Thiệp Miễn phí vào Checkout.
7. Hệ thống không áp dụng giá Size hoặc phụ phí Thiệp Custom.
8. Khách hàng tiếp tục sang bước thanh toán.

#### ALT-04 — Chọn Thiệp Custom dạng In
1. Khách hàng chọn **Có thiệp**.
2. Khách hàng chọn loại **Thiệp Custom**.
3. Khách hàng chọn hình thức **In**.
4. Hệ thống hiển thị danh sách Size và Mẫu thiệp đang khả dụng.
5. Khách hàng chọn Size, Mẫu thiệp và có thể chọn 01 ảnh đính kèm.
6. Khách hàng nhập Người gửi, Người nhận và Nội dung.
7. Hệ thống kiểm tra dữ liệu và giới hạn số từ theo Size được chọn.
8. Hệ thống xác định: Giá Thiệp Custom dạng In = Giá Size được chọn.
9. Nếu dữ liệu hợp lệ, hệ thống lưu cấu hình Thiệp Custom dạng In và giá tương ứng vào Checkout.
10. Hệ thống **không tạo ảnh thiệp** và **không gọi AI** tại Checkout.
11. Khách hàng tiếp tục sang bước thanh toán.

#### ALT-05 — Chọn Thiệp Custom dạng Viết tay
1. Khách hàng chọn **Có thiệp**.
2. Khách hàng chọn loại **Thiệp Custom**.
3. Khách hàng chọn hình thức **Viết tay**.
4. Hệ thống hiển thị các Size được phép áp dụng cho Thiệp Custom dạng Viết tay.
5. Khách hàng chọn Size, Mẫu thiệp và có thể chọn 01 ảnh đính kèm.
6. Khách hàng nhập Người gửi, Người nhận và Nội dung.
7. Hệ thống đếm số từ của Nội dung theo quy tắc đếm từ hiện hành.
8. Hệ thống xác định cấu hình phụ phí viết tay Active tương ứng với số lượng từ.
9. Hệ thống xác định: Giá Thiệp Custom dạng Viết tay = Giá Size + Phụ phí viết tay theo số lượng từ.
10. Nếu dữ liệu hợp lệ, hệ thống lưu cấu hình Thiệp Custom dạng Viết tay và giá tương ứng vào Checkout.
11. Hệ thống **không tạo ảnh thiệp** và **không gọi AI** tại Checkout.
12. Khách hàng tiếp tục sang bước thanh toán.

---

### Exception Flows

#### EXC-01 — Dữ liệu bắt buộc không hợp lệ
1. Khách hàng đang cấu hình thiệp tại Checkout.
2. Khách hàng nhập thiếu hoặc nhập không hợp lệ một trong các trường bắt buộc tương ứng với loại thiệp đã chọn.
3. Khách hàng thực hiện lưu cấu hình thiệp hoặc tiếp tục Checkout.
4. Backend kiểm tra lại toàn bộ dữ liệu đầu vào.
5. Backend phát hiện dữ liệu không hợp lệ.
6. Hệ thống **không lưu** cấu hình thiệp vào Checkout.
7. Hệ thống **không cập nhật** giá thiệp vào Checkout.
8. Hệ thống hiển thị lỗi tại trường tương ứng.

#### EXC-02 — File upload không hợp lệ
1. Khách hàng đang cấu hình thiệp tại Checkout.
2. Khách hàng chọn ảnh đính kèm.
3. File ảnh không thuộc định dạng PNG, JPG, hoặc có dung lượng vượt quá 10 MB.
4. Hệ thống từ chối file ảnh.
5. Hệ thống hiển thị lý do file không hợp lệ.
6. Hệ thống cho phép khách hàng chọn file khác.
7. Khách hàng vẫn có thể tiếp tục lưu cấu hình Thiệp Custom không có ảnh đính kèm nếu các dữ liệu bắt buộc còn lại hợp lệ.

#### EXC-03 — Template hoặc Size không còn khả dụng
1. Khách hàng đang cấu hình thiệp tại Checkout.
2. Khách hàng đã chọn Template hoặc Size.
3. Khách hàng thực hiện lưu cấu hình thiệp hoặc tiếp tục Checkout.
4. Backend kiểm tra lại trạng thái khả dụng của Template và Size từ Core system.
5. Backend phát hiện Template hoặc Size đã ngừng khả dụng hoặc bị xóa mềm.
6. Hệ thống **không lưu** cấu hình Thiệp Custom vào Checkout.
7. Hệ thống **không sử dụng** Template hoặc Size không còn khả dụng để tính giá.
8. Hệ thống yêu cầu khách hàng chọn Template hoặc Size hiện còn khả dụng.

#### EXC-04 — Checkout không hợp lệ
1. Khách hàng thực hiện lưu cấu hình thiệp trong Checkout.
2. Backend kiểm tra Checkout hiện tại.
3. Backend phát hiện Checkout không tồn tại, không thuộc khách hàng hiện tại hoặc không còn ở trạng thái cho phép chỉnh sửa.
4. Hệ thống từ chối lưu cấu hình thiệp.
5. Hệ thống không cập nhật giá hoặc thông tin thiệp vào Checkout.
6. Hệ thống hiển thị lỗi tương ứng và **không để lộ dữ liệu** của Checkout/khách hàng khác.

---

## Acceptance Criteria

### AC-001 — Không chọn thiệp
- **Given:** Checkout hợp lệ và thuộc khách hàng hiện tại.
- **When:** Khách hàng chọn **"Không có thiệp"**.
- **Then:** Hệ thống không yêu cầu nhập thông tin thiệp.
- **And:** Hệ thống không tính thêm giá thiệp.
- **And:** Hệ thống cho phép khách hàng tiếp tục Checkout.

### AC-002 — Chọn Thiệp Miễn phí
- **Given:** Checkout hợp lệ và khách hàng đã chọn "Có thiệp".
- **When:** Khách hàng chọn **"Thiệp Miễn phí"** và nhập đầy đủ thông tin bắt buộc hợp lệ.
- **Then:** Hệ thống lưu thông tin Thiệp Miễn phí vào Checkout.
- **And:** Hệ thống không áp dụng giá Size hoặc phụ phí của Thiệp Custom.
- **And:** Hệ thống cho phép khách hàng tiếp tục Checkout.

### AC-003 — Chọn Thiệp Custom dạng In
- **Given:** Checkout hợp lệ và khách hàng đã chọn "Thiệp Custom".
- **When:** Khách hàng chọn hình thức **"In"**, Size và Mẫu thiệp hợp lệ.
- **Then:** Hệ thống xác định giá thiệp bằng giá của Size được chọn.
- **And:** Hệ thống lưu cấu hình Thiệp Custom dạng In vào Checkout.
- **And:** Hệ thống **không tạo ảnh thiệp** và **không gọi AI** tại bước này.

### AC-004 — Chọn Thiệp Custom dạng Viết tay
- **Given:** Checkout hợp lệ và khách hàng đã chọn "Thiệp Custom".
- **When:** Khách hàng chọn hình thức **"Viết tay"**, Size và Mẫu thiệp hợp lệ.
- **Then:** Hệ thống đếm số từ của Nội dung theo quy tắc hiện hành.
- **And:** Hệ thống xác định cấu hình phụ phí viết tay Active tương ứng với số lượng từ.
- **And:** Giá thiệp bằng Giá Size + Phụ phí viết tay.
- **And:** Hệ thống lưu cấu hình Thiệp Custom dạng Viết tay vào Checkout.
- **And:** Hệ thống **không gọi AI** tại bước này.

### AC-005 — Dữ liệu bắt buộc không hợp lệ
- **Given:** Khách hàng đang cấu hình thiệp tại Checkout.
- **When:** Một hoặc nhiều trường bắt buộc bị thiếu hoặc không hợp lệ.
- **Then:** Backend từ chối lưu cấu hình thiệp.
- **And:** Hệ thống không cập nhật giá thiệp vào Checkout.
- **And:** Hệ thống hiển thị lỗi tại trường tương ứng.

### AC-006 — File ảnh đính kèm không hợp lệ
- **Given:** Khách hàng đang cấu hình Thiệp Custom và chọn ảnh đính kèm.
- **When:** File không thuộc định dạng PNG/JPG hoặc vượt quá 10 MB.
- **Then:** Hệ thống từ chối file.
- **And:** Hệ thống hiển thị lý do file không hợp lệ.
- **And:** Hệ thống cho phép khách hàng chọn file khác hoặc tiếp tục không có ảnh đính kèm.

### AC-007 — Không có ảnh đính kèm
- **Given:** Khách hàng đang cấu hình Thiệp Custom và các dữ liệu bắt buộc khác hợp lệ.
- **When:** Khách hàng không chọn ảnh đính kèm.
- **Then:** Hệ thống vẫn cho phép lưu cấu hình thiệp vào Checkout.
- **And:** Hệ thống không yêu cầu ảnh đính kèm để tiếp tục Checkout.

### AC-008 — Template hoặc Size không còn khả dụng
- **Given:** Khách hàng đã chọn Template và Size cho Thiệp Custom.
- **When:** Backend phát hiện Template hoặc Size đã Inactive hoặc bị xóa mềm.
- **Then:** Hệ thống không lưu cấu hình Thiệp Custom.
- **And:** Hệ thống không sử dụng Template hoặc Size không còn khả dụng để tính giá.
- **And:** Hệ thống yêu cầu khách hàng chọn Template hoặc Size còn khả dụng.

### AC-009 — Nội dung vượt giới hạn số từ
- **Given:** Khách hàng đang cấu hình thiệp tại Checkout.
- **When:** Một trường nội dung vượt quá giới hạn số từ áp dụng.
- **Then:** Backend từ chối dữ liệu.
- **And:** Hệ thống không lưu cấu hình thiệp vào Checkout.
- **And:** Hệ thống hiển thị lỗi tương ứng.

### AC-010 — Checkout không hợp lệ
- **Given:** Checkout không tồn tại, không thuộc khách hàng hiện tại hoặc không còn ở trạng thái cho phép chỉnh sửa.
- **When:** Khách hàng thực hiện lưu cấu hình thiệp.
- **Then:** Backend từ chối thao tác.
- **And:** Hệ thống không cập nhật thông tin hoặc giá thiệp vào Checkout.
- **And:** Hệ thống không để lộ dữ liệu của Checkout hoặc khách hàng khác.

### AC-011 — Không tạo Thiệp Custom trước khi thanh toán thành công
- **Given:** Khách hàng đã cấu hình Thiệp Custom hợp lệ tại Checkout.
- **When:** Hệ thống lưu cấu hình thiệp thành công.
- **Then:** Hệ thống **không tạo AI Job**, không tạo ảnh thiệp, không tạo History record và không ghi nhận lượt generate.
- **And:** Thiệp Custom chỉ được phép tạo sau khi Order được thanh toán thành công.

### AC-012 — Cập nhật giá thiệp vào Checkout
- **Given:** Khách hàng đã chọn loại thiệp và nhập dữ liệu hợp lệ.
- **When:** Hệ thống xác định được giá thiệp.
- **Then:** Hệ thống cập nhật giá thiệp vào tổng giá tạm tính của Checkout.
- **And:** Giá hiển thị phải tương ứng với loại thiệp, hình thức, Size và phụ phí áp dụng.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-051**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d8ba05bd-9cc4-4522-9c99-fe5f2bd1ca7a) | Nội dung bắt buộc | Tạo thiệp | Người gửi tối đa 20 từ, Người nhận tối đa 20 từ và Lời chúc tối đa 100 từ theo quy tắc đếm từ của STORY-035. | Khách hàng lưu cấu hình thiệp hoặc tiếp tục Checkout. | Hệ thống kiểm tra giới hạn từ của Người gửi, Người nhận và Lời chúc. | Dữ liệu vượt quá giới hạn số từ không được chấp nhận và cấu hình thiệp không được lưu vào Checkout. | Đức Bình | STORY-035 | Draft | v0 | 2026-09-03 |
| [**BR-052**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c17b8c1c-62a5-42c0-95cd-91620de4366d) | Ảnh đính kèm | Tạo thiệp | Mỗi yêu cầu có tối đa 01 ảnh đính kèm định dạng PNG hoặc JPG và dung lượng tối đa 10 MB. | Khách hàng tải ảnh đính kèm khi cấu hình Thiệp Custom tại Checkout. | Hệ thống kiểm tra số lượng, định dạng và dung lượng ảnh đính kèm. | File không đúng định dạng, vượt quá dung lượng hoặc vượt quá số lượng không được chấp nhận. Khách hàng vẫn có thể tiếp tục mà không có ảnh đính kèm nếu các dữ liệu bắt buộc khác hợp lệ. | Đức Bình | STORY-035 | Draft | v0 | 2026-08-14 |
| [**BR-053**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4e460995-5b6c-45b3-8491-13c0c1c80b05) | Một thiệp trong Checkout | Tạo thiệp | Mỗi Checkout được có tối đa 01 lựa chọn thiệp tại một thời điểm. | Khách hàng chọn hoặc thay đổi thiệp tại Checkout. | Nếu Checkout đã có cấu hình hoặc thiệp được chọn, lựa chọn mới thay thế lựa chọn trước đó. | Việc thay đổi lựa chọn trong Checkout không xóa dữ liệu thiệp đã tồn tại trong History. | Đức Bình | STORY-035 | Draft | v0 | 2026-08-14 |
| [**BR-054**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b93ed86e-be68-4fbe-a7ab-8f1532f369bb) | Giá tạm tính | Tạo thiệp | Giá thiệp được xác định theo loại thiệp và cấu hình hiện hành tại thời điểm khách hàng cấu hình thiệp tại Checkout. | Khách hàng chọn loại thiệp và nhập đầy đủ dữ liệu cần thiết để hệ thống tính giá. | **Thiệp Miễn phí:** không áp dụng giá Size hoặc phụ phí Thiệp Custom. **Thiệp Custom dạng In:** Giá thiệp = Giá hiện hành của Size được chọn. **Thiệp Custom dạng Viết tay:** Giá thiệp = Giá hiện hành của Size được chọn + Phụ phí viết tay Active tương ứng với số lượng từ của Nội dung. Hệ thống lấy giá Size và cấu hình phụ phí hiện hành từ Core Database. Giá tính được được cập nhật vào tổng giá tạm tính của Checkout. | Nếu không tìm thấy giá Size hoặc không tìm thấy cấu hình phụ phí viết tay hợp lệ tương ứng, hệ thống không cho lưu hoàn tất cấu hình Thiệp Custom và thông báo dữ liệu giá hiện không khả dụng. | Đức Bình | STORY-035 | Draft | v0 | 2026-08-14 |
| [**BR-056**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3c9e47f4-edaa-45ea-9230-9ebdb7d4637f) | Quyền sở hữu | Tạo thiệp | Khách hàng chỉ được gắn thiệp vào Checkout thuộc tài khoản của mình. | Khách hàng tạo thiệp trong Checkout. | Backend kiểm tra quyền sở hữu của Checkout. | Không cho phép gắn thiệp vào Checkout không thuộc tài khoản khách hàng hiện tại. | Đức Bình | STORY-035 | Draft | v0 | 2026-08-14 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-051 | [Nội dung bắt buộc](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d8ba05bd-9cc4-4522-9c99-fe5f2bd1ca7a) |
| BR-052 | [Ảnh đính kèm](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c17b8c1c-62a5-42c0-95cd-91620de4366d) |
| BR-053 | [Một thiệp trong Checkout](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4e460995-5b6c-45b3-8491-13c0c1c80b05) |
| BR-054 | [Giá tạm tính](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b93ed86e-be68-4fbe-a7ab-8f1532f369bb) |
| BR-056 | [Quyền sở hữu](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3c9e47f4-edaa-45ea-9230-9ebdb7d4637f) |

### Dependencies
- [**STORY-038**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)
- [**STORY-042**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)

---

## Non-Functional Requirements

- Mở biểu mẫu tạo thiệp **p95 ≤ 1 giây** trong điều kiện bình thường.
- Tải Template và Size **p95 ≤ 1,5 giây**, không tính thời gian Core system bị chậm.
- API metadata không trả binary ảnh hoặc expose đường dẫn storage nội bộ.
- Thông báo lỗi không được chứa **stack trace** hoặc thông tin kỹ thuật nhạy cảm.

---

## Out of Scope

- Chọn thiệp đã có từ History cho Checkout.
- Hoàn tất Checkout, tạo Order và thanh toán.
- Tạo liên kết Thiệp-Order và tính bill cuối.
- Tải xuống, chia sẻ, chỉnh sửa hoặc xóa thiệp.