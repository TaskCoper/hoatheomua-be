# UT-008-03

## Thông tin tài liệu

- **Tiêu đề:** Card Config không khả dụng trả danh sách rỗng
- **Ghi chú:** Mọi nguyên nhân làm Card Config không đủ điều kiện đọc đều được biểu diễn bằng danh sách rỗng, không phải lỗi nghiệp vụ.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-008-03
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CONFIG - Danh sách Card Config
- **Unit under test:** `HoaTheoMua.Service.Config.Service.GetConfigSetting`
- **Loại:** Boundary
- **Mục tiêu:** Xác nhận endpoint loại record không khả dụng và vẫn trả HTTP 200.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập.
  - Lần chạy chính chỉ có size-01 với group=card_config, key=card_config.sizes, kind=Setting, isPublic=false, isDeleted=false.
- **Dữ liệu đại diện**:
  - isPublic=false là dữ liệu chính và đại diện trạng thái inactive.
  - Các lần riêng biệt với record isDeleted=true, kind khác Setting, group sai, key ngoài allowlist hoặc không có record đều đi đến cùng kết quả value=[] vì không còn phần tử thỏa query.
  - Mỗi biến thể phải được chạy riêng; không dùng một biến thể thay thế việc kiểm tra các điều kiện loại trừ còn lại.
- **Input**:
  - GET /api/v1/configs/settings?key=card_config.sizes
- **Các bước thực hiện**:
  - Gọi endpoint với dữ liệu chính inactive.
  - Thay lần lượt từng biến thể đại diện, giữ các field khác hợp lệ.
  - Kiểm tra status, wrapper và danh sách trả về.
- **Expected output (bắt buộc)**:
  - HTTP 200; isSuccess=true, isFailed=false và value=[].
  - Không phát sinh messageCode nghiệp vụ vì danh sách rỗng là kết quả hợp lệ.
  - Mỗi biến thể đều không làm lộ record bị xóa, private, sai kind/group hoặc ngoài allowlist.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** Bài test giải thích rõ danh sách rỗng là cách API thể hiện không có Card Config khả dụng, bất kể nguyên nhân loại trừ cụ thể.

## TEST_LINKS

- [TDD-008 - Lấy danh sách Config Setting và Card Config](../../tdd/TDD-008/TDD-008-lay-danh-sach-card-configs.md)
- BR-008-02, BR-008-04, BR-008-08

