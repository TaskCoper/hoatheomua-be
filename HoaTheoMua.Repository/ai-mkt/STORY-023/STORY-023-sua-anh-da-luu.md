# STORY-023: Sửa ảnh đã lưu lại

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Nháp - Cập nhật: 05/09/2026)

**Author:** Hồ Hoàng Nam
**Reviewer:** Nguyễn Đức Bình
**Approver:** Chưa chỉ định
**Owner:** Nguyễn Anh Quân

**Feedback yêu cầu sửa gần nhất:**
> Ba trường sửa được (tên ảnh, mô tả, thẻ) đều không được STORY-003 khởi tạo. STR này nên có thêm Out Of Scope
> *- Nguyễn Đức Bình · 10:14 03/09/2026*

**Thống kê:** Rules: 2 | Unit Tests: 0 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn chỉnh sửa thông tin của ảnh đã lưu trong hệ thống để cập nhật các thông tin chưa chính xác hoặc không còn phù hợp mà không cần xóa và tạo lại ảnh.
- **Context:** Hệ thống lưu trữ các ảnh được tạo hoặc tải lên để phục vụ quá trình quản lý và sử dụng nội dung trên các nền tảng được hỗ trợ. Trong quá trình sử dụng, thông tin của ảnh đã lưu có thể cần được cập nhật do nhập sai, thiếu thông tin hoặc thông tin không còn phù hợp. Chức năng này cho phép Admin chỉnh sửa thông tin quản lý của ảnh đã lưu, không chỉnh sửa trực tiếp nội dung hình ảnh. Các thông tin được phép chỉnh sửa gồm tên ảnh, mô tả. Các thông tin do hệ thống quản lý như ID ảnh, tệp ảnh gốc, người tạo và thời gian tạo không được phép chỉnh sửa. Story này chỉ xử lý việc cập nhật thông tin của ảnh đã tồn tại trong hệ thống; không bao gồm tạo mới, xóa hoặc chỉnh sửa trực tiếp nội dung hình ảnh.
- **Sprint:** S1
- **Priority:** Must
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Admin đã đăng nhập vào trang quản trị
- Ảnh cần chỉnh sửa đang tồn tại trong hệ thống

### Trigger
- Admin chọn một ảnh đã lưu và chọn chức năng "Chỉnh sửa"

## 3. FLOW
### Main Flow: Sửa thông tin ảnh đã lưu
1. Quản trị viên chọn “Chỉnh sửa” tại một ảnh đã lưu.
2. Hệ thống tải thông tin mới nhất và thông tin phiên bản hiện tại của ảnh.
3. Hệ thống hiển thị màn hình chỉnh sửa với tên ảnh, mô tả được điền sẵn.
4. Hệ thống hiển thị mã định danh của ảnh, tệp ảnh gốc, người tạo và thời gian tạo ở chế độ chỉ xem.
5. Quản trị viên thay đổi tên ảnh, mô tả hoặc nhiều thông tin cùng lúc.
6. Quản trị viên chọn “Lưu”.
7. Hệ thống kiểm tra tính hợp lệ của dữ liệu.
8. Hệ thống kiểm tra lại quyền, tình trạng tồn tại và phiên bản hiện tại của ảnh tại thời điểm lưu.
9. Hệ thống chỉ cập nhật các thông tin được phép chỉnh sửa và giữ nguyên tệp ảnh gốc cùng các thông tin do hệ thống quản lý.
10. Hệ thống thông báo “Cập nhật thông tin ảnh thành công”.
11. Hệ thống hiển thị thông tin mới nhất của ảnh.

### Alternative Flow
**ALT-01 — Quản trị viên hủy chỉnh sửa**
- Màn hình chỉnh sửa đang hiển thị.
- Quản trị viên chọn “Hủy”.
- Hệ thống không gửi yêu cầu cập nhật.
- Hệ thống đóng màn hình chỉnh sửa.
- Thông tin ảnh hiện tại được giữ nguyên.

**ALT-02 — Chỉ sửa một phần thông tin**
- Quản trị viên chỉ thay đổi tên ảnh, mô tả.
- Quản trị viên chọn “Lưu”.
- Hệ thống chỉ cập nhật thông tin đã thay đổi.
- Các thông tin còn lại và tệp ảnh gốc được giữ nguyên.
- Luồng tiếp tục từ bước 8 của luồng chính.

**ALT-03 — Không có thay đổi để lưu**
- Quản trị viên mở màn hình chỉnh sửa nhưng không thay đổi dữ liệu.
- Quản trị viên chọn “Lưu”.
- Hệ thống xác định không có thay đổi.
- Hệ thống không thực hiện cập nhật.
- Hệ thống thông báo “Không có thay đổi để lưu”.
- Quản trị viên vẫn ở màn hình chỉnh sửa.

**ALT-04 — Thoát khi có thay đổi chưa lưu**
- Quản trị viên đã thay đổi dữ liệu nhưng chưa lưu.
- Quản trị viên đóng màn hình hoặc yêu cầu quay lại.
- Hệ thống cảnh báo có thay đổi chưa được lưu.
- Nếu Quản trị viên chọn “Tiếp tục chỉnh sửa”, hệ thống giữ màn hình và toàn bộ dữ liệu đang nhập.
- Nếu Quản trị viên chọn “Thoát không lưu”, hệ thống giữ nguyên thông tin ảnh hiện tại và loại bỏ dữ liệu chưa lưu.

### Exception Flow
**EXC-01 — Dữ liệu không hợp lệ**
- Tên ảnh bị để trống, chỉ chứa khoảng trắng hoặc dữ liệu nhập không đáp ứng giới hạn đã được cấu hình.
- Hệ thống không thực hiện cập nhật.
- Hệ thống hiển thị lỗi tại trường dữ liệu tương ứng và nêu rõ lý do.
- Hệ thống giữ toàn bộ dữ liệu Quản trị viên đang nhập.
- Quản trị viên được phép sửa dữ liệu và thử lưu lại.

**EXC-02 — Ảnh không còn tồn tại**
- Ảnh đã bị xóa hoặc không còn tồn tại trước khi Quản trị viên mở hoặc lưu màn hình chỉnh sửa.
- Hệ thống không thực hiện cập nhật.
- Hệ thống thông báo “Ảnh không còn tồn tại”.
- Hệ thống cho phép Quản trị viên quay lại danh sách ảnh.

**EXC-03 — Không thể cập nhật thông tin ảnh**
- Quá trình cập nhật tên ảnh, mô tả xảy ra lỗi.
- Hệ thống không lưu dữ liệu một phần.
- Thông tin ảnh hiện tại và tệp ảnh gốc được giữ nguyên.
- Hệ thống không ghi nhận hoặc thông báo cập nhật thành công.
- Hệ thống giữ dữ liệu Quản trị viên đang nhập.
- Hệ thống thông báo “Không thể cập nhật thông tin ảnh, vui lòng thử lại”.
- Quản trị viên được phép thử lại.

**EXC-04 — Thông tin ảnh đã thay đổi trong lúc chỉnh sửa**
- Quản trị viên A mở thông tin ảnh để chỉnh sửa.
- Quản trị viên B cập nhật thông tin ảnh thành công trước.
- Quản trị viên A lưu dữ liệu dựa trên phiên bản cũ.
- Hệ thống kiểm tra phiên bản tại thời điểm lưu.
- Hệ thống không ghi đè thông tin mới nhất.
- Hệ thống thông báo thông tin ảnh đã được người khác cập nhật.
- Hệ thống giữ dữ liệu Quản trị viên A đang nhập để đối chiếu.
- Quản trị viên A phải tải lại thông tin mới nhất trước khi lưu lại.

**EXC-05 — Phiên đăng nhập hết hạn**
- Phiên đăng nhập hết hạn trước khi Quản trị viên lưu.
- Hệ thống từ chối yêu cầu cập nhật.
- Thông tin ảnh và tệp ảnh gốc không bị thay đổi.
- Hệ thống thông báo phiên đăng nhập đã hết hạn.
- Hệ thống yêu cầu Quản trị viên đăng nhập lại.

**EXC-06 — Không thể tải thông tin hiện tại của ảnh**
- Hệ thống không thể tải đầy đủ tên ảnh, mô tả hoặc thông tin phiên bản hiện tại.
- Hệ thống không hiển thị biểu mẫu chỉnh sửa chứa dữ liệu thiếu.
- Hệ thống thông báo “Không thể tải thông tin ảnh, vui lòng thử lại”.
- Quản trị viên được phép tải lại hoặc quay về màn hình trước.

## 4. ACCEPTANCE CRITERIA
**AC-001 — Hiển thị thông tin chỉnh sửa ảnh**
- **Given:** Quản trị viên đang ở trang chi tiết ảnh
- **When:** Quản trị viên chọn “Chỉnh sửa”.
- **Then:** Hệ thống hiển thị tên ảnh, mô tả hiện tại để chỉnh sửa.
- **And:** Mã định danh của ảnh, tệp ảnh gốc, người tạo và thời gian tạo được hiển thị ở chế độ chỉ xem.
- **And:** Dữ liệu chưa bị thay đổi trước khi Quản trị viên chọn “Lưu”.

**AC-002 — Cập nhật thông tin ảnh thành công**
- **Given:** Quản trị viên đã nhập dữ liệu hợp lệ và thông tin ảnh chưa bị người khác cập nhật.
- **When:** Quản trị viên chọn “Lưu”.
- **Then:** Hệ thống chỉ cập nhật tên ảnh và mô tả đã thay đổi.
- **And:** Các thông tin còn lại và tệp ảnh gốc được giữ nguyên.
- **And:** Hệ thống ghi nhận người cập nhật và thời gian cập nhật.
- **And:** Hệ thống chỉ thông báo thành công sau khi toàn bộ thay đổi được lưu.
- **And:** Hệ thống hiển thị thông tin mới nhất của ảnh.

**AC-003 — Dữ liệu chỉnh sửa không hợp lệ**
- **Given:** Quản trị viên đang chỉnh sửa thông tin ảnh.
- **When:** Quản trị viên lưu tên ảnh để trống, chỉ chứa khoảng trắng hoặc dữ liệu vượt quá giới hạn đã được cấu hình.
- **Then:** Hệ thống không cập nhật thông tin ảnh.
- **And:** Hệ thống hiển thị lỗi tại trường dữ liệu tương ứng và nêu rõ lý do.
- **And:** Hệ thống giữ dữ liệu Quản trị viên đang nhập để chỉnh sửa.

**AC-004 — Hủy chỉnh sửa**
- **Given:** Màn hình chỉnh sửa thông tin ảnh đang hiển thị.
- **When:** Quản trị viên chọn “Hủy”.
- **Then:** Hệ thống không gửi yêu cầu cập nhật.
- **And:** Thông tin ảnh và tệp ảnh gốc được giữ nguyên.

**AC-005 — Không có thay đổi để lưu**
- **Given:** Quản trị viên chưa thay đổi tên ảnh hoặc mô tả.
- **When:** Quản trị viên chọn “Lưu”.
- **Then:** Hệ thống không thực hiện cập nhật.
- **And:** Thông tin ảnh hiện tại được giữ nguyên.
- **And:** Hệ thống thông báo “Không có thay đổi để lưu”.

**AC-006 — Xử lý thoát khi chưa lưu**
- **Given:** Quản trị viên có thay đổi chưa lưu.
- **When:** Quản trị viên yêu cầu đóng hoặc rời màn hình.
- **Then:** Hệ thống yêu cầu xác nhận.
- **And:** Chọn tiếp tục chỉnh sửa phải giữ nguyên dữ liệu đang nhập.
- **And:** Chọn thoát không lưu phải giữ nguyên thông tin ảnh hiện tại.

**AC-007 — Ảnh không còn tồn tại**
- **Given:** Ảnh đã bị xóa hoặc không còn tồn tại.
- **When:** Quản trị viên mở hoặc lưu màn hình chỉnh sửa.
- **Then:** Hệ thống không thực hiện cập nhật.
- **And:** Hệ thống thông báo ảnh không còn tồn tại.
- **And:** Hệ thống cho phép Quản trị viên quay lại danh sách ảnh.

**AC-008 — Lỗi cập nhật không tạo dữ liệu một phần**
- **Given:** Quản trị viên đang lưu dữ liệu hợp lệ.
- **When:** Quá trình cập nhật tên ảnh hoặc mô tả xảy ra lỗi.
- **Then:** Hệ thống không ghi nhận cập nhật thành công.
- **And:** Không có thông tin ảnh nào được cập nhật một phần.
- **And:** Thông tin hiện tại và tệp ảnh gốc được giữ nguyên.
- **And:** Dữ liệu Quản trị viên đang nhập được giữ lại để thử lại.

**AC-009 — Ngăn ghi đè khi thông tin ảnh đã thay đổi**
- **Given:** Quản trị viên đang chỉnh sửa dựa trên một phiên bản thông tin ảnh cũ.
- **When:** Thông tin ảnh đã được quản trị viên khác cập nhật trước khi Quản trị viên lưu.
- **Then:** Hệ thống không ghi đè thông tin mới nhất.
- **And:** Hệ thống thông báo thông tin ảnh đã thay đổi.
- **And:** Quản trị viên phải tải lại thông tin mới nhất trước khi lưu lại.

**AC-010 — Phiên đăng nhập hết hạn**
- **Given:** Quản trị viên đang chỉnh sửa thông tin ảnh.
- **When:** Phiên đăng nhập hết hạn và Quản trị viên chọn “Lưu”.
- **Then:** Hệ thống từ chối yêu cầu cập nhật.
- **And:** Thông tin ảnh và tệp ảnh gốc không bị thay đổi.
- **And:** Hệ thống yêu cầu Quản trị viên đăng nhập lại.

**AC-012 — Không thay đổi tệp ảnh gốc**
- **Given:** Quản trị viên đang chỉnh sửa tên ảnh hoặc mô tả.
- **When:** Hệ thống lưu thay đổi thành công.
- **Then:** Tệp ảnh gốc và các phiên bản theo tỷ lệ không bị thay đổi.
- **And:** Mã định danh của ảnh, người tạo và thời gian tạo được giữ nguyên.

## 5. REFERENCES
### Rules
- [BR-066](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/017d3150-35dc-47e7-91c9-39b0dac8e258)
- [BR-067](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/d1115968-e111-4cde-9899-80fd76368feb)

### NON-FUNCTIONAL
- Thời gian tải thông tin và phản hồi thao tác cập nhật dưới 2 giây trong điều kiện hoạt động bình thường.
- Hệ thống phải kiểm tra quyền trước khi cho phép cập nhật ảnh.
- Việc cập nhật thông tin không được làm thay đổi tệp ảnh gốc.
- Hệ thống phải đảm bảo dữ liệu ảnh được cập nhật nhất quán sau khi lưu thành công.

### OUT OF SCOPE
- Không chỉnh sửa trực tiếp hình ảnh.
- Không cắt, xoay, thay đổi kích thước hoặc tỷ lệ ảnh.
- Không thay đổi định dạng hoặc dung lượng tệp.
- Không thay thế tệp ảnh hiện tại bằng tệp khác.
- Không gắn hoặc sử dụng ảnh trong content, lịch đăng bài hoặc bài đăng.
- Không chỉnh sửa đồng thời nhiều ảnh.
- Không tạo ảnh mới hoặc tạo lại ảnh bằng AI.

### Rule Details

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Story liên quan |
|---|---|---|---|---|---|---|---|
| BR-066 | Phạm vi chỉnh sửa thông tin ảnh | AI Image | Ảnh đã lưu chỉ cho phép chỉnh sửa tên và mô tả. | Khi Quản trị viên mở hoặc lưu màn hình chỉnh sửa thông tin ảnh. | Hệ thống cho sửa tên bắt buộc từ 1 đến 200 ký tự và mô tả không bắt buộc tối đa 1.000 ký tự. ID ảnh, tệp ảnh gốc, các phiên bản tỷ lệ, người tạo và thời gian tạo không được chỉnh sửa. | Không | STORY-023 |
| BR-067 | Cập nhật thông tin ảnh phải toàn vẹn | AI Image | Tên và mô tả ảnh phải được cập nhật đầy đủ hoặc giữ nguyên toàn bộ dữ liệu cũ. | Khi Quản trị viên lưu thông tin ảnh hợp lệ. | Hệ thống cập nhật tên, mô tả, người cập nhật và thời gian cập nhật mà không thay đổi nội dung ảnh. Nếu lưu lỗi hoặc ảnh đã có thay đổi mới hơn, hệ thống không ghi đè và yêu cầu tải lại dữ liệu mới nhất. | Không | STORY-023 |
