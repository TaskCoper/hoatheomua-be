# STORY-022: Xóa ảnh đã lưu lại

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Nháp - Cập nhật: 05/09/2026)

**Author:** Nguyễn Anh Quân
**Reviewer:** Nguyễn Đức Bình
**Approver:** Chưa chỉ định
**Owner:** Nguyễn Anh Quân

**Feedback yêu cầu sửa gần nhất:**
> Admin không được truy cập chức năng xóa ảnh nếu chưa đăng nhập hoặc tài khoản không có quyền quản lý thư viện ảnh. Câu này đặt ở Preconditions chưa hợp lý, Precondition là các điều phải đúng, phải có để thực thi chứ không chứa quy tắc. Cần làm rõ quan hệ cha-con của các loại ảnh được sinh ra từ ảnh gốc, định nghĩa rõ tụi nó có quan hệ với nhau không hay độc lập.
> *- Nguyễn Đức Bình · 10:13 03/09/2026*

**Thống kê:** Rules: 2 | Unit Tests: 0 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn xóa ảnh đã lưu không còn cần thiết để thư viện ảnh được quản lý gọn gàng và tránh sử dụng nhầm hình ảnh cũ
- **Context:** Admin cần loại bỏ các ảnh đã lưu không còn giá trị sử dụng. Trước khi xóa, hệ thống phải kiểm tra quyền, trạng thái sử dụng và hiển thị popup xác nhận để tránh mất dữ liệu ngoài ý muốn. Chức năng này chỉ bao gồm xóa ảnh đã lưu, không bao gồm tạo mới, xem, chỉnh sửa, tải xuống hoặc sử dụng ảnh trong bài đăng
- **Sprint:** S3
- **Priority:** Must
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Admin đã đăng nhập vào hệ thống.

### Trigger
- Admin chọn “Xóa” tại một ảnh đã lưu

## 3. FLOW
### Main Flow: Xóa ảnh đã lưu
1. Quản trị viên chọn “Xóa” tại một ảnh đã lưu.
2. Nếu ảnh đủ điều kiện xóa, hệ thống hiển thị cửa sổ xác nhận gồm ảnh thu nhỏ, tên ảnh và cảnh báo thao tác xóa không thể hoàn tác.
3. Quản trị viên chọn “Xóa” trên cửa sổ xác nhận.
4. Hệ thống kiểm tra lại quyền, tình trạng tồn tại và tình trạng sử dụng của ảnh tại thời điểm xác nhận.
5. Hệ thống xóa ảnh gốc, các phiên bản theo tỷ lệ và toàn bộ thông tin được lưu cùng ảnh trong một giao dịch thống nhất.
6. Hệ thống thông báo “Xóa ảnh thành công”.
7. Hệ thống loại ảnh khỏi danh sách ảnh có thể sử dụng.

### Alternative Flow
**ALT-01 — Quản trị viên hủy xóa**
- Cửa sổ xác nhận xóa đang hiển thị.
- Quản trị viên chọn “Hủy”.
- Hệ thống đóng cửa sổ xác nhận và không gửi yêu cầu xóa.
- Ảnh gốc, các phiên bản theo tỷ lệ và thông tin ảnh được giữ nguyên.

**ALT-02 — Xóa ảnh từ màn hình chi tiết**
- Quản trị viên đang xem thông tin chi tiết của một ảnh.
- Quản trị viên chọn “Xóa”.
- Hệ thống thực hiện cùng bước kiểm tra và xác nhận như khi xóa ảnh từ danh sách.
- Sau khi xóa thành công, hệ thống quay lại danh sách và giữ nguyên từ khóa tìm kiếm cùng các điều kiện lọc trước đó.

### Exception Flow
**EXC-01 — Ảnh không còn tồn tại**
- Ảnh đã bị xóa hoặc không còn tồn tại trước khi Quản trị viên xác nhận.
- Hệ thống dừng thao tác và không gửi yêu cầu xóa tiếp theo.
- Hệ thống không ghi nhận xóa thành công.
- Hệ thống thông báo ảnh không còn tồn tại.
- Hệ thống tải lại danh sách để hiển thị dữ liệu hiện tại.

**EXC-02 — Ảnh đang được sử dụng**
- Ảnh đang được nội dung, lịch đăng bài hoặc dữ liệu nghiệp vụ khác tham chiếu.
- Hệ thống không cho phép xóa ảnh.
- Ảnh gốc, các phiên bản theo tỷ lệ và thông tin ảnh được giữ nguyên.
- Hệ thống thông báo ảnh đang được sử dụng và hiển thị thông tin tham chiếu cần được xử lý trước.

**EXC-03 — Không thể xóa ảnh**
- Quá trình xóa ảnh gốc, phiên bản theo tỷ lệ hoặc thông tin ảnh xảy ra lỗi.
- Hệ thống xử lý toàn bộ thao tác xóa trong một giao dịch thống nhất.
- Hệ thống không xóa một phần dữ liệu.
- Ảnh và toàn bộ dữ liệu liên quan được giữ nguyên nếu thao tác không hoàn tất.
- Hệ thống không ghi nhận hoặc thông báo xóa thành công.
- Hệ thống ghi nhận kết quả xóa thất bại.
- Hệ thống thông báo “Không thể xóa ảnh, vui lòng thử lại”.
- Quản trị viên được phép thử lại.

**EXC-04 — Phiên đăng nhập hết hạn khi xác nhận xóa**
- Phiên đăng nhập hết hạn trước khi Quản trị viên xác nhận xóa.
- Hệ thống từ chối yêu cầu xóa.
- Ảnh và toàn bộ dữ liệu liên quan không bị thay đổi.
- Hệ thống thông báo phiên đăng nhập đã hết hạn.
- Hệ thống yêu cầu Quản trị viên đăng nhập lại.

**EXC-05 — Tình trạng ảnh thay đổi trước khi xóa**
- Cửa sổ xác nhận xóa đang hiển thị.
- Ảnh bắt đầu được dữ liệu khác tham chiếu hoặc quyền của Quản trị viên thay đổi trước khi xác nhận.
- Quản trị viên chọn “Xóa”.
- Hệ thống kiểm tra lại tình trạng hiện tại của ảnh.
- Hệ thống không xóa ảnh không còn đủ điều kiện.
- Hệ thống thông báo tình trạng ảnh đã thay đổi.
- Hệ thống tải lại dữ liệu hiện tại.

## 4. ACCEPTANCE CRITERIA
**AC-001 — Hiển thị xác nhận xóa ảnh**
- **Given:** ảnh còn tồn tại và không được dữ liệu khác tham chiếu.
- **When:** Quản trị viên chọn “Xóa”.
- **Then:** Hệ thống hiển thị cửa sổ xác nhận gồm ảnh thu nhỏ, tên ảnh và cảnh báo thao tác không thể hoàn tác.
- **And:** Ảnh chưa bị xóa trước khi Quản trị viên xác nhận.
- **And:** Hệ thống chưa gửi yêu cầu xóa dữ liệu.

**AC-002 — Hủy xóa ảnh**
- **Given:** Cửa sổ xác nhận xóa đang hiển thị.
- **When:** Quản trị viên chọn “Hủy”.
- **Then:** Hệ thống đóng cửa sổ xác nhận và không gửi yêu cầu xóa.
- **And:** Ảnh và toàn bộ dữ liệu liên quan được giữ nguyên.

**AC-003 — Xóa ảnh thành công**
- **Given:** Quản trị viên bấm nút "Xóa" ảnh
- **When:** Quản trị viên xác nhận xóa.
- **Then:** Hệ thống xóa ảnh gốc, các phiên bản theo tỷ lệ và thông tin ảnh trong một giao dịch thống nhất.
- **And:** Ảnh không còn xuất hiện trong danh sách ảnh có thể sử dụng.
- **And:** Hệ thống chỉ thông báo thành công sau khi toàn bộ thao tác xóa hoàn tất.

**AC-004 — Ảnh không còn tồn tại**
- **Given:** Ảnh đã bị xóa hoặc không còn tồn tại.
- **When:** Quản trị viên mở hoặc xác nhận thao tác xóa.
- **Then:** Hệ thống dừng thao tác và không ghi nhận xóa thành công.
- **And:** Hệ thống thông báo ảnh không còn tồn tại.
- **And:** Hệ thống tải lại danh sách để hiển thị dữ liệu hiện tại.

**AC-005 — Ảnh đang được sử dụng**
- **Given:** Ảnh đang được nội dung, lịch đăng bài.
- **When:** Quản trị viên mở hoặc xác nhận thao tác xóa.
- **Then:** Hệ thống từ chối yêu cầu xóa.
- **And:** Ảnh và toàn bộ dữ liệu liên quan được giữ nguyên.
- **And:** Hệ thống thông báo ảnh đang được sử dụng và hiển thị thông tin tham chiếu cần xử lý.

**AC-006 — Lỗi xóa không làm mất dữ liệu một phần**
- **Given:** Quản trị viên đang xóa một ảnh đủ điều kiện.
- **When:** Quá trình xóa ảnh hoặc dữ liệu liên quan xảy ra lỗi.
- **Then:** Hệ thống không ghi nhận xóa thành công.
- **And:** Không có ảnh gốc, phiên bản theo tỷ lệ hoặc thông tin ảnh nào bị xóa một phần.
- **And:** Dữ liệu hiện tại được giữ nguyên.
- **And:** Hệ thống cho phép Quản trị viên thử lại.

**AC-007 — Kiểm tra lại tình trạng ảnh trước khi xóa**
- **Given:** Cửa sổ xác nhận xóa đang hiển thị.
- **When:** Tình trạng sử dụng trước khi Quản trị viên xác nhận.
- **Then:** Hệ thống kiểm tra lại và không xóa ảnh không còn đủ điều kiện.
- **And:** Hệ thống thông báo tình trạng ảnh đã thay đổi.
- **And:** Hệ thống tải lại dữ liệu hiện tại.

**AC-008 — Phiên đăng nhập hết hạn**
- **Given:** Cửa sổ xác nhận xóa đang hiển thị.
- **When:** Phiên đăng nhập hết hạn và Quản trị viên xác nhận xóa.
- **Then:** Hệ thống từ chối yêu cầu xóa.
- **And:** Ảnh và toàn bộ dữ liệu liên quan không bị thay đổi.
- **And:** Hệ thống yêu cầu Quản trị viên đăng nhập lại.

## 5. REFERENCES
### Rules
- [BR-039](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/590b277d-94d2-4b88-9adb-469ac97b7554)
- [BR-065](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/69f06c2b-30d2-4cca-95bb-624a2f01b648)

### NON-FUNCTIONAL
- Không có

### OUT OF SCOPE
- Không có

### Rule Details

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Story liên quan |
|---|---|---|---|---|---|---|---|
| BR-039 | Ràng buộc xóa Hình ảnh | AI Image | Bảo vệ toàn vẹn dữ liệu cho các bài đăng có đính kèm ảnh. | Khi bấm 'Xóa' hình ảnh trong thư viện. | Hệ thống chặn xóa nếu ảnh đang được Content, lịch đăng tham chiếu. | Không | STORY-022 |
| BR-065 | Xóa toàn vẹn ảnh | AI Image | Xóa ảnh gốc, thì phải xóa luôn phiên bản theo tỷ lệ và thông tin liên quan trong một lần xử lý. | Khi Quản trị viên xác nhận xóa một ảnh đủ điều kiện. | Hệ thống kiểm tra lại tình trạng sử dụng, xóa toàn bộ dữ liệu ảnh. Nếu có lỗi, không thành phần nào bị xóa một phần và dữ liệu cũ được giữ nguyên. | Không | STORY-022 |
