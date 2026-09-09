# UT-007-33: Không lưu dữ liệu khi hàm gọi AI báo lỗi cuối cùng

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Không lưu dữ liệu khi hàm gọi AI báo lỗi cuối cùng
- **Ghi chú**: Mọi dữ liệu nguồn hợp lệ. Hàm gọi AI không trả kết quả ảnh mà trả lỗi cuối cùng.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-007-33
- **Phiên bản**: v1.0
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD - Tạo lại thiệp AI
- **Unit under test (bắt buộc)**: `AICardService.RegenerateAsync` (phần chương trình tạo Card mới từ Card và History nguồn)
- **Loại**: Error
- **Precondition / Mock setup**:
  - Khách hàng `user-01` đã đăng nhập và tài khoản đang hoạt động.
  - Mọi dữ liệu nguồn hợp lệ. Hàm gọi AI không trả kết quả ảnh mà trả lỗi cuối cùng.
  - Các dữ liệu không được nêu là nguyên nhân lỗi đều hợp lệ, giúp kết quả chỉ phụ thuộc vào tình huống đang kiểm tra.
- **Input**:

  ```http
  POST /api/ai-cards/550e8400-e29b-41d4-a716-446655440001/regenerate
  Content-Type: application/json
  Authorization: Bearer <token của user-01>
  ```

  ```json
  {}
  ```

  Toàn bộ field nhận vào:

| Field nhận vào | Vị trí | Trạng thái trong API này |
|---|---|---|
| `sourceCardId` | Đường dẫn | Bắt buộc; là ID Card nguồn. |
| `cardTemplateId` | Body | Tùy chọn; là field duy nhất được phép trong body. |
| `productId`, `generatedFlowerId` | Body | Không được phép. |
| `senderName`, `receiverName`, `messageContent` | Body | Không được phép thay đổi. |
| `formType`, `attachedImageUrl`, `sizeKey` | Body | Không được phép. |

- **Expected output (bắt buộc)**:

```json
{
  "title": "Internal Server Error",
  "status": 500,
  "detail": "Không thể tạo ảnh thiệp.",
  "messageCode": "INTERNAL_SERVER_ERROR",
  "errors": null,
  "traceId": "<mã theo dõi request>",
  "timestampUtc": "<thời điểm UTC>"
}
```

  Hệ thống trả HTTP 500 với `messageCode=INTERNAL_SERVER_ERROR`. Response không có `value`, vì vậy không trả bất kỳ field Card nào. `errors` bằng null trong môi trường công khai; `traceId` và `timestampUtc` phải có giá trị để tìm đúng lần gọi. Không tạo `generated_cards` hoặc `client_histories` mới, không gọi AI nếu lỗi được phát hiện trước bước đó và không thay đổi Card/History nguồn.

  Kết quả này đúng vì hệ thống phải dừng đúng tại nguyên nhân đã mô tả, thay vì đoán dữ liệu hoặc tạo một Card không thể truy lại nguồn gốc.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Không lưu dữ liệu khi hàm gọi AI báo lỗi cuối cùng; kết quả phải rõ ràng và không để lại Card hoặc History thiếu nguồn gốc.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-007
- **Section**: BR-007-10, BR-007-11
- **Ghi chú**: Các quy tắc trực tiếp quyết định dữ liệu đầu vào, hành động và kết quả của tình huống này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-036
- **Section**: Luồng tạo lại thiệp từ lịch sử
- **Ghi chú**: Xác nhận khách hàng chỉ nhận Card mới khi nguồn và dữ liệu tạo lại đều hợp lệ.

