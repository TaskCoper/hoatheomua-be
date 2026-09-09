# STORY-005: Sửa System Prompt

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Nháp - Cập nhật: 05/09/2026)

**Author:** Hồ Hoàng Nam
**Reviewer:** Nguyễn Đức Bình
**Approver:** Chưa chỉ định
**Owner:** Nguyễn Anh Quân

**Feedback yêu cầu sửa gần nhất:**
> Làm rõ lại chỗ preview khi thay đổi system prompt, input để AI run ra output cho phần này là gì chưa thấy đề cập Chưa có AC cho EXC-04, 05, 01
> *- Nguyễn Đức Bình · 09:01 03/09/2026*

**Thống kê:** TDDs: 1 | Rules: 4 | Unit Tests: 8 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn chỉnh sửa System Prompt và quản lý phiên bản để có thể điều chỉnh cách AI xử lý các chức năng của hệ thống khi yêu cầu nghiệp vụ thay đổi
- **Context:** System Prompt là thành phần được sử dụng để định hướng cách AI xử lý và trả về kết quả. Khi cần thay đổi nội dung System Prompt, Admin thực hiện chỉnh sửa trên phiên bản hiện tại. Hệ thống cập nhật phiên bản mới từ nội dung đã chỉnh sửa 
Quy định của System prompt:
  - Tối thiểu 1 ký tự sau khi loại bỏ khoảng trắng ở đầu và cuối.
  - Tối đa 20.000 ký tự.
  - Cho phép tiếng Việt, Unicode, xuống dòng và Markdown.
  - Không cho nhập ký tự điều khiển không hiển thị.
  - Không tự động thay đổi khoảng trắng, xuống dòng hoặc nội dung bên trong Prompt.
- **Sprint:** S1
- **Priority:** Must
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Quản trị viên đã đăng nhập vào trang admin

### Trigger
- Admin chọn một System Prompt và nhấn “Chỉnh sửa”

## 3. FLOW
### Main Flow
1. Admin mở chi tiết một System Prompt
2. Admin chọn chức năng Chỉnh sửa
3. Hệ thống hiển thị nội dung phiên bản hiện tại trong biểu mẫu chỉnh sửa
4. Admin chỉnh sửa nội dung System Prompt
5. Hệ thống kiểm tra nội dung hợp lệ theo quy định của System Prompt
6. Admin chọn Lưu
7. Hệ thống kiểm tra nội dung System Prompt không để trống và đáp ứng các quy tắc định dạng được cấu hình
8. Hệ thống tạo phiên bản mới và vẫn lưu phiên bản trước trong lịch sử
9. Phiên bản vừa chỉnh sửa trở thành phiên bản được AI sử dụng hiện tại

### Alternative Flow
**ALT-01 — Hủy chỉnh sửa**
- Admin đã thay đổi nội dung nhưng chọn Hủy Quay lại
- Hệ thống không lưu nội dung vừa chỉnh sửa
- Phiên bản hiện tại được giữ nguyên

**ALT-02 — Không có thay đổi để lưu**
- Admin chọn Lưu khi chưa thay đổi nội dung
- Hệ thống thông báo "Không có thay đổi để lưu" và giữ nguyên màn hình chỉnh sửa

**ALT-03 — Xử lý xác nhận thoát**
- Nếu Quản trị viên chọn “Tiếp tục chỉnh sửa”, hệ thống giữ màn hình và toàn bộ nội dung chưa lưu.
- Nếu Quản trị viên chọn “Thoát không lưu”, hệ thống giữ nguyên phiên bản hiện tại và loại bỏ nội dung chưa lưu.

### Exception Flow
**EXC-01 — Nội dung Prompt không hợp lệ**
- Admin để trống nội dung hoặc nhập nội dung không hợp lệ
- Hệ thống không cho phép lưu
- Hệ thống yêu cầu Admin chỉnh sửa lại

**EXC-02 — Lỗi khi lưu**
- Server, API xảy ra lỗi khi tạo phiên bản mới.
- Hệ thống thông báo: "Không thể lưu, vui lòng thử lại"
- Không tạo phiên bản mới chưa hoàn chỉnh.
- Không chuyển nội dung đang chỉnh sửa thành phiên bản hiện tại.
- Không thay đổi hoặc làm mất phiên bản hiện tại.
- Không ghi nhận cập nhật thành công.
- Giữ nội dung đang nhập để Quản trị viên có thể thử lại.

**EXC-04 — Phiên bản đã thay đổi trong lúc chỉnh sửa**
- Quản trị viên A mở phiên bản hiện tại để chỉnh sửa.
- Quản trị viên B cập nhật System Prompt thành công trước.
- Quản trị viên A lưu nội dung dựa trên phiên bản cũ.
- Hệ thống không ghi đè phiên bản mới nhất.
- Hệ thống thông báo System Prompt đã được người khác cập nhật.
- Quản trị viên A được phép tải lại phiên bản mới nhất.

## 4. ACCEPTANCE CRITERIA
**AC-002**
- **Given:** Admin cập nhật System Prompt thành công
- **When:** Phiên bản mới được tạo
- **Then:** Hệ thống lưu thông tin người chỉnh sửa và thời gian cập nhật

**AC-003**
- **Given:** Admin đang chỉnh sửa System Prompt
- **When:** Nội dung không đáp ứng điều kiện bắt buộc như bỏ trống
- **Then:** Hệ thống không lưu
- **And:** Hiển thị lỗi cho Admin

**AC-005 - Admin chưa bấm lưu mà đã thoát**
- **Given:** Admin đã chỉnh sửa nội dung System Prompt nhưng chưa lưu
- **When:** Admin thoát khỏi màn hình chỉnh sửa
- **Then:** Hệ thống hiển thị thông báo xác nhận thoát và cảnh báo các thay đổi chưa được lưu

**AC-006 - Admin bấm lưu nhưng chưa thay đổi gì prompt**
- **Given:** Admin không thay đổi nội dung System Prompt
- **When:** Admin chọn Lưu
- **Then:** Hệ thống thông báo "Không có thay đổi để lưu" và không tạo phiên bản mới

**AC-008 — Lưu phiên bản mới đầy đủ**
- **Given:** Nội dung chỉnh sửa đáp ứng điều kiện lưu.
- **When:** Quản trị viên xác nhận lưu.
- **Then:** Hệ thống tạo một phiên bản mới.
- **And:** Phiên bản trước được giữ nguyên trong lịch sử.
- **And:** Phiên bản mới trở thành phiên bản hiện tại.
- **And:** Hệ thống lưu người chỉnh sửa và thời gian chỉnh sửa.

**AC-009 — Lỗi lưu giữ nguyên phiên bản hiện tại**
- **Given:** Quản trị viên đang lưu nội dung hợp lệ.
- **When:** Quá trình tạo phiên bản mới xảy ra lỗi.
- **Then:** Hệ thống không ghi nhận cập nhật thành công.
- **And:** Không tồn tại phiên bản mới chưa hoàn chỉnh.
- **And:** Phiên bản hiện tại và lịch sử phiên bản được giữ nguyên.
- **And:** Nội dung đang chỉnh sửa được giữ lại để thử lại.

**AC-010 — Xử lý thoát khi chưa lưu**
- **Given:** Quản trị viên có nội dung chỉnh sửa chưa lưu.
- **When:** Quản trị viên yêu cầu thoát.
- **Then:** Hệ thống yêu cầu xác nhận.
- **And:** Chọn tiếp tục chỉnh sửa phải giữ nguyên nội dung đang nhập.
- **And:** Chọn thoát không lưu phải giữ nguyên phiên bản hiện tại.

**AC-011 — Xử lý xung đột phiên bản khi lưu**
- **Given:** Quản trị viên A đang chỉnh sửa System Prompt dựa trên phiên bản hiện tại. Trong thời gian đó, Quản trị viên B đã cập nhật System Prompt thành công và tạo phiên bản mới hơn.
- **When:** Quản trị viên A chọn Lưu nội dung đang chỉnh sửa.
- **Then:** Hệ thống không lưu nội dung của Quản trị viên A dựa trên phiên bản cũ.
- **And:** Không ghi đè hoặc thay đổi phiên bản mới nhất đã được Quản trị viên B cập nhật.
- **And:** Hệ thống thông báo: “System Prompt đã được người khác cập nhật. Vui lòng tải lại phiên bản mới nhất.”

## 5. REFERENCES
### Rules
- [BR-041](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/7fcca618-4169-41e1-9191-35ad76d245d6)
- [BR-042](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/8e47c852-d79c-4345-946a-e3fd36e9ab53)
- BR-043
- BR-044
- [BR-045](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/aa2f2bab-03a7-4455-a2e5-c638749cb6f4)
- [BR-052](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/0bc478ae-bb1f-4f44-ba13-46f161d807de)

### NON-FUNCTIONAL
- Thao tác mở, lưu System Prompt phản hồi dưới 2 giây trong điều kiện bình thường, hệ thống phải lưu lịch sử phiên bản, người thao tác và thời gian thao tác, không được ghi đè hoặc làm mất phiên bản trước

### OUT OF SCOPE
- Không bao gồm tạo mới hoặc xóa System Prompt, không bao gồm tự động tối ưu Prompt bằng AI

### Rule Details

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Story liên quan |
|---|---|---|---|---|---|---|---|
| BR-041 | Độ dài System Prompt | System Prompt | Đảm bảo câu lệnh điều khiển AI đủ không gian nhưng không quá tải payload. | Khi lưu chỉnh sửa System Prompt. | Kiểm tra chiều dài nội dung Prompt từ 1 đến 20.000 ký tự. | Không | STORY-005 |
| BR-042 | Định dạng text của System Prompt | System Prompt | System Prompt phải giữ nguyên cấu trúc nội dung, ngắt dòng và khoảng trắng khi lưu và hiển thị. | Khi hiển thị và lưu System Prompt. | Hệ thống hỗ trợ parse và lưu trữ ký tự Unicode và chuẩn Markdown. | Không | STORY-005 |
| BR-045 | Xử lý xung đột khi nhiều người sửa Prompt (Optimistic Locking) | System Prompt | Ngăn chặn việc 2 người cùng sửa một Prompt và ghi đè mất dữ liệu của nhau. | Khi bấm 'Lưu' System Prompt. | Nếu System Prompt đã có phiên bản mới hơn kể từ khi Quản trị viên mở màn hình chỉnh sửa, hệ thống không ghi đè dữ liệu mới nhất và yêu cầu tải lại phiên bản hiện tại. | Không | STORY-005 |
| BR-052 | Không tạo phiên bản Prompt khi nội dung không thay đổi | System Prompt | System Prompt chỉ tạo phiên bản mới khi nội dung thực sự thay đổi. | Khi Quản trị viên lưu màn hình chỉnh sửa System Prompt. | Nếu nội dung không thay đổi, hệ thống không tạo phiên bản mới và giữ nguyên phiên bản hiện tại. | Không | STORY-005 |
