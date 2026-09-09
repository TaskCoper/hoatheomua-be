# STORY-002
## User Story
Cần làm
• có thay đổi chưa tạo phiên bản
Lên lịch và tự động đăng bài Nội dung chi tiết của User Story STORY-002

## METADATA
**Story**
Là một Quản trị viên, tôi muốn chọn nội dung và hình ảnh đã được lưu, chọn nền tảng được hỗ trợ và thiết lập thời gian đăng để hệ thống tự động đăng bài đúng thời gian đã lên lịch mà không cần thao tác thủ công tại thời điểm đăng

**Context**
Trước đây, sau khi nội dung và hình ảnh được tạo và lưu, Quản trị viên phải đăng bài thủ công lên từng nền tảng vào thời gian mong muốn. Khi số lượng bài viết và nền tảng tăng lên, việc đăng thủ công dễ dẫn đến quên lịch, đăng sai thời gian hoặc mất nhiều thời gian thao tác. Chức năng này cho phép Quản trị viên lựa chọn nội dung và hình ảnh đã được lưu, lựa chọn một hoặc nhiều nền tảng đã được kết nối với hệ thống và thiết lập thời gian đăng. Quy định dữ liệu khi tạo lịch đăng bài
1. Nội dung
   - Bắt buộc chọn đúng một nội dung đã được lưu.
   - Nội dung phải còn tồn tại và thuộc phạm vi Quản trị viên được phép sử dụng.
2. Hình ảnh
   - Hình ảnh không bắt buộc.
   - Mỗi lịch đăng được chọn một hoặc nhiều ảnh tối đa 10 ảnh đã lưu hoặc trên máy.
   - Nếu là ảnh đã lưu thì phải còn tồn tại.
   - Ảnh phải có định dạng JPG, JPEG hoặc PNG và dung lượng không vượt quá 10 MB.
3. Nền tảng
   - Bắt buộc chọn ít nhất một nền tảng.
   - Mỗi nền tảng chỉ được chọn một lần trong cùng một lịch đăng.
   - Nền tảng phải được hệ thống hỗ trợ, đang được bật và đang kết nối hợp lệ.
4. Ngày và giờ đăng
   - Bắt buộc nhập đầy đủ ngày và giờ đăng.
   - Ngày và giờ đăng phải lớn hơn thời điểm hiện tại
5. Kiểm tra lịch trùng
   - Một lịch được xem là trùng khi có cùng nội dung, cùng nền tảng và cùng thời điểm đăng với một lịch đang ở trạng thái “Đã lên lịch”.
   - Nếu Quản trị viên chọn nhiều nền tảng, hệ thống kiểm tra trùng riêng cho từng nền tảng.
   - Khi phát hiện trùng, hệ thống không tạo lịch và phải chỉ rõ nền tảng cùng thời điểm đang bị trùng.

**Sprint**: S1
**Priority**: Must
**Assignee**: FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
**Creator**: Nguyễn Anh Quân
**Status**: Cần làm

## CONDITIONS
**Preconditions**
Quản trị viên đã đăng nhập vào trang admin

**Trigger**
Quản trị viên chọn mục "Quản lí bài đăng" trên menu điều hướng của trang admin

## FLOW
### Main Flow
1. Admin truy cập màn hình danh sách các content
2. Hệ thống hiển thị danh sách content để lên lịch đăng
3. Admin chọn content muốn lên lịch
4. Hệ thống hiển thị thông tin content và hình ảnh/media đã gắn với content để Admin kiểm tra
5. Hệ thống hiển thị các nền tảng hiện đang được kết nối và có thể sử dụng
6. Admin lựa chọn một hoặc nhiều nền tảng
7. Admin chọn ngày và thời gian muốn đăng bài
8. Hệ thống kiểm tra: Content hợp lệ, media hợp lệ, đã chọn ít nhất một nền tảng, thời gian đăng hợp lệ và lớn hơn thời gian hiện tại, và không trùng lịch với nội dung tương tự
9. Nội dung còn tồn tại, đã được lưu và không để trống.
10. Media còn tồn tại, đã được lưu và đúng định dạng hỗ trợ.
11. Đã chọn ít nhất một nền tảng đang kết nối hợp lệ.
12. Ngày và giờ đăng lớn hơn thời điểm hiện tại.
13. Không có lịch đăng trùng nội dung, nền tảng và thời điểm.
14. Admin nhấn “Lên lịch”.
15. Hệ thống tạo lịch đăng. Lịch được lưu với trạng thái: "Đã lên lịch"
16. Hệ thống hiển thị thông báo: “Lên lịch đăng bài thành công".

### Alternative Flow
**ALT-01 — Admin thay đổi nền tảng trước khi lên lịch**
Tại bước chọn nền tảng, Admin thay đổi nền tảng đã chọn (Facebook, zalo, Ins,..)
Hệ thống cập nhật lựa chọn
Admin tiếp tục thiết lập thời gian và lên lịch.

**ALT-02 — Admin hủy tạo lịch**
Trước khi nhấn “Lên lịch”, Admin chọn "Hủy"
Hệ thống không tạo lịch
Hệ thống quay về danh sách content

### Exception Flow
**EXC-01 — Dữ liệu lên lịch không hợp lệ**
Không chọn nền tảng
Không chọn thời gian đăng
Thời gian đăng nhỏ hơn hoặc bằng thời gian hiện tại
Content hoặc media không còn hợp lệ

**EXC-03 — Token của nền tảng hết hạn**
Nếu thông tin xác thực của một nền tảng đã hết hạn -> Hệ thống không thực hiện đăng lên nền tảng đó
Cập nhật nền tảng thành Đăng thất bại
Thông báo cho Admin cần kết nối lại nền tảng

**EXC-04 — Nền tảng trả về rate-limit**
Nếu nền tảng từ chối request do vượt giới hạn gọi API
Hệ thống hiển thị nút "Thử lại"
Nếu đăng lại thành công → cập nhật “Đăng thành công”
Nếu đã thử lại đủ số lần quy định nhưng vẫn bị giới hạn → cập nhật “Đăng thất bại”
Hệ thống lưu lý do lỗi và thông báo cho Admin

**EXC-05 — Lịch bị trùng với một lịch khác**
Tại bước 8, nếu lịch bị trùng với một lịch khác, trùng content, media, thời gian và nền tảng được chọn
Hệ thống thông báo lỗi sau khi Admin nhấn "Lên lịch"
Thông báo lỗi cho biết đang bị trùng với lịch nào
Hệ thống không gửi dữ liệu về Server

## ACCEPTANCE CRITERIA
**AC-001 - Chỉ hiển thị content đủ điều kiện**
Given
Admin đã đăng nhập
When
Admin truy cập chức năng lên lịch đăng bài
Then
Hệ thống chỉ hiển thị các content để Admin lựa chọn

**AC-002 - Chưa chọn nền tảng và bấm đăng**
Given
Admin đã chọn content và thời gian đăng hợp lệ
When
Admin chưa chọn bất kỳ nền tảng nào và nhấn “Lên lịch”
Then
Hệ thống không tạo lịch đăng
And
Hiển thị thông báo yêu cầu Admin chọn ít nhất một nền tảng

**AC-003 - Tự động thực hiện đăng đúng lịch**
Given
Một lịch đăng hợp lệ đang ở trạng thái Đã lên lịch
When
Đến thời gian đăng đã thiết lập
Then
Hệ thống tự động thực hiện tác vụ đăng bài lên các nền tảng được chọn
And
Admin không cần thực hiện thêm thao tác đăng thủ công

**AC-004 - Đăng thành công**
Given
Lịch đã đến thời điểm đăng
When
Tất cả nền tảng được chọn trả về kết quả đăng thành công
Then
Hệ thống cập nhật từng nền tảng thành Đăng thành công
And
Cập nhật trạng thái tổng của lịch thành Đăng thành công

**AC-006 - Xử lý rate-limit**
Given
Hệ thống đang gọi API để đăng bài
When
Nền tảng trả về rate-limit
Then
Hệ thống hiển thị nút thử lại
And
Đăng lại thành công, hệ thống hiển thị đăng thành công

**AC-007 — Nội dung bị nền tảng từ chối**
Given
Hệ thống gửi content và media đến nền tảng
When
Nền tảng từ chối bài viết do nội dung hoặc media không đáp ứng chính sách
Then
Hệ thống không tự động retry
And
Cập nhật nền tảng thành Đăng thất bại
And
Lưu lý do thất bại
And
Thông báo cho Admin

**AC-008 — Kiểm tra dữ liệu trước khi tạo lịch**
Given
Quản trị viên đang thiết lập lịch đăng bài.
When
Quản trị viên chọn “Lên lịch”.
Then
Hệ thống chỉ tạo lịch khi nội dung còn tồn tại, đã được lưu và không để trống; ảnh còn tồn tại, đã được lưu hoặc được tải lên đúng định dạng; đã chọn ít nhất một nền tảng đang kết nối hợp lệ; ngày và giờ đăng được nhập đầy đủ và lớn hơn thời điểm hiện tại.
And
Nếu có dữ liệu không hợp lệ, hệ thống không tạo lịch, hiển thị lỗi tại trường tương ứng và giữ nguyên dữ liệu Quản trị viên đã nhập.

**AC-009 — Admin thay đổi nền tảng trước khi bấm lên lịch**
Given
Admin đang thiết lập lịch đăng bài.
When
Admin chọn đổi nền tảng và chưa bấm "Lên lịch"
Then
Chưa có thông tin nào được gửi về server cho tới khi Admin bấm "Lên lịch"

**AC-010 — Bấm hủy trước khi bấm lên lịch**
Given
Admin đang thiết lập lịch đăng bài.
When
Admin bấm hủy
Then
Màn hình quay về danh sách các Content

**AC-011— Token của các nền tảng bị hết hạn**
Given
Hệ thống đã liên kết với các nền tảng
When
Token của một trong các nền tảng hết hạn và có bài admin muốn đăng và đang gửi lên nền tảng đó
Then
Hệ thống hiển thị thông báo lỗi hết hạn Token cho Admin, và đề xuất Admin đăng nhập lại

## REFERENCES
### Rules
- [BR-001](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/f47d6261-cae6-4ed8-b879-249b26d17464)
- [BR-002](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/4f9d052b-085c-404f-a824-3aba19aff7a1)
- [BR-003](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/829d7c04-6572-41e0-99ab-bde96247c61f)
- [BR-004](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/0b6c451c-d250-4513-aa5d-7354e6e4537b)
- [BR-005](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/b69f2298-64eb-4355-ab2d-d8f18afdc46e)
- [BR-006](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/94ad04a1-10ed-4d41-9345-aa73112ca1cb)
- [BR-046](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/a90db8e1-0a6f-4bac-90d1-637449f6c024)
- [BR-047](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/f8a54fb3-59bc-4856-a49f-ad5155633da0)
- [BR-048](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/853f7ce5-9493-499a-8304-68c56403ac74)
- [BR-049](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/841ae103-814f-49d5-bba4-2f3a7e354756)

### NON-FUNCTIONAL
Không có

### OUT OF SCOPE
Không có

### Rule Details

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Story liên quan |
|---|---|---|---|---|---|---|---|
| BR-001 | Định dạng file ảnh đính kèm bài đăng | Schedule & Auto-Post | Chỉ chấp nhận các định dạng hình ảnh phổ biến được hỗ trợ bởi các mạng xã hội. | Khi tải ảnh đính kèm lên form lên lịch đăng bài. | Hệ thống kiểm tra đuôi file, chỉ cho phép JPG, JPEG, PNG. Từ chối và báo lỗi với các định dạng khác. | Không | STORY-002 |
| BR-002 | Dung lượng tối đa của ảnh đính kèm | Schedule & Auto-Post | Mỗi file ảnh đính kèm không được vượt quá 10MB. | Khi file ảnh được tải lên hệ thống. | Hệ thống chặn upload và hiển thị cảnh báo file vượt quá dung lượng 10MB. | Không | STORY-002 |
| BR-003 | Giới hạn số lượng ảnh đính kèm bài đăng | Schedule & Auto-Post | Một bài đăng chỉ được đính kèm tối đa 10 hình ảnh. | Khi chọn hoặc tải ảnh vào danh sách đính kèm bài đăng. | Nếu vượt quá 10 ảnh, hệ thống không cho thêm và thông báo đạt giới hạn. | Không | STORY-002 |
| BR-004 | Thời điểm hẹn giờ đăng bài hợp lệ | Schedule & Auto-Post | Thời gian đăng bài phải diễn ra trong tương lai so với thời điểm thao tác. | Khi lưu thiết lập thời gian cho lịch đăng bài. | Hệ thống kiểm tra thời gian chọn > thời gian hiện tại của hệ thống. Nếu không, yêu cầu chọn lại. | Không | STORY-002 |
| BR-005 | Bắt buộc chọn nền tảng mạng xã hội | Schedule & Auto-Post | Một lịch đăng phải được gắn với ít nhất một nền tảng kết nối. | Khi bấm lưu lịch đăng bài. | Nếu danh sách nền tảng trống, hệ thống chặn lưu và yêu cầu chọn nền tảng. | Không | STORY-002 |
| BR-006 | Chống trùng lặp lịch đăng bài | Schedule & Auto-Post | Không cho phép tạo 2 lịch đăng bài giống hệt nhau về nội dung, nền tảng và thời gian. | Khi bấm xác nhận lưu lịch đăng mới. | Hệ thống quét DB, nếu trùng 3 yếu tố (Content, Platform, Time), hệ thống chặn lại và báo lỗi trùng lặp. | Không | STORY-002 |
| BR-046 | Nội dung hợp lệ của lịch đăng bài | Schedule & Auto-Post | Mỗi lịch đăng phải sử dụng đúng một Content đã lưu và còn tồn tại. | Khi Quản trị viên lưu lịch đăng bài. | Hệ thống chỉ tạo lịch nếu đã chọn đúng một Content hợp lệ; nếu Content không tồn tại hoặc để trống thì không tạo lịch. | Không | STORY-002 |
| BR-047 | Nền tảng hợp lệ và không trùng trong lịch đăng | Schedule & Auto-Post | Nền tảng được chọn phải được hỗ trợ, kết nối hợp lệ và không lặp trong cùng một lịch. | Khi Quản trị viên chọn nền tảng và lưu lịch đăng. | Hệ thống kiểm tra riêng từng nền tảng, loại trừ lựa chọn trùng và không tạo lịch nếu không còn nền tảng hợp lệ. Nền tảng mất kết nối trước khi lưu được xem là không hợp lệ. | Không | STORY-002 |
| BR-048 | Trạng thái đăng được ghi nhận riêng theo nền tảng | Schedule & Auto-Post | Kết quả đăng bài phải được ghi nhận riêng cho từng nền tảng. | Khi lịch đến thời điểm thực hiện đăng bài. | Hệ thống cập nhật trạng thái và lý do lỗi của từng nền tảng; trạng thái tổng chỉ là “Đăng thành công” khi tất cả nền tảng đều thành công. Lỗi của một nền tảng không được làm mất kết quả đã ghi nhận của nền tảng khác. | Không | STORY-002 |
| BR-049 | Xử lý lỗi từ nền tảng khi tự động đăng | Schedule & Auto-Post | Việc thử lại phụ thuộc vào loại lỗi do nền tảng trả về. Tối đa thử lại 3 lần | Khi nền tảng từ chối hoặc không thực hiện được yêu cầu đăng bài. | Hệ thống yêu cầu kết nối lại khi thông tin xác thực hết hạn; thử lại theo số lần cấu hình khi bị giới hạn API; không tự động thử lại khi nội dung hoặc hình ảnh vi phạm chính sách nền tảng. Sau khi thử lại đủ số lần mà vẫn thất bại, hệ thống ghi trạng thái “Đăng thất bại”. | Không | STORY-002 |
