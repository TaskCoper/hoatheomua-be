# STORY-035 — Khách hàng cấu hình thiệp tại Checkout

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã đăng nhập, tôi muốn cấu hình thiệp tại Checkout, để lựa chọn loại thiệp và cung cấp nội dung thiệp cần thiết cho đơn hàng trước khi thanh toán. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Tại Checkout, khách hàng có thể không chọn thiệp hoặc chọn thêm thiệp cho bó hoa. Thiệp gồm hai loại: Thiệp Miễn phí và Thiệp Custom. Checkout chỉ thu thập và lưu thông tin yêu cầu thiệp để tính giá và tạo Order. Đối với Thiệp Custom, hệ thống chỉ cho phép thực hiện bước tạo thiệp sau khi Order đã được thanh toán thành công.
---

## Conditions

### Preconditions

- Khách hàng đã đăng nhập.
- Checkout hợp lệ và thuộc khách hàng hiện tại.
- Bó hoa của Checkout còn hợp lệ.
- Các Size và Mẫu thiệp được hiển thị phải đang khả dụng.

### Trigger

> - Khách hàng chọn thêm thiệp hoặc cấu hình thiệp tại Checkout.


---

## Flow

### Main Flow

1. Khách hàng mở chức năng cấu hình thiệp tại Checkout.
2. Hệ thống hiển thị tùy chọn Có thiệp / Không có thiệp.
3. Nếu chọn Có thiệp, hệ thống cho khách hàng chọn Thiệp Miễn phí hoặc Thiệp Custom.
4. Khách hàng nhập Nội dung thiệp.
5. Nếu chọn Thiệp Custom, khách hàng chọn hình thức In hoặc Viết tay.
6. Nếu chọn Thiệp Custom, khách hàng chọn Size, Mẫu thiệp và có thể chọn 01 ảnh đính kèm.
7. Hệ thống validate dữ liệu.
8. Hệ thống đếm số lượng chữ/từ của Nội dung thiệp theo quy tắc hiện hành.
9. Hệ thống tính giá thiệp tương ứng.
10. Hệ thống lưu thông tin cấu hình thiệp vào Checkout.
11. Hệ thống lưu số lượng chữ/từ dùng để tính giá thiệp vào dữ liệu Checkout để làm cơ sở snapshot khi tạo Order.
12. Không tạo ảnh thiệp và không gọi AI tại bước này.
13. Khách hàng tiếp tục thanh toán.

### Alternative Flows

#### ALT-01 — Không có ảnh đính kèm
- Khách hàng cấu hình Thiệp Custom và không chọn ảnh đính kèm.
- Hệ thống vẫn cho phép lưu cấu hình Thiệp Custom nếu các dữ liệu bắt buộc còn lại hợp lệ.

#### ALT-02 — Không chọn thiệp
- Khách hàng chọn Không có thiệp tại Checkout.
- Hệ thống không hiển thị yêu cầu nhập Nội dung thiệp, Size, Mẫu thiệp hoặc Hình thức.
- Hệ thống không tính thêm giá thiệp.
- Checkout được tiếp tục với thông tin bó hoa hiện tại.
- Khách hàng tiếp tục sang bước thanh toán.

#### ALT-03 — Chọn Thiệp Miễn phí
- Khách hàng chọn Có thiệp.
- Khách hàng chọn loại Thiệp Miễn phí.
- Hệ thống hiển thị các trường thông tin áp dụng cho Thiệp Miễn phí.
- Khách hàng nhập Nội dung thiệp.
- Hệ thống kiểm tra giới hạn nội dung theo cấu hình Thiệp Miễn phí.
- Nếu dữ liệu hợp lệ, hệ thống lưu thông tin Thiệp Miễn phí vào Checkout.
- Hệ thống không áp dụng giá Size hoặc phụ phí Thiệp Custom.
- Khách hàng tiếp tục sang bước thanh toán.

#### ALT-04 — Chọn Thiệp Custom dạng In
- Khách hàng chọn Có thiệp.
- Khách hàng chọn loại Thiệp Custom.
- Khách hàng chọn hình thức In.
- Hệ thống hiển thị danh sách Size và Mẫu thiệp đang khả dụng.
- Khách hàng chọn Size, Mẫu thiệp và có thể chọn 01 ảnh đính kèm.
- Khách hàng nhập Nội dung thiệp.
- Hệ thống kiểm tra dữ liệu và giới hạn số lượng chữ/từ theo Size được chọn.
- Hệ thống xác định Giá Thiệp Custom dạng In bằng Giá Size được chọn.
- Nếu dữ liệu hợp lệ, hệ thống lưu cấu hình Thiệp Custom dạng In và giá tương ứng vào Checkout.
- Hệ thống không tạo ảnh thiệp và không gọi AI tại Checkout.
- Khách hàng tiếp tục sang bước thanh toán.

#### ALT-05 — Chọn Thiệp Custom dạng Viết tay
- Khách hàng chọn Có thiệp.
- Khách hàng chọn loại Thiệp Custom.
- Khách hàng chọn hình thức Viết tay.
- Hệ thống hiển thị các Size được phép áp dụng cho Thiệp Custom dạng Viết tay.
- Khách hàng chọn Size, Mẫu thiệp và có thể chọn 01 ảnh đính kèm.
- Khách hàng nhập Nội dung thiệp.
- Hệ thống đếm số lượng chữ/từ của Nội dung thiệp theo quy tắc hiện hành.
- Hệ thống xác định cấu hình phụ phí viết tay Active tương ứng với số lượng chữ/từ.
- Hệ thống xác định Giá Thiệp Custom dạng Viết tay bằng Giá Size cộng Phụ phí viết tay theo số lượng chữ/từ.
- Nếu dữ liệu hợp lệ, hệ thống lưu cấu hình Thiệp Custom dạng Viết tay và giá tương ứng vào Checkout.
- Hệ thống không tạo ảnh thiệp và không gọi AI tại Checkout.
- Khách hàng tiếp tục sang bước thanh toán.

### Exception Flows

#### EXC-01 — Dữ liệu bắt buộc không hợp lệ
- Khách hàng đang cấu hình thiệp tại Checkout.
- Khách hàng nhập thiếu hoặc nhập không hợp lệ một trong các trường bắt buộc tương ứng với loại thiệp đã chọn.
- Khách hàng thực hiện lưu cấu hình thiệp hoặc tiếp tục Checkout.
- Backend kiểm tra lại toàn bộ dữ liệu đầu vào.
- Backend phát hiện dữ liệu không hợp lệ.
- Hệ thống không lưu cấu hình thiệp vào Checkout.
- Hệ thống không cập nhật giá thiệp vào Checkout.
- Hệ thống hiển thị lỗi tại trường tương ứng.

#### EXC-02 — File upload không hợp lệ
- Khách hàng đang cấu hình thiệp tại Checkout.
- Khách hàng chọn ảnh đính kèm.
- File ảnh không thuộc định dạng PNG, JPG hoặc có dung lượng vượt quá 10 MB.
- Hệ thống từ chối file ảnh.
- Hệ thống hiển thị lý do file không hợp lệ.
- Hệ thống cho phép khách hàng chọn file khác.
- Khách hàng vẫn có thể tiếp tục lưu cấu hình Thiệp Custom không có ảnh đính kèm nếu các dữ liệu bắt buộc còn lại hợp lệ.

#### EXC-03 — Template hoặc Size không còn khả dụng
- Khách hàng đang cấu hình thiệp tại Checkout.
- Khách hàng đã chọn Template hoặc Size.
- Khách hàng thực hiện lưu cấu hình thiệp hoặc tiếp tục Checkout.
- Backend kiểm tra lại trạng thái khả dụng của Template và Size từ Core system.
- Backend phát hiện Template hoặc Size đã ngừng khả dụng hoặc bị xóa mềm.
- Hệ thống không lưu cấu hình Thiệp Custom vào Checkout.
- Hệ thống không sử dụng Template hoặc Size không còn khả dụng để tính giá.
- Hệ thống yêu cầu khách hàng chọn Template hoặc Size hiện còn khả dụng.

#### EXC-04 — Checkout không hợp lệ
- Khách hàng thực hiện lưu cấu hình thiệp trong Checkout.
- Backend kiểm tra Checkout hiện tại.
- Backend phát hiện Checkout không tồn tại, không thuộc khách hàng hiện tại hoặc không còn ở trạng thái cho phép chỉnh sửa.
- Hệ thống từ chối lưu cấu hình thiệp.
- Hệ thống không cập nhật giá hoặc thông tin thiệp vào Checkout.
- Hệ thống hiển thị lỗi tương ứng và không để lộ dữ liệu của Checkout/khách hàng khác.

---

## Acceptance Criteria

### AC-001
- **Given**: Checkout hợp lệ và thuộc khách hàng hiện tại.
- **When**: Khách hàng chọn "Không có thiệp".
- **Then**:
  - Hệ thống không yêu cầu nhập thông tin thiệp.
  - Hệ thống không tính thêm giá thiệp.
  - Hệ thống cho phép khách hàng tiếp tục Checkout.

### AC-002
- **Given**: Checkout hợp lệ và khách hàng đã chọn "Có thiệp".
- **When**: Khách hàng chọn "Thiệp Miễn phí" và nhập Nội dung thiệp hợp lệ.
- **Then**:
  - Hệ thống lưu thông tin Thiệp Miễn phí vào Checkout.
  - Hệ thống không yêu cầu Người gửi hoặc Người nhận.
  - Hệ thống không áp dụng giá Size hoặc phụ phí của Thiệp Custom.
  - Hệ thống cho phép khách hàng tiếp tục Checkout.

### AC-003
- **Given**: Checkout hợp lệ và khách hàng đã chọn "Thiệp Custom".
- **When**: Khách hàng chọn hình thức "In", Size, Mẫu thiệp và Nội dung thiệp hợp lệ.
- **Then**:
  - Hệ thống xác định giá thiệp bằng giá của Size được chọn.
  - Hệ thống lưu cấu hình Thiệp Custom dạng In vào Checkout.
  - Hệ thống không tạo ảnh thiệp và không gọi AI tại bước này.

### AC-004
- **Given**: Checkout hợp lệ và khách hàng đã chọn "Thiệp Custom".
- **When**: Khách hàng chọn hình thức "Viết tay", Size, Mẫu thiệp và Nội dung thiệp hợp lệ.
- **Then**:
  - Hệ thống đếm số lượng chữ/từ của Nội dung thiệp theo quy tắc hiện hành.
  - Hệ thống xác định cấu hình phụ phí viết tay Active tương ứng với số lượng chữ/từ.
  - Giá thiệp bằng Giá Size cộng Phụ phí viết tay.
  - Hệ thống lưu cấu hình Thiệp Custom dạng Viết tay vào Checkout.
  - Hệ thống không gọi AI tại bước này.

### AC-005
- **Given**: Khách hàng đang cấu hình thiệp tại Checkout.
- **When**: Một hoặc nhiều trường bắt buộc bị thiếu hoặc không hợp lệ.
- **Then**:
  - Backend từ chối lưu cấu hình thiệp.
  - Hệ thống không cập nhật giá thiệp vào Checkout.
  - Hệ thống hiển thị lỗi tại trường tương ứng.

### AC-006
- **Given**: Khách hàng đang cấu hình Thiệp Custom và chọn ảnh đính kèm.
- **When**: File không thuộc định dạng PNG/JPG hoặc vượt quá 10 MB.
- **Then**:
  - Hệ thống từ chối file.
  - Hệ thống hiển thị lý do file không hợp lệ.
  - Hệ thống cho phép khách hàng chọn file khác hoặc tiếp tục không có ảnh đính kèm.

### AC-007
- **Given**: Khách hàng đang cấu hình Thiệp Custom và các dữ liệu bắt buộc khác hợp lệ.
- **When**: Khách hàng không chọn ảnh đính kèm.
- **Then**:
  - Hệ thống vẫn cho phép lưu cấu hình thiệp vào Checkout.
  - Hệ thống không yêu cầu ảnh đính kèm để tiếp tục Checkout.

### AC-008
- **Given**: Khách hàng đã chọn Template và Size cho Thiệp Custom.
- **When**: Backend phát hiện Template hoặc Size đã Inactive hoặc bị xóa mềm.
- **Then**:
  - Hệ thống không lưu cấu hình Thiệp Custom.
  - Hệ thống không sử dụng Template hoặc Size không còn khả dụng để tính giá.
  - Hệ thống yêu cầu khách hàng chọn Template hoặc Size còn khả dụng.

### AC-009
- **Given**: Khách hàng đang cấu hình thiệp tại Checkout.
- **When**: Nội dung thiệp vượt quá giới hạn số lượng chữ/từ áp dụng.
- **Then**:
  - Backend từ chối dữ liệu.
  - Hệ thống không lưu cấu hình thiệp vào Checkout.
  - Hệ thống hiển thị lỗi tương ứng.

### AC-010
- **Given**: Checkout không tồn tại, không thuộc khách hàng hiện tại hoặc không còn ở trạng thái cho phép chỉnh sửa.
- **When**: Khách hàng thực hiện lưu cấu hình thiệp.
- **Then**:
  - Backend từ chối thao tác.
  - Hệ thống không cập nhật thông tin hoặc giá thiệp vào Checkout.
  - Hệ thống không để lộ dữ liệu của Checkout hoặc khách hàng khác.

### AC-011
- **Given**: Khách hàng đã cấu hình Thiệp Custom hợp lệ tại Checkout.
- **When**: Hệ thống lưu cấu hình thiệp thành công.
- **Then**:
  - Hệ thống không tạo AI Job, không tạo ảnh thiệp, không tạo History record và không ghi nhận lượt generate.
  - Thiệp Custom chỉ được phép tạo sau khi Order được thanh toán thành công.

### AC-012
- **Given**: Khách hàng đã chọn loại thiệp và nhập dữ liệu hợp lệ.
- **When**: Hệ thống xác định được giá thiệp.
- **Then**:
  - Hệ thống cập nhật giá thiệp vào tổng giá tạm tính của Checkout.
  - Giá hiển thị phải tương ứng với loại thiệp, hình thức, Size và phụ phí áp dụng.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Nguồn | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực | Ghi chú / Link logic |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| [BR-051](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d8ba05bd-9cc4-4522-9c99-fe5f2bd1ca7a) | Nội dung bắt buộc | Tạo thiệp | Nội dung thiệp/Lời chúc là nội dung bắt buộc khi khách hàng chọn thiệp; hệ thống không thu thập Người gửi và Người nhận trong luồng thiệp AI. | Khách hàng lưu cấu hình thiệp hoặc tiếp tục Checkout. | Hệ thống kiểm tra Nội dung thiệp/Lời chúc theo giới hạn số lượng từ áp dụng cho loại thiệp, Size và hình thức đã chọn. Hệ thống không yêu cầu, không validate và không lưu Người gửi hoặc Người nhận như dữ liệu bắt buộc của luồng thiệp AI. | Dữ liệu vượt quá giới hạn số lượng từ không được chấp nhận và cấu hình thiệp không được lưu vào Checkout. | Product discussion 2026-09-11; STORY-035 | Đức Bình | STORY-035 | Draft | v0 | 2026-09-11 | Rule này thay thế yêu cầu cũ có Người gửi tối đa 20 từ và Người nhận tối đa 20 từ. |
| [BR-052](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c17b8c1c-62a5-42c0-95cd-91620de4366d) | Ảnh đính kèm | Tạo thiệp | Mỗi yêu cầu có tối đa 01 ảnh đính kèm định dạng PNG hoặc JPG và dung lượng tối đa 10 MB. | Khách hàng tải ảnh đính kèm khi cấu hình Thiệp Custom tại Checkout. | Hệ thống kiểm tra số lượng, định dạng và dung lượng ảnh đính kèm. | File không đúng định dạng, vượt quá dung lượng hoặc vượt quá số lượng không được chấp nhận. Khách hàng vẫn có thể tiếp tục mà không có ảnh đính kèm nếu các dữ liệu bắt buộc khác hợp lệ. | Google Sheet rule source | Đức Bình | STORY-035 | Draft | v0 | 2026-08-14 | — |
| [BR-053](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4e460995-5b6c-45b3-8491-13c0c1c80b05) | Một thiệp trong Checkout | Tạo thiệp | Mỗi Checkout được có tối đa 01 lựa chọn thiệp tại một thời điểm. | Khách hàng chọn hoặc thay đổi thiệp tại Checkout. | Nếu Checkout đã có cấu hình hoặc thiệp được chọn, lựa chọn mới thay thế lựa chọn trước đó. | Việc thay đổi lựa chọn trong Checkout không xóa dữ liệu thiệp đã tồn tại trong History. | Google Sheet rule source | Đức Bình | STORY-035 | Draft | v0 | 2026-08-14 | — |
| [BR-054](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b93ed86e-be68-4fbe-a7ab-8f1532f369bb) | Giá tạm tính | Tạo thiệp | Giá thiệp được xác định theo loại thiệp và cấu hình hiện hành tại thời điểm khách hàng cấu hình thiệp tại Checkout. | Khách hàng chọn loại thiệp và nhập đầy đủ dữ liệu cần thiết để hệ thống tính giá. | Thiệp Miễn phí không áp dụng giá Size hoặc phụ phí Thiệp Custom. Thiệp Custom dạng In có Giá thiệp bằng Giá hiện hành của Size được chọn. Thiệp Custom dạng Viết tay có Giá thiệp bằng Giá hiện hành của Size được chọn cộng Phụ phí viết tay Active tương ứng với số lượng từ của Nội dung thiệp. Hệ thống lấy giá Size và cấu hình phụ phí hiện hành từ Core Database. Giá tính được được cập nhật vào tổng giá tạm tính của Checkout. Khi Order được tạo, hệ thống lưu snapshot giá áp dụng và số lượng từ đã dùng để tính tiền. | Nếu không tìm thấy giá Size hoặc không tìm thấy cấu hình phụ phí viết tay hợp lệ tương ứng, hệ thống không cho lưu hoàn tất cấu hình Thiệp Custom và thông báo dữ liệu giá hiện không khả dụng. | Product discussion 2026-09-11; STORY-035; STORY-039 | Đức Bình | STORY-035; STORY-039 | Draft | v0 | 2026-09-11 | Snapshot số lượng từ đã thanh toán được dùng để kiểm tra mọi chỉnh sửa Nội dung thiệp sau thanh toán theo BR-272. |
| [BR-056](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3c9e47f4-edaa-45ea-9230-9ebdb7d4637f) | Quyền sở hữu | Tạo thiệp | Khách hàng chỉ được gắn thiệp vào Checkout thuộc tài khoản của mình. | Khách hàng tạo thiệp trong Checkout. | Backend kiểm tra quyền sở hữu của Checkout. | Không cho phép gắn thiệp vào Checkout không thuộc tài khoản khách hàng hiện tại. | Google Sheet rule source | Đức Bình | STORY-035 | Draft | v0 | 2026-08-14 | — |
| [BR-272](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/767acec8-626b-48e7-9352-622cf0a55a61) | Giới hạn chỉnh sửa nội dung thiệp theo số lượng từ đã thanh toán | Tạo thiệp | Sau khi Order đã thanh toán thành công, nếu khách hàng chỉnh sửa Nội dung thiệp trước khi tạo ảnh AI, nội dung sau chỉnh sửa không được vượt quá số lượng từ đã được thanh toán trong Order. | Khách hàng chỉnh sửa Nội dung thiệp của Thiệp Custom sau khi Order đã thanh toán thành công và trước khi gửi yêu cầu tạo ảnh AI, bao gồm cả thao tác Tạo lại hợp lệ trong bước sau thanh toán. | Hệ thống đếm số lượng từ của Nội dung thiệp sau chỉnh sửa theo quy tắc đếm từ hiện hành. Hệ thống so sánh số lượng từ sau chỉnh sửa với số lượng từ đã được dùng để tính tiền và lưu trong snapshot Order. Nếu số lượng từ sau chỉnh sửa nhỏ hơn hoặc bằng số lượng từ đã thanh toán, hệ thống cho phép tiếp tục tạo thiệp AI. Nếu số lượng từ sau chỉnh sửa vượt quá số lượng từ đã thanh toán, hệ thống không cho phép tiếp tục tạo thiệp AI và yêu cầu khách hàng rút gọn nội dung hoặc cập nhật thanh toán theo nghiệp vụ được quy định riêng. | Không áp dụng rule này cho thao tác chọn thiệp đã tạo từ History cho Checkout, vì thao tác đó không mở biểu mẫu chỉnh sửa nội dung và không gọi AI. | Product discussion 2026-09-11; STORY-035; STORY-039 | Đức Bình | STORY-035; STORY-039 | Draft | v0 | 2026-09-11 | Rule này không tự định nghĩa quy trình thu thêm tiền khi khách hàng muốn tăng số lượng từ sau thanh toán; quy trình đó nằm ngoài phạm vi STORY-035 và STORY-039 nếu chưa được chốt riêng. Chức năng Tạo lại thiệp từ History không còn được hỗ trợ; Tạo lại hợp lệ chỉ diễn ra trong bước tạo Thiệp Custom sau khi Order đã thanh toán thành công. |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-051 | [BR-051](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d8ba05bd-9cc4-4522-9c99-fe5f2bd1ca7a) |
| BR-052 | [BR-052](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c17b8c1c-62a5-42c0-95cd-91620de4366d) |
| BR-053 | [BR-053](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4e460995-5b6c-45b3-8491-13c0c1c80b05) |
| BR-054 | [BR-054](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b93ed86e-be68-4fbe-a7ab-8f1532f369bb) |
| BR-056 | [BR-056](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3c9e47f4-edaa-45ea-9230-9ebdb7d4637f) |
| BR-272 | [BR-272](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/767acec8-626b-48e7-9352-622cf0a55a61) |

### Dependencies

- [STORY-038](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)
- [STORY-039](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [STORY-042](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35d883ea-e9bc-42c0-93e8-6dfcb380bd39)

---

## Non-Functional Requirements

- Mở biểu mẫu tạo thiệp p95 ≤ 1 giây trong điều kiện bình thường.
- Tải Template và Size p95 ≤ 1,5 giây, không tính thời gian Core system bị chậm.
- API metadata không trả binary ảnh hoặc expose đường dẫn storage nội bộ.
- Thông báo lỗi không được chứa stack trace hoặc thông tin kỹ thuật nhạy cảm.

---

## Out of Scope

- Chọn thiệp đã có từ History cho Checkout.
- Hoàn tất Checkout, tạo Order và thanh toán.
- Tạo liên kết Thiệp-Order và tính bill cuối.
- Tải xuống, chia sẻ, chỉnh sửa hoặc xóa thiệp.

---
