# STORY-019: Xóa content đã lưu lại

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Nháp - Cập nhật: 05/09/2026)

**Author:** Hồ Hoàng Nam
**Reviewer:** Nguyễn Đức Bình
**Approver:** Chưa chỉ định
**Owner:** Nguyễn Anh Quân

**Feedback yêu cầu sửa gần nhất:**
> Chưa có AC cho trường hợp xoá content hợp lệ - happy case. Cần định nghĩa rõ content đủ điều kiện xoá là thế nào.
> *- Nguyễn Đức Bình · 09:59 03/09/2026*

**Thống kê:** Rules: 3 | Unit Tests: 0 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn xóa content đã lưu không còn cần thiết để danh sách được quản lý gọn gàng và tránh sử dụng nhầm nội dung cũ
- **Context:** Admin cần loại bỏ các content đã lưu không còn giá trị sử dụng. Trước khi xóa, hệ thống phải kiểm tra quyền, trạng thái sử dụng và hiển thị popup xác nhận để tránh mất dữ liệu ngoài ý muốn. Chức năng này chỉ bao gồm xóa content đã lưu, không bao gồm tạo mới, xem, chỉnh sửa, lên lịch hoặc đăng bài
**Content đủ điều kiện xóa là khi:** 
  - Content không nằm trong bài đăng đã được lên lịch. 
  - Content không nằm trong bài đăng chưa được lên lịch
- **Sprint:** S2
- **Priority:** Must
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Admin đã đăng nhập

### Trigger
- Admin chọn “Xóa” tại một content đã lưu

## 3. FLOW
### Main Flow: Xóa content đã lưu
1. Admin chọn content cần xóa và nhấn “Xóa”.
2. Hệ thống kiểm tra quyền của Admin, trạng thái sử dụng của content.
3. Nếu content đủ điều kiện xóa, hệ thống hiển thị popup gồm tên content và cảnh báo thao tác không thể hoàn tác.
4. Admin chọn “Xóa” trên popup để xác nhận.
5. Hệ thống kiểm tra lại quyền và trạng thái sử dụng của content lần hai.
6. Hệ thống xóa content và hashtag được lưu.
7. Hệ thống thông báo “Xóa content thành công” và loại content khỏi danh sách.

### Alternative Flow
**ALT-01 — Admin hủy xóa**
- Khi popup xác nhận đang hiển thị, Admin chọn “Hủy”.
- Hệ thống đóng popup và không gửi yêu cầu xóa.
- Content và hashtag được giữ nguyên.

**ALT-02 — Xóa từ màn hình chi tiết**
- Tại màn hình chi tiết content, Admin chọn “Xóa”.
- Hệ thống tiếp tục kiểm tra và xử lý từ bước 2 của luồng chính.

### Exception Flow
**EXC-01 — Content không còn tồn tại**
- Tại bước 2 hoặc bước 5, nếu content không còn tồn tại, hệ thống dừng thao tác.
- Hệ thống không ghi nhận xóa thành công.
- Hệ thống thông báo content không còn tồn tại.
- Hệ thống tải lại danh sách để hiển thị dữ liệu hiện tại.

**EXC-02 — Content đang được sử dụng**
- Tại bước 2 hoặc bước 5, hệ thống xác định content đang được lịch đăng bài, bài đăng hoặc dữ liệu nghiệp vụ khác tham chiếu.
- Hệ thống không cho phép xóa content.
- Content và danh sách hashtag được giữ nguyên.
- Hệ thống thông báo content đang được sử dụng và hiển thị thông tin tham chiếu cần được xử lý trước.
- Hệ thống tải lại trạng thái sử dụng hiện tại của content.

**EXC-03 — Không thể xóa content**
- Tại bước 6, hệ thống không thể hoàn tất việc xóa content hoặc hashtag.
- Hệ thống xử lý việc xóa content và hashtag trong một giao dịch thống nhất.
- Hệ thống không xóa một phần dữ liệu.
- Content và hashtag hiện tại được giữ nguyên.
- Hệ thống không hiển thị hoặc ghi nhận xóa thành công.
- Hệ thống ghi nhật ký kết quả xóa thất bại.
- Hệ thống thông báo “Không thể xóa content, vui lòng thử lại” và cho phép Admin thử lại.

**EXC-04 — Phiên đăng nhập hết hạn khi xác nhận xóa**
- Phiên đăng nhập hết hạn trước khi Admin xác nhận xóa.
- Hệ thống từ chối yêu cầu xóa.
- Content và hashtag không bị thay đổi.
- Hệ thống thông báo phiên đăng nhập đã hết hạn.
- Hệ thống yêu cầu Admin đăng nhập lại.

**EXC-05 — Trạng thái content thay đổi trước khi xóa**
- Popup xác nhận xóa đang hiển thị.
- Content được cập nhật hoặc bắt đầu được đối tượng khác tham chiếu trước khi Admin xác nhận.
- Admin chọn “Xóa” trên popup.
- Hệ thống kiểm tra lại trạng thái content tại thời điểm xóa.
- Hệ thống không xóa content không còn đủ điều kiện.
- Hệ thống thông báo trạng thái content đã thay đổi.
- Hệ thống tải lại dữ liệu hiện tại và yêu cầu Admin thực hiện lại nếu content vẫn có thể xóa.

## 4. ACCEPTANCE CRITERIA
**AC-001**
- **Given:** Admin đang ở danh sách các content
- **When:** Admin chọn “Xóa” trên 1 content.
- **Then:** Hệ thống hiển thị popup gồm tên content và cảnh báo thao tác không thể hoàn tác. Admin bấm xác nhận
- **And:** Hệ thống xóa content.

**AC-002**
- **Given:** Popup xác nhận xóa đang hiển thị.
- **When:** Admin chọn “Hủy”.
- **Then:** Hệ thống đóng popup.
- **And:** Hệ thống không gửi yêu cầu xóa.
- **And:** Content và hashtag được giữ nguyên.

**AC-003**
- **Given:** Content đã bị xóa hoặc không còn tồn tại.
- **When:** Admin chọn hoặc xác nhận “Xóa”.
- **Then:** Hệ thống dừng thao tác và không ghi nhận xóa thành công.
- **And:** Hệ thống thông báo content không còn tồn tại.
- **And:** Hệ thống tải lại danh sách để hiển thị dữ liệu hiện tại.

**AC-005**
- **Given:** Bài đăng hoặc dữ liệu nghiệp vụ khác tham chiếu.
- **When:** Admin chọn hoặc xác nhận “Xóa”.
- **Then:** Hệ thống từ chối thao tác xóa.
- **And:** Hệ thống thông báo content đang được sử dụng.
- **And:** Content và danh sách hashtag được giữ nguyên.
- **And:** Hệ thống hiển thị thông tin tham chiếu cần được xử lý trước khi content có thể bị xóa.

**AC-006**
- **Given:** Admin đang xóa một content đủ điều kiện.
- **When:** Quá trình xóa content hoặc hashtag xảy ra lỗi.
- **Then:** Hệ thống không ghi nhận xóa thành công.
- **And:** Không có dữ liệu nào bị xóa một phần.
- **And:** Content và hashtag hiện tại được giữ nguyên.
- **And:** Hệ thống cho phép Admin thử lại.

**AC-007**
- **Given:** Popup xác nhận xóa đang hiển thị.
- **When:** Content được cập nhật hoặc bắt đầu được tham chiếu trước khi Admin xác nhận.
- **Then:** Hệ thống kiểm tra lại và không xóa content không còn đủ điều kiện.
- **And:** Hệ thống thông báo trạng thái content đã thay đổi.
- **And:** Hệ thống tải lại dữ liệu hiện tại.

**AC-008**
- **Given:** Popup xác nhận xóa đang hiển thị.
- **When:** Phiên đăng nhập hết hạn và Admin xác nhận xóa.
- **Then:** Hệ thống từ chối yêu cầu xóa.
- **And:** Content và hashtag không bị thay đổi.
- **And:** Hệ thống yêu cầu Admin đăng nhập lại.

**AC-009**
- **Given:** Admin đang xem chi tiết một content.
- **When:** Admin chọn “Xóa”.
- **Then:** Hệ thống thực hiện cùng bước kiểm tra và xác nhận như khi xóa từ danh sách.
- **And:** Kết quả xóa tuân theo cùng quy tắc quyền, trạng thái sử dụng và giao dịch dữ liệu.

## 5. REFERENCES
### Rules
- [BR-028](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/f57b8dc5-f3cc-49ad-9833-41018e846869)
- [BR-029](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/efe66a56-d882-4eb1-b50c-dbe53e03197d)
- [BR-060](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/be5d785f-b838-4112-b336-bfd9bef893a6)

### NON-FUNCTIONAL
- Backend phải kiểm tra quyền và trạng thái hiện tại của content trước khi xóa
- Content và dữ liệu hashtag phải được xóa trong cùng một giao dịch, nếu lỗi phải giữ nguyên toàn bộ dữ liệu

### OUT OF SCOPE
- Không bao gồm tạo content mới
- Không bao gồm xem danh sách hoặc chi tiết content
- Không bao gồm chỉnh sửa nội dung hoặc hashtag
- Không bao gồm lên lịch hoặc đăng content

### Rule Details

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Story liên quan |
|---|---|---|---|---|---|---|---|
| BR-028 | Ràng buộc dữ liệu khi xóa Content | AI Content | Content đang được dùng cho chức năng khác không được phép xóa. | Khi thao tác 'Xóa' một Content. | Hệ thống chặn xóa nếu Content đang được lịch đăng, bài đã đăng tham chiếu. | Không | STORY-019 |
| BR-029 | Xóa toàn vẹn dữ liệu Content | AI Content | Khi xóa Content, các dữ liệu phụ thuộc như liên kết Hashtag cũng phải được xóa sạch. | Khi xóa Content hợp lệ thành công. | Hệ thống chỉ ghi nhận xóa thành công khi Content và toàn bộ liên kết hashtag được xóa đầy đủ; nếu có lỗi phải giữ nguyên toàn bộ dữ liệu. | Không | STORY-019 |
| BR-060 | Kiểm tra lại và ghi nhận thao tác xóa Content | AI Content | Điều kiện xóa Content phải được kiểm tra lại tại thời điểm xác nhận và kết quả phải được ghi nhận. | Khi Quản trị viên xác nhận xóa Content. | Hệ thống kiểm tra Content còn tồn tại và không được dữ liệu khác tham chiếu (Lịch đăng bài đã lên lịch, chưa lên lịch). Nếu điều kiện sử dụng thay đổi trước khi xác nhận, hệ thống dừng xóa và tải lại dữ liệu hiện tại. | Không | STORY-019 |
