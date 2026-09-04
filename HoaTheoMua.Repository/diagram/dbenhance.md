```text
// AI Customize Database Schema
// Project: HTM_Flourist_AI
// Date: 2026-08-26

Enum platform_type {
  zalo
  facebook
  instagram
}

Enum generation_type {
  flower
  card
}

Enum system_prompt_types {
  flower
  card
  post
}

Enum form_type {
  go_may
  calligraphy
}

// Prompt hệ thống cho từng loại generation
Table system_prompts {
  id uuid [pk]
  type system_prompt_types
  content text
}

// Tiêu chuẩn nội dung của từng nền tảng
Table platform_standards {
  id uuid [pk]
  platform platform_type
  description text
}



// Template dùng để tạo card
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

// Nội dung bài viết mẫu
Table base_posts {
  id uuid [pk]
  content text
  image_url varchar
}

// Bài viết được tạo ra từ AI
Table generated_posts {
  id uuid [pk]
  content text
  image_url varchar
  user_id uuid
  created_at timestamp
}

// Hình ảnh hoa được tạo ra
Table generated_flowers {
  id uuid [pk]
  input_snapshot text
  image_url varchar
  user_id uuid
  order_id uuid
  is_deleted boolean
  created_at timestamp
  updated_at timestamp
}

// Card được tạo ra - với snapshot giá tại thời điểm tạo
Table generated_cards {
  id uuid [pk]
  content text
  image_url varchar
  user_id uuid
  order_id uuid
  form_type form_type

  // Thông tin thiệp
  size_key varchar
  sender_name varchar
  receiver_name varchar
  message_content text
  attached_image_url varchar

  // Snapshot Size tại thời điểm tạo
  size_name varchar
  size_width decimal
  size_height decimal
  size_base_price decimal
  size_max_words int

  // Snapshot Word count tại thời điểm tạo
  word_count int
  word_config_snapshot json

  // Trạng thái
  is_confirmed boolean

  // Giá
  base_price decimal
  extra_price decimal
  total_price decimal

  //
  is_deleted boolean
  created_at timestamp
  updated_at timestamp
}

// Lịch sử tạo bài viết
Table post_histories {
  id uuid [pk]
  input text
  base_id uuid
  output_id uuid
  system_prompt_id uuid
  metadata json
  created_at timestamp
}

// Hình ảnh mockup
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

// Lịch sử thao tác của người dùng
Table client_histories {
  id uuid [pk]
  user_id uuid
  input text
  base_id uuid
  output_id uuid
  order_id uuid
  type generation_type
  system_prompt_id uuid
  metadata json
  is_deleted boolean
  created_at timestamp
  updated_at timestamp
}

// Relationships
Ref: post_histories.system_prompt_id > system_prompts.id
Ref: client_histories.system_prompt_id > system_prompts.id
```
