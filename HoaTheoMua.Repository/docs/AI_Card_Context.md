# AI Card Context - HTM_Flourist_AI

> Document First Project: hoa-theo-mua-ai-customize
> Date: 2026-08-27
> Last Updated: 2026-09-09

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
- Có ít nhất 01 Template và singleton Size Config đang ở trạng thái khả dụng từ Core DB

**Trigger:**
Khách hàng chọn "Tạo thiệp" tại Checkout.

#### Input Data - Thông tin bắt buộc
| Trường | Giới hạn |
|--------|----------|
| Người gửi (`senderName`) | tối đa 20 từ |
| Người nhận (`receiverName`) | tối đa 20 từ |
| Lời chúc (`messageContent`) | tối đa 100 từ |
| Template (`cardTemplateId`) | bắt buộc - chọn từ `card_templates` |
| Hình thức (`formType`) | Calligraphy hoặc Gõ máy |

#### Ảnh đính kèm
- Không bắt buộc
- Tối đa 01 ảnh
- Định dạng: PNG, JPG
- Dung lượng tối đa: 10 MB

#### Thông tin bổ sung cho AI Module
- **`cardSize`**: Size chung duy nhất từ chuỗi JSON của `card_config.sizes` (`name`, `width`, `height`, `base_price`, `max_words`)
- **`cardConfig`**: Price rule được chọn từ chuỗi JSON của `card_config.prices` (`min_words`, `max_words`, `extra_price`)
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
- History mới kế thừa cột `base_id` gốc của History Card được chọn; metadata lưu ID Card được chọn. Không thay đổi Order/Checkout trong flow này.

### Chọn thiệp HandMade có sẵn

- Admin quản lý chung `card_templates`; `template_type="ai"` dùng cho AI, `template_type="handmade"` dùng làm mẫu thiệp có sẵn. Field bắt buộc, không nullable và không được đổi sau khi tạo; dữ liệu cũ mặc định `ai`.
- Client gọi chung `POST /api/ai-cards`. Service load Template trước: `ai` chạy flow AI hiện tại; `handmade` chạy flow không AI.
- HandMade chỉ nhận nguồn hoa, `cardTemplateId`, sender, receiver và message. Nếu truyền `formType` hoặc `attachedImageUrl`, trả lỗi riêng `400`.
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
  key varchar                    -- Card Config chỉ dùng "card_config.sizes" hoặc "card_config.prices"
  value jsonb                   -- JSON chứa cấu hình chi tiết
  is_public boolean              -- Dùng để xác định config có đang hoạt động để người dùng chọn
  group varchar                  -- "card_config"
  kind varchar                   -- "Setting" cho card configs
  user_id guid
  created_at timestamp
  updated_at timestamp
  is_deleted boolean            -- Dùng cho xóa mềm
}
```

**Group và key dành cho Card:**
- `group="card_config"`, `key="card_config.sizes"`: một record chứa đúng một Size chung cho mọi Card.
- `group="card_config"`, `key="card_config.prices"`: một record chứa toàn bộ bậc giá Calligraphy.

Hai key trên là singleton: chỉ được có một record chưa xóa cho mỗi key. `group`, `key` và `kind` không được đổi sau khi tạo; Admin chỉ cập nhật payload `value`, trạng thái `is_public`, hoặc xóa mềm record. API tạo Card Config chủ động ghi `is_public=true`, còn default chung ở DB vẫn là `false`. Việc xóa `card_config.sizes` hoặc `card_config.prices` làm dependency tương ứng không khả dụng cho request mới; Card đã tạo vẫn dùng snapshot.

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
  sender_name varchar           -- Người gửi
  receiver_name varchar         -- Người nhận
  message_content text          -- Lời chúc
  attached_image_url varchar     -- Ảnh đính kèm; luôn null với HandMade

  -- Snapshot Size tại thời điểm tạo:
  size_name varchar              -- Nhãn của Size chung tại thời điểm tạo
  size_width decimal             -- 10 (cm)
  size_height decimal            -- 15 (cm)
  size_base_price decimal       -- Giá base tại thời điểm đó: 100000
  size_max_words int            -- Giới hạn từ của size đó: 100

  -- Snapshot Word count tại thời điểm tạo:
  word_count int               -- Số từ thực tế: 60
  word_config_snapshot json     -- Chỉ rule áp dụng: { "min_words": 36, "max_words": 70, "extra_price": 39000 }

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
  type varchar                   -- "flower", "card", "handmade_card"; post hiện chưa dùng
  system_form json [null]        -- Snapshot {type, content}; null khi type="handmade_card"
  metadata json                 -- Snapshot/provenance bất biến, schemaVersion=2
  created_at timestamp
  root uuid [not null]          -- Product ID gốc; do code gán, không có FK

  -- base_id: polymorphic reference - xác định nguồn gốc
  --   - Nếu type = "card" hoặc "handmade_card" và base là combo: product.id
  --   - Nếu type = "card" hoặc "handmade_card" và base là bó hoa AI: generated_flowers.id
  --   - Nếu type = "flower": reference đến product.id (combo)
  base_id uuid

  -- output_id: polymorphic reference đến kết quả
  --   - Nếu type = "card" hoặc "handmade_card": generated_cards.id
  --   - Nếu type = "flower": reference đến generated_flowers.id
  output_id uuid

}
```

`client_histories` không có column `input`. `system_form` thay cho `system_prompt_id`, không có FK tới `system_prompts`, và snapshot toàn bộ field của System Prompt ngoại trừ `id`. Với schema hiện tại, giá trị là `{ "type": "card", "content": "Full prompt đã dùng" }`; HandMade lưu `null`.

`root` không nằm trong public API. Với `type="flower"`, `root=base_id=products.id`. Với `type="card"` hoặc `type="handmade_card"`, `root` luôn là Product ID gốc dù `base_id` là Product hay Generated Flower. Service quản lý quy tắc này; database không có FK, navigation hoặc cascade cho `root`. Post chưa được sử dụng trong flow này và nằm ngoài contract `root` hiện tại.

### 3.3 Mối quan hệ giữa client_histories và Card

#### Trường hợp 1: Thiệp AI cho bó hoa bình thường
```
base_id = product.id (combo không có biến thể HOẶC biến thể là con của biến thể cha)
output_id = generated_cards.id
root = base_id
metadata = { ...card_template_info... }
```
**Ý nghĩa:** Thiệp AI được tạo ra cho bó hoa bình thường, dựa vào `cardTemplate` mà người dùng chọn.

#### Trường hợp 2: Thiệp AI cho bó hoa AI
```
base_id = generated_flowers.id
output_id = generated_cards.id
root = client_histories(type=flower, output_id=base_id).base_id
metadata = { ...card_template_info... }
```
**Ý nghĩa:** Thiệp AI được tạo cho bó hoa AI, dựa vào `cardTemplate` mà người dùng chọn.

#### Trường hợp 3: Thiệp HandMade

```text
type = handmade_card
base_id = product.id hoặc generated_flowers.id
output_id = generated_cards.id
root = product.id gốc theo cùng quy tắc Card AI
system_form = null
metadata = provenance + Template snapshot không gồm imageUrl; không có dữ liệu Size/Calligraphy/pricing/cardType
```

Lineage đều nằm trong cùng bảng `client_histories`, không có bảng Flower History/Card History riêng:

```text
GeneratedCard
  -> client_histories(type=card hoặc handmade_card, output_id=GeneratedCardId)
     root = ProductId gốc
     base_id = ProductId
     hoặc base_id = GeneratedFlowerId
        -> client_histories(type=flower, output_id=GeneratedFlowerId).base_id = ProductId
```

Với nguồn Generated Flower, service bắt buộc tìm đúng Flower History bằng `type="flower"` và `output_id=generatedFlowerId`, sau đó lấy `base_id` của Flower History làm `root`. Thiếu History, `base_id` thiếu hoặc bằng `Guid.Empty`, hoặc lineage không nhất quán thì trả `SOURCE_PRODUCT_PROVENANCE_INVALID/409`; không tạo Card/History và không gọi AI. Không dùng metadata để đoán Product ID thay thế.

### 3.4 Cấu trúc JSON

#### Size Config — `key="card_config.sizes"`
```json
{
  "group": "card_config",
  "label": "Kích thước thiệp",
  "value": "{\"items\":[{\"name\":\"Kích thước chung\",\"width\":15,\"height\":25,\"base_price\":15000,\"max_words\":150}]}",
  "options": [],
  "required": false,
  "inputType": "json",
  "validations": []
}
```

#### Calligraphy Words Config — `key="card_config.prices"`
```json
{
  "group": "card_config",
  "label": "Bậc giá calligraphy",
  "value": "{\"items\":[{\"min_words\":0,\"max_words\":35,\"extra_price\":0},{\"min_words\":36,\"max_words\":70,\"extra_price\":39000},{\"min_words\":71,\"max_words\":100,\"extra_price\":69000}]}",
  "options": [],
  "required": false,
  "inputType": "json",
  "validations": []
}
```

Đây là schema cố định của hai payload: không thêm hoặc bớt field. Cột `Config.value` vẫn là `jsonb`, nhưng field `value` trong wrapper bắt buộc là chuỗi JSON và vì vậy service phải parse hai lớp. Các field bên trong chuỗi giữ nguyên snake_case như mẫu. `card_config.sizes.items` bắt buộc có đúng một phần tử dùng chung cho mọi Card; `name` chỉ là nhãn được snapshot vào `generated_cards.size_name`, không phải key để client chọn. Request không nhận trường chọn Size và `generated_cards` không lưu định danh lựa chọn Size.

Khi tạo/cập nhật, service phải kiểm tra wrapper đúng schema, chuỗi `value` parse được và các item hợp lệ. Size yêu cầu `items` có đúng một phần tử với `name` không rỗng, `width > 0`, `height > 0`, `base_price >= 0`, `max_words > 0`. Price yêu cầu `items` không rỗng; `min_words >= 0`, `max_words >= min_words`, `extra_price >= 0`, sắp tăng dần, không chồng lấn và không có khoảng hở giữa các rule. Không bắt buộc `card_config.prices` phủ đến `max_words` của Size Config tại thời điểm Admin lưu; nếu request Card có `wordCount` không khớp rule nào thì trả lỗi nghiệp vụ và không tiếp tục.

#### Card Template Snapshot dùng trong runtime
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
  "originProductId": "uuid",
  "generatedFlowerId": "uuid-or-null",
  "provenanceStatus": "available",
  "cardTemplate": {
    "id": "uuid",
    "name": "Template A"
  },
  "sizeConfig": { "group": "card_config", "key": "card_config.sizes" },
  "validatedAt": "2026-08-28T08:00:00Z"
}
```

Snapshot runtime có ảnh để gọi AI. Khi persist, ảnh hiệu lực nằm ở `generated_cards.image_url`; `client_histories.metadata.cardTemplate` không lưu lại `templateImageUrl`.

`client_histories.system_form` của Card AI:

```json
{
  "type": "card",
  "content": "Full prompt đã dùng"
}
```

`metadata` không lặp `cardType`, `cardTemplate.imageUrl`, `sizeConfig.value`, `calligraphyRule`, `pricing`, System Prompt hoặc nội dung người dùng. Các dữ liệu đó lần lượt nằm ở `generated_cards` và `client_histories.system_form`.

Với Card được tạo lại, `metadata` bổ sung `operation="regenerate"`, `regeneratedFromCardId` và phần Effective Template không trùng với `generated_cards`. Cột `base_id` và `root` đều kế thừa History Card nguồn; `base_id` không đổi thành ID Card nguồn và `root` tiếp tục là Product ID gốc. Field `systemPromptSource` có giá trị `current` hoặc `source_card_fallback` để phân biệt prompt hiện hành với prompt snapshot được dùng dự phòng.

Với TDD-027, `metadata` bổ sung `operation="recreate_content"`, `recreatedFromCardId`, `referenceRawImage` và giữ phần định danh Template/Size không trùng. Giá trị Size, Calligraphy/pricing và nội dung hiệu lực nằm trong `generated_cards` mới. Cột `base_id` và `root` đều kế thừa History nguồn; `base_id` không đổi thành Card ID và `root` tiếp tục là Product ID gốc. Card nguồn thiếu `rawImage` trả `CARD_SOURCE_RAW_IMAGE_MISSING/409`.

#### `client_histories.metadata` khi `type=handmade_card`

```json
{
  "schemaVersion": 2,
  "sourceType": "product|generated_flower",
  "originProductId": "uuid",
  "generatedFlowerId": "uuid-or-null",
  "provenanceStatus": "available",
  "cardTemplate": {
    "id": "uuid",
    "name": "Mẫu thiệp viết tay",
    "templateType": "handmade",
    "description": "..."
  },
  "sizeConfig": { "group": "card_config", "key": "card_config.sizes" },
  "validatedAt": "2026-09-04T08:00:00Z"
}
```

HandMade có `system_form=null`; metadata không có `systemPrompt` và không có các field snapshot dành riêng cho lần gọi AI. `client_histories` không lưu input của Create, Regenerate, Recreate hoặc HandMade.

---

## 4. QUY TẮC NGHIỆP VỤ

### 4.0 Dependency matrix và validation boundary

| Flow | Product | Generated Flower | Template | Size Config | Calligraphy Config |
|---|---|---|---|---|---|
| Card từ Product | Validate live lúc admission | N/A | Validate live | Validate live | Validate live nếu Calligraphy |
| Card từ Generated Flower | Không revalidate trạng thái Product/Mockup gốc; phải resolve Product ID để gán root | Tồn tại + ownership + ảnh + Flower History hợp lệ | Validate live | Validate live | Validate live nếu Calligraphy |
| HandMade từ Product | Validate live lúc admission | N/A | Bắt buộc `templateType=handmade`, validate live | Validate live để snapshot, không thu base price | Validate live, luôn áp dụng |
| HandMade từ Generated Flower | Không revalidate Product/Mockup gốc | Tồn tại + ownership + provenance | Bắt buộc `templateType=handmade`, validate live | Validate live để snapshot, không thu base price | Validate live, luôn áp dụng |
| Regenerate | Không dùng trạng thái Product để chặn | Không revalidate nguồn hoa | Chỉ validate live khi client chọn ID Template khác nguồn; bỏ trống/null/đúng ID nguồn thì dùng snapshot nguồn, không validate live | Dùng snapshot Card nguồn, không validate live | Dùng snapshot Card nguồn, không validate live |
| Recreate content (TDD-027) | Không dùng trạng thái Product để chặn | Không revalidate nguồn hoa | Khóa snapshot Card nguồn; request không được gửi Template | Khóa snapshot Card nguồn; request không được gửi Size | Validate live nếu form type hiệu lực là Calligraphy; tính giá/snapshot mới |
| Order | Validate Product gốc live | Resolve lineage trong `client_histories` | Không quyết định tồn kho | Không quyết định tồn kho | Không quyết định tồn kho |

Thứ tự endpoint Card Create: `auth/request → load Template để phân nhánh`. Nếu `templateType=ai`: `source Product hoặc Generated Flower → xác định base_id và root → validate Template AI → Size Config → Calligraphy Config → system prompt → reserve atomically quota ngày + quota theo base_id → CardGenerationSnapshot → AI`. Nếu nguồn là Product thì `root=base_id`; nếu nguồn là Generated Flower thì service tìm Flower History và lấy `root=FlowerHistory.base_id`. Nếu không resolve được Product ID, trả lỗi trước quota/snapshot/AI. Nếu `templateType=handmade`, tiếp tục theo thứ tự HandMade bên dưới.

Thứ tự Regenerate: `auth/request → ownership → source Card + client_histories snapshot → chặn handmade_card → kiểm tra và kế thừa base_id/root → lấy base_id kế thừa làm quota source key → resolve Effective Template (chỉ validate live nếu ID khác nguồn) → system prompt hiện hành hoặc source fallback → reserve atomically quota ngày + quota theo base_id → RegenerationSnapshot → AI`.

Thứ tự Recreate content: `auth/request → source Card AI + History + ownership → rawImage nguồn → kiểm tra và kế thừa base_id/root → lấy cột base_id kế thừa làm quota source key → fallback/validate input hiệu lực → khóa Template/Size snapshot → Calligraphy/pricing live → system prompt current → reserve atomically quota ngày + quota theo base_id → CardRecreationSnapshot → AI trả raw+final → transaction Card/History`.

Thứ tự HandMade: `auth/request → source Product hoặc Generated Flower → xác định base_id/root → Template handmade → Size Config → Calligraphy Config/rule → snapshot → transaction generated_cards + client_histories`. Không resolve được Product ID gốc thì trả `SOURCE_PRODUCT_PROVENANCE_INVALID/409`. Không có prompt/quota/AI/retry.

- Validation nguồn/dependency chỉ một lần trước AI. Sau snapshot, Admin inactive/soft-delete dependency không hủy request, retry hoặc persist.
- Một lần đầu + tối đa hai retry cùng dùng snapshot bất biến; không query lại availability và không tải lại URL ảnh live.
- Admission fail thì không AI, không record/history và không consume quota.
- Mọi flow Card AI (Create, Regenerate, Recreate content) đều bắt buộc reserve trong một thao tác atomic hai slot: quota ngày theo user và quota theo `base_id` nguồn. `base_id` có thể là `products.id` hoặc `generated_flowers.id`; hai loại nguồn dùng chung quota và quy tắc. Success consume cả hai; AI/upload/persistence fail release cả hai.

#### Error taxonomy và thứ tự

- Product: `PRODUCT_NOT_FOUND/404` → `PRODUCT_DELETED/410` → `PRODUCT_INACTIVE/409` → `PRODUCT_OUT_OF_STOCK/409`; `PRODUCT_NOT_SELLABLE/422`; `PRODUCT_AVAILABILITY_UNAVAILABLE/503`.
- Generated Flower: không tồn tại hoặc khác owner → `GENERATED_FLOWER_NOT_FOUND/404`; thiếu ảnh cho AI → `GENERATED_FLOWER_INPUT_INVALID/409`; thiếu Flower History/Product ID gốc hoặc lineage không nhất quán → `SOURCE_PRODUCT_PROVENANCE_INVALID/409`.
- Template client chủ động chọn cho Create hoặc chọn ID khác nguồn khi Regenerate: `CARD_TEMPLATE_NOT_FOUND/404` → `CARD_TEMPLATE_DELETED/410` → `CARD_TEMPLATE_INACTIVE/409`. Regenerate bỏ trống/null hoặc truyền đúng ID Template nguồn thì dùng snapshot nguồn, không query hay kiểm tra trạng thái live.
- Size singleton `card_config.sizes`: `CARD_SIZE_CONFIG_NOT_FOUND/404` → `CARD_SIZE_CONFIG_DELETED/410` → `CARD_SIZE_CONFIG_INACTIVE/409` → `CARD_SIZE_CONFIG_INVALID/409`. `items` không có đúng một Size chung cũng là Config không hợp lệ.
- Calligraphy singleton `card_config.prices`: `CALLIGRAPHY_CONFIG_NOT_FOUND/404` → `CALLIGRAPHY_CONFIG_DELETED/410` → `CALLIGRAPHY_CONFIG_INACTIVE/409` → `CALLIGRAPHY_CONFIG_INVALID/409`; `wordCount` không thuộc rule nào → `CALLIGRAPHY_PRICE_RULE_NOT_FOUND/422`.
- Card Create không có prompt hiện hành type `card` hoặc content rỗng/không hợp lệ → `SYSTEM_PROMPT_INVALID/409`. Regenerate chỉ trả lỗi history nếu cả prompt hiện hành và prompt snapshot nguồn đều không dùng được.
- Hai Card Config là singleton; nếu dữ liệu legacy tạo nhiều record chưa xóa cùng key thì trả `CARD_CONFIG_AMBIGUOUS/409`, không chọn ngẫu nhiên. Regenerate không query Config live. Source Card thiếu snapshot, `base_id` hoặc `root` Product hợp lệ → `CARD_SOURCE_HISTORY_INVALID/409`.
- Regenerate source có history `type="handmade_card"` → `HANDMADE_CARD_REGENERATE_NOT_SUPPORTED/409`.
- TDD-027: source không phải Card AI → `HANDMADE_CARD_RECREATE_NOT_SUPPORTED/409`; thiếu `rawImage` usable → `CARD_SOURCE_RAW_IMAGE_MISSING/409`; request gửi Template/Size/source flower → `RECREATE_LOCKED_FIELD_NOT_ALLOWED/400`. Prompt hiện hành type `card` thiếu/rỗng/invalid → `SYSTEM_PROMPT_INVALID/409`.

### 4.1 Logic xác định `base_id` và `root` cho thiệp

| Nguồn Card | `base_id` | `root` |
|---|---|---|
| Product/biến thể | Product ID nhận từ request | Bằng `base_id` |
| Generated Flower | `generated_flowers.id` nhận từ request | Tìm `client_histories(type="flower", output_id=base_id)` và lấy `base_id` của Flower History |
| Regenerate/Recreate Card AI | Kế thừa `base_id` của Card History nguồn | Kế thừa `root` của Card History nguồn |

`root` bắt buộc khác `Guid.Empty`. Không tìm được đúng một Flower History hợp lệ hoặc Product ID gốc thì trả lỗi, không dùng metadata để đoán và không persist một phần.

- Khi source là Product có biến thể, request bắt buộc dùng đúng `productId` biến thể; không dùng ID cha. Card Create kiểm tra trạng thái và lượng bán của đúng ID nhận được. Việc kiểm tra cha active là rule Order, không chặn Card Generate.
- Khi source là Generated Flower, kiểm tra record tồn tại/ownership/ảnh và Flower History để lấy Product ID gốc; không revalidate trạng thái live của Product hoặc Mockup gốc.

### 4.2 Regenerate snapshot policy

- Mọi Card AI có history `type="card"` thuộc user đều có thể là Card nguồn. Card mới luôn có ID, ảnh kết quả và `createdAt` mới, lấy `userId` từ phiên đăng nhập; không ghi đè Card nguồn. History `handmade_card` bị chặn.
- Effective Template là `cardTemplateId` client chọn, hoặc Template snapshot của Card nguồn khi request để trống. Người dùng được chọn lại Template AI cũ hoặc Template AI khác; không được chuyển sang HandMade. Mọi Template AI tương thích với Size chung và mọi form type.
- Nếu client bỏ trống/null hoặc truyền đúng ID Template nguồn, dùng nguyên Template snapshot nguồn, không query live và không kiểm tra active/soft-delete. Chỉ khi client chọn ID Template khác nguồn, Template mới phải tồn tại, chưa soft-delete, active và có `templateType="ai"` tại admission; Template HandMade trả `CARD_TEMPLATE_TYPE_INVALID/409`. Trạng thái Template cũ không chặn request. Dependency mới đổi trạng thái sau snapshot không ảnh hưởng request đang chạy.
- Dùng nguyên sender/receiver/message/attached image/form type/size, giá và Config snapshot của Card nguồn; không query hoặc validate Size/Calligraphy Config live và không lấy giá/rule mới.
- Mỗi `system_prompts.type` (`flower`, `card`, `post`) có đúng một prompt hiện hành. Regenerate ưu tiên prompt hiện hành của type `card`; record không tồn tại hoặc có content rỗng/không hợp lệ đều được xem là không khả dụng. Khi đó dùng `system_form` của History Card nguồn, không kiểm tra trạng thái record prompt nguồn, và ghi `systemPromptSource="source_card_fallback"`.
- Cột `base_id` và `root` của History mới lần lượt bằng `base_id` và `root` của History Card nguồn; cột `output_id` bằng ID Card mới. `metadata.regeneratedFromCardId` giữ quan hệ Card cha-con. Thiếu `root` Product hợp lệ trả `CARD_SOURCE_HISTORY_INVALID/409`.
- Product gốc inactive/deleted/out-of-stock không chặn Regenerate; chỉ Order kiểm tra Product live.

### 4.3 HandMade Card policy

- `cardTemplateId` phải trỏ tới Template có cột `template_type="handmade"`, tồn tại, chưa xóa và active. Template snapshot lưu đầy đủ nhưng `generated_cards.image_url` chỉ lưu trực tiếp URL Template hiện hành.
- Request bắt buộc source, `cardTemplateId`, sender, receiver, message. Truyền `formType` → `HANDMADE_CARD_FORM_TYPE_NOT_ALLOWED/400`; truyền `attachedImageUrl` → `HANDMADE_CARD_ATTACHMENT_NOT_ALLOWED/400`.
- Server gán `formType="calligraphy"`; áp dụng cùng quy tắc field, đếm từ, `sizeMaxWords` và matching Calligraphy rule như Card AI.
- `sizeBasePrice` snapshot đúng giá Config; `basePrice=0`; `extraPrice` bằng phụ phí rule số từ; `totalPrice=extraPrice`.
- Với HandMade, không gọi API thì không có record; request thành công tạo Card/history trong một transaction.
- History dùng `type="handmade_card"`, cột `system_form=null`, provenance đầy đủ, không có `systemPrompt`; cột `base_id` là Product hoặc Generated Flower trực tiếp, `root` là Product ID gốc theo mục 4.1, và `output_id` là HandMade Generated Card mới.
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
// Mỗi key là singleton. Phải phân biệt not-found, soft-deleted và inactive trước khi parse.
var sizesConfig = await GetCardConfigAsync("card_config.sizes");
var sizeItems = ParseInnerItems<SizeItem>(sizesConfig.Value); // parse wrapper rồi parse chuỗi value
var selectedSize = sizeItems.Single(); // validator bảo đảm đúng một Size chung

var pricesConfig = await GetCardConfigAsync("card_config.prices");
var priceItems = ParseInnerItems<PriceItem>(pricesConfig.Value);
```

### 6.2 Tính giá

```csharp
public async Task<decimal> CalculateExtraPriceAsync(string formType, int wordCount)
{
    if (formType != "calligraphy")
        return 0;

    var pricesConfig = await GetCardConfigAsync("card_config.prices");
    var rules = ParseInnerItems<WordRule>(pricesConfig.Value);

    var rule = rules.FirstOrDefault(r => wordCount >= r.MinWords && wordCount <= r.MaxWords);
    if (rule is null)
        throw CalligraphyPriceRuleNotFound(); // CALLIGRAPHY_PRICE_RULE_NOT_FOUND/422

    return rule.ExtraPrice;
}

// AI Card: basePrice = sizeBasePrice; totalPrice = basePrice + extraPrice
// HandMade: basePrice = 0; totalPrice = extraPrice
```

### 6.3 Ví dụ kết quả Card AI

**Input:**
- `formType`: `"calligraphy"`
- `messageContent`: `"Chúc bạn sinh nhật vui vẻ, hạnh phúc và thành công trong cuộc sống"`

**Xử lý:**
1. Lấy Size chung duy nhất từ `Group="card_config"`, `Key="card_config.sizes"`: `{ "name": "Kích thước chung", "width": 15, "height": 25, "base_price": 15000, "max_words": 150 }`
2. Word count: 12
3. Chọn rule từ `Key="card_config.prices"`: `{ "min_words": 0, "max_words": 35, "extra_price": 0 }`

**Generated Card:**
```json
{
  "sizeName": "Kích thước chung",
  "sizeWidth": 15,
  "sizeHeight": 25,
  "sizeBasePrice": 15000,
  "sizeMaxWords": 150,
  "wordCount": 12,
  "wordConfigSnapshot": { "min_words": 0, "max_words": 35, "extra_price": 0 },
  "basePrice": 15000,
  "extraPrice": 0,
  "totalPrice": 15000
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
- **Body**: source + `cardTemplateId` + sender/receiver/message; không nhận `formType` hoặc ảnh đính kèm

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
- History HandMade vẫn có `base_id`, `root`, `output_id` và metadata provenance như quy định, nhưng `system_form=null`. `root` chỉ dùng nội bộ và không được thêm vào response công khai.

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
- **Discriminator dạng string** - `template_type/card_type` dùng `ai|handmade`; History trong phạm vi hiện tại dùng `flower|card|handmade_card`; Post chưa sử dụng và nằm ngoài quy tắc `root`.
- **Snapshot giá** - tất cả thông tin ảnh hưởng đến giá được lưu tại thời điểm tạo thiệp
- **AI Module tách riêng** - các hàm xử lý logic gọi đến AI Module để thực hiện việc gọi AI
- **Kết quả AI Card** - Card AI mới trả cặp field ảnh: `rawImage` (trước chèn chữ) và `imageUrl` (sau chèn chữ). Thiếu/upload lỗi một ảnh được xem là cả attempt thất bại, retry/release quota theo cùng policy.
- **Input cho Card AI**: `senderName`, `receiverName`, `messageContent`, `formType`, `cardTemplateId`, `attachedImageUrl`, `cardSize` chung (từ Config), `cardConfig` (từ Config), `flowerInfo` (`imageUrl` của Product hoặc Generated Flower)
- **Input cho HandMade Card**: đúng một source, `cardTemplateId`, `senderName`, `receiverName`, `messageContent`; server gán Calligraphy, lấy Size chung từ Config và không nhận `attachedImageUrl`
- **Input cho Regenerate**: `sourceCardId` ở route và `cardTemplateId` tùy chọn trong body; các dữ liệu còn lại lấy từ Card/history nguồn.
- **Input cho Recreate content (TDD-027)**: `sourceCardId` ở route; chỉ sửa sender/receiver/message/form type/ảnh đính kèm. Dùng raw image Card nguồn làm reference, khóa Template/Size snapshot và tính lại Calligraphy/pricing.

### Order context và quyết định còn mở

Order lấy Product gốc từ `client_histories.root`, sau đó đối chiếu với lineage `base_id`/Flower History, kiểm tra Product live và `sellableQuantity >= tổng quantity` rồi mới tiếp tục kiểm kho depot Nhanh.vn. `metadata.originProductId` chỉ là dữ liệu audit, không phải nguồn quyết định. Nếu `root` thiếu, bằng `Guid.Empty` hoặc không nhất quán với lineage thì chặn `SOURCE_PRODUCT_PROVENANCE_INVALID/409`.

Luồng context-only bắt buộc cho implementation Order tương lai:

1. Direct Product: dùng đúng Product/variant trong item.
2. Card từ Product: tìm `client_histories(type=card|handmade_card, output_id=GeneratedCardId)`; `root` phải bằng `base_id=ProductId`.
3. Card từ Generated Flower: History Card có `base_id=GeneratedFlowerId`; tìm `client_histories(type=flower, output_id=GeneratedFlowerId)` và yêu cầu `HistoryCard.root=HistoryFlower.base_id=HistoryFlower.root=ProductId`.
4. `root` thiếu, bằng `Guid.Empty`, không resolve hoặc không nhất quán: `SOURCE_PRODUCT_PROVENANCE_INVALID/409`; không tạo Order/OrderDetail và không chuẩn bị payment. Không fallback sang metadata để đoán Product ID.
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
