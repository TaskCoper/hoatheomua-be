# UT-021-31: Không giữ thiệp khi bước lưu lịch sử bị lỗi

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Không giữ thiệp khi bước lưu lịch sử bị lỗi
- **Ghi chú**: Khách hàng gửi một yêu cầu hoàn toàn hợp lệ. Hệ thống đã chuẩn bị xong thiệp nhưng gặp lỗi khi lưu lịch sử nguồn gốc. Thiệp và lịch sử phải cùng được lưu hoặc cùng bị hủy để khách hàng không nhìn thấy một kết quả thiếu thông tin nguồn.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-31
- **Phiên bản**: v1.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD - Thiệp HandMade
- **Unit under test (bắt buộc)**: `CardService.CreateAsync` (phần chương trình tạo thiệp và lưu lịch sử đi kèm)
- **Loại**: Error
- **Precondition / Mock setup**:
  - Khách hàng `user-01` đã đăng nhập và tài khoản đang hoạt động.
  - Product `product-01` là đúng sản phẩm khách hàng chọn:
    - Có tồn tại và chưa bị đánh dấu xóa.
    - Đang hoạt động.
    - Có công thức và dữ liệu lõi cần thiết.
    - Có số lượng có thể bán lớn hơn `0`.
  - `card_templates` có Template `template-handmade-01`:
    - `template_type=handmade`.
    - Chưa bị đánh dấu xóa và đang hoạt động.
    - `image_url=https://storage.example.com/templates/handmade-01.png`.
  - Cấu hình Size có đúng một kích thước dùng chung:
    - `name=Kích thước chung`.
    - `width=15`.
    - `height=25`.
    - `base_price=20000`.
    - `max_words=100`.
  - Cấu hình giá có bậc từ `0` đến `35` từ với `extra_price=0`.
  - Lời chúc `Chúc bạn luôn vui và khỏe` có 6 từ, vì vậy thuộc bậc giá trên.
  - Hệ thống chuẩn bị các field mà TDD-021 quy định rõ cho thiệp mới trước khi lưu. Entity còn có field `content`, nhưng TDD-021 chưa quy định cách tạo giá trị này cho HandMade nên UT không tự đặt giá trị:

    | Field của thiệp | Giá trị được chuẩn bị | Lý do |
    |---|---|---|
    | `id` | Một ID mới | Mỗi thiệp có mã riêng. |
    | `image_url` | `https://storage.example.com/templates/handmade-01.png` | Thiệp HandMade dùng trực tiếp ảnh của Template. |
    | `raw_image` | `null` | Thiệp HandMade không có ảnh thô do AI tạo. |
    | `card_type` | `handmade` | Template được chọn là loại HandMade. |
    | `form_type` | `calligraphy` | Thiệp HandMade luôn dùng hình thức viết tay. |
    | `sender_name` | `An` | Lấy từ request. |
    | `receiver_name` | `Bình` | Lấy từ request. |
    | `message_content` | `Chúc bạn luôn vui và khỏe` | Lấy từ request. |
    | `attached_image_url` | `null` | Luồng HandMade không nhận ảnh đính kèm. |
    | `size_name` | `Kích thước chung` | Lấy từ Size dùng chung. |
    | `size_width` | `15` | Lấy từ Size dùng chung. |
    | `size_height` | `25` | Lấy từ Size dùng chung. |
    | `size_base_price` | `20000` | Chỉ lưu để xem lại, không thu vào giá thiệp HandMade. |
    | `size_max_words` | `100` | Giới hạn lời chúc của Size. |
    | `word_count` | `6` | Chỉ đếm các từ trong `messageContent`. |
    | `word_config_snapshot` | `{min_words:0,max_words:35,extra_price:0}` | Lưu bậc giá thực tế đã chọn. |
    | `base_price` | `0` | Thiệp HandMade không thu giá nền của Size. |
    | `extra_price` | `0` | Lời chúc nằm trong bậc không thu phụ phí. |
    | `total_price` | `0` | Bằng `extra_price` đối với thiệp HandMade. |
    | `user_id` | `user-01` | Thiệp thuộc khách hàng đang gọi API. |
    | `created_at` | Thời điểm tạo hiện tại | Ghi nhận lúc thiệp được chuẩn bị. |

  - Hệ thống cũng chuẩn bị một History mới:

    | Field của History | Giá trị được chuẩn bị | Ý nghĩa |
    |---|---|---|
    | `user_id` | `user-01` | History thuộc cùng khách hàng với thiệp. |
    | `type` | `handmade_card` | Phân biệt với History của hoa hoặc Card AI. |
    | `system_form` | `null` | Thiệp HandMade không dùng System Prompt. |
    | `base_id` | `product-01` | Product là nguồn trực tiếp của thiệp. |
    | `root` | `product-01` | Product cũng là nguồn gốc cuối cùng. |
    | `output_id` | ID mới của thiệp | Nối History với kết quả thiệp bằng quy tắc trong code, không phải khóa ngoại. |
    | `metadata` | Nguồn Product, Template, Size Config và `validatedAt` | Giữ thông tin cần xem lại nhưng không chép lại nội dung hoặc giá đã có trong thiệp. |
    | `created_at` | Thời điểm tạo hiện tại | Ghi nhận lúc History được chuẩn bị. |

  - Thiết lập nơi lưu dữ liệu để bước ghi `client_histories` báo lỗi sau khi `generated_cards` đã được thêm vào lần lưu hiện tại.
  - `generated_cards` và `client_histories` nằm trong cùng một transaction (một lần lưu chỉ được hoàn tất khi cả hai phần đều thành công).
  - Không thiết lập bất kỳ lỗi nào khác. UT này chỉ kiểm tra lỗi ở bước lưu History.
- **Input**:
  ```http
  POST /api/ai-cards
  Content-Type: application/json
  Authorization: Bearer <token của user-01>
  ```

  ```json
  {
    "productId": "product-01",
    "cardTemplateId": "template-handmade-01",
    "senderName": "An",
    "receiverName": "Bình",
    "messageContent": "Chúc bạn luôn vui và khỏe"
  }
  ```

  Ý nghĩa và trạng thái của toàn bộ field nhận vào:

  | Field | Giá trị | Ý nghĩa |
  |---|---|---|
  | `productId` | `product-01` | Chọn Product làm nguồn trực tiếp cho thiệp. |
  | `cardTemplateId` | `template-handmade-01` | Chọn mẫu thiệp HandMade. |
  | `senderName` | `An` | Tên người gửi. |
  | `receiverName` | `Bình` | Tên người nhận. |
  | `messageContent` | `Chúc bạn luôn vui và khỏe` | Lời chúc có 6 từ. |
  | `generatedFlowerId` | Không gửi | Request đã dùng Product làm nguồn và chỉ được có đúng một nguồn hoa. |
  | `formType` | Không gửi | Hệ thống tự đặt `calligraphy`; khách hàng không được gửi field này. |
  | `attachedImageUrl` | Không gửi | Thiệp HandMade không nhận ảnh đính kèm. |
  | `sizeKey` | Không gửi | Hệ thống tự lấy Size dùng chung. |

- **Expected output (bắt buộc)**:
  ```text
  HTTP 500
  messageCode = INTERNAL_SERVER_ERROR
  ```

  Dữ liệu lỗi công khai trả về phải có đầy đủ các field sau:

  ```json
  {
    "title": "Internal Server Error",
    "status": 500,
    "detail": "An unexpected error occurred.",
    "messageCode": "INTERNAL_SERVER_ERROR",
    "errors": null,
    "traceId": "<mã theo dõi của request>",
    "timestampUtc": "<thời điểm trả lỗi theo giờ UTC>"
  }
  ```

  Ý nghĩa của kết quả:

  - `title`, `status` và `messageCode` cho khách hàng biết đây là lỗi bên trong hệ thống, không phải lỗi do request sai.
  - `detail` dùng câu chung để không làm lộ nội dung lỗi lưu dữ liệu.
  - `errors=null` trong môi trường công khai; chi tiết thật được ghi lại cho người vận hành. Môi trường phát triển có thể bổ sung chi tiết hỗ trợ tìm lỗi, nhưng UT không so sánh nội dung nội bộ đó.
  - `traceId` phải có giá trị để người vận hành tìm đúng lỗi trong nhật ký hệ thống.
  - `timestampUtc` phải có giá trị thời gian của lần trả lỗi.
  - Response không có `value`. Vì vậy không có bất kỳ field kết quả thiệp nào như `id`, `imageUrl`, `rawImage`, `cardType`, `formType`, `senderName`, `receiverName`, `messageContent`, `attachedImageUrl`, `sizeName`, `sizeWidth`, `sizeHeight`, `sizeBasePrice`, `sizeMaxWords`, `wordCount`, `wordConfigSnapshot`, `basePrice`, `extraPrice` hoặc `totalPrice`.

  Kết quả trong cơ sở dữ liệu và các bước liên quan:

  - Số bản ghi trong `generated_cards` không thay đổi. Thiệp đã được chuẩn bị phải bị hủy cùng lần lưu.
  - Số bản ghi trong `client_histories` không thay đổi. Không giữ lại History chưa hoàn tất.
  - Không gọi AI, không dùng hạn mức AI và không gọi dịch vụ lưu ảnh, vì Template là HandMade.
  - Không tự thử lưu lại riêng thiệp hoặc riêng History sau khi transaction đã thất bại.

  Kết quả này đúng vì một thiệp không có History sẽ mất thông tin về Product nguồn, còn một History không có thiệp sẽ trỏ tới kết quả không tồn tại. Hai bản ghi phải luôn tồn tại cùng nhau.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Trường hợp này xác nhận lỗi lưu History không thể để lại một thiệp đơn lẻ, nhờ đó dữ liệu khách hàng nhìn thấy luôn có đầy đủ nguồn gốc đi kèm.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-17
- **Ghi chú**: Thiệp HandMade không gọi AI, không dùng System Prompt và không dùng hạn mức AI.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-18
- **Ghi chú**: `generated_cards` và `client_histories` phải cùng được lưu hoặc cùng bị hủy.

**Link 3**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-19, BR-021-20, BR-021-21
- **Ghi chú**: Xác định toàn bộ dữ liệu thiệp và History đáng lẽ được lưu nếu giao dịch thành công.

**Link 4**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Luồng tạo thiệp thiết kế HandMade
- **Ghi chú**: Bảo vệ kết quả tạo thiệp khỏi trạng thái chỉ lưu được một nửa.
