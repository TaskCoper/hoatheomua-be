# TDD-008: Lấy danh sách Card Configs

## Thông tin tài liệu
- **Tiêu đề**: Lấy danh sách cấu hình card có phân trang và lọc
- **Ghi chú**: API cho phép Admin lấy danh sách card configs `Kind="Setting"` từ bảng Config với filter theo group, key và phân trang.

## Metadata quản trị tài liệu
| Trường | Giá trị |
|---|---|
| Mã tài liệu | TDD-008 |
| Phiên bản | v0.4 |
| Author | Codex |
| Reviewer | |
| Approver | Chưa chỉ định |
| Owner | Nhóm Hoa Theo Mùa |
| Cập nhật gần nhất | 2026-09-04 |

---

## BƯỚC 1 — Thông tin & Bối cảnh

### Thông tin tài liệu
- **Mã tài liệu**: TDD-008
- **Tính năng**: Lấy danh sách card configs
- **Tác giả**: Codex
- **Người review**: 
- **Phiên bản**: v0.4
- **Cập nhật (YYYY-MM-DD)**: 2026-09-04
- **Story liên quan**: Không có

### Business Rules
- BR-008-01: Chỉ Admin có quyền xem danh sách
- BR-008-02: Sử dụng bảng Config có sẵn
- BR-008-03: Kind = "Setting" để filter card configs
- BR-008-04: Chỉ trả về các config có IsDeleted = false (chưa bị xóa mềm)
- BR-008-05: Chỉ trả về các config có IsPublic = true (đang hoạt động)
- BR-008-06: Filter theo group và/hoặc key - nếu có param thì lọc theo đó
- BR-008-07: Pagination - mặc định page = 1, page_size = 20
- BR-008-08: Response trả về tổng số bản ghi để frontend phân trang

### Bối cảnh & Mục tiêu

**Vấn đề**
> Admin cần xem danh sách card configs để quản lý. Danh sách cần có filter theo group, key và pagination để dễ xem.

**Mục tiêu**
- Lấy danh sách card configs từ bảng Config
- Filter theo group và/hoặc key (tùy chọn)
- Phân trang (page, page_size)
- Không trả về các config có IsPublic = false

**Ngoài phạm vi** (Out of scope)
- Tạo card config (TDD-009)
- Cập nhật card config (TDD-010)
- Xóa card config (TDD-011)

---

## BƯỚC 2 — Kiến trúc & Sơ đồ

### Kiến trúc tổng quan (Architecture)
**Tiêu đề**: Trình tự lấy danh sách card configs
**Mô tả**: Client gọi API → Controller nhận request → Service query với filter → Trả kết quả có phân trang

```mermaid
flowchart LR
    User["Admin"]
    API["ConfigController"]
    Svc["ConfigService"]
    EF["EF Core"]
    DB[("Database<br/>Config")]

    User -->|"GET /api/v1/configs/settings"| API
    API --> Svc
    Svc -->|"Query với filter<br/>Kind=Setting<br/>IsPublic=true<br/>IsDeleted=false"| DB
    DB --> Svc
    Svc --> API
    API --> User
```

### Sequence Diagram
**Tiêu đề**: Trình tự lấy danh sách card configs
**Mô tả**: Từng bước gọi qua lại giữa các thành phần

```mermaid
sequenceDiagram
    autonumber
    actor User as Admin
    participant C as ConfigController
    participant S as ConfigService
    participant DB as Database

    User->>C: GET /api/v1/configs/settings?groupName=card_size&key=size_A&page=1&page_size=20

    Note over S: Build query
    S->>DB: Query Config
    S->>DB: Where kind = "Setting"
    S->>DB: Where is_deleted = false
    S->>DB: Where is_public = true
    S->>DB: Filter by groupName (nếu có)
    S->>DB: Filter by key (nếu có)
    S->>DB: Order by created_at desc
    S->>DB: Skip/Take for pagination

    DB-->>S: List<Config>
    S->>S: Count total records

    S-->>C: { items, total_count, page, page_size }
    C-->>User: HTTP 200
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

#### Endpoint #1: Lấy danh sách card configs
- **Method**: GET
- **Endpoint**: `/api/v1/configs/settings`
- **Tên endpoint**: Lấy danh sách card configs
- **Mô tả**: Lấy danh sách với filter theo group, key và phân trang
- **Quyền**: Chỉ Admin

**Query Parameters:**
| Parameter | Type | Required | Default | Mô tả |
|-----------|------|----------|---------|-------|
| groupName | string | No | null | Filter theo group (card_size, card_config) |
| key | string | No | null | Filter theo key |
| isPublic | bool | No | null | Filter theo isPublic |
| page | int | No | 1 | Trang hiện tại |
| page_size | int | No | 20 | Số item trên trang |

**Ví dụ 1 — Happy path: Lấy tất cả card configs (Kind=Setting)** — HTTP `200`

Request:
```
GET /api/v1/configs/settings
```

Response:
```json
{
  "value": {
    "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "key": "size_A",
      "value": { "name": "A", "width": 10, "height": 15, "base_price": 100000, "max_words": 100 },
      "group": "card_size",
      "kind": "Setting",
      "is_public": true,
      "is_deleted": false,
      "created_at": "2026-08-26T10:00:00Z",
      "updated_at": null
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "key": "size_B",
      "value": { "name": "B", "width": 15, "height": 20, "base_price": 150000, "max_words": 150 },
      "group": "card_size",
      "kind": "Setting",
      "is_public": true,
      "is_deleted": false,
      "created_at": "2026-08-26T10:00:00Z",
      "updated_at": null
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440003",
      "key": "calligraphy_words",
      "value": [ { "min_words": 0, "max_words": 35, "extra_price": 0 }, ... ],
      "group": "card_config",
      "kind": "Setting",
      "is_public": true,
      "is_deleted": false,
      "created_at": "2026-08-26T10:00:00Z",
      "updated_at": null
    }
    ],
    "total_count": 3,
    "page": 1,
    "page_size": 20
  }
}
```

**Ví dụ 2 — Happy path: Filter theo group = card_size** — HTTP `200`

Request:
```
GET /api/v1/configs/settings?groupName=card_size
```

Response:
```json
{
  "value": {
    "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "key": "size_A",
      "value": { "name": "A", ... },
      "group": "card_size",
      "kind": "Setting",
      ...
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "key": "size_B",
      "value": { "name": "B", ... },
      "group": "card_size",
      "kind": "Setting",
      ...
    }
    ],
    "total_count": 2,
    "page": 1,
    "page_size": 20
  }
}
```

**Ví dụ 3 — Happy path: Filter theo key = calligraphy_words** — HTTP `200`

Request:
```
GET /api/v1/configs/settings?key=calligraphy_words
```

Response:
```json
{
  "value": {
    "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440003",
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
      "created_at": "2026-08-26T10:00:00Z",
      "updated_at": null
    }
    ],
    "total_count": 1,
    "page": 1,
    "page_size": 20
  }
}
```

**Ví dụ 4 — Happy path: Kết hợp filter và pagination** — HTTP `200`

Request:
```
GET /api/v1/configs/settings?groupName=card_size&page=1&page_size=2
```

Response:
```json
{
  "value": {
    "items": [
      { "id": "...001", "group": "card_size", "key": "size_A", "kind": "Setting" },
      { "id": "...002", "group": "card_size", "key": "size_B", "kind": "Setting" }
    ],
    "total_count": 7,
    "page": 1,
    "page_size": 2
  }
}
```

**Ví dụ 5 — Happy path: Không có dữ liệu** — HTTP `200`

Request:
```
GET /api/v1/configs/settings?key=nonexistent_key
```

Response:
```json
{
  "value": {
    "items": [],
    "total_count": 0,
    "page": 1,
    "page_size": 20
  }
}
```

**Ví dụ 6 — Chưa đăng nhập** — HTTP `401`

Request:
```
GET /api/v1/configs/settings
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

**Ví dụ — User không có quyền xem Config** — HTTP `403`

Request:
```
GET /api/v1/configs/settings?group=card_size
```

Response:
```json
{
  "error": {
    "code": "ACCESS_DENIED",
    "message": "Bạn không có quyền xem danh sách cấu hình card."
  }
}
```

#### Mã lỗi
| Code | HTTP | Khi nào xảy ra |
|---|---|---|
| UNAUTHORIZED | 401 | Bạn cần đăng nhập để thực hiện thao tác này |
| ACCESS_DENIED | 403 | Bạn không có quyền xem danh sách cấu hình card |

### API Contract bên ngoài

- **Endpoints sử dụng**: Không áp dụng; endpoint này không gọi service hoặc API bên thứ ba.
- **Field quan trọng**: Không áp dụng.
- **Xử lý lỗi từ đối tác**: Không áp dụng.
- **Quirks / cạm bẫy**: Không áp dụng.

---

## BƯỚC 4 — Tham chiếu

> Chú thích: 🔴 Tham chiếu đến (tài liệu này đọc/phụ thuộc) · ⚫ Trỏ vào tài liệu này (tài liệu khác phụ thuộc vào tài liệu này) · ⋯ Bị ảnh hưởng (thay đổi ở đây có thể làm tài liệu kia sai theo)

### 🔴 Tham chiếu đến
- Không có

### ⚫ Trỏ vào tài liệu này
- TDD-009 - Tạo card config
- TDD-010 - Cập nhật card config
- TDD-011 - Xóa card config
- TDD-006 - Tạo thiệp thiết kế AI (sử dụng Config cho pricing)
- TDD-007 - Tạo lại thiệp từ lịch sử (sử dụng Config cho pricing)

### ⋯ Bị ảnh hưởng
- Không có

---

### Ghi chú bổ sung

- Filter theo group, key sử dụng exact match
- Kind luôn là "Setting" cho card configs
- Các record có IsDeleted = true (đã xóa mềm) sẽ không được trả về
- Các record có IsPublic = false (không hoạt động) sẽ không được trả về
- Sử dụng endpoint `/api/v1/configs/settings`; API phải bổ sung pagination theo contract TDD và chỉ trả record `isPublic=true`, `isDeleted=false`
- **IsDeleted**: dùng cho xóa mềm
- **IsPublic**: dùng để xác định config có đang hoạt động để áp dụng cho người dùng chọn (true = đang hoạt động)
