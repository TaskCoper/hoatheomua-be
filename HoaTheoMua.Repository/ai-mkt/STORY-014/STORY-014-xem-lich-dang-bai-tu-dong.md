# STORY-014: Xem lịch đăng bài tự động

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Nháp - Cập nhật: 04/09/2026)

**Author:** Hồ Hoàng Nam
**Reviewer:** Nguyễn Đức Bình
**Approver:** Chưa chỉ định
**Owner:** Nguyễn Anh Quân

**Feedback yêu cầu sửa gần nhất:**
> Kiểm tra lại AC - 005, có thực sự là phân loại theo timezone không? Chưa có AC cho EXC - 04
> *- Nguyễn Đức Bình · 09:08 03/09/2026*

**Thống kê:** Rules: 4 | Unit Tests: 0 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn xem danh sách lịch đăng bài để theo dõi các lịch đăng bài đã lên của mình.
- **Context:** Sau khi lịch đăng được tạo, Quản trị viên cần màn hình tổng hợp để theo dõi kế hoạch đăng bài. Hệ thống hiển thị lịch theo ngày. Story này chỉ bao gồm xem danh sách; không bao gồm tạo, tìm kiếm, lọc, xem chi tiết, sửa hoặc xóa lịch.
- **Sprint:** S1
- **Priority:** Must
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Admin đã đăng nhập vào trang quản trị.

### Trigger
- Admin chọn mục “Lịch đăng bài” trên menu quản trị.

## 3. FLOW
### Main Flow: Xem lịch đăng bài tự động
1. Admin mở màn hình Lịch đăng bài.
2. Hệ thống hiển thị lịch đăng bài theo dạng bảng, sắp xếp theo thời gian tăng dần theo ngày
3. Admin xem danh sách và tổng số lịch.

### Alternative Flow
**ALT-01 — Không có lịch đăng bài**
- Tại bước 3, nếu không có lịch đăng bài, hệ thống hiển thị danh sách rỗng.
- Hệ thống hiển thị thông báo “Chưa có lịch đăng bài nào”.

**ALT-04 — Nhiều lịch có cùng thời điểm đăng**
- Có nhiều lịch đăng bài có cùng thời điểm dự kiến đăng.
- Hệ thống giữ và hiển thị đầy đủ tất cả các lịch.
- Hệ thống không gộp hoặc loại bỏ lịch trùng thời điểm.
- Hệ thống áp dụng tiêu chí sắp xếp phụ đã được cấu hình để bảo đảm thứ tự hiển thị ổn định.

### Exception Flow
**EXC-01 — Phiên đăng nhập hết hạn**
- Tại bước 2, nếu phiên đăng nhập hết hạn, hệ thống không tải dữ liệu lịch.
- Hệ thống thông báo phiên hết hạn và chuyển Admin đến màn hình đăng nhập.

**EXC-02 — Không thể tải dữ liệu lịch**
- Tại bước 3, nếu không thể tải dữ liệu, hệ thống không hiển thị danh sách chưa đầy đủ.
- Hệ thống thông báo “Không thể tải lịch đăng bài. Vui lòng thử lại” và cho phép tải lại.
- Hệ thống không cập nhật tổng số lịch bằng dữ liệu tải chưa hoàn chỉnh.
- Nếu màn hình đang có dữ liệu từ lần tải thành công trước, hệ thống giữ dữ liệu đó và thông báo dữ liệu chưa được làm mới.
- Hệ thống không tự động tạo, cập nhật hoặc xóa lịch đăng bài.

## 4. ACCEPTANCE CRITERIA
**AC-001**
- **Given:** Admin đã đăng nhập.
- **When:** Admin truy cập màn hình Lịch đăng bài.
- **Then:** Hệ thống hiển thị lịch theo dạng bảng sắp xếp mặc định theo ngày tăng dần

**AC-002**
- **Given:** Hệ thống chưa có lịch đăng bài.
- **When:** Admin truy cập màn hình Lịch đăng bài.
- **Then:** Hệ thống hiển thị trạng thái danh sách rỗng.
- **And:** Thông báo “Chưa có lịch đăng bài nào”.

**AC-003 — Sắp xếp theo thời gian**
- **Given:** Một lịch có thời gian cụ thể
- **When:** Hệ thống hiển thị danh sách
- **Then:** Lịch hiển thị đầy đủ theo thời gian tăng dần

**AC-004 — Sắp xếp và tổng số lịch chính xác**
- **Given:** Hệ thống tải thành công danh sách lịch hợp lệ.
- **When:** Danh sách được hiển thị.
- **Then:** Các lịch được sắp xếp theo thời gian đăng tăng dần.
- **And:** Tổng số lịch bằng số bản ghi hiển thị.
- **And:** Không có lịch bị lặp hoặc bị bỏ sót.

**AC-005 — Không thể tải dữ liệu lịch**
- **Given:** Admin đã đăng nhập.
- **When:** Yêu cầu tải lịch bị lỗi hoặc quá thời gian phản hồi.
- **Then:** Hệ thống không hiển thị danh sách chưa đầy đủ.
- **And:** Hệ thống không cập nhật tổng số lịch bằng dữ liệu chưa hoàn chỉnh.
- **And:** Hệ thống thông báo không thể tải lịch.
- **And:** Admin được phép thử lại.

**AC-006 — Phiên đăng nhập hết hạn**
- **Given:** Phiên đăng nhập của Admin đã hết hạn.
- **When:** Admin truy cập hoặc tải lại màn hình Lịch đăng bài.
- **Then:** Hệ thống không hiển thị dữ liệu lịch.
- **And:** Hệ thống thông báo phiên đăng nhập đã hết hạn.
- **And:** Hệ thống chuyển Admin đến màn hình đăng nhập.

## 5. REFERENCES
### Rules
- [BR-007](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/64397395-3bee-4810-b0fb-36a88065b493)
- [BR-012](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/82bb5dcd-988b-449a-9f8d-0676025e2e25)
- [BR-013](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/ea2b2e70-3b1b-4843-af44-02c91f3c6050)
- [BR-014](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/4792eb80-b0dd-4e70-9238-cee45ac86922)

### Dependencies
- [STORY-002](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/f5539aa1-f2b9-4857-9618-ad3fcb6b3e9e)

### NON-FUNCTIONAL
- Thời gian hiển thị phải dùng thống nhất timezone.
- Không thay đổi dữ liệu khi xem.

### OUT OF SCOPE
- Không bao gồm tạo hoặc tự động đăng bài
- Không bao gồm sửa lịch
- Không bao gồm xóa hoặc hủy lịch
- Không bao gồm tìm kiếm, lọc, xem chi tiết hoặc sắp xếp thủ công.

### Rule Details

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Story liên quan |
|---|---|---|---|---|---|---|---|
| BR-007 | Sắp xếp danh sách lịch đăng bài | Schedule & Auto-Post | Danh sách lịch đăng hiển thị ưu tiên theo dòng thời gian từ gần đến xa. | Khi tải trang danh sách lịch đăng bài tự động. | Hệ thống tự động sort dữ liệu tăng dần theo cột Thời gian đăng (ngày giờ). | Không | STORY-014 |
| BR-012 | Thứ tự ổn định khi nhiều lịch có cùng thời điểm | Schedule & Auto-Post | Các lịch có cùng thời điểm đăng phải được hiển thị đầy đủ. | Khi danh sách có nhiều lịch cùng thời điểm đăng. | Hệ thống không gộp hoặc loại bỏ lịch. Phải hiện ra đầy đủ, FE đề xuất hướng giải quyết | Không | STORY-014 |
| BR-013 | Tổng số lịch phải khớp danh sách hiển thị | Schedule & Auto-Post | Tổng số lịch phải phản ánh đúng số lịch được tải và hiển thị. | Khi hệ thống tải thành công danh sách lịch đăng. | Hệ thống hiển thị đầy đủ danh sách, không lặp hoặc bỏ sót lịch và tổng số bằng số lịch hiển thị. | Không | STORY-014 |
| BR-014 | Không hiển thị danh sách lịch chưa đầy đủ | Schedule & Auto-Post | Danh sách và tổng số lịch chỉ được cập nhật khi dữ liệu tải về đầy đủ. | Khi yêu cầu tải danh sách lịch bị lỗi hoặc quá thời gian phản hồi. | Hệ thống không hiển thị dữ liệu chưa đầy đủ, không cập nhật tổng số và giữ dữ liệu từ lần tải thành công trước nếu có. | Không | STORY-014 |
