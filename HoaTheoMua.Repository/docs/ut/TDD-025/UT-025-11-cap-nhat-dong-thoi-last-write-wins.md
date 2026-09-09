# UT-025-11: Cập nhật đồng thời theo quy tắc last-write-wins

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Cập nhật đồng thời theo quy tắc last-write-wins
- **Ghi chú**: Hai request A và B cùng đọc name=Gốc rồi cùng sửa name. A Save trước, B Save sau; B thắng vì hệ thống chưa có concurrency token. Dữ liệu này đại diện cho concurrent update cùng field. Khi hai request sửa field khác nhau, mỗi request chỉ được ghi field có mặt để tránh ghi đè field còn lại.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-025-11
- **Phiên bản**: v0.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Nguyễn Tùng Dương
- **Cập nhật gần nhất**: 2026-09-08

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD TEMPLATE - Cập nhật
- **Unit under test (bắt buộc)**: `CardTemplateService.UpdateAsync`
- **Loại**: Determinism
- **Mục đích kiểm thử**: Làm rõ kết quả xác định khi hai Admin cập nhật gần như cùng lúc.

- **Precondition / Mock setup**:

1. Template ban đầu có name=Gốc, description=Mô tả cũ, isActive=true.
2. Request A và B cùng load phiên bản ban đầu.
3. A gửi name=Tên A; B gửi name=Tên B.
4. Không có row version, ETag hoặc optimistic concurrency token.
5. Thứ tự Save cố định: A trước, B sau.

- **Dữ liệu đại diện**:

- Hai request cùng sửa name đại diện cho xung đột trên bất kỳ cùng một mutable field.
- Nếu request sửa field khác nhau, repository phải phát sinh update chỉ cho field hiện diện; policy vẫn không trả conflict.

- **Input**:

```http
PATCH A /api/card-templates/550e8400-e29b-41d4-a716-446655440025  {"name":"Tên A"}
PATCH B /api/card-templates/550e8400-e29b-41d4-a716-446655440025  {"name":"Tên B"}
Save A trước, Save B sau
```

- **Các bước thực hiện**:

1. A và B cùng đọc name=Gốc.
2. A Save name=Tên A và nhận HTTP 200.
3. B Save name=Tên B sau A, không có token để phát hiện xung đột.
4. Database giữ giá trị từ lần ghi cuối.

- **Expected output (bắt buộc)**:

```text
Request A: HTTP 200, response name = Tên A
Request B: HTTP 200, response name = Tên B
Persisted cuối cùng name = Tên B
Không response nào trả conflict
description và isActive giữ nguyên
```

Giải thích kết quả:

- Tên B thắng vì B là lần ghi sau cùng: đây là last-write-wins.
- Không có optimistic concurrency nên hệ thống không thể từ chối B dựa trên phiên bản A đã đọc.
- Field B không gửi không được ghi đè.

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P2
- **Owner**: Nguyễn Tùng Dương
- **Rationale (bắt buộc)**: Xác nhận chính sách concurrency hiện tại có kết quả lặp lại và được hiểu đúng trước khi triển khai optimistic locking.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BR-021-12
- **Ghi chú**: Kiểm tra last-write-wins và chỉ cập nhật field hiện diện.
