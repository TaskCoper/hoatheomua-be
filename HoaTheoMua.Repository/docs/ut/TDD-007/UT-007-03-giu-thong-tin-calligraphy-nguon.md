# UT-007-03: Giữ nguyên nội dung và giá viết tay của Card nguồn

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Giữ nguyên nội dung và giá viết tay của Card nguồn
- **Ghi chú**: Card nguồn là Card AI có form_type=calligraphy, word_count=45, bậc giá 36-70 từ, extra_price=39000 và total_price=54000. History nguồn và Template snapshot đều hợp lệ.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-007-03
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
  - Card nguồn là Card AI có form_type=calligraphy, word_count=45, bậc giá 36-70 từ, extra_price=39000 và total_price=54000. History nguồn và Template snapshot đều hợp lệ.
  - Các dữ liệu không được nêu là nguyên nhân lỗi đều hợp lệ, giúp kết quả chỉ phụ thuộc vào tình huống đang kiểm tra.
- **Input**:

  ```http
  POST /api/ai-cards/550e8400-e29b-41d4-a716-446655440002/regenerate
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
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": "<mã theo dõi request>",
  "timestampUtc": "2026-09-09T11:00:00Z",
  "value": {
    "id": "990e8400-e29b-41d4-a716-446655440100",
    "content": "Nội dung nguồn",
    "imageUrl": "https://storage.example.com/cards/calligraphy-final.png",
    "rawImage": "https://storage.example.com/cards/calligraphy-raw.png",
    "userId": "770e8400-e29b-41d4-a716-446655440003",
    "createdAt": "2026-09-09T11:00:00Z",
    "cardType": "ai",
    "formType": "calligraphy",
    "senderName": "Hoàng Lan",
    "receiverName": "Lê Minh",
    "messageContent": "Nội dung nguồn",
    "attachedImageUrl": null,
    "sizeName": "Kích thước chung",
    "sizeWidth": 15,
    "sizeHeight": 25,
    "sizeBasePrice": 15000,
    "sizeMaxWords": 150,
    "wordCount": 45,
    "wordConfigSnapshot": {
      "min_words": 36,
      "max_words": 70,
      "extra_price": 39000
    },
    "basePrice": 15000,
    "extraPrice": 39000,
    "totalPrice": 54000
  }
}
```

  Tạo lại chỉ thay hình ảnh. Hệ thống không đếm lại số từ và không lấy bảng giá hiện tại. Response có đủ lớp ngoài `isSuccess=true`, `isFailed=false`, `error=null`, `traceId`, `timestampUtc` và `value`. Trong `value`, `id`, `imageUrl`, `rawImage`, `userId`, `createdAt` là dữ liệu mới; `content`, `formType`, tên người gửi/nhận, lời chúc, ảnh đính kèm, toàn bộ Size, số từ và giá được giữ từ Card nguồn. History mới kế thừa `base_id/root`, trỏ `output_id` đến Card mới và không sửa Card/History nguồn.

  Kết quả này đúng vì tạo lại chỉ tạo phiên bản hình ảnh mới; dữ liệu cá nhân hóa và nguồn gốc đã được khách hàng xác nhận ở Card trước phải được giữ nguyên.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Giữ nguyên nội dung và giá viết tay của Card nguồn; kết quả phải rõ ràng và không để lại Card hoặc History thiếu nguồn gốc.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-007
- **Section**: BR-007-05, BR-007-06, BR-007-14
- **Ghi chú**: Các quy tắc trực tiếp quyết định dữ liệu đầu vào, hành động và kết quả của tình huống này.

**Link 2**
- **Loại**: User Story
- **Mã**: STORY-036
- **Section**: Luồng tạo lại thiệp từ lịch sử
- **Ghi chú**: Xác nhận khách hàng chỉ nhận Card mới khi nguồn và dữ liệu tạo lại đều hợp lệ.

