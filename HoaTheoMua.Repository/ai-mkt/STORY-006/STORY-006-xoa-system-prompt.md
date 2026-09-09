# STORY-006: Xóa System Prompt

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Đã duyệt - Cập nhật: 04/09/2026)

**Author:** Hồ Hoàng Nam
**Reviewer:** Nguyễn Đức Bình
**Approver:** Nguyễn Đức Bình
**Owner:** Nguyễn Anh Quân

**Feedback yêu cầu sửa gần nhất:**
> Story này Won't hả?
> *- Nguyễn Đức Bình · 09:05 03/09/2026*

**Thống kê:** Unit Tests: 0 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn xóa System Prompt không còn sử dụng để danh sách Prompt được quản lý gọn gàng và tránh sử dụng nhầm các Prompt không còn phù hợp.
- **Context:** Trong trường hợp hệ thống không còn nhu cầu sử dụng một System Prompt do Prompt được thay thế hoặc yêu cầu nghiệp vụ tương ứng không còn tồn tại, Admin có thể thực hiện xóa System Prompt.
- **Sprint:** S1
- **Priority:** Won't
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Quản trị viên đã đăng nhập vào trang admin

### Trigger
- Admin chọn “Xóa” tại một System Prompt

## 3. FLOW
### Main Flow
1. Admin chọn System Prompt muốn xóa
2. Admin nhấn Xóa
3. Hệ thống kiểm tra System Prompt có đang được cấu hình cho chức năng AI sử dụng hay không để xác định System Prompt có được phép xóa
4. Nếu Prompt đáp ứng điều kiện xóa, hệ thống hiển thị pop-up xác nhận
5. Pop-up hiển thị thông tin System Prompt và cảnh báo về thao tác xóa
6. Admin chọn Xác nhận xóa
7. Hệ thống thực hiện xóa System Prompt
8. Hệ thống cập nhật lại danh sách System Prompt
9. Hệ thống thông báo “Xóa System Prompt thành công”

### Alternative Flow
**ALT-01 — Hủy xóa**
- Admin nhấn Xóa
- Hệ thống hiển thị popup xác nhận
- Admin chọn Hủy
- Hệ thống đóng popup và không thay đổi dữ liệu

**ALT-02 — Đóng popup xác nhận**
- Admin chọn Xóa nhưng nhấn nút đóng hoặc click ra ngoài popup nếu UI cho phép
- Hệ thống xem đây là thao tác hủy
- System Prompt được giữ nguyên.

### Exception Flow
**EXC-01 — Lỗi hệ thống khi xóa**
- API/server/database xảy ra lỗi
- Hệ thống không coi thao tác là thành công
- Hệ thống thông báo: "Không thể xóa System Prompt, vui lòng thử lại"

## 4. ACCEPTANCE CRITERIA
**AC-001**
- **Given:** Admin đang xem một System Prompt có thể xóa
- **When:** Admin chọn Xóa
- **Then:** Hệ thống phải hiển thị popup xác nhận trước khi thực hiện xóa

**AC-002**
- **Given:** Popup xác nhận xóa đang được hiển thị
- **When:** Admin chọn Hủy
- **Then:** Hệ thống không xóa System Prompt
- **And:** Dữ liệu được giữ nguyên

**AC-003 - Xóa thành công**
- **Given:** Popup xác nhận xóa System Prompt đang được hiển thị
- **When:** Admin xác nhận xóa System Prompt
- **Then:** Hệ thống xóa System Prompt thành công
- **And:** System Prompt không còn hiển thị trong danh sách quản lý

**AC-004 - Chức năng AI hoạt động sau khi xóa System Prompt**
- **Given:** System Prompt đã bị xóa và không còn được sử dụng bởi chức năng AI
- **When:** Chức năng AI được thực hiện
- **Then:** Hệ thống sử dụng System Prompt đang được cấu hình hiện tại của chức năng AI
