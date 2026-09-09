# STORY-016: Sửa lịch đăng bài tự động

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Nháp - Cập nhật: 04/09/2026)

**Author:** Hồ Hoàng Nam
**Reviewer:** Nguyễn Đức Bình
**Approver:** Chưa chỉ định
**Owner:** Nguyễn Anh Quân

**Feedback yêu cầu sửa gần nhất:**
> Context không cho sửa ảnh hả? Chưa liệt kê trạng thái cho phép sửa. US này không có NFR hả? Chưa có AC cho việc lịch trùng.
> *- Nguyễn Đức Bình · 09:47 03/09/2026*

**Thống kê:** Rules: 4 | Unit Tests: 0 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn sửa thông tin của lịch đăng bài tự động để kế hoạch đăng luôn phù hợp với nhu cầu thực tế
- **Context:** Trong quá trình quản lý, thông tin của lịch đăng bài có thể không còn phù hợp. Admin cần cập nhật lịch hiện có mà không phải tạo lại từ đầu. Trước khi lưu, hệ thống phải kiểm tra quyền, trạng thái lịch và tính hợp lệ của dữ liệu. Chức năng này chỉ bao gồm sửa lịch, không bao gồm tạo, xem, xóa hoặc chỉnh sửa bài đã đăng trên nền tảng
**Các trường được phép sửa:**
  - Content đã lưu: chọn một content còn tồn tại.
  - Nền tảng đăng: chọn một hoặc nhiều nền tảng đang kết nối với hệ thống.
  - Ngày đăng: Ngày mới phải đầy đủ và lớn hơn thời điểm hiện tại.
  - Giờ đăng: Giờ mới phải đầy đủ và lớn hơn thời điểm hiện tại.
  - Hình ảnh gắn với lịch đăng bài
**Chỉ được phép sửa khi:**
  - Lịch chưa được đăng ("Đã lên lịch"). 
**Không được sửa khi:**
  - Đang được đăng (Job đang chạy) hoặc đã đăng thành công ("Đăng thành công")
**Validate:**
  - Không được tạo lịch trùng content, nền tảng và thời điểm.
- **Sprint:** S1
- **Priority:** Must
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Admin đã đăng nhập
- Lịch đăng bài còn tồn tại và ở trạng thái được phép sửa

### Trigger
- Admin chọn “Sửa” tại một lịch đăng bài

## 3. FLOW
### Main Flow: Sửa lịch đăng bài tự động
1. Admin chọn lịch cần sửa
2. Admin nhấn “Sửa”
3. Hệ thống kiểm tra trạng thái hiện tại của lịch đã được đăng hay chưa
4. Hệ thống hiển thị màn hình chỉnh sửa và điền sẵn toàn bộ dữ liệu hiện tại của lịch
5. Admin thay đổi một hoặc nhiều trường được phép sửa(đã đề cập trong context)
6. Admin chọn “Lưu”
7. Hệ thống kiểm tra dữ liệu theo:
   - Ngày và giờ mới phải đầy đủ và lớn hơn thời điểm hiện tại.
   - Content phải còn tồn tại, đã được lưu và không để trống.
8. Hệ thống cập nhật lịch và cấu hình thực thi theo dữ liệu mới
9. Hệ thống hiển thị thông báo “Cập nhật lịch đăng bài thành công” và dữ liệu mới

### Alternative Flow
**ALT-01 — Admin hủy chỉnh sửa**
- Trong khi màn hình chỉnh sửa đang hiển thị, Admin chọn “Hủy”.
- Hệ thống đóng màn hình chỉnh sửa.
- Hệ thống không gửi yêu cầu cập nhật lịch.
- Lịch đăng bài và cấu hình thực thi được giữ nguyên.

**ALT-02 — Chỉ sửa một phần thông tin**
- Tại bước 5, nếu Admin chỉ cập nhật một phần thông tin được phép sửa: content, nền tảng hoặc thời gian
- Hệ thống giữ nguyên các thông tin còn lại và tiếp tục bước 6

**ALT-03 — Không có thay đổi để lưu**
- Admin mở màn hình chỉnh sửa nhưng không thay đổi dữ liệu.
- Admin chọn “Lưu”.
- Hệ thống xác định không có thay đổi.
- Hệ thống không cập nhật lịch hoặc cấu hình thực thi.
- Hệ thống thông báo “Không có thay đổi để lưu”.
- Dữ liệu hiện tại được giữ nguyên.

**ALT-04 — Thoát khi có thay đổi chưa lưu**
- Admin đã thay đổi dữ liệu nhưng chưa lưu.
- Admin đóng màn hình hoặc yêu cầu quay lại.
- Hệ thống cảnh báo có thay đổi chưa được lưu.
- Nếu Admin chọn tiếp tục chỉnh sửa, hệ thống giữ màn hình và toàn bộ dữ liệu đang nhập.
- Nếu Admin chọn thoát không lưu, hệ thống giữ nguyên lịch hiện tại và loại bỏ nội dung chưa lưu.

### Exception Flow
**EXC-01 — Lịch không còn hợp lệ để sửa**
- Tại bước 3, nếu lịch không tồn tại hoặc trạng thái không cho phép sửa, hệ thống dừng thao tác
- Hệ thống thông báo lịch không thể sửa và tải lại dữ liệu hiện tại

**EXC-02 — Dữ liệu không hợp lệ**
- Tại bước 7, nếu dữ liệu không hợp lệ (Content không còn tồn tại, Media gắn với content không còn tồn tại, Ngày và giờ mới nhỏ hơn thời điểm hiện tại. Lịch bị trùng content, nền tảng và thời điểm.)
- Hệ thống không cập nhật lịch
- Hệ thống đánh dấu các trường không hợp lệ, hiển thị lý do và giữ nguyên dữ liệu cũ
- Hệ thống không cập nhật lịch hoặc cấu hình thực thi.
- Hệ thống giữ lại dữ liệu Admin đang nhập để chỉnh sửa.
- Hệ thống hiển thị lỗi tại từng trường tương ứng.
- Hệ thống cho phép Admin sửa dữ liệu và thử lưu lại.
- Không ghi nhận cập nhật thành công.

**EXC-03 — Không thể cập nhật lịch**
- Tại bước 8, nếu hệ thống không thể hoàn tất cập nhật, thao tác không được ghi nhận thành công
- Hệ thống giữ nguyên dữ liệu cũ và thông báo “Không thể cập nhật lịch đăng bài. Vui lòng thử lại”
- Không lưu một phần dữ liệu lịch hoặc cấu hình thực thi.
- Không thay đổi dữ liệu hiện tại nếu quá trình cập nhật không hoàn tất.
- Không hiển thị thông báo hoặc ghi nhận cập nhật thành công.
- Hệ thống giữ dữ liệu Admin đang nhập.
- Admin được phép thử lại.

**EXC-04 — Lịch thay đổi trong lúc chỉnh sửa**
- Admin A mở lịch để chỉnh sửa.
- Admin B cập nhật lịch hoặc lịch chuyển sang trạng thái không còn được phép sửa.
- Admin A lưu dữ liệu dựa trên phiên bản cũ.
- Hệ thống kiểm tra lại phiên bản và trạng thái lịch tại thời điểm lưu.
- Hệ thống không ghi đè phiên bản mới nhất hoặc lịch không còn được phép sửa.
- Hệ thống thông báo lịch đã thay đổi.
- Admin A được phép tải lại dữ liệu mới nhất.

**EXC-05 — Phiên đăng nhập hết hạn trong lúc chỉnh sửa**
- Phiên đăng nhập hết hạn trước khi Admin lưu.
- Hệ thống từ chối yêu cầu cập nhật.
- Hệ thống không thay đổi lịch hoặc cấu hình thực thi.
- Hệ thống thông báo phiên đăng nhập đã hết hạn.
- Hệ thống chuyển Admin đến màn hình đăng nhập.

**EXC-06 — Không thể tải dữ liệu hiện tại của lịch**
- Tại bước 4, hệ thống không thể tải đầy đủ dữ liệu hiện tại của lịch.
- Hệ thống không hiển thị biểu mẫu với dữ liệu thiếu.
- Hệ thống thông báo không thể tải thông tin lịch.
- Admin được phép tải lại hoặc quay về màn hình trước.

## 4. ACCEPTANCE CRITERIA
**AC-001**
- **Given:** lịch đang ở trạng thái được phép sửa
- **When:** Admin chọn “Sửa”
- **Then:** Hệ thống hiển thị màn hình chỉnh sửa và điền sẵn dữ liệu hiện tại, và cho phép sửa các trường được quy định
- **And:** Dữ liệu chưa bị thay đổi trước khi Admin lưu

**AC-002**
- **Given:** Màn hình chỉnh sửa đang hiển thị và dữ liệu mới hợp lệ
- **When:** Admin cập nhật thông tin và chọn “Lưu”
- **Then:** Hệ thống cập nhật lịch và cấu hình thực thi theo dữ liệu mới, chỉ các trường được phép sửa
- **And:** Lịch được thực thi theo thông tin đã cập nhật

**AC-003**
- **Given:** Admin đã thay đổi thông tin trên màn hình chỉnh sửa
- **When:** Admin chọn “Hủy”
- **Then:** Hệ thống đóng màn hình chỉnh sửa
- **And:** Lịch đăng bài được giữ nguyên

**AC-004**
- **Given:** Màn hình chỉnh sửa đang hiển thị
- **When:** Admin lưu dữ liệu không hợp lệ
- **Then:** Hệ thống không cập nhật lịch
- **And:** Hệ thống đánh dấu trường không hợp lệ tại từng trường tương ứng và hiển thị lý do

**AC-005**
- **Given:** Màn hình chỉnh sửa đang hiển thị dữ liệu hiện tại.
- **When:** Admin thay đổi một phần thông tin hợp lệ và chọn “Lưu”.
- **Then:** Hệ thống cập nhật các trường đã thay đổi.
- **And:** Các trường còn lại được giữ nguyên.
- **And:** Cấu hình thực thi được đồng bộ với dữ liệu mới.

**AC-006**
- **Given:** Admin chưa thay đổi dữ liệu lịch.
- **When:** Admin chọn “Lưu”.
- **Then:** Hệ thống không gửi hoặc không thực hiện cập nhật.
- **And:** Lịch và cấu hình thực thi được giữ nguyên.
- **And:** Hệ thống thông báo “Không có thay đổi để lưu”.

**AC-007**
- **Given:** Lịch đã bị xóa hoặc chuyển sang trạng thái không được phép sửa.
- **When:** Admin mở hoặc lưu màn hình chỉnh sửa.
- **Then:** Hệ thống từ chối cập nhật.
- **And:** Không dữ liệu lịch hoặc cấu hình thực thi nào bị thay đổi.
- **And:** Hệ thống tải lại trạng thái hiện tại.

**AC-008**
- **Given:** Admin đang lưu dữ liệu hợp lệ.
- **When:** Quá trình cập nhật lịch hoặc cấu hình thực thi xảy ra lỗi.
- **Then:** Hệ thống không ghi nhận cập nhật thành công.
- **And:** Không tồn tại dữ liệu hoặc cấu hình thực thi được cập nhật một phần.
- **And:** Dữ liệu hiện tại được giữ nguyên.
- **And:** Nội dung Admin đang nhập được giữ lại để thử lại.

**AC-009**
- **Given:** Admin đang chỉnh sửa dựa trên một phiên bản lịch cũ.
- **When:** Lịch đã được cập nhật hoặc đổi trạng thái trước khi Admin lưu.
- **Then:** Hệ thống không ghi đè dữ liệu mới nhất.
- **And:** Hệ thống thông báo lịch đã thay đổi.
- **And:** Admin được phép tải lại dữ liệu hiện tại.

**AC-010**
- **Given:** Admin đang chỉnh sửa lịch.
- **When:** Phiên đăng nhập hết hạn và Admin chọn “Lưu”.
- **Then:** Hệ thống từ chối cập nhật.
- **And:** Lịch và cấu hình thực thi không bị thay đổi.
- **And:** Hệ thống yêu cầu Admin đăng nhập lại.

**AC-011**
- **Given:** Admin có thay đổi chưa lưu.
- **When:** Admin yêu cầu đóng hoặc rời màn hình.
- **Then:** Hệ thống yêu cầu xác nhận.
- **And:** Chọn tiếp tục chỉnh sửa phải giữ nguyên dữ liệu đang nhập.
- **And:** Chọn thoát không lưu phải giữ nguyên lịch hiện tại.

**AC-012**
- **Given:** Hệ thống tải dữ liệu chỉnh sửa lịch.
- **When:** Hệ thống không tải được đầy đủ dữ liệu hiện tại.
- **Then:** Hệ thống không hiển thị biểu mẫu chứa dữ liệu thiếu.
- **And:** Hệ thống thông báo không thể tải thông tin lịch.
- **And:** Admin được phép tải lại hoặc quay về màn hình trước.

**AC-013 — Không cho phép cập nhật thành lịch bị trùng**
- **Given:** Đã tồn tại một lịch đăng bài khác có cùng Content, nền tảng và thời điểm đăng. Admin đang chỉnh sửa một lịch đăng bài ở trạng thái được phép sửa.
- **When:** Admin thay đổi dữ liệu khiến lịch đang chỉnh sửa trùng với lịch đã tồn tại và chọn “Lưu”.
- **Then:** Hệ thống hiển thị thông báo rằng đã tồn tại lịch đăng bài có cùng Content, media, nền tảng và thời điểm.
- **And:** Dữ liệu hiện tại của lịch được giữ nguyên.

## 5. REFERENCES
### Rules
- [BR-008](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/c90b51f9-dde9-4ad8-8374-d7a0f1ba3c2e)
- [BR-009](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/13ba9f66-51a7-4cc9-9596-bb5725b8aac1)
- [BR-053](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/9c59b3ec-029f-420a-8eae-643b64d2319d)
- [BR-054](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/fb457dd2-6461-4c3d-8732-fdbd18217696)

### NON-FUNCTIONAL
- Thao tác mở và cập nhật lịch đăng bài phản hồi trong vòng 2 giây trong điều kiện hoạt động bình thường.
- Sau khi cập nhật thành công, Job phải thực thi theo thời gian, Content, Media và nền tảng mới nhất của lịch.
- Hệ thống không được ghi đè dữ liệu mới hơn khi có nhiều Admin cùng chỉnh sửa một lịch.
- Hệ thống phải kiểm tra lại quyền, trạng thái lịch, tính hợp lệ và lịch trùng tại Backend trước khi thực hiện cập nhật.

### OUT OF SCOPE
- Không bao gồm tạo lịch mới
- Không bao gồm xem danh sách hoặc chi tiết lịch
- Không bao gồm xóa lịch
- Không bao gồm chỉnh sửa nội dung gốc hoặc bài đã đăng trên nền tảng

### Rule Details

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Story liên quan |
|---|---|---|---|---|---|---|---|
| BR-008 | Trạng thái hợp lệ để chỉnh sửa lịch đăng | Schedule & Auto-Post | Chỉ lịch đăng chưa được thực thi mới được phép thay đổi nội dung/cấu hình. | Khi thao tác bấm nút 'Sửa' trên một lịch đăng. | Hệ thống kiểm tra trạng thái phải là 'Đã lên lịch'. Chặn sửa nếu trạng thái khác (Đang đăng, Đã đăng, Lỗi). | Không | STORY-016 |
| BR-009 | Cập nhật dữ liệu khi sửa lịch đăng | Schedule & Auto-Post | Khi sửa lịch đăng, toàn bộ thông tin (Nội dung, Ảnh, Thời gian, Nền tảng) đều có thể được làm mới. | Khi lưu lại các thay đổi của lịch đăng. | Hệ thống cập nhật dữ liệu mới và áp dụng lại quy tắc chống trùng lặp (BR-006). | Không | STORY-016 |
| BR-053 | Dữ liệu hợp lệ khi sửa lịch đăng | Schedule & Auto-Post | Dữ liệu sau khi sửa lịch phải tiếp tục đáp ứng các điều kiện tạo lịch hợp lệ. | Khi Quản trị viên lưu thay đổi của lịch đăng. | Hệ thống kiểm tra Content và ảnh còn tồn tại, nền tảng còn kết nối, thời gian mới ở tương lai và lịch mới không trùng Content, nền tảng, thời điểm. Nếu có trường không hợp lệ, hệ thống không cập nhật lịch và giữ dữ liệu Quản trị viên đang nhập để sửa lại. | Không | STORY-016 |
| BR-054 | Không ghi đè lịch đã thay đổi trong lúc sửa | Schedule & Auto-Post | Không được ghi đè lịch nếu lịch đã thay đổi hoặc không còn được phép sửa. | Khi Quản trị viên lưu dữ liệu dựa trên thông tin lịch đã mở trước đó. | Hệ thống kiểm tra lại trạng thái và dữ liệu hiện tại; nếu đã thay đổi thì từ chối lưu và yêu cầu tải lại dữ liệu mới nhất. | Không | STORY-016 |
