# AI Card Context - HTM_Flourist_AI

> Document First Project: hoa-theo-mua-ai-customize
> Date: 2026-08-27
> Last Updated: 2026-08-28

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
Hệ thống thiệp AI cho phép khách hàng tạo thiệp cá nhân hóa bằng AI trong quá trình Checkout.

### Luồng chính
```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         CARD FLOW                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│  Checkout (từ Flower Flow)                                                 │
│      │                                                                      │
│      ▼                                                                      │
│  [STORY-035] "Tạo thiệp AI" / "Tạo lại" (trong Checkout)                 │
│      │                                                                      │
│      ▼                                                                      │
│  Thiệp AI + client_histories                                               │
│      │                                                                      │
│      ▼                                                                      │
│  Xác nhận thiệp → Gắn vào Checkout                                        │
│      │                                                                      │
│      ▼                                                                      │
│  Order với thiệp đã chọn                                                   │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Stories liên quan
| Story | Tên | Sprint | Priority | Status |
|-------|-----|--------|----------|--------|
| **STORY-035** | Tạo thiệp thiết kế | 1 | Must | Draft |
| **STORY-036** | Tạo lại thiệp từ lịch sử | 1 | Must | Draft |

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
| Người gửi (sender_name) | tối đa 20 từ |
| Người nhận (receiver_name) | tối đa 20 từ |
| Lời chúc (message_content) | tối đa 100 từ |
| Template (card_template_id) | bắt buộc - chọn từ card_templates |
| Size (size_key) | bắt buộc - chọn từ Config group="card_size" |
| Hình thức (form_type) | Calligraphy hoặc Gõ máy |

#### Ảnh đính kèm
- Không bắt buộc
- Tối đa 01 ảnh
- Định dạng: PNG, JPG
- Dung lượng tối đa: 10 MB

#### Thông tin bổ sung cho AI Module
- **card_size**: Thông tin size được chọn từ Config (name, width, height, base_price, max_words)
- **card_config**: Thông tin calligraphy config từ Config (min_words, max_words, extra_price)
- **flower_info**: Thông tin bó hoa mà thiệp dựa vào:
  - Nếu dựa vào bó hoa bình thường: image_url của product
  - Nếu dựa vào bó hoa AI: image_url của generated_flowers

### STORY-036: Tạo lại thiệp từ lịch sử

#### Metadata
- **Story**: Là một khách hàng đã đăng nhập, tôi muốn tạo lại từ một thiệp đã có, để có một mẫu thiệp cá nhân hóa phù hợp với nhu cầu của mình.
- **Context**: Khách hàng có thể chọn "Tạo lại" tại màn Chi tiết thiệp đã tạo.
- **Sprint**: 1
- **Priority**: Must

#### Đặc điểm
- Sử dụng nguyên dữ liệu đầu vào của lần generate hiện tại
- Không mở form chỉnh sửa
- Kết quả mới không tự động gắn vào Checkout

---

## 3. DATABASE SCHEMA

### 3.1 Enums

```sql
Enum form_type {
  go_may       -- Gõ máy
  calligraphy  -- Calligraphy
}

Enum generation_type {
  flower
  card
  post
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
  type system_prompt_types      -- flower, card, post
  content text
}
```

#### card_templates

Template dùng để tạo card.

```sql
Table card_templates {
  id uuid [pk]
  name varchar
  description text
  character_limit int
  image_url varchar
  metadata json                 -- Thông tin template cho AI
  is_active boolean
  created_at timestamp
  updated_at timestamp
  is_deleted boolean
}
```

#### generated_cards

Card được tạo ra từ AI.

```sql
Table generated_cards {
  id uuid [pk]
  content text                  -- Nội dung thiệp
  image_url varchar             -- URL ảnh thiệp
  user_id uuid
  created_at timestamp

  -- Thông tin thiệp:
  form_type form_type           -- "go_may" hoặc "calligraphy"
  size_key varchar              -- Key của size: "size_A", "size_B"
  sender_name varchar           -- Người gửi
  receiver_name varchar         -- Người nhận
  message_content text          -- Lời chúc
  attached_image_url varchar     -- Ảnh đính kèm (nullable)

  -- Snapshot Size tại thời điểm tạo:
  size_name varchar              -- "A", "B", "C"
  size_width decimal             -- 10 (cm)
  size_height decimal            -- 15 (cm)
  size_base_price decimal       -- Giá base tại thời điểm đó: 100000
  size_max_words int            -- Giới hạn từ của size đó: 100

  -- Snapshot Word count tại thời điểm tạo:
  word_count int               -- Số từ thực tế: 60
  word_config_snapshot json     -- JSON lưu rule áp dụng: { "min_words": 36, "max_words": 70, "extra_price": 39000 }

  -- Trạng thái:
  is_confirmed boolean         -- Đã xác nhận gắn vào Checkout chưa (default: false)

  -- Giá đã tính toán:
  base_price decimal            -- = size_base_price
  extra_price decimal           -- Phụ phí Calligraphy = word_config_snapshot.extra_price
  total_price decimal           -- = base_price + extra_price
}
```

#### client_histories

Lịch sử thao tác của người dùng (dùng chung cho flower và card).

```sql
Table client_histories {
  id uuid [pk]
  user_id uuid                  -- Ai tạo history
  input text                    -- JSON string của request gốc
  type generation_type          -- "flower", "card", "post"
  system_prompt_id uuid
  metadata json                 -- Snapshot/provenance bất biến, schema_version=2
  created_at timestamp

  -- base_id: polymorphic reference - xác định nguồn gốc
  --   - Nếu type = "card" và base là combo bình thường: reference đến product.id
  --   - Nếu type = "card" và base là bó hoa AI: reference đến generated_flowers.id
  --   - Nếu type = "flower": reference đến product.id (combo)
  base_id uuid

  -- output_id: polymorphic reference đến kết quả AI
  --   - Nếu type = "card": reference đến generated_cards.id
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
**Ý nghĩa:** Thiệp AI được tạo ra cho bó hoa bình thường, dựa vào card_template mà người dùng chọn.

#### Trường hợp 2: Thiệp AI cho bó hoa AI
```
base_id = generated_flowers.id
output_id = generated_cards.id
metadata = { ...card_template_info... }
```
**Ý nghĩa:** Thiệp AI được tạo cho bó hoa AI, dựa vào card_template mà người dùng chọn.

Lineage đều nằm trong cùng bảng `client_histories`, không có bảng Flower History/Card History riêng:

```text
GeneratedCard
  -> client_histories(type=card, output_id=GeneratedCardId)
     base_id = ProductId
     hoặc base_id = GeneratedFlowerId
        -> client_histories(type=flower, output_id=GeneratedFlowerId).base_id = ProductId
```

Với dữ liệu cũ có Generated Flower đúng ownership và đủ ảnh nhưng không có record `client_histories(type=flower)` hợp lệ: vẫn cho tạo Card; metadata Card ghi `origin_product_id=null`, `provenance_status="unavailable"`. Regenerate vẫn được nếu snapshot Card đủ; Order bị chặn `SOURCE_PRODUCT_PROVENANCE_INVALID/409`.

### 3.4 Cấu trúc JSON

#### Size Config
```json
{
  "name": "A",
  "width": 10,
  "height": 15,
  "base_price": 100000,
  "max_words": 100
}
```

#### Calligraphy Words Config
```json
[
  { "min_words": 0, "max_words": 35, "extra_price": 0 },
  { "min_words": 36, "max_words": 70, "extra_price": 39000 },
  { "min_words": 71, "max_words": 100, "extra_price": 69000 }
]
```

#### Card Template Metadata
```json
{
  "template_id": "uuid",
  "template_name": "Tên template",
  "template_image_url": "url"
}
```

#### `client_histories.metadata` khi `type=card` — schema_version 2

```json
{
  "schema_version": 2,
  "source_type": "product|generated_flower",
  "origin_product_id": "uuid-or-null",
  "generated_flower_id": "uuid-or-null",
  "provenance_status": "available|unavailable",
  "card_template": {
    "id": "uuid",
    "name": "Template A",
    "metadata": {},
    "image": { "url": "https://...", "object_key": "templates/a.png", "version_id": "v3", "sha256": "..." }
  },
  "size_config": { "group": "card_size", "key": "size_A", "value": {} },
  "calligraphy_config": { "group": "card_config", "key": "calligraphy_words", "value": [] },
  "system_prompt": { "id": "uuid", "content": "Full prompt đã dùng", "sha256": "..." },
  "validated_at": "2026-08-28T08:00:00Z"
}
```

---

## 4. QUY TẮC NGHIỆP VỤ

### 4.0 Dependency matrix và validation boundary

| Flow | Product | Generated Flower | Template | Size Config | Calligraphy Config |
|---|---|---|---|---|---|
| Card từ Product | Validate live lúc admission | N/A | Validate live | Validate live | Validate live nếu Calligraphy |
| Card từ Generated Flower | Không revalidate Product/Mockup gốc | Tồn tại + ownership; ảnh đủ dùng | Validate live | Validate live | Validate live nếu Calligraphy |
| Regenerate | Không dùng trạng thái Product để chặn | Không revalidate nguồn hoa | Validate live | Validate live | Validate live nếu Calligraphy |
| Order | Validate Product gốc live | Resolve lineage trong `client_histories` | Không quyết định tồn kho | Không quyết định tồn kho | Không quyết định tồn kho |

Thứ tự Card Create: `auth/request → source Product hoặc Generated Flower → Template → Size Config → Calligraphy Config → system prompt → reserve quota → CardGenerationSnapshot → AI`.

Thứ tự Regenerate: `auth/ownership → source Card + client_histories snapshot → Template → Size Config → Calligraphy Config → system prompt → reserve quota → RegenerationSnapshot → AI`.

- Validation nguồn/dependency chỉ một lần trước AI. Sau snapshot, Admin inactive/soft-delete dependency không hủy request, retry hoặc persist.
- Một lần đầu + tối đa hai retry cùng dùng snapshot bất biến; không query lại availability và không tải lại URL ảnh live.
- Admission fail thì không AI, không record/history và không consume quota.
- Mọi quota slot bắt buộc của flow được reserve trong một thao tác atomic sau dependency validation; success consume, AI/persistence fail release toàn bộ.

#### Error taxonomy và thứ tự

- Product: `PRODUCT_NOT_FOUND/404` → `PRODUCT_DELETED/410` → `PRODUCT_INACTIVE/409` → `PRODUCT_OUT_OF_STOCK/409`; `PRODUCT_NOT_SELLABLE/422`; `PRODUCT_AVAILABILITY_UNAVAILABLE/503`.
- Generated Flower: không tồn tại hoặc khác owner → `GENERATED_FLOWER_NOT_FOUND/404`; thiếu ảnh cho AI → `GENERATED_FLOWER_INPUT_INVALID/409`. Thiếu provenance nhưng có ảnh không chặn Card.
- Template: `CARD_TEMPLATE_NOT_FOUND/404` → `CARD_TEMPLATE_DELETED/410` → `CARD_TEMPLATE_INACTIVE/409`.
- Size: `CARD_SIZE_CONFIG_NOT_FOUND/404` → `CARD_SIZE_CONFIG_DELETED/410` → `CARD_SIZE_CONFIG_INACTIVE/409`.
- Calligraphy: `CALLIGRAPHY_CONFIG_NOT_FOUND/404` → `CALLIGRAPHY_CONFIG_DELETED/410` → `CALLIGRAPHY_CONFIG_INACTIVE/409`.
- Nhiều Config active cùng `(group,key)` → `CARD_CONFIG_AMBIGUOUS/409`; source Card thiếu snapshot regenerate → `CARD_SOURCE_HISTORY_INVALID/409`.

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

- Khi source là Product có biến thể, request bắt buộc dùng đúng `product_id` biến thể; không dùng ID cha. Card Create kiểm tra trạng thái và lượng bán của đúng ID nhận được. Việc kiểm tra cha active là rule Order, không chặn Card Generate.
- Khi source là Generated Flower, chỉ kiểm tra record tồn tại/ownership/ảnh; không revalidate Product hoặc Mockup gốc.

### 4.2 Regenerate snapshot policy

- Trước admission phải xác minh Template, Size Config và Calligraphy Config hiện tại còn usable. Dependency đổi trạng thái sau snapshot không ảnh hưởng request đang chạy.
- Sau khi được phép regenerate, dùng nguyên nội dung, giá và config snapshot của Card nguồn; không lấy giá/rule mới, kể cả Config hiện hành đã đổi value nhưng vẫn active.
- Product gốc inactive/deleted/out-of-stock không chặn Regenerate; chỉ Order kiểm tra Product live.

### 4.3 Quy tắc đếm từ

- Một từ là một chuỗi ký tự liên tục được phân tách bởi khoảng trắng, tab hoặc ký tự xuống dòng
- Khoảng trắng ở đầu và cuối nội dung không được tính
- Nhiều khoảng trắng, tab hoặc ký tự xuống dòng liên tiếp được tính là một dấu phân tách
- Dấu câu đi liền với một từ không được tính thành từ riêng
- Nội dung chỉ gồm khoảng trắng được tính là 0 từ

### 4.4 Render nội dung thiệp

| Hình thức | Render Người gửi, Người nhận, Lời chúc | Ghi chú |
|-----------|-----------------------------------------|---------|
| Gõ máy | Hiển thị nguyên văn | AI giữ nguyên nội dung |
| Calligraphy | Không hiển thị trên ảnh | Lưu trong history, chỉ render khi in |

### 4.5 Ảnh đính kèm

- AI giữ nguyên nội dung gốc của ảnh
- Không cắt mất chủ thể, xoay, đổi màu, thêm/xóa hoặc làm biến dạng
- Được phép scale đồng dạng hoặc thêm khoảng đệm để phù hợp Template
- Ảnh kết quả lưu dưới định dạng PNG, không gắn logo

---

## 5. QUOTA RULES

### Giới hạn
| Loại | Giới hạn | Đơn vị |
|------|-----------|---------|
| Tạo thiệp | 10 lượt | /ngày/khách hàng |
| Tạo thiệp theo mẫu hoa | 3 lượt | /mẫu hoa nguồn |

### Quy trình Quota

1. **Reserve atomically mọi slot bắt buộc** → sau dependency validation, trước snapshot/AI
2. **Consume các slot** → khi AI và persistence thành công
3. **Release toàn bộ reservation** → khi AI thất bại sau retry hoặc persistence rollback

### Retry Logic

- **1 lần gọi đầu + tối đa 2 retry** (tổng tối đa 3 lần) với cùng snapshot/quota slot
- Nếu vẫn thất bại sau retry → **release đúng quota slot đã reserve**
- Mỗi request chỉ consume hoặc release slot đúng một lần

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
// sizeConfig.Value: { "name": "A", "width": 10, "height": 15, "base_price": 100000, "max_words": 100 }

// Lấy cấu hình calligraphy words
var wordsConfigs = await _dbContext.Config
    .Where(c => c.Group == "card_config" && c.Key == "calligraphy_words" && c.Kind == "Setting" && !c.IsDeleted && c.IsPublic)
    .ToListAsync();
if (wordsConfigs.Count > 1) throw CardConfigAmbiguous();
var wordsConfig = wordsConfigs.SingleOrDefault() ?? throw CalligraphyConfigNotFound();
// wordsConfig.Value: [ { "min_words": 0, "max_words": 35, "extra_price": 0 }, ... ]
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

// Total Price = base_price + extra_price
```

### 6.3 Ví dụ kết quả

**Input:**
- size: "A"
- form_type: "calligraphy"
- message_content: "Chúc bạn sinh nhật vui vẻ, hạnh phúc và thành công trong cuộc sống"

**Xử lý:**
1. Size config (Group="card_size", Key="size_A"): `{ "name": "A", "width": 10, "height": 15, "base_price": 100000, "max_words": 100 }`
2. Word count: 12
3. Word rule: `{ "min_words": 0, "max_words": 35, "extra_price": 0 }`

**Generated Card:**
```json
{
  "size_key": "size_A",
  "size_name": "A",
  "size_width": 10,
  "size_height": 15,
  "size_base_price": 100000,
  "size_max_words": 100,
  "word_count": 12,
  "word_config_snapshot": { "min_words": 0, "max_words": 35, "extra_price": 0 },
  "base_price": 100000,
  "extra_price": 0,
  "total_price": 100000
}
```

---

## 7. API ENDPOINTS

### TDD-006: Tạo thiệp mới
- **Method**: POST
- **Route**: `/api/ai-cards`

### TDD-007: Tạo lại thiệp từ History
- **Method**: POST
- **Route**: `/api/ai-cards/{id}/regenerate`

### Card Config CRUD (Sử dụng bảng Config có sẵn)
| TDD | Chức năng | Method | Route | Quyền |
|-----|-----------|--------|-------|--------|
| TDD-009 | Tạo card config | POST | `/api/v1/configs` | Admin |
| TDD-008 | Lấy danh sách | GET | `/api/v1/configs/content` | Admin, Staff |
| TDD-010 | Cập nhật card config | PUT | `/api/v1/configs/{id}` | Admin |
| TDD-011 | Xóa card config | DELETE | `/api/v1/configs/{id}` | Admin |

---

## Ghi chú quan trọng

- **`source_flower_id` đã được bỏ khỏi `generated_cards`** - mối quan hệ được xác định qua `client_histories`
- **thiệp không gắn logo** - ảnh kết quả được lưu dưới định dạng PNG mà không có logo
- **Snapshot giá** - tất cả thông tin ảnh hưởng đến giá được lưu tại thời điểm tạo thiệp
- **AI Module tách riêng** - các hàm xử lý logic gọi đến AI Module để thực hiện việc gọi AI
- **Input cho Card**: sender_name, receiver_name, message_content, form_type, size_key, card_template_id, attached_image_url, card_size (từ Config), card_config (từ Config), flower_info (image_url của product hoặc generated_flowers) |

### Order context và quyết định còn mở

Order phải resolve `origin_product_id` qua chuỗi `client_histories`, kiểm tra Product live và `sellableQuantity >= tổng quantity` rồi mới tiếp tục kiểm kho depot Nhanh.vn. Nếu không resolve được lineage, chặn `SOURCE_PRODUCT_PROVENANCE_INVALID/409`.

Luồng context-only bắt buộc cho implementation Order tương lai:

1. Direct Product: dùng đúng Product/variant trong item.
2. Card từ Product: tìm `client_histories(type=card, output_id=GeneratedCardId).base_id`.
3. Card từ Generated Flower: từ record `client_histories(type=card)` lấy `base_id=GeneratedFlowerId`, rồi tìm record `client_histories(type=flower, output_id=GeneratedFlowerId).base_id=ProductId`. Có thể dùng `origin_product_id` để resolve nhanh nhưng vẫn phải kiểm tra consistency.
4. Không resolve/không nhất quán: `SOURCE_PRODUCT_PROVENANCE_INVALID/409`; không tạo Order/OrderDetail và không chuẩn bị payment.
5. Product live giữ thứ tự not-found → deleted → inactive → quantity. Cộng gộp các dòng cùng Product và yêu cầu `sellableQuantity >= tổng Number`; thiếu lượng trả `PRODUCT_OUT_OF_STOCK/409`.
6. Nếu ID là variant con: kiểm tra child active, sau đó parent; parent inactive chặn Order bằng `PRODUCT_INACTIVE/409`. Sau local `CalculateAvailableQty`, vẫn chạy kiểm kho depot Nhanh.vn; không xác minh được external inventory trả `PRODUCT_AVAILABILITY_UNAVAILABLE/503`.
7. Vì hiện chưa reserve inventory, payment webhook dự kiến phải revalidate Product trước khi chuyển PAID/fulfillment; xử lý tiền đã nhận nhưng fail vẫn là open decision dưới đây.

Hai quyết định chưa được chốt và AI đọc Context ở phiên sau phải hỏi lại kèm bối cảnh:

1. `CreateOrderRequest` hiện không có `generated_card_id`. Chưa chốt cách Order nhận biết Card AI. Phương án chờ duyệt là thêm field tùy chọn, validate ownership, khớp Product nguồn với `Items.ProductId`, lưu provenance trong `OrderDetails.Metadata`, không đổi diagram.
2. Hiện không reserve inventory và payment hoàn tất qua webhook SePay/Bảo Kim. Nếu tiền đã nhận nhưng Product fail revalidation, chưa chốt dùng `CANCELLED`, `REFUNDED` hay trạng thái mới; không được tự suy diễn.

---

## LIÊN KẾT DOCUMENT FIRST

### Stories (Draft)
- **STORY-035**: Tạo thiệp thiết kế
- **STORY-036**: Tạo lại thiệp từ lịch sử

### System Tests (Approved)
- ST-035-01-01: Tạo thiệp thành công
- ST-036-01-01: Tạo lại thiệp thành công từ History
- ST-036-05-01: Chặn Tạo lại khi hết quota
