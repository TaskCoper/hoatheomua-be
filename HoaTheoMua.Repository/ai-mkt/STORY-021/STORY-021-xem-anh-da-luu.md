# STORY-021: Xem ảnh đã lưu lại

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Nháp - Cập nhật: 04/09/2026)

**Author:** Nguyễn Anh Quân
**Reviewer:** Nguyễn Đức Bình
**Approver:** Chưa chỉ định
**Owner:** Nguyễn Anh Quân

**Feedback yêu cầu sửa gần nhất:**
> NFR và Out of Scope không có? Ảnh do STORY-003 sinh ra không có tên nhưng tìm kiếm ở đây chỉ theo tên? Cái này thuộc SP S1
> *- Nguyễn Đức Bình · 10:02 03/09/2026*

**Thống kê:** Rules: 3 | Unit Tests: 0 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn xem danh sách và chi tiết ảnh đã lưu để kiểm tra và lựa chọn hình ảnh phù hợp khi cần
- **Context:** Admin cần tra cứu các ảnh đã lưu từ quá trình tải lên hoặc tạo tự động. Hệ thống phải hiển thị ảnh thu nhỏ, thông tin cơ bản và bản xem chi tiết truy cập. Chức năng này chỉ bao gồm xem dữ liệu ảnh đã lưu, không bao gồm tạo mới, chỉnh sửa, xóa, tải xuống hoặc sử dụng ảnh trong bài đăng
- **Sprint:** S1
- **Priority:** Must
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Admin đã đăng nhập

### Trigger
- Admin chọn chức năng “Ảnh đã lưu”

## 3. FLOW
### Main Flow: Xem ảnh đã lưu
1. Quản trị viên mở chức năng “Ảnh đã lưu”.
2. Hệ thống tải danh sách ảnh thuộc phạm vi Quản trị viên được phép truy cập.
3. Hệ thống sắp xếp danh sách theo thời gian cập nhật từ mới nhất đến cũ nhất.
4. Hệ thống hiển thị ảnh thu nhỏ, tên ảnh, tỷ lệ ảnh, kích thước ảnh theo chiều rộng và chiều cao, định dạng tệp và thời gian cập nhật của từng ảnh.
5. Quản trị viên chọn một ảnh trong danh sách.
6. Hệ thống kiểm tra ảnh còn tồn tại, còn tồn tại trong hệ thống và tải ảnh cùng thông tin chi tiết.
7. Hệ thống hiển thị bản xem lớn và các thông tin gồm tên ảnh, mô tả, tỷ lệ ảnh, kích thước ảnh theo chiều rộng và chiều cao, định dạng tệp, dung lượng tệp, nguồn tạo, người tạo, thời gian tạo, người cập nhật, thời gian cập nhật và các phiên bản theo tỷ lệ hiện có.
8. Thông tin chi tiết được hiển thị ở chế độ chỉ xem.
9. Quản trị viên đóng màn hình chi tiết. Hệ thống quay lại danh sách và giữ nguyên từ khóa tìm kiếm cùng các điều kiện lọc trước đó.

### Alternative Flow
**ALT-01 — Không có ảnh đã lưu**
- Hệ thống không tìm thấy ảnh nào trong phạm vi Quản trị viên được phép truy cập.
- Hệ thống hiển thị trạng thái danh sách trống.
- Hệ thống hướng dẫn Quản trị viên tải lên hoặc tạo ảnh.
- Luồng kết thúc.

**ALT-02 — Tìm kiếm ảnh theo tên**
- Danh sách ảnh đã lưu đang hiển thị.
- Quản trị viên nhập từ khóa theo tên ảnh và thực hiện tìm kiếm.
- Hệ thống tìm các ảnh có tên phù hợp với từ khóa.
- Hệ thống hiển thị danh sách kết quả theo thời gian cập nhật từ mới nhất đến cũ nhất.

**ALT-03 — Lọc danh sách ảnh**
- Danh sách ảnh đã lưu đang hiển thị.
- Quản trị viên chọn một hoặc nhiều điều kiện lọc gồm định dạng tệp, khoảng thời gian lưu hoặc cập nhật và người thực hiện lưu hoặc cập nhật ảnh.
- Hệ thống áp dụng đồng thời tất cả điều kiện đã chọn.
- Hệ thống hiển thị danh sách ảnh phù hợp theo thời gian cập nhật từ mới nhất đến cũ nhất.

**ALT-04 — Không có kết quả phù hợp**
- Không có ảnh nào phù hợp với từ khóa tìm kiếm hoặc các điều kiện lọc.
- Hệ thống hiển thị trạng thái không có kết quả.
- Hệ thống giữ nguyên từ khóa và các điều kiện lọc để Quản trị viên có thể điều chỉnh.
- Hệ thống không hiển thị trạng thái này như lỗi tải dữ liệu.

**ALT-05 — Xóa điều kiện tìm kiếm và lọc**
- Quản trị viên đang xem danh sách đã áp dụng từ khóa tìm kiếm hoặc điều kiện lọc.
- Quản trị viên xóa từ khóa và tất cả điều kiện lọc.
- Hệ thống tải lại danh sách ảnh mặc định trong hệ thống.
- Danh sách được sắp xếp theo thời gian cập nhật từ mới nhất đến cũ nhất.

### Exception Flow
**EXC-01 — Không thể tải danh sách ảnh**
- Hệ thống không thể tải đầy đủ danh sách ảnh.
- Hệ thống không hiển thị danh sách chứa dữ liệu thiếu.
- Hệ thống thông báo “Không thể tải danh sách ảnh, vui lòng thử lại”.
- Quản trị viên được phép tải lại hoặc quay về màn hình trước.

**EXC-02 — Ảnh không còn khả dụng**
- Ảnh đã bị xóa hoặc không còn thuộc phạm vi Quản trị viên được phép truy cập trước khi mở chi tiết.
- Hệ thống không hiển thị thông tin chi tiết của ảnh.
- Hệ thống thông báo ảnh không còn khả dụng.
- Hệ thống tải lại danh sách và giữ nguyên từ khóa tìm kiếm cùng các điều kiện lọc còn phù hợp.

**EXC-03 — Không thể tải thông tin chi tiết của ảnh**
- Hệ thống không thể tải đầy đủ ảnh hoặc thông tin chi tiết của ảnh đã chọn.
- Hệ thống không hiển thị thông tin chi tiết chứa dữ liệu thiếu.
- Hệ thống giữ nguyên trạng thái danh sách trước khi Quản trị viên chọn ảnh.
- Hệ thống thông báo “Không thể tải thông tin chi tiết của ảnh, vui lòng thử lại”.
- Quản trị viên được phép thử lại hoặc quay về danh sách.

**EXC-04 — Không thể tải ảnh thu nhỏ**
- Hệ thống không thể tải ảnh thu nhỏ của một ảnh trong danh sách.
- Hệ thống hiển thị hình ảnh thay thế tại vị trí tương ứng.
- Các ảnh và thông tin khác trong danh sách vẫn được hiển thị bình thường.
- Quản trị viên vẫn được phép chọn ảnh để thử mở thông tin chi tiết.

**EXC-05 — Phiên đăng nhập hết hạn**
- Phiên đăng nhập hết hạn khi Quản trị viên đang xem danh sách hoặc thông tin chi tiết của ảnh.
- Hệ thống dừng tải dữ liệu được bảo vệ.
- Hệ thống không tiếp tục hiển thị dữ liệu ảnh chưa được tải.
- Hệ thống thông báo phiên đăng nhập đã hết hạn.
- Hệ thống yêu cầu Quản trị viên đăng nhập lại.

**EXC-06 — Không thể tải kết quả tìm kiếm hoặc lọc**
- Hệ thống không thể tải kết quả sau khi Quản trị viên thực hiện tìm kiếm hoặc lọc.
- Hệ thống không thay thế danh sách hiện tại bằng dữ liệu thiếu.
- Hệ thống giữ nguyên từ khóa và các điều kiện lọc đã nhập.
- Hệ thống thông báo “Không thể tải kết quả, vui lòng thử lại”.
- Quản trị viên được phép thử lại.

## 4. ACCEPTANCE CRITERIA
**AC-001 — Hiển thị danh sách ảnh đã lưu**
- **Given:** .
- **When:** Quản trị viên mở chức năng “Ảnh đã lưu”.
- **Then:** Hệ thống hiển thị danh sách ảnh theo thời gian cập nhật từ mới nhất đến cũ nhất.
- **And:** Mỗi ảnh hiển thị ảnh thu nhỏ, tên ảnh, tỷ lệ ảnh, kích thước ảnh theo chiều rộng và chiều cao, định dạng tệp và thời gian cập nhật.
- **And:** Hệ thống chỉ hiển thị các ảnh thuộc phạm vi Quản trị viên được phép truy cập.

**AC-002 — Hiển thị thông tin chi tiết của ảnh**
- **Given:** Danh sách ảnh đã lưu đang hiển thị và ảnh được chọn còn khả dụng.
- **When:** Quản trị viên chọn một ảnh.
- **Then:** Hệ thống hiển thị bản xem lớn và đầy đủ thông tin của ảnh.
- **And:** Thông tin hiển thị gồm tên ảnh, mô tả, tỷ lệ ảnh, kích thước ảnh, định dạng tệp, dung lượng tệp, nguồn tạo, người tạo, thời gian tạo, người cập nhật, thời gian cập nhật và các phiên bản theo tỷ lệ hiện có.
- **And:** Thông tin chi tiết được hiển thị ở chế độ chỉ xem.

**AC-003 — Giữ trạng thái danh sách khi đóng chi tiết**
- **Given:** Quản trị viên đã mở thông tin chi tiết từ một danh sách có từ khóa tìm kiếm hoặc điều kiện lọc.
- **When:** Quản trị viên đóng màn hình chi tiết.
- **Then:** Hệ thống quay lại danh sách ảnh.
- **And:** Từ khóa tìm kiếm, các điều kiện lọc và thứ tự danh sách trước đó được giữ nguyên.

**AC-004 — Không có ảnh đã lưu**
- **Given:** Không có ảnh nào trong phạm vi Quản trị viên được phép truy cập.
- **When:** Quản trị viên mở chức năng “Ảnh đã lưu”.
- **Then:** Hệ thống hiển thị trạng thái danh sách trống.
- **And:** Hệ thống không hiển thị trạng thái danh sách trống như lỗi tải dữ liệu.
- **And:** Hệ thống hiển thị hướng dẫn tải lên hoặc tạo ảnh.

**AC-005 — Tìm kiếm ảnh theo tên**
- **Given:** Danh sách ảnh đã lưu đang hiển thị.
- **When:** Quản trị viên nhập từ khóa theo tên ảnh và thực hiện tìm kiếm.
- **Then:** Hệ thống hiển thị các ảnh có tên phù hợp với từ khóa.
- **And:** Kết quả được sắp xếp theo thời gian cập nhật từ mới nhất đến cũ nhất.

**AC-006 — Lọc danh sách ảnh**
- **Given:** Danh sách ảnh đã lưu đang hiển thị.
- **When:** Quản trị viên chọn một hoặc nhiều điều kiện lọc.
- **Then:** Hệ thống chỉ hiển thị các ảnh đáp ứng tất cả điều kiện đã chọn.
- **And:** Các điều kiện lọc gồm định dạng tệp, khoảng thời gian lưu hoặc cập nhật và người thực hiện lưu hoặc cập nhật ảnh.

**AC-007 — Không có kết quả phù hợp**
- **Given:** Quản trị viên đã nhập từ khóa hoặc chọn điều kiện lọc.
- **When:** Không có ảnh nào phù hợp.
- **Then:** Hệ thống hiển thị trạng thái không có kết quả.
- **And:** Hệ thống giữ nguyên từ khóa và các điều kiện lọc để Quản trị viên có thể điều chỉnh.
- **And:** Hệ thống không hiển thị trạng thái này như lỗi tải dữ liệu.

**AC-008 — Xóa điều kiện tìm kiếm và lọc**
- **Given:** Danh sách đang áp dụng từ khóa tìm kiếm hoặc điều kiện lọc.
- **When:** Quản trị viên xóa từ khóa và tất cả điều kiện lọc.
- **Then:** Hệ thống hiển thị lại danh sách ảnh mặc định trong hệ thống.
- **And:** Danh sách được sắp xếp theo thời gian cập nhật từ mới nhất đến cũ nhất.

**AC-009 — Không thể tải danh sách ảnh**
- **Given:** .
- **When:** Hệ thống không thể tải đầy đủ danh sách ảnh.
- **Then:** Hệ thống không hiển thị danh sách chứa dữ liệu thiếu.
- **And:** Hệ thống thông báo không thể tải danh sách ảnh.
- **And:** Quản trị viên được phép tải lại hoặc quay về màn hình trước.

**AC-010 — Ảnh không còn khả dụng**
- **Given:** Một ảnh đã bị xóa hoặc không còn tồn tại trong hệ thống.
- **When:** Quản trị viên chọn ảnh đó từ danh sách.
- **Then:** Hệ thống không hiển thị thông tin chi tiết của ảnh.
- **And:** Hệ thống thông báo ảnh không còn khả dụng.
- **And:** Hệ thống tải lại danh sách và giữ nguyên các điều kiện tìm kiếm hoặc lọc còn phù hợp.

**AC-011 — Không thể tải thông tin chi tiết của ảnh**
- **Given:** Quản trị viên đã chọn một ảnh còn khả dụng.
- **When:** Hệ thống không thể tải đầy đủ ảnh hoặc thông tin chi tiết.
- **Then:** Hệ thống không hiển thị thông tin chi tiết chứa dữ liệu thiếu.
- **And:** Trạng thái danh sách trước đó được giữ nguyên.
- **And:** Hệ thống thông báo không thể tải thông tin chi tiết và cho phép Quản trị viên thử lại.

**AC-012 — Không thể tải ảnh thu nhỏ**
- **Given:** Danh sách ảnh đã lưu đang hiển thị.
- **When:** Hệ thống không thể tải ảnh thu nhỏ của một ảnh.
- **Then:** Hệ thống hiển thị hình ảnh thay thế tại vị trí tương ứng.
- **And:** Các ảnh và thông tin khác trong danh sách vẫn được hiển thị bình thường.
- **And:** Quản trị viên vẫn được phép chọn ảnh để thử mở thông tin chi tiết.

**AC-013 — Phiên đăng nhập hết hạn**
- **Given:** Quản trị viên đang xem danh sách hoặc thông tin chi tiết của ảnh.
- **When:** Phiên đăng nhập hết hạn.
- **Then:** Hệ thống dừng tải dữ liệu được bảo vệ.
- **And:** Hệ thống thông báo phiên đăng nhập đã hết hạn.
- **And:** Hệ thống yêu cầu Quản trị viên đăng nhập lại.

**AC-014 — Không thể tải kết quả tìm kiếm hoặc lọc**
- **Given:** Quản trị viên đã nhập từ khóa hoặc chọn điều kiện lọc.
- **When:** Hệ thống không thể tải kết quả.
- **Then:** Hệ thống không thay thế danh sách hiện tại bằng dữ liệu thiếu.
- **And:** Hệ thống giữ nguyên từ khóa và các điều kiện lọc đã nhập.
- **And:** Hệ thống thông báo không thể tải kết quả và cho phép Quản trị viên thử lại.

## 5. REFERENCES
### Rules
- [BR-038](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/a474fc1f-80af-466d-b19c-a026976db2d2)
- [BR-063](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/f377e9a9-7114-47cc-885c-6b0934720ec7)
- [BR-064](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/1a495c4d-33e8-423a-9116-ce2b21e362e4)

### NON-FUNCTIONAL
- Danh sách ảnh phải hiển thị trong vòng 2 giây trong điều kiện hoạt động bình thường.
- Chi tiết ảnh phải hiển thị trong vòng 2 giây sau khi Admin chọn ảnh.
- Ảnh thu nhỏ được tải theo từng trang hoặc khi xuất hiện trên màn hình để tránh tải đồng thời toàn bộ thư viện.

### OUT OF SCOPE
- Tải ảnh mới lên hệ thống.
- Chỉnh sửa ảnh như cắt, xoay, thay đổi kích thước hoặc định dạng.
- Chọn hoặc gắn ảnh vào content, lịch đăng bài hoặc bài đăng.
- Quản lý lịch sử hoặc khôi phục phiên bản ảnh.

### Rule Details

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Story liên quan |
|---|---|---|---|---|---|---|---|
| BR-038 | Sắp xếp danh sách Ảnh lưu trữ | AI Image | Thư viện ảnh ưu tiên hiển thị ảnh mới tạo. | Khi mở trang danh sách Hình ảnh. | Hệ thống sắp xếp giảm dần theo thời gian cập nhật và hiển thị ảnh mới cập nhật trước. | Không | STORY-021 |
| BR-063 | Tìm kiếm và lọc ảnh đã lưu | AI Image | Danh sách ảnh hỗ trợ tìm theo tên và lọc theo định dạng. | Khi Quản trị viên nhập từ khóa hoặc áp dụng một hay nhiều điều kiện lọc. | Hệ thống hiển thị các ảnh đáp ứng từ khóa và tất cả điều kiện đã chọn, sắp xếp theo thời gian cập nhật mới nhất. Khi xóa toàn bộ điều kiện, hệ thống hiển thị lại danh sách mặc định. | Không | STORY-021 |
| BR-064 | Chi tiết ảnh ở chế độ chỉ xem | AI Image | Màn hình chi tiết ảnh chỉ hiển thị dữ liệu và không cho phép sửa trực tiếp. | Khi Quản trị viên mở một ảnh trong danh sách ảnh đã lưu. | Hệ thống hiển thị ảnh lớn, tên, mô tả, tỷ lệ, kích thước, định dạng, dung lượng, nguồn tạo, người và thời gian tạo hoặc cập nhật cùng các phiên bản tỷ lệ. Không hiển thị hoặc quản lý thẻ ảnh. | Không | STORY-021 |
