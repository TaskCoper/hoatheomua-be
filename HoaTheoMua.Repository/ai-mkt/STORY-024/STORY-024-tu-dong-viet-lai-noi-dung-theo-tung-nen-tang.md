# STORY-024: Tự động viết lại nội dung theo từng nền tảng

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Nháp - Cập nhật: 04/09/2026)

**Author:** Hồ Hoàng Nam
**Reviewer:** Nguyễn Đức Bình
**Approver:** Chưa chỉ định
**Owner:** Nguyễn Anh Quân

**Feedback yêu cầu sửa gần nhất:**
> Chưa có AC cho EXC - 06, 02
> *- Nguyễn Đức Bình · 10:16 03/09/2026*

**Thống kê:** Rules: 4 | Unit Tests: 0 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn chọn một nội dung gốc đã lưu và một hoặc nhiều nền tảng được chọn để hệ thống sử dụng trí tuệ nhân tạo viết lại nội dung và đề xuất hashtag phù hợp cho từng nền tảng, giúp tôi tạo nhanh các nội dung mới mà vẫn giữ được thông điệp chính của nội dung gốc.
- **Context:** Một nội dung marketing có thể được sử dụng trên nhiều nền tảng như Facebook, Instagram hoặc Zalo Official Account (Zalo OA). Tuy nhiên, cách diễn đạt, độ dài, cấu trúc và hashtag phù hợp có thể khác nhau giữa các nền tảng. Chức năng này cho phép Quản trị viên chọn một nội dung (content) gốc đã lưu hoặc một content từ bên ngoài không có trong hệ thống, chọn một hoặc nhiều nền tảng được chọn và yêu cầu hệ thống sử dụng trí tuệ nhân tạo (AI) để viết lại nội dung và đề xuất hashtag cho từng nền tảng. Quản trị viên có thể xem, chỉnh sửa và lưu riêng từng kết quả. Mỗi kết quả được lưu thành một content mới có ID riêng. Content mới sao chép tiêu đề từ content gốc, lưu nền tảng được chọn, nội dung, danh sách hashtag và ID của content gốc để duy trì mối liên kết. Content gốc không bị ghi đè hoặc thay đổi. Story này chỉ thực hiện việc tạo và lưu content mới theo nền tảng. Việc lên lịch và đăng bài được xử lý ở chức năng khác.
- **Sprint:** S1
- **Priority:** Must
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Quản trị viên đã đăng nhập vào hệ thống.
- Content gốc đã được lưu và vẫn còn tồn tại trong hệ thống.
- Hệ thống đã cấu hình ít nhất một nền tảng được hỗ trợ.

### Trigger
- Quản trị viên mở một content đã lưu và chọn chức năng “Viết lại theo nền tảng”.

## 3. FLOW
### Main Flow
1. Quản trị viên chọn content gốc đã lưu, hoặc content gốc từ bên ngoài hệ thống được admin đưa vào. Sau đó admin chọn “Viết lại theo nền tảng”
2. Hệ thống tải và hiển thị tiêu đề, nội dung, danh sách hashtag của content gốc cùng danh sách nền tảng được hỗ trợ
3. Quản trị viên kiểm tra content gốc và chọn một hoặc nhiều nền tảng
4. Quản trị viên chọn “Tạo nội dung”
5. Hệ thống gửi một yêu cầu riêng đến chức năng AI cho từng nền tảng đã chọn, sử dụng nội dung, hashtag của content gốc và quy tắc của nền tảng được chọn
6. Chức năng AI trả về nội dung viết lại và danh sách hashtag đề xuất cho từng nền tảng
7. Hệ thống kiểm tra từng kết quả và hiển thị riêng kết quả cùng trạng thái xử lý của từng nền tảng
8. Quản trị viên xem và có thể chỉnh sửa nội dung hoặc danh sách hashtag của từng kết quả
9. Quản trị viên chọn “Lưu” tại kết quả muốn lưu
10. Hệ thống kiểm tra lại content gốc và tính hợp lệ của kết quả tại thời điểm lưu
11. Hệ thống tạo một content mới có ID riêng, sao chép tiêu đề của content gốc và lưu nền tảng được chọn, nội dung, danh sách hashtag được đề xuất tùy theo nền tảng.
12. Hệ thống ghi nhận người tạo và thời gian tạo content mới
13. Hệ thống thông báo lưu thành công; content gốc và các kết quả chưa lưu được giữ nguyên

### Alternative Flow
**ALT-01 — Viết lại nội dung cho một nền tảng**
- Tại bước 3 của Main Flow, Quản trị viên chọn một nền tảng
- Hệ thống chỉ gửi một yêu cầu đến chức năng AI cho nền tảng đã chọn
- Hệ thống hiển thị một kết quả để Quản trị viên xem, chỉnh sửa và lưu
- Luồng tiếp tục từ bước 9 của Main Flow

**ALT-02 — Viết lại nội dung cho nhiều nền tảng**
- Tại bước 3 của Main Flow, Quản trị viên chọn từ hai nền tảng trở lên
- Hệ thống gửi và xử lý yêu cầu riêng cho từng nền tảng
- Hệ thống hiển thị từng kết quả trong khu vực riêng và ghi rõ nền tảng tương ứng
- Quản trị viên có thể chỉnh sửa và lưu từng kết quả theo bất kỳ thứ tự nào
- Mỗi kết quả được lưu thành một content mới có ID và nền tảng được chọn riêng
- Kết quả chưa lưu và content gốc không bị thay đổi khi một kết quả khác được lưu

**ALT-03 — Tạo lại nội dung cho một nền tảng**
- Sau khi xem kết quả, Quản trị viên chọn “Tạo lại” tại một nền tảng
- Hệ thống chỉ gửi lại yêu cầu cho nền tảng đó
- Kết quả chưa lưu trước đó của nền tảng được thay bằng kết quả mới sau khi AI xử lý thành công
- Kết quả của các nền tảng khác và content gốc được giữ nguyên
- Quản trị viên tiếp tục xem, chỉnh sửa hoặc lưu kết quả mới

**ALT-04 — Thoát khi có kết quả chưa lưu**
- Quản trị viên yêu cầu đóng hoặc rời màn hình khi còn kết quả chưa lưu
- Hệ thống cảnh báo có nội dung chưa được lưu
- Nếu Quản trị viên chọn “Tiếp tục chỉnh sửa”, hệ thống giữ màn hình và toàn bộ kết quả đang hiển thị
- Nếu Quản trị viên chọn “Thoát không lưu”, hệ thống loại bỏ các kết quả chưa lưu
- Content gốc và các content đã lưu trước đó được giữ nguyên

### Exception Flow
**EXC-01 — Content gốc không còn tồn tại**
- Tại bước 2, bước 5 hoặc bước 11 của Main Flow, hệ thống xác định content gốc đã bị xóa hoặc không còn tồn tại
- Hệ thống không gửi yêu cầu đến chức năng AI hoặc không lưu content mới
- Hệ thống thông báo “Content gốc không còn tồn tại”
- Quản trị viên được phép quay lại danh sách và chọn content khác

**EXC-02 — Dữ liệu đầu vào không hợp lệ**
- Tại bước 5 của Main Flow, Quản trị viên chưa chọn nền tảng
- Hệ thống không gửi yêu cầu đến chức năng AI
- Hệ thống hiển thị lỗi tại thông tin chưa hợp lệ. Thiếu nền tảng, nền tảng là bắt buộc
- Quản trị viên bổ sung thông tin và thử lại

**EXC-03 — Không thể tạo bất kỳ kết quả nào**
- Tại bước 6 hoặc bước 7 của Main Flow, chức năng AI xảy ra lỗi hoặc không phản hồi trong thời gian cho phép đối với tất cả nền tảng đã chọn
- Hệ thống thông báo không thể tạo nội dung
- Hệ thống không tạo hoặc lưu content mới
- Content gốc không bị thay đổi
- Quản trị viên được phép thử lại

**EXC-04 — Một phần nền tảng tạo nội dung thất bại**
- Tại bước 6 hoặc bước 7 của Main Flow, một hoặc nhiều nền tảng xảy ra lỗi nhưng vẫn có nền tảng tạo kết quả thành công
- Hệ thống hiển thị kết quả thành công và trạng thái lỗi theo từng nền tảng
- Quản trị viên được phép xem, chỉnh sửa và lưu các kết quả thành công
- Quản trị viên được phép thử lại riêng các nền tảng bị lỗi
- Việc thử lại không làm mất hoặc tạo trùng các kết quả đã lưu

**EXC-05 — Kết quả do AI tạo không hợp lệ**
- Tại bước 8 của Main Flow, một kết quả bị thiếu nội dung hoặc không đáp ứng quy tắc của nền tảng được chọn
- Hệ thống đánh dấu kết quả đó là không hợp lệ và không cho phép lưu
- Các kết quả hợp lệ khác vẫn được giữ nguyên
- Quản trị viên được phép tạo lại nội dung cho nền tảng có kết quả không hợp lệ

**EXC-06 — Không thể lưu content mới**
- Tại bước 12 hoặc bước 13 của Main Flow, quá trình tạo content mới xảy ra lỗi
- Hệ thống không lưu content chưa hoàn chỉnh
- Hệ thống giữ kết quả Quản trị viên đang chỉnh sửa và thông báo không thể lưu content
- Quản trị viên được phép thử lại

**EXC-07 — Content gốc đã thay đổi trong lúc xử lý**
- Quản trị viên tạo hoặc chỉnh sửa kết quả dựa trên một phiên bản content gốc cũ
- Content gốc được người khác cập nhật trước khi Quản trị viên lưu kết quả
- Tại bước 11 của Main Flow, hệ thống phát hiện content gốc đã thay đổi
- Hệ thống không lưu kết quả dựa trên phiên bản cũ
- Hệ thống thông báo content gốc đã được cập nhật
- Hệ thống giữ kết quả đang chỉnh sửa để Quản trị viên đối chiếu
- Quản trị viên phải tải lại content gốc và tạo lại nội dung trước khi lưu

**EXC-08 — Phiên đăng nhập hết hạn**
- Phiên đăng nhập hết hạn trước khi Quản trị viên tạo hoặc lưu content mới
- Hệ thống từ chối yêu cầu
- Content gốc và các content đã lưu không bị thay đổi
- Hệ thống thông báo phiên đăng nhập đã hết hạn và yêu cầu Quản trị viên đăng nhập lại

**EXC-09 — Không thể tải dữ liệu content gốc**
- Tại bước 2 của Main Flow, hệ thống không thể tải đầy đủ content gốc hoặc danh sách nền tảng được hỗ trợ
- Hệ thống không hiển thị màn hình với dữ liệu thiếu
- Hệ thống thông báo không thể tải thông tin cần thiết
- Quản trị viên được phép tải lại hoặc quay về màn hình trước

## 4. ACCEPTANCE CRITERIA
**AC-001 — Hiển thị dữ liệu content gốc**
- **Given:** content gốc còn tồn tại
- **When:** Quản trị viên chọn “Viết lại theo nền tảng”
- **Then:** Hệ thống hiển thị tiêu đề, nội dung và danh sách hashtag hiện tại của content gốc
- **And:** Hệ thống hiển thị danh sách nền tảng được hỗ trợ
- **And:** Content gốc chưa bị thay đổi

**AC-002 — Yêu cầu chọn nền tảng**
- **Given:** Quản trị viên đã mở content gốc nhưng chưa chọn nền tảng
- **When:** Quản trị viên chọn “Tạo nội dung”
- **Then:** Hệ thống không gửi yêu cầu đến chức năng AI
- **And:** Hệ thống yêu cầu Quản trị viên chọn ít nhất một nền tảng

**AC-003 — Viết lại nội dung cho một nền tảng**
- **Given:** Quản trị viên đã chọn content gốc hợp lệ và một nền tảng
- **When:** Quản trị viên chọn “Tạo nội dung”
- **Then:** Hệ thống tạo và hiển thị nội dung cùng danh sách hashtag đề xuất cho nền tảng đã chọn
- **And:** Kết quả được hiển thị để Quản trị viên kiểm tra trước khi lưu
- **And:** Content gốc không bị thay đổi

**AC-004 — Viết lại nội dung cho nhiều nền tảng**
- **Given:** Quản trị viên đã chọn content gốc hợp lệ và nhiều nền tảng
- **When:** Quản trị viên chọn “Tạo nội dung”
- **Then:** Hệ thống gửi và xử lý yêu cầu riêng cho từng nền tảng
- **And:** Mỗi kết quả được hiển thị riêng và ghi rõ nền tảng tương ứng
- **And:** Lỗi của một nền tảng không làm mất kết quả của nền tảng khác

**AC-005 — Chỉnh sửa kết quả trước khi lưu**
- **Given:** Hệ thống đã tạo một kết quả hợp lệ
- **When:** Quản trị viên chỉnh sửa nội dung hoặc danh sách hashtag
- **Then:** Hệ thống giữ nội dung và hashtag đã chỉnh sửa để Quản trị viên tiếp tục kiểm tra hoặc lưu
- **And:** Việc chỉnh sửa kết quả không làm thay đổi content gốc

**AC-006 — Lưu content mới**
- **Given:** Quản trị viên có một kết quả hợp lệ và content gốc chưa thay đổi
- **When:** Quản trị viên chọn “Lưu”
- **Then:** Hệ thống tạo một content mới có ID riêng
- **And:** Content mới sao chép tiêu đề từ content gốc
- **And:** Content mới lưu nền tảng được chọn, nội dung và danh sách hashtag
- **And:** Người tạo và thời gian tạo được ghi nhận
- **And:** Hệ thống chỉ thông báo thành công sau khi toàn bộ dữ liệu được lưu

**AC-007 — Giữ nguyên content gốc**
- **Given:** Một content mới đã được lưu từ kết quả viết lại
- **When:** Hệ thống hoàn tất thao tác lưu
- **Then:** Content gốc được giữ nguyên
- **And:** Các content khác và các kết quả chưa lưu không bị thay đổi

**AC-008 — Xử lý lỗi một phần**
- **Given:** Quản trị viên yêu cầu tạo nội dung cho nhiều nền tảng
- **When:** Một hoặc nhiều nền tảng xử lý thất bại nhưng vẫn có kết quả thành công
- **Then:** Hệ thống hiển thị trạng thái thành công hoặc thất bại theo từng nền tảng
- **And:** Quản trị viên được phép lưu các kết quả thành công
- **And:** Quản trị viên được phép thử lại riêng các nền tảng bị lỗi

**AC-009 — Lỗi lưu không tạo dữ liệu chưa hoàn chỉnh**
- **Given:** Quản trị viên đang lưu một kết quả hợp lệ
- **When:** Quá trình tạo content mới xảy ra lỗi
- **Then:** Hệ thống không ghi nhận lưu thành công
- **And:** Kết quả đang chỉnh sửa được giữ lại để thử lại

**AC-010 — Content gốc đã thay đổi**
- **Given:** Quản trị viên đang làm việc với kết quả được tạo từ một phiên bản content gốc cũ
- **When:** Content gốc đã được cập nhật trước khi Quản trị viên lưu
- **Then:** Hệ thống không lưu kết quả dựa trên phiên bản cũ
- **And:** Hệ thống thông báo content gốc đã thay đổi
- **And:** Quản trị viên phải tải lại content gốc và tạo lại nội dung trước khi lưu

**AC-011 — Thoát khi có kết quả chưa lưu**
- **Given:** Quản trị viên có kết quả chưa lưu
- **When:** Quản trị viên yêu cầu đóng hoặc rời màn hình
- **Then:** Hệ thống yêu cầu xác nhận
- **And:** Chọn tiếp tục chỉnh sửa phải giữ nguyên các kết quả đang hiển thị
- **And:** Chọn thoát không lưu chỉ loại bỏ kết quả chưa lưu
- **And:** Content gốc và các content đã lưu được giữ nguyên

**AC-012 — Phiên đăng nhập hết hạn**
- **Given:** Quản trị viên đang sử dụng chức năng viết lại nội dung
- **When:** Phiên đăng nhập hết hạn và Quản trị viên tạo hoặc lưu content
- **Then:** Hệ thống từ chối yêu cầu
- **And:** Content gốc và các content đã lưu không bị thay đổi
- **And:** Hệ thống yêu cầu Quản trị viên đăng nhập lại

## 5. REFERENCES
### Rules
- [BR-068](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/34dcdf09-ae77-4289-a543-4dc358144acc)
- [BR-069](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/6d7ccf54-c00a-47ac-8188-1f8e34bd84d4)
- [BR-070](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/4b698021-f4a6-4a2f-aed8-ce74dfd9d546)
- [BR-071](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/0c1b3d9d-e24c-465c-9d58-12563df8cb90)

### NON-FUNCTIONAL
- Yêu cầu viết lại cho từng nền tảng được xử lý độc lập để lỗi của một nền tảng không làm mất kết quả của nền tảng khác.
- Việc lưu một content mới và mối liên kết với content gốc phải hoàn tất toàn bộ; hệ thống không được lưu dữ liệu chưa hoàn chỉnh.

### OUT OF SCOPE
- Không thực hiện lên lịch hoặc đăng content lên nền tảng.
- Không thay đổi content gốc hoặc cấu hình quy tắc viết nội dung của từng nền tảng.

### Rule Details

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Story liên quan |
|---|---|---|---|---|---|---|---|
| BR-068 | Xử lý viết lại độc lập theo từng nền tảng | AI Content | Mỗi nền tảng được chọn phải có yêu cầu, kết quả và trạng thái xử lý riêng. | Khi Quản trị viên yêu cầu viết lại Content cho một hoặc nhiều nền tảng. | Hệ thống gửi một yêu cầu riêng theo quy tắc từng nền tảng và cho phép xem, sửa, tạo lại hoặc lưu riêng từng kết quả. Lỗi của một nền tảng không làm mất kết quả thành công hoặc kết quả chưa lưu của nền tảng khác. | Không | STORY-024 |
| BR-069 | Kết quả viết lại (paraphrase) phải hợp lệ theo nền tảng | AI Content | Chỉ kết quả có nội dung và hashtag đáp ứng quy tắc của nền tảng mới được phép lưu. | Khi AI trả kết quả hoặc Quản trị viên lưu kết quả viết lại. | Hệ thống kiểm tra riêng từng kết quả và chỉ bật thao tác lưu cho kết quả hợp lệ. Kết quả thiếu nội dung hoặc không đáp ứng quy tắc nền tảng được đánh dấu không hợp lệ nhưng không ảnh hưởng kết quả khác. | Không | STORY-024 |
| BR-070 | Lưu kết quả viết lại thành Content mới | AI Content | Mỗi kết quả viết lại được lưu thành một Content mới và không thay đổi Content gốc. | Khi Quản trị viên lưu một kết quả viết lại hợp lệ. | Hệ thống tạo ID riêng, sao chép tiêu đề, lưu nền tảng, nội dung, hashtag, người tạo, thời gian tạo. Content gốc và các kết quả chưa lưu luôn được giữ nguyên. | Không | STORY-024 |
| BR-071 | Lưu toàn vẹn Content viết lại | AI Content | Content viết lại phải được lưu đầy đủ trong cùng một lần xử lý. | Khi hệ thống lưu kết quả viết lại từ một Content đã lưu. | Hệ thống chỉ thông báo thành công sau khi Content mới được lưu đầy đủ. Nếu lưu lỗi, hệ thống không giữ Content chưa hoàn chỉnh. | Không | STORY-024 |
