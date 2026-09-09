# TDD-010: Cập nhật Card Config

## Thông tin tài liệu
- **Tiêu đề**: Cập nhật cấu hình card
- **Ghi chú**: API nhận đầy đủ `group`, `key`, `kind`, `value` khi Admin cập nhật Card Config; `is_public` là tùy chọn. `kind` phải giữ nguyên, còn group/key/value được cập nhật sau validation.

## Metadata quản trị tài liệu
| Trường | Giá trị |
|---|---|
| Mã tài liệu | TDD-010 |
| Phiên bản | v0.4 |
| Author | Codex |
| Reviewer | |
| Approver | Chưa chỉ định |
| Owner | Nhóm Hoa Theo Mùa |
| Cập nhật gần nhất | 2026-09-04 |

---

## BƯỚC 1 — Thông tin & Bối cảnh

### Thông tin tài liệu
- **Mã tài liệu**: TDD-010
- **Tính năng**: Cập nhật card config
- **Tác giả**: Codex
- **Người review**: 
- **Phiên bản**: v0.4
- **Cập nhật (YYYY-MM-DD)**: 2026-09-04
- **Story liên quan**: Không có

### Business Rules
- BR-010-01: Chỉ Admin có quyền cập nhật card config
- BR-010-02: Sử dụng bảng Config có sẵn
- BR-010-03: Request bắt buộc truyền đầy đủ `group`, `key`, `kind`, `value`; `is_public` là tùy chọn
- BR-010-04: `kind` là bất biến và phải trùng với record hiện tại; group/key/value được phép cập nhật sau validation
- BR-010-05: Value phải là JSON hợp lệ
- BR-010-06: UpdatedAt được cập nhật khi lưu
- BR-010-07: Không thể cập nhật config có IsDeleted = true (đã xóa mềm)
- BR-010-08: Config `IsPublic=false` vẫn được cập nhật và có thể đổi trạng thái qua `is_public`; chỉ `IsDeleted=true` bị chặn
- BR-010-09: Group, key và dữ liệu `value.group` nếu có phải đồng nhất theo convention hiện hành; key mới không được trùng Config chưa xóa khác

### Bối cảnh & Mục tiêu

**Vấn đề**
> Admin cần cập nhật thông tin Card Config theo contract hiện hành. Các thiệp đã tạo vẫn dùng snapshot nên việc đổi group/key/value của record live không làm thay đổi lịch sử.

**Mục tiêu**
- Cập nhật group, key, value và tùy chọn `is_public`
- Giữ nguyên Kind của record
- Validate value là JSON hợp lệ
- Cập nhật UpdatedAt

**Ngoài phạm vi** (Out of scope)
- Tạo card config (TDD-009)
- Xóa card config (TDD-011)

---

## BƯỚC 2 — Kiến trúc & Sơ đồ

### Kiến trúc tổng quan (Architecture)
**Tiêu đề**: Trình tự cập nhật card config
**Mô tả**: Admin gọi API → Controller nhận request → Service validate → Cập nhật DB → Trả kết quả

```mermaid
flowchart LR
    Admin["Admin"]
    API["ConfigController"]
    Svc["ConfigService"]
    EF["EF Core"]
    DB[("Database<br/>Config")]

    Admin -->|"PUT /api/v1/configs/{id}"| API
    API --> Svc
    Svc -->|"Validate<br/>Find record"| DB
    Svc -->|"Update value"| DB
    Svc -->|"Update updated_at"| DB
    Svc --> API
    API --> Admin
```

### Sequence Diagram
**Tiêu đề**: Trình tự cập nhật card config
**Mô tả**: Từng bước gọi qua lại giữa các thành phần

```mermaid
sequenceDiagram
    autonumber
    actor Admin
    participant C as ConfigController
    participant S as ConfigService
    participant DB as Database

    Admin->>C: PUT /api/v1/configs/{id} {group, key, kind, value, is_public?}

    Note over S: Validate request
    S->>S: Validate required fields, JSON, group/key convention và immutable kind

    alt Invalid JSON
        S-->>C: Trả lỗi validation
        C-->>Admin: HTTP 400
    end

    Note over S: Tìm record
    S->>DB: Find Config by id
    alt Not found or IsDeleted = true
        S-->>C: Trả lỗi not found
        C-->>Admin: HTTP 404
    end
    Note over S: Cập nhật
    S->>DB: Update group, key, value và is_public nếu truyền
    S->>DB: Update updated_at

    DB-->>S: updated record
    S-->>C: HTTP 200 {config}
    C-->>Admin: HTTP 200
```

### Activity Diagram

- **Không áp dụng**: Luồng điều kiện đã được thể hiện đầy đủ trong Sequence Diagram và không có nhánh nghiệp vụ phức tạp cần một Activity Diagram riêng.

### State Diagram

- **Không áp dụng**: Endpoint không định nghĩa một state machine riêng cho entity chính.

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

#### Endpoint #1: Cập nhật card config
- **Method**: PUT
- **Endpoint**: `/api/v1/configs/{id}`
- **Tên endpoint**: Cập nhật card config
- **Mô tả**: Cập nhật Card Config bằng request đầy đủ; Kind bắt buộc nhưng bất biến
- **Quyền**: Chỉ Admin

**Path Parameters:**
| Parameter | Type | Required | Mô tả |
|-----------|------|----------|-------|
| id | uuid | Yes | ID của card config |

**Request Body:**
```json
{
  "group": "card_size",
  "key": "size_A",
  "kind": "Setting",
  "is_public": true,
  "value": { ... }
}
```

**Lưu ý:** `group`, `key`, `kind`, `value` đều bắt buộc. `kind` phải trùng record hiện tại; `is_public` là tùy chọn.

**Ví dụ 1 — Happy path: Cập nhật giá size_A** — HTTP `200`

Request:
```json
PUT /api/v1/configs/550e8400-e29b-41d4-a716-446655440001
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

Response:
```json
{
  "value": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "key": "size_A",
    "value": {
      "name": "A",
      "width": 12,
      "height": 18,
      "base_price": 120000,
      "max_words": 120
    },
    "group": "card_size",
    "kind": "Setting",
    "is_public": true,
    "is_deleted": false,
    "user_id": "...",
    "created_at": "2026-08-26T10:00:00Z",
    "updated_at": "2026-08-26T12:30:00Z"
  }
}
```

**Ví dụ 2 — Happy path: Cập nhật phụ phí calligraphy** — HTTP `200`

Request:
```json
PUT /api/v1/configs/550e8400-e29b-41d4-a716-446655440002
{
  "group": "card_config",
  "key": "calligraphy_words",
  "kind": "Setting",
  "value": [
    { "min_words": 0, "max_words": 35, "extra_price": 10000 },
    { "min_words": 36, "max_words": 70, "extra_price": 49000 },
    { "min_words": 71, "max_words": 100, "extra_price": 79000 }
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
      { "min_words": 0, "max_words": 35, "extra_price": 10000 },
      { "min_words": 36, "max_words": 70, "extra_price": 49000 },
      { "min_words": 71, "max_words": 100, "extra_price": 79000 }
    ],
    "group": "card_config",
    "kind": "Setting",
    "is_public": true,
    "is_deleted": false,
    "created_at": "2026-08-26T10:00:00Z",
    "updated_at": "2026-08-26T12:35:00Z"
  }
}
```

**Ví dụ 3 — Validation: Invalid JSON** — HTTP `400`

Request:
```json
PUT /api/v1/configs/550e8400-e29b-41d4-a716-446655440001
{
  "group": "card_size",
  "key": "size_A",
  "kind": "Setting",
  "value": "not valid json {"
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

**Ví dụ 4 — Not found** — HTTP `404`

Request:
```json
PUT /api/v1/configs/00000000-0000-0000-0000-000000000000
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
    "code": "NOT_FOUND",
    "message": "Config not found."
  }
}
```

**Ví dụ 5 — Authorization: Non-admin user** — HTTP `403`

Request:
```json
PUT /api/v1/configs/550e8400-e29b-41d4-a716-446655440001
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

**Ví dụ 6 — Chưa đăng nhập** — HTTP `401`

Request:
```json
PUT /api/v1/configs/550e8400-e29b-41d4-a716-446655440001
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
    "code": "UNAUTHORIZED",
    "message": "Bạn cần đăng nhập để thực hiện thao tác này."
  }
}
```

#### Mã lỗi
| Code | HTTP | Khi nào xảy ra |
|---|---|---|
| VALIDATION_ERROR | 400 | Thiếu `group`, `key`, `kind`, `value` hoặc value không phải JSON hợp lệ |
| NOT_FOUND | 404 | Cấu hình card không tồn tại hoặc đã bị xóa |
| CONFLICT | 409 | Kind khác record hiện tại, group/key/value.group không đồng nhất hoặc key mới bị trùng |
| ACCESS_DENIED | 403 | Bạn không có quyền cập nhật cấu hình card |
| UNAUTHORIZED | 401 | Bạn cần đăng nhập để thực hiện thao tác này |

### API Contract bên ngoài

- **Endpoints sử dụng**: Không áp dụng; endpoint này không gọi service hoặc API bên thứ ba.
- **Field quan trọng**: Không áp dụng.
- **Xử lý lỗi từ đối tác**: Không áp dụng.
- **Quirks / cạm bẫy**: Không áp dụng.

---

## BƯỚC 4 — Tham chiếu

> Chú thích: 🔴 Tham chiếu đến (tài liệu này đọc/phụ thuộc) · ⚫ Trỏ vào tài liệu này (tài liệu khác phụ thuộc vào tài liệu này) · ⋯ Bị ảnh hưởng (thay đổi ở đây có thể làm tài liệu kia sai theo)

### 🔴 Tham chiếu đến
- TDD-008 - Lấy danh sách card configs (sử dụng chung bảng)

### ⚫ Trỏ vào tài liệu này
- TDD-009 - Tạo card config
- TDD-008 - Lấy danh sách card configs
- TDD-011 - Xóa card config
- TDD-006 - Tạo thiệp thiết kế AI (sử dụng Config cho pricing)
- TDD-007 - Tạo lại thiệp từ lịch sử (sử dụng Config cho pricing)

### ⋯ Bị ảnh hưởng
- TDD-006, TDD-007 - Thay đổi Config ảnh hưởng đến pricing (nhưng các thiệp đã tạo dùng snapshot nên không bị ảnh hưởng)

---

### Ghi chú bổ sung

- Request bắt buộc `group`, `key`, `kind`, `value`; `is_public` tùy chọn
- `kind` bất biến; group/key/value có thể thay đổi sau validation
- UpdatedAt tự động cập nhật khi lưu
- Các thiệp đã tạo sử dụng snapshot nên không bị ảnh hưởng khi config thay đổi
- **IsDeleted**: dùng cho xóa mềm (config không tìm thấy khi IsDeleted = true)
- **IsPublic**: dùng để xác định config có đang hoạt động để áp dụng cho người dùng chọn (true = đang hoạt động)
