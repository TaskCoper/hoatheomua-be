# AI Card Context - HTM_Flourist_AI

> Document First Project: hoa-theo-mua-ai-customize
> Date: 2026-08-27
> Last Updated: 2026-09-07

---

## MỤC LỤC

1. [Tổng quan](#1-tổng-quan)
2. [Luồng nghiệp vụ](#2-luồng-nghiệp-vụ)
3. [Database Schema](#3-database-schema)
4. [Quy tắc nghiệp vụ](#4-quy-tắc-nghiệp-vụ)
5. [Quota Rules](#5-quota-rules)
6. [Pricing Rules](#6-pricing-rules)
7. [API Endpoints](#7-api-endpoints)

---

## 1. TỔNG QUAN

### Mô tả
Hệ thống Card cho phép khách hàng tạo thiệp cá nhân hóa bằng AI hoặc chọn mẫu HandMade có sẵn do Admin quản lý. Hai flow dùng chung `card_templates`, `generated_cards`, `client_histories` và API tạo Card nhưng có dependency, giá và loại history riêng.

### Luồng chính
```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         CARD FLOW                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│  Checkout (từ Flower Flow)                                                 │
│      │                                                                      │
│      ▼                                                                      │
│  [STORY-035] "Tạo thiệp AI" / "Tạo lại" (trong Checkout)                 │
│  Hoặc chọn Template HandMade và xác nhận tạo thiệp viết tay               │
│      │                                                                      │
│      ▼                                                                      │
│  generated_cards + client_histories(type=card|handmade_card)               │
│      │                                                                      │
│      ▼                                                                      │
│  Kết thúc phạm vi Card; Checkout/Order được xử lý ở tài liệu khác          │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Stories liên quan
| Story | Tên | Sprint | Priority | Status |
|-------|-----|--------|----------|--------|
| **STORY-035** | Tạo thiệp thiết kế | 1 | Must | Draft |
| **STORY-036** | Tạo lại thiệp từ lịch sử | 1 | Must | Draft |
| **STORY-037** | Tạo lại thiệp AI, giữ thiết kế gốc và thay nội dung | 1 | Must | Draft |

---

## 2. LUỒNG NGHIỆP VỤ

### STORY-035: Tạo thiệp thiết kế

#### Metadata
- **Story**: Là một khách hàng đã đăng nhập, tôi muốn tạo thiệp mới tại Checkout, để có một mẫu thiệp cá nhân hóa phù hợp với đơn hàng của mình.
- **Context**: Khách hàng có thể tạo thiệp mới tại Checkout.
- **Sprint**: 1
- **Priority**: Must

#### Điều kiện

**Preconditions:**
- Khách hàng đã đăng nhập
- Checkout phải tồn tại, thuộc khách hàng hiện tại và chưa hoàn tất
- Có ít nhất 01 Template và 01 Size đang ở trạng thái khả dụng từ Core DB

**Trigger:**
Khách hàng chọn "Tạo thiệp" tại Checkout.

#### Input Data - Thông tin bắt buộc
| Trường | Giới hạn |
|--------|----------|
| Người gửi (`senderName`) | tối đa 20 từ |
| Người nhận (`receiverName`) | tối đa 20 từ |
| Lời chúc (`messageContent`) | tối đa 100 từ |
| Template (`cardTemplateId`) | bắt buộc - chọn từ `card_templates` |
| Size (`sizeKey`) | bắt buộc - chọn từ Config `group="card_size"` |
| Hình thức (`formType`) | Calligraphy hoặc Gõ máy |

#### Ảnh đính kèm
- Không bắt buộc
- Tối đa 01 ảnh
- Định dạng: PNG, JPG
- Dung lượng tối đa: 10 MB

#### Thông tin bổ sung cho AI Module
- **`cardSize`**: Thông tin size được chọn từ Config (`name`, `width`, `height`, `basePrice`, `maxWords`)
- **`cardConfig`**: Thông tin calligraphy config từ Config (`minWords`, `maxWords`, `extraPrice`)
- **`flowerInfo`**: Thông tin bó hoa mà thiệp dựa vào:
  - Nếu dựa vào bó hoa bình thường: `imageUrl` của Product
  - Nếu dựa vào bó hoa AI: `imageUrl` của Generated Flower

### STORY-036: Tạo lại thiệp từ lịch sử

#### Metadata
- **Story**: Là một khách hàng đã đăng nhập, tôi muốn tạo lại từ một thiệp đã có, để có một mẫu thiệp cá nhân hóa phù hợp với nhu cầu của mình.
- **Context**: Khách hàng có thể chọn "Tạo lại" tại màn Chi tiết thiệp đã tạo.
- **Sprint**: 1
- **Priority**: Must

#### Đặc điểm
- Chỉ Card AI có history `type="card"` thuộc khách hàng hiện tại được tạo lại; HandMade Card không hỗ trợ regenerate.
- Giữ nguyên nội dung, ảnh đính kèm, hình thức, size, giá, Config snapshot và lineage của Card nguồn.
- Không mở lại form nội dung; người dùng chỉ được chọn Template khác hoặc để trống để dùng lại Template nguồn.
- Kết quả mới không tự động gắn vào Checkout

### STORY-037: Tạo lại thiệp AI, giữ thiết kế gốc và thay nội dung

#### Đặc điểm
- Flow độc lập với STORY-036/TDD-007, chỉ nhận Card AI thuộc History của khách hàng.
- Card nguồn phải có `rawImage`: ảnh AI sinh trước khi chèn chữ. AI dùng ảnh này làm image-reference và trả cả `rawImage` mới lẫn `imageUrl` đã chèn chữ.
- Khóa Template và Size bằng snapshot nguồn, không validate live hai dependency này; request truyền chúng bị từ chối.
- Cho sửa sender, receiver, lời chúc, form type và ảnh đính kèm. Sender/receiver/message không truyền, null, rỗng hoặc blank dùng lại giá trị nguồn; các giá trị hiệu lực được validate, tính số từ, Calligraphy và giá lại như Card Create.
- History mới kế thừa cột `base_id` gốc của History Card được chọn; `input.sourceCardId` và metadata lưu ID Card được chọn. Không thay đổi Order/Checkout trong flow này.

### Chọn thiệp HandMade có sẵn

- Admin quản lý chung `card_templates`; `template_type="ai"` dùng cho AI, `template_type="handmade"` dùng làm mẫu thiệp có sẵn. Field bắt buộc, không nullable và không được đổi sau khi tạo; dữ liệu cũ mặc định `ai`.
- Client gọi chung `POST /api/ai-cards`. Service load Template trước: `ai` chạy flow AI hiện tại; `handmade` chạy flow không AI.
- HandMade chỉ nhận nguồn hoa, `cardTemplateId`, `sizeKey`, sender, receiver và message. Nếu truyền `formType` hoặc `attachedImageUrl`, trả lỗi riêng `400`.
- HandMade luôn `formType="calligraphy"`; ảnh kết quả dùng trực tiếp `card_templates.image_url`, không render nội dung lên preview, không gọi AI/System Prompt/retry và không consume quota AI.
- Size và Calligraphy Config được validate live đúng một lần để snapshot. Size chỉ quyết định kích thước/max words, không tính tiền: giữ `sizeBasePrice` theo Config để audit nhưng `basePrice=0`; `extraPrice` tính theo word count và `totalPrice=extraPrice`.
- Chỉ khi client xác nhận bằng cách gửi request hợp lệ mới transaction tạo `generated_cards(card_type="handmade")` và `client_histories(type="handmade_card")`. Không gọi API thì không có record.
- HandMade không hỗ trợ regenerate. API Regenerate nhận source history `handmade_card` trả `HANDMADE_CARD_REGENERATE_NOT_SUPPORTED/409`.

---

## 3. DATABASE SCHEMA

### 3.1 Enums

```sql
Enum form_type {
  go_may       -- Gõ máy
  calligraphy  -- Calligraphy
}

Enum system_prompt_types {
  flower
  card
  post
}
```

### 3.2 Tables

#### Config (Bảng có sẵn - sử dụng cho card configs)

**Quy tắc sử dụng field:**
- `is_public`: true = đang hoạt động (người dùng có thể chọn), false = không hoạt động
- `is_deleted`: true = đã xóa mềm, false = chưa xóa

```sql
Table Config {
  id uuid [pk]
  key varchar                    -- Key định danh: "size_A", "size_B", "calligraphy_words"
  value jsonb                   -- JSON chứa cấu hình chi tiết
  is_public boolean              -- Dùng để xác định config có đang hoạt động để người dùng chọn
  group varchar                  -- "card_size" hoặc "card_config"
  kind varchar                   -- "Setting" cho card configs
  user_id guid
  created_at timestamp
  updated_at timestamp
  is_deleted boolean            -- Dùng cho xóa mềm
}
```

**Group values:**
- `card_size` - cấu hình size thiệp
- `card_config` - cấu hình calligraphy words

Cho phép nhiều Config cùng `group`, nhưng mỗi lựa chọn dùng `key` riêng (`size_A`, `size_B`, ...). Trong các record chưa xóa, chỉ được có một record `is_public = true` cho cùng cặp `(group, key)`. Dữ liệu cũ có nhiều record active cùng cặp bị chặn bằng `CARD_CONFIG_AMBIGUOUS/409`; không chọn ngẫu nhiên. Calligraphy dùng đúng cặp `group=card_config`, `key=calligraphy_words`, `value` là mảng rule giá.

**Kind values:**
- `Setting` - cho card configs

#### system_prompts

Prompt hệ thống cho từng loại generation.

```sql
Table system_prompts {
  id uuid [pk]
  type system_prompt_types [unique] -- mỗi type có đúng một prompt hiện hành
  content text
}
```

#### card_templates

Template dùng để tạo Card AI hoặc làm mẫu Card HandMade có sẵn.

```sql
Table card_templates {
  id uuid [pk]
  name varchar
  description text
  image_url varchar
  template_type varchar         -- "ai" | "handmade", bắt buộc, bất biến
  is_active boolean
  created_at timestamp
  updated_at timestamp
  is_deleted boolean
}
```

#### generated_cards

Card được tạo từ AI hoặc từ Template HandMade.

```sql
Table generated_cards {
  id uuid [pk]
  content text                  -- Nội dung thiệp
  image_url varchar             -- URL ảnh thiệp hoàn chỉnh sau chèn chữ
  raw_image varchar             -- URL ảnh gốc AI trước chèn chữ; nullable cho Card legacy
  user_id uuid
  created_at timestamp
  card_type varchar             -- "ai" | "handmade", bắt buộc

  -- Thông tin thiệp:
  form_type form_type           -- "go_may" hoặc "calligraphy"
  size_key varchar              -- Key của size: "size_A", "size_B"
  sender_name varchar           -- Người gửi
  receiver_name varchar         -- Người nhận
  message_content text          -- Lời chúc
  attached_image_url varchar     -- Ảnh đính kèm; luôn null với HandMade

  -- Snapshot Size tại thời điểm tạo:
  size_name varchar              -- "A", "B", "C"
  size_width decimal             -- 10 (cm)
  size_height decimal            -- 15 (cm)
  size_base_price decimal       -- Giá base tại thời điểm đó: 100000
  size_max_words int            -- Giới hạn từ của size đó: 100

  -- Snapshot Word count tại thời điểm tạo:
  word_count int               -- Số từ thực tế: 60
  word_config_snapshot json     -- JSON lưu rule áp dụng: { "minWords": 36, "maxWords": 70, "extraPrice": 39000 }

  -- Giá đã tính toán:
  base_price decimal            -- AI: size_base_price; HandMade: 0
  extra_price decimal           -- Phụ phí Calligraphy = word_config_snapshot.extra_price
  total_price decimal           -- AI: base_price + extra_price; HandMade: extra_price
}
```

#### client_histories

Lịch sử thao tác của người dùng (dùng chung cho flower và card).

```sql
Table client_histories {
  id uuid [pk]
  user_id uuid                  -- Ai tạo history
  input jsonb                   -- JSON request gốc
  type varchar                   -- "flower", "card", "handmade_card", "post"
  system_prompt_id uuid [null]   -- null khi type="handmade_card"
  metadata json                 -- Snapshot/provenance bất biến, schemaVersion=2
  created_at timestamp

  -- base_id: polymorphic reference - xác định nguồn gốc
  --   - Nếu type = "card" hoặc "handmade_card" và base là combo: product.id
  --   - Nếu type = "card" hoặc "handmade_card" và base là bó hoa AI: generated_flowers.id
  --   - Nếu type = "flower": reference đến product.id (combo)
  base_id uuid

  -- output_id: polymorphic reference đến kết quả
  --   - Nếu type = "card" hoặc "handmade_card": generated_cards.id
  --   - Nếu type = "flower": reference đến generated_flowers.id
  output_id uuid

  -- Refs:
  system_prompt_id -> system_prompts.id
}
```

### 3.3 Mối quan hệ giữa client_histories và Card

#### Trường hợp 1: Thiệp AI cho bó hoa bình thường
```
base_id = product.id (combo không có biến thể HOẶC biến thể là con của biến thể cha)
output_id = generated_cards.id
metadata = { ...card_template_info... }
```
**Ý nghĩa:** Thiệp AI được tạo ra cho bó hoa bình thường, dựa vào `cardTemplate` mà người dùng chọn.

#### Trường hợp 2: Thiệp AI cho bó hoa AI
```
base_id = generated_flowers.id
output_id = generated_cards.id
metadata = { ...card_template_info... }
```
**Ý nghĩa:** Thiệp AI được tạo cho bó hoa AI, dựa vào `cardTemplate` mà người dùng chọn.

#### Trường hợp 3: Thiệp HandMade

```text
type = handmade_card
base_id = product.id hoặc generated_flowers.id
output_id = generated_cards.id
system_prompt_id = null
metadata = snapshot Template HandMade + Size + Calligraphy + provenance; không có `systemPrompt`
```

Lineage đều nằm trong cùng bảng `client_histories`, không có bảng Flower History/Card History riêng:

```text
GeneratedCard
  -> client_histories(type=card hoặc handmade_card, output_id=GeneratedCardId)
     base_id = ProductId
     hoặc base_id = GeneratedFlowerId
        -> client_histories(type=flower, output_id=GeneratedFlowerId).base_id = ProductId
```

Với dữ liệu cũ có Generated Flower đúng ownership và đủ ảnh nhưng không có record `client_histories(type=flower)` hợp lệ: vẫn cho tạo Card; metadata Card ghi `originProductId=null`, `provenanceStatus="unavailable"`. Regenerate vẫn được nếu snapshot Card đủ; Order bị chặn `SOURCE_PRODUCT_PROVENANCE_INVALID/409`.

### 3.4 Cấu trúc JSON

#### Size Config
```json
{
  "name": "A",
  "width": 10,
  "height": 15,
  "basePrice": 100000,
  "maxWords": 100
}
```

#### Calligraphy Words Config
```json
[
  { "minWords": 0, "maxWords": 35, "extraPrice": 0 },
  { "minWords": 36, "maxWords": 70, "extraPrice": 39000 },
  { "minWords": 71, "maxWords": 100, "extraPrice": 69000 }
]
```

#### Card Template Snapshot
```json
{
  "templateId": "uuid",
  "templateName": "Tên template",
  "templateType": "ai|handmade",
  "templateImageUrl": "url"
}
```

#### `client_histories.metadata` khi `type=card` — `schemaVersion=2`

```json
{
  "schemaVersion": 2,
  "sourceType": "product|generated_flower",
  "originProductId": "uuid-or-null",
  "generatedFlowerId": "uuid-or-null",
  "provenanceStatus": "available|unavailable",
  "cardTemplate": {
    "id": "uuid",
    "name": "Template A",
    "image": { "url": "https://...", "objectKey": "templates/a.png", "versionId": "v3", "sha256": "..." }
  },
  "sizeConfig": { "group": "card_size", "key": "size_A", "value": {} },
  "calligraphyConfig": { "group": "card_config", "key": "calligraphy_words", "value": [] },
  "systemPrompt": { "id": "uuid", "content": "Full prompt đã dùng", "sha256": "..." },
  "validatedAt": "2026-08-28T08:00:00Z"
}
```

Với Card được tạo lại, `metadata` bổ sung `operation="regenerate"`, `regeneratedFromCardId` và snapshot của Effective Template thực tế. Cột `base_id` vẫn giữ nguồn Product/Generated Flower của history Card nguồn, không đổi thành ID Card nguồn. Field `systemPromptSource` có giá trị `current` hoặc `source_card_fallback` để phân biệt prompt hiện hành với prompt snapshot được dùng dự phòng.

Với TDD-027, `metadata` bổ sung `operation="recreate_content"`, `recreatedFromCardId` và `referenceRawImage`; Template/Size là snapshot Card nguồn, Calligraphy/pricing là snapshot mới được tính tại admission. Cột `base_id` vẫn kế thừa `base_id` gốc của History nguồn; không đổi thành Card ID. Card nguồn thiếu `rawImage` trả `CARD_SOURCE_RAW_IMAGE_MISSING/409`.

#### `client_histories.metadata` khi `type=handmade_card`

```json
{
  "schemaVersion": 2,
  "cardType": "handmade",
  "sourceType": "product|generated_flower",
  "originProductId": "uuid",
  "generatedFlowerId": "uuid-or-null",
  "provenanceStatus": "available",
  "cardTemplate": {
    "id": "uuid",
    "name": "Mẫu thiệp viết tay",
    "templateType": "handmade",
    "description": "...",
    "imageUrl": "https://..."
  },
  "sizeConfig": { "group": "card_size", "key": "size_A", "value": {} },
  "calligraphyRule": { "minWords": 36, "maxWords": 70, "extraPrice": 39000 },
  "pricing": { "configuredSizeBasePrice": 100000, "chargedBasePrice": 0, "extraPrice": 39000, "totalPrice": 39000 },
  "validatedAt": "2026-09-04T08:00:00Z"
}
```

HandMade không có `systemPrompt` trong metadata và không có các field snapshot dành riêng cho lần gọi AI.

`client_histories.input` của regenerate chỉ ghi hành động client thực sự gửi:

```json
{
  "operation": "regenerate",
  "sourceCardId": "uuid",
  "cardTemplateId": "uuid-or-null"
}
```

---

## 4. QUY TẮC NGHIỆP VỤ

### 4.0 Dependency matrix và validation boundary

| Flow | Product | Generated Flower | Template | Size Config | Calligraphy Config |
|---|---|---|---|---|---|
| Card từ Product | Validate live lúc admission | N/A | Validate live | Validate live | Validate live nếu Calligraphy |
| Card từ Generated Flower | Không revalidate Product/Mockup gốc | Tồn tại + ownership; ảnh đủ dùng | Validate live | Validate live | Validate live nếu Calligraphy |
| HandMade từ Product | Validate live lúc admission | N/A | Bắt buộc `templateType=handmade`, validate live | Validate live để snapshot, không thu base price | Validate live, luôn áp dụng |
| HandMade từ Generated Flower | Không revalidate Product/Mockup gốc | Tồn tại + ownership + provenance | Bắt buộc `templateType=handmade`, validate live | Validate live để snapshot, không thu base price | Validate live, luôn áp dụng |
| Regenerate | Không dùng trạng thái Product để chặn | Không revalidate nguồn hoa | Chỉ validate live khi client chọn ID Template khác nguồn; bỏ trống/null/đúng ID nguồn thì dùng snapshot nguồn, không validate live | Dùng snapshot Card nguồn, không validate live | Dùng snapshot Card nguồn, không validate live |
| Recreate content (TDD-027) | Không dùng trạng thái Product để chặn | Không revalidate nguồn hoa | Khóa snapshot Card nguồn; request không được gửi Template | Khóa snapshot Card nguồn; request không được gửi Size | Validate live nếu form type hiệu lực là Calligraphy; tính giá/snapshot mới |
| Order | Validate Product gốc live | Resolve lineage trong `client_histories` | Không quyết định tồn kho | Không quyết định tồn kho | Không quyết định tồn kho |

Thứ tự endpoint Card Create: `auth/request → load Template để phân nhánh`. Nếu `templateType=ai`: `source Product hoặc Generated Flower → xác định base_id quota → validate Template AI → Size Config → Calligraphy Config → system prompt → reserve atomically quota ngày + quota theo base_id → CardGenerationSnapshot → AI`. Nếu `templateType=handmade`, tiếp tục theo thứ tự HandMade bên dưới.

Thứ tự Regenerate: `auth/request → ownership → source Card + client_histories snapshot → chặn handmade_card → lấy base_id kế thừa làm quota source key → resolve Effective Template (chỉ validate live nếu ID khác nguồn) → system prompt hiện hành hoặc source fallback → reserve atomically quota ngày + quota theo base_id → RegenerationSnapshot → AI`.

Thứ tự Recreate content: `auth/request → source Card AI + History + ownership → rawImage nguồn → lấy cột base_id kế thừa làm quota source key → fallback/validate input hiệu lực → khóa Template/Size snapshot → Calligraphy/pricing live → system prompt current → reserve atomically quota ngày + quota theo base_id → CardRecreationSnapshot → AI trả raw+final → transaction Card/History`.

Thứ tự HandMade: `auth/request → source Product hoặc Generated Flower → Template handmade → Size Config → Calligraphy Config/rule → snapshot → transaction generated_cards + client_histories`. Không có prompt/quota/AI/retry.

- Validation nguồn/dependency chỉ một lần trước AI. Sau snapshot, Admin inactive/soft-delete dependency không hủy request, retry hoặc persist.
- Một lần đầu + tối đa hai retry cùng dùng snapshot bất biến; không query lại availability và không tải lại URL ảnh live.
- Admission fail thì không AI, không record/history và không consume quota.
- Mọi flow Card AI (Create, Regenerate, Recreate content) đều bắt buộc reserve trong một thao tác atomic hai slot: quota ngày theo user và quota theo `base_id` nguồn. `base_id` có thể là `products.id` hoặc `generated_flowers.id`; hai loại nguồn dùng chung quota và quy tắc. Success consume cả hai; AI/upload/persistence fail release cả hai.

#### Error taxonomy và thứ tự

- Product: `PRODUCT_NOT_FOUND/404` → `PRODUCT_DELETED/410` → `PRODUCT_INACTIVE/409` → `PRODUCT_OUT_OF_STOCK/409`; `PRODUCT_NOT_SELLABLE/422`; `PRODUCT_AVAILABILITY_UNAVAILABLE/503`.
- Generated Flower: không tồn tại hoặc khác owner → `GENERATED_FLOWER_NOT_FOUND/404`; thiếu ảnh cho AI → `GENERATED_FLOWER_INPUT_INVALID/409`. Thiếu provenance nhưng có ảnh không chặn Card.
- Template client chủ động chọn cho Create hoặc chọn ID khác nguồn khi Regenerate: `CARD_TEMPLATE_NOT_FOUND/404` → `CARD_TEMPLATE_DELETED/410` → `CARD_TEMPLATE_INACTIVE/409`. Regenerate bỏ trống/null hoặc truyền đúng ID Template nguồn thì dùng snapshot nguồn, không query hay kiểm tra trạng thái live.
- Size: `CARD_SIZE_CONFIG_NOT_FOUND/404` → `CARD_SIZE_CONFIG_DELETED/410` → `CARD_SIZE_CONFIG_INACTIVE/409`.
- Calligraphy: `CALLIGRAPHY_CONFIG_NOT_FOUND/404` → `CALLIGRAPHY_CONFIG_DELETED/410` → `CALLIGRAPHY_CONFIG_INACTIVE/409`.
- Card Create không có prompt hiện hành type `card` hoặc content rỗng/không hợp lệ → `SYSTEM_PROMPT_INVALID/409`. Regenerate chỉ trả lỗi history nếu cả prompt hiện hành và prompt snapshot nguồn đều không dùng được.
- Nhiều Config active cùng `(group,key)` chỉ áp dụng cho Card Create; Regenerate không query Config live. Source Card thiếu snapshot regenerate → `CARD_SOURCE_HISTORY_INVALID/409`.
- Regenerate source có history `type="handmade_card"` → `HANDMADE_CARD_REGENERATE_NOT_SUPPORTED/409`.
- TDD-027: source không phải Card AI → `HANDMADE_CARD_RECREATE_NOT_SUPPORTED/409`; thiếu `rawImage` usable → `CARD_SOURCE_RAW_IMAGE_MISSING/409`; request gửi Template/Size/source flower → `RECREATE_LOCKED_FIELD_NOT_ALLOWED/400`. Prompt hiện hành type `card` thiếu/rỗng/invalid → `SYSTEM_PROMPT_INVALID/409`.

### 4.1 Logic xác định base_id cho thiệp

```csharp
public Guid DetermineBaseId(Guid? flowerRequestId, Guid? generatedFlowerId, Product product)
{
    // Nếu có generatedFlowerId (bó hoa AI) -> base_id = generatedFlowerId
    if (generatedFlowerId.HasValue)
        return generatedFlowerId.Value;

    // Nếu không có generatedFlowerId (bó hoa bình thường)
    // -> base_id = product.id (combo không có biến thể HOẶC biến thể là con của biến thể cha)
    return product.Id;
}
```

- Khi source là Product có biến thể, request bắt buộc dùng đúng `productId` biến thể; không dùng ID cha. Card Create kiểm tra trạng thái và lượng bán của đúng ID nhận được. Việc kiểm tra cha active là rule Order, không chặn Card Generate.
- Khi source là Generated Flower, chỉ kiểm tra record tồn tại/ownership/ảnh; không revalidate Product hoặc Mockup gốc.

### 4.2 Regenerate snapshot policy

- Mọi Card AI có history `type="card"` thuộc user đều có thể là Card nguồn. Card mới luôn có ID, ảnh kết quả và `createdAt` mới, lấy `userId` từ phiên đăng nhập; không ghi đè Card nguồn. History `handmade_card` bị chặn.
- Effective Template là `cardTemplateId` client chọn, hoặc Template snapshot của Card nguồn khi request để trống. Người dùng được chọn lại Template AI cũ hoặc Template AI khác; không được chuyển sang HandMade. Mọi Template AI tương thích với mọi size và form type.
- Nếu client bỏ trống/null hoặc truyền đúng ID Template nguồn, dùng nguyên Template snapshot nguồn, không query live và không kiểm tra active/soft-delete. Chỉ khi client chọn ID Template khác nguồn, Template mới phải tồn tại, chưa soft-delete, active và có `templateType="ai"` tại admission; Template HandMade trả `CARD_TEMPLATE_TYPE_INVALID/409`. Trạng thái Template cũ không chặn request. Dependency mới đổi trạng thái sau snapshot không ảnh hưởng request đang chạy.
- Dùng nguyên sender/receiver/message/attached image/form type/size, giá và Config snapshot của Card nguồn; không query hoặc validate Size/Calligraphy Config live và không lấy giá/rule mới.
- Mỗi `system_prompts.type` (`flower`, `card`, `post`) có đúng một prompt hiện hành. Regenerate ưu tiên prompt hiện hành của type `card`; record không tồn tại hoặc có content rỗng/không hợp lệ đều được xem là không khả dụng. Khi đó dùng full prompt snapshot của Card nguồn, không kiểm tra trạng thái record prompt nguồn, và ghi `systemPromptSource="source_card_fallback"`.
- Cột `base_id` history mới bằng `base_id` history Card nguồn; cột `output_id` bằng ID Card mới. `metadata.regeneratedFromCardId` giữ quan hệ Card cha-con.
- Product gốc inactive/deleted/out-of-stock không chặn Regenerate; chỉ Order kiểm tra Product live.

### 4.3 HandMade Card policy

- `cardTemplateId` phải trỏ tới Template có cột `template_type="handmade"`, tồn tại, chưa xóa và active. Template snapshot lưu đầy đủ nhưng `generated_cards.image_url` chỉ lưu trực tiếp URL Template hiện hành.
- Request bắt buộc source, `cardTemplateId`, `sizeKey`, sender, receiver, message. Truyền `formType` → `HANDMADE_CARD_FORM_TYPE_NOT_ALLOWED/400`; truyền `attachedImageUrl` → `HANDMADE_CARD_ATTACHMENT_NOT_ALLOWED/400`.
- Server gán `formType="calligraphy"`; áp dụng cùng quy tắc field, đếm từ, `sizeMaxWords` và matching Calligraphy rule như Card AI.
- `sizeBasePrice` snapshot đúng giá Config; `basePrice=0`; `extraPrice` bằng phụ phí rule số từ; `totalPrice=extraPrice`.
- Với HandMade, không gọi API thì không có record; request thành công tạo Card/history trong một transaction.
- History dùng `type="handmade_card"`, cột `system_prompt_id=null`, provenance đầy đủ, không có `systemPrompt`; cột `base_id` là Product hoặc Generated Flower trực tiếp, cột `output_id` là HandMade Generated Card mới.
- Không gọi AI, không dùng prompt, không retry và không reserve/consume quota AI.

### 4.4 Quy tắc đếm từ

- Một từ là một chuỗi ký tự liên tục được phân tách bởi khoảng trắng, tab hoặc ký tự xuống dòng
- Khoảng trắng ở đầu và cuối nội dung không được tính
- Nhiều khoảng trắng, tab hoặc ký tự xuống dòng liên tiếp được tính là một dấu phân tách
- Dấu câu đi liền với một từ không được tính thành từ riêng
- Nội dung chỉ gồm khoảng trắng được tính là 0 từ

### 4.5 Render nội dung thiệp

| Hình thức | Render Người gửi, Người nhận, Lời chúc | Ghi chú |
|-----------|-----------------------------------------|---------|
| Gõ máy | Hiển thị nguyên văn | AI giữ nguyên nội dung |
| Calligraphy | Không hiển thị trên ảnh | Lưu trong history, chỉ render khi in |
| HandMade | Không hiển thị trên ảnh mẫu | Lưu trong Card/history để viết tay hoặc in sau |

### 4.6 Ảnh đính kèm

- AI giữ nguyên nội dung gốc của ảnh
- Không cắt mất chủ thể, xoay, đổi màu, thêm/xóa hoặc làm biến dạng
- Được phép scale đồng dạng hoặc thêm khoảng đệm để phù hợp Template
- Ảnh kết quả lưu dưới định dạng PNG, không gắn logo

---

## 5. QUOTA RULES

### Giới hạn
| Loại | Giới hạn | Đơn vị |
|------|-----------|---------|
| Tạo thiệp AI | 10 lượt | /ngày/khách hàng |
| Tạo thiệp AI theo nguồn | 3 lượt | /ngày/khách hàng/`base_id` nguồn |

HandMade Card không thuộc hai quota này.

Quota theo nguồn dùng chung cho cả hai loại Card AI: Card từ Product dùng `base_id=productId`; Card từ Generated Flower dùng `base_id=generatedFlowerId`. Regenerate và Recreate content dùng `base_id` kế thừa từ History Card nguồn. Mỗi request Card AI luôn reserve đồng thời quota ngày và quota theo `base_id`.

### Quy trình Quota

1. **Reserve atomically cả hai slot** → quota 10 lượt/user/ngày và quota 3 lượt/user/base_id/ngày, sau dependency validation và trước snapshot/AI. Nếu một slot không reserve được thì không giữ slot còn lại.
2. **Consume cả hai slot** → khi AI, upload và persistence thành công
3. **Release cả hai reservation** → khi AI thất bại sau retry, upload thất bại hoặc persistence rollback

### Retry Logic

- **1 lần gọi đầu + tối đa 2 retry** (tổng tối đa 3 lần) với cùng snapshot/quota slot
- Nếu vẫn thất bại sau retry → **release cả quota ngày và quota theo base_id đã reserve**
- Mỗi request chỉ consume hoặc release mỗi slot đúng một lần

---

## 6. PRICING RULES

### 6.1 Lấy cấu hình từ bảng Config

```csharp
// Lấy cấu hình size
var sizeConfigs = await _dbContext.Config
    .Where(c => c.Group == "card_size" && c.Key == $"size_{size}" && c.Kind == "Setting" && !c.IsDeleted && c.IsPublic)
    .ToListAsync();
if (sizeConfigs.Count > 1) throw CardConfigAmbiguous();
var sizeConfig = sizeConfigs.SingleOrDefault() ?? throw CardSizeConfigNotFound();
// sizeConfig.Value: { "name": "A", "width": 10, "height": 15, "basePrice": 100000, "maxWords": 100 }

// Lấy cấu hình calligraphy words
var wordsConfigs = await _dbContext.Config
    .Where(c => c.Group == "card_config" && c.Key == "calligraphy_words" && c.Kind == "Setting" && !c.IsDeleted && c.IsPublic)
    .ToListAsync();
if (wordsConfigs.Count > 1) throw CardConfigAmbiguous();
var wordsConfig = wordsConfigs.SingleOrDefault() ?? throw CalligraphyConfigNotFound();
// wordsConfig.Value: [ { "minWords": 0, "maxWords": 35, "extraPrice": 0 }, ... ]
```

### 6.2 Tính giá

```csharp
public async Task<decimal> CalculateExtraPriceAsync(string formType, int wordCount)
{
    if (formType != "calligraphy")
        return 0;

    var wordsConfig = await GetCalligraphyWordsConfigAsync();
    var rules = JsonSerializer.Deserialize<List<WordRule>>(wordsConfig.Value);

    var rule = rules.FirstOrDefault(r => wordCount >= r.MinWords && wordCount <= r.MaxWords);
    if (rule is null)
        throw ValidationError("Số từ trong lời chúc không nằm trong giới hạn cho phép của hình thức calligraphy.");

    return rule.ExtraPrice;
}

// AI Card: basePrice = sizeBasePrice; totalPrice = basePrice + extraPrice
// HandMade: basePrice = 0; totalPrice = extraPrice
```

### 6.3 Ví dụ kết quả Card AI

**Input:**
- size: "A"
- `formType`: `"calligraphy"`
- `messageContent`: `"Chúc bạn sinh nhật vui vẻ, hạnh phúc và thành công trong cuộc sống"`

**Xử lý:**
1. Size config (`Group="card_size"`, `Key="size_A"`): `{ "name": "A", "width": 10, "height": 15, "basePrice": 100000, "maxWords": 100 }`
2. Word count: 12
3. Word rule: `{ "minWords": 0, "maxWords": 35, "extraPrice": 0 }`

**Generated Card:**
```json
{
  "sizeKey": "size_A",
  "sizeName": "A",
  "sizeWidth": 10,
  "sizeHeight": 15,
  "sizeBasePrice": 100000,
  "sizeMaxWords": 100,
  "wordCount": 12,
  "wordConfigSnapshot": { "minWords": 0, "maxWords": 35, "extraPrice": 0 },
  "basePrice": 100000,
  "extraPrice": 0,
  "totalPrice": 100000
}
```

---

## 7. API ENDPOINTS

### TDD-006: Tạo thiệp mới
- **Method**: POST
- **Route**: `/api/ai-cards`
- **Phân nhánh**: field `templateType="ai"` theo TDD-006; `"handmade"` theo TDD-021

### TDD-007: Tạo lại thiệp từ History
- **Method**: POST
- **Route**: `/api/ai-cards/{sourceCardId}/regenerate`
- **Body**: `{ "cardTemplateId": "uuid-or-null" }`; null, bỏ field hoặc đúng ID Template nguồn thì dùng snapshot Template nguồn, không validate live
- **Giới hạn**: chỉ history `type="card"`; `handmade_card` trả `HANDMADE_CARD_REGENERATE_NOT_SUPPORTED/409`

### TDD-027: Tạo lại thiệp AI, giữ thiết kế gốc và thay nội dung
- **Method**: POST
- **Route**: `/api/ai-cards/{sourceCardId}/recreate-content`
- **Source**: `sourceCardId` là `generated_cards.id` trong `client_histories.output_id`; chỉ Card AI của owner và bắt buộc có `rawImage`.
- **Body**: `senderName`, `receiverName`, `messageContent`, `formType`, `attachedImageUrl`. Template/Size/source flower bị khóa; null/rỗng/blank của ba field text dùng giá trị nguồn.

### TDD-021: Chọn và tạo thiệp HandMade
- **Method**: POST
- **Route**: `/api/ai-cards`
- **Body**: source + `cardTemplateId` + `sizeKey` + sender/receiver/message; không nhận `formType` hoặc ảnh đính kèm

### Card Template list/admin

- Danh sách hỗ trợ filter string `templateType=ai|handmade`; màn AI bắt buộc lấy `ai`, màn chọn thiệp có sẵn bắt buộc lấy `handmade`.
- Admin Create bắt buộc truyền `templateType`; Admin Update không được thay đổi discriminator này.
- `templateType` sai hoặc viết khác lowercase `ai|handmade` bị từ chối validation.

| TDD | Chức năng | Method | Route | Quyền |
|---|---|---|---|---|
| TDD-022 | Lấy danh sách Card Template | GET | `/api/card-templates` | Admin/Customer theo visibility |
| TDD-023 | Lấy chi tiết Card Template | GET | `/api/card-templates/{id}` | Admin/Customer theo visibility |
| TDD-024 | Tạo Card Template | POST | `/api/card-templates` | Admin |
| TDD-025 | Cập nhật Card Template | PUT | `/api/card-templates/{id}` | Admin |
| TDD-026 | Xóa mềm Card Template | DELETE | `/api/card-templates/{id}` | Admin |

### Client History list

- Khi không truyền filter loại Card, danh sách Card history trả cả `type="card"` và `type="handmade_card"`.
- Filter `type=card` chỉ trả Card AI; filter `type=handmade_card` chỉ trả HandMade Card.
- History HandMade vẫn có `base_id`, `output_id`, input và metadata snapshot như quy định, nhưng `system_prompt_id=null`.

### Card Config CRUD (Sử dụng bảng Config có sẵn)
| TDD | Chức năng | Method | Route | Quyền |
|-----|-----------|--------|-------|--------|
| TDD-009 | Tạo card config | POST | `/api/v1/configs` | Admin |
| TDD-008 | Lấy danh sách | GET | `/api/v1/configs/settings` | Admin |
| TDD-010 | Cập nhật card config | PUT | `/api/v1/configs/{id}` | Admin |
| TDD-011 | Xóa card config | DELETE | `/api/v1/configs/{id}` | Admin |

---

## Ghi chú quan trọng

- **`source_flower_id` đã được bỏ khỏi `generated_cards`** - mối quan hệ được xác định qua `client_histories`
- **thiệp không gắn logo** - ảnh kết quả được lưu dưới định dạng PNG mà không có logo
- **Discriminator dạng string** - `template_type/card_type` dùng `ai|handmade`; history dùng `flower|card|handmade_card|post`
- **Snapshot giá** - tất cả thông tin ảnh hưởng đến giá được lưu tại thời điểm tạo thiệp
- **AI Module tách riêng** - các hàm xử lý logic gọi đến AI Module để thực hiện việc gọi AI
- **Kết quả AI Card** - Card AI mới trả cặp field ảnh: `rawImage` (trước chèn chữ) và `imageUrl` (sau chèn chữ). Thiếu/upload lỗi một ảnh được xem là cả attempt thất bại, retry/release quota theo cùng policy.
- **Input cho Card AI**: `senderName`, `receiverName`, `messageContent`, `formType`, `sizeKey`, `cardTemplateId`, `attachedImageUrl`, `cardSize` (từ Config), `cardConfig` (từ Config), `flowerInfo` (`imageUrl` của Product hoặc Generated Flower)
- **Input cho HandMade Card**: đúng một source, `cardTemplateId`, `sizeKey`, `senderName`, `receiverName`, `messageContent`; server gán Calligraphy và không nhận `attachedImageUrl`
- **Input cho Regenerate**: `sourceCardId` ở route và `cardTemplateId` tùy chọn trong body; các dữ liệu còn lại lấy từ Card/history nguồn.
- **Input cho Recreate content (TDD-027)**: `sourceCardId` ở route; chỉ sửa sender/receiver/message/form type/ảnh đính kèm. Dùng raw image Card nguồn làm reference, khóa Template/Size snapshot và tính lại Calligraphy/pricing.

### Order context và quyết định còn mở

Order phải resolve `originProductId` trong metadata qua chuỗi `client_histories`, kiểm tra Product live và `sellableQuantity >= tổng quantity` rồi mới tiếp tục kiểm kho depot Nhanh.vn. Nếu không resolve được lineage, chặn `SOURCE_PRODUCT_PROVENANCE_INVALID/409`.

Luồng context-only bắt buộc cho implementation Order tương lai:

1. Direct Product: dùng đúng Product/variant trong item.
2. Card từ Product: tìm `client_histories(type=card, output_id=GeneratedCardId).base_id`.
3. Card từ Generated Flower: từ record `client_histories(type=card)` lấy `base_id=GeneratedFlowerId`, rồi tìm record `client_histories(type=flower, output_id=GeneratedFlowerId).base_id=ProductId`. Có thể dùng `originProductId` trong metadata để resolve nhanh nhưng vẫn phải kiểm tra consistency.
4. Không resolve/không nhất quán: `SOURCE_PRODUCT_PROVENANCE_INVALID/409`; không tạo Order/OrderDetail và không chuẩn bị payment.
5. Product live giữ thứ tự not-found → deleted → inactive → quantity. Cộng gộp các dòng cùng Product và yêu cầu `sellableQuantity >= tổng Number`; thiếu lượng trả `PRODUCT_OUT_OF_STOCK/409`.
6. Nếu ID là variant con: kiểm tra child active, sau đó parent; parent inactive chặn Order bằng `PRODUCT_INACTIVE/409`. Sau local `CalculateAvailableQty`, vẫn chạy kiểm kho depot Nhanh.vn; không xác minh được external inventory trả `PRODUCT_AVAILABILITY_UNAVAILABLE/503`.
7. Vì hiện chưa reserve inventory, payment webhook dự kiến phải revalidate Product trước khi chuyển PAID/fulfillment; xử lý tiền đã nhận nhưng fail vẫn là open decision dưới đây.

Hai quyết định chưa được chốt và AI đọc Context ở phiên sau phải hỏi lại kèm bối cảnh:

1. `CreateOrderRequest` hiện không có `generatedCardId`. Chưa chốt cách Order nhận biết Card AI. Phương án chờ duyệt là thêm field tùy chọn, validate ownership, khớp Product nguồn với `Items.ProductId`, lưu provenance trong `OrderDetails.Metadata`, không đổi diagram.
2. Hiện không reserve inventory và payment hoàn tất qua webhook SePay/Bảo Kim. Nếu tiền đã nhận nhưng Product fail revalidation, chưa chốt dùng `CANCELLED`, `REFUNDED` hay trạng thái mới; không được tự suy diễn.

---

## LIÊN KẾT DOCUMENT FIRST

### Stories (Draft)
- **STORY-035**: Tạo thiệp thiết kế
- **STORY-036**: Tạo lại thiệp từ lịch sử
- **STORY-037**: Tạo lại thiệp AI, giữ thiết kế gốc và thay nội dung

### System Tests (Approved)
- ST-035-01-01: Tạo thiệp thành công
- ST-036-01-01: Tạo lại thiệp thành công từ History
- ST-036-05-01: Chặn Tạo lại khi hết quota
