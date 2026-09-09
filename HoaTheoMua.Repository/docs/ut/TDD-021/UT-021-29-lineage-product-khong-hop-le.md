# UT-021-29: Từ chối khi hai mã sản phẩm gốc của mẫu hoa không khớp nhau

## Thông tin tài liệu

- **Tiêu đề (bắt buộc)**: Từ chối khi hai mã sản phẩm gốc của mẫu hoa không khớp nhau
- **Ghi chú**: Khách hàng đang tạo thiệp HandMade từ một mẫu hoa đã tạo trước đó. Mẫu hoa và lịch sử đều tồn tại, nhưng `base_id` và `root` trong lịch sử lại chỉ đến hai Product khác nhau. Hệ thống phải dừng vì không thể biết Product nào mới là nguồn gốc đúng.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-021-29
- **Phiên bản**: v1.2
- **Author (bắt buộc)**: Nguyễn Tùng Dương
- **Reviewer**: Tân Trần
- **Approver**: Tân Trần
- **Owner (bắt buộc)**: Backend Team
- **Cập nhật gần nhất**: 2026-09-09

## Đơn vị kiểm thử

- **Module (bắt buộc)**: CARD - Thiệp HandMade
- **Unit under test (bắt buộc)**: `CardService.CreateAsync` (phần chương trình tiếp nhận yêu cầu và chuẩn bị thiệp mới)
- **Loại**: Error
- **Precondition / Mock setup**:
  - Khách hàng `user-01` đã đăng nhập và tài khoản đang hoạt động.
  - `generated_flowers` có mẫu hoa `flower-01`:
    - `id=flower-01`.
    - `user_id=user-01`, nên mẫu hoa thuộc đúng khách hàng đang gọi API.
    - `image_url=https://storage.example.com/flowers/flower-01.png`, nên ảnh có thể dùng làm nguồn hoa cho thiệp.
  - `client_histories` có một Flower History `flower-history-01`:
    - `user_id=user-01`.
    - `type=flower`.
    - `output_id=flower-01`, nên History này đúng là lịch sử của mẫu hoa được chọn.
    - `base_id=product-01`.
    - `root=product-02`.
  - `product-01` và `product-02` đều là mã Product có thật và đều khác `Guid.Empty`. Việc dùng hai Product có thật giúp khẳng định lỗi đến từ sự mâu thuẫn giữa `base_id` và `root`, không phải do Product bị thiếu.
  - `card_templates` có Template `template-handmade-01`:
    - `template_type=handmade`.
    - Chưa bị đánh dấu xóa và đang hoạt động.
    - `image_url=https://storage.example.com/templates/handmade-01.png`.
  - Cấu hình Size có đúng một kích thước dùng chung: tên `Kích thước chung`, rộng `15`, cao `25`, giá ghi nhận `20000`, giới hạn `100` từ.
  - Cấu hình giá có một bậc chứa lời chúc 6 từ và phụ phí của bậc đó là `0`.
  - Không thiết lập lỗi nào khác. Mục đích là chỉ kiểm tra việc `base_id` và `root` không khớp nhau.
- **Input**:
  ```http
  POST /api/ai-cards
  Content-Type: application/json
  Authorization: Bearer <token của user-01>
  ```

  ```json
  {
    "generatedFlowerId": "flower-01",
    "cardTemplateId": "template-handmade-01",
    "senderName": "An",
    "receiverName": "Bình",
    "messageContent": "Chúc bạn luôn vui và khỏe"
  }
  ```

  Ý nghĩa và trạng thái của toàn bộ field nhận vào:

  | Field | Giá trị | Ý nghĩa |
  |---|---|---|
  | `generatedFlowerId` | `flower-01` | Chọn mẫu hoa AI làm nguồn trực tiếp cho thiệp. |
  | `cardTemplateId` | `template-handmade-01` | Chọn mẫu thiệp HandMade hợp lệ. |
  | `senderName` | `An` | Tên người gửi, không rỗng và không quá 20 từ. |
  | `receiverName` | `Bình` | Tên người nhận, không rỗng và không quá 20 từ. |
  | `messageContent` | `Chúc bạn luôn vui và khỏe` | Lời chúc có 6 từ, nằm trong giới hạn của Size. |
  | `productId` | Không gửi | Request đã dùng `generatedFlowerId`; gửi thêm `productId` sẽ tạo ra hai nguồn và làm sai mục tiêu của UT này. |
  | `formType` | Không gửi | Đây là field không được phép nhận từ khách hàng trong luồng HandMade. |
  | `attachedImageUrl` | Không gửi | Thiệp HandMade không nhận ảnh đính kèm từ request. |
  | `sizeKey` | Không gửi | Hệ thống tự lấy kích thước dùng chung. |

- **Expected output (bắt buộc)**:
  ```text
  HTTP 409
  messageCode = SOURCE_PRODUCT_PROVENANCE_INVALID
  ```

  Dữ liệu lỗi trả ra phải có đầy đủ các field của mẫu lỗi chung:

  ```json
  {
    "title": "<tên nhóm lỗi cho biết dữ liệu đang xung đột>",
    "status": 409,
    "detail": "<mô tả rằng không xác định được Product gốc vì base_id khác root>",
    "messageCode": "SOURCE_PRODUCT_PROVENANCE_INVALID",
    "errors": null,
    "traceId": "<mã theo dõi của request>",
    "timestampUtc": "<thời điểm trả lỗi theo giờ UTC>"
  }
  ```

  TDD-021 chưa khóa câu chữ chính xác của `title` và `detail`, vì vậy UT kiểm tra ý nghĩa của hai field này thay vì buộc chúng phải trùng một câu tiếng Anh cụ thể. Các field còn lại được hiểu như sau:

  - `status=409`: dữ liệu có tồn tại nhưng hai nguồn xác định Product đang mâu thuẫn.
  - `messageCode=SOURCE_PRODUCT_PROVENANCE_INVALID`: cho biết chính xác lỗi nằm ở nguồn gốc Product của mẫu hoa.
  - `errors=null` trong môi trường công khai vì đây không phải lỗi thiếu hoặc sai định dạng một field trong request. Môi trường phát triển có thể bổ sung chi tiết để hỗ trợ tìm lỗi; UT không dùng phần chi tiết đó để quyết định đúng hoặc sai.
  - `traceId` phải có giá trị để người vận hành tìm đúng lần gọi bị lỗi.
  - `timestampUtc` phải có giá trị thời gian của lần trả lỗi.
  - Response không có `value` và không có bất kỳ field kết quả thiệp nào như `id`, `imageUrl`, `cardType` hoặc `totalPrice`, vì thiệp chưa được phép tạo.

  Kết quả trong cơ sở dữ liệu và các bước liên quan:

  - Không thêm bản ghi vào `generated_cards`.
  - Không thêm bản ghi vào `client_histories`.
  - Không lấy Product ID từ `metadata` để thay thế `base_id` hoặc `root`.
  - Không gọi AI, không dùng hạn mức AI và không gọi dịch vụ lưu ảnh, vì đây là thiệp HandMade và request đã bị dừng trước bước lưu.

  Kết quả này đúng vì `base_id` và `root` phải cùng chỉ đến một Product gốc. Nếu hệ thống tự chọn một trong hai giá trị, thiệp có thể bị gắn với sai sản phẩm và bước đặt hàng sau này có thể kiểm tra nhầm tồn kho.

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Backend Team
- **Rationale (bắt buộc)**: Trường hợp này xác nhận hệ thống không đoán Product gốc khi `base_id` và `root` mâu thuẫn, nhờ đó thiệp và đơn hàng sau này không bị gắn sai sản phẩm.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-09
- **Ghi chú**: Flower History phải có `base_id` và `root` cùng là một Product ID hợp lệ.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-021
- **Section**: BR-021-22
- **Ghi chú**: `root` là Product ID gốc bắt buộc và không được dùng giá trị rỗng.

**Link 3**
- **Loại**: User Story
- **Mã**: STORY-035
- **Section**: Luồng tạo thiệp thiết kế từ Generated Flower
- **Ghi chú**: Bảo vệ yêu cầu tạo thiệp từ đúng mẫu hoa và đúng Product gốc.
