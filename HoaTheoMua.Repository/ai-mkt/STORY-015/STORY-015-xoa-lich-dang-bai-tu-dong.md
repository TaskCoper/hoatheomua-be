# STORY-015: Xóa lịch đăng bài tự động

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Nháp - Cập nhật: 04/09/2026)

**Author:** Hồ Hoàng Nam
**Reviewer:** Nguyễn Đức Bình
**Approver:** Chưa chỉ định
**Owner:** Nguyễn Anh Quân

**Feedback yêu cầu sửa gần nhất:**
> Cần thêm định nghĩa thế nào là trạng thái cho phép xoá. Chưa có AC cho EXC - 02, 03. Nếu Admin xác nhận xóa đúng lúc job chạy, bài có được đăng không?
> *- Nguyễn Đức Bình · 09:43 03/09/2026*

**Thống kê:** Rules: 2 | Unit Tests: 0 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn xóa lịch đăng bài tự động không còn cần thiết để ngăn lịch tiếp tục được thực thi và giữ kế hoạch đăng bài chính xác
- **Context:** Admin có thể cần loại bỏ lịch đăng bài không còn phù hợp để tránh lịch tiếp tục được thực thi. Trước khi xóa, hệ thống phải kiểm tra quyền, trạng thái lịch và yêu cầu Admin xác nhận. Chức năng này chỉ xử lý xóa lịch; không bao gồm xem, tạo, sửa hoặc xóa bài đã đăng trên nền tảng
**Được phép xóa khi:**
  - Lịch còn tồn tại.
  - Lịch đang ở trạng thái “Đã lên lịch”.
  - Cron Job chưa bắt đầu thực hiện đăng bài.
**Không được phép xóa khi:**
  - Lịch không còn tồn tại hoặc đã bị xóa.
  - Lịch đang được hệ thống thực hiện đăng bài.
  - Lịch đã ở trạng thái “Đăng thành công”.
- **Sprint:** S1
- **Priority:** Must
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Admin đã đăng nhập
- Lịch đăng bài tồn tại và ở trạng thái được phép xóa

### Trigger
- Admin chọn “Xóa” tại một lịch đăng bài

## 3. FLOW
### Main Flow: Xóa lịch đăng bài tự động
1. Admin chọn lịch cần xóa
2. Admin nhấn “Xóa”
3. Hệ thống kiểm tra lịch còn tồn tại và trạng thái thiệp đang được phép xóa (quy định trên context).
4. Nếu trạng thái = Đã lên lịch, hệ thống hiển thị popup xác nhận.
5. Admin chọn “Xác nhận xóa”
6. Hệ thống hủy Cron Job tương ứng
7. Sau khi Job được hủy thành công, hệ thống xóa lịch.
8. Hệ thống thông báo “Xóa lịch đăng bài thành công”

### Alternative Flow
**ALT-01 — Admin hủy thao tác**
- Tại bước 5, nếu Admin chọn “Hủy”, hệ thống đóng popup xác nhận
- Lịch đăng bài được giữ nguyên

### Exception Flow
**EXC-01 — Lịch không còn hợp lệ để xóa**
- Tại bước 3, nếu lịch không tồn tại hoặc trạng thái không cho phép xóa, hệ thống dừng thao tác
- Hệ thống thông báo lịch không thể xóa và tải lại dữ liệu hiện tại

**EXC-02 — Không thể xóa lịch**
- Tại bước 6, nếu hệ thống không thể hoàn tất xóa, thao tác không được ghi nhận thành công
- Hệ thống giữ nguyên lịch và thông báo “Không thể xóa lịch đăng bài. Vui lòng thử lại”

## 4. ACCEPTANCE CRITERIA
**AC-001**
- **Given:** Lịch đăng bài tồn tại và đang ở các trạng thái được phép xóa (quy định trên context).
- **When:** Admin chọn “Xóa”
- **Then:** Hệ thống hiển thị popup xác nhận xóa
- **And:** Lịch chưa bị xóa cho tới khi khi Admin bấm xác nhận

**AC-002**
- **Given:** Lịch chưa tới thời điểm đăng
- **When:** Admin chọn “Xác nhận xóa”
- **Then:** Hệ thống hủy job và xóa lịch
- **And:** Lịch biến mất khỏi danh sách.

**AC-003**
- **Given:** Popup xác nhận xóa đang hiển thị
- **When:** Admin chọn “Hủy”
- **Then:** Hệ thống đóng popup
- **And:** Lịch đăng bài được giữ nguyên

**AC-004 — Không thể hoàn tất xóa lịch**
- **Given:** Lịch đăng bài đang ở trạng thái được phép xóa và Admin đã xác nhận xóa.
- **When:** Hệ thống xảy ra lỗi trong quá trình thực hiện xóa lịch.
- **Then:** Hệ thống không ghi nhận thao tác xóa thành công.
- **And:** Lịch đăng bài vẫn được giữ nguyên trong hệ thống.
- **And:** Hệ thống hiển thị thông báo: “Không thể xóa lịch đăng bài. Vui lòng thử lại.”
- **And:** Lịch vẫn hiển thị trong danh sách sau khi dữ liệu được tải lại.

## 5. REFERENCES
### Rules
- [BR-010](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/70d8230c-9045-4024-9853-595928975dfc)
- [BR-011](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/eab55803-cba0-42a1-bf12-6f66349b3055)

### NON-FUNCTIONAL
- Backend phải kiểm tra quyền; thao tác xóa phải nguyên tử và không để lịch tiếp tục thực thi sau khi báo thành công

### OUT OF SCOPE
- Không bao gồm tạo hoặc tự động đăng bài
- Không bao gồm xem danh sách lịch
- Không bao gồm sửa lịch
- Không bao gồm xóa bài đã đăng trên nền tảng bên thứ ba

### Rule Details

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Story liên quan |
|---|---|---|---|---|---|---|---|
| BR-010 | Trạng thái hợp lệ để xóa lịch đăng | Schedule & Auto-Post | Không được phép xóa các lịch đăng đã diễn ra để bảo toàn lịch sử dữ liệu. | Khi thao tác bấm nút 'Xóa' trên một lịch đăng. | Hệ thống chỉ cho phép tiến hành xóa nếu trạng thái hiện tại là 'Đã lên lịch'. | Không | STORY-015 |
| BR-011 | Xóa lệnh Cron Job khi xóa lịch đăng | Schedule & Auto-Post | Việc xóa lịch đăng bài phải hủy bỏ hoàn toàn tác vụ ngầm đã lên lịch trước đó. | Khi Quản trị viên xác nhận xóa một lịch đang ở trạng thái được phép xóa. | Hệ thống phải hủy tác vụ đăng tương ứng thành công trước khi xóa lịch; nếu không thể hoàn tất thì giữ nguyên lịch và không ghi nhận xóa thành công. | Không | STORY-015 |
