# STORY-013: Lọc System Prompt

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Đã duyệt - Cập nhật: 04/09/2026)

**Author:** Nguyễn Anh Quân
**Reviewer:** Nguyễn Đức Bình
**Approver:** Nguyễn Đức Bình
**Owner:** Nguyễn Anh Quân

**Feedback yêu cầu sửa gần nhất:**
> Story này Won't hả?
> *- Nguyễn Đức Bình · 09:05 03/09/2026*

**Thống kê:** Unit Tests: 0 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn lọc danh sách System Prompt theo các tiêu chí: chức năng áp dụng, trạng thái và thời gian cập nhật
- **Context:** Khi cần kiểm tra một nhóm Prompt cụ thể, việc xem toàn bộ danh sách gây mất thời gian. Vì vậy, Quản trị viên cần chức năng lọc để thu hẹp danh sách theo các thuộc tính đã được hệ thống quản lý
- **Sprint:** S1
- **Priority:** Won't
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Quản trị viên đã đăng nhập vào trang admin

### Trigger
- Admin mở khu vực Bộ lọc trên màn hình danh sách System Prompt

## 3. FLOW
### Main Flow
1. Admin truy cập màn hình danh sách System Prompt
2. Admin mở bộ lọc
3. Hệ thống hiển thị các tiêu chí lọc
4. Admin lựa chọn một hoặc nhiều điều kiện lọc
5. Admin áp dụng bộ lọc
6. Hệ thống kiểm tra các điều kiện lọc
7. Hệ thống hiển thị danh sách System Prompt phù hợp
8. Hệ thống hiển thị tổng số kết quả sau khi lọc

### Alternative Flow
**ALT-01 — Không có System Prompt phù hợp**
- Hệ thống hiển thị danh sách rỗng
- Hệ thống thông báo: "Không có System Prompt phù hợp với bộ lọc"

## 4. ACCEPTANCE CRITERIA
**AC-001**
- **Given:** Danh sách có các System Prompt thuộc nhiều chức năng khác nhau
- **When:** Admin chọn một chức năng áp dụng và thực hiện lọc
- **Then:** Hệ thống chỉ hiển thị các System Prompt thuộc chức năng đã chọn

**AC-002 - Filter thành công**
- **Given:** Danh sách có các System Prompt thuộc nhiều chức năng, trạng thái hoặc thời gian cập nhật khác nhau
- **When:** Admin chọn một hoặc nhiều điều kiện lọc và thực hiện lọc
- **Then:** Hệ thống hiển thị danh sách System Prompt phù hợp với các điều kiện lọc đã chọn

## 5. NON-FUNCTIONAL
- Chưa có

## 6. OUT OF SCOPE
- Không bao gồm: tìm kiếm, chỉnh sửa, xóa, tạo System Prompt
