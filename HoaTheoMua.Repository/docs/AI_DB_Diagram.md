# AI Customize Database Schema - HTM_Flourist_AI

> Document First Project: hoa-theo-mua-ai-customize  
> Date: 2026-09-09

## Phạm vi và nguyên tắc

- Flow tạo hoa của STORY-030 và STORY-033 dùng chung một API đồng bộ.
- Không có bảng `flower_requests` hoặc `flower_ai_jobs`.
- API nhận input, gọi AI (kèm retry), trả kết quả; chỉ khi thành công mới lưu `generated_flowers` và `client_histories`.
- `client_histories` không lưu request input; dữ liệu kết quả Card nằm ở `generated_cards`, dữ liệu tạo Flower nằm ở `generated_flowers.input_snapshot`, còn history chỉ giữ loại, lineage, output, provenance và System Prompt snapshot.
- `client_histories.root` là UUID bắt buộc, luôn lưu `products.id` gốc của History `flower`, `card` hoặc `handmade_card`. Giá trị do service xác định; không tạo FK, navigation hoặc quan hệ database cho cột này.
- Card giữ logic hiện tại: cấu hình dùng bảng `Config`, còn quan hệ Card–Flower được suy ra từ `client_histories`, không dùng `generated_cards.source_flower_id`.
- `card_templates` dùng string bắt buộc `template_type="ai"|"handmade"`; cả hai loại kết quả đều lưu trong `generated_cards` và phân biệt bằng string bắt buộc `card_type`.
- Khi áp dụng schema cho dữ liệu hiện có, backfill `card_templates.template_type="ai"` và `generated_cards.card_type="ai"` trước khi bật ràng buộc không null. Hai discriminator chỉ nhận lowercase; `template_type` không được đổi sau khi tạo.
- HandMade Card không gọi AI, không dùng prompt/quota, luôn viết tay và chỉ tính phụ phí số từ; `system_form` của history bằng null.
- `client_histories.type` là string mở rộng. Phạm vi hiện tại của `root` gồm `flower`, `card`, `handmade_card`; `post` chưa được sử dụng trong flow này và không được suy diễn quy tắc `root`.
- Không thêm bảng mới. `generated_flowers.input_snapshot`, `client_histories.metadata` và `client_histories.system_form` giữ snapshot/provenance bất biến; soft-delete nguồn không xóa history/snapshot.
- `products.sellableQuantity` là giá trị dẫn xuất từ dữ liệu live bằng cùng thuật toán `CalculateAvailableQty`, không phải column trong schema.

## Enums

```sql
Enum platform_type { zalo facebook instagram }
Enum system_prompt_types { flower card post }
Enum form_type { go_may calligraphy }
```

## Tables

```sql
Table system_prompts {
  id uuid [pk]
  type system_prompt_types
  content text
}

Table platform_standards {
  id uuid [pk]
  platform platform_type
  description text
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
  // card_config.sizes có đúng một Size chung; client không chọn Size.
}

Table products {
  id uuid [pk]
  is_active boolean
  is_deleted boolean
  // sellableQuantity là derived value, không lưu column
}

Table card_templates {
  id uuid [pk]
  name varchar
  description text
  image_url varchar
  template_type varchar          // "ai" | "handmade", bắt buộc; không đổi sau khi tạo
  is_active boolean
  created_at timestamp
  updated_at timestamp
  is_deleted boolean
}

Table base_posts {
  id uuid [pk]
  content text
  image_url varchar
}

Table generated_posts {
  id uuid [pk]
  content text
  image_url varchar
  user_id uuid
  created_at timestamp
}

Table generated_flowers {
  id uuid [pk]
  image_url varchar
  user_id uuid
  input_snapshot json
  created_at timestamp
}

Table generated_cards {
  id uuid [pk]
  content text
  image_url varchar                 // ảnh hoàn chỉnh sau khi chèn chữ
  raw_image varchar                  // ảnh gốc AI sinh trước khi chèn chữ; nullable cho legacy
  user_id uuid
  created_at timestamp
  card_type varchar              // "ai" | "handmade", bắt buộc
  form_type form_type
  sender_name varchar
  receiver_name varchar
  message_content text
  attached_image_url varchar     // null với handmade
  size_name varchar
  size_width decimal
  size_height decimal
  size_base_price decimal
  size_max_words int
  word_count int
  word_config_snapshot json
  base_price decimal
  extra_price decimal
  total_price decimal
}

Table post_histories {
  id uuid [pk]
  input json
  base_id uuid
  output_id uuid
  system_prompt_id uuid
  metadata json
  created_at timestamp
}

Table mockup {
  id uuid [pk]
  name varchar
  description text
  image_url varchar
  is_active boolean
  is_deleted boolean
  created_at timestamp
  updated_at timestamp
}

Table client_histories {
  id uuid [pk]
  user_id uuid
  type varchar                   // flower | card | handmade_card; post hiện chưa dùng
  system_form json [null]         // snapshot system_prompts trừ id; null với handmade_card
  metadata json
  created_at timestamp
  root uuid [not null]          // products.id gốc; do code gán, không có FK
  base_id uuid
  output_id uuid
}
```

Ràng buộc nghiệp vụ cho `generated_cards.card_type="handmade"`: `form_type="calligraphy"`, `attached_image_url=null`, `base_price=0`, `total_price=extra_price`. `size_base_price` vẫn giữ giá Size Config tại thời điểm tạo để audit. Với Card AI mới, `raw_image` và `image_url` phải cùng tồn tại sau persist; `raw_image` nullable chỉ để giữ tương thích dữ liệu cũ.

## Quan hệ

`client_histories` dùng các tham chiếu đa hình; `root` luôn là Product ID gốc:

| type | base_id | output_id | root | metadata |
|---|---|---|---|---|
| flower | `products.id` | `generated_flowers.id` | Bằng `base_id` | Snapshot Product, Mockup, ảnh và validation; System Prompt nằm trong `system_form` |
| card (hoa thường) | `products.id` | `generated_cards.id` | Bằng `base_id` | `source_type=product`, `origin_product_id`, provenance và Template snapshot không gồm ảnh |
| card (hoa AI) | `generated_flowers.id` | `generated_cards.id` | `base_id` của `client_histories(type=flower, output_id=generated_flowers.id)` | Provenance bắt buộc đầy đủ; Template snapshot không gồm ảnh |
| handmade_card (hoa thường) | `products.id` | `generated_cards.id` | Bằng `base_id` | Provenance và Template snapshot không gồm ảnh; `system_form=null` |
| handmade_card (hoa AI) | `generated_flowers.id` | `generated_cards.id` | `base_id` của Flower History tương ứng | Provenance và Template snapshot không gồm ảnh; `system_form=null` |

Đối với các loại đang dùng trong `client_histories`, column `input` bị loại bỏ. `system_form` là JSON nullable, không có FK: AI Flower/Card lưu `{type, content}` của System Prompt thực tế và HandMade lưu `null`. `post_histories` là bảng khác nên tiếp tục giữ `system_prompt_id` hiện tại; Post nằm ngoài phạm vi quy tắc `root` này.

```sql
Ref: post_histories.system_prompt_id > system_prompts.id
```

Không khai báo `Ref` cho `client_histories.base_id`, `root` hoặc `output_id`. Đây là các giá trị lineage đa hình do code quản lý, không phải quan hệ database.

`mockup` được truyền vào API tạo hoa và snapshot trong `generated_flowers.input_snapshot` cùng `client_histories.metadata`; không cần FK riêng cho flow tạo hoa.

Lineage khi Order kiểm tra nguồn live:

```text
generated_card
  -> client_histories(type=card hoặc handmade_card, output_id=generated_card.id)
  -> root = product.id gốc
  -> base_id = product.id
     hoặc base_id = generated_flower.id
        -> client_histories(type=flower, output_id=generated_flower.id).base_id = product.id
  -> products live status + CalculateAvailableQty
```

Không được thêm `generated_cards.source_flower_id`. Card AI history cũ không resolve được Product gốc vẫn xem được nhưng không được dùng làm nguồn regenerate/recreate; HandMade Card mới bắt buộc có provenance đầy đủ và không hỗ trợ regenerate. Product live quyết định Order; Mockup, Template và Config không quyết định tồn kho Order.

TDD-027 dùng `generated_cards.raw_image` của Card AI nguồn làm image-reference để tạo phiên bản thay nội dung. Card legacy có `raw_image=null` không dùng được cho flow này; History Card mới kế thừa cả `base_id` và `root` của History nguồn, đồng thời lưu Card được chọn trong metadata, không thêm FK mới.

Khi backfill dữ liệu cũ, History `flower` lấy `root=base_id`; Card trực tiếp từ Product lấy `root=base_id`; Card từ Generated Flower lấy `root` từ `base_id` của Flower History tương ứng. Không dùng `Guid.Empty`, không tự đoán và phải dừng migration nếu còn record `flower`, `card` hoặc `handmade_card` không xác định được Product ID gốc. `root` không được thêm vào response API công khai.
