# STORY-004: Xem danh sách và chi tiết System Prompt

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Nháp - Cập nhật: 04/09/2026)

**Author:** Hồ Hoàng Nam
**Reviewer:** Nguyễn Đức Bình
**Approver:** Chưa chỉ định
**Owner:** Nguyễn Anh Quân

**Thống kê:** TDDs: 1 | Rules: 1 | Unit Tests: 9 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn xem danh sách và thông tin chi tiết của System Prompt để có thể kiểm tra nội dung, ngày cập nhật của từng Prompt trước khi thực hiện các thao tác quản lý khác
- **Context:** Hệ thống sử dụng các System Prompt để định hướng cách các chức năng AI xử lý từng nghiệp vụ như tạo nội dung, paraphrase nội dung hoặc các tác vụ liên quan. Chức năng này cung cấp màn hình danh sách System Prompt và cho phép Quản trị viên chọn một System Prompt để xem thông tin chi tiết. Story này chỉ phục vụ mục đích xem dữ liệu, không thực hiện tìm kiếm, lọc, chỉnh sửa, khôi phục hoặc xóa System Prompt.
- **Sprint:** S1
- **Priority:** Must
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Quản trị viên đã đăng nhập vào trang admin
- Tài khoản

### Trigger
- Quản trị viên chọn mục "System Prompt" trên menu điều hướng của trang Admin.

## 3. FLOW
### Main Flow
1. Admin chọn mục System Prompt trên menu quản trị
2. Hệ thống tải danh sách System Prompt
3. Hệ thống hiển thị tên hoặc ID và thời gian cập nhật gần nhất của từng System Prompt
4. Hệ thống sắp xếp danh sách theo thời gian cập nhật giảm dần
5. Admin chọn một System Prompt trong danh sách
6. Hệ thống tải nội dung và thời gian cập nhật của System Prompt được chọn
7. Hệ thống chuyển đến màn hình chi tiết
8. Admin xem đầy đủ thông tin của System Prompt

### Alternative Flow
**ALT-01 — Danh sách chưa có System Prompt**
- Tại bước 3 của Main Flow, hệ thống xác định chưa có System Prompt
- Hệ thống hiển thị trạng thái danh sách rỗng
- Hệ thống hiển thị thông báo “Chưa có System Prompt”
- Luồng kết thúc

**ALT-02 — Quay lại danh sách**
- Tại màn hình chi tiết, Admin chọn “Quay lại”
- Hệ thống trở về màn hình danh sách System Prompt
- Hệ thống hiển thị lại danh sách theo thứ tự thời gian cập nhật giảm dần

### Exception Flow
**EXC-01 — Không tải được chi tiết System Prompt**
- Tại bước 7 của Main Flow, hệ thống không thể tải thông tin chi tiết
- Hệ thống không hiển thị dữ liệu chi tiết không đầy đủ
- Hệ thống thông báo “Không thể tải chi tiết System Prompt. Vui lòng thử lại”
- Hệ thống hiển thị nút “Thử lại”
- Khi Admin chọn “Thử lại”, hệ thống thực hiện lại bước 7

**EXC-02 — Không tải được danh sách System Prompt**
- Tại bước 3 của Main Flow, hệ thống không thể tải danh sách
- Hệ thống không hiển thị danh sách không đầy đủ
- Hệ thống thông báo “Không thể tải danh sách System Prompt. Vui lòng thử lại”
- Hệ thống hiển thị nút “Thử lại”
- Khi Admin chọn “Thử lại”, hệ thống thực hiện lại bước 3

**EXC-03 — System Prompt không còn tồn tại**
- Tại bước 7 của Main Flow, System Prompt được chọn không còn tồn tại
- Hệ thống không hiển thị màn hình chi tiết
- Hệ thống thông báo “System Prompt không còn tồn tại”
- Admin chọn “Quay lại danh sách”
- Hệ thống tải lại danh sách System Prompt hiện tại

## 4. ACCEPTANCE CRITERIA
**AC-001**
- **Given:** Admin đã đăng nhập
- **When:** Admin truy cập màn hình System Prompt
- **Then:** Hệ thống hiển thị danh sách System Prompt hiện có
- **And:** Mỗi System Prompt hiển thị tên hoặc ID và thời gian cập nhật gần nhất

**AC-002**
- **Given:** Hệ thống có nhiều System Prompt
- **When:** Admin truy cập màn hình danh sách System Prompt
- **Then:** Danh sách được sắp xếp theo thời gian cập nhật giảm dần, System Prompt có thời gian cập nhật gần nhất được hiển thị trước

**AC-003**
- **Given:** Hệ thống chưa có System Prompt
- **When:** Admin truy cập màn hình System Prompt
- **Then:** Hệ thống hiển thị trạng thái danh sách rỗng
- **And:** Hệ thống hiển thị thông báo “Chưa có System Prompt”

**AC-004**
- **Given:** Danh sách System Prompt đã được hiển thị và có ít nhất một System Prompt
- **When:** Admin chọn một System Prompt trong danh sách
- **Then:** Hệ thống chuyển đến màn hình chi tiết của System Prompt được chọn
- **And:** Hệ thống hiển thị đầy đủ nội dung và thời gian cập nhật gần nhất
- **And:** Nội dung được hiển thị ở chế độ chỉ xem

**AC-005**
- **Given:** Admin đang xem chi tiết một System Prompt
- **When:** Admin chọn “Quay lại”
- **Then:** Hệ thống trở về màn hình danh sách System Prompt
- **And:** Danh sách được hiển thị theo thứ tự thời gian cập nhật giảm dần

**AC-006**
- **Given:** Admin đang mở màn hình danh sách System Prompt
- **When:** Hệ thống không thể tải danh sách System Prompt
- **Then:** Hệ thống hiển thị thông báo không thể tải danh sách
- **And:** Hệ thống hiển thị nút “Thử lại”
- **And:** Hệ thống không hiển thị dữ liệu không đầy đủ

**AC-007**
- **Given:** Admin chọn một System Prompt đang tồn tại
- **When:** Hệ thống không thể tải thông tin chi tiết
- **Then:** Hệ thống hiển thị thông báo không thể tải chi tiết
- **And:** Hệ thống hiển thị nút “Thử lại”
- **And:** Hệ thống không hiển thị dữ liệu chi tiết không đầy đủ

**AC-008**
- **Given:** Hệ thống đang hiển thị lỗi không tải được chi tiết System Prompt
- **When:** Admin chọn “Thử lại”
- **Then:** Hệ thống gửi lại yêu cầu tải chi tiết
- **And:** Nếu tải thành công, hệ thống hiển thị đầy đủ thông tin System Prompt
- **And:** Nếu tiếp tục thất bại, hệ thống giữ trạng thái lỗi và cho phép Admin thử lại

**AC-009**
- **Given:** Admin đã chọn một System Prompt trong danh sách
- **When:** System Prompt không còn tồn tại tại thời điểm tải chi tiết
- **Then:** Hệ thống không hiển thị màn hình chi tiết
- **And:** Hệ thống hiển thị thông báo “System Prompt không còn tồn tại”
- **And:** Hệ thống cho phép Admin quay lại danh sách

## 5. REFERENCES
### Rules
- [BR-040](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/1610ca66-ea96-494a-a1fe-b80ee018ce9a)

### NON-FUNCTIONAL
- Thời gian tải danh sách và chi tiết System Prompt dưới 2 giây trong điều kiện hoạt động bình thường, nội dung Prompt phải hiển thị đầy đủ và không bị thay đổi khi ở chế độ xem

### OUT OF SCOPE
- Không bao gồm chỉnh sửa, khôi phục, tạo mới hoặc xóa System Prompt

### Rule Details

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Story liên quan |
|---|---|---|---|---|---|---|---|
| BR-040 | Sắp xếp danh sách System Prompt | System Prompt | Ưu tiên hiển thị các Prompt vừa được thao tác/cập nhật. | Khi mở danh sách System Prompt. | Sắp xếp giảm dần theo thời gian cập nhật (Updated_At). | Không | STORY-004 |
