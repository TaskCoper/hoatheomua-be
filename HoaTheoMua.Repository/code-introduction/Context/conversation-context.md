# Conversation Context — HoaTheoMua Backend

> **Lưu ý quan trọng — đã được thay thế một phần (2026-09-12):** File này là bản tổng hợp cũ và còn chứa các quyết định đã đổi, ví dụ `senderName`/`receiverName` trong Card snapshot, `GeneratedFlowerIds` dạng mảng, Cards ở cấp request và một số rule snapshot Flower/Card.  
> Trước khi code, đọc bộ context mới theo thứ tự tại [00-read-me-first.md](00-read-me-first.md). Nếu có mâu thuẫn, các file `00` đến `06` luôn được ưu tiên hơn nội dung bên dưới.

> Tài liệu đóng gói các quyết định, quy tắc nghiệp vụ, database context, API và tài liệu đã thống nhất trong cuộc trò chuyện.
>
> Ngày cập nhật: 2026-09-11

## 1. Phạm vi context đã đọc

Các nguồn chính đã được đối chiếu:

- `HoaTheoMua.Repository/diagram/dbcurrent.md`
- `HoaTheoMua.Repository/diagram/dbenhance.md`
- `HoaTheoMua.Repository/diagram/db_tong.md`
- `HoaTheoMua.Repository/docs-us` hoặc `doc-us`
- `HoaTheoMua.Repository/docs (1)`
- `HoaTheoMua.Repository/code-introduction/introduction.md`
- `HoaTheoMua.Repository/SKILL.md`
- Các folder Service và Model trong solution.

Database chính được dùng để thiết kế là `db_tong.md`. Các file trong `docs-db`/`docs (1)` chủ yếu giúp nhìn quan hệ và context; thiết kế thực tế vẫn ưu tiên database tổng và quyết định nghiệp vụ mới nhất.

## 2. Nguyên tắc database đã chốt

- Database có tính dynamic, không dựa vào FK cứng cho các quan hệ polymorphic.
- `client_histories` dùng:
  - `type`: xác định loại dữ liệu (`flower`, `card`, `handmade_card`, `post`).
  - `base_id`: ID nguồn tạo ra kết quả.
  - `output_id`: ID kết quả được tạo.
- Không tạo thêm bảng `flower_requests`, `flower_ai_jobs` hoặc bảng history riêng cho flower/card.
- Không lưu draft. Nếu customer reload, thoát trang hoặc đóng tab khi chưa tạo thành công thì dữ liệu chưa hoàn tất sẽ mất.
- Những dữ liệu cần giữ khi nguồn có thể bị đổi hoặc xóa phải được snapshot.
- Generated Flower chỉ được lưu khi AI tạo thành công.
- Generated Card được lưu khi:
  - AI tạo thành công; hoặc
  - Tạo thiệp HandMade/free thành công.
- `generated_flowers.input_snapshot` lưu toàn bộ input/provenance của lần tạo hoa.
- `client_histories.metadata` lưu snapshot/provenance của History.
- Không dùng `base_on_id` trong `generated_flowers`.
- Không dùng đệ quy để xác định nhóm; lineage card/flower được suy ra bằng `client_histories` và snapshot.

### 2.1. Mapping chính của `client_histories`

| `type` | `base_id` | `output_id` |
|---|---|---|
| `flower` | `products.id` của Combo | `generated_flowers.id` |
| `card` từ Combo | `products.id` của Combo | `generated_cards.id` |
| `card` từ hoa AI | `generated_flowers.id` | `generated_cards.id` |
| `handmade_card` từ Combo | `products.id` của Combo | `generated_cards.id` |
| `handmade_card` từ hoa AI | `generated_flowers.id` | `generated_cards.id` |

`output_id` không được bỏ vì dùng để truy vấn chính xác kết quả trong bảng tương ứng.

### 2.2. Regenerate

- Regenerate Flower tạo một `generated_flowers` mới và một `client_histories` mới.
- Regenerate Card tạo một `generated_cards` mới và một `client_histories` mới.
- History mới kế thừa nguồn gốc (`base_id`) theo quy tắc đã chốt; quan hệ trực tiếp được lưu trong `metadata` nếu cần.
- Card regenerate chỉ đổi Template; các dữ liệu khác lấy từ snapshot Card nguồn.
- Flower regenerate chỉ được chọn lại Mockup; Product và input người dùng lấy từ snapshot nguồn.
- Không ghi đè kết quả cũ.

## 3. Context tạo hoa AI — US-030 và US-033

### 3.1. Luồng chính

```text
Customer chọn Combo
→ nhập tên và thông tin tạo hoa
→ chọn Mockup
→ Backend validate Combo/Mockup/Prompt/quota
→ gọi AI
→ upload ảnh lên storage
→ tạo generated_flowers
→ tạo client_histories(type=flower)
→ trả ảnh cho FE
```

- Không tạo draft trước khi AI thành công.
- Một lần generate thành công tạo một record `generated_flowers`.
- Quota tạo hoa: tối đa 3 lượt/ngày/customer.
- Validation nguồn diễn ra trước khi gọi AI.
- Sau khi snapshot được tạo, trạng thái live mới của Combo/Mockup không làm thay đổi dữ liệu đã tạo.

### 3.2. Snapshot Flower

`generated_flowers.input_snapshot` và `client_histories.metadata` có thể chứa:

```json
{
  "schemaVersion": 2,
  "sourceProductId": "uuid-selected-combo",
  "product": {
    "name": "Combo A",
    "attributes": {},
    "image": {
      "url": "https://...",
      "objectKey": "products/a.jpg",
      "versionId": "v15",
      "sha256": "..."
    },
    "observedStatus": {
      "isActive": true,
      "isDeleted": false,
      "sellableQuantity": 3
    }
  },
  "userInput": {
    "name": "Bó hoa sinh nhật mẹ",
    "occasion": "sinh_nhat",
    "style": "dang_cap",
    "budget": 500000,
    "note": "Ưa thích hoa hồng"
  },
  "mockup": {
    "id": "uuid",
    "name": "Phòng khách",
    "image": { "url": "https://..." },
    "observedStatus": { "isActive": true, "isDeleted": false }
  },
  "systemPrompt": { "id": "uuid", "content": "...", "sha256": "..." },
  "operation": "create-or-regenerate",
  "regeneratedFromFlowerId": null,
  "validatedAt": "2026-08-28T08:00:00Z"
}
```

`userInput` là input của form tạo hoa, không phải hồ sơ account. Ví dụ `name` nên là “Bó hoa sinh nhật mẹ”, không nên dùng tên tài khoản nếu business không yêu cầu.

Lưu ý phải thống nhất tên JSON giữa database và DTO: `userInput` khác với `user_input`.

### 3.3. API Flower đã thống nhất

- `POST /api/ai-flowers`: tạo mẫu hoa AI.
- `POST /api/ai-flowers/{sourceFlowerId}/regenerate`: tạo lại hoa.
- `GET /api/mockups`: lấy Mockup.
- `GET /api/v1/flower-histories?pageIndex=1&pageSize=20`: lấy toàn bộ lịch sử.
- `GET /api/v1/flower-histories/latest?comboId={comboId}`: lấy 3 ảnh gần nhất theo Combo.
- `GET /api/v1/flower-histories/{generatedFlowerId}`: lấy chi tiết một kết quả.
- `GET /api/v1/generated-flowers/{generatedFlowerId}/image-url`: lấy URL ảnh.

## 4. Context thiệp — Card Flow

### 4.1. Generated Card

Các field chính:

```text
id
content
image_url
raw_image
user_id
created_at
card_type (ai | handmade)
form_type (go_may | calligraphy)
size_key
sender_name
receiver_name
message_content
attached_image_url
size_name
size_width
size_height
size_base_price
size_max_words
word_count
word_config_snapshot
base_price
extra_price
total_price
```

`generated_cards.image_url` là ảnh hoàn chỉnh; `raw_image` là ảnh AI trước khi chèn chữ và có thể null với dữ liệu cũ.

### 4.2. Card History

- History Card dùng `type = card`.
- History HandMade dùng `type = handmade_card`.
- Template, Size, Config, giá và provenance phải được snapshot.
- Card chưa có Order vẫn xuất hiện trong History.
- Một Card có thể được dùng cho nhiều Order nhưng chỉ hiển thị một History item.
- Status Order thay đổi không xóa History.

### 4.3. OrderDetails

Theo quyết định hiện tại, OrderDetails có thể chứa danh sách ID:

```text
GeneratedCardIds uuid[]
GeneratedFlowerIds uuid[]
```

`Metadata` dùng để snapshot Combo, giá, nguyên liệu và provenance; không dùng để thay thế hai danh sách ID này nếu field riêng đã được chốt.

## 5. Checkout và Order context

- Không lưu draft Checkout/Order trên local storage hoặc database nếu customer chưa thanh toán.
- Customer nhập thông tin hoa/thiệp tại Checkout; reload trước khi tạo Order thì dữ liệu có thể mất.
- Giá thiệp phụ thuộc Size và nội dung, đặc biệt phụ phí Calligraphy theo số từ.
- Customer thanh toán trước; sau khi thanh toán thành công mới tạo/cập nhật các flow tiếp theo theo nghiệp vụ đã chốt.
- Khi tạo Order, các link hoa/thiệp cần được đưa vào nội dung để nhân viên xử lý.
- Do private description của Nhanh.vn có giới hạn độ dài, Backend cần tạo PDF chứa:
  - Nội dung private description.
  - Link ảnh hoa AI.
  - Link ảnh thiệp.
  - Thông tin cần thiết cho nhân viên đóng gói.
- PDF/link được dùng để cập nhật hoặc đính kèm khi gọi Nhanh.vn.

## 6. Các US customer phụ trách

| US | Chức năng BE |
|---|---|
| US-034 | Customer lấy URL ảnh mẫu hoa AI đã tạo. |
| US-038 | Khởi tạo Checkout từ mẫu hoa thường hoặc hoa AI hợp lệ. |
| US-039 | Hoàn tất Checkout, tạo Order và thanh toán. |
| US-040 | Customer xem lịch sử hoa AI. |
| US-041 | Customer lấy URL ảnh thiệp đã tạo. |
| US-042 | Customer chọn thiệp từ History tại Checkout. |
| US-043 | Admin xem chi tiết thiệp AI. |
| US-044 | Admin xem danh sách thiệp AI. |
| US-045 | Customer xem lịch sử thiệp đã tạo. |
| US-046 | Admin lấy URL/tải ảnh thiệp AI. |
| US-047 | Admin xem danh sách mẫu hoa Custom AI. |
| US-048 | Admin xem chi tiết mẫu hoa Custom AI. |

## 7. API customer đã define

### US-040

TDD-028 hiện chỉ mô tả API thứ nhất:

```http
GET /api/v1/flower-histories?pageIndex=1&pageSize=20
```

- Trả `BasePaginationResponse`.
- `value.items` gồm `generatedFlowerId`, `comboId`, `imageUrl`, `generatedAt`.
- Lọc theo JWT user, `type = flower`, chưa xóa.
- Join động `client_histories.output_id = generated_flowers.id`.
- Danh sách rỗng trả HTTP 200, không trả 404.

API gần nhất và API chi tiết là các API riêng trong introduction, không nằm trong TDD-028 sau lần chỉnh sửa gần nhất.

### US-034

```http
GET /api/v1/generated-flowers/{generatedFlowerId}/image-url
```

- Chỉ trả URL trong `BaseResponse.value`.
- FE tải trực tiếp từ Cloudinary/Storage.
- BE không stream binary, không tạo tên file, không ép PNG, không gọi Storage.

### US-045

```http
GET /api/v1/card-histories?pageIndex=1&pageSize=20
GET /api/v1/card-histories/{generatedCardId}
```

API danh sách:

- Lọc `client_histories.type = card`.
- Join `output_id` với `generated_cards.id`.
- Trả `BasePaginationResponse`.
- Không join OrderDetails để tránh nhân bản thiệp.

API chi tiết:

- Kiểm tra ID, ownership và `isDeleted`.
- Không tìm thấy ID hoặc không thuộc customer → `404 CARD_HISTORY_NOT_FOUND`.
- Tìm thấy mới truy vấn OrderDetails.
- Trả toàn bộ field Card, metadata và `orders`.
- Không có Order → `orders: []`.

### US-041

```http
GET /api/v1/generated-cards/{generatedCardId}/image-url
```

- Dùng chung khi mở từ History hoặc Order.
- Không truyền `orderId`.
- Không kiểm tra liên kết Order trong API URL.
- Trả `generatedCardId` và `imageUrl`.

## 8. API admin đã define

### US-043

Admin xem chi tiết thiệp AI:

- Trả ảnh, mã thiệp, thông tin Card, Customer ID, người gửi/nhận, lời chúc, Template, Size, Form, giá và Order liên kết.
- Không lấy hồ sơ User Service vì User nằm ở microservice khác.
- Không dùng Audit Log.

### US-044

Admin xem danh sách thiệp AI:

- Có phân trang.
- Có thể tìm theo:
  - `generated_cards.id` nếu keyword là Card ID.
  - `orders.id` nếu keyword là Order ID.
  - `generated_cards.sender_name` nếu tìm theo tên người gửi.
- Response dùng `BasePaginationResponse`.

### US-046

Admin lấy URL ảnh thiệp AI:

- Chỉ trả link ảnh.
- Không stream file qua Backend.
- Không lưu Audit Log.

### US-047

Admin xem danh sách mẫu hoa Custom AI:

- Lấy Generated Flower và snapshot.
- `customerName` phải hiểu là tên người gửi/tên trong input snapshot theo business, không tự suy diễn là tên account.
- Có tìm kiếm và phân trang theo introduction.

### US-048

Admin xem chi tiết mẫu hoa Custom AI:

- Trả ảnh, Combo, Mockup, Core, Support, input snapshot và Order liên kết.
- `userInput` là dữ liệu form yêu cầu tạo hoa, không phải hồ sơ User.
- `userInput.name` nên là tên mẫu hoa như “Bó hoa sinh nhật mẹ”.
- Snapshot ưu tiên `generated_flowers.input_snapshot`; fallback `client_histories.metadata` nếu được chốt trong implementation.
- Không trả giá/bill nếu US không yêu cầu.
- Ảnh không khả dụng không làm mất metadata.

## 9. Response wrapper chuẩn

### BaseResponse

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": null,
  "timestampUtc": "2026-09-11T10:00:00Z",
  "value": {}
}
```

### BasePaginationResponse

```json
{
  "isSuccess": true,
  "isFailed": false,
  "error": null,
  "traceId": null,
  "timestampUtc": "2026-09-11T10:00:00Z",
  "value": {
    "items": [],
    "pageIndex": 1,
    "pageSize": 20,
    "totalCount": 0,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

FE đọc dữ liệu tại `response.value`; danh sách đọc tại `response.value.items`.

## 10. Quy chuẩn viết TDD/UT

Tất cả TDD/UT mới bám `HoaTheoMua.Repository/SKILL.md`.

### TDD

Gồm bốn phần:

1. Thông tin và Bối cảnh.
2. Kiến trúc và Mermaid diagrams.
3. API Contract: Endpoint, Request, Response, IService, Service logic, mã lỗi.
4. Tham chiếu.

### UT

Mỗi UT phải có:

- Metadata.
- Module và Unit under test.
- Precondition/Mock setup có thể tái lập.
- Input.
- Expected output có lý do.
- Suite, Priority, Owner, Rationale.
- TEST_LINKS tới Story và TDD section cụ thể.

## 11. Các file TDD/UT đã tạo

### Customer

- `US-040/TDD-028-customer-xem-lich-su-hoa-custom-ai.md`
- `US-040/UT-028-customer-xem-lich-su-hoa-custom-ai.md`
- `US-034/TDD-029-customer-lay-url-anh-hoa-ai.md`
- `US-034/UT-029-customer-lay-url-anh-hoa-ai.md`
- `US-045/TDD-030-customer-xem-lich-su-thiep.md`
- `US-045/UT-030-customer-xem-lich-su-thiep.md`
- `US-041/TDD-031-customer-lay-url-thiep-ai.md`
- `US-041/UT-031-customer-lay-url-thiep-ai.md`

### Admin

- `US-044/TDD-023-admin-xem-danh-sach-thiep-ai.md`
- `US-044/UT-023-admin-xem-danh-sach-thiep-ai.md`
- `US-043/TDD-024-admin-xem-chi-tiet-thiep-ai.md`
- `US-043/UT-024-admin-xem-chi-tiet-thiep-ai.md`
- `US-046/TDD-025-admin-tai-xuong-thiep-ai.md`
- `US-046/UT-025-admin-tai-xuong-thiep-ai.md`
- `US-047/TDD-026-admin-xem-danh-sach-mau-hoa-custom-ai.md`
- `US-047/UT-026-admin-xem-danh-sach-mau-hoa-custom-ai.md`
- `US-048/TDD-027-admin-xem-chi-tiet-mau-hoa-custom-ai.md`
- `US-048/UT-027-admin-xem-chi-tiet-mau-hoa-custom-ai.md`

## 12. Các điểm cần kiểm tra trước khi code

- Thống nhất JSON snapshot: camelCase (`userInput`) hay snake_case (`user_input`).
- Xác nhận `OrderDetails.GeneratedCardIds` và `GeneratedFlowerIds` là field riêng hay còn giữ trong Metadata.
- Xác nhận enum/string thực tế của `type`, `card_type`, `form_type`.
- TDD-028 chỉ còn API danh sách toàn bộ; nếu cần API latest/detail phải tạo TDD riêng hoặc cập nhật phạm vi rõ ràng.
- API tải ảnh hoa/thiệp hiện trả URL; không quay lại thiết kế BE stream file nếu chưa có quyết định mới.
- System Test cũ yêu cầu draft/job hoặc Backend stream PNG cần được đánh dấu là không còn phù hợp với context mới.
- Khi đổi trạng thái Product, Mockup, Template hoặc Config, dữ liệu lịch sử vẫn dùng snapshot.
- Order mới phải kiểm tra lineage/provenance và trạng thái Product live; History không thay thế validation Order.

## 13. Kết luận ngắn

Kiến trúc hiện tại ưu tiên:

```text
Snapshot bất biến
→ client_histories để truy vết polymorphic
→ generated_flowers/generated_cards để lưu kết quả
→ FE nhận URL và tải trực tiếp
→ OrderDetails lưu danh sách kết quả được dùng
→ Không lưu draft và không tạo bảng request trung gian
```
