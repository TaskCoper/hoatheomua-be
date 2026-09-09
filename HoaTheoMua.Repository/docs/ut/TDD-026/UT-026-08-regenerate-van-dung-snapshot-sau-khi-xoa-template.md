# UT-026-08

## Thông tin tài liệu

- **Tiêu đề:** Regenerate vẫn dùng snapshot sau khi template nguồn đã xóa
- **Ghi chú:** Template live bị xóa sau khi Card AI được tạo không được làm hỏng khả năng regenerate từ lịch sử đã lưu.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-026-08
- **Phiên bản:** 0.2
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CARD - Regenerate Card AI
- **Unit under test:** `CardService.RegenerateAsync`
- **Loại:** Positive / Regression
- **Mục tiêu:** Xác nhận regenerate dựa vào snapshot của Card/History nguồn và không truy vấn trạng thái template hiện tại.
- **Precondition / Mock setup**:
  - Người dùng đã đăng nhập, sở hữu Card AI nguồn và còn đủ quota theo TDD-007.
  - Card/History nguồn lưu đầy đủ prompt, cấu hình và dữ liệu cần để sinh lại.
  - Card Template từng tạo nguồn hiện có isDeleted = true.
- **Dữ liệu đại diện**:
  - Không truyền cardTemplateId mới là dữ liệu chính; truyền đúng ID template nguồn là biến thể đại diện nhưng vẫn phải ưu tiên snapshot đã lưu.
  - Template live đã xóa là trạng thái quan trọng; loại bỏ hẳn bản ghi live cũng không được làm thay đổi dữ liệu snapshot đã tồn tại nếu luồng không cần truy vấn nó.
- **Input**:
  - Gọi regenerate Card AI nguồn; lần chính không truyền cardTemplateId.
- **Các bước thực hiện**:
  - Cho service tải Card và History nguồn của đúng owner.
  - Cho service dựng request sinh lại từ snapshot, không gọi repository lấy template live.
  - Cho AI trả kết quả thành công và kiểm tra Card/History mới.
- **Expected output (bắt buộc)**:
  - Luồng tiếp tục theo TDD-007 và không trả CARD_TEMPLATE_DELETED hoặc CARD_TEMPLATE_NOT_FOUND.
  - Không có truy vấn trạng thái Card Template live trong quyết định regenerate.
  - AI nhận dữ liệu từ snapshot nguồn; tạo Card AI và History mới theo giao dịch/quota của TDD-007.
  - Card/History cũ và template đã xóa vẫn giữ nguyên.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Dữ liệu lịch sử phải tự đủ để tái tạo; nếu phụ thuộc template live, xóa quản trị sẽ làm hỏng chức năng của Card đã phát hành.

## TEST_LINKS

- [TDD-026 - Xóa Card Template](../../tdd/TDD-026/TDD-026-xoa-card-template.md)
- BR-022-09
- Đối chiếu TDD-007 - Tạo lại thiệp từ lịch sử
