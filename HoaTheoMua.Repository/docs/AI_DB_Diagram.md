# AI Customize Database Schema - HTM_Flourist_AI

> Document First Project: hoa-theo-mua-ai-customize  
> Date: 2026-09-11

## Phạm vi và nguyên tắc

- Flow tạo hoa của STORY-030 và STORY-033 dùng chung một API đồng bộ.
- Không có bảng `flower_requests` hoặc `flower_ai_jobs`.
- API nhận input, gọi AI (kèm retry), trả kết quả; chỉ khi thành công mới lưu `GeneratedFlower` và `ClientHistory`.
- `userInput` của Flower không được lưu. Dữ liệu kết quả nằm ở `GeneratedFlower`; tên, thuộc tính, ảnh và trạng thái Product cùng Mockup/cấu hình cần dùng lại nằm ở `ClientHistory.Metadata`.
- `ClientHistory.RootID` là UUID bắt buộc, luôn lưu `products.id` gốc của History `flower`, `card` hoặc `handmade_card`. Giá trị do service xác định; không tạo FK, navigation hoặc quan hệ database cho cột này.
- Card AI Create đọc toàn bộ dữ liệu từ `OrderDetail.SnapshotCard`; Regenerate dùng snapshot của `GeneratedCard` và `ClientHistory` nguồn. Hai mode không đọc Card Config live. Quan hệ Card–Flower tiếp tục được suy ra từ `ClientHistory`, không dùng `GeneratedCard.source_flower_id`.
- `CardTemplate` dùng string bắt buộc `template_type="ai"|"handmade"`; cả hai loại kết quả đều lưu trong `GeneratedCard` và phân biệt bằng string bắt buộc `card_type`.
- Khi áp dụng schema cho dữ liệu hiện có, backfill `CardTemplate.TemplateType="ai"` và `GeneratedCard.CardType="ai"` trước khi bật ràng buộc không null. Hai discriminator chỉ nhận lowercase; `template_type` không được đổi sau khi tạo.
- HandMade Card không gọi AI, không dùng prompt/quota, luôn viết tay và chỉ tính phụ phí số từ; `system_form` của history bằng null.
- `ClientHistory.Type` dùng enum `ClientHistoryType`. Phạm vi hiện tại của `RootID` gồm `flower`, `card`, `handmade_card`; `post` chưa được sử dụng trong flow này và không được suy diễn quy tắc `RootID`.
- Không thêm bảng AI mới. Sơ đồ chỉ tham chiếu `OrderDetail` có sẵn ở Core Database. `ClientHistory.Metadata` và `ClientHistory.SystemForm` giữ dữ liệu bất biến cần dùng lại; soft-delete nguồn không xóa History.
- `OrderDetail.SnapshotCard` là JSON bất biến được chuẩn bị sau khi có Order/OrderDetail ở flow ngoài TDD-006. API Card AI chỉ đọc field này và không ghi Generated Card ID ngược về OrderDetail.
- `products.sellableQuantity` là giá trị dẫn xuất từ dữ liệu live bằng cùng thuật toán `CalculateAvailableQty`, không phải column trong schema.

## Enums

```sql
Enum PlatformType { zalo facebook instagram }
Enum SystemPromptType { flower card post }
Enum CardFormType { go_may calligraphy }
Enum ClientHistoryType { flower card handmade_card post }
```

## Tables

```sql
Table SystemPrompt {
  Id uuid [pk]
  Type SystemPromptType
  Content text
}

Table PlatformStandard {
  Id uuid [pk]
  Platform PlatformType
  Description text
}

Table Config {
  id uuid [pk]
  key varchar
  value jsonb
  is_public boolean
  group varchar
  kind varchar
  user_id uuid
  created_at timestamp
  updated_at timestamp
  is_deleted boolean
  // Card dùng 2 singleton: card_config.sizes và card_config.prices; group/key/kind bất biến.
  // card_config.sizes là một record có items gồm một hoặc nhiều Size; client chọn theo items[].name.
}

Table products {
  id uuid [pk]
  is_active boolean
  is_deleted boolean
  // sellableQuantity là derived value, không lưu column
}

Table OrderDetail {
  Id uuid [pk]
  OrderId uuid
  SnapshotCard json              // Order Card Snapshot; nullable khi OrderDetail không dùng Card AI
}

Table CardTemplate {
  Id uuid [pk]
  Name varchar
  Description text
  ImageUrl varchar
  TemplateType varchar          // "ai" | "handmade", bắt buộc; không đổi sau khi tạo
  IsActive boolean
  CreatedAt timestamp
  UpdatedAt timestamp
  IsDeleted boolean
}

Table BasePost {
  Id uuid [pk]
  Content text
  ImageUrl varchar
}

Table GeneratedPost {
  Id uuid [pk]
  Content text
  ImageUrl varchar
  UserId uuid
  CreatedAt timestamp
}

Table GeneratedFlower {
  Id uuid [pk]
  ImageUrl varchar
  UserId uuid
  CreatedAt timestamp
}

Table GeneratedCard {
  Id uuid [pk]
  Content text
  ImageUrl varchar                 // ảnh hoàn chỉnh sau khi chèn chữ
  RawImage varchar                  // ảnh gốc AI sinh trước khi chèn chữ; nullable cho legacy
  UserId uuid
  CreatedAt timestamp
  CardType varchar              // "ai" | "handmade", bắt buộc
  FormType FormType
  SenderName varchar
  ReceiverName varchar
  MessageContent text
  AttachedImageUrl varchar     // null với handmade
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

Table PostHistory {
  Id uuid [pk]
  Input json
  BaseId uuid
  OutputId uuid
  SystemPromptId uuid
  Metadata json
  CreatedAt timestamp
}

Table Mockup {
  Id uuid [pk]
  Name varchar
  Description text
  ImageUrl varchar
  IsActive boolean
  IsDeleted boolean
  CreatedAt timestamp
  UpdatedAt timestamp
}

Table ClientHistory {
  Id uuid [pk]
  UserId uuid
  Type ClientHistoryType      // flower | card | handmade_card; post hiện chưa dùng
  SystemForm json [null]         // snapshot SystemPrompt trừ Id; null với handmade_card
  Metadata json
  CreatedAt timestamp
  RootID uuid [not null]          // products.Id gốc; do code gán, không có FK
  BaseId uuid
  OutputId uuid
}
```

Ràng buộc nghiệp vụ cho `GeneratedCard.CardType="handmade"`: `FormType="calligraphy"`, `attached_image_url=null`, `base_price=0`, `total_price=extra_price`. `size_base_price` vẫn giữ giá Size Config tại thời điểm tạo để audit. Với Card AI mới, `raw_image` và `image_url` phải cùng tồn tại sau persist; `raw_image` nullable chỉ để giữ tương thích dữ liệu cũ.

## Quan hệ

`ClientHistory` dùng các tham chiếu đa hình; `RootID` luôn là Product ID gốc:

| type | base_id | output_id | RootID | metadata |
|---|---|---|---|---|
| flower | `products.id` | `GeneratedFlower.Id` | Bằng `base_id` | Tên, thuộc tính, ảnh, trạng thái Product; Mockup, logo/config, thao tác và nguồn tạo lại; System Prompt nằm trong `system_form`; không có `userInput` |
| card (hoa thường) | `products.id` | `GeneratedCard.Id` | Bằng `base_id` | Provenance, `source{type,id,imageUrl}`, Template snapshot gồm ảnh, nguồn prompt và thao tác |
| card (hoa AI) | `GeneratedFlower.Id` | `GeneratedCard.Id` | `base_id` của `ClientHistory(type=flower, output_id=GeneratedFlower.Id)` | Provenance bắt buộc đầy đủ, ảnh nguồn hoa và Template snapshot gồm ảnh |
| handmade_card (hoa thường) | `products.id` | `GeneratedCard.Id` | Bằng `base_id` | Provenance và Template snapshot không gồm ảnh; `system_form=null` |
| handmade_card (hoa AI) | `GeneratedFlower.Id` | `GeneratedCard.Id` | `base_id` của Flower History tương ứng | Provenance và Template snapshot không gồm ảnh; `system_form=null` |

Đối với các loại đang dùng trong `ClientHistory`, column `input` bị loại bỏ. `system_form` là JSON nullable, không có FK: AI Flower/Card lưu `{type, content}` của System Prompt thực tế và HandMade lưu `null`. `PostHistory` là bảng khác nên tiếp tục giữ `system_prompt_id` hiện tại; Post nằm ngoài phạm vi quy tắc `RootID` này.

Flower History dùng metadata `schemaVersion=3` với các nhóm dữ liệu: `operation`, `product{name, attributes, image, observedStatus}`, `mockup{id, name, description, image, observedStatus}`, `logoConfig`, `regeneratedFromFlowerId`, `systemPromptSource` và `validatedAt`. Không lưu Product ID trong metadata vì đã có ở `base_id/RootID`; không lưu Flower ID kết quả vì đã có ở `output_id`; không lưu System Prompt vì đã có ở `system_form`; không lưu `userInput`.

Card History mới không dùng `schemaVersion`. Metadata lưu `operation=create|regenerate`, provenance, `source{type,id,imageUrl}`, `cardTemplate{id,name,templateType,imageUrl}`, `sizeConfig`, `regeneratedFromCardId`, `systemPromptSource` và `validatedAt`. Không lưu `orderDetailId`. Regenerate lấy ảnh hoa từ `metadata.source.imageUrl`, không dùng `GeneratedCard.RawImage`; Card legacy thiếu ảnh nguồn hoặc ảnh Template trong metadata không đủ điều kiện Regenerate.

```sql
Ref: PostHistory.SystemPromptId > SystemPrompt.Id
```

Không khai báo `Ref` cho `ClientHistory.BaseId`, `RootID` hoặc `output_id`. Đây là các giá trị lineage đa hình do code quản lý, không phải quan hệ database.

`mockup` được truyền vào API tạo hoa và lưu trong `ClientHistory.Metadata`; không cần FK riêng cho flow tạo hoa. Flower History metadata là nguồn duy nhất của Product và Mockup khi tạo lại.

Lineage dùng để chuẩn bị `OrderDetail.SnapshotCard` ở flow Order nằm ngoài TDD-006:

```text
generated_card
  -> ClientHistory(type=card hoặc handmade_card, output_id=generated_card.id)
  -> RootID = product.id gốc
  -> base_id = product.id
     hoặc base_id = generated_flower.id
        -> ClientHistory(type=flower, output_id=generated_flower.id).base_id = product.id
  -> tạo Order/OrderDetail và chụp SnapshotCard
```

Không được thêm `GeneratedCard.source_flower_id`. Card AI history cũ không resolve được Product gốc hoặc thiếu snapshot ảnh vẫn xem được nhưng không được dùng làm nguồn Regenerate; HandMade Card mới bắt buộc có provenance đầy đủ và không hỗ trợ Regenerate. Card AI Create từ một `orderDetailId` không phụ thuộc trạng thái hiện tại của Order hoặc dependency live.

Khi backfill dữ liệu cũ, History `flower` lấy `RootID=base_id`; Card trực tiếp từ Product lấy `RootID=base_id`; Card từ Generated Flower lấy `RootID` từ `base_id` của Flower History tương ứng. Không dùng `Guid.Empty`, không tự đoán và phải dừng migration nếu còn record `flower`, `card` hoặc `handmade_card` không xác định được Product ID gốc. `RootID` không được thêm vào response API công khai.
