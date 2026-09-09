# STORY-018: Xem content đã lưu lại

**Trạng thái:** Cần làm | **Phiên bản:** v0.1 (Nháp - Cập nhật: 04/09/2026)

**Author:** Nguyễn Anh Quân
**Reviewer:** Nguyễn Đức Bình
**Approver:** Chưa chỉ định
**Owner:** Nguyễn Anh Quân

**Feedback yêu cầu sửa gần nhất:**
> Sai định dạng ở AC 007, 008, 009 Cần làm rõ Content có version history hay không?
> *- Nguyễn Đức Bình · 09:57 03/09/2026*

**Thống kê:** Rules: 5 | Unit Tests: 0 | System Tests: 0

---

## 1. METADATA
- **Story:** Là một Quản trị viên, tôi muốn xem danh sách và chi tiết content đã lưu để kiểm tra và sử dụng lại nội dung khi cần
- **Context:** Admin cần tra cứu các content và hashtag đã lưu trong quá trình tạo nội dung. Hệ thống phải hiển thị danh sách, hỗ trợ tìm kiếm và cho phép xem đầy đủ thông tin của từng content. Chức năng này chỉ bao gồm xem dữ liệu đã lưu, không bao gồm tạo mới, chỉnh sửa, xóa, lên lịch hoặc đăng bài
- **Sprint:** S2
- **Priority:** Must
- **Assignee:** FE Nguyễn Anh Quân, BE Nguyễn Anh Quân
- **Creator:** Nguyễn Anh Quân
- **Status:** Cần làm

## 2. CONDITIONS
### Preconditions
- Admin đã đăng nhập
### Trigger
- Admin chọn chức năng “Content đã lưu”

## 3. FLOW
### Main Flow: Xem content đã lưu
1. Admin mở chức năng “Content đã lưu”
2. Hệ thống hiển thị và sắp xếp danh sách các content được lưu theo thời gian cập nhật mới nhất
3. Hệ thống hiển thị tiêu đề, nội dung rút gọn, hashtag, nền tảng và thời gian cập nhật của từng content. Nội dung rút gọn là phần đầu của nội dung gốc được cắt theo giới hạn hiển thị.
4. Admin chọn một content trong danh sách
5. Hệ thống truy xuất và hiển thị phiên bản mới nhất đã được lưu của content mà Admin đã chọn
6. Hệ thống hiển thị đầy đủ nội dung, hashtag, nền tảng, người tạo và thời gian tạo hoặc cập nhật
7. Admin đóng chi tiết và hệ thống quay lại danh sách với điều kiện tìm kiếm hoặc lọc được giữ nguyên

### Alternative Flow
**ALT-01 — Không có content đã lưu**
- Tại bước 3, nếu không có content, hệ thống không hiển thị danh sách
- Hệ thống hiển thị trạng thái trống và hướng dẫn Admin tạo hoặc lưu content

**ALT-02 — Tìm kiếm hoặc lọc content**
- Tại bước 5, Admin có thể tìm kiếm hoặc lọc content theo: Từ khóa theo tiêu đề hoặc nội dung, Hashtag, Thời gian cập nhật và nền tảng.
- Hệ thống hiển thị danh sách content phù hợp với điều kiện tìm kiếm/lọc đã chọn.

### Exception Flow
**EXC-01 — Không thể tải danh sách**
- Tại bước 3, nếu hệ thống không thể tải danh sách, hệ thống không hiển thị dữ liệu không đầy đủ
- Hệ thống thông báo “Không thể tải danh sách content, vui lòng thử lại” và cho phép tải lại

**EXC-02 — Content không còn khả dụng**
- Tại bước 7, nếu content đã bị xóa hoặc không còn tồn tại trong hệ thống, hệ thống không mở chi tiết
- Hệ thống thông báo content không còn khả dụng và tải lại danh sách

**EXC-03 — Không thể tải chi tiết**
- Tại bước 7, nếu xảy ra lỗi kỹ thuật, hệ thống giữ nguyên trạng thái danh sách và không hiển thị dữ liệu không đầy đủ
- Hệ thống thông báo “Không thể tải chi tiết content, vui lòng thử lại”

## 4. ACCEPTANCE CRITERIA
**AC-001**
- **Given:** có content đã lưu trong phạm vi được truy cập
- **When:** Admin mở chức năng “Content đã lưu”
- **Then:** Hệ thống hiển thị danh sách theo thời gian cập nhật mới nhất
- **And:** Mỗi dòng hiển thị tiêu đề, nội dung rút gọn, hashtag, nền tảng và thời gian cập nhật

**AC-002**
- **Given:** Danh sách content đã lưu đang hiển thị
- **When:** Admin chọn một content
- **Then:** Hệ thống hiển thị đầy đủ nội dung, hashtag và thông tin liên quan
- **And:** Hệ thống hiển thị dữ liệu mới nhất đã được lưu của content và ở chế độ chỉ xem

**AC-003**
- **Given:** Không có content đã lưu trong phạm vi Admin được truy cập
- **When:** Admin mở chức năng “Content đã lưu”
- **Then:** Hệ thống hiển thị trạng thái danh sách trống
- **And:** Hệ thống hiển thị hướng dẫn tạo hoặc lưu content

**AC-004**
- **Given:** Danh sách content đã lưu đang hiển thị
- **When:** Admin nhập từ khóa hoặc chọn điều kiện lọc
- **Then:** Hệ thống hiển thị kết quả phù hợp và tổng số kết quả
- **And:** Khi xóa từ khóa và bộ lọc, hệ thống hiển thị lại danh sách mặc định

**AC-005 - Lỗi tải danh sách**
- **Given:** Admin đang mở màn hình danh sách content đã lưu
- **When:** Hệ thống không thể tải danh sách content do lỗi hệ thống
- **Then:** Hệ thống thông báo không thể tải danh sách content và cho phép Admin thực hiện lại thao tác

**AC-006 - Lỗi tải chi tiết**
- **Given:** Admin đã chọn một content trong danh sách
- **When:** Hệ thống không thể tải thông tin chi tiết của content do lỗi hệ thống
- **Then:** Hệ thống thông báo không thể tải thông tin chi tiết content và cho phép Admin thực hiện lại thao tác

**AC-007 - Admin tìm kiếm content**
- **Given:** Admin đang ở màn hình danh sách content
- **When:** Admin nhập từ khóa tìm kiếm theo tiêu đề hoặc nội dung
- **Then:** Hệ thống hiển thị các content phù hợp với từ khóa tìm kiếm
- **And:** Nếu không có content phù hợp, hệ thống hiển thị trạng thái không có kết quả

**AC-008 - Admin filter content**
- **Given:** Admin đang ở màn hình danh sách content
- **When:** Admin chọn một hoặc nhiều điều kiện lọc gồm hashtag, nền tảng hoặc thời gian cập nhật
- **Then:** Hệ thống hiển thị các content phù hợp với điều kiện lọc đã chọn

## 5. REFERENCES
### Rules
- [BR-023](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/d8931e9c-6c78-4fed-be86-63cc33ab6ee2)
- [BR-024](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/6d8234f3-f64e-4b81-8417-937717758ab5)
- [BR-030](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/915eefb3-ff1d-42fe-b434-a0fbf3da89d6)
- [BR-031](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/863f2aac-4f15-4862-8f19-d916ba861707)
- [BR-059](https://document-first.vnzdna.com/projects/11303185-e537-4531-bf3f-a90af664ff77/documents/9fa969b5-cc09-4247-b9a0-852771d49c18)

### NON-FUNCTIONAL
- Xem content còn tồn tại trong hệ thống

### OUT OF SCOPE
- Không bao gồm tạo content mới
- Không bao gồm chỉnh sửa nội dung hoặc hashtag
- Không bao gồm xóa content đã lưu
- Không bao gồm lên lịch hoặc đăng content
- Không bao gồm lưu, xem hoặc khôi phục lịch sử phiên bản của content.
- Hệ thống chỉ hiển thị nội dung hiện tại của content. Chức năng không bao gồm xem lịch sử phiên bản hoặc khôi phục nội dung cũ.

### Rule Details

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Story liên quan |
|---|---|---|---|---|---|---|---|
| BR-023 | Sắp xếp danh sách Content | AI Content | Content nào mới được sửa gần đây nhất sẽ hiển thị lên đầu. | Khi tải danh sách Content đã lưu. | Hệ thống sắp xếp giảm dần theo cột `Updated_At`. | Không | STORY-018 |
| BR-024 | Xem chi tiết Content (Read-only) | AI Content | Chế độ xem chi tiết không cho phép can thiệp dữ liệu trực tiếp. | Khi click vào xem chi tiết một Content. | Hệ thống mở popup/page chi tiết ở chế độ disabled mọi input field. | Không | STORY-018 |
| BR-030 | Tìm kiếm Content theo từ khóa | AI Content | Hỗ trợ tìm kiếm theo chuỗi ký tự trong nội dung bài viết. | Khi nhập từ khóa tìm kiếm tại màn hình Quản lý Content. | Hệ thống trả về các Content có tiêu đề hoặc nội dung phù hợp với từ khóa. | Không | STORY-018 |
| BR-031 | Lọc Content theo Hashtag | AI Content | Cho phép gom nhóm và tìm các bài viết dùng chung hashtag chiến dịch. | Khi chọn/nhập hashtag vào bộ lọc. | Hệ thống trả về danh sách các Content có gắn hashtag tương ứng. | Không | STORY-018 |
| BR-059 | Bộ lọc Content đã lưu | AI Content | Danh sách Content được phép lọc theo hashtag, nền tảng và thời gian cập nhật. | Khi Quản trị viên áp dụng một hoặc nhiều điều kiện lọc. | Hệ thống chỉ hiển thị các Content đáp ứng tất cả điều kiện đã chọn và khôi phục danh sách mặc định khi xóa bộ lọc. | Không | STORY-018 |
