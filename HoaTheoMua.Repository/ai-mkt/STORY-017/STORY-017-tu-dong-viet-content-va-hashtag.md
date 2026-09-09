# STORY-017: Tự động viết content và hashtag

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Nháp - Cập nhật: 05/09/2026)

**Author:** Hồ Hoàng Nam
**Reviewer:** Nguyễn Đức Bình
**Approver:** Chưa chỉ định
**Owner:** Nguyễn Anh Quân

**Feedback yêu cầu sửa gần nhất:**
> Cần có thêm định nghĩa cho phần chủ đề trong context. Cái này thuộc SP S1. Cần có thêm quy tắc cho hashtag như số lượng, định dạng (có/không dấu #), độ dài, ngôn ngữ, khử trùng lặp. Chưa có AC cho Alt - 02
> *- Nguyễn Đức Bình · 09:54 03/09/2026*

**Thống kê:** Rules: 12 | Unit Tests: 0 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn hệ thống tự động viết nội dung và hashtag từ thông tin đầu vào để tạo bài đăng nhanh chóng và nhất quán
- **Context:** Admin cần tạo nội dung đăng bài phù hợp với chủ đề và nền tảng. Hệ thống sử dụng AI để sinh nội dung và hashtag từ thông tin đầu vào, cho phép Admin xem lại và yêu cầu tạo lại trước khi sử dụng. Chức năng này chỉ bao gồm sinh nội dung và hashtag, không bao gồm tạo ảnh, lên lịch hoặc đăng bài
**Chủ đề:** 
  - Nhập chủ đề chính và thông tin bổ sung để AI hiểu rõ nội dung cần viết. 
  - Nội dung nhập phải có từ 5 đến 1.000 ký tự sau khi loại bỏ khoảng trắng ở đầu và cuối.
**Mục tiêu:** 
  - Bắt buộc chọn một: Giới thiệu sản phẩm, Khuyến mãi, Tăng nhận diện, Kêu gọi tương tác, Thông báo hoặc Khác. 
  - Nếu chọn Khác phải nhập mô tả tối đa 200 ký tự.
**Đối tượng:** 
  - Bắt buộc, 5–300 ký tự.
**Nền tảng:** 
  - Bắt buộc chọn đúng một nền tảng đang được hỗ trợ và kết nối. Việc tạo cho nhiều nền tảng thuộc STORY-024.
**Giọng văn:** 
  - Bắt buộc chọn một: Chuyên nghiệp, Thân thiện, Truyền cảm hứng, Ngắn gọn hoặc Vui vẻ hoặc Khác. 
  - Nếu chọn Khác phải nhập mô tả tối đa 200 ký tự.
**Ghi chú:** 
  - Có placeholder ghi chú là những thông tin thêm dành cho AI để xử lí.
**Quy tắc Hashtag:**
  - Số hashtag tối đa được quy định trong system prompt, tối đa 30, tối thiểu 1.
  - Mỗi hashtag phải bắt đầu bằng ký tự #. Sau dấu # không được có khoảng trắng.
  - Hashtag chỉ được chứa chữ cái, chữ số và dấu gạch dưới _. Không cho phép ký tự đặc biệt khác như ! @ $ % & *.
  - Mỗi hashtag có độ dài từ 2 đến 50 ký tự, không tính dấu #.
  - Cho phép hashtag bằng tiếng Việt có dấu, tiếng Việt không dấu hoặc tiếng Anh.
  - Hashtag phải liên quan đến nội dung được sinh, không tạo hashtag không liên quan chỉ để tăng số lượng.
  - Hệ thống phải loại bỏ hashtag trùng lặp, không phân biệt chữ hoa/chữ thường.
- **Sprint:** S1
- **Priority:** Must
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Admin đã đăng nhập
### Trigger
- Admin chọn chức năng “Tự động viết content và hashtag”

## 3. FLOW
### Main Flow: Tự động viết content và hashtag
1. Admin mở chức năng “Tự động viết content và hashtag”
2. Hệ thống hiển thị form nhập thông tin gồm chủ đề, mục tiêu, đối tượng, nền tảng và giọng văn bắt buộc. Ghi chú không bắt buộc
3. Admin nhập thông tin và chọn “Tạo nội dung”
4. Hệ thống kiểm tra tính hợp lệ (đã quy định trong context) của dữ liệu đầu vào
5. Hệ thống gửi yêu cầu bao gồm các thông tin được nhập đến AI
6. AI sinh nội dung và danh sách hashtag phù hợp
7. Hệ thống hiển thị kết quả để Admin xem lại
8. Admin chọn “Sử dụng nội dung”
9. Hệ thống lưu nội dung và hashtag đã chọn

### Alternative Flow
**ALT-01 — Tạo lại kết quả**
- Tại bước 7, nếu Admin chưa hài lòng, Admin chọn “Tạo lại”
- Hệ thống gửi lại yêu cầu với cùng dữ liệu đầu vào
- Hệ thống hiển thị kết quả mới và quay lại bước 7

**ALT-02 — Chỉnh sửa kết quả**
- Tại bước 7, Admin chỉnh sửa nội dung hoặc hashtag do AI tạo
- Hệ thống giữ thay đổi và tiếp tục bước 8

**ALT-03 — Thay đổi đầu vào và tạo lại kết quả**
- Tại bước 7, Admin thay đổi một hoặc nhiều thông tin đầu vào.
- Hệ thống xác định kết quả đang hiển thị không còn tương ứng với dữ liệu mới. Hiển thị thông báo "Đầu vào đã bị thay đổi so với ban đầu...(FE đề xuất) " kèm nút "Tạo lại""
- Admin chọn “Tạo nội dung” hoặc “Tạo lại”. Hoặc bấm "Sử dụng nội dung", hệ thông sẽ lưu nội dung hiện tại
- Nếu Admin chọn “Tạo nội dung” hoặc “Tạo lại”. Hệ thống gửi yêu cầu mới bằng dữ liệu đầu vào đã cập nhật.
- Hệ thống hiển thị kết quả mới.

**ALT-04 — Thoát khi có kết quả chưa sử dụng**
- Admin đã nhập dữ liệu hoặc có kết quả AI nhưng chưa chọn “Lưu nội dung”.
- Admin đóng màn hình hoặc yêu cầu quay lại.
- Hệ thống cảnh báo nội dung chưa được lưu.
- Nếu Admin chọn tiếp tục, hệ thống giữ màn hình, dữ liệu đầu vào và kết quả đang hiển thị.
- Nếu Admin chọn thoát không lưu, hệ thống không lưu nội dung hoặc hashtag và loại bỏ dữ liệu chưa lưu.

### Exception Flow
**EXC-01 — Dữ liệu đầu vào không hợp lệ**
- Tại bước 4, nếu thiếu trường bắt buộc hoặc vượt giới hạn, hệ thống không gửi yêu cầu đến AI
- Hệ thống đánh dấu trường không hợp lệ và hiển thị lý do cho từng trường
- Hệ thống giữ lại toàn bộ dữ liệu Admin đã nhập.
- Admin được phép sửa dữ liệu và thử lại.
- Hệ thống không gửi dữ liệu cho AI tạo nội dung.

**EXC-02 — AI không phản hồi**
- Tại bước 5, nếu AI không phản hồi trong thời gian quy định (60 giây), hệ thống dừng xử lý
- Hệ thống thông báo “Không thể tạo nội dung, vui lòng thử lại”
- Hệ thống giữ nguyên dữ liệu đầu vào Admin đã nhập.
- Hệ thống không lưu nội dung hoặc hashtag.
- Admin chọn "thử lại" yêu cầu tạo nội dung.

**EXC-03 — Không thể lưu nội dung**
- Tại bước 9, quá trình lưu nội dung hoặc hashtag xảy ra lỗi.
- Hệ thống xử lý việc lưu nội dung và hashtag như một giao dịch thống nhất.
- Hệ thống không lưu một phần nội dung hoặc hashtag.
- Hệ thống không tạo content đã lưu nếu quá trình lưu không hoàn tất.
- Hệ thống giữ lại kết quả AI và các chỉnh sửa của Admin.
- Hệ thống không hiển thị hoặc ghi nhận lưu thành công.
- Admin được phép thử lại và việc thử lại không tạo dữ liệu trùng lặp.

**EXC-05 — Phiên đăng nhập hết hạn trong lúc tạo nội dung**
- Phiên đăng nhập hết hạn trước khi Admin tạo lại hoặc sử dụng nội dung.
- Hệ thống từ chối yêu cầu tương ứng.
- Hệ thống thông báo phiên đăng nhập đã hết hạn.
- Hệ thống yêu cầu Admin đăng nhập lại.

## 4. ACCEPTANCE CRITERIA
**AC-001**
- **Given:** Admin đã đăng nhập
- **When:** Admin mở chức năng
- **Then:** Hệ thống hiển thị biểu mẫu nhập thông tin cần thiết
- **And:** Nút “Tạo nội dung” chỉ khả dụng khi đã nhập đủ và đúng các trường bắt buộc

**AC-002**
- **Given:** Admin đã nhập dữ liệu hợp lệ
- **When:** Admin chọn “Tạo nội dung”
- **Then:** Hệ thống sinh nội dung và hashtag phù hợp với thông tin đầu vào
- **And:** Hệ thống hiển thị kết quả để Admin xem lại

**AC-003**
- **Given:** Kết quả AI đang hiển thị
- **When:** Admin chọn “Tạo lại”
- **Then:** Hệ thống tạo kết quả mới với cùng dữ liệu đầu vào
- **And:** Hệ thống giữ nguyên thông tin Admin đã nhập

**AC-004**
- **Given:** Kết quả AI đang hiển thị
- **When:** Admin chỉnh sửa và chọn “Sử dụng nội dung”
- **Then:** Hệ thống giữ các thay đổi của Admin
- **And:** Hệ thống tạo một content đã lưu gồm nội dung và hashtag Admin đã chọn.

**AC-005**
- **Given:** Admin đã nhập thông tin nhưng dữ liệu không đáp ứng các điều kiện hợp lệ(quy định trong context)
- **When:** Admin thực hiện yêu cầu tạo content
- **Then:** Hệ thống không tạo content và hiển thị thông báo dữ liệu không hợp lệ để Admin kiểm tra và chỉnh sửa.
- **And:** Hệ thống không gửi yêu cầu đến AI.
- **And:** Hệ thống giữ nguyên dữ liệu Admin đã nhập.
- **And:** Không nội dung hoặc hashtag nào được lưu.
- **And:** Admin được phép sửa dữ liệu và thử lại.

**AC-006**
- **Given:** AI không thể xử lý yêu cầu tạo content
- **When:** Admin thực hiện yêu cầu tạo content
- **Then:** Hệ thống không tạo content và hiển thị thông báo lỗi phù hợp, cho phép Admin thực hiện lại thao tác
- **And:** Hệ thống giữ nguyên dữ liệu đầu vào Admin đã nhập.
- **And:** Hệ thống không lưu nội dung hoặc hashtag.
- **And:** Admin được phép thử lại yêu cầu tạo nội dung.

**AC-007**
- **Given:** Hệ thống đã tạo content thành công và hiển thị kết quả cho Admin
- **When:** Admin thực hiện thao tác lưu content
- **Then:** Hệ thống lưu content thành công và hiển thị thông báo xác nhận
- **And:** Nội dung và hashtag được lưu đúng theo kết quả Admin đã chọn hoặc chỉnh sửa.
- **And:** Hệ thống chỉ thông báo thành công sau khi nội dung và hashtag được lưu đầy đủ.
- **And:** Hệ thống không tự động đăng bài hoặc lên lịch đăng.

**AC-008**
- **Given:** Kết quả AI đang hiển thị và dữ liệu đầu vào vẫn hợp lệ.
- **When:** Admin chọn “Tạo lại”.
- **Then:** Hệ thống gửi yêu cầu mới bằng cùng dữ liệu đầu vào.
- **And:** Hệ thống giữ nguyên dữ liệu Admin đã nhập.

**AC-009**
- **Given:** Kết quả AI đang hiển thị.
- **When:** Admin thay đổi một hoặc nhiều thông tin đầu vào.
- **Then:** Hệ thống xác định kết quả hiện tại không còn tương ứng với dữ liệu mới.
- **And:** Admin có thể tạo lại kết quả trước khi chọn “Sử dụng nội dung”. Hoặc chọn “Sử dụng nội dung”, hệ thống lưu nội dung hiện tại

**AC-010**
- **Given:** Admin đã nhập dữ liệu hoặc có kết quả chưa chọn “Sử dụng nội dung”.
- **When:** Admin yêu cầu đóng hoặc rời màn hình.
- **Then:** Hệ thống yêu cầu xác nhận.
- **And:** Chọn tiếp tục phải giữ nguyên dữ liệu đầu vào và kết quả đang hiển thị.
- **And:** Chọn thoát không lưu, không thêm vào content đã lưu.

**AC-011**
- **Given:** Admin đã nhập dữ liệu hợp lệ(quy định trong context) và AI có phản hồi.
- **When:** Kết quả không có nội dung, không có hashtag hoặc không đúng cấu trúc yêu cầu.
- **Then:** Hệ thống không hiển thị kết quả đó như một kết quả hợp lệ.
- **And:** Hệ thống không lưu nội dung hoặc hashtag.
- **And:** Hệ thống giữ nguyên dữ liệu đầu vào và cho phép Admin thử lại.

**AC-012**
- **Given:** Admin đã chọn “Sử dụng nội dung” với một kết quả hợp lệ.
- **When:** Quá trình lưu nội dung hoặc hashtag xảy ra lỗi.
- **Then:** Hệ thống không ghi nhận lưu thành công.
- **And:** Không tồn tại nội dung hoặc hashtag được lưu một phần.
- **And:** Bản nháp hiện tại được giữ nguyên.
- **And:** Kết quả và các chỉnh sửa của Admin được giữ lại để thử lại.

**AC-013**
- **Given:** Admin đang nhập dữ liệu hoặc xem kết quả AI.
- **When:** Phiên đăng nhập hết hạn và Admin tạo lại hoặc sử dụng nội dung.
- **Then:** Hệ thống từ chối yêu cầu.
- **And:** Bản nháp không bị thay đổi.
- **And:** Hệ thống yêu cầu Admin đăng nhập lại.

## 5. REFERENCES
### Rules
- [BR-015](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/bd997782-4580-4519-b96d-91ef12536a20)
- [BR-016](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/87f9f8f0-515f-48c1-8f46-4aa57ffef4bf)
- [BR-017](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/f2b1c31e-65cf-4fe1-be04-b4fe035b0779)
- [BR-018](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/8b93f9ae-a327-48be-8e1d-78f40bdd9c23)
- [BR-019](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/2dd59a4b-f4ed-489b-8788-ab3684cd7da4)
- [BR-020](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/8c0f117b-25fe-4070-9e3f-682a955ddd4c)
- [BR-021](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/af6d57ef-0350-4009-a454-7f8f04f37eaf)
- [BR-022](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/1b8d95f0-954e-48d3-8a45-fee5ddcfe830)
- [BR-055](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/960037a7-033a-4ea1-8ab1-3cf6820e5e7b)
- [BR-056](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/8d6b4823-8fd0-4af2-a63e-e13ad7e5844c)
- [BR-057](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/918f53c2-dd63-4962-98d7-b4383ae5cf2e)
- [BR-058](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/6a28f2d2-9f3e-44e4-9d4a-8855cd933f99)

### NON-FUNCTIONAL
- Hệ thống không tự động lưu hoặc đăng kết quả trước khi Admin xác nhận

### OUT OF SCOPE
- Không bao gồm tạo hoặc chỉnh sửa hình ảnh
- Không bao gồm lên lịch hoặc tự động đăng bài
- Không bao gồm quản lý cấu hình hoặc huấn luyện mô hình AI

### Rule Details

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Story liên quan |
|---|---|---|---|---|---|---|---|
| BR-015 | Độ dài Chủ đề (Topic) khi sinh content | AI Content | Input đầu vào cho AI về chủ đề phải đủ thông tin nhưng không quá dài. | Khi nhập dữ liệu vào trường Chủ đề và gọi AI. | Hệ thống validate độ dài từ 5 đến 1.000 ký tự. Ngoài khoảng này sẽ chặn và báo lỗi. | Không | STORY-017 |
| BR-016 | Độ dài Đối tượng (Target Audience) | AI Content | Input về đối tượng mục tiêu cần giới hạn độ dài hợp lý. | Khi nhập dữ liệu vào trường Đối tượng và gọi AI. | Hệ thống validate độ dài từ 5 đến 300 ký tự. | Không | STORY-017 |
| BR-017 | Giới hạn số lượng hashtag | AI Content | Số lượng hashtag trên mỗi bài content bị giới hạn để tránh spam. | Khi AI trả về kết quả hoặc khi Admin lưu form Content. | Hệ thống kiểm đếm số lượng hashtag, yêu cầu phải nằm trong khoảng từ 1 đến 30. | Không | STORY-017 |
| BR-018 | Định dạng bắt đầu của Hashtag | AI Content | Hashtag chuẩn phải mở đầu bằng ký hiệu thăng. | Khi lưu hoặc format hashtag. | Hệ thống kiểm tra ký tự đầu tiên của chuỗi hashtag phải là '#' (VD: #promotion). | Không | STORY-017 |
| BR-019 | Không chứa khoảng trắng trong Hashtag | AI Content | Hashtag phải là một chuỗi liền mạch. | Khi lưu hashtag. | Hệ thống kiểm tra không có khoảng trắng ngay sau dấu # và bên trong nội dung hashtag. | Không | STORY-017 |
| BR-020 | Độ dài cho phép của một Hashtag | AI Content | Hashtag không được quá ngắn vô nghĩa hoặc quá dài khó đọc. | Khi xác thực từng hashtag. | Độ dài nội dung hashtag, không tính dấu #, phải từ 2 đến 50 ký tự. | Không | STORY-017 |
| BR-021 | Ký tự hợp lệ trong Hashtag | AI Content | Hashtag chỉ chấp nhận chữ, số và dấu gạch dưới. | Khi kiểm tra tính hợp lệ của hashtag. | Chỉ cho phép RegEx: chữ cái, số, và dấu `_`. | Không | STORY-017 |
| BR-022 | Loại bỏ Hashtag trùng lặp | AI Content | Một bài content không được chứa 2 hashtag giống nhau. | Khi lưu danh sách hashtag của một bài. | Hệ thống loại bỏ hashtag trùng lặp, không phân biệt chữ hoa và chữ thường, và chỉ giữ lại một hashtag. | Không | STORY-017 |
| BR-055 | Giá trị Mục tiêu và Giọng văn khi tạo Content | AI Content | Mục tiêu và Giọng văn là thông tin bắt buộc, được chọn từ danh sách quy định hoặc nhập mô tả cho lựa chọn “Khác”. | Khi Quản trị viên yêu cầu AI tạo Content. | Hệ thống chỉ gửi yêu cầu khi Mục tiêu và Giọng văn hợp lệ; mô tả của lựa chọn “Khác” không được vượt quá 200 ký tự cho mỗi trường. | Không | STORY-017 |
| BR-056 | Chỉ tạo Content cho một nền tảng hợp lệ | AI Content | Mỗi yêu cầu tạo Content chỉ áp dụng cho đúng một nền tảng đang được hỗ trợ và kết nối. | Khi Quản trị viên gửi yêu cầu tạo Content và hashtag. | Hệ thống không gửi yêu cầu AI nếu chưa chọn, chọn nhiều hơn một hoặc chọn nền tảng không còn hợp lệ. Tạo nội dung cho nhiều nền tảng được xử lý trong STORY-024. | Không | STORY-017 |
| BR-057 | Kết quả AI hợp lệ trước khi sử dụng | AI Content | Kết quả AI chỉ được sử dụng khi có Content và hashtag đáp ứng cấu trúc cùng các quy tắc đã định nghĩa. | Khi AI trả về kết quả tạo Content. | Hệ thống kiểm tra nội dung, số lượng, định dạng, tính duy nhất và mức độ liên quan của hashtag trước khi cho phép sử dụng kết quả. Kết quả thiếu Content, thiếu hashtag hoặc sai cấu trúc không được hiển thị như kết quả hợp lệ và không được lưu. | Không | STORY-017 |
| BR-058 | Lưu toàn vẹn Content và hashtag do AI tạo | AI Content | Content và hashtag được chọn phải được lưu đầy đủ trong cùng một lần xác nhận. | Khi Quản trị viên chọn sử dụng một kết quả AI hợp lệ. | Hệ thống chỉ thông báo thành công sau khi Content và toàn bộ hashtag được lưu đầy đủ. Nếu lưu lỗi, không được tồn tại Content hoặc hashtag lưu một phần; kết quả hiện tại được giữ lại và thử lại không tạo dữ liệu trùng. | Không | STORY-017 |
