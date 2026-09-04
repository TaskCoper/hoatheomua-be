# STORY-033 — Khách hàng tạo mẫu hoa bằng AI

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã đăng nhập và có yêu cầu tạo mẫu hoa, tôi muốn sử dụng AI để tạo ra hình ảnh mẫu hoa dựa trên yêu cầu tạo mẫu hoa đã khởi tạo, giúp tôi hình dung kết quả trước khi quyết định tải xuống, thêm vào giỏ hàng hoặc đặt hàng. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Khách hàng có thể tạo mẫu hoa bằng AI từ màn hình **Chi tiết yêu cầu tạo mẫu hoa**.

- Nếu yêu cầu tạo mẫu hoa **chưa có kết quả AI**, hệ thống hiển thị nút **"Tạo bó hoa AI ngay"**.
- Nếu yêu cầu tạo mẫu hoa **đã có ít nhất một kết quả AI** trước đó, hệ thống hiển thị nút **"Tạo lại"**.

Mỗi lần AI tạo thành công, hệ thống tạo ra **1 ảnh mẫu hoa** và lưu kết quả thành một bản ghi mới trong lịch sử tạo AI.

### Dữ liệu đầu vào cho AI

| Dữ liệu | Mô tả |
|---|---|
| **Combo đã chọn** | Combo nguồn của yêu cầu tạo mẫu hoa |
| **Danh sách hoa / thành phần** | Các loại hoa trong Combo |
| **Số lượng từng loại hoa** | Số lượng tương ứng từ Combo nguồn |
| **Dịp sử dụng** | Dịp sử dụng của yêu cầu |
| **Phong cách** | Phong cách mong muốn |
| **Ngân sách** | Ngân sách dự kiến |
| **Kích thước** | Kích thước mong muốn |
| **Ghi chú / Yêu cầu thêm** | Thông tin bổ sung từ khách hàng |
| **Mockup** | Dữ liệu tham chiếu để AI bám theo kiểu dáng, bố cục hoặc bối cảnh thể hiện |
| **Giấy gói** | Loại giấy gói đã chọn |
| **Ruy băng** | Loại ruy băng đã chọn |

> **Lưu ý:** Mockup được sử dụng làm dữ liệu tham chiếu để AI bám theo kiểu dáng, bố cục hoặc bối cảnh thể hiện của mẫu hoa.

> **Điều kiện khả dụng:**
> - Combo nguồn của yêu cầu tạo mẫu hoa phải còn khả dụng, chưa bị xóa mềm, chưa Inactive và chưa hết hàng tại thời điểm hệ thống validate request tạo AI.
> - Mockup được chọn phải còn Active và chưa bị xóa mềm tại thời điểm hệ thống validate request tạo AI.

---

## Conditions

### Preconditions
- Khách hàng đã đăng nhập.
- Yêu cầu tạo mẫu hoa tồn tại.
- Yêu cầu tạo mẫu hoa thuộc về khách hàng hiện tại.
- Yêu cầu tạo mẫu hoa có đầy đủ dữ liệu cần thiết để tạo mẫu hoa bằng AI.
- AI Job tạo mẫu hoa không ở trạng thái **"Đang tạo"**.
- Khách hàng còn ít nhất 1 lượt AI trong ngày.

### Trigger
Khách hàng thực hiện một trong các thao tác sau:
- Ấn chọn **"Tạo bó hoa AI ngay"** khi yêu cầu tạo mẫu hoa chưa có kết quả AI.
- Ấn chọn **"Tạo lại"** khi yêu cầu tạo mẫu hoa đã có kết quả AI trước đó.

---

## Flow

### Main Flow

1. Khách hàng truy cập màn hình **Chi tiết yêu cầu tạo mẫu hoa**.
2. Hệ thống kiểm tra trạng thái và lịch sử tạo AI của yêu cầu tạo mẫu hoa.
3. Hệ thống hiển thị chức năng tạo AI tương ứng.
4. Khách hàng ấn chọn **"Tạo bó hoa AI ngay"** hoặc **"Tạo lại"**.
5. Hệ thống kiểm tra Combo nguồn còn khả dụng, chưa bị xóa mềm, chưa Inactive, chưa hết hàng và Mockup còn Active, chưa bị xóa mềm.
6. Hệ thống kiểm tra yêu cầu tạo mẫu hoa hiện tại không có tiến trình AI đang chạy và số lượt AI trong ngày.
7. Hệ thống thu thập dữ liệu đầu vào của yêu cầu tạo mẫu hoa.
8. Hệ thống tạo nội dung đầu vào cho AI theo các Business Rule về prompt.
9. Hệ thống tạo AI job cho lần generate hiện tại và cập nhật trạng thái AI job thành **"Đang tạo"**.
10. Hệ thống trừ 1 lượt AI trong ngày của khách hàng.
11. Hệ thống gửi yêu cầu tạo ảnh sang AI service.
12. AI xử lý dữ liệu và tạo **1 ảnh mẫu hoa**.
13. Hệ thống nhận và kiểm tra kết quả AI.
14. Nếu ảnh output hợp lệ, hệ thống gắn logo theo quy tắc cấu hình.
15. Hệ thống lưu ảnh kết quả đã gắn logo.
16. Hệ thống tạo một bản ghi kết quả mới trong lịch sử tạo AI.
17. Hệ thống lưu snapshot dữ liệu nguồn tại thời điểm generate, gồm Combo, danh sách hoa/Core/Support và số lượng, Mockup, giấy gói, ruy băng và yêu cầu tùy chỉnh.
18. Hệ thống cập nhật AI job thành **"Đã tạo"**.
19. Hệ thống chuyển khách hàng đến màn hình **Kết quả bó hoa AI**.
20. Hệ thống hiển thị ảnh AI vừa được tạo.
21. Hệ thống hiển thị các chức năng **"Tải xuống"**, **"Tạo lại"**, **"Đặt hàng ngay"**, **"Thêm giỏ hàng"**.

---

### Alternative Flows

#### ALT-01 — Khách hàng rời trang trong khi AI đang tạo
1. Khách hàng đã bắt đầu quá trình tạo mẫu hoa bằng AI.
2. AI Job tạo mẫu hoa đang ở trạng thái **"Đang tạo"**.
3. Khách hàng reload hoặc rời khỏi trang.
4. Job AI vẫn tiếp tục được xử lý.
5. Khi khách hàng quay lại, hệ thống hiển thị trạng thái hiện tại của AI Job.
6. Nếu quá trình AI đã hoàn thành, hệ thống hiển thị kết quả đã tạo.

#### ALT-02 — Tạo lại mẫu hoa
1. Yêu cầu tạo mẫu hoa đã có ít nhất một kết quả AI trước đó.
2. Khách hàng ấn chọn **"Tạo lại"**.
3. Hệ thống xử lý đây là một request generate mới.
4. Nếu request được chấp nhận, hệ thống trừ 1 lượt AI của khách hàng.
5. Hệ thống tiếp tục xử lý generate theo Main Flow.
6. Nếu generate thành công, hệ thống lưu kết quả mới thành một bản ghi lịch sử riêng.
7. Các kết quả AI trước đó vẫn được giữ lại.
8. Nếu generate thất bại, hệ thống hoàn lại 1 lượt AI đã trừ cho request đó.

#### ALT-03 — Combo nguồn hoặc Mockup bị xóa mềm sau khi AI Job đã bắt đầu
1. Khách hàng gửi yêu cầu tạo mẫu hoa bằng AI hợp lệ.
2. Backend đã validate thành công yêu cầu tạo mẫu hoa, Combo nguồn, Mockup, quota và điều kiện generate.
3. Hệ thống đã tạo AI Job và gửi yêu cầu sang AI service.
4. Admin xóa mềm, chuyển Inactive hoặc ngừng khả dụng Combo nguồn hoặc Mockup trong lúc AI Job đang xử lý.
5. Hệ thống tiếp tục xử lý AI Job theo dữ liệu snapshot tại thời điểm validate.
6. Nếu AI tạo được ảnh output hợp lệ, hệ thống gắn logo theo quy tắc cấu hình, lưu ảnh kết quả và tạo đúng **01 History item**.
7. Hệ thống giữ nguyên lượt AI đã ghi nhận.
8. Từ các request tạo mẫu hoa AI mới sau đó, Combo nguồn hoặc Mockup đã không còn khả dụng không được validate là nguồn hợp lệ.

---

### Exception Flows

#### EXC-01 — Hết lượt AI trong ngày
1. Khách hàng không còn lượt AI trong ngày.
2. Khách hàng truy cập chức năng tạo mẫu hoa bằng AI.
3. Hệ thống không cho phép gửi yêu cầu AI mới.
4. Hệ thống disable nút tạo AI.
5. Hệ thống thông báo khách hàng đã hết lượt AI trong ngày.
6. Hệ thống thông báo lượt AI sẽ được cấp lại vào ngày hôm sau.

#### EXC-02 — Đã có job AI đang chạy
1. AI Job tạo mẫu hoa đang ở trạng thái **"Đang tạo"**.
2. Khách hàng tiếp tục ấn nút tạo AI hoặc gửi nhiều request liên tiếp.
3. Hệ thống chỉ chấp nhận request đầu tiên.
4. Hệ thống không tạo thêm job AI mới.
5. Hệ thống tiếp tục hiển thị trạng thái **"Đang tạo"**.

#### EXC-03 — AI service gặp lỗi
1. AI service gặp lỗi hoặc không thể tạo ảnh.
2. Hệ thống tự động thử lại tối đa **2 lần** và không trừ thêm lượt AI.
3. Trong thời gian thử lại, AI job giữ trạng thái **"Đang tạo"**.
4. Hệ thống cập nhật AI job thành **"Lỗi"**, hoàn lại **01 lượt AI** và không tạo History item.

#### EXC-04 — Yêu cầu tạo mẫu hoa không hợp lệ
1. Yêu cầu không tồn tại hoặc không thuộc khách hàng hiện tại.
2. Hệ thống từ chối thao tác.
3. Hệ thống không tạo job AI, không tự động thử lại và không trừ lượt AI.
4. Hệ thống không trả dữ liệu của khách hàng khác.

#### EXC-05 — Combo nguồn hoặc Mockup không còn khả dụng trước khi chạy AI
1. Khách hàng chọn **"Tạo bó hoa AI ngay"** hoặc **"Tạo lại"**.
2. Trước khi hệ thống tạo AI Job, Combo nguồn bị hết hàng, chuyển Inactive, bị xóa mềm hoặc ngừng khả dụng; hoặc Mockup bị chuyển Inactive, bị xóa mềm hoặc ngừng khả dụng.
3. Backend kiểm tra lại trạng thái khả dụng của Combo nguồn và Mockup.
4. Backend phát hiện Combo nguồn hoặc Mockup không còn khả dụng.
5. Hệ thống không gửi yêu cầu sang AI service.
6. Hệ thống không tạo ảnh mới.
7. Hệ thống không tạo History item.
8. Hệ thống không trừ lượt AI.
9. Hệ thống thông báo: **"Combo hoa hoặc Mockup hiện không còn khả dụng nên hệ thống chưa thể tạo mẫu hoa phù hợp. Vui lòng chọn lại Combo hoa hoặc Mockup khác để tiếp tục."**

---

## Acceptance Criteria

### AC-001 — Hiển thị nút tạo AI lần đầu
- **Given:** Khách hàng đang xem một yêu cầu tạo mẫu hoa chưa có kết quả AI.
- **When:** Màn hình Chi tiết yêu cầu tạo mẫu hoa được hiển thị.
- **Then:** Hệ thống phải hiển thị nút **"Tạo bó hoa AI ngay"**.

### AC-002 — Hiển thị nút Tạo lại
- **Given:** Yêu cầu tạo mẫu hoa đã có ít nhất một kết quả AI.
- **When:** Khách hàng xem màn hình kết quả AI.
- **Then:** Hệ thống phải hiển thị nút **"Tạo lại"**.

### AC-003 — Kiểm tra quota AI
- **Given:** Khách hàng đang ở màn hình có chức năng tạo mẫu hoa bằng AI.
- **When:** Khách hàng ấn chọn **"Tạo bó hoa AI ngay"** hoặc **"Tạo lại"**.
- **Then:** Hệ thống phải kiểm tra số lượt AI còn lại trong ngày trước khi chấp nhận request.
- **And:** Nếu khách hàng không còn lượt AI trong ngày, hệ thống không được tạo job AI mới.

### AC-004 — Thu thập dữ liệu đầu vào
- **Given:** Yêu cầu tạo mẫu hoa hợp lệ và khách hàng còn ít nhất 01 lượt AI.
- **When:** Khách hàng yêu cầu tạo mẫu hoa bằng AI.
- **Then:** Hệ thống sử dụng dữ liệu của yêu cầu tạo mẫu hoa làm đầu vào gồm: Combo, danh sách hoa/thành phần và số lượng, dịp sử dụng, phong cách, ngân sách, kích thước, ghi chú/yêu cầu thêm, Mockup.

### AC-005 — Áp dụng quy tắc Prompt
- **Given:** Hệ thống đã thu thập đầy đủ dữ liệu đầu vào.
- **When:** Hệ thống xây dựng nội dung đầu vào gửi sang AI.
- **Then:** Hệ thống phải áp dụng thứ tự ưu tiên: **Combo > Mockup > Ghi chú/Yêu cầu thêm > Phong cách**.
- **And:** Hệ thống phải truyền danh sách loại hoa và số lượng tương ứng từ Combo nguồn cho AI.
- **And:** AI phải cố gắng tạo kết quả bám sát loại hoa, số lượng, kiểu dáng, bố cục hoặc bối cảnh của dữ liệu đầu vào và Mockup đã chọn.
- **And:** Ảnh AI chỉ mang tính chất minh họa và không được sử dụng làm dữ liệu chính thức để xác định thành phần hoặc số lượng nguyên vật liệu của Combo.

### AC-006 — Chuyển trạng thái khi bắt đầu tạo
- **Given:** Yêu cầu tạo mẫu hoa đủ điều kiện sử dụng AI và khách hàng còn ít nhất 1 lượt AI.
- **When:** Request tạo mẫu hoa bằng AI được hệ thống chấp nhận.
- **Then:** Hệ thống phải tạo AI job cho lần generate và cập nhật trạng thái AI job thành **"Đang tạo"**.

### AC-007 — Trừ lượt khi request được chấp nhận
- **Given:** Khách hàng còn ít nhất 1 lượt AI và yêu cầu tạo mẫu hoa đủ điều kiện generate.
- **When:** Request tạo mẫu hoa bằng AI được hệ thống chấp nhận.
- **Then:** Hệ thống phải trừ đúng 1 lượt AI của khách hàng.
- **And:** Số lượt AI còn lại phải được cập nhật tương ứng.

### AC-008 — Không tạo duplicate job
- **Given:** Yêu cầu tạo mẫu hoa đã có một AI Job đang ở trạng thái **"Đang tạo"**.
- **When:** Hệ thống nhận thêm một hoặc nhiều request generate cho cùng yêu cầu, kể cả request đồng thời từ nhiều tab hoặc client.
- **Then:** Hệ thống không được tạo AI Job mới.
- **And:** Tại mọi thời điểm, một yêu cầu tạo mẫu hoa chỉ được có tối đa **01 AI Job** đang xử lý.
- **And:** Các request bị từ chối không được trừ thêm lượt AI.

### AC-009 — Tạo mẫu hoa AI thành công
- **Given:** Request AI đã được chấp nhận và đã trừ 01 lượt AI.
- **When:** AI trả về ảnh output hợp lệ, hệ thống lưu ảnh thành công và tạo bản ghi lịch sử thành công.
- **Then:** Hệ thống lưu đúng **01 ảnh mẫu hoa** cho lần generate đó.
- **And:** Hệ thống tạo đúng **01 History item** cho lần generate.
- **And:** AI job được cập nhật thành **"Đã tạo"**.
- **And:** Hệ thống không hoàn lại lượt AI đã trừ.

### AC-010 — Hoàn lượt khi tạo thất bại
- **Given:** Request AI đã được chấp nhận và đã trừ 1 lượt AI.
- **When:** Job vẫn thất bại sau khi hoàn tất các lần tự động thử lại.
- **Then:** Hệ thống phải hoàn lại đúng 1 lượt AI.
- **And:** Không tạo History item.
- **And:** Mỗi job chỉ được hoàn lượt tối đa một lần.

### AC-011 — Lưu lịch sử tạo
- **Given:** AI đã tạo và lưu ảnh thành công.
- **When:** Quá trình tạo kết thúc thành công.
- **Then:** Hệ thống phải lưu kết quả AI thành một bản ghi lịch sử mới.
- **And:** Các kết quả AI trước đó không được bị ghi đè hoặc xóa.

### AC-012 — Giữ job khi khách hàng rời trang
- **Given:** AI job của lần generate hiện tại đang ở trạng thái **"Đang tạo"**.
- **When:** Khách hàng reload hoặc rời khỏi trang.
- **Then:** Job AI vẫn phải tiếp tục xử lý.
- **And:** Khi khách hàng quay lại, hệ thống phải hiển thị đúng trạng thái hiện tại hoặc kết quả cuối cùng.

### AC-013 — Tự động thử lại khi gặp lỗi kỹ thuật
- **Given:** Request AI đã được chấp nhận và đã trừ 1 lượt AI.
- **When:** AI service lỗi, không trả ảnh hợp lệ hoặc hệ thống không lưu được kết quả.
- **Then:** Hệ thống tự động thử lại tối đa **2 lần** trong cùng job.
- **And:** Không trừ thêm lượt AI và AI Job tiếp tục giữ trạng thái **"Đang tạo"**.
- **And:** Chỉ khi vẫn thất bại sau các lần thử lại, hệ thống mới chuyển trạng thái AI Job thành **"Lỗi"**, hoàn lại 1 lượt AI và không tạo History item.

### AC-014 — Tạo lại
- **Given:** Yêu cầu tạo mẫu hoa đã có ít nhất một kết quả AI trước đó và khách hàng còn ít nhất 1 lượt AI.
- **When:** Khách hàng ấn chọn **"Tạo lại"**.
- **Then:** Hệ thống phải xử lý đây là một lần generate mới.
- **And:** Nếu generate thành công, hệ thống phải tạo một kết quả AI mới và giữ lại các kết quả cũ trong lịch sử.
- **And:** Nếu lần generate mới thất bại sau các lần retry, hệ thống hoàn lại 01 lượt AI đã trừ và không tạo History item.

### AC-015 — Hiển thị kết quả AI
- **Given:** AI đã tạo và lưu ảnh thành công.
- **When:** Khách hàng được chuyển đến màn hình Kết quả bó hoa AI.
- **Then:** Hệ thống phải hiển thị ảnh AI vừa được tạo.
- **And:** Hệ thống phải hiển thị các chức năng **"Tải xuống"**, **"Tạo lại"**, **"Đặt hàng ngay"**, **"Thêm giỏ hàng"**.

### AC-016 — Hết lượt AI
- **Given:** Khách hàng không còn lượt AI trong ngày.
- **When:** Hệ thống hiển thị chức năng tạo mẫu hoa bằng AI.
- **Then:** Nút **"Tạo bó hoa AI ngay"** hoặc **"Tạo lại"** phải ở trạng thái **disabled**.
- **And:** Hệ thống phải thông báo khách hàng đã hết lượt AI trong ngày.

### AC-017 — Hiển thị cảnh báo về ảnh AI
- **Given:** Khách hàng đang xem màn hình Kết quả bó hoa AI.
- **When:** Hệ thống hiển thị ảnh mẫu hoa do AI tạo.
- **Then:** Hệ thống phải hiển thị thông báo cho biết ảnh AI chỉ mang tính chất minh họa và có thể có sai khác so với Combo thực tế.
- **And:** Nếu UI sử dụng thông tin "độ tương đồng khoảng 80%", hệ thống phải thể hiện đây là mức ước lượng/tham khảo, không phải cam kết độ chính xác tuyệt đối.

### AC-018 — Không tạo AI Job nếu Combo nguồn hoặc Mockup không còn khả dụng trước validate
- **Given:** Yêu cầu tạo mẫu hoa có Combo nguồn và Mockup.
- **When:** Khách hàng ấn **"Tạo bó hoa AI ngay"** hoặc **"Tạo lại"** và Backend phát hiện Combo nguồn hoặc Mockup không còn khả dụng trước khi tạo AI Job.
- **Then:** Hệ thống không được gửi yêu cầu sang AI service.
- **And:** Hệ thống không được tạo ảnh mới.
- **And:** Hệ thống không được tạo History item.
- **And:** Hệ thống không được trừ lượt AI.
- **And:** Hệ thống phải thông báo nguồn tạo mẫu hoa không còn khả dụng.

### AC-019 — Vẫn hoàn tất AI Job nếu Combo nguồn hoặc Mockup bị xóa sau validate
- **Given:** AI Job tạo mẫu hoa đang được xử lý cho một yêu cầu có Combo nguồn và Mockup.
- **When:** Admin xóa mềm, chuyển Inactive hoặc ngừng khả dụng Combo nguồn hoặc Mockup trong lúc AI Job đang xử lý.
- **Then:** Hệ thống vẫn tiếp tục AI Job theo dữ liệu đã được ghi nhận tại thời điểm request được chấp nhận.
- **And:** Nếu AI tạo được ảnh output hợp lệ, hệ thống vẫn gắn logo, lưu ảnh kết quả và tạo đúng **01 History item**.
- **And:** Hệ thống giữ nguyên lượt AI đã ghi nhận.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-020**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/19df37d9-dd95-4cfd-bf58-cedda0daceb5) | Điều kiện tạo mẫu hoa bằng AI | Tạo mẫu hoa bằng AI | Chỉ yêu cầu tạo mẫu hoa hợp lệ, thuộc khách hàng hiện tại và còn ít nhất 1 lượt sử dụng AI mới được phép tạo mẫu hoa bằng AI. | Khách hàng ấn chọn "Tạo bó hoa AI ngay" hoặc "Tạo lại". | Hệ thống kiểm tra quyền sở hữu yêu cầu tạo mẫu hoa, trạng thái hiện tại và số lượt AI còn lại trước khi chấp nhận request. | Không cho phép tạo AI nếu yêu cầu không tồn tại, không thuộc khách hàng hiện tại, yêu cầu đang có AI Job ở trạng thái "Đang tạo", hoặc khách hàng không còn lượt AI trong ngày. | Đức Bình | STORY-033 | Draft | v0 | 2026-08-14 |
| [**BR-021**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/904674d6-8dcd-4f8e-bdd7-4385ce7a9aba) | Giới hạn lượt AI mỗi ngày | Tạo mẫu hoa | Mỗi tài khoản khách hàng được sử dụng tối đa 03 lượt tạo mẫu hoa bằng AI mỗi ngày. Một lượt AI được ghi nhận tạm thời khi hệ thống chấp nhận yêu cầu tạo mẫu hoa và tạo AI job hợp lệ. Nếu AI job kết thúc thất bại mà không tạo được ảnh output hợp lệ, hệ thống hoàn lại lượt đã ghi nhận cho khách hàng. Số lượt được cấp lại vào 00:00 mỗi ngày theo múi giờ Asia/Ho_Chi_Minh. | Khách hàng thực hiện yêu cầu tạo mẫu hoa bằng AI hoặc tạo lại mẫu hoa bằng AI. | Hệ thống kiểm tra số lượt AI còn lại của tài khoản trong ngày hiện tại. Nếu tài khoản còn lượt: Hệ thống chấp nhận yêu cầu tạo AI hợp lệ. Hệ thống ghi nhận sử dụng 01 lượt AI. Hệ thống tạo AI job tương ứng. Nếu AI job tạo thành công ảnh output hợp lệ, lượt đã ghi nhận được giữ nguyên. Nếu AI job kết thúc thất bại và không có ảnh output hợp lệ, hệ thống hoàn lại 01 lượt AI cho tài khoản. Mỗi AI job chỉ được hoàn lượt tối đa 01 lần. | Nếu tài khoản không còn lượt AI trong ngày: Hệ thống không cho phép tạo AI job mới. Không ghi nhận thêm lượt sử dụng. Hệ thống thông báo khách hàng đã sử dụng hết lượt AI trong ngày. Khách hàng được cấp lại lượt vào 00:00 ngày tiếp theo theo múi giờ Asia/Ho_Chi_Minh. Nếu cùng một yêu cầu được gửi lặp lại do double-click, retry hoặc request trùng: Hệ thống không được tạo nhiều AI job ngoài quy tắc xử lý request trùng. Không được ghi nhận nhiều lượt AI cho cùng một AI job. Không được hoàn lượt nhiều hơn một lần cho cùng một AI job. | Đức Bình | STORY-033 | Draft | v0 | 2026-08-14 |
| [**BR-022**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2fb20e00-ff18-4cb9-815e-8d18ef4d2ac7) | Quy tắc trừ và hoàn lượt AI | Tạo mẫu hoa bằng AI | Mỗi request tạo mẫu hoa bằng AI được hệ thống chấp nhận sẽ sử dụng 1 lượt AI của khách hàng. | Request tạo mẫu hoa bằng AI được hệ thống chấp nhận và bắt đầu xử lý. | Hệ thống trừ đúng 1 lượt AI của khách hàng. | Nếu quá trình tạo mẫu hoa không hoàn tất thành công do lỗi AI, kết quả AI không hợp lệ hoặc lỗi hệ thống, hệ thống phải hoàn lại đúng 1 lượt AI đã trừ cho lần tạo đó. Mỗi job AI chỉ được trừ tối đa 1 lượt và chỉ được hoàn lại tối đa 1 lượt tương ứng. | Đức Bình | STORY-033 | Draft | v0 | 2026-08-14 |
| [**BR-023**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e59ab341-da80-43fa-9127-9d27e91a61fc) | Input tạo mẫu hoa bằng AI | Tạo mẫu hoa bằng AI | AI phải sử dụng dữ liệu của yêu cầu tạo mẫu hoa hiện tại làm đầu vào cho quá trình tạo ảnh. | Hệ thống chuẩn bị dữ liệu để gửi sang AI service. | Hệ thống sử dụng các dữ liệu sau: Combo đã chọn. Thành phần trong Combo. Số lượng từng loại hoa. Dịp sử dụng. Phong cách. Ngân sách. Kích thước. Ghi chú / Yêu cầu thêm. Mockup. Giấy gói. Ruy băng. | Không sử dụng dữ liệu của yêu cầu tạo mẫu hoa khác làm đầu vào cho lần generate hiện tại. | Đức Bình | STORY-033 | Draft | v0 | 2026-08-14 |
| [**BR-024**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c4b6e286-44b5-4c50-9c56-5909816d54d3) | Thứ tự ưu tiên Prompt | Tạo mẫu hoa bằng AI | Khi các dữ liệu đầu vào có nội dung ảnh hưởng hoặc xung đột với nhau, hệ thống phải áp dụng thứ tự ưu tiên đã xác định. | Hệ thống xây dựng nội dung đầu vào gửi sang AI. | Thứ tự ưu tiên phải là: Combo > Mockup > Ghi chú / Yêu cầu thêm > Phong cách. | Thông tin có mức ưu tiên thấp hơn không được làm thay đổi ràng buộc của thông tin có mức ưu tiên cao hơn. | Đức Bình | STORY-033 | Draft | v0 | 2026-08-14 |
| [**BR-025**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fdde7f40-004d-484b-925f-81aae8917bbb) | Thành phần hoa của kết quả AI | Tạo mẫu hoa bằng AI | Ảnh mẫu hoa AI phải được tạo dựa trên danh sách loại hoa và số lượng của Combo nguồn; ảnh kết quả chỉ mang tính chất minh họa và có thể có sai khác so với Combo thực tế. | Hệ thống xây dựng dữ liệu đầu vào và gửi yêu cầu tạo ảnh sang AI. | Hệ thống phải truyền đầy đủ danh sách loại hoa và số lượng tương ứng từ Combo nguồn sang AI. AI phải cố gắng tạo hình ảnh bám sát thành phần hoa của Combo nguồn. Danh sách loại hoa và số lượng của Combo nguồn là dữ liệu chính thức được sử dụng cho nghiệp vụ thực hiện đơn hàng. | Sai khác về loại hoa, số lượng, màu sắc, hình dáng hoặc cách bố trí thể hiện trên ảnh AI không làm thay đổi dữ liệu thành phần của Combo nguồn. | Đức Bình | STORY-033 | Draft | v0 | 2026-08-14 |
| [**BR-026**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4f4f6bad-810a-449d-88e6-d6626ae04ec6) | Quy tắc sử dụng Mockup | Tạo mẫu hoa bằng AI | Mockup là dữ liệu tham chiếu giúp AI xác định kiểu dáng, bố cục và bối cảnh thể hiện của mẫu hoa. | AI tạo hình ảnh dựa trên yêu cầu tạo mẫu hoa. | AI phải cố gắng tạo kết quả bám sát Mockup đã chọn về kiểu dáng, bố cục hoặc bối cảnh thể hiện. | Mockup chỉ được sử dụng làm dữ liệu tham chiếu về kiểu dáng, bố cục hoặc bối cảnh thể hiện; Mockup không làm thay đổi dữ liệu loại hoa và số lượng được xác định từ Combo nguồn. | Đức Bình | STORY-033 | Draft | v0 | 2026-08-14 |
| [**BR-027**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/efbc6956-6fcd-44f7-9bbe-417b0c3ffe0b) | Số lượng ảnh đầu ra | Tạo mẫu hoa bằng AI | Mỗi lần tạo mẫu hoa bằng AI thành công chỉ sinh ra 1 ảnh kết quả. | AI hoàn thành một lần generate thành công. | Hệ thống ghi nhận và lưu đúng 1 ảnh kết quả cho lần generate đó. | Không tính nhiều ảnh là nhiều kết quả trong cùng một lượt generate. | Đức Bình | STORY-033 | Draft | v0 | 2026-08-14 |
| [**BR-028**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2d405952-3d9d-4c82-8640-a348c9ef8cd8) | Trạng thái AI Job tạo mẫu hoa | Tạo mẫu hoa bằng AI | Mỗi lần generate mẫu hoa bằng AI phải có trạng thái phản ánh tiến trình xử lý của AI Job tương ứng. | Trạng thái xử lý của AI Job thay đổi. | AI Job sử dụng các trạng thái: Đang tạo: AI Job đang được xử lý. Đã tạo: ảnh hợp lệ và History item đã được lưu thành công. Lỗi: AI Job kết thúc thất bại sau các lần retry. | Không được chuyển AI Job sang "Đã tạo" nếu ảnh hoặc History item chưa được lưu thành công. | Đức Bình | STORY-033 | Draft | v0 | 2026-08-14 |
| [**BR-029**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8dca1a5b-f00f-4840-be33-b14658d53526) | Chống tạo nhiều job AI đồng thời | Tạo mẫu hoa bằng AI | Một yêu cầu tạo mẫu hoa chỉ được có tối đa 1 job AI đang chạy tại một thời điểm. | Một request AI đã được chấp nhận và đang được xử lý. | Hệ thống không chấp nhận thêm job AI mới cho cùng yêu cầu tạo mẫu hoa. | Các thao tác hoặc request lặp lại trong khi job hiện tại đang chạy không được tạo thêm job và không được trừ thêm lượt AI. | Đức Bình | STORY-033 | Draft | v0 | 2026-08-14 |
| [**BR-030**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/77ac0f5a-7ccc-4a7a-b171-296afa1599df) | Xử lý job khi khách hàng rời trang | Tạo mẫu hoa bằng AI | Job tạo mẫu hoa bằng AI không phụ thuộc vào việc khách hàng còn mở màn hình tạo AI hay không. | Khách hàng reload trang hoặc rời khỏi màn hình trong lúc AI Job của yêu cầu đang ở trạng thái "Đang tạo". | Job AI vẫn phải tiếp tục được xử lý. Khi khách hàng quay lại, hệ thống phải hiển thị trạng thái hiện tại hoặc kết quả cuối cùng nếu job đã hoàn thành. | Không được tự động hủy job AI chỉ vì khách hàng reload, đóng trang hoặc điều hướng sang màn hình khác. | Đức Bình | STORY-033 | Draft | v0 | 2026-08-14 |
| [**BR-031**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a3b98880-74cd-4516-a30c-79e8b8dc724f) | Lưu lịch sử kết quả AI | Tạo mẫu hoa bằng AI | Mỗi lần tạo mẫu hoa bằng AI thành công phải được lưu thành một kết quả riêng trong lịch sử tạo. | AI tạo ảnh thành công và hệ thống lưu kết quả hoàn tất. | Hệ thống tạo một bản ghi lịch sử mới chứa ảnh của lần generate đó. Các kết quả AI đã tạo trước đó phải tiếp tục được giữ lại. | Job đang xử lý hoặc kết thúc thất bại mà không có ảnh hợp lệ không được tạo History item và không được tính vào tổng số mẫu. Không được ghi đè hoặc xóa kết quả cũ khi khách hàng sử dụng chức năng "Tạo lại". | Đức Bình | STORY-033 | Draft | v0 | 2026-08-21 |
| [**BR-032**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7816fafe-0980-408b-9227-b17595c35dbf) | Tách biệt quota Hoa AI và Thiệp AI | Tạo mẫu hoa bằng AI | Quota sử dụng chức năng tạo mẫu hoa AI và quota sử dụng chức năng tạo thiệp AI phải được quản lý độc lập với nhau. | Khi khách hàng sử dụng chức năng tạo mẫu hoa AI hoặc tạo thiệp AI. | Hệ thống phải quản lý quota theo từng chức năng riêng biệt: Tạo mẫu hoa AI: tối đa 3 lượt / khách hàng / ngày. Tạo thiệp AI: tối đa 10 lượt / khách hàng / ngày. Việc sử dụng lượt tạo mẫu hoa AI không được làm giảm quota tạo thiệp AI. Việc sử dụng lượt tạo thiệp AI không được làm giảm quota tạo mẫu hoa AI. Hai quota được reset độc lập khi sang ngày mới. | Không được cộng gộp quota tạo mẫu hoa AI và quota tạo thiệp AI thành một hạn mức dùng chung. Khách hàng hết quota của một chức năng vẫn được sử dụng chức năng AI còn lại nếu quota của chức năng đó vẫn còn. | Đức Bình | STORY-033 | Draft | v0 | 2026-08-14 |
| [**BR-155**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b5a30cd0-97a5-433b-ac63-0661748348a7) | Điều kiện generate ảnh | AI | Backend phải kiểm tra Combo nguồn chưa hết hàng, chưa Inactive, chưa bị xóa mềm; và Mockup còn Active, chưa bị xóa mềm trước khi gửi yêu cầu đến AI. | Khách hàng thực hiện "Tạo mẫu hoa bằng AI" hoặc "Tạo lại". | Backend phải kiểm tra tồn kho hiện tại của Combo nguồn thông qua Nhanh.vn trước khi gửi yêu cầu đến AI. Nếu Combo hết hàng: Không gửi request generate đến AI. Không tạo ảnh mới. Không cho phép tiếp tục thao tác generate. Hiển thị thông báo Combo đã hết hàng. | N/A | Đức Bình | STORY-033 | Draft | v0 | 2026-08-19 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-020 | [Điều kiện tạo mẫu hoa bằng AI](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/19df37d9-dd95-4cfd-bf58-cedda0daceb5) |
| BR-021 | [Giới hạn lượt AI mỗi ngày](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/904674d6-8dcd-4f8e-bdd7-4385ce7a9aba) |
| BR-022 | [Quy tắc trừ và hoàn lượt AI](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2fb20e00-ff18-4cb9-815e-8d18ef4d2ac7) |
| BR-023 | [Input tạo mẫu hoa bằng AI](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e59ab341-da80-43fa-9127-9d27e91a61fc) |
| BR-024 | [Thứ tự ưu tiên Prompt](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c4b6e286-44b5-4c50-9c56-5909816d54d3) |
| BR-025 | [Thành phần hoa của kết quả AI](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fdde7f40-004d-484b-925f-81aae8917bbb) |
| BR-026 | [Quy tắc sử dụng Mockup](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4f4f6bad-810a-449d-88e6-d6626ae04ec6) |
| BR-027 | [Số lượng ảnh đầu ra](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/efbc6956-6fcd-44f7-9bbe-417b0c3ffe0b) |
| BR-028 | [Trạng thái AI Job tạo mẫu hoa](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2d405952-3d9d-4c82-8640-a348c9ef8cd8) |
| BR-029 | [Chống tạo nhiều job AI đồng thời](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8dca1a5b-f00f-4840-be33-b14658d53526) |
| BR-030 | [Xử lý job khi khách hàng rời trang](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/77ac0f5a-7ccc-4a7a-b171-296afa1599df) |
| BR-031 | [Lưu lịch sử kết quả AI](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a3b98880-74cd-4516-a30c-79e8b8dc724f) |
| BR-032 | [Tách biệt quota Hoa AI và Thiệp AI](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/7816fafe-0980-408b-9227-b17595c35dbf) |
| BR-155 | [Điều kiện generate ảnh](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b5a30cd0-97a5-433b-ac63-0661748348a7) |

### Dependencies
- [**STORY-030**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce) — Tạo yêu cầu tạo mẫu hoa

---

## Non-Functional Requirements

- Sau khi khách hàng ấn **"Tạo bó hoa AI ngay"** hoặc **"Tạo lại"**, hệ thống phải phản hồi việc request đã được chấp nhận hoặc bị từ chối trong **p95 ≤ 1.5 giây**, không bao gồm thời gian AI generate ảnh.
- Sau khi request AI được chấp nhận, giao diện phải chuyển sang trạng thái **"Đang tạo"** trong vòng **≤ 500 ms** kể từ khi frontend nhận response xác nhận từ backend.
- Việc kiểm tra quota AI phải hoàn tất trong **p95 ≤ 500 ms**.
- Việc cập nhật số lượt AI sau khi trừ hoặc hoàn lượt phải được phản ánh cho khách hàng trong vòng **≤ 2 giây** kể từ khi hệ thống xác nhận thao tác tương ứng.
- Hệ thống phải đảm bảo **100%** request trùng lặp trong cùng một job đang chạy không tạo thêm AI job hoặc trừ thêm quota.
- Mỗi yêu cầu tạo mẫu hoa chỉ được có tối đa **1 job AI** ở trạng thái đang xử lý tại cùng một thời điểm.
- Khi khách hàng reload hoặc rời khỏi trang, job AI đã được backend chấp nhận phải tiếp tục xử lý và không được bị hủy do mất kết nối phía client.
- Khi khách hàng quay lại màn hình trong lúc job đang chạy, trạng thái hiện tại phải được tải và hiển thị trong **p95 ≤ 2 giây**.
- Sau khi AI trả ảnh thành công, việc lưu ảnh và cập nhật lịch sử phải hoàn tất trong **p95 ≤ 3 giây**, không tính thời gian AI generate.
- Một kết quả AI chỉ được đánh dấu **"Đã tạo"** sau khi ảnh và bản ghi lịch sử đã được lưu thành công.
- Sau khi tất cả lần tự động thử lại đều thất bại, việc cập nhật trạng thái sang **"Lỗi"** và hoàn lại quota phải hoàn tất trong **p95 ≤ 1.5 giây** kể từ khi backend xác định job thất bại cuối cùng.
- Hệ thống phải đảm bảo mỗi AI job chỉ được trừ tối đa **1 lượt** và hoàn tối đa **1 lượt**, kể cả khi có retry, callback lặp hoặc request trùng.
- Lịch sử các lần tạo AI thành công phải được lưu bền vững; một lần **"Tạo lại"** không được ghi đè kết quả trước đó.
- Response lỗi gửi về client không được chứa stack trace, SQL error, connection string, API key, prompt nội bộ hoặc thông tin kỹ thuật nhạy cảm.
- Các API liên quan đến generate AI, quota và lịch sử phải yêu cầu xác thực; người dùng chỉ được truy cập dữ liệu AI thuộc tài khoản của mình.

---

## Out of Scope

- Xóa lịch sử kết quả AI.
- Mua thêm lượt AI.
- Nâng cấp gói AI.
- Quản trị model AI.
- Cho khách hàng chỉnh sửa prompt trực tiếp.
- Quy trình tải xuống ảnh.
- Quy trình thêm giỏ hàng.
- Quy trình đặt hàng.
