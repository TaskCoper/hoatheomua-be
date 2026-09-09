# UT-010-01

## Thông tin tài liệu

- **Tiêu đề:** Cập nhật Size Config hợp lệ
- **Ghi chú:** Kiểm tra Admin thay đổi nội dung và trạng thái public của Size Config nhưng không đổi danh tính singleton.

## Metadata quản trị tài liệu

- **Mã tài liệu:** UT-010-01
- **Phiên bản:** 0.4
- **Tác giả:** Nguyễn Tùng Dương
- **Reviewer:** Tân Trần
- **Approver:** Tân Trần
- **Owner:** Backend Team
- **Ngày cập nhật:** 2026-09-08

## Đơn vị kiểm thử

- **Module:** CONFIG - Cập nhật Card Config
- **Unit under test:** `HoaTheoMua.Service.Config.Service.UpdateConfig`
- **Loại:** Happy
- **Mục tiêu:** Xác nhận chỉ value, isPublic và UpdatedAt được cập nhật; Card đã tạo tiếp tục giữ snapshot cũ.
- **Precondition / Mock setup**:
  - Admin admin-01 đã đăng nhập.
  - Size Config size-01 tồn tại, isDeleted=false, group=card_config, key=card_config.sizes, kind=Setting, isPublic=false, CreatedAt=T-created, UpdatedAt=T0, UserId=owner-01.
  - Card old-card-01 đã lưu snapshot size S với width=10, basePrice=10000.
  - Request mới có wrapper hợp lệ, size S đổi width=12, base_price=12000, max_words=100; đồng hồ là T1>T0.
- **Dữ liệu đại diện**:
  - Chuyển isPublic từ false sang true đại diện khả năng bật lại config inactive; chiều true sang false dùng cùng field update nhưng tạo trạng thái inactive.
  - Thay width/base_price đại diện cho việc chỉnh nội dung value; mọi Size item mới vẫn phải qua validator của TDD-009.
  - Card old-card-01 chứng minh update config live không hồi tố dữ liệu snapshot đã phát hành.
- **Input**:
  - PUT /api/v1/configs/550e8400-e29b-41d4-a716-446655440001 với full request giữ nguyên group/key/kind, isPublic=true và wrapper Size mới.
- **Các bước thực hiện**:
  - Gọi endpoint với ID size-01.
  - Cho service tải record chưa xóa, đối chiếu identity và validate Size schema.
  - Lưu thay đổi, gọi revalidation rồi đọc lại Config và Card cũ.
- **Expected output (bắt buộc)**:
  - HTTP 200; BaseResponse value='Cập nhật thành công'.
  - Value đổi sang wrapper mới, isPublic=true và UpdatedAt=T1 UTC.
  - Group, key, kind, CreatedAt, UserId và IsDeleted giữ nguyên.
  - Snapshot của old-card-01 vẫn width=10/basePrice=10000; revalidation chỉ gọi sau SaveChanges.

## Phân loại và trách nhiệm

- **Suite:** SMOKE
- **Priority:** P0
- **Owner:** Backend Team
- **Rationale:** Luồng chính xác nhận phạm vi field được phép sửa và bảo vệ lịch sử Card khỏi thay đổi hồi tố.

## TEST_LINKS

- [TDD-010 - Cập nhật Config có hỗ trợ Card Config](../../tdd/TDD-010/TDD-010-cap-nhat-card-config.md)
- BR-010-01, BR-010-05, BR-010-06, BR-010-08 đến BR-010-10, BR-010-12

