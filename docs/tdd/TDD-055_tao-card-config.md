# TDD-055: Tạo Card Config

## Thông tin tài liệu
- **Tiêu đề**: Tạo cấu hình card (size/calligraphy)
- **Ghi chú**: API cho phép Admin tạo cấu hình card (size hoặc calligraphy words) sử dụng bảng Config có sẵn với Kind="Setting".

## Metadata quản trị tài liệu
| Trường | Giá trị |
|---|---|
| Mã tài liệu | TDD-055 |
| Phiên bản | v0.3 |
| Author | |
| Reviewer | |
| Approver | Chưa chỉ định |
| Owner | |
| Cập nhật gần nhất | 2026-08-26 |

---

## BƯỚC 1 — Thông tin & Bối cảnh

### Thông tin tài liệu
- **Mã tài liệu**: TDD-050
- **Tính năng**: Tạo card config
- **Tác giả**: 
- **Người review**: 
- **Phiên bản**: v0.2
- **Cập nhật (YYYY-MM-DD)**: 2026-08-26
- **Story liên quan**: Không có

### Business Rules
- BR-055-01: Chỉ Admin có quyền tạo card config
- BR-055-02: Sử dụng bảng Config có sẵn
- BR-055-03: Kind = "Setting" cho tất cả card configs
- BR-055-04: Group phân loại:
  - "card_size" cho size configs
  - "card_config" cho calligraphy_words config
- BR-055-05: Key được chọn từ danh sách predefined:
  - Size: `size_A`, `size_B`, `size_C`, ...
  - Calligraphy: `calligraphy_words`
- BR-055-06: Không check trùng key
- BR-055-07: Value được format thành JSON khi lưu vào DB
- BR-055-08: Value phải là JSON hợp lệ khi tạo
- BR-055-09: IsPublic = true khi tạo mới (đang hoạt động)
- BR-055-10: IsDeleted = false khi tạo mới

### Bối cảnh & Mục tiêu

**Vấn đề**
> Admin cần tạo cấu hình card (size với giá, kích thước; calligraphy words với phụ phí theo số từ) sử dụng bảng Config có sẵn trong hệ thống.

**Mục tiêu**
- Tạo card config mới với Group, Key, Kind và Value
- Validate value là JSON hợp lệ
- Kind luôn là "Setting"

**Ngoài phạm vi** (Out of scope)
- Cập nhật card config (TDD-052)
- Xóa card config (TDD-053)
- Seed data mặc định

---

## BƯỚC 2 — Kiến trúc & Sơ đồ

### Kiến trúc tổng quan (Architecture)
**Tiêu đề**: Trình tự tạo card config
**Mô tả**: Admin gọi API tạo config → Controller nhận request → Service validate → Lưu vào bảng Config → Trả kết quả

```mermaid
flowchart LR
    Admin["Admin"]
    API["ConfigController"]
    Svc["ConfigService"]
    EF["EF Core"]
    DB[("Database<br/>Config")]

    Admin -->|"POST /api/v1/configs"| API
    API --> Svc
    Svc -->|"Validate JSON"| Svc
    Svc --> EF
    EF --> DB
    Svc --> API
    API --> Admin
```

### Sequence Diagram
**Tiêu đề**: Trình tự tạo card config
**Mô tả**: Từng bước gọi qua lại giữa các thành phần

```mermaid
sequenceDiagram
    autonumber
    actor Admin
    participant C as ConfigController
    participant S as ConfigService
    participant DB as Database

    Admin->>C: POST /api/v1/configs {group, key, kind, value}

    Note over S: Validate request
    S->>S: Validate key not empty
    S->>S: Validate value is valid JSON
    S->>S: Set Kind = "Setting"
    S->>S: Set IsPublic = true
    S->>S: Set IsDeleted = false

    alt Invalid JSON
        S-->>C: Trả lỗi validation
        C-->>Admin: HTTP 400
    end

    Note over S: Tạo record
    S->>DB: Insert Config { group, key, kind, value, is_public, is_deleted }
    DB-->>S: config mới

    S-->>C: HTTP 201 {config}
    C-->>Admin: HTTP 201 Created
```

### Mô hình dữ liệu (Data Model / ERD)
**Tiêu đề**: Bảng Config
**Mô tả**: Cấu trúc bảng Config

```mermaid
erDiagram
    Config ||--o{ generated_cards : "size_config"

    Config {
        uuid id PK
        string key
        jsonb value
        bool is_public
        string group
        string kind
        guid user_id
        timestamp created_at
        timestamp updated_at
        bool is_deleted
    }
```

---

## BƯỚC 3 — API

### API Contract nội bộ

#### Endpoint #1: Tạo card config
- **Method**: POST
- **Endpoint**: `/api/v1/configs`
- **Tên endpoint**: Tạo card config
- **Mô tả**: Tạo cấu hình card mới (size hoặc calligraphy)
- **Quyền**: Chỉ Admin

**Request Body:**
```json
{
  "group": "card_size",
  "key": "size_A",
  "kind": "Setting",
  "value": { ... }
}
```

**Ví dụ 1 — Happy path: Tạo size config** — HTTP `201`

Request:
```json
POST /api/v1/configs
{
  "group": "card_size",
  "key": "size_A",
  "kind": "Setting",
  "value": {
    "name": "A",
    "width": 10,
    "height": 15,
    "base_price": 100000,
    "max_words": 100
  }
}
```

Response:
```json
{
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "key": "size_A",
    "value": {
      "name": "A",
      "width": 10,
      "height": 15,
      "base_price": 100000,
      "max_words": 100
    },
    "group": "card_size",
    "kind": "Setting",
    "is_public": true,
    "is_deleted": false,
    "user_id": "...",
    "created_at": "2026-08-26T10:00:00Z",
    "updated_at": null
  }
}
```

**Ví dụ 2 — Happy path: Tạo calligraphy words config** — HTTP `201`

Request:
```json
POST /api/v1/configs
{
  "group": "card_config",
  "key": "calligraphy_words",
  "kind": "Setting",
  "value": [
    { "min_words": 0, "max_words": 35, "extra_price": 0 },
    { "min_words": 36, "max_words": 70, "extra_price": 39000 },
    { "min_words": 71, "max_words": 100, "extra_price": 69000 }
  ]
}
```

Response:
```json
{
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440002",
    "key": "calligraphy_words",
    "value": [
      { "min_words": 0, "max_words": 35, "extra_price": 0 },
      { "min_words": 36, "max_words": 70, "extra_price": 39000 },
      { "min_words": 71, "max_words": 100, "extra_price": 69000 }
    ],
    "group": "card_config",
    "kind": "Setting",
    "is_public": true,
    "is_deleted": false,
    "user_id": "...",
    "created_at": "2026-08-26T10:00:00Z",
    "updated_at": null
  }
}
```

**Ví dụ 3 — Happy path: Tạo trùng key (cho phép)** — HTTP `201`

Request:
```json
POST /api/v1/configs
{
  "group": "card_size",
  "key": "size_A",
  "kind": "Setting",
  "value": {
    "name": "A",
    "width": 12,
    "height": 18,
    "base_price": 120000,
    "max_words": 120
  }
}
```

**Ví dụ 4 — Validation: Invalid JSON** — HTTP `400`

Request:
```json
POST /api/v1/configs
{
  "group": "card_size",
  "key": "size_A",
  "kind": "Setting",
  "value": "this is not valid json {"
}
```

Response:
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Value must be a valid JSON."
  }
}
```

**Ví dụ 5 — Validation: Missing key** — HTTP `400`

Request:
```json
POST /api/v1/configs
{
  "group": "card_size",
  "kind": "Setting",
  "value": { "name": "A" }
}
```

Response:
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Key is required."
  }
}
```

**Ví dụ 6 — Authorization: Non-admin user** — HTTP `403`

Request:
```json
POST /api/v1/configs
{
  "group": "card_size",
  "key": "size_A",
  "kind": "Setting",
  "value": { ... }
}
```

Response:
```json
{
  "error": {
    "code": "ACCESS_DENIED",
    "message": "Bạn không có quyền thực hiện thao tác này."
  }
}
```

#### Mã lỗi
| Code | HTTP | Khi nào xảy ra |
|---|---|---|
| VALIDATION_ERROR | 400 | Value không phải JSON hợp lệ hoặc thiếu field bắt buộc |
| ACCESS_DENIED | 403 | Người dùng không phải Admin |

---

## BƯỚC 4 — Tham chiếu

> Chú thích: 🔴 Tham chiếu đến (tài liệu này đọc/phụ thuộc) · ⚫ Trỏ vào tài liệu này (tài liệu khác phụ thuộc vào tài liệu này) · ⋯ Bị ảnh hưởng (thay đổi ở đây có thể làm tài liệu kia sai theo)

### 🔴 Tham chiếu đến
- Không có

### ⚫ Trỏ vào tài liệu này
- TDD-051 - Lấy danh sách card configs
- TDD-052 - Cập nhật card config
- TDD-053 - Xóa card config
- TDD-035 - Tạo thiệp thiết kế AI (sử dụng Config cho pricing)
- TDD-036 - Tạo lại thiệp từ lịch sử (sử dụng Config cho pricing)

### ⋯ Bị ảnh hưởng
- TDD-035, TDD-036 - Nếu thay đổi cấu trúc Config, cần kiểm tra lại logic pricing

---

## Seed Data

```json
[
  {
    "group": "card_size",
    "key": "size_A",
    "kind": "Setting",
    "value": { "name": "A", "width": 10, "height": 15, "base_price": 100000, "max_words": 100 }
  },
  {
    "group": "card_size",
    "key": "size_B",
    "kind": "Setting",
    "value": { "name": "B", "width": 15, "height": 20, "base_price": 150000, "max_words": 150 }
  },
  {
    "group": "card_size",
    "key": "size_C",
    "kind": "Setting",
    "value": { "name": "C", "width": 20, "height": 25, "base_price": 200000, "max_words": 200 }
  },
  {
    "group": "card_config",
    "key": "calligraphy_words",
    "kind": "Setting",
    "value": [
      { "min_words": 0, "max_words": 35, "extra_price": 0 },
      { "min_words": 36, "max_words": 70, "extra_price": 39000 },
      { "min_words": 71, "max_words": 100, "extra_price": 69000 }
    ]
  }
]
```

---

## Ghi chú quan trọng

- Sử dụng bảng **Config** có sẵn trong codebase
- **Kind = "Setting"** cho tất cả card configs
- **Group** phân loại: "card_size" hoặc "card_config"
- **IsPublic = true** khi tạo mới (đang hoạt động để người dùng chọn)
- **IsDeleted = false** khi tạo mới (chưa xóa mềm)
- **IsPublic**: dùng để xác định config có đang hoạt động để áp dụng cho người dùng chọn (true = đang hoạt động)
- **IsDeleted**: dùng cho xóa mềm
