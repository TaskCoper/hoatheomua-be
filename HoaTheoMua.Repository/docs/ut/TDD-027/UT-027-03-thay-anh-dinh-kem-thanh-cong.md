# UT-027-03

## Thông tin tài liệu

- **Tiêu đề:** Thay ảnh đính kèm thành công
- **Ghi chú:** Kiểm tra URL ảnh mới được validate và đưa vào snapshot hiệu lực.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-03
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** `CardService.RecreateContentAsync`
- **Loại:** Positive
- **Mục tiêu:** Xác nhận ảnh đính kèm mới thay ảnh nguồn nhưng không thay ảnh tham chiếu thiết kế.
- **Precondition / Mock setup**:
  - Card/History nguồn, prompt và các snapshot đều hợp lệ.
  - URL ảnh mới vượt qua kiểm tra URL/upload.
  - AIModule và persistence thành công.
- **Dữ liệu đại diện**:
  - Một URL PNG hợp lệ đại diện cho các định dạng ảnh được Card Create chấp nhận.
- **Input**:
  - Chỉ truyền `attached_image_url=https://storage.example.com/uploads/new-photo.png`.
- **Các bước thực hiện**:
  - Gọi recreate-content và kiểm tra snapshot gửi AIModule.
- **Expected output (bắt buộc)**:
  - HTTP 200.
  - `attached_image_url` hiệu lực và Card mới bằng URL mới.
  - Các text field và form type bị bỏ được lấy từ Card nguồn.
  - Ảnh tham chiếu thiết kế vẫn là `raw_image` của Card nguồn, không phải ảnh đính kèm hoặc `image_url` nguồn.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** Ảnh đính kèm và ảnh tham chiếu thiết kế có vai trò khác nhau, cần tránh dùng nhầm.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-10, BR-027-14
