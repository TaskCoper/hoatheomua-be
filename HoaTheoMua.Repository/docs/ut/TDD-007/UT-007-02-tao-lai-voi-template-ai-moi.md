# UT-007-02: Tạo lại thiệp thành công với Template AI mới

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Tạo lại thiệp thành công với Template AI mới
- **Ghi chú**: Card và History nguồn hợp lệ. Template nguồn có ID 660e8400-e29b-41d4-a716-446655440001. Template mới tồn tại, chưa bị đánh dấu xóa, đang hoạt động, có template_type=ai và ảnh dùng được.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-007-02
- **Phiên bản**: v1.0
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD - Tạo lại thiệp AI
- **Unit under test (bắt buộc)**: `AICardService.RegenerateAsync` (phần chương trình tạo Card mới từ Card và History nguồn)
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Khách hàng `user-01` đã đăng nhập và tài khoản đang hoạt động.
  - Card và History nguồn hợp lệ. Template nguồn có ID 660e8400-e29b-41d4-a716-446655440001. Template mới tồn tại, chưa bị đánh dấu xóa, đang hoạt động, có template_type=ai và ảnh dùng được.
  - Các dữ liệu không được nêu là nguyên nhân lỗi đều hợp lệ, giúp kết quả chỉ phụ thuộc vào tình huống đang kiểm tra.
- **Input**:

  ```http
  POST /api/ai-cards/550e8400-e29b-41d4-a716-446655440001/regenerate
  Content-Type: application/json
  Authorization: Bearer <token của user-01>
  ```

  ```json
  {
    "cardTemplateId": "660e8400-e29b-41d4-a716-446655440010"
  }
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
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "<mã theo dõi request>",
  "timestampUtc": "2026-09-09T11:00:00Z",
  "value": {
    "id": "880e8400-e29b-41d4-a716-446655440100",
    "content": "Nội dung Card nguồn",
    "imageUrl": "https://storage.example.com/cards/template-new-final.png",
    "rawImage": "https://storage.example.com/cards/template-new-raw.png",
    "userId": "770e8400-e29b-41d4-a716-446655440003",
    "createdAt": "2026-09-09T11:00:00Z",
    "cardType": "ai",
    "formType": "go_may",
    "senderName": "Nguyễn An",
    "receiverName": "Trần Bình",
    "messageContent": "Chúc bạn sinh nhật vui vẻ",
    "attachedImageUrl": null,
    "sizeName": "Kích thước chung",
    "sizeWidth": 15,
    "sizeHeight": 25,
    "sizeBasePrice": 15000,
    "sizeMaxWords": 150,
    "wordCount": 5,
    "wordConfigSnapshot": null,
    "basePrice": 15000,
    "extraPrice": 0,
    "totalPrice": 15000
  }
}
```

  Hệ thống dùng Template AI mới nhưng vẫn giữ nội dung, kích thước và giá của Card nguồn. Response có đủ lớp ngoài `isSuccess=true`, `isFailed=false`, `error=null`, `traceId`, `timestampUtc` và `value`. Trong `value`, `id`, `imageUrl`, `rawImage`, `userId`, `createdAt` là dữ liệu mới; `content`, `formType`, tên người gửi/nhận, lời chúc, ảnh đính kèm, toàn bộ Size, số từ và giá được giữ từ Card nguồn. History mới kế thừa `base_id/root`, trỏ `output_id` đến Card mới và không sửa Card/History nguồn.

  Kết quả này đúng vì tạo lại chỉ tạo phiên bản hình ảnh mới; dữ liệu cá nhân hóa và nguồn gốc đã được khách hàng xác nhận ở Card trước phải được giữ nguyên.

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Tạo lại thiệp thành công với Template AI mới; kết quả phải rõ ràng và không để lại Card hoặc History thiếu nguồn gốc.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-007
- **Section**: BR-007-03, BR-007-04, BR-007-05, BR-007-13
- **Ghi chú**: Các quy tắc trực tiếp quyết định dữ liệu đầu vào, hành động và kết quả của tình huống này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-036
- **Section**: Luồng tạo lại thiệp từ lịch sử
- **Ghi chú**: Xác nhận khách hàng chỉ nhận Card mới khi nguồn và dữ liệu tạo lại đều hợp lệ.

