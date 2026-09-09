# UT-011-03

## Thông tin tài liệu

- **Tiêu đề:** Chặn Card mới khi Size Config đã bị xóa
- **Ghi chú:** Các flow tạo Card cần Size Config live để kiểm tra size, giới hạn từ và lưu snapshot.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-011-03
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CARD - Admission theo Card Config
- **Unit under test:** `CardService.CreateAsync`
- **Loại:** Error
- **Mục tiêu:** Trả lỗi Gone trước khi gọi AI hoặc lưu Card nếu singleton kích thước đã bị xóa.
- **Precondition / Mock setup**:
  - Người dùng tạo Card đã đăng nhập; request hợp lệ đến bước tải cấu hình.
  - Exact Config card_config.sizes tồn tại với IsDeleted=true.
  - Mock AI và repository Card/History được theo dõi để xác nhận không bị gọi.
- **Dữ liệu đại diện**:
  - Luồng tạo Card AI của TDD-006 là dữ liệu chính; tạo HandMade tại TDD-021 và các flow Card mới khác cần Size Config đại diện cùng dependency.
  - Mỗi flow phải được chạy riêng vì dữ liệu đầu vào khác nhau, nhưng đều dừng tại cùng trạng thái singleton deleted.
  - Size key trong request vẫn hợp lệ về tên; lỗi đến từ bản thân Config đã xóa, không phải CARD_SIZE_NOT_FOUND.
- **Input**:
  - POST endpoint tạo Card hiện có với sizeKey=S và các trường còn lại hợp lệ.
- **Các bước thực hiện**:
  - Gọi flow tạo Card với Size Config deleted.
  - Cho service tải singleton và kiểm tra IsDeleted trước schema hoặc nghiệp vụ phía sau.
  - Theo dõi AI, Card và History.
- **Expected output (bắt buộc)**:
  - HTTP 410 với messageCode=CARD_SIZE_CONFIG_DELETED.
  - Không chuyển thành NOT_FOUND hoặc CARD_SIZE_NOT_FOUND vì record tồn tại nhưng đã bị xóa.
  - Không gọi AI, không trừ quota và không persist Card/History.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Lỗi chuyên biệt cho biết dependency từng tồn tại nhưng không còn được dùng, đồng thời ngăn tạo Card thiếu snapshot size.

## TEST_LINKS

- [TDD-011 - Xóa Config có hỗ trợ Card Config](../../tdd/TDD-011/TDD-011-xoa-card-config.md)
- BR-011-06
- Đối chiếu TDD-006, TDD-021 và TDD-027

