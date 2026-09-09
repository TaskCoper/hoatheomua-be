# UT-009-06

## Thông tin tài liệu

- **Tiêu đề:** Từ chối Size Config sai schema
- **Ghi chú:** Các dạng lỗi wrapper, inner JSON và Size item khác nhau nhưng dùng chung mã CARD_SIZE_CONFIG_INVALID.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-009-06
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CONFIG - Tạo Card Config
- **Unit under test:** `HoaTheoMua.Service.Config.Service.CreateConfig`
- **Loại:** Boundary
- **Mục tiêu:** Không lưu Size Config khi bất kỳ tầng dữ liệu nào không thể tái lập đúng cấu trúc quy định.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập; group/key/kind canonical và chưa có Size singleton.
  - Request chính có wrapper chuẩn, inner items chứa một item S nhưng width=0.
- **Dữ liệu đại diện**:
  - width=0 là biên chính vì width bắt buộc lớn hơn 0.
  - Các biến thể phải chạy riêng gồm: wrapper thiếu/thừa field hoặc sai constant; inner JSON không parse được; object thừa field; items rỗng/sai kiểu.
  - Ở cấp item, kiểm tra thiếu/thừa/sai tên snake_case, name rỗng hoặc trùng, width/height không dương, base_price âm và max_words không dương.
  - Tất cả biến thể cùng mã lỗi vì đều khiến Size Config không đáp ứng schema, nhưng kết quả test phải ghi rõ biến thể đã chạy.
- **Input**:
  - POST Size Config với item name=S, width=0, height=20, base_price=10000, max_words=100.
- **Các bước thực hiện**:
  - Gọi endpoint với width=0.
  - Lặp lại từng biến thể đại diện, mỗi lần chỉ tạo một lỗi.
  - Kiểm tra response và xác nhận không có side effect.
- **Expected output (bắt buộc)**:
  - HTTP 400 với messageCode=CARD_SIZE_CONFIG_INVALID cho từng biến thể.
  - Detail cho biết Size Config không hợp lệ; không lưu một phần dữ liệu.
  - Không insert, không SaveChanges và không gọi revalidation.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Bao phủ từng lớp schema giúp phát hiện cấu hình hỏng ngay lúc quản trị, trước khi các luồng tạo Card sử dụng.

## TEST_LINKS

- [TDD-009 - Tạo Config có hỗ trợ Card Config](../../tdd/TDD-009/TDD-009-tao-card-config.md)
- BR-009-07, BR-009-08, BR-009-09

