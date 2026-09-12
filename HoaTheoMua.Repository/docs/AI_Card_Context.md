# AI Card Context - HTM_Flourist_AI

> **Phiên bản:** 4.0  
> **Cập nhật gần nhất:** 2026-09-11  
> **Trạng thái:** Đã chốt cho TDD-006 và TDD-021

## 1. Phạm vi

Tài liệu này là nguồn context chung cho Card AI, Card HandMade, snapshot giá và lineage trong `ClientHistory`.

- Card AI dùng một endpoint duy nhất `POST /api/ai-cards` cho cả Create và Regenerate.
- Create nhận `orderDetailId` và dùng `OrderDetail.SnapshotCard`.
- Regenerate nhận `sourceCardId` cùng Template hoặc nội dung tùy chọn và dùng snapshot của Card/History nguồn.
- HandMade dùng endpoint riêng `POST /api/handmade-cards`.
- Việc tạo Order, OrderDetail và `OrderDetail.SnapshotCard` nằm ngoài phạm vi API Card AI và ngoài TDD-006.
- Mỗi request Card AI thành công tạo một `GeneratedCard` và một `ClientHistory` mới; không cập nhật OrderDetail hoặc Card/History nguồn.

TDD liên quan:

- TDD-006: tạo mới và tạo lại Card AI.
- TDD-021: chọn và tạo Card HandMade.
- TDD-008 đến TDD-011: quản lý Card Config.
- TDD-022 đến TDD-026: quản lý Card Template.

Các tài liệu tạo lại riêng trước đây đã được loại bỏ. Không còn endpoint `/regenerate` hoặc `/recreate-content` riêng cho Card.

## 2. Endpoint và cách chọn mode

### 2.1 Contract chung

```json
{
  "orderDetailId": "uuid-or-null",
  "sourceCardId": "uuid-or-null",
  "cardTemplateId": "uuid-or-null",
  "senderName": "string-or-null",
  "receiverName": "string-or-null",
  "messageContent": "string-or-null"
}
```

Quy tắc chọn mode:

| Dữ liệu request | Mode | Kết quả |
|---|---|---|
| Chỉ có `orderDetailId` | Create | Tạo Card AI từ `OrderDetail.SnapshotCard` |
| Chỉ có `sourceCardId` | Regenerate | Tạo Card AI mới từ Card/History nguồn |
| Không có cả hai hoặc có đồng thời cả hai | Không hợp lệ | `VALIDATION_ERROR/400` |

`sourceCardId` luôn là `GeneratedCard.Id`, không phải `ClientHistory.Id`.

### 2.2 Create

Request Create chỉ được truyền `orderDetailId`. Nếu request có bất kỳ field override nào trong `cardTemplateId`, `senderName`, `receiverName`, `messageContent`, kể cả giá trị null, API trả `CARD_CREATE_OVERRIDE_NOT_ALLOWED/400`.

Service thực hiện:

1. Tìm OrderDetail theo `orderDetailId`.
2. Kiểm tra OrderDetail thuộc người dùng hiện tại; Order có thể ở bất kỳ trạng thái nào.
3. Parse và kiểm tra cấu trúc `OrderDetail.SnapshotCard`.
4. Dùng nguyên snapshot để dựng input cho mô-đun AI; không đọc hoặc revalidate Product, Generated Flower, Card Template, Card Config hay System Prompt hiện hành.
5. Nếu `source.type=product`, gán `ClientHistory.base_id=RootID=source.id`.
6. Nếu `source.type=generated_flower`, vẫn truy vấn đúng Flower History của người dùng để lấy Product gốc và kiểm tra lineage; `base_id=source.id`, `RootID` bằng Product ID gốc.
7. Khi AI trả đủ `rawImage` và `imageUrl`, tạo `GeneratedCard` và `ClientHistory` trong cùng transaction.

Các giá trị `wordCount`, Size và giá trong snapshot được tin là kết quả đã chốt tại thời điểm tạo OrderDetail. API chỉ kiểm tra JSON parse được, đủ field, đúng kiểu và UUID/URL/string bắt buộc có hình thức dùng được; không kiểm tra lại quan hệ giữa các giá trị và không tính lại giá.

### 2.3 Regenerate

Request Regenerate bắt buộc có `sourceCardId`. Các field sau là tùy chọn:

- `cardTemplateId`
- `senderName`
- `receiverName`
- `messageContent`

Request không có `sizeName`, `formType`, `attachedImageUrl` hoặc field giá.

Service tải đúng một `GeneratedCard` nguồn và đúng một `ClientHistory` có `type=card`, `output_id=sourceCardId`, cùng người dùng. Card HandMade bị từ chối bằng `HANDMADE_CARD_REGENERATE_NOT_SUPPORTED/409`.

Quy tắc fallback và kế thừa:

- `senderName`, `receiverName`, `messageContent`: nếu field bị bỏ, null, rỗng hoặc chỉ có khoảng trắng thì dùng giá trị nguồn.
- `formType` và `attachedImageUrl`: luôn kế thừa từ Card nguồn.
- `SizeName`, `SizeWidth`, `SizeHeight`, `SizeBasePrice`, `SizeMaxWords`, `WordConfigSnapshot`, `BasePrice`, `ExtraPrice`, `TotalPrice`: kế thừa nguyên trạng.
- Chỉ `WordCount` được đếm lại từ `messageContent` hiệu lực.
- Không kiểm tra cận dưới của Calligraphy. Ví dụ nguồn có `min_words=36` nhưng nội dung mới 20 từ vẫn hợp lệ và giữ nguyên giá nguồn.
- Nội dung hiệu lực không được vượt 100 từ, `SizeMaxWords`, và nếu `formType=calligraphy` thì không được vượt `WordConfigSnapshot.max_words`.
- `senderName` và `receiverName` không được vượt 20 từ.
- `go_may` yêu cầu `WordConfigSnapshot=null`; Calligraphy yêu cầu snapshot rule nguồn dùng được. Không đọc Config hiện hành.

Quy tắc Template:

- Bỏ `cardTemplateId`, gửi null, chuỗi rỗng/blank hoặc gửi đúng Template ID nguồn: dùng nguyên Template snapshot nguồn, không truy vấn Template live.
- Gửi một Template ID khác: truy vấn Template hiện hành và kiểm tra tồn tại, chưa xóa, active và `templateType=ai`.

Quy tắc prompt:

- Ưu tiên `SystemPrompt` type `card` hiện hành.
- Nếu prompt hiện hành không dùng được, fallback về `ClientHistory.SystemForm` của Card nguồn.
- Nếu cả hai không dùng được, trả `CARD_SOURCE_HISTORY_INVALID/409`.

Ảnh hoa làm đầu vào AI được lấy từ `ClientHistory.Metadata.source.imageUrl` của Card nguồn. Không dùng `GeneratedCard.RawImage` làm ảnh nguồn Regenerate.

History mới kế thừa `base_id` và `RootID` đúng quy tắc cũ, có `output_id` là Card mới và lưu `regeneratedFromCardId=sourceCardId`.

## 3. OrderDetail.SnapshotCard

Snapshot được tạo khi đã có Order/OrderDetail, ở một flow ngoài API này. Đây là dữ liệu bất biến mà Create sử dụng.

```json
{
  "source": {
    "type": "product|generated_flower",
    "id": "uuid",
    "imageUrl": "https://..."
  },
  "cardTemplate": {
    "templateId": "uuid",
    "templateName": "Tên template",
    "templateType": "ai",
    "templateImageUrl": "https://..."
  },
  "cardSize": {
    "name": "Lớn",
    "width": 15,
    "height": 25,
    "base_price": 15000,
    "max_words": 100
  },
  "cardConfig": {
    "min_words": 36,
    "max_words": 70,
    "extra_price": 39000
  },
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "formType": "calligraphy",
  "attachedImageUrl": null,
  "wordCount": 5,
  "basePrice": 15000,
  "extraPrice": 0,
  "totalPrice": 15000,
  "systemPrompt": {
    "type": "card",
    "content": "Nội dung prompt đầy đủ"
  },
  "validatedAt": "UTC timestamp"
}
```

Quy tắc cấu trúc:

- `source.type` chỉ nhận `product` hoặc `generated_flower`.
- `cardTemplate.templateType` phải là `ai`.
- `cardConfig` nhận object đúng cấu trúc hoặc null. Create không kiểm tra quan hệ giữa field này với `formType`, `wordCount` hoặc các field giá.
- `validatedAt` là thời điểm snapshot được tạo, không phải thời điểm gọi Card API.
- Snapshot lưu đầy đủ ảnh nguồn và ảnh Template để History mới có thể tiếp tục lưu chúng.
- Không thêm các field chỉ xuất hiện sau khi tạo Card thành công, ví dụ `GeneratedCard.Id` hoặc lineage History.

## 4. Database schema liên quan

### 4.1 Enums

```sql
Enum CardFormType {
  go_may
  calligraphy
}

Enum ClientHistoryType {
  flower
  card
  handmade_card
  post
}
```

### 4.2 OrderDetail

```sql
Table OrderDetail {
  Id uuid [pk]
  OrderId uuid
  SnapshotCard json
}
```

`SnapshotCard` có thể null nếu OrderDetail không dùng Card AI. TDD-006 không tạo hoặc cập nhật field này và không thêm quan hệ tới GeneratedCard.

### 4.3 GeneratedCard

```sql
Table GeneratedCard {
  Id uuid [pk]
  Content text
  ImageUrl varchar
  RawImage varchar
  UserId uuid
  CreatedAt timestamp
  CardType varchar
  FormType varchar
  SenderName varchar
  ReceiverName varchar
  MessageContent text
  AttachedImageUrl varchar
  SizeName varchar
  SizeWidth decimal
  SizeHeight decimal
  SizeBasePrice decimal
  SizeMaxWords int
  WordCount int
  WordConfigSnapshot json
  BasePrice decimal
  ExtraPrice decimal
  TotalPrice decimal
}
```

`RawImage` là ảnh AI trước bước render chữ. Regenerate không dùng field này làm ảnh hoa nguồn.

### 4.4 ClientHistory

```sql
Table ClientHistory {
  Id uuid [pk]
  UserId uuid
  Type varchar
  SystemForm json
  Metadata json
  CreatedAt timestamp
  RootID uuid
  BaseId uuid
  OutputId uuid
}
```

- Card AI dùng `type=card`; HandMade dùng `type=handmade_card`.
- `SystemForm` của Card AI lưu `{type,content}` của prompt thực tế; HandMade lưu null.
- `RootID`, `BaseId`, `OutputId` là tham chiếu đa hình do code quản lý, không phải foreign key.
- Không lưu `orderDetailId` trong metadata.

### 4.5 ClientHistory.Metadata của Card AI

Metadata Card mới không dùng `schemaVersion`. `sizeConfig` vẫn được giữ để ghi nhận nguồn cấu hình nghiệp vụ, dù TDD-006 không đọc Config live.

```json
{
  "operation": "create|regenerate",
  "sourceType": "product|generated_flower",
  "originProductId": "uuid",
  "generatedFlowerId": "uuid-or-null",
  "provenanceStatus": "available",
  "source": {
    "type": "product|generated_flower",
    "id": "uuid",
    "imageUrl": "https://..."
  },
  "cardTemplate": {
    "id": "uuid",
    "name": "Tên template",
    "templateType": "ai",
    "imageUrl": "https://..."
  },
  "sizeConfig": {
    "group": "card_config",
    "key": "card_config.sizes"
  },
  "regeneratedFromCardId": "uuid-or-null",
  "systemPromptSource": "order_detail_snapshot|current|source_card_fallback",
  "validatedAt": "UTC timestamp"
}
```

- Create lấy `source`, `cardTemplate` và `validatedAt` từ `OrderDetail.SnapshotCard`.
- Regenerate kế thừa source snapshot; dùng Template snapshot nguồn hoặc snapshot Template mới đã validate; `validatedAt` là thời điểm chuẩn bị dữ liệu của request hiện tại.
- Card cũ thiếu `source.imageUrl` hoặc `cardTemplate.imageUrl` không đủ dữ liệu để Regenerate và trả `CARD_SOURCE_HISTORY_INVALID/409`; không fallback sang entity live.
- `originProductId` phải trùng `RootID`. `generatedFlowerId` bằng `base_id` khi nguồn là Generated Flower, ngược lại bằng null.

### 4.6 ClientHistory.Metadata của HandMade

```json
{
  "sourceType": "product|generated_flower",
  "originProductId": "uuid",
  "generatedFlowerId": "uuid-or-null",
  "provenanceStatus": "available",
  "cardTemplate": {
    "id": "uuid",
    "name": "Tên template",
    "templateType": "handmade"
  },
  "sizeConfig": {
    "group": "card_config",
    "key": "card_config.sizes"
  },
  "priceConfig": {
    "group": "card_config",
    "key": "card_config.prices"
  },
  "validatedAt": "UTC timestamp"
}
```

HandMade vẫn theo TDD-021 và không thuộc Regenerate của TDD-006.

## 5. Lineage

| Nguồn | `ClientHistory.type` | `base_id` | `RootID` | `output_id` |
|---|---|---|---|---|
| Product | `card` | Product ID | Product ID | Card mới |
| Generated Flower | `card` | Generated Flower ID | Product ID gốc từ Flower History | Card mới |
| Regenerate Card | `card` | Kế thừa History nguồn | Kế thừa History nguồn | Card mới |
| HandMade từ Product | `handmade_card` | Product ID | Product ID | Card mới |
| HandMade từ Generated Flower | `handmade_card` | Generated Flower ID | Product ID gốc | Card mới |

Với Generated Flower, phải tìm đúng một Flower History thuộc người dùng, `type=flower`, `output_id=GeneratedFlower.Id`, có `base_id` và `RootID` dùng được và nhất quán. Không đoán Product ID từ metadata. Sai lineage trả `SOURCE_PRODUCT_PROVENANCE_INVALID/409`.

## 6. Dependency và validation boundary

| Dependency | Create | Regenerate |
|---|---|---|
| OrderDetail | Bắt buộc, kiểm tra ownership; mọi Order status | Không đọc |
| Product live | Không đọc/revalidate | Không đọc/revalidate |
| Generated Flower live | Không đọc/revalidate | Không đọc/revalidate |
| Flower History | Chỉ dùng để resolve RootID khi snapshot là Generated Flower | Không đọc; kế thừa lineage Card nguồn |
| Card Template live | Không đọc | Chỉ đọc khi chọn ID khác Template nguồn |
| Card Config live | Không đọc | Không đọc |
| System Prompt live | Không đọc | Ưu tiên hiện hành, fallback prompt nguồn |
| GeneratedCard/ClientHistory nguồn | Không đọc | Bắt buộc và phải cùng owner |

Thứ tự kiểm tra tổng quát:

1. Authentication và trạng thái tài khoản.
2. Hình thức request và XOR mode.
3. Từ chối field override ở Create.
4. Tồn tại và ownership của aggregate nguồn.
5. Loại Card, lineage và cấu trúc snapshot.
6. Template live nếu Regenerate chọn Template khác.
7. Fallback nội dung, đếm từ và kiểm tra cận trên.
8. Dựng snapshot cố định trong bộ nhớ.
9. Gọi mô-đun AI.
10. Transaction tạo Card và History.

## 7. Quota và retry

Quota/retry do mô-đun AI tự quản lý. TDD-006 chỉ coi đây là trách nhiệm của mô-đun AI; các con số sau được giữ trong Context để bảo toàn quy tắc hệ thống.

| Loại | Giới hạn | Đơn vị |
|---|---:|---|
| Tạo Card AI | 10 lượt | mỗi ngày, mỗi khách hàng |
| Tạo Card AI theo nguồn | 3 lượt | mỗi ngày, mỗi khách hàng, mỗi `base_id` |

- Mỗi request Card AI reserve đồng thời quota ngày và quota theo `base_id`.
- Reserve sau dependency validation và trước khi gọi AI; nếu một slot thất bại thì không giữ slot còn lại.
- Consume khi AI, upload và persistence thành công; release nếu thất bại.
- Một lần gọi đầu và tối đa hai lần retry, dùng cùng snapshot và reservation.
- HandMade không thuộc quota này.

## 8. Pricing

### 8.1 Card AI Create

TDD-006 không tính giá và không đọc bảng Config. `cardSize`, `cardConfig`, `wordCount`, `basePrice`, `extraPrice`, `totalPrice` đến từ `OrderDetail.SnapshotCard` và được lưu sang Card mới.

Flow tạo Order/OrderDetail có trách nhiệm chuẩn bị snapshot bằng Card Config hiện hành, nhưng chi tiết đó nằm ngoài TDD-006.

### 8.2 Card AI Regenerate

Regenerate chỉ đếm lại `WordCount`; toàn bộ Size, rule và giá được kế thừa từ Card nguồn. Nội dung ngắn hơn `min_words` vẫn giữ giá nguồn. Không gửi `sizeName` và không chọn lại khoảng giá.

### 8.3 HandMade

HandMade tiếp tục đọc Config live theo TDD-021. `BasePrice=0`; `ExtraPrice` là phụ phí Calligraphy; `TotalPrice=ExtraPrice`.

### 8.4 Cấu trúc và giá trị Card Config đang được tài liệu ghi nhận

Card Config nằm trong bảng `Config`, có `group=card_config`, `kind=Setting` và hai key singleton. Field `value` dùng wrapper bảy field; `value.value` là một chuỗi JSON cần parse thêm một lớp.

`card_config.sizes`:

```json
{
  "group": "card_config",
  "label": "Kích thước thiệp",
  "value": "{\"items\":[{\"name\":\"Nhỏ\",\"width\":10,\"height\":15,\"base_price\":10000,\"max_words\":70},{\"name\":\"Lớn\",\"width\":15,\"height\":25,\"base_price\":15000,\"max_words\":150}]}",
  "options": [],
  "required": false,
  "inputType": "json",
  "validations": []
}
```

`card_config.prices`:

```json
{
  "group": "card_config",
  "label": "Bậc giá",
  "value": "{\"items\":[{\"min_words\":0,\"max_words\":35,\"extra_price\":0},{\"min_words\":36,\"max_words\":70,\"extra_price\":39000}]}",
  "options": [],
  "required": false,
  "inputType": "json",
  "validations": []
}
```

Đây là giá trị được ghi nhận trong bộ tài liệu Config hiện tại. TDD-006 không dùng các giá trị live này để ghi đè snapshot: Create tin giá trong OrderDetail và Regenerate tin giá trong Card nguồn.

## 9. AI input và persistence

Input gửi mô-đun AI phải chứa snapshot cố định gồm ảnh nguồn hoa, Template, Size, nội dung, form, ảnh đính kèm, giá và prompt hiệu lực. Mô-đun AI tự quản lý quota/retry và trả cả:

- `rawImage`: ảnh Card trước khi render chữ.
- `imageUrl`: ảnh Card hoàn chỉnh.

Chỉ khi cả hai URL dùng được mới tạo dữ liệu. `GeneratedCard` và `ClientHistory` phải được lưu nguyên tử; AI, upload hoặc persistence thất bại thì không giữ record mới.

## 10. Error taxonomy

| Message code | HTTP | Khi dùng |
|---|---:|---|
| `UNAUTHORIZED` | 401 | Chưa đăng nhập hoặc tài khoản không hoạt động |
| `VALIDATION_ERROR` | 400 | XOR mode sai, UUID sai, field sai hoặc nội dung vượt cận trên |
| `CARD_CREATE_OVERRIDE_NOT_ALLOWED` | 400 | Create có field override ngoài `orderDetailId` |
| `ORDER_DETAIL_NOT_FOUND` | 404 | Không có OrderDetail tương ứng |
| `ACCESS_DENIED` | 403 | OrderDetail, Card hoặc History không thuộc người dùng |
| `ORDER_DETAIL_CARD_SNAPSHOT_INVALID` | 409 | Snapshot OrderDetail thiếu hoặc sai cấu trúc |
| `SOURCE_PRODUCT_PROVENANCE_INVALID` | 409 | Không resolve được Product gốc từ Flower History |
| `CARD_NOT_FOUND` | 404 | Không có GeneratedCard nguồn |
| `HANDMADE_CARD_REGENERATE_NOT_SUPPORTED` | 409 | Card nguồn là HandMade |
| `CARD_SOURCE_HISTORY_INVALID` | 409 | History/lineage/snapshot/prompt nguồn không dùng được |
| `CARD_TEMPLATE_NOT_FOUND` | 404 | Template mới không tồn tại |
| `CARD_TEMPLATE_DELETED` | 410 | Template mới đã xóa mềm |
| `CARD_TEMPLATE_INACTIVE` | 409 | Template mới không active |
| `CARD_TEMPLATE_TYPE_INVALID` | 409 | Template mới không thuộc loại AI |
| `INTERNAL_SERVER_ERROR` | 500 | AI, upload hoặc persistence thất bại sau xử lý nội bộ |

## 11. API inventory

| Chức năng | Method | Route | TDD |
|---|---|---|---|
| Create/Regenerate Card AI | POST | `/api/ai-cards` | TDD-006 |
| Tạo Card HandMade | POST | `/api/handmade-cards` | TDD-021 |
| Danh sách Card Config | GET | `/api/card-configs` | TDD-008 |
| Tạo Card Config | POST | `/api/card-configs` | TDD-009 |
| Cập nhật Card Config | PUT | `/api/card-configs/{configId}` | TDD-010 |
| Xóa Card Config | DELETE | `/api/card-configs/{configId}` | TDD-011 |
| Danh sách Card Template | GET | `/api/card-templates` | TDD-022 |
| Chi tiết Card Template | GET | `/api/card-templates/{templateId}` | TDD-023 |
| Tạo Card Template | POST | `/api/card-templates` | TDD-024 |
| Cập nhật Card Template | PUT | `/api/card-templates/{templateId}` | TDD-025 |
| Xóa Card Template | DELETE | `/api/card-templates/{templateId}` | TDD-026 |

## 12. Quyết định đã chốt

- Không còn câu hỏi mở về cách truyền Card AI vào Order trong tài liệu này: Order/OrderDetail và SnapshotCard đã tồn tại trước Create.
- Cùng một `orderDetailId` có thể được gọi nhiều lần; mỗi lần hợp lệ tạo Card mới.
- `validatedAt` của Create là thời điểm tạo snapshot; của Regenerate là thời điểm chuẩn bị request hiện tại.
- Card metadata không thêm `orderDetailId` và không dùng `schemaVersion`; vẫn giữ `sizeConfig`.
- Không tạo Unit Test trong phạm vi thay đổi tài liệu này.
