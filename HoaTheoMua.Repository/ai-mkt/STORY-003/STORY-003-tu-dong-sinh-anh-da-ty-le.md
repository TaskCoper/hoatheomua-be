# STORY-003: Tự động sinh ảnh đa tỷ lệ từ ảnh core

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Nháp - Cập nhật: 04/09/2026)

**Author:** Hồ Hoàng Nam
**Reviewer:** Nguyễn Đức Bình
**Approver:** Chưa chỉ định
**Owner:** Nguyễn Anh Quân

**Thống kê:** Rules: 8 | Unit Tests: 0 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn tải lên một ảnh core và yêu cầu hệ thống tự động tạo các phiên bản ảnh theo tỷ lệ phù hợp với từng nền tảng để có thể sử dụng cùng một thiết kế trên Facebook, Instagram và Zalo OA mà không cần chỉnh từng ảnh thủ công.
- **Context:** Admin cần cung cấp ảnh core làm nguồn. Hệ thống sử dụng AI để điều chỉnh bố cục và mở rộng/cắt ảnh nhằm tạo các phiên bản tỷ lệ khác nhau, đồng thời phải giữ các yếu tố quan trọng như sản phẩm, logo và nội dung chính trong [khu vực an toàn: Cần chốt lúc họp]. Admin có thể xem kết quả, tạo lại nếu chưa phù hợp và lưu các phiên bản mong muốn. Định dạng: JPG, JPEG, PNG Dung lượng: tối đa 10 MB. Tỷ lệ hỗ trợ: 
  - 1:1 — 1080×1080 
  - 4:5 — 1080×1350 
  - 9:16 — 1080×1920 
  - 16:9 — 1920×1080 
  - 2:1 — 1200×600
- **Sprint:** S1
- **Priority:** Must
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Admin đã đăng nhập
- Hệ thống có cấu hình tỷ lệ ảnh cho các nền tảng

### Trigger
- Admin mở chức năng Tự động sinh ảnh đa tỷ lệ.

## 3. FLOW
### Main Flow: Tạo và lưu ảnh đa tỷ lệ
1. Quản trị viên mở chức năng “Tạo ảnh đa tỷ lệ”.
2. Hệ thống hiển thị khu vực tải ảnh nguồn và danh sách tỉ lệ được hỗ trợ.
3. Quản trị viên tải lên một ảnh nguồn.
4. Hệ thống kiểm tra định dạng, dung lượng, chiều rộng và tỉ lệ của ảnh.
5. Quản trị viên chọn một tỉ lệ muốn tạo.
6. Quản trị viên chọn “Tạo ảnh”.
7. Hệ thống kiểm tra lại ảnh nguồn.
8. Hệ thống chuyển sang trạng thái “Đang tạo ảnh” và không cho phép gửi lại cùng một yêu cầu trong thời gian xử lý.
9. Hệ thống tạo một ảnh theo tỉ lệ đã chọn.
10. Hệ thống hiển thị ảnh kết quả.
11. Quản trị viên xem ảnh kết quả.
12. Quản trị viên chọn “Lưu”.
13. Hệ thống hiển thị modal nhập tên và mô tả cho ảnh do AI trả về
14. Admin nhập tên cho ảnh, bắt buộc, tối thiểu 1 kí tự, tối đa 200 kí tự
15. Admin nhập mô tả cho ảnh, Không bắt buộc, tối đa 1000 kí tự
16. Hệ thống lưu ảnh kết quả, tên, tỉ lệ, kích thước, người tạo, thời gian tạo.
17. Hệ thống chỉ thông báo “Lưu ảnh thành công”.

### Alternative Flow
**ALT-01 — Tạo lại ảnh**
- Tại bước 11 của Luồng chính, nếu chưa hài lòng với ảnh kết quả, Quản trị viên chọn “Tạo lại”.
- Hệ thống kiểm tra lại ảnh nguồn và tỉ lệ đã chọn.
- Hệ thống chuyển sang trạng thái “Đang tạo ảnh”.
- Hệ thống tạo một ảnh mới theo cùng tỉ lệ.
- Hệ thống thay ảnh đang hiển thị bằng ảnh vừa tạo lại.
- Quản trị viên tiếp tục từ bước 11 của Luồng chính.

**ALT-02 — Thay đổi tỉ lệ trước khi tạo**
- Tại bước 5 của Luồng chính, Quản trị viên thay đổi tỉ lệ đã chọn.
- Hệ thống bỏ lựa chọn trước đó và chỉ ghi nhận tỉ lệ được chọn cuối cùng.
- Quản trị viên tiếp tục từ bước 6 của Luồng chính.

**ALT-03 — Thay đổi ảnh nguồn**
- Trước khi chọn “Tạo ảnh”, Quản trị viên tải lên ảnh nguồn khác.
- Hệ thống thay thế ảnh nguồn đang chọn.
- Hệ thống kiểm tra lại ảnh nguồn mới.
- Quản trị viên tiếp tục từ bước 5 của Luồng chính.

### Exception Flow
**EXC-01 — Ảnh nguồn không hợp lệ**
- Tại bước 4 của Luồng chính, ảnh nguồn không đáp ứng điều kiện về định dạng, dung lượng, chiều rộng hoặc tỉ lệ.
- Hệ thống không cho phép tiếp tục.
- Hệ thống hiển thị rõ điều kiện không hợp lệ.
- Quản trị viên tải lên ảnh nguồn khác.

**EXC-02 — Chưa chọn tỉ lệ**
- Tại bước 6 của Luồng chính, nếu Quản trị viên chưa chọn tỉ lệ, Hệ thống không tạo ảnh.
- Hệ thống yêu cầu Quản trị viên chọn một tỉ lệ được hỗ trợ.
- Quản trị viên tiếp tục từ bước 5 của Luồng chính.

**EXC-03 — Tạo ảnh thất bại**
- Tại bước 9 của Luồng chính, nếu Hệ thống không tạo được ảnh, Hệ thống kết thúc trạng thái “Đang tạo ảnh”.
- Hệ thống không hiển thị ảnh kết quả.
- Hệ thống thông báo “Không thể tạo ảnh. Vui lòng thử lại.”
- Quản trị viên có thể thực hiện lại yêu cầu tạo ảnh.

**EXC-04 — Lưu ảnh thất bại**
- Tại bước 13 của Luồng chính, nếu Hệ thống không thể lưu đầy đủ ảnh hoặc thông tin liên quan, Hệ thống không ghi nhận dữ liệu lưu dở dang.
- Hệ thống không thông báo lưu thành công.
- Hệ thống thông báo “Không thể lưu ảnh. Vui lòng thử lại.”
- Quản trị viên có thể thực hiện lại thao tác lưu.

## 4. ACCEPTANCE CRITERIA
**AC-001**
- **Given:** Quản trị viên đã đăng nhập.
- **When:** Quản trị viên mở chức năng “Tạo ảnh đa tỷ lệ”.
- **Then:** Hệ thống hiển thị khu vực tải một ảnh nguồn và danh sách tỉ lệ được hỗ trợ.

**AC-002**
- **Given:** Quản trị viên đang sử dụng chức năng tạo ảnh đa tỷ lệ.
- **When:** Quản trị viên tải ảnh nguồn lên.
- **Then:** Hệ thống chỉ tiếp nhận một ảnh nguồn tại một thời điểm.

**AC-003**
- **Given:** Ảnh nguồn đáp ứng điều kiện về định dạng, dung lượng, chiều rộng và tỉ lệ.
- **When:** Hệ thống kiểm tra ảnh nguồn.
- **Then:** Hệ thống cho phép Quản trị viên tiếp tục chọn tỉ lệ.

**AC-004**
- **Given:** Ảnh nguồn không đáp ứng ít nhất một điều kiện về định dạng, dung lượng, chiều rộng hoặc tỉ lệ.
- **When:** Hệ thống kiểm tra ảnh nguồn.
- **Then:** Hệ thống không cho phép tiếp tục.
- **And:** Hệ thống hiển thị rõ điều kiện không hợp lệ.

**AC-005**
- **Given:** Quản trị viên đã tải lên một ảnh nguồn nhưng chưa chọn “Tạo ảnh”.
- **When:** Quản trị viên tải lên ảnh nguồn khác.
- **Then:** Hệ thống thay thế ảnh nguồn đang chọn.
- **And:** Hệ thống kiểm tra lại ảnh nguồn mới.

**AC-006**
- **Given:** Ảnh nguồn đã hợp lệ.
- **When:** Quản trị viên lựa chọn tỉ lệ.
- **Then:** Hệ thống chỉ cho phép chọn một tỉ lệ tại một thời điểm.

**AC-008**
- **Given:** Ảnh nguồn hợp lệ nhưng Quản trị viên chưa chọn tỉ lệ.
- **When:** Quản trị viên chọn “Tạo ảnh”.
- **Then:** Hệ thống không tạo ảnh.
- **And:** Hệ thống yêu cầu Quản trị viên chọn một tỉ lệ được hỗ trợ.

**AC-009**
- **Given:** Ảnh nguồn và tỉ lệ đã được chọn.
- **When:** Quản trị viên chọn “Tạo ảnh”.
- **Then:** Hệ thống kiểm tra lại ảnh nguồn trước khi bắt đầu tạo ảnh.

**AC-010**
- **Given:** Ảnh nguồn và tỉ lệ đã chọn đều hợp lệ.
- **When:** Hệ thống bắt đầu tạo ảnh.
- **Then:** Hệ thống hiển thị trạng thái “Đang tạo ảnh”.

**AC-011**
- **Given:** Một yêu cầu tạo ảnh đang được xử lý.
- **When:** Quản trị viên gửi lại yêu cầu với cùng ảnh nguồn và cùng tỉ lệ.
- **Then:** Hệ thống không tiếp nhận thêm yêu cầu trùng.

**AC-012**
- **Given:** Ảnh nguồn và tỉ lệ đã chọn đều hợp lệ.
- **When:** Quá trình tạo ảnh hoàn tất.
- **Then:** Hệ thống tạo đúng một ảnh theo tỉ lệ đã chọn.
- **And:** Hệ thống chỉ hiển thị một ảnh kết quả.

**AC-013**
- **Given:** Quản trị viên chưa hài lòng với ảnh kết quả.
- **When:** Quản trị viên chọn “Tạo lại”.
- **Then:** Hệ thống tạo một ảnh mới theo cùng tỉ lệ.
- **And:** Hệ thống thay ảnh đang hiển thị bằng ảnh vừa tạo lại.
- **And:** Hệ thống không giữ đồng thời ảnh cũ và ảnh mới.

**AC-014**
- **Given:** Hệ thống không thể tạo ảnh.
- **When:** Quá trình tạo ảnh kết thúc.
- **Then:** Hệ thống kết thúc trạng thái “Đang tạo ảnh”.
- **And:** Hệ thống không hiển thị ảnh kết quả.
- **And:** Hệ thống thông báo “Không thể tạo ảnh. Vui lòng thử lại.”

**AC-015**
- **Given:** Hệ thống đang hiển thị một ảnh kết quả.
- **When:** Quản trị viên chọn “Lưu”.
- **Then:** Hệ thống lưu ảnh kết quả.
- **And:** Hệ thống lưu tỉ lệ và kích thước của ảnh.
- **And:** Hệ thống lưu người tạo và thời gian tạo.

**AC-017**
- **Given:** Ảnh kết quả và toàn bộ thông tin liên quan đã được lưu thành công.
- **When:** Quá trình lưu hoàn tất.
- **Then:** Hệ thống chỉ thông báo “Lưu ảnh thành công”.

**AC-018**
- **Given:** Có lỗi khi lưu ảnh hoặc thông tin liên quan.
- **When:** Quá trình lưu không hoàn tất.
- **Then:** Hệ thống không ghi nhận dữ liệu lưu dở dang.
- **And:** Hệ thống không thông báo lưu thành công.
- **And:** Hệ thống thông báo “Không thể lưu ảnh. Vui lòng thử lại.”

**AC-016**
- **Given:** Hệ thống đã tạo xong ảnh kết quả
- **When:** Khi quản trị viên bấm nút "Lưu"
- **Then:** Hệ thống hiển thị modal nhập tên và mô tả cho ảnh
- **And:** Admin nhập tên bắt buộc cho ảnh, tối thiểu 1, tối đa 200 kí tự. Nhập mô tả cho ảnh, không bắt buộc, tối đa 1000 kí tự
- **And:** Admin bấm "Lưu"

## 5. REFERENCES
### Rules
- [BR-032](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/422cb56a-ad99-48c9-8598-7d518003905c)
- [BR-033](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/a35a33f1-0c45-4939-97fd-6c9b231db023)
- [BR-034](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/975e2c2e-6677-4299-a158-632a1a8a7317)
- [BR-035](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/9e4b0677-442e-4a96-9a9e-ff0f17eabf9f)
- [BR-036](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/bfeef817-f4cd-4491-b81b-8fde08a6a752)
- [BR-037](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/861df50a-8c52-4214-bd53-aaa6e06fd86a)
- [BR-050](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/a3b24a75-f5c8-430c-8b8a-70d840b1ee7a)
- [BR-051](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/39702271-8c70-45ec-befc-8dd7e5b20aa0)

### NON-FUNCTIONAL
- Phải hiển thị trạng thái Đang tạo ảnh trong thời gian AI xử lý
- Không được ghi đè ảnh core
- Mỗi ảnh kết quả phải liên kết đúng tỷ lệ tương ứng
- Không cho gửi lặp nhiều yêu cầu trong khi cùng một yêu cầu đang xử lý

### OUT OF SCOPE
Không có

### Rule Details

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Story liên quan |
|---|---|---|---|---|---|---|---|
| BR-032 | Định dạng file ảnh gốc (Core Image) | AI Image | Ảnh đầu vào cung cấp cho AI để xử lý phải đạt chuẩn định dạng. | Khi tải ảnh gốc lên chức năng sinh ảnh AI. | Hệ thống kiểm tra và chỉ cho phép JPG, JPEG, PNG. | Không | STORY-003 |
| BR-033 | Dung lượng ảnh gốc tối đa | AI Image | Giới hạn dung lượng để tối ưu chi phí xử lý AI. | Khi tải ảnh gốc lên chức năng sinh ảnh AI. | Hệ thống chặn upload và cảnh báo nếu file > 10MB. | Không | STORY-003 |
| BR-034 | Hỗ trợ các tỷ lệ sinh ảnh AI | AI Image | Hệ thống chỉ hỗ trợ sinh ảnh theo các tỷ lệ khung hình đã được quy chuẩn. | Khi chọn cấu hình tỷ lệ và gọi AI. | Chỉ cho phép chọn một trong các tỷ lệ: 1:1, 4:5, 9:16, 16:9, 2:1. | Không | STORY-003 |
| BR-035 | Ràng buộc giữ vùng an toàn (Safe Zone) | AI Image | Ảnh được sinh ra không được làm mất chủ thể chính của ảnh gốc. | Khi AI trả kết quả hình ảnh. | Hệ thống giữ sản phẩm, logo và nội dung chính trong vùng hiển thị an toàn theo cấu hình được phê duyệt. Kích thước vùng an toàn cần được chốt trước khi đưa rule sang trạng thái hiệu lực. | Không | STORY-003 |
| BR-036 | Độ dài tên hình ảnh lưu trữ | AI Image | Tên ảnh phải được định danh với độ dài quy định. | Khi lưu ảnh do AI sinh ra vào thư viện. | Độ dài trường Tên ảnh phải từ 1 đến 200 ký tự. | Không | STORY-003 |
| BR-037 | Độ dài mô tả hình ảnh | AI Image | Mô tả ảnh có thể để trống hoặc điền tối đa 1000 ký tự. | Khi lưu thông tin mô tả cho hình ảnh. | Hệ thống chặn lưu nếu text mô tả vượt quá 1000 ký tự. | Không | STORY-003 |
| BR-050 | Một yêu cầu sinh ảnh chỉ có một ảnh nguồn và một tỷ lệ | AI Image | Mỗi lần sinh ảnh chỉ xử lý một ảnh nguồn và một tỷ lệ được chọn. | Khi Quản trị viên gửi yêu cầu tạo ảnh đa tỷ lệ. | Hệ thống chỉ tiếp nhận một ảnh nguồn, một tỷ lệ và trả về đúng một ảnh kết quả cho yêu cầu đó. Tạo lại kết quả vẫn sử dụng cùng ảnh nguồn và tỷ lệ cho đến khi Quản trị viên thay đổi đầu vào. | Không | STORY-003 |
| BR-051 | Lưu toàn vẹn ảnh kết quả và thông tin liên quan | AI Image | Ảnh kết quả và toàn bộ thông tin quản lý phải được lưu đầy đủ hoặc không lưu. | Khi Quản trị viên lưu ảnh do AI tạo. | Hệ thống lưu đồng thời ảnh, tên, mô tả, tỷ lệ, kích thước, người tạo và thời gian tạo; ảnh phải liên kết đúng tỷ lệ và không ghi đè ảnh nguồn. Nếu bất kỳ dữ liệu nào không lưu được, hệ thống không giữ dữ liệu dở dang và không thông báo thành công. | Không | STORY-003 |
