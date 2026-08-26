# TDD-036: Tạo lại thiệp từ lịch sử

## Thông tin tài liệu
- **Tiêu đề**: Tạo lại thiệp thiết kế AI từ lịch sử
- **Ghi chú**: API cho phép khách hàng tạo lại thiệp từ một thiệp đã có trong lịch sử. Hệ thống sử dụng nguyên thông tin của thiệp nguồn (bao gồm snapshot giá), không mở form chỉnh sửa. Có giới hạn quota riêng theo mẫu hoa (3 lần/ngày cho mỗi mẫu hoa đã generate).

## Metadata quản trị tài liệu
| Trường | Giá trị |
|---|---|
| Mã tài liệu | TDD-036 |
| Phiên bản | v0.2 |
| Author | |
| Reviewer | |
| Approver | Chưa chỉnh định |
| Owner | |
| Cập nhật gần nhất | 2026-08-26 |

---

## BƯỚC 1 — Thông tin & Bối cảnh

### Thông tin tài liệu
- **Mã tài liệu**: TDD-036
- **Tính năng**: Tạo lại thiệp từ lịch sử
- **Tác giả**: 
- **Người review**: 
- **Phiên bản**: v0.2
- **Cập nhật (YYYY-MM-DD)**: 2026-08-26
- **Story liên quan**:
  - STORY-036

### Business Rules
- BR-036-01: Khi tạo lại, hệ thống lấy thông tin từ thiệp nguồn (source_card_id) kết hợp với System form để generate thiệp mới
- BR-036-02: Hệ thống KHÔNG mở form cho khách hàng chỉnh sửa - dùng nguyên dữ liệu thiệp nguồn
- BR-036-03: source_card_id bắt buộc phải tồn tại và thuộc về khách hàng hiện tại
- BR-036-04: source_flower_id được ghi nhận nếu thiệp nguồn được tạo từ mẫu hoa
- BR-036-05: Thiệp mới sử dụng snapshot config từ thiệp nguồn (size_name, size_width, size_height, size_base_price, size_max_words, word_config_snapshot)
- BR-036-06: Trước khi gọi AI, kiểm tra:
  - Quota tạo thiệp của khách hàng (10 lượt/ngày)
  - Quota theo mẫu hoa (3 lượt/mẫu hoa/ngày) - nếu có source_flower_id
- BR-036-07: Mỗi mẫu hoa đã generate ra chỉ được tạo tối đa 3 lần/ngày
- BR-036-08: Khi tạo thành công:
  - Trừ 1 lượt quota tạo thiệp
  - Trừ 1 lượt quota theo mẫu hoa (nếu có)
  - Tạo client_histories với source_card_id = ID thiệp nguồn
  - Tạo record generated_cards mới (KHÔNG ghi đè thiệp nguồn)
- BR-036-09: Khi AI thất bại (sau 2 lần retry), hoàn quota đã trừ
- BR-036-10: client_histories chỉ được tạo khi có ảnh hợp lệ
- BR-036-11: Thiệp nguồn KHÔNG bị xóa hoặc ghi đè
- BR-036-12: Kết quả mới không tự động gắn vào Checkout (vì thao tác từ lịch sử)

### Bối cảnh & Mục tiêu

**Vấn đề**
> Khách hàng muốn tạo phiên bản mới của một thiệp đã có mà không cần nhập lại thông tin. Hệ thống cần lấy nguyên thông tin từ thiệp nguồn (bao gồm snapshot config về giá), đồng thời quản lý quota theo mẫu hoa để tránh spam tạo ảnh giống nhau.

**Mục tiêu**
- Tạo thiệp mới từ thiệp nguồn mà không cần form chỉnh sửa
- Giữ nguyên thông tin: sender_name, receiver_name, message_content, attached_image_url, form_type, size_key, và các snapshot giá
- Kiểm tra và quản lý quota (10 lượt/ngày + 3 lượt/mẫu hoa/ngày)
- Không ghi đè hoặc xóa thiệp nguồn
- Ghi nhận source_card_id và source_flower_id trong client_histories

**Ngoài phạm vi** (Out of scope)
- AI generation thực tế (được tách trong module riêng)
- Retry logic khi AI thất bại (được xử lý trong AI module)
- Tự động gắn kết quả vào Checkout
- Chỉnh sửa thông tin trước khi tạo lại

---

## BƯỚC 2 — Kiến trúc & Sơ đồ

### Kiến trúc tổng quan (Architecture)
**Tiêu đề**: Trình tự tạo lại thiệp từ lịch sử
**Mô tả**: Client gọi API tạo lại → Controller nhận request với source_card_id → Service lấy thông tin thiệp nguồn (bao gồm snapshot) → Kiểm tra quota → Gọi AI module → Lưu thiệp mới (không ghi đè) → Trừ quota → Trả kết quả

```mermaid
flowchart LR
    Client["Khách hàng"]
    FE["Frontend"]
    API["AICardController"]
    Svc["AICardService"]
    AI["AIModule<br/>(Generate ảnh)"]
    DB[("Database")]

    Client --> FE
    FE -->|"POST /api/ai-cards/{id}/regenerate"| API
    API --> Svc
    Svc -->|"Lấy thiệp nguồn<br/>(với snapshot)"| DB
    Svc -->|"Check Quota Ngày<br/>Check Quota Mẫu Hoa"| DB
    Svc --> AI
    AI -->|"Ảnh thiệp"| Svc
    Svc -->|"Copy snapshot từ<br/>thiệp nguồn"| Svc
    Svc -->|"Lưu thiệp MỚI"| DB
    Svc -->|"Trừ Quota"| DB
    Svc -->|"Tạo History<br/>(source_card_id)"| DB
    Svc --> API
    API --> FE
    FE --> Client
```

### Sequence Diagram
**Tiêu đề**: Trình tự tạo lại thiệp từ lịch sử
**Mô tả**: Từng bước gọi qua lại giữa các thành phần

```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant FE as Frontend
    participant C as AICardController
    participant S as AICardService
    participant AI as AIModule
    participant DB as Database

    Client->>FE: Chọn "Tạo lại" từ thiệp trong lịch sử
    FE->>C: POST /api/ai-cards/{source_card_id}/regenerate

    Note over S: GIAI ĐOẠN 1: Lấy thông tin thiệp nguồn
    S->>DB: Lấy thiệp nguồn theo source_card_id
    alt Thiệp nguồn không tồn tại
        S-->>C: Trả lỗi not found
        C-->>FE: HTTP 404
    end
    alt Thiệp nguồn không thuộc khách hàng
        S-->>C: Trả lỗi unauthorized
        C-->>FE: HTTP 403
    end

    Note over S: GIAI ĐOẠN 2: Kiểm tra Quota
    S->>DB: Kiểm tra quota tạo thiệp (10 lượt/ngày)
    alt Hết quota tạo thiệp
        S-->>C: Trả lỗi quota exceeded
        C-->>FE: HTTP 403
        FE-->>Client: "Bạn đã sử dụng hết 10 lượt tạo thiệp AI trong ngày."
    end

    alt Có source_flower_id
        S->>DB: Kiểm tra quota theo mẫu hoa (3 lượt/mẫu hoa/ngày)
        alt Hết quota mẫu hoa
            S-->>C: Trả lỗi quota exceeded
            C-->>FE: HTTP 403
            FE-->>Client: "Bạn đã sử dụng hết số lượt tạo thiệp cho mẫu hoa này."
        end
    end

    Note over S: GIAI ĐOẠN 3: Gọi AI generate
    S->>AI: Gọi AI với dữ liệu từ thiệp nguồn
    AI-->>S: Ảnh thiệp (thành công/thất bại)

    alt AI thất bại (sau retry)
        Note over S: Hoàn quota nếu đã trừ
        S-->>C: Trả lỗi AI failed
    end

    Note over S: GIAI ĐOẠN 4: Copy snapshot & Lưu
    S->>S: Copy snapshot từ thiệp nguồn:
    Note over S: - size_key, size_name, size_width
    Note over S: - size_height, size_base_price, size_max_words
    Note over S: - word_count, word_config_snapshot
    S->>DB: Lưu generated_cards MỚI (không ghi đè)
    S->>DB: Trừ quota tạo thiệp (+1)
    alt Có source_flower_id
        S->>DB: Trừ quota theo mẫu hoa (+1)
    end
    S->>DB: Tạo client_histories với source_card_id

    S-->>C: GeneratedCard result
    C-->>FE: HTTP 200 {card}
    FE-->>Client: Hiển thị thiệp mới (thiệp nguồn vẫn giữ nguyên)
```

### Activity Diagram
**Tiêu đề**: Quy trình tạo lại thiệp từ lịch sử
**Mô tả**: Sơ đồ quyết định khi tạo lại thiệp

```mermaid
flowchart TD
    A[Bắt đầu] --> B{Lấy thiệp nguồn}
    B -->|Không tìm thấy| E[Trả lỗi 404]
    B -->|Không thuộc KH| F[Trả lỗi 403]
    B -->|Thành công| C{Kiểm tra quota<br/>tạo thiệp}

    C -->|Hết quota| G[Trả lỗi 403<br/>Hết quota tạo thiệp]
    C -->|Còn quota| D{Có<br/>source_flower_id?}

    D -->|Không| H{Kiểm tra quota<br/>mẫu hoa}
    D -->|Có| I{Gọi AI}
    H -->|Hết quota| J[Trả lỗi 403<br/>Hết quota mẫu hoa]
    H -->|Còn quota| I

    I -->|Thất bại| K[Hoàn quota<br/>Trả lỗi AI]
    I -->|Thành công| L[Copy snapshot từ<br/>thiệp nguồn<br/>Lưu thiệp MỚI<br/>Trừ quota<br/>Tạo History]

    E --> Z[Kết thúc]
    F --> Z
    G --> Z
    J --> Z
    K --> Z
    L --> Z
```

### Mô hình dữ liệu (Data Model / ERD)
**Tiêu đề**: Các bảng liên quan đến tạo lại thiệp
**Mô tả**: Config (bảng có sẵn), generated_cards (nguồn và mới), client_histories

```mermaid
erDiagram
    Config ||--o{ generated_cards : "size_config"
    generated_cards ||--o{ client_histories : "source_card"
    generated_cards ||--|| generated_flowers : "source_flower"

    Config {
        uuid id PK
        string key
        jsonb value
        string group
        string kind
        bool is_public
        bool is_deleted
        guid user_id
        timestamp created_at
        timestamp updated_at
    }

    generated_cards {
        uuid id PK
        string content
        string image_url
        uuid user_id
        uuid order_id
        timestamp created_at
        string form_type
        string size_key
        string sender_name
        string receiver_name
        string message_content
        string attached_image_url
        string size_name
        decimal size_width
        decimal size_height
        decimal size_base_price
        int size_max_words
        int word_count
        json word_config_snapshot
        boolean is_confirmed
        decimal base_price
        decimal extra_price
        decimal total_price
    }

    generated_flowers {
        uuid id PK
        string image_url
        uuid user_id
        uuid order_id
        timestamp created_at
    }

    client_histories {
        uuid id PK
        uuid user_id
        string input
        uuid base_id
        uuid output_id
        uuid order_id
        string type
        uuid system_prompt_id
        json metadata
        timestamp created_at
        uuid source_card_id FK
        uuid source_flower_id FK
    }
```

---

## BƯỚC 3 — API

### API Contract nội bộ

#### Endpoint #1: Tạo lại thiệp từ lịch sử
- **Method**: POST
- **Endpoint**: `/api/ai-cards/{id}/regenerate`
- **Tên endpoint**: Tạo lại thiệp từ lịch sử
- **Mô tả**: Tạo thiệp mới từ thiệp nguồn trong lịch sử, sử dụng nguyên snapshot config, không cần form chỉnh sửa

**Ví dụ 1 — Happy path: Tạo lại thành công** — HTTP `200`

Request:
```
POST /api/ai-cards/550e8400-e29b-41d4-a716-446655440001/regenerate
```
(Không cần body - lấy thông tin từ source_card_id)

Response:
```json
{
  "value": {
    "id": "880e8400-e29b-41d4-a716-446655440099",
    "content": "...",
    "image_url": "https://storage.example.com/cards/880e8400.png",
    "user_id": "770e8400-e29b-41d4-a716-446655440003",
    "order_id": null,
    "created_at": "2026-08-26T11:00:00Z",
    "form_type": "go_may",
    "size_key": "size_A",
    "sender_name": "Nguyễn An",
    "receiver_name": "Trần Bình",
    "message_content": "Chúc bạn sinh nhật vui vẻ",
    "attached_image_url": null,
    "size_name": "A",
    "size_width": 10,
    "size_height": 15,
    "size_base_price": 100000,
    "size_max_words": 100,
    "word_count": 5,
    "word_config_snapshot": null,
    "is_confirmed": false,
    "base_price": 100000,
    "extra_price": 0,
    "total_price": 100000
  }
}
```

**Ghi chú:** Thiệp nguồn (id: 550e8400...) vẫn tồn tại trong DB với snapshot giá cũ, không bị ghi đè.

**Ví dụ 2 — Happy path: Tạo lại thiệp Calligraphy từ mẫu hoa** — HTTP `200`

Request:
```
POST /api/ai-cards/550e8400-e29b-41d4-a716-446655440002/regenerate
```

Response:
```json
{
  "value": {
    "id": "990e8400-e29b-41d4-a716-446655440100",
    "form_type": "calligraphy",
    "size_key": "size_B",
    "sender_name": "Hoàng Thị Lan",
    "receiver_name": "Lê Văn Minh",
    "message_content": "Chúc mừng năm mới...",
    "attached_image_url": "https://storage.example.com/images/flower-002.jpg",
    "size_name": "B",
    "size_width": 15,
    "size_height": 20,
    "size_base_price": 150000,
    "size_max_words": 150,
    "word_count": 45,
    "word_config_snapshot": {
      "min_words": 36,
      "max_words": 70,
      "extra_price": 39000
    },
    "base_price": 150000,
    "extra_price": 39000,
    "total_price": 189000
  }
}
```

**Ví dụ 3 — Thiệp nguồn không tồn tại** — HTTP `404`

Request:
```
POST /api/ai-cards/00000000-0000-0000-0000-000000000000/regenerate
```

Response:
```json
{
  "error": {
    "code": "CARD_NOT_FOUND",
    "message": "Thiệp không tồn tại."
  }
}
```

**Ví dụ 4 — Thiệp nguồn không thuộc khách hàng** — HTTP `403`

Request:
```
POST /api/ai-cards/550e8400-e29b-41d4-a716-446655440003/regenerate
```

Response:
```json
{
  "error": {
    "code": "ACCESS_DENIED",
    "message": "Bạn không có quyền truy cập thiệp này."
  }
}
```

**Ví dụ 5 — Hết quota tạo thiệp trong ngày** — HTTP `403`

Request:
```
POST /api/ai-cards/550e8400-e29b-41d4-a716-446655440001/regenerate
```

Response:
```json
{
  "error": {
    "code": "QUOTA_EXCEEDED",
    "message": "Bạn đã sử dụng hết 10 lượt tạo thiệp AI trong ngày."
  }
}
```

**Ví dụ 6 — Hết quota theo mẫu hoa** — HTTP `403`

Request:
```
POST /api/ai-cards/550e8400-e29b-41d4-a716-446655440001/regenerate
```

Response:
```json
{
  "error": {
    "code": "FLOWER_QUOTA_EXCEEDED",
    "message": "Bạn đã sử dụng hết số lượt tạo thiệp cho mẫu hoa này."
  }
}
```

**Ví dụ 7 — AI thất bại sau retry** — HTTP `500`

Response:
```json
{
  "error": {
    "code": "AI_GENERATION_FAILED",
    "message": "Không thể tạo ảnh thiệp. Vui lòng thử lại sau."
  }
}
```

#### Mã lỗi
| Code | HTTP | Khi nào xảy ra |
|---|---|---|
| CARD_NOT_FOUND | 404 | source_card_id không tồn tại |
| ACCESS_DENIED | 403 | Thiệp nguồn không thuộc khách hàng hiện tại |
| QUOTA_EXCEEDED | 403 | Khách hàng đã sử dụng hết 10 lượt tạo thiệp trong ngày |
| FLOWER_QUOTA_EXCEEDED | 403 | Đã tạo tối đa 3 lần cho mẫu hoa này trong ngày |
| AI_GENERATION_FAILED | 500 | AI không tạo được ảnh sau 2 lần retry |

---

## BƯỚC 4 — Tham chiếu

> Chú thích: 🔴 Tham chiếu đến (tài liệu này đọc/phụ thuộc) · ⚫ Trỏ vào tài liệu này (tài liệu khác phụ thuộc vào tài liệu này) · ⋯ Bị ảnh hưởng (thay đổi ở đây có thể làm tài liệu kia sai theo)

### 🔴 Tham chiếu đến
- TDD-035 - Tạo thiệp thiết kế AI (sử dụng chung generated_cards, quota logic, bảng Config)
- AIModule - Module xử lý AI generation (tách riêng)

### ⚫ Trỏ vào tài liệu này
- ST-036-01-01 - System Test: Tạo lại thiệp thành công từ History
- ST-036-02-01 - System Test: Giữ nguyên nội dung và ảnh đính kèm khi Tạo lại
- ST-036-05-01 - System Test: Chặn Tạo lại khi hết quota ngày hoặc đủ 3 lượt theo mẫu hoa

### ⋯ Bị ảnh hưởng
- TDD-035 - Nếu thay đổi cấu trúc generated_cards hoặc quota logic, cần kiểm tra lại TDD-036
