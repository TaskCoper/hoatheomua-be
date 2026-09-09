# TDD-011: Xóa Card Config

## Thông tin tài liệu
- **Tiêu đề**: Xóa cấu hình card
- **Ghi chú**: API cho phép Admin xóa card config trong bảng Config. Không có ràng buộc với generated_cards vì các thiệp đã sử dụng snapshot.

## Metadata quản trị tài liệu
| Trường | Giá trị |
|---|---|
| Mã tài liệu | TDD-011 |
| Phiên bản | v0.4 |
| Author | Codex |
| Reviewer | |
| Approver | Chưa chỉ định |
| Owner | Nhóm Hoa Theo Mùa |
| Cập nhật gần nhất | 2026-09-04 |

---

## BƯỚC 1 — Thông tin & Bối cảnh

### Thông tin tài liệu
- **Mã tài liệu**: TDD-011
- **Tính năng**: Xóa card config
- **Tác giả**: Codex
- **Người review**: 
- **Phiên bản**: v0.4
- **Cập nhật (YYYY-MM-DD)**: 2026-09-04
- **Story liên quan**: Không có

### Business Rules
- BR-011-01: Chỉ Admin có quyền xóa card config
- BR-011-02: Sử dụng bảng Config có sẵn
- BR-011-03: Xóa mềm: set IsDeleted = true
- BR-011-04: UpdatedAt được cập nhật khi xóa
- BR-011-05: Không check ràng buộc với generated_cards
- BR-011-06: Các thiệp đã tạo dùng snapshot nên không bị ảnh hưởng
- BR-011-07: Không thể xóa config đã bị xóa trước đó (IsDeleted = true)

### Bối cảnh & Mục tiêu

**Vấn đề**
> Admin cần xóa card config không còn sử dụng. Không có ràng buộc với generated_cards vì các thiệp đã dùng snapshot.

**Mục tiêu**
- Xóa card config (IsDeleted = true)
- Cập nhật UpdatedAt

**Ngoài phạm vi** (Out of scope)
- Tạo card config (TDD-009)
- Cập nhật card config (TDD-010)
- Khôi phục card config đã xóa

---

## BƯỚC 2 — Kiến trúc & Sơ đồ

### Kiến trúc tổng quan (Architecture)
**Tiêu đề**: Trình tự xóa card config
**Mô tả**: Admin gọi API → Controller nhận request → Service tìm và xóa → Cập nhật DB → Trả kết quả

```mermaid
flowchart LR
    Admin["Admin"]
    API["ConfigController"]
    Svc["ConfigService"]
    EF["EF Core"]
    DB[("Database<br/>Config")]

    Admin -->|"DELETE /api/v1/configs/{id}"| API
    API --> Svc
    Svc -->|"Find record"| DB
    Svc -->|"Set is_deleted = true"| DB
    Svc -->|"Update updated_at"| DB
    Svc --> API
    API --> Admin
```

### Sequence Diagram
**Tiêu đề**: Trình tự xóa card config
**Mô tả**: Từng bước gọi qua lại giữa các thành phần

```mermaid
sequenceDiagram
    autonumber
    actor Admin
    participant C as ConfigController
    participant S as ConfigService
    participant DB as Database

    Admin->>C: DELETE /api/v1/configs/{id}

    Note over S: Tìm record
    S->>DB: Find Config by id

    alt Not found or IsDeleted = true
        S-->>C: Trả lỗi not found
        C-->>Admin: HTTP 404
    end

    Note over S: Xóa mềm
    S->>DB: Set is_deleted = true
    S->>DB: Update updated_at

    DB-->>S: updated record
    S-->>C: HTTP 200 { success: true }
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
    Config ||--o{ generated_cards : "snapshot"

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

    generated_cards {
        uuid id PK
        string size_key
        string size_name
        decimal size_base_price
        json word_config_snapshot
        string card_type "ai | handmade; required"
    }
```

---

## BƯỚC 3 — API

### API Contract nội bộ

#### Endpoint #1: Xóa card config
- **Method**: DELETE
- **Endpoint**: `/api/v1/configs/{id}`
- **Tên endpoint**: Xóa card config
- **Mô tả**: Xóa card config (IsDeleted = true)
- **Quyền**: Chỉ Admin

**Path Parameters:**
| Parameter | Type | Required | Mô tả |
|-----------|------|----------|-------|
| id | uuid | Yes | ID của card config |

**Ví dụ 1 — Happy path: Xóa thành công** — HTTP `200`

Request:
```
DELETE /api/v1/configs/550e8400-e29b-41d4-a716-446655440001
```

Response:
```json
{
  "value": {
    "success": true,
    "message": "Config deleted successfully."
  }
}
```

**Ví dụ 2 — Not found: ID không tồn tại** — HTTP `404`

Request:
```
DELETE /api/v1/configs/00000000-0000-0000-0000-000000000000
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

**Ví dụ 3 — Not found: Đã bị xóa trước đó** — HTTP `404`

Request:
```
DELETE /api/v1/configs/550e8400-e29b-41d4-a716-446655440001
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

**Ví dụ 4 — Authorization: Non-admin user** — HTTP `403`

Request:
```
DELETE /api/v1/configs/550e8400-e29b-41d4-a716-446655440001
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

**Ví dụ 5 — Chưa đăng nhập** — HTTP `401`

Request:
```
DELETE /api/v1/configs/550e8400-e29b-41d4-a716-446655440001
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
| NOT_FOUND | 404 | Cấu hình card không tồn tại hoặc đã bị xóa |
| ACCESS_DENIED | 403 | Bạn không có quyền xóa cấu hình card |
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
- TDD-008 - Lấy danh sách card configs (sử dụng chung bảng, filter IsPublic)

### ⚫ Trỏ vào tài liệu này
- TDD-009 - Tạo card config
- TDD-008 - Lấy danh sách card configs
- TDD-010 - Cập nhật card config

### ⋯ Bị ảnh hưởng
- Không có - generated_cards dùng snapshot nên không bị ảnh hưởng

---

### Ghi chú bổ sung

- Xóa mềm: record vẫn còn trong DB với IsDeleted = true
- generated_cards dùng snapshot nên khi config bị xóa, các thiệp đã tạo vẫn giữ nguyên thông tin giá
- TDD-008 không trả về các record có IsDeleted = true hoặc IsPublic = false
- **IsDeleted**: dùng cho xóa mềm (config không tìm thấy khi IsDeleted = true)
- **IsPublic**: dùng để xác định config có đang hoạt động để áp dụng cho người dùng chọn (true = đang hoạt động)
