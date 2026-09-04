# AI Customize Database Schema - HTM_Flourist_AI

> Document First Project: hoa-theo-mua-ai-customize  
> Date: 2026-08-28

## Phạm vi và nguyên tắc

- Flow tạo hoa của STORY-030 và STORY-033 dùng chung một API đồng bộ.
- Không có bảng `flower_requests` hoặc `flower_ai_jobs`.
- API nhận input, gọi AI (kèm retry), trả kết quả; chỉ khi thành công mới lưu `generated_flowers` và `client_histories`.
- `client_histories.input` là JSON request gốc của lần tạo thành công; đây là history, không phải bảng trạng thái/tác vụ tạm.
- Card giữ logic hiện tại: cấu hình dùng bảng `Config`, còn quan hệ Card–Flower được suy ra từ `client_histories`, không dùng `generated_cards.source_flower_id`.
- Không thêm bảng mới. `generated_flowers.input_snapshot` và `client_histories.metadata` giữ snapshot/provenance bất biến; soft-delete nguồn không xóa history/snapshot.
- `products.sellableQuantity` là giá trị dẫn xuất từ dữ liệu live bằng cùng thuật toán `CalculateAvailableQty`, không phải column trong schema.

## Enums

```sql
Enum platform_type { zalo facebook instagram }
Enum generation_type { flower card post }
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
  // Nhiều key trong cùng group được phép; tối đa một active/chưa xóa cho exact (group,key)
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
  character_limit int
  image_url varchar
  metadata json
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
  image_url varchar
  user_id uuid
  created_at timestamp
  form_type form_type
  size_key varchar
  sender_name varchar
  receiver_name varchar
  message_content text
  attached_image_url varchar
  size_name varchar
  size_width decimal
  size_height decimal
  size_base_price decimal
  size_max_words int
  word_count int
  word_config_snapshot json
  is_confirmed boolean
  base_price decimal
  extra_price decimal
  total_price decimal
}

Table post_histories {
  id uuid [pk]
  input text
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
  input text
  type generation_type
  system_prompt_id uuid
  metadata json
  created_at timestamp
  base_id uuid
  output_id uuid
}
```

## Quan hệ

`client_histories` dùng các tham chiếu đa hình:

| type | base_id | output_id | metadata |
|---|---|---|---|
| flower | `products.id` | `generated_flowers.id` | Snapshot Product, Mockup, ảnh, full system prompt và validation |
| card (hoa thường) | `products.id` | `generated_cards.id` | `source_type=product`, `origin_product_id`, snapshot template/config/prompt |
| card (hoa AI) | `generated_flowers.id` | `generated_cards.id` | `source_type=generated_flower`, `generated_flower_id`, `origin_product_id` hoặc provenance unavailable |
| post | `base_posts.id` | `generated_posts.id` | Theo loại bài viết |

```sql
Ref: post_histories.system_prompt_id > system_prompts.id
Ref: client_histories.system_prompt_id > system_prompts.id
Ref: client_histories.base_id > products.id
Ref: client_histories.base_id > generated_flowers.id
Ref: client_histories.output_id > generated_flowers.id
Ref: client_histories.output_id > generated_cards.id
Ref: client_histories.output_id > generated_posts.id
```

`mockup` được truyền vào API tạo hoa và snapshot trong `generated_flowers.input_snapshot` cùng `client_histories.metadata`; không cần FK riêng cho flow tạo hoa.

Lineage khi Order kiểm tra nguồn live:

```text
generated_card
  -> client_histories(type=card, output_id=generated_card.id)
  -> base_id = product.id
     hoặc base_id = generated_flower.id
        -> client_histories(type=flower, output_id=generated_flower.id).base_id = product.id
  -> products live status + CalculateAvailableQty
```

Không được thêm `generated_cards.source_flower_id`. Nếu history cũ không resolve được Product gốc, history/Card vẫn xem và regenerate được khi snapshot đủ, nhưng Order bị chặn bằng `SOURCE_PRODUCT_PROVENANCE_INVALID`. Product live quyết định Order; Mockup, Template và Config không quyết định tồn kho Order.
