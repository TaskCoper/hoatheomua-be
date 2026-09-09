# STORY-012: Tìm kiếm System Prompt

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
- **Story:** Là một Quản trị viên, tôi muốn tìm kiếm System Prompt bằng từ khóa để có thể nhanh chóng tìm được Prompt cần kiểm tra hoặc quản lý mà không phải xem toàn bộ danh sách
- **Context:** Khi số lượng System Prompt trong hệ thống tăng lên → Khi hệ thống có nhiều System Prompt thuộc các chức năng khác nhau, Admin cần có khả năng tìm kiếm nhanh System Prompt cần thao tác. System Prompt được quản lý theo từng chức năng; khi chỉnh sửa, hệ thống tạo phiên bản mới và chỉ sử dụng phiên bản hiện tại, không sử dụng các phiên bản trong lịch sử để xử lý
- **Sprint:** S1
- **Priority:** Won't
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Quản trị viên đã đăng nhập vào trang admin

### Trigger
- Admin nhập nội dung vào ô Tìm kiếm System Prompt

## 3. FLOW
### Main Flow
1. Admin đang ở màn hình danh sách System Prompt
2. Hệ thống hiển thị ô tìm kiếm
3. Admin nhập từ khóa cần tìm
4. Admin thực hiện tìm kiếm
5. Hệ thống tìm các System Prompt có mã hoặc tên phù hợp với từ khóa
6. Hệ thống trả về danh sách kết quả phù hợp
7. Hệ thống hiển thị tổng số kết quả tìm được
8. Admin xem danh sách kết quả

### Alternative Flow
**ALT-01 — Không có kết quả phù hợp**
- Hệ thống trả về danh sách rỗng
- Hệ thống thông báo: "Không tìm thấy System Prompt phù hợp"

**ALT-02 — Xóa từ khóa tìm kiếm**
- Admin xóa toàn bộ từ khóa tìm kiếm
- Hiển thị lại toàn bộ danh sách System Prompt

## 4. ACCEPTANCE CRITERIA
**AC-001**
- **Given:** Admin nhập một từ khóa không khớp với bất kỳ System Prompt nào
- **When:** Hệ thống hoàn tất tìm kiếm
- **Then:** Hệ thống hiển thị danh sách rỗng
- **And:** Thông báo “Không tìm thấy System Prompt phù hợp”

**AC-002**
- **Given:** Hệ thống tồn tại System Prompt có mã phù hợp
- **When:** Admin nhập mã hoặc một phần mã vào ô tìm kiếm
- **Then:** Hệ thống trả về System Prompt phù hợp

**AC-003 - Tìm kiếm thành công**
- **Given:** Admin nhập từ khóa tìm kiếm phù hợp với một hoặc nhiều System Prompt
- **When:** Admin thực hiện tìm kiếm
- **Then:** Hệ thống hiển thị danh sách System Prompt phù hợp với từ khóa tìm kiếm

## 5. NON-FUNCTIONAL
- Chưa có

## 6. OUT OF SCOPE
- Không bao gồm: lọc, chỉnh sửa, xóa, tạo System Prompt
