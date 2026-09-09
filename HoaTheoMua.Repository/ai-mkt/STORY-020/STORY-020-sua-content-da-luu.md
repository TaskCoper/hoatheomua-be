# STORY-020: Sửa content đã lưu lại

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Nháp - Cập nhật: 05/09/2026)

**Author:** Nguyễn Anh Quân
**Reviewer:** Nguyễn Đức Bình
**Approver:** Chưa chỉ định
**Owner:** Nguyễn Anh Quân

**Feedback yêu cầu sửa gần nhất:**
> Hashtag không bắt buộc - mâu thuẫn STORY-017 Giới hạn 1–10.000 ký tự mâu thuẫn STORY-017
> *- Nguyễn Đức Bình · 10:01 03/09/2026*

**Thống kê:** Rules: 5 | Unit Tests: 0 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn chỉnh sửa content và hashtag đã lưu để cập nhật nội dung phù hợp trước khi sử dụng
- **Context:** Content đã lưu có thể cần điều chỉnh khi mục tiêu, nền tảng hoặc yêu cầu truyền thông thay đổi. Hệ thống phải hiển thị dữ liệu hiện tại, cho phép Admin cập nhật các trường được phép và kiểm tra dữ liệu trước khi lưu. Chức năng này chỉ bao gồm sửa content đã lưu, không bao gồm tạo mới, xem, xóa, lên lịch hoặc đăng bài
  - Content chứa: 1–10.000 ký tự sau khi loại bỏ khoảng trắng đầu/cuối kể cả hashtag.
  - Hashtag: tạo tối thiểu 1, tối đa 30 hashtag.
  - Chỉ xem: tiêu đề, nền tảng, nguồn tạo, người tạo và thời gian tạo. Hệ thống cập nhật trực tiếp content hiện tại và lưu người cập nhật, thời gian cập nhật. Hệ thống không lưu lại nội dung trước khi chỉnh sửa.
- **Sprint:** S2
- **Priority:** Must
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Admin đã đăng nhập
- Trạng thái hiện tại của content cho phép chỉnh sửa

### Trigger
- Admin chọn “Sửa” tại một content đã lưu

## 3. FLOW
### Main Flow: Sửa content đã lưu
1. Admin chọn content cần sửa
2. Hệ thống kiểm tra quyền của Admin, content còn tồn tại và trạng thái hiện tại cho phép chỉnh sửa.
3. Hệ thống tải nội dung, danh sách hashtag và thông tin phiên bản hiện tại của content.
4. Admin nhấn “Sửa”
5. Hệ thống hiển thị màn hình chỉnh sửa và điền sẵn dữ liệu hiện tại của content
6. Admin thay đổi các trường được phép chỉnh sửa, bao gồm: nội dung content, hashtag
7. Admin chọn “Lưu”
8. Hệ thống kiểm tra tính hợp lệ của dữ liệu Admin nhập.
9. Hệ thống kiểm tra lại quyền, trạng thái và phiên bản hiện tại của content tại thời điểm lưu.
10. Hệ thống cập nhật content và danh sách hashtag trong một giao dịch thống nhất.
11. Hệ thống thông báo “Cập nhật content thành công”.
12. Hệ thống hiển thị dữ liệu mới nhất của content.

### Alternative Flow
**ALT-01 — Admin hủy chỉnh sửa**
- Trong khi màn hình chỉnh sửa đang hiển thị, Admin chọn “Hủy”.
- Hệ thống không gửi yêu cầu cập nhật content.
- Hệ thống đóng màn hình chỉnh sửa.
- Content và danh sách hashtag hiện tại được giữ nguyên.

**ALT-02 — Chỉ sửa một phần thông tin**
- Admin chỉ thay đổi content hoặc danh sách hashtag.
- Admin chọn “Lưu”.
- Hệ thống chỉ cập nhật thông tin đã thay đổi.
- Các thông tin còn lại được giữ nguyên.
- Luồng tiếp tục từ bước 7 của Main Flow.

**ALT-03 — Không có thay đổi để lưu**
- Admin mở màn hình chỉnh sửa nhưng không thay đổi dữ liệu.
- Admin chọn “Lưu”.
- Hệ thống xác định không có thay đổi.
- Hệ thống không thực hiện cập nhật.
- Hệ thống thông báo “Không có thay đổi để lưu”.
- Content và danh sách hashtag hiện tại được giữ nguyên.

**ALT-04 — Thoát khi có thay đổi chưa lưu**
- Admin đã thay đổi dữ liệu nhưng chưa lưu.
- Admin đóng màn hình hoặc yêu cầu quay lại.
- Hệ thống cảnh báo có thay đổi chưa được lưu.
- Nếu Admin chọn “Tiếp tục chỉnh sửa”, hệ thống giữ màn hình và toàn bộ dữ liệu đang nhập.
- Nếu Admin chọn “Thoát không lưu”, hệ thống giữ nguyên content hiện tại và loại bỏ dữ liệu chưa lưu.

### Exception Flow
**EXC-01 — Content không còn khả dụng để sửa**
- Content đã bị xóa hoặc chuyển sang trạng thái không được phép chỉnh sửa.
- Hệ thống không hiển thị biểu mẫu chỉnh sửa hoặc không thực hiện lưu.
- Content và hashtag không bị thay đổi.
- Hệ thống thông báo content không còn khả dụng để sửa.
- Admin được phép quay lại danh sách hoặc tải lại dữ liệu hiện tại.

**EXC-02 — Dữ liệu không hợp lệ**
- Tại bước 7, nếu dữ liệu Admin nhập không đáp ứng các quy tắc kiểm tra của trường dữ liệu(quy định ở context)
- Hệ thống không lưu các thay đổi.
- Hệ thống hiển thị thông báo xác định trường dữ liệu không hợp lệ để Admin kiểm tra và chỉnh sửa.
- Các trường hợp không hợp lệ bao gồm: Nội dung content không đáp ứng điều kiện bắt buộc của hệ thống, ví dụ để trống khi trường này bắt buộc nhập. Hashtag không đúng định dạng hoặc không đáp ứng quy tắc nhập liệu của hệ thống. Giá trị nhập vượt quá giới hạn được hệ thống cho phép.

**EXC-03 — Không thể cập nhật content**
- Quá trình cập nhật content hoặc hashtag xảy ra lỗi.
- Hệ thống xử lý content và danh sách hashtag trong một giao dịch thống nhất.
- Hệ thống không lưu dữ liệu một phần.
- Content và hashtag hiện tại được giữ nguyên.
- Hệ thống không ghi nhận hoặc thông báo cập nhật thành công.
- Hệ thống giữ dữ liệu Admin đang nhập.
- Hệ thống thông báo “Không thể cập nhật content, vui lòng thử lại”.
- Admin được phép thử lại.
- Việc thử lại không tạo hashtag hoặc dữ liệu liên kết trùng lặp.

**EXC-04 — Content đã thay đổi trong lúc chỉnh sửa**
- Admin A mở content để chỉnh sửa.
- Admin B cập nhật content thành công trước.
- Admin A lưu dữ liệu dựa trên phiên bản cũ.
- Hệ thống kiểm tra phiên bản tại thời điểm lưu.
- Hệ thống không ghi đè phiên bản mới nhất.
- Hệ thống thông báo content đã được người khác cập nhật.
- Hệ thống giữ dữ liệu Admin A đang nhập để Admin có thể đối chiếu.
- Admin A phải tải lại phiên bản mới nhất trước khi thực hiện lưu lại.

**EXC-05 — Phiên đăng nhập hết hạn**
- Phiên đăng nhập hết hạn trước khi Admin lưu.
- Hệ thống từ chối yêu cầu cập nhật.
- Content và hashtag không bị thay đổi.
- Hệ thống thông báo phiên đăng nhập đã hết hạn.
- Hệ thống yêu cầu Admin đăng nhập lại.

**EXC-06 — Không thể tải dữ liệu hiện tại**
- Hệ thống không thể tải đầy đủ content, hashtag hoặc thông tin phiên bản hiện tại.
- Hệ thống không hiển thị biểu mẫu chứa dữ liệu thiếu.
- Hệ thống thông báo “Không thể tải thông tin content”.
- Admin được phép tải lại hoặc quay về màn hình trước.

## 4. ACCEPTANCE CRITERIA
**AC-001 — Hiển thị dữ liệu chỉnh sửa**
- **Given:** .
- **When:** Admin chọn “Sửa”.
- **Then:** Hệ thống hiển thị nội dung content và danh sách hashtag hiện tại.
- **And:** Dữ liệu chưa bị thay đổi trước khi Admin chọn “Lưu”.

**AC-002 — Cập nhật content thành công**
- **Given:** Admin đã nhập dữ liệu hợp lệ và content chưa bị thay đổi bởi người khác.
- **When:** Admin chọn “Lưu”.
- **Then:** Hệ thống cập nhật content và danh sách hashtag trong một giao dịch thống nhất.
- **And:** Chỉ các thông tin đã thay đổi được cập nhật.
- **And:** Các thông tin còn lại được giữ nguyên.
- **And:** Hệ thống chỉ thông báo thành công sau khi toàn bộ dữ liệu được lưu.

**AC-003 — Hủy chỉnh sửa**
- **Given:** Màn hình chỉnh sửa đang hiển thị.
- **When:** Admin chọn “Hủy”.
- **Then:** Hệ thống không gửi yêu cầu cập nhật.
- **And:** Content và hashtag hiện tại được giữ nguyên.

**AC-004 — Dữ liệu không hợp lệ**
- **Given:** Admin đang chỉnh sửa content.
- **When:** Admin lưu dữ liệu không hợp lệ.
- **Then:** Hệ thống không cập nhật content hoặc hashtag.
- **And:** Hệ thống hiển thị lỗi tại trường tương ứng.
- **And:** Dữ liệu Admin đang nhập được giữ lại để chỉnh sửa.

**AC-005 — Không có thay đổi để lưu**
- **Given:** Admin chưa thay đổi content hoặc hashtag.
- **When:** Admin chọn “Lưu”.
- **Then:** Hệ thống không thực hiện cập nhật.
- **And:** Content và hashtag hiện tại được giữ nguyên.
- **And:** Hệ thống thông báo “Không có thay đổi để lưu”.

**AC-006 — Xử lý thoát khi chưa lưu**
- **Given:** Admin có thay đổi chưa lưu.
- **When:** Admin yêu cầu đóng hoặc rời màn hình.
- **Then:** Hệ thống yêu cầu xác nhận.
- **And:** Chọn tiếp tục chỉnh sửa phải giữ nguyên dữ liệu đang nhập.
- **And:** Chọn thoát không lưu phải giữ nguyên content hiện tại.

**AC-007 — Lỗi cập nhật không tạo dữ liệu một phần**
- **Given:** Admin đang lưu dữ liệu hợp lệ.
- **When:** Quá trình cập nhật content hoặc hashtag xảy ra lỗi.
- **Then:** Hệ thống không ghi nhận cập nhật thành công.
- **And:** Không tồn tại content, hashtag hoặc dữ liệu liên kết được cập nhật một phần.
- **And:** Dữ liệu hiện tại được giữ nguyên.
- **And:** Dữ liệu Admin đang nhập được giữ lại để thử lại.

**AC-008 — Ngăn ghi đè khi content đã thay đổi**
- **Given:** Admin đang chỉnh sửa dựa trên một phiên bản content cũ.
- **When:** Content đã được người khác cập nhật trước khi Admin lưu.
- **Then:** Hệ thống không ghi đè phiên bản mới nhất.
- **And:** Hệ thống thông báo content đã thay đổi.
- **And:** Admin phải tải lại dữ liệu mới nhất trước khi lưu lại.

**AC-009 — Content không còn được phép sửa**
- **Given:** Content đã bị xóa hoặc chuyển sang trạng thái không được phép chỉnh sửa.
- **When:** Admin mở hoặc lưu màn hình chỉnh sửa.
- **Then:** Hệ thống từ chối cập nhật.
- **And:** Content và hashtag không bị thay đổi.
- **And:** Hệ thống cho phép Admin quay lại danh sách hoặc tải lại trạng thái hiện tại.

**AC-010 — Phiên đăng nhập hết hạn**
- **Given:** Admin đang chỉnh sửa content.
- **When:** Phiên đăng nhập hết hạn và Admin chọn “Lưu”.
- **Then:** Hệ thống từ chối cập nhật.
- **And:** Content và hashtag không bị thay đổi.
- **And:** Hệ thống yêu cầu Admin đăng nhập lại.

**AC-011 — Không thể tải dữ liệu chỉnh sửa**
- **Given:** .
- **When:** Hệ thống không tải được đầy đủ content, hashtag hoặc thông tin phiên bản.
- **Then:** Hệ thống không hiển thị biểu mẫu chứa dữ liệu thiếu.
- **And:** Hệ thống thông báo không thể tải thông tin content.
- **And:** Admin được phép tải lại hoặc quay về màn hình trước.

## 5. REFERENCES
### Rules
- [BR-025](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/421d8701-2950-41f8-908f-4a6838c5b7cb)
- [BR-026](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/900ead81-d7af-4e4e-87a8-b093e883833d)
- [BR-027](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/d1720b9d-f09b-4541-9c1e-d305c6fb22a8)
- [BR-061](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/94d6e933-928d-47fd-a918-f8e0f9f0935b)
- [BR-062](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/9376331f-36e6-4f4d-9ff4-2b6514d88fa8)

### NON-FUNCTIONAL
- Backend phải kiểm tra quyền và phiên bản hiện tại của content trước khi cập nhật
- Content, hashtag và thời gian cập nhật phải được lưu trong cùng một giao dịch, nếu lỗi phải giữ nguyên dữ liệu cũ
- 95% yêu cầu phải trả kết quả cập nhật trong tối đa 2 giây sau khi Admin lưu
- Nhật ký phải lưu người sửa, thời gian, phiên bản và các trường đã thay đổi

### OUT OF SCOPE
- Không bao gồm tạo content mới
- Không bao gồm xem danh sách hoặc chi tiết content
- Không bao gồm xóa content đã lưu
- Không bao gồm lên lịch hoặc đăng content
- Không bao gồm lưu, xem hoặc khôi phục lịch sử phiên bản của content.
- Hệ thống chỉ hiển thị nội dung hiện tại của content. Chức năng không bao gồm xem lịch sử phiên bản hoặc khôi phục nội dung cũ.

### Rule Details

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Story liên quan |
|---|---|---|---|---|---|---|---|
| BR-025 | Giới hạn độ dài nội dung Content | AI Content | Nội dung Content phải đáp ứng giới hạn độ dài được quy định trước khi lưu. | Khi lưu nội dung Content (đã bao gồm cả text và hashtag). | Hệ thống kiểm tra tổng độ dài nội dung phải nằm trong khoảng từ 1 đến 10.000 ký tự. | Không | STORY-020 |
| BR-026 | Đồng bộ giao dịch khi sửa Content | AI Content | Việc cập nhật nội dung bài viết và hashtag phải diễn ra trọn vẹn hoặc thất bại toàn bộ. | Khi bấm lưu chỉnh sửa Content. | Hệ thống chỉ ghi nhận thành công khi nội dung và danh sách hashtag đều được cập nhật đầy đủ; nếu có lỗi phải giữ nguyên toàn bộ dữ liệu cũ. | Không | STORY-020 |
| BR-027 | Không đọc được rule. | | | | | | |
| BR-061 | Phạm vi chỉnh sửa Content | AI Content | Chức năng chỉnh sửa Content chỉ cho phép thay đổi nội dung và danh sách hashtag hiện tại. | Khi Quản trị viên mở và lưu màn hình chỉnh sửa Content. | Hệ thống cập nhật trực tiếp Content hiện tại, lưu người cập nhật và thời gian cập nhật. Hệ thống không lưu, hiển thị hoặc khôi phục nội dung trước khi chỉnh sửa. | Không | STORY-020 |
| BR-062 | Không ghi đè Content đã thay đổi trong lúc sửa | AI Content | Không được ghi đè Content nếu dữ liệu đã thay đổi kể từ khi bắt đầu chỉnh sửa. | Khi Quản trị viên lưu Content dựa trên dữ liệu đã mở trước đó. | Hệ thống kiểm tra dữ liệu hiện tại; nếu có thay đổi mới hơn thì từ chối lưu và yêu cầu tải lại dữ liệu mới nhất. | Không | STORY-020 |
