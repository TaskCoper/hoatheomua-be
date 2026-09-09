# UT-009-03

## Thông tin tài liệu

- **Tiêu đề:** Từ chối group Card Config không hợp lệ
- **Ghi chú:** Key có prefix card_config vẫn kích hoạt Card mode để payload sai group không rơi sang logic Config thường.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-009-03
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CONFIG - Tạo Card Config
- **Unit under test:** `HoaTheoMua.Service.Config.Service.CreateConfig`
- **Loại:** Error
- **Mục tiêu:** Yêu cầu group của Card Config phải bằng chính xác card_config.
- **Precondition / Mock setup**:
  - Admin đã đăng nhập; chưa có singleton cùng key.
  - Các field kind và value được chuẩn bị hợp lệ để chỉ group gây lỗi.
- **Dữ liệu đại diện**:
  - group=other là dữ liệu chính; group rỗng, null hoặc sai hoa thường như Card_Config là các biến thể cùng vi phạm exact group.
  - Wrapper group sai là lỗi schema Size/Price, không phải lỗi identity ở UT này; cần kiểm tra trong UT-009-06 hoặc UT-009-07.
- **Input**:
  - POST /api/v1/configs với group=other, key=card_config.sizes, kind=Setting và Size value hợp lệ.
- **Các bước thực hiện**:
  - Gọi endpoint cho dữ liệu chính.
  - Lặp lại từng biến thể group, mỗi lần giữ phần còn lại hợp lệ.
  - Theo dõi insert và revalidation.
- **Expected output (bắt buộc)**:
  - HTTP 400 với messageCode=CARD_CONFIG_GROUP_INVALID cho từng biến thể.
  - Không insert Config, không SaveChanges và không gọi revalidation.
  - Không chuyển request sang nhánh Config thường vì key đã nhận diện namespace Card.

## Phân loại và trách nhiệm

- **Suite:** REGRESSION
- **Priority:** P1
- **Owner:** Backend Team
- **Rationale:** Bài test khóa namespace Card ở tầng request và ngăn dữ liệu sai group né validator chuyên biệt.

## TEST_LINKS

- [TDD-009 - Tạo Config có hỗ trợ Card Config](../../tdd/TDD-009/TDD-009-tao-card-config.md)
- BR-009-02, BR-009-03

