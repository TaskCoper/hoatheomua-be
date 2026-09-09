# TDD-006: Tạo thiệp thiết kế AI

## Thông tin tài liệu
- **Tiêu đề**: Tạo thiệp thiết kế AI cho khách hàng
- **Ghi chú**: API cho phép khách hàng tạo thiệp thiết kế mới với thông tin cá nhân hóa (người gửi, người nhận, lời chúc) và hình thức (gõ máy/calligraphy). Tính toán giá dựa trên cấu hình size và phụ phí calligraphy từ bảng Config. **Mối quan hệ với hoa được xác định qua `client_histories`, không qua column `generated_cards.source_flower_id`**.

## Metadata quản trị tài liệu
| Trường | Giá trị |
|---|---|
| Mã tài liệu | TDD-006 |
| Phiên bản | v0.9 |
| Author | Nguyễn Tùng Dương |
| Reviewer | Tân Trần |
| Approver | Chưa chỉ định |
| Owner | Nguyễn Tùng Dương |
| Cập nhật gần nhất | 2026-09-07 |

---

## BƯỚC 1 — Thông tin & Bối cảnh

### Thông tin tài liệu
- **Mã tài liệu**: TDD-006
- **Tính năng**: Tạo thiệp thiết kế AI
- **Tác giả**: Nguyễn Tùng Dương
- **Người review**: Tân Trần
- **Phiên bản**: v0.9
- **Cập nhật (YYYY-MM-DD)**: 2026-09-07
- **Story liên quan**:
  - STORY-035

### Business Rules
- BR-006-01: `sizeKey` bắt buộc nhận `size_A|size_B|size_C`. Size Config được lấy chính xác theo `(group="card_size", key=sizeKey)`; phải tồn tại, `isDeleted=false`, `isPublic=true` và không ambiguous, chứa `name`, `width`, `height`, `basePrice`, `maxWords`.
- BR-006-02: Calligraphy Config được lấy chính xác theo `(group="card_config", key="calligraphy_words")`; phải tồn tại, `isDeleted=false`, `isPublic=true` và không ambiguous, chứa mảng JSON rule `minWords`, `maxWords`, `extraPrice`.
- BR-006-03: Chỉ Calligraphy mới có phụ phí theo số từ, Gõ máy không phụ phí
- BR-006-04: Tìm rule phù hợp trong calligraphy_words config bằng điều kiện `minWords <= wordCount <= maxWords`; nếu wordCount vẫn hợp lệ theo Size Config nhưng không khớp bất kỳ rule nào thì trả `VALIDATION_ERROR/400`, không mặc định phụ phí bằng 0
- BR-006-05: Tổng giá = Base Price + Extra Price (nếu có)
- BR-006-06: Khi tạo thiệp, lưu snapshot tất cả thông tin ảnh hưởng đến giá: size config, word config snapshot
- BR-006-07: Sau dependency validation, mọi request Card AI reserve atomically cả quota ngày (tối đa 10 lượt/user/ngày) và quota theo nguồn (tối đa 3 lượt/user/`baseId`/ngày) trước snapshot/AI. Card từ Product dùng `baseId=productId`; Card từ Generated Flower dùng `baseId=generatedFlowerId`.
- BR-006-08: Khi AI, upload và persistence thành công, consume đúng cả hai quota slot và tạo client_histories.
- BR-006-09: Khi AI thất bại sau một lần đầu + tối đa hai retry, upload thất bại hoặc persistence rollback, release cả quota ngày và quota theo `baseId`. Nếu không reserve được một trong hai slot thì không giữ slot còn lại và không gọi AI.
- BR-006-10: client_histories chỉ được tạo khi có ảnh hợp lệ (thành công)
- **BR-006-12: baseId trong client_histories được xác định như sau:**
  - Nếu có `generatedFlowerId` (thiệp cho bó hoa AI): baseId = generatedFlowerId
  - Ngược lại (thiệp cho bó hoa bình thường): baseId = product.id (combo không có biến thể HOẶC biến thể là con của biến thể cha)
- **BR-006-13: outputId trong client_histories = generated_cards.id**
- **BR-006-14: metadata trong client_histories = snapshot/provenance đầy đủ của Card, bao gồm cardTemplate**
- **BR-006-15: type trong client_histories = "card"**
- **BR-006-16**: Endpoint dùng thứ tự `auth/request → load Template để phân nhánh`. Với `templateType="ai"`, thứ tự tiếp theo là `source Product hoặc Generated Flower → xác định baseId quota → validate Template AI → Size Config → Calligraphy Config → system prompt → reserve atomically daily + source quota → CardGenerationSnapshot → AI`; chỉ validate dependency một lần tại admission.
- **BR-006-17**: Product source validate theo thứ tự `PRODUCT_NOT_FOUND/404` → `PRODUCT_DELETED/410` → `PRODUCT_INACTIVE/409` → `PRODUCT_OUT_OF_STOCK/409`; `no_formula|no_core` → `PRODUCT_NOT_SELLABLE/422`; calculation error → `PRODUCT_AVAILABILITY_UNAVAILABLE/503`.
- **BR-006-18**: Nếu Product cha có biến thể, request bắt buộc truyền ID biến thể được chọn; Card Create tính quantity đúng biến thể, không dùng max của Product cha. Parent active chỉ được kiểm tra ở Order theo policy đã chốt.
- **BR-006-19**: Generated Flower source chỉ validate tồn tại + ownership (khác owner trả 404) + dữ liệu ảnh. Không revalidate Product/Mockup gốc. Thiếu ảnh → `GENERATED_FLOWER_INPUT_INVALID/409`.
- **BR-006-20**: Generated Flower legacy thiếu record `client_histories(type=flower)` vẫn được tạo Card khi thuộc user và đủ ảnh; Card metadata ghi `originProductId=null`, `provenanceStatus=unavailable`; Order chặn `SOURCE_PRODUCT_PROVENANCE_INVALID/409`.
- **BR-006-21**: Endpoint load Template theo ID trước; không có record trả `CARD_TEMPLATE_NOT_FOUND/404`. `templateType="ai"` tiếp tục TDD-006 và validate `CARD_TEMPLATE_DELETED/410` → `CARD_TEMPLATE_INACTIVE/409`; `templateType="handmade"` chuyển sang TDD-021 trong cùng endpoint.
- **BR-006-22**: Size Config validate `CARD_SIZE_CONFIG_NOT_FOUND/404` → `CARD_SIZE_CONFIG_DELETED/410` → `CARD_SIZE_CONFIG_INACTIVE/409`; Calligraphy validate độc lập bằng `CALLIGRAPHY_CONFIG_NOT_FOUND/404`, `..._DELETED/410`, `..._INACTIVE/409`.
- **BR-006-23**: Nhiều Config active/chưa xóa cùng `(group,key)` → `CARD_CONFIG_AMBIGUOUS/409`, không chọn ngẫu nhiên.
- **BR-006-24**: Tạo immutable `CardGenerationSnapshot` gồm source, template/config, bytes/version/hash ảnh, full prompt, `validatedAt` UTC và provenance trước AI. AI/retry dùng cùng snapshot; không query lại dependency.
- **BR-006-25**: Dependency đổi trạng thái sau snapshot không hủy request hoặc persist. Admission fail thì không AI, record/history hay consume quota.
- **BR-006-26**: Record `client_histories(type=card).metadata` có `schemaVersion=2`, `sourceType`, `originProductId`, `generatedFlowerId`, `provenanceStatus`, template/config/prompt snapshot.
- **BR-006-27**: Mỗi `system_prompts.type` chỉ có đúng một prompt hiện hành. Card Create dùng prompt type `card`; record thiếu hoặc content rỗng/không hợp lệ trả `SYSTEM_PROMPT_INVALID/409` vì Create không có prompt snapshot nguồn để fallback.
- **BR-006-28**: Card AI thành công lưu `generated_cards.card_type="ai"` và `client_histories.type="card"`. `client_histories.type` là string, không phải DB enum.
- **BR-006-29**: Card AI thành công phải lưu cặp ảnh từ AI Module: `rawImage` là ảnh trước chèn chữ và `imageUrl` là ảnh hoàn chỉnh. Thiếu hoặc upload thất bại một ảnh thì attempt thất bại, không tạo Card/History và release quota theo BR-006-09. `rawImage` nullable chỉ để tương thích dữ liệu legacy.

### Bối cảnh & Mục tiêu

**Vấn đề**
> Khách hàng cần tạo thiệp thiết kế AI với thông tin cá nhân hóa (tên người gửi, người nhận, lời chúc) và chọn hình thức (gõ máy/calligraphy). Hệ thống cần lấy cấu hình giá từ bảng Config có sẵn (`group="card_size"` hoặc `group="card_config"`, `kind="Setting"`), tính giá chính xác và lưu snapshot tại thời điểm tạo.

**Mục tiêu**
- Tạo thiệp mới với đầy đủ thông tin
- Lấy cấu hình size và calligraphy từ bảng Config
- Tính toán và lưu giá (basePrice, extraPrice, totalPrice) vào DB
- Lưu snapshot: sizeName, sizeWidth, sizeHeight, sizeBasePrice, sizeMaxWords, wordConfigSnapshot
- Kiểm tra và quản lý đồng thời quota tạo thiệp (10 lượt/user/ngày) và quota theo nguồn (3 lượt/user/`baseId`/ngày)
- Tạo client_histories khi có ảnh hợp lệ
- Reserve atomically cả quota ngày và quota theo `baseId`; success consume cả hai, AI/upload/persistence fail release cả hai

**Ngoài phạm vi** (Out of scope)
- AI generation thực tế (được tách trong module riêng)
- Retry logic khi AI thất bại (được xử lý trong AI module)
- Tạo Order tự động sau khi xác nhận thiệp
- Bảng trung gian Card ↔ Order

---

## BƯỚC 2 — Kiến trúc & Sơ đồ

### Kiến trúc tổng quan (Architecture)
**Tiêu đề**: Trình tự tạo thiệp thiết kế AI
**Mô tả**: Client gọi API → validate source/Template/Config/prompt live → reserve atomically quota ngày + quota theo `baseId` → tính giá và tạo immutable CardGenerationSnapshot → AI/retry dùng snapshot → transaction lưu Card/history → consume cả hai quota; lỗi AI/upload/persistence thì release cả hai slot.

```mermaid
flowchart LR
    Client["Khách hàng"]
    FE["Frontend"]
    API["AICardController"]
    Svc["AICardService"]
    AI["AIModule<br/>(Generate ảnh)"]
    EF["EF Core"]
    DB[("Database<br/>Config<br/>generated_cards")]

    Client --> FE
    FE -->|"POST /api/ai-cards"| API
    API --> Svc
    Svc -->|"Validate source, Template,<br/>Size/Calligraphy, prompt"| DB
    Svc -->|"Reserve atomically<br/>daily + source quota"| DB
    Svc -->|"Calculate price +<br/>CardGenerationSnapshot"| Svc
    Svc -->|"AI/retry cùng snapshot"| AI
    AI -->|"Ảnh thiệp"| Svc
    Svc --> EF
    EF --> DB
    DB --> EF
    EF --> Svc
    Svc --> API
    API --> FE
    FE --> Client
```

### Sequence Diagram
**Tiêu đề**: Trình tự tạo thiệp thiết kế AI
**Mô tả**: Từng bước gọi qua lại giữa các thành phần

```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant FE as Frontend
    participant C as AICardController
    participant S as AICardService
    participant Config as CardConfigService
    participant AI as AIModule
    participant DB as Database

    Client->>FE: Nhập thông tin thiệp
    FE->>C: POST /api/ai-cards {request}

    Note over S: GIAI ĐOẠN 1: Admission validation một lần
    S->>DB: Load Card Template để phân nhánh theo templateType
    alt templateType = handmade
        S->>S: Chuyển sang flow TDD-021, không tiếp tục flow AI
    end
    S->>DB: Validate source Product hoặc Generated Flower
    S->>DB: Validate Card Template AI live
    S->>Config: Validate/GetSizeConfig(group,key)
    Config-->>S: { name, width, height, basePrice, maxWords }
    opt formType = calligraphy
        S->>Config: Validate/GetCalligraphyConfig()
        Config-->>S: [ { minWords, maxWords, extraPrice }, ... ]
    end
    S->>DB: Load full system prompt hiện hành
    S->>S: CountWords + calculate price từ config đã validate

    Note over S: GIAI ĐOẠN 2: Reserve quota và snapshot
    S->>DB: Reserve atomically daily slot (10/user/ngày) + source slot (3/user/baseId/ngày)
    alt Hết quota
        S-->>C: Trả lỗi quota exceeded
        C-->>FE: HTTP 403
        FE-->>Client: Thông báo hết quota
    end
    S->>S: Create immutable CardGenerationSnapshot gồm source/template/config/price/prompt

    Note over S: GIAI ĐOẠN 3: AI/retry không revalidate nguồn
    S->>AI: Generate(snapshot), tối đa 3 lần gọi
    AI-->>S: Ảnh thiệp (thành công/thất bại)

    alt AI thất bại (sau retry)
        S->>DB: Release cả daily slot và source slot
        S-->>C: Trả lỗi AI failed
    else AI trả ảnh hợp lệ
        Note over S: GIAI ĐOẠN 4: Persist từ snapshot bất biến
        S->>DB: Transaction lưu generated_cards + client_histories
        alt Persistence thất bại
            S->>DB: Rollback transaction và release cả daily/source slot
            S-->>C: Trả lỗi persistence
        else Persistence thành công
            S->>DB: Consume cả daily slot và source slot
            S-->>C: GeneratedCard result
            C-->>FE: HTTP 200 {card}
            FE-->>Client: Hiển thị thiệp vừa tạo
        end
    end
```

### Activity Diagram

**Tiêu đề**: Ranh giới validation và snapshot khi tạo Card
**Mô tả**: Sơ đồ tách rõ các nhánh admission failure khỏi request đã được chấp nhận. Mọi lần gọi AI và retry sau snapshot đều không truy vấn lại trạng thái live của dependency.

```mermaid
flowchart TD
    A[Bắt đầu request] --> B{Auth và request hợp lệ?}
    B -->|Không| X[Trả lỗi, không gọi AI, không consume quota]
    B -->|Có| C{Source hợp lệ tại admission?}
    C -->|Không| X
    C -->|Có| D{Template và Config hợp lệ?}
    D -->|Không| X
    D -->|Có| E{System prompt và quota slot sẵn sàng?}
    E -->|Không| X
    E -->|Có| F[Tạo immutable CardGenerationSnapshot]
    F --> G[Gọi AI bằng snapshot]
    G -->|Thất bại và còn retry| G
    G -->|Thất bại hết retry| H[Release cả daily/source quota slot]
    G -->|Thành công| I[Persist generated_cards và client_histories]
    I -->|Persistence thất bại| H
    I -->|Persistence thành công| J[Consume cả daily/source quota slot và trả success]
```

### State Diagram

- **Không áp dụng**: TDD này mô tả vòng đời của một request đồng bộ và kết quả bất biến, không định nghĩa state machine cho entity chính.

### Mô hình dữ liệu (Data Model / ERD)
**Tiêu đề**: Các bảng liên quan đến tạo thiệp
**Mô tả**: Config (bảng có sẵn), generated_cards, client_histories (polymorphic)

**Quy ước**: ERD bên dưới dùng đúng tên table/column PostgreSQL dạng `snake_case`; field API và JSON snapshot trong các phần còn lại dùng `camelCase`.

```mermaid
erDiagram
    Config ||--o{ generated_cards : "size_snapshot"
    products ||--o{ client_histories : "base_normal_card"
    generated_flowers ||--o{ client_histories : "base_ai_card"
    generated_cards ||--o{ client_histories : "output_card"
    card_templates ||--o{ client_histories : "metadata"
    system_prompts ||--o{ client_histories : "prompt"

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
        string raw_image "AI original before text; nullable only for legacy"
        uuid user_id
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
        string card_type "ai; required for this flow"
        decimal base_price
        decimal extra_price
        decimal total_price
    }

    client_histories {
        uuid id PK
        uuid user_id
        json input
        string type "flower | card | handmade_card | post"
        uuid system_prompt_id "nullable với handmade_card"
        json metadata
        timestamp created_at
        uuid base_id "polymorphic: product.id hoặc generated_flowers.id"
        uuid output_id "polymorphic: generated_flowers.id hoặc generated_cards.id"
    }

    products {
        uuid id PK
    }

    generated_flowers {
        uuid id PK
        string image_url
        uuid user_id
        json input_snapshot
    }

    card_templates {
        uuid id PK
        string name
        string image_url
        string template_type "ai | handmade; required; immutable"
        boolean is_active
        boolean is_deleted
    }

    system_prompts {
        uuid id PK
        string type UK
        text content
    }
```

**LƯU Ý:** `generated_cards` KHÔNG có column `source_flower_id`. Mối quan hệ với bó hoa được xác định qua `client_histories.base_id`:
- Thiệp cho bó hoa bình thường: `baseId = products.id`
- Thiệp cho bó hoa AI: `baseId = generated_flowers.id`

---

## BƯỚC 3 — API

### API Contract nội bộ

#### Endpoint #1: Tạo thiệp thiết kế AI
- **Method**: POST
- **Endpoint**: `/api/ai-cards`
- **Tên endpoint**: Tạo thiệp thiết kế AI
- **Mô tả**: Tạo thiệp thiết kế mới với thông tin cá nhân hóa và tính giá từ config

**Request Body:**
| Field | Type | Required | Mô tả |
|-------|------|---------|-------|
| cardTemplateId | uuid | Có | Template thiệp |
| sizeKey | string | Có | Config key: `size_A`, `size_B` hoặc `size_C` |
| formType | string | Có | Hình thức: "go_may", "calligraphy" |
| senderName | string | Có | Người gửi (tối đa 20 từ) |
| receiverName | string | Có | Người nhận (tối đa 20 từ) |
| messageContent | string | Có | Lời chúc (tối đa 100 từ) |
| attachedImageUrl | string? | Không | Ảnh đính kèm |
| productId | uuid | Không | Combo nguồn (khi thiệp cho bó hoa bình thường) |
| generatedFlowerId | uuid | Không | Bó hoa AI nguồn (khi thiệp cho bó hoa AI) |

**Ghi chú:** Phải cung cấp `productId` HOẶC `generatedFlowerId`, không được cả hai cùng lúc.

**Ví dụ 1 — Happy path: Tạo thiệp Gõ máy cho bó hoa bình thường** — HTTP `200`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "550e8400-e29b-41d4-a716-446655440001",
  "sizeKey": "size_A",
  "formType": "go_may",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "attachedImageUrl": null,
  "productId": "440e8400-e29b-41d4-a716-446655440010"
}
```

Response:
```json
{
  "value": {
    "id": "660e8400-e29b-41d4-a716-446655440002",
    "content": "...",
    "imageUrl": "https://storage.example.com/cards/660e8400.png",
    "rawImage": "https://storage.example.com/cards/660e8400-raw.png",
    "userId": "770e8400-e29b-41d4-a716-446655440003",
    "createdAt": "2026-08-27T10:30:00Z",
    "formType": "go_may",
    "sizeKey": "size_A",
    "senderName": "Nguyễn An",
    "receiverName": "Trần Bình",
    "messageContent": "Chúc bạn sinh nhật vui vẻ",
    "attachedImageUrl": null,
    "sizeName": "A",
    "sizeWidth": 10,
    "sizeHeight": 15,
    "sizeBasePrice": 100000,
    "sizeMaxWords": 100,
    "wordCount": 5,
    "wordConfigSnapshot": null,
    "basePrice": 100000,
    "extraPrice": 0,
    "totalPrice": 100000
  }
}
```

**Ví dụ 2 — Happy path: Tạo thiệp Calligraphy cho bó hoa AI** — HTTP `200`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "550e8400-e29b-41d4-a716-446655440001",
  "sizeKey": "size_B",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Nhân dịp sinh nhật bạn, tôi xin gửi đến bạn những lời chúc tốt đẹp nhất. Chúc bạn luôn hạnh phúc, khỏe mạnh và thành công trong cuộc sống.",
  "attachedImageUrl": "https://storage.example.com/images/flower-001.jpg",
  "generatedFlowerId": "880e8400-e29b-41d4-a716-446655440005"
}
```

**Ví dụ 3 — Happy path: Tạo thiệp Calligraphy (36-70 từ)** — HTTP `200`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "550e8400-e29b-41d4-a716-446655440001",
  "sizeKey": "size_B",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Nhân dịp sinh nhật bạn, tôi xin gửi đến bạn những lời chúc tốt đẹp nhất. Chúc bạn luôn hạnh phúc, khỏe mạnh và thành công trong cuộc sống.",
  "productId": "440e8400-e29b-41d4-a716-446655440010"
}
```

Response:
```json
{
  "value": {
    "id": "660e8400-e29b-41d4-a716-446655440002",
    "imageUrl": "https://storage.example.com/cards/660e8400.png",
    "rawImage": "https://storage.example.com/cards/660e8400-raw.png",
    "formType": "calligraphy",
    "sizeKey": "size_B",
    "sizeName": "B",
    "sizeWidth": 15,
    "sizeHeight": 20,
    "sizeBasePrice": 150000,
    "sizeMaxWords": 150,
    "wordCount": 42,
    "wordConfigSnapshot": {
      "minWords": 36,
      "maxWords": 70,
      "extraPrice": 39000
    },
    "basePrice": 150000,
    "extraPrice": 39000,
    "totalPrice": 189000
  }
}
```

**Ví dụ 4 — Hết quota tạo thiệp trong ngày** — HTTP `403`

Response:
```json
{
  "error": {
    "code": "FORBIDDEN",
    "message": "Bạn đã sử dụng hết 10 lượt tạo thiệp AI trong ngày."
  }
}
```

**Ví dụ 4A — Hết quota tạo thiệp theo nguồn** — HTTP `403`

Quota này áp dụng giống nhau khi `baseId` là Product hoặc Generated Flower.

Response:
```json
{
  "error": {
    "code": "FORBIDDEN",
    "message": "Bạn đã sử dụng hết 3 lượt tạo thiệp AI cho nguồn này trong ngày."
  }
}
```

**Ví dụ 5 — AI thất bại sau retry** — HTTP `500`

Response:
```json
{
  "error": {
    "code": "INTERNAL_SERVER_ERROR",
    "message": "Không thể tạo ảnh thiệp. Vui lòng thử lại sau."
  }
}
```

**Ví dụ 6 — Chưa đăng nhập** — HTTP `401`

Response:
```json
{
  "error": {
    "code": "UNAUTHORIZED",
    "message": "Bạn cần đăng nhập để thực hiện thao tác này."
  }
}
```

**Ví dụ 7 — Card Template không tồn tại** — HTTP `404`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "00000000-0000-0000-0000-000000000000",
  "sizeKey": "size_A",
  "formType": "go_may",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "440e8400-e29b-41d4-a716-446655440010"
}
```

Response:
```json
{
  "error": {
    "code": "CARD_TEMPLATE_NOT_FOUND",
    "message": "Card template không tồn tại."
  }
}
```

**Ví dụ 8 — Size Config không tồn tại** — HTTP `404`

Dữ liệu live tại admission không có record Config cho exact `group="card_size"`, `key="size_A"`, `kind="Setting"`. Giá trị `sizeKey="size_A"` vẫn hợp lệ ở request validation nên request đi tới đúng nhánh lookup Size Config.

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "550e8400-e29b-41d4-a716-446655440001",
  "sizeKey": "size_A",
  "formType": "go_may",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "440e8400-e29b-41d4-a716-446655440010"
}
```

Response:
```json
{
  "error": {
    "code": "CARD_SIZE_CONFIG_NOT_FOUND",
    "message": "Size config không tồn tại."
  }
}
```

**Ví dụ 9 — Không truyền productId và generatedFlowerId** — HTTP `400`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "550e8400-e29b-41d4-a716-446655440001",
  "sizeKey": "size_A",
  "formType": "go_may",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ"
}
```

Response:
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Phải cung cấp productId hoặc generatedFlowerId."
  }
}
```

**Ví dụ 10 — Cả productId và generatedFlowerId đều truyền** — HTTP `400`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "550e8400-e29b-41d4-a716-446655440001",
  "sizeKey": "size_A",
  "formType": "go_may",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "440e8400-e29b-41d4-a716-446655440010",
  "generatedFlowerId": "880e8400-e29b-41d4-a716-446655440005"
}
```

Response:
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Chỉ được cung cấp productId hoặc generatedFlowerId, không được cả hai."
  }
}
```

**Ví dụ 11 — `sizeKey` không hợp lệ** — HTTP `400`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "550e8400-e29b-41d4-a716-446655440001",
  "sizeKey": "size_X",
  "formType": "go_may",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "440e8400-e29b-41d4-a716-446655440010"
}
```

Response:
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "sizeKey không hợp lệ. Vui lòng chọn size_A, size_B hoặc size_C."
  }
}
```

**Ví dụ 12 — `formType` không hợp lệ** — HTTP `400`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "550e8400-e29b-41d4-a716-446655440001",
  "sizeKey": "size_A",
  "formType": "invalid",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "440e8400-e29b-41d4-a716-446655440010"
}
```

Response:
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "formType không hợp lệ. Vui lòng chọn go_may hoặc calligraphy."
  }
}
```

**Ví dụ 13 — Message vượt maxWords** — HTTP `400`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "550e8400-e29b-41d4-a716-446655440001",
  "sizeKey": "size_A",
  "formType": "go_may",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ với rất nhiều lời chúc tốt đẹp và ý nghĩa cho một ngày đặc biệt trong năm mà chúng ta cùng nhau đón mừng và chia sẻ niềm vui với gia đình và bạn bè thân yêu của mình trong suốt quãng đời dài phía trước",
  "productId": "440e8400-e29b-41d4-a716-446655440010"
}
```

Response:
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Số từ trong lời chúc vượt quá giới hạn cho phép."
  }
}
```

**Ví dụ 14 — Sender name quá dài** — HTTP `400`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "550e8400-e29b-41d4-a716-446655440001",
  "sizeKey": "size_A",
  "formType": "go_may",
  "senderName": "Nguyễn Văn A B C D E F G H I J K L M N O P Q R S T U V W X Y Z",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "440e8400-e29b-41d4-a716-446655440010"
}
```

Response:
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Tên người gửi không được vượt quá 20 từ."
  }
}
```

+**Ví dụ 15 — Product không tồn tại** — HTTP `404`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "product-variant-a"
}
```

Response:
```json
{
  "error": {
    "code": "PRODUCT_NOT_FOUND",
    "message": "Không tìm thấy sản phẩm nguồn."
  }
}
```

**Ví dụ 16 — Product đã soft-delete** — HTTP `410`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "product-variant-a"
}
```

Response:
```json
{
  "error": {
    "code": "PRODUCT_DELETED",
    "message": "Sản phẩm nguồn đã bị ngừng cung cấp."
  }
}
```

**Ví dụ 17 — Product inactive** — HTTP `409`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "product-variant-a"
}
```

Response:
```json
{
  "error": {
    "code": "PRODUCT_INACTIVE",
    "message": "Sản phẩm nguồn hiện đang tạm ngưng bán."
  }
}
```

**Ví dụ 18 — Product hết hàng** — HTTP `409`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "product-variant-a"
}
```

Response:
```json
{
  "error": {
    "code": "PRODUCT_OUT_OF_STOCK",
    "message": "Sản phẩm nguồn hiện đã hết hàng."
  }
}
```

**Ví dụ 19 — Product không có formula usable** — HTTP `422`

Dữ liệu live tại admission: `CalculateAvailableQty(product-variant-a)` trả `UnavailableReason="no_formula"`.

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "product-variant-a"
}
```

Response:
```json
{
  "error": {
    "code": "PRODUCT_NOT_SELLABLE",
    "message": "Sản phẩm hiện chưa đủ điều kiện để bán."
  }
}
```

**Ví dụ 19A — Product không có CORE usable** — HTTP `422`

Dữ liệu live tại admission: `CalculateAvailableQty(product-variant-a)` trả `UnavailableReason="no_core"`.

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "product-variant-a"
}
```

Response:
```json
{
  "error": {
    "code": "PRODUCT_NOT_SELLABLE",
    "message": "Sản phẩm hiện chưa đủ điều kiện để bán vì không có CORE usable."
  }
}
```

**Ví dụ 20 — Không tính được tồn kho Product** — HTTP `503`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "product-variant-a"
}
```

Response:
```json
{
  "error": {
    "code": "PRODUCT_AVAILABILITY_UNAVAILABLE",
    "message": "Chưa thể xác minh tồn kho sản phẩm. Vui lòng thử lại sau."
  }
}
```

**Ví dụ 21 — Generated Flower không tồn tại** — HTTP `404`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "generatedFlowerId": "flower-missing"
}
```

Response:
```json
{
  "error": {
    "code": "GENERATED_FLOWER_NOT_FOUND",
    "message": "Không tìm thấy mẫu hoa AI thuộc quyền sử dụng của bạn."
  }
}
```

**Ví dụ 21A — Generated Flower thuộc user khác** — HTTP `404`

Dữ liệu live tại admission: `flower-other-user.userId="user-002"`, trong khi user hiện tại là `user-001`.

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "generatedFlowerId": "flower-other-user"
}
```

Response:
```json
{
  "error": {
    "code": "GENERATED_FLOWER_NOT_FOUND",
    "message": "Không tìm thấy mẫu hoa AI thuộc quyền sử dụng của bạn."
  }
}
```

**Ví dụ 22 — Generated Flower thiếu dữ liệu ảnh** — HTTP `409`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "generatedFlowerId": "flower-without-image"
}
```

Response:
```json
{
  "error": {
    "code": "GENERATED_FLOWER_INPUT_INVALID",
    "message": "Mẫu hoa AI không có dữ liệu ảnh hợp lệ để tạo thiệp."
  }
}
```

**Ví dụ 23 — Card Template đã soft-delete** — HTTP `410`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "product-variant-a"
}
```

Response:
```json
{
  "error": {
    "code": "CARD_TEMPLATE_DELETED",
    "message": "Mẫu thiệp đã bị ngừng cung cấp."
  }
}
```

**Ví dụ 24 — Card Template inactive** — HTTP `409`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "product-variant-a"
}
```

Response:
```json
{
  "error": {
    "code": "CARD_TEMPLATE_INACTIVE",
    "message": "Mẫu thiệp hiện đang tạm ngưng sử dụng."
  }
}
```

**Ví dụ 25 — Size Config đã soft-delete** — HTTP `410`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "product-variant-a"
}
```

Response:
```json
{
  "error": {
    "code": "CARD_SIZE_CONFIG_DELETED",
    "message": "Cấu hình kích thước đã bị xóa."
  }
}
```

**Ví dụ 26 — Size Config inactive** — HTTP `409`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "product-variant-a"
}
```

Response:
```json
{
  "error": {
    "code": "CARD_SIZE_CONFIG_INACTIVE",
    "message": "Cấu hình kích thước hiện đang bị ngưng."
  }
}
```

**Ví dụ 27 — Calligraphy Config không tồn tại** — HTTP `404`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "product-variant-a"
}
```

Response:
```json
{
  "error": {
    "code": "CALLIGRAPHY_CONFIG_NOT_FOUND",
    "message": "Không tìm thấy cấu hình Calligraphy."
  }
}
```

**Ví dụ 28 — Calligraphy Config đã soft-delete** — HTTP `410`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "product-variant-a"
}
```

Response:
```json
{
  "error": {
    "code": "CALLIGRAPHY_CONFIG_DELETED",
    "message": "Cấu hình Calligraphy đã bị xóa."
  }
}
```

**Ví dụ 29 — Calligraphy Config inactive** — HTTP `409`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "product-variant-a"
}
```

Response:
```json
{
  "error": {
    "code": "CALLIGRAPHY_CONFIG_INACTIVE",
    "message": "Cấu hình Calligraphy hiện đang bị ngưng."
  }
}
```

**Ví dụ 30 — Config active trùng exact group và key** — HTTP `409`

Request:
```json
POST /api/ai-cards
{
  "cardTemplateId": "template-001",
  "sizeKey": "size_A",
  "formType": "calligraphy",
  "senderName": "Nguyễn An",
  "receiverName": "Trần Bình",
  "messageContent": "Chúc bạn sinh nhật vui vẻ",
  "productId": "product-variant-a"
}
```

Response:
```json
{
  "error": {
    "code": "CARD_CONFIG_AMBIGUOUS",
    "message": "Có nhiều cấu hình đang hoạt động cho cùng group và key."
  }
}
```

#### Mã lỗi
| Code | HTTP | Khi nào xảy ra |
|---|---|---|
| VALIDATION_ERROR | 400 | Request thiếu hoặc sai field bắt buộc, sizeKey/formType không hợp lệ, message vượt maxWords, tên người gửi quá dài hoặc Calligraphy wordCount không khớp rule giá nào dù vẫn hợp lệ theo Size Config |
| PRODUCT_NOT_FOUND | 404 | Không có Product nguồn |
| PRODUCT_DELETED | 410 | Product đã soft-delete |
| PRODUCT_INACTIVE | 409 | Product inactive |
| PRODUCT_OUT_OF_STOCK | 409 | Đúng Product/variant đã hết hàng |
| PRODUCT_NOT_SELLABLE | 422 | Combo thiếu formula/CORE usable |
| PRODUCT_AVAILABILITY_UNAVAILABLE | 503 | Không tính được tồn kho |
| GENERATED_FLOWER_NOT_FOUND | 404 | Flower không tồn tại hoặc không thuộc user |
| GENERATED_FLOWER_INPUT_INVALID | 409 | Flower thiếu dữ liệu ảnh dùng cho AI |
| CARD_TEMPLATE_NOT_FOUND | 404 | Không có Card Template |
| CARD_TEMPLATE_DELETED | 410 | Card Template đã soft-delete |
| CARD_TEMPLATE_INACTIVE | 409 | Card Template inactive |
| CARD_SIZE_CONFIG_NOT_FOUND | 404 | Không có Size Config theo exact group/key |
| CARD_SIZE_CONFIG_DELETED | 410 | Size Config đã xóa |
| CARD_SIZE_CONFIG_INACTIVE | 409 | Size Config `isPublic=false` |
| CALLIGRAPHY_CONFIG_NOT_FOUND | 404 | Không có Calligraphy Config |
| CALLIGRAPHY_CONFIG_DELETED | 410 | Calligraphy Config đã xóa |
| CALLIGRAPHY_CONFIG_INACTIVE | 409 | Calligraphy Config `isPublic=false` |
| SYSTEM_PROMPT_INVALID | 409 | Prompt hiện hành type `card` không tồn tại hoặc content rỗng/không hợp lệ |
| CARD_CONFIG_AMBIGUOUS | 409 | Nhiều Config active cùng exact group/key |
| FORBIDDEN | 403 | Bạn đã sử dụng hết 10 lượt tạo thiệp AI trong ngày hoặc hết 3 lượt cho cùng `baseId` nguồn trong ngày |
| UNAUTHORIZED | 401 | Bạn cần đăng nhập để thực hiện thao tác này |
| INTERNAL_SERVER_ERROR | 500 | Không thể tạo thiệp thiết kế AI. Vui lòng thử lại sau |

### API Contract bên ngoài

- **Endpoints sử dụng**: Không áp dụng ở mức TDD này. AI Module là module nội bộ; nhà cung cấp AI và endpoint bên thứ ba chưa được xác định trong codebase.
- **Field quan trọng**: AI Module chỉ nhận immutable generation snapshot đã được tạo sau khi toàn bộ dependency vượt qua validation tại request admission.
- **Xử lý lỗi từ đối tác**: Một lần gọi đầu và tối đa hai lần retry dùng cùng snapshot và cùng hai reservation quota; hết retry thì release cả quota ngày và quota theo `baseId`, không tạo kết quả/history.
- **Quirks / cạm bẫy**: Không được tải lại ảnh, prompt hoặc query lại trạng thái dependency live trong retry.

---

## BƯỚC 4 — Tham chiếu

> Chú thích: 🔴 Tham chiếu đến (tài liệu này đọc/phụ thuộc) · ⚫ Trỏ vào tài liệu này (tài liệu khác phụ thuộc vào tài liệu này) · ⋯ Bị ảnh hưởng (thay đổi ở đây có thể làm tài liệu kia sai theo)

### 🔴 Tham chiếu đến
- AIModule - Module xử lý AI generation (tách riêng)
- Config Entity - Bảng Config có sẵn cho card_size và card_config
- Product Entity - Lấy thông tin combo nguồn
- generated_flowers Entity - Khi thiệp cho bó hoa AI
- TDD-022 và TDD-023 - Lấy danh sách/chi tiết Template AI
- TDD-024 đến TDD-026 - Quản trị vòng đời Card Template
- TDD-021 - Cùng endpoint nhưng xử lý Template `handmade`, không gọi AI
- system_prompts Entity - Lấy prompt cho AI type="card"

### ⚫ Trỏ vào tài liệu này
- TDD-007 - Tạo lại thiệp từ lịch sử (sử dụng lại generated_cards và quota logic)
- AI_DB_Diagram - Tham chiếu cấu trúc client_histories
- AI_Card_Context - Tham chiếu input cho AI Module

### ⋯ Bị ảnh hưởng
- ST-035-05-01 - System Test: Hiển thị giá tạm tính theo Size và Hình thức
- ST-035-10-01 - System Test: Retry và hoàn lượt khi AI không tạo được ảnh
