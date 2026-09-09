# UT-027-20

## Thông tin tài liệu

- **Tiêu đề:** Lỗi tạo ảnh hoặc lưu dữ liệu không để lại kết quả một phần
- **Ghi chú:** Card và History mới chỉ được xem là thành công khi có đủ hai ảnh và giao dịch lưu hoàn tất.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-027-20
- **Phiên bản:** 0.1
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-09

## Đơn vị kiểm thử

- **Module:** CARD - Tạo lại Card AI giữ thiết kế
- **Unit under test:** `CardService.RecreateContentAsync`
- **Loại:** Negative / Failure handling
- **Mục tiêu:** Đảm bảo mọi lỗi cuối luồng trả lỗi chung và không để Card hoặc History mồ côi.
- **Precondition / Mock setup**:
  - Toàn bộ admission, input, pricing và prompt hợp lệ.
  - Transaction hỗ trợ rollback; Card/History nguồn được theo dõi để xác nhận không bị sửa.
- **Dữ liệu đại diện**:
  - Chạy riêng các biến thể: AIModule trả lỗi; thiếu `raw_image`; thiếu `image_url`; upload/lưu một ảnh thất bại; insert Card thất bại; insert History thất bại sau khi Card được thêm tạm; commit thất bại.
- **Input**:
  - Request recreate-content hợp lệ như UT-027-01.
- **Các bước thực hiện**:
  - Kích hoạt từng điểm lỗi trong một lần chạy riêng.
  - Kiểm tra response và dữ liệu sau khi transaction kết thúc.
- **Expected output (bắt buộc)**:
  - Mỗi biến thể trả HTTP 500 với `messageCode=INTERNAL_SERVER_ERROR`.
  - Không có Card mới và không có History mới sau rollback; không persist kết quả chỉ có một ảnh.
  - Card/History nguồn giữ nguyên.
  - Lỗi nội bộ được ghi log nhưng không lộ chi tiết nhà cung cấp hoặc database trong response.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Card và History là một kết quả nghiệp vụ duy nhất; dữ liệu một phần sẽ làm hỏng lịch sử người dùng.

## TEST_LINKS

- [TDD-027 - Tạo lại thiệp AI giữ thiết kế gốc](../../tdd/TDD-027/TDD-027-tao-lai-thiep-ai-giu-thiet-ke-goc.md)
- BR-027-02, BR-027-15, BR-027-16
