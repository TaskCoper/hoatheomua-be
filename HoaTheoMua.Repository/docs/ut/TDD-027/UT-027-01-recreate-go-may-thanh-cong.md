# UT-027-01

## Thông tin tài liệu

- **Tiêu đề:** Tạo Card AI mới dạng gõ máy thành công
- **Ghi chú:** Kiểm tra đường đi thành công khi người dùng đổi nội dung, giữ thiết kế nguồn và chủ động bỏ ảnh đính kèm.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-01
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** `CardService.RecreateContentAsync`
- **Loại:** Positive / Smoke
- **Mục tiêu:** Xác nhận hệ thống tạo Card và History mới từ ảnh gốc, giữ Size/Template snapshot và không thay đổi Card nguồn.
- **Precondition / Mock setup**:
  - Người dùng đã đăng nhập và sở hữu Card AI nguồn.
  - Card nguồn có `raw_image`, `image_url`, nội dung và Size/Template snapshot đầy đủ.
  - History nguồn có `type=card`, `base_id` và `root` hợp lệ; prompt `card` hiện hành hợp lệ.
  - AIModule trả đủ `raw_image` và `image_url` mới; giao dịch lưu thành công.
- **Dữ liệu đại diện**:
  - Form hiệu lực là `go_may`, đại diện cho nhánh không cần Price Config và không có phụ phí viết chữ.
- **Input**:
  - `source_card_id` là ID Card nguồn.
  - Truyền sender, receiver, message mới, `form_type=go_may`, `attached_image_url=null`.
- **Các bước thực hiện**:
  - Gọi endpoint recreate-content.
  - Theo dõi snapshot gửi vào AIModule và dữ liệu được lưu trong giao dịch.
- **Expected output (bắt buộc)**:
  - HTTP 200, trả Card AI mới có ID và thời gian tạo mới.
  - AIModule nhận `raw_image` nguồn làm ảnh tham chiếu, không nhận `image_url` đã chèn chữ thay thế.
  - Card mới dùng Template/Size snapshot nguồn, nội dung mới, `attached_image_url=null`, `word_config_snapshot=null`, `extra_price=0` và `total_price=base_price`.
  - Tạo đúng một Card và một History mới; History kế thừa `base_id/root`, trỏ `output_id` đến Card mới, lưu prompt thực tế trong `system_form`.
  - Card/History nguồn không đổi và không tạo liên kết Checkout/Order.

## Phân loại và trách nhiệm

- **Suite:** SMOKE
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Đây là luồng thành công cơ bản và xác nhận các dữ liệu bất biến quan trọng nhất.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-01 đến BR-027-04, BR-027-08 đến BR-027-11, BR-027-13 đến BR-027-19
