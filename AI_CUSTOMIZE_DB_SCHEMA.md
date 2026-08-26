# AI Customize Database Schema

> Document First Project: HTM_Flourist_AI (hoa-theo-mua-ai-customize)
> Date: 2026-08-25
> Last Updated: 2026-08-26

## Liên kết Document First

- **STORY-035**: Tạo thiệp thiết kế (chưa phê duyệt)
- **STORY-036**: Tạo lại thiệp từ lịch sử (chưa phê duyệt)

---

## TDDs

### AI Card (STORY-035, STORY-036)
- **TDD-035**: Tạo thiệp thiết kế AI (STORY-035) - `docs/tdd/TDD-035_tao-thep-thiet-ke-ai.md`
- **TDD-036**: Tạo lại thiệp từ lịch sử (STORY-036) - `docs/tdd/TDD-036_tao-lai-thep-tu-lich-su.md`

### Card Config CRUD (Sử dụng bảng Config có sẵn)
- **TDD-054**: Lấy danh sách card configs - `docs/tdd/TDD-054_lay-danh-sach-card-configs.md`
- **TDD-055**: Tạo card config - `docs/tdd/TDD-055_tao-card-config.md`
- **TDD-056**: Xóa card config - `docs/tdd/TDD-056_xoa-card-config.md`
- **TDD-062**: Cập nhật card config - `docs/tdd/TDD-062_cap-nhat-card-config.md`

---

## API Endpoints

### TDD-035: Tạo thiệp mới
- **Method**: POST
- **Route**: `/api/ai-cards`

### TDD-036: Tạo lại thiệp từ History
- **Method**: POST
- **Route**: `/api/ai-cards/{id}/regenerate`

### Card Config CRUD (Sử dụng bảng Config có sẵn)
| TDD | Chức năng | Method | Route | Quyền |
|-----|-----------|--------|-------|--------|
| TDD-055 | Tạo card config | POST | `/api/v1/configs` | Admin |
| TDD-054 | Lấy danh sách | GET | `/api/v1/configs/content` | Admin, Staff |
| TDD-062 | Cập nhật card config | PUT | `/api/v1/configs/{id}` | Admin |
| TDD-056 | Xóa card config | DELETE | `/api/v1/configs/{id}` | Admin |

---

## Enums

```sql
Enum platform_type {
  zalo
  facebook
  instagram
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

Enum form_type {
  go_may      -- Gõ máy
  calligraphy -- Calligraphy
}

Enum kind_config {
  Content
  Setting
  Secret
}
```

---

## Tables

### Config (Bảng có sẵn - sử dụng cho card configs)
Cấu hình card (size, calligraphy) sử dụng bảng Config có sẵn.

**Quy tắc sử dụng field:**
- `is_public`: true = đang hoạt động (người dùng có thể chọn), false = không hoạt động
- `is_deleted`: true = đã xóa mềm, false = chưa xóa

```sql
Table Config {
  id guid [pk]
  key varchar                    -- Key định danh: "size_A", "size_B", "calligraphy_words"
  value jsonb                   -- JSON chứa cấu hình chi tiết
  is_public boolean             -- Dùng để xác định config có đang hoạt động để người dùng chọn
  group varchar                 -- "card_size" hoặc "card_config"
  kind varchar                  -- "Setting" cho card configs
  user_id guid
  created_at timestamp
  updated_at timestamp
  is_deleted boolean            -- Dùng cho xóa mềm
}
```

**Group values:**
- `card_size` - cấu hình size thiệp
- `card_config` - cấu hình calligraphy words

**Kind values:**
- `Setting` - cho card configs

**Ví dụ dữ liệu:**

| group | key | kind | value |
|-------|-----|------|-------|
| `card_size` | `size_A` | `Setting` | `{ "name": "A", "width": 10, "height": 15, "base_price": 100000, "max_words": 100 }` |
| `card_size` | `size_B` | `Setting` | `{ "name": "B", "width": 15, "height": 20, "base_price": 150000, "max_words": 150 }` |
| `card_config` | `calligraphy_words` | `Setting` | `[ { "min_words": 0, "max_words": 35, "extra_price": 0 }, ... ]` |

**Cấu trúc JSON cho Size:**
```json
{
  "name": "A",
  "width": 10,
  "height": 15,
  "base_price": 100000,
  "max_words": 100
}
```

**Cấu trúc JSON cho Calligraphy Words:**
```json
[
  { "min_words": 0, "max_words": 35, "extra_price": 0 },
  { "min_words": 36, "max_words": 70, "extra_price": 39000 },
  { "min_words": 71, "max_words": 100, "extra_price": 69000 }
]
```

**Seed Data:**
```json
[
  { "group": "card_size", "key": "size_A", "kind": "Setting", "value": { "name": "A", "width": 10, "height": 15, "base_price": 100000, "max_words": 100 } },
  { "group": "card_size", "key": "size_B", "kind": "Setting", "value": { "name": "B", "width": 15, "height": 20, "base_price": 150000, "max_words": 150 } },
  { "group": "card_size", "key": "size_C", "kind": "Setting", "value": { "name": "C", "width": 20, "height": 25, "base_price": 200000, "max_words": 200 } },
  { "group": "card_config", "key": "calligraphy_words", "kind": "Setting", "value": [
    { "min_words": 0, "max_words": 35, "extra_price": 0 },
    { "min_words": 36, "max_words": 70, "extra_price": 39000 },
    { "min_words": 71, "max_words": 100, "extra_price": 69000 }
  ]}
]
```

### system_prompts
Prompt hệ thống cho từng loại generation.

```sql
Table system_prompts {
  id guid [pk]
  type system_prompt_types
  content string
}
```

### platform_standards
Tiêu chuẩn nội dung của từng nền tảng (zalo, facebook, instagram).

```sql
Table platform_standards {
  id guid [pk]
  platform platform_type
  description string
}
```

### card_templates
Template dùng để tạo card.

```sql
Table card_templates {
  id guid [pk]
  name string
  description string
  character_limit int
  image_url string
  metadata json
}
```

### base_posts
Nội dung bài viết mẫu.

```sql
Table base_posts {
  id guid [pk]
  content string
  image_url string
}
```

### generated_posts
Bài viết được tạo ra từ AI.

```sql
Table generated_posts {
  id guid [pk]
  content string
  image_url string
  user_id guid
  created_at timestamp
}
```

### generated_flowers
Hình ảnh hoa được tạo ra.

```sql
Table generated_flowers {
  id guid [pk]
  image_url string
  user_id guid
  order_id guid
  occasion varchar
  style varchar
  budget decimal
  note text
  created_at timestamp
}
```

### generated_cards
Card được tạo ra.

```sql
Table generated_cards {
  id guid [pk]
  content string
  image_url string
  user_id guid
  order_id guid
  created_at timestamp

  -- Thông tin thiệp:
  form_type form_type          -- "go_may" hoặc "calligraphy"
  size_key varchar             -- Key của size: "size_A", "size_B"
  sender_name string           -- Người gửi
  receiver_name string         -- Người nhận
  message_content string       -- Lời chúc
  attached_image_url string    -- Ảnh đính kèm (nullable)

  -- Snapshot Size tại thời điểm tạo:
  size_name varchar             -- "A", "B", "C"
  size_width decimal            -- 10 (cm)
  size_height decimal           -- 15 (cm)
  size_base_price decimal       -- Giá base tại thời điểm đó: 100000
  size_max_words int            -- Giới hạn từ của size đó: 100

  -- Snapshot Word count tại thời điểm tạo:
  word_count int               -- Số từ thực tế: 60
  word_config_snapshot json     -- JSON lưu rule áp dụng: { "min_words": 36, "max_words": 70, "extra_price": 39000 }

  -- Trạng thái:
  is_confirmed boolean         -- Đã xác nhận gắn vào Checkout chưa (default: false)

  -- Giá đã tính toán:
  base_price decimal           -- = size_base_price
  extra_price decimal          -- Phụ phí Calligraphy = word_config_snapshot.extra_price
  total_price decimal          -- = base_price + extra_price
}
```

### post_histories
Lịch sử tạo bài viết.

```sql
Table post_histories {
  id guid [pk]
  input string
  base_id guid
  output_id guid
  system_prompt_id guid
  metadata json
  created_at timestamp

  -- Refs:
  system_prompt_id -> system_prompts.id
}
```

### mockup
Hình ảnh mockup.

```sql
Table mockup {
  id guid [pk]
  image_url string
  is_active boolean
  description string
}
```

### client_histories
Lịch sử thao tác của người dùng (dùng chung cho flower và card).

```sql
Table client_histories {
  id guid [pk]
  user_id guid                 -- Ai tạo history
  input string                 -- JSON string của request gốc
  base_id guid
  output_id guid
  order_id guid
  type generation_type         -- "flower", "card", "post"
  system_prompt_id guid
  metadata json
  created_at timestamp

  -- Cho "Tạo lại":
  source_card_id guid          -- ID của thiệp nguồn khi "Tạo lại" (nullable)
  source_flower_id guid        -- ID của mẫu hoa nguồn (nullable)

  -- Refs:
  system_prompt_id -> system_prompts.id
}
```

---

## Quy tắc Pricing (Logic BE)

### 1. Lấy cấu hình từ bảng Config

```csharp
// Lấy cấu hình size
var sizeConfig = await _dbContext.Config
    .Where(c => c.Group == "card_size" && c.Key == $"size_{size}" && c.Kind == "Setting")
    .FirstOrDefaultAsync();
// sizeConfig.Value: { "name": "A", "width": 10, "height": 15, "base_price": 100000, "max_words": 100 }

// Lấy cấu hình calligraphy words
var wordsConfig = await _dbContext.Config
    .Where(c => c.Group == "card_config" && c.Key == "calligraphy_words" && c.Kind == "Setting")
    .FirstOrDefaultAsync();
// wordsConfig.Value: [ { "min_words": 0, "max_words": 35, "extra_price": 0 }, ... ]
```

### 2. Tính giá

```csharp
public async Task<decimal> CalculateExtraPriceAsync(string formType, int wordCount)
{
    if (formType != "calligraphy")
        return 0;

    var wordsConfig = await GetCalligraphyWordsConfigAsync();
    var rules = JsonSerializer.Deserialize<List<WordRule>>(wordsConfig.Value);

    var rule = rules.FirstOrDefault(r => wordCount >= r.MinWords && wordCount <= r.MaxWords);
    return rule?.ExtraPrice ?? 0;
}
```

### 3. Snapshot khi tạo thiệp

```csharp
public async Task<GeneratedCard> CreateCardAsync(CreateCardRequest request)
{
    // 1. Lấy size config
    var sizeConfig = await GetSizeConfigAsync(request.Size);
    var sizeData = JsonSerializer.Deserialize<SizeConfig>(sizeConfig.Value);

    // 2. Đếm số từ
    var wordCount = CountWords(request.MessageContent);

    // 3. Tính extra price (nếu calligraphy)
    var extraPrice = await CalculateExtraPriceAsync(request.FormType, wordCount);

    // 4. Snapshot word config
    var wordRule = GetWordRule(wordCount);

    var card = new GeneratedCard
    {
        // ... basic fields ...

        // Size snapshot
        SizeKey = $"size_{request.Size}",
        SizeName = sizeData.Name,
        SizeWidth = sizeData.Width,
        SizeHeight = sizeData.Height,
        SizeBasePrice = sizeData.BasePrice,
        SizeMaxWords = sizeData.MaxWords,

        // Word snapshot
        WordCount = wordCount,
        WordConfigSnapshot = JsonSerializer.Serialize(wordRule),

        // Pricing
        BasePrice = sizeData.BasePrice,
        ExtraPrice = extraPrice,
        TotalPrice = sizeData.BasePrice + extraPrice
    };

    return card;
}
```

### 4. Ví dụ kết quả

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

## Quy tắc Quota (Logic BE)

### Giới hạn theo ngày
- **10 lượt tạo thiệp/ngày/khách hàng** (cho STORY-035)
- **3 lượt tạo hoa/ngày/khách hàng** (cho tính năng tạo hoa)

### Giới hạn theo mẫu hoa (áp dụng cho thiệp generate ra từ hoa)
- Mỗi mẫu hoa đã generate ra chỉ được tạo tối đa **3 lần/ngày**
- Muốn tạo thêm lần thứ 4 phải đợi qua ngày hôm sau

### Quy trình Quota
1. **Check quota** → TRƯỚC khi gọi AI
2. **Trừ quota** → KHI CÓ KẾT QUẢ THÀNH CÔNG
3. **Hoàn quota** → KHI AI THẤT BẠI (sau retry)

### Retry Logic
- **2 lần retry** với interval **2 giây**
- Nếu vẫn thất bại sau retry → **Hoàn quota**

---

## Quy tắc History

### Khi nào tạo client_histories?
- **CHỈ tạo khi có ảnh hợp lệ** (thành công)
- Không tạo khi thất bại (sau retry vẫn thất bại)

### Khi "Tạo lại" thiệp (STORY-036):
- `source_card_id` = ID của thiệp nguồn
- `source_flower_id` = ID của mẫu hoa nguồn (nếu có)
- Lấy thông tin từ thiệp cha + System form + User input để generate thiệp mới

### Input trong client_histories
- Lưu JSON string của request gốc

---

## Lưu ý quan trọng

> Phần AI (generate ảnh, retry, business rules liên quan) sẽ được tách riêng trong 1 hàm/ module khác. TDD này chỉ tập trung vào logic nghiệp vụ xử lý sau khi có kết quả từ AI.

---

## Relationships

```sql
Ref: post_histories.system_prompt_id > system_prompts.id
Ref: client_histories.system_prompt_id > system_prompts.id
```

---

## TODO

- [x] Xem Product Entity trong codebase - đã có sẵn
- [x] Xem bảng Config có sẵn trong codebase - sử dụng cho card configs
- [x] Cập nhật TDD-035, TDD-036 sử dụng bảng Config
- [x] Tạo TDD-050, TDD-051, TDD-052, TDD-053 cho CRUD card configs
- [ ] Seed data cho card configs
- [ ] Implement pricing logic trong Backend
- [ ] Implement quota logic trong Backend

---

## Ghi chú

- Card ↔ Order: tạm thời bỏ qua bảng trung gian
- generated_flowers: có thêm fields occasion, style, budget, note
- mockup: có thêm field is_active
- Bảng Config có sẵn trong codebase, không cần tạo bảng mới
