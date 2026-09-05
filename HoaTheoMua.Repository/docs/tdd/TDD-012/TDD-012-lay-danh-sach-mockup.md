# TDD-012: Lấy danh sách Mockup

## Thông tin tài liệu
- **Tiêu đề**: Lấy danh sách Mockup có phân trang và lọc
- **Ghi chú**: Cùng API phục vụ Admin và Customer theo role: Admin quản lý cả Active/Inactive; Customer chỉ nhận Mockup Active để chọn tạo hoa.

## Metadata quản trị tài liệu
| Trường | Giá trị |
|---|---|
| Mã tài liệu | TDD-012 |
| Phiên bản | v0.4 |
| Author | Codex |
| Reviewer | |
| Approver | Chưa chỉ định |
| Owner | Nhóm Hoa Theo Mùa |
| Cập nhật gần nhất | 2026-09-04 |

---

## BƯỚC 1 — Thông tin & Bối cảnh

### Thông tin tài liệu
- **Mã tài liệu**: TDD-012
- **Tính năng**: Lấy danh sách Mockup
- **Tác giả**: Codex
- **Người review**: 
- **Phiên bản**: v0.4
- **Cập nhật (YYYY-MM-DD)**: 2026-09-04
- **Story liên quan**:
  - STORY-050
  - STORY-030 — Customer lấy Mockup Active để tạo hoa

### Business Rules
- BR-012-01: Cùng endpoint xác định phạm vi kết quả theo role và chỉ hiển thị Mockup có `is_deleted = false`
- BR-012-02: Admin thấy cả Mockup Active/Inactive; Customer luôn chỉ thấy `is_active=true`
- BR-012-03: Danh sách được sắp xếp theo `created_at` giảm dần (mới nhất trước)
- BR-012-04: Hỗ trợ phân trang với các tùy chọn: 5, 10, 20, 30, 40, 50 dòng/trang
- BR-012-05: Mặc định hiển thị 10 dòng/trang
- BR-012-06: Filter theo trạng thái `is_active` (tùy chọn)
- BR-012-07: `is_active` là filter quản trị cho Admin. Với Customer, service luôn ép điều kiện `is_active=true`, không để query parameter làm lộ Mockup Inactive.

### Bối cảnh & Mục tiêu

**Vấn đề**
> Admin cần quản lý toàn bộ Mockup chưa xóa; Customer cần xem cùng route nhưng chỉ được thấy Mockup Active để chọn khi tạo hoa.

**Mục tiêu**
- Lấy danh sách Mockup từ bảng mockup
- Filter theo trạng thái `is_active` (tùy chọn)
- Sắp xếp theo `created_at` giảm dần
- Phân trang (page, page_size)

**Ngoài phạm vi** (Out of scope)
- Thêm Mockup (TDD-013)
- Chỉnh sửa Mockup
- Chuyển trạng thái Mockup (TDD-014)
- Xóa Mockup (TDD-015)

---

## BƯỚC 2 — Kiến trúc & Sơ đồ

### Kiến trúc tổng quan (Architecture)
**Tiêu đề**: Trình tự lấy danh sách Mockup
**Mô tả**: Client gọi API → Controller nhận request → Service query với filter → Trả kết quả có phân trang

```mermaid
flowchart LR
    User["Admin / Customer"]
    API["MockupController"]
    Svc["MockupService"]
    EF["EF Core"]
    DB[("Database<br/>mockup")]

    User -->|"GET /api/mockups"| API
    API --> Svc
    Svc -->|"Query với filter<br/>is_deleted=false<br/>OrderBy created_at desc"| DB
    DB --> Svc
    Svc --> API
    API --> User
```

### Sequence Diagram
**Tiêu đề**: Trình tự lấy danh sách Mockup
**Mô tả**: Từng bước gọi qua lại giữa các thành phần

```mermaid
sequenceDiagram
    autonumber
    actor User as Admin hoặc Customer
    participant C as MockupController
    participant S as MockupService
    participant DB as Database

    User->>C: GET /api/mockups?is_active=&page=1&page_size=10

    Note over S: Build query
    S->>DB: Query mockup
    S->>DB: Where is_deleted = false
    alt Admin
        S->>DB: Filter by is_active nếu có
    else Customer
        S->>DB: Luôn Where is_active = true
    end
    S->>DB: OrderBy created_at desc
    S->>DB: Skip/Take for pagination

    DB-->>S: List<Mockup>
    S->>S: Count total records

    S-->>C: { items, total_count, page, page_size }
    C-->>User: HTTP 200
```

### Activity Diagram

- **Không áp dụng**: Luồng điều kiện đã được thể hiện đầy đủ trong Sequence Diagram và không có nhánh nghiệp vụ phức tạp cần một Activity Diagram riêng.

### State Diagram

- **Không áp dụng**: Endpoint không định nghĩa một state machine riêng cho entity chính.

### Mô hình dữ liệu (Data Model / ERD)
**Tiêu đề**: Bảng Mockup
**Mô tả**: Cấu trúc bảng mockup

```mermaid
erDiagram
    mockup ||--o{ client_histories : "snapshotted_in_metadata"

    mockup {
        uuid id PK
        string name
        string description
        string image_url
        bool is_active
        bool is_deleted
        timestamp created_at
        timestamp updated_at
    }

    client_histories {
        uuid id PK
        string type "flower | card | handmade_card | post"
        json metadata
        uuid base_id
        uuid output_id
    }
```

---

## BƯỚC 3 — API

### API Contract nội bộ

#### Endpoint #1: Lấy danh sách Mockup
- **Method**: GET
- **Endpoint**: `/api/mockups`
- **Tên endpoint**: Lấy danh sách Mockup
- **Mô tả**: Lấy danh sách có phân trang; phạm vi và filter trạng thái được áp dụng theo role
- **Quyền**: Admin, Customer

**Query Parameters:**
| Parameter | Type | Required | Default | Mô tả |
|-----------|------|----------|---------|-------|
| is_active | bool? | No | null | Admin: filter trạng thái; Customer: service luôn áp dụng `true` |
| page | int | No | 1 | Trang hiện tại |
| page_size | int | No | 10 | Số item trên trang (5, 10, 20, 30, 40, 50) |

**Ví dụ 1 — Happy path: Lấy tất cả Mockup** — HTTP `200`

Request:
```
GET /api/mockups
```

Response:
```json
{
  "value": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "name": "Mockup Sinh Nhật 1",
      "description": "Mockup bó hoa sinh nhật với màu sắc tươi sáng",
      "image_url": "https://storage.example.com/mockups/sinh-nhat-1.jpg",
      "is_active": true,
      "is_deleted": false,
      "created_at": "2026-08-27T10:00:00Z",
      "updated_at": null
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "name": "Mockup Cưới Hỏi 1",
      "description": "Mockup bó hoa cưới với kiểu dáng sang trọng",
      "image_url": "https://storage.example.com/mockups/cuoi-hoi-1.jpg",
      "is_active": false,
      "is_deleted": false,
      "created_at": "2026-08-26T10:00:00Z",
      "updated_at": null
    }
  ],
  "total_count": 2,
  "page": 1,
  "page_size": 10
}
```

**Ví dụ 2 — Happy path: Filter theo is_active = true** — HTTP `200`

Request:
```
GET /api/mockups?is_active=true
```

Response:
```json
{
  "value": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "name": "Mockup Sinh Nhật 1",
      "is_active": true,
      "is_deleted": false,
      "created_at": "2026-08-27T10:00:00Z"
    }
  ],
  "total_count": 1,
  "page": 1,
  "page_size": 10
}
```

**Ví dụ 3 — Happy path: Kết hợp filter và pagination** — HTTP `200`

Request:
```
GET /api/mockups?is_active=true&page=1&page_size=5
```

Response:
```json
{
  "value": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "name": "Mockup Sinh Nhật 1",
      "description": "Mockup bó hoa sinh nhật với màu sắc tươi sáng",
      "image_url": "https://storage.example.com/mockups/sinh-nhat-1.jpg",
      "is_active": true,
      "is_deleted": false,
      "created_at": "2026-08-27T10:00:00Z",
      "updated_at": null
    }
  ],
  "total_count": 1,
  "page": 1,
  "page_size": 5
}
```

**Ví dụ 4 — Happy path: Danh sách rỗng** — HTTP `200`

Request:
```
GET /api/mockups?is_active=false
```

Response:
```json
{
  "value": [],
  "total_count": 0,
  "page": 1,
  "page_size": 10
}
```

**Ví dụ 5 — Happy path: Phân trang với page 2** — HTTP `200`

Request:
```
GET /api/mockups?page=2&page_size=5
```

Response:
```json
{
  "value": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440003",
      "name": "Mockup Cưới Hỏi 1",
      "description": "Mockup bó hoa cưới với kiểu dáng sang trọng",
      "image_url": "https://storage.example.com/mockups/cuoi-hoi-1.jpg",
      "is_active": false,
      "is_deleted": false,
      "created_at": "2026-08-25T10:00:00Z",
      "updated_at": null
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440004",
      "name": "Mockup Tang Lễ 1",
      "description": "Mockup bó hoa tang lễ trang nhã",
      "image_url": "https://storage.example.com/mockups/tang-le-1.jpg",
      "is_active": true,
      "is_deleted": false,
      "created_at": "2026-08-24T10:00:00Z",
      "updated_at": null
    }
  ],
  "total_count": 7,
  "page": 2,
  "page_size": 5
}
```

**Ví dụ 6 — Chưa đăng nhập** — HTTP `401`

Request:
```
GET /api/mockups
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

**Ví dụ 7 — Không có quyền xem** — HTTP `403`

Request:
```
GET /api/mockups
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
| UNAUTHORIZED | 401 | Bạn cần đăng nhập để thực hiện thao tác này |
| ACCESS_DENIED | 403 | Bạn không có quyền xem danh sách Mockup |

### API Contract bên ngoài

- **Endpoints sử dụng**: Không áp dụng; endpoint này không gọi service hoặc API bên thứ ba.
- **Field quan trọng**: Không áp dụng.
- **Xử lý lỗi từ đối tác**: Không áp dụng.
- **Quirks / cạm bẫy**: Không áp dụng.

---

## BƯỚC 4 — Tham chiếu

> Chú thích: 🔴 Tham chiếu đến (tài liệu này đọc/phụ thuộc) · ⚫ Trỏ vào tài liệu này (tài liệu khác phụ thuộc vào tài liệu này) · ⋯ Bị ảnh hưởng (thay đổi ở đây có thể làm tài liệu kia sai theo)

### 🔴 Tham chiếu đến
- Mockup Entity - Bảng mockup trong Database

### ⚫ Trỏ vào tài liệu này
- TDD-013 - Thêm Mockup
- TDD-014 - Chuyển trạng thái Mockup
- TDD-015 - Xóa Mockup
- AI_Mockup_Context - Tham chiếu context và business rules

### ⋯ Bị ảnh hưởng
- STORY-030 - Khi chọn Mockup, cần filter theo is_active=true và is_deleted=false
