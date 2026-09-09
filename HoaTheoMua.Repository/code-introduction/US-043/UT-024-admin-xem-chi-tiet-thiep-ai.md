# UT-024: Unit Tests - Admin xem chi tiết thiệp AI

## Thông tin tài liệu

- **Tiêu đề**: Unit test cho API Admin xem chi tiết thiệp AI.
- **Ghi chú**: Mỗi UT bên dưới kiểm tra một nhánh xử lý chính của TDD-024.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-024
- **Phiên bản**: v0.1
- **Author**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-07

---

## UT-024-01: Xem chi tiết thiệp AI thành công

## Thông tin tài liệu

- **Tiêu đề**: Admin xem được đầy đủ chi tiết thiệp AI.
- **Ghi chú**: Kiểm tra luồng chính khi thiệp tồn tại và có ảnh.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetById`
- **Loại**: Happy
- **Precondition / Mock setup**:
  - Có `client_histories` type `card`, không deleted.
  - `client_histories.output_id = generated_cards.id`.
  - `generated_cards` có `image_url`, sender, receiver, lời chúc, size và hình thức.
  - `client_histories.metadata` có snapshot template.
  - Có 1 Order liên quan chứa `generatedCardId` trong `order_details.generated_card_ids`.
- **Input**:

```http
GET /api/v1/admin/generated-cards/ca000000-0000-0000-0000-000000000001
```

- **Expected output**:

```text
HTTP 200
Trả đúng generatedCardId.
Trả ảnh thiệp, template, size, hình thức, người gửi, người nhận, lời chúc, ngày tạo.
fileStatus = available.
canDownload = true.
Có danh sách Order liên quan.
Response có giá của generated card: basePrice, extraPrice, totalPrice.
Order liên quan có orderId, status, totalFee.
Response không có bill snapshot chi tiết của Order.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận Admin xem được chi tiết thiệp AI theo luồng chính.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-043
- **Section**: AC-003, AC-005
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: BƯỚC 3 - API
- **Ghi chú**:

---

## UT-024-02: Thiệp liên kết nhiều Order

## Thông tin tài liệu

- **Tiêu đề**: Chi tiết thiệp trả đầy đủ nhiều Order liên quan.
- **Ghi chú**: Một thiệp có thể được dùng trong nhiều Order.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetById`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Có 1 generated card hợp lệ.
  - Có 1 history trỏ đến generated card đó.
  - Có 2 Order khác nhau.
  - Cả 2 Order đều có `order_details.generated_card_ids` chứa `generatedCardId`.
- **Input**:

```http
GET /api/v1/admin/generated-cards/ca000000-0000-0000-0000-000000000002
```

- **Expected output**:

```text
HTTP 200
orders trả đủ 2 Order.
Không làm mất hoặc gộp sai Order.
Mỗi Order trả orderId, status, totalFee.
Không trả bill snapshot chi tiết của từng Order.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận danh sách Order liên quan được trả đầy đủ.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-043
- **Section**: AC-001
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: API - Ví dụ nhiều Order liên quan
- **Ghi chú**:

---

## UT-024-03: Thiệp Calligraphy vẫn trả nội dung để Admin xem

## Thông tin tài liệu

- **Tiêu đề**: Nội dung Calligraphy được trả riêng trong chi tiết.
- **Ghi chú**: Ảnh output có thể không in sẵn người gửi/người nhận/lời chúc, nhưng BE vẫn phải trả dữ liệu này.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetById`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Card có `form_type = Calligraphy`.
  - Card có `sender_name = "Minh"`.
  - Card có `receiver_name = "Lan"`.
  - Card có `message_content = "Chúc bạn luôn hạnh phúc"`.
- **Input**:

```http
GET /api/v1/admin/generated-cards/ca000000-0000-0000-0000-000000000003
```

- **Expected output**:

```text
HTTP 200
formType = Calligraphy.
senderName = "Minh".
receiverName = "Lan".
messageContent = "Chúc bạn luôn hạnh phúc".
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận dữ liệu viết tay vẫn được Admin xem trong phần chi tiết.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-043
- **Section**: AC-007
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: Business Rules
- **Ghi chú**:

---

## UT-024-04: File ảnh không khả dụng

## Thông tin tài liệu

- **Tiêu đề**: Thiệp vẫn trả metadata khi không có imageUrl.
- **Ghi chú**: BE không xóa history và không gọi AI tạo lại.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetById`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Có history type `card` hợp lệ.
  - Có generated card tương ứng.
  - `generated_cards.image_url = null` hoặc rỗng.
  - Card vẫn có sender, receiver, lời chúc, template snapshot.
- **Input**:

```http
GET /api/v1/admin/generated-cards/ca000000-0000-0000-0000-000000000004
```

- **Expected output**:

```text
HTTP 200
Vẫn trả metadata của thiệp.
fileStatus = unavailable.
canDownload = false.
Không gọi AI tạo lại ảnh.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận file ảnh lỗi không làm mất dữ liệu chi tiết thiệp.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-043
- **Section**: AC-004
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: API - Ví dụ file ảnh không khả dụng
- **Ghi chú**:

---

## UT-024-05: Không tìm thấy thiệp

## Thông tin tài liệu

- **Tiêu đề**: API trả lỗi khi generatedCardId không tồn tại.
- **Ghi chú**: Không được trả nhầm dữ liệu của thiệp khác.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetById`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Không có `generated_cards.id` trùng `generatedCardId`.
  - Hoặc có card nhưng không có `client_histories.type = card` trỏ đến card đó.
- **Input**:

```http
GET /api/v1/admin/generated-cards/ffffffff-ffff-ffff-ffff-ffffffffffff
```

- **Expected output**:

```text
HTTP 404
Trả lỗi GENERATED_CARD_NOT_FOUND.
Không trả dữ liệu của thiệp khác.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận API xử lý đúng khi tài nguyên không tồn tại.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-043
- **Section**: EXC-03
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: Mã lỗi
- **Ghi chú**:

---

## UT-024-06: generatedCardId không hợp lệ

## Thông tin tài liệu

- **Tiêu đề**: API báo lỗi khi mã thiệp rỗng/không hợp lệ.
- **Ghi chú**: Với route Guid, case không parse được thường bị chặn ở model binding; service vẫn cần guard `Guid.Empty`.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetById`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Không cần dữ liệu đặc biệt.
- **Input**:

```http
GET /api/v1/admin/generated-cards/00000000-0000-0000-0000-000000000000
```

- **Expected output**:

```text
HTTP 400
Trả lỗi INVALID_GENERATED_CARD_ID.
Không query chi tiết thiệp.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận BE chặn mã thiệp không hợp lệ trước khi xử lý.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-043
- **Section**: EXC-03
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: Mã lỗi
- **Ghi chú**:

---

## UT-024-07: Chặn tài khoản không có quyền

## Thông tin tài liệu

- **Tiêu đề**: Tài khoản không có quyền không xem được chi tiết thiệp.
- **Ghi chú**: Kiểm tra ở tầng controller/policy.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết thiệp AI
- **Unit under test**: `AdminGeneratedCardController.GetById`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Trường hợp 1: request không có token.
  - Trường hợp 2: request có token nhưng không có quyền Admin.
- **Input**:

```http
GET /api/v1/admin/generated-cards/ca000000-0000-0000-0000-000000000001
```

- **Expected output**:

```text
Trường hợp chưa đăng nhập: HTTP 401.
Trường hợp không có quyền: HTTP 403.
Service không được gọi.
Không trả ảnh, thông tin thiệp, khách hàng hoặc Order liên quan.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận dữ liệu chi tiết thiệp AI chỉ được trả cho tài khoản có quyền.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-043
- **Section**: AC-006
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: Mã lỗi
- **Ghi chú**:

---

## UT-024-08: Đọc templateName từ snapshot

## Thông tin tài liệu

- **Tiêu đề**: API lấy tên template từ metadata của history.
- **Ghi chú**: Template có thể bị admin đổi sau này, nên màn hình chi tiết phải xem theo snapshot lúc tạo.

## Đơn vị kiểm thử

- **Module**: Admin - Chi tiết thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetById`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - History có metadata:

```json
{
  "card_template": {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "name": "Cổ điển"
  }
}
```

- **Input**:

```http
GET /api/v1/admin/generated-cards/ca000000-0000-0000-0000-000000000001
```

- **Expected output**:

```text
HTTP 200
templateName = "Cổ điển".
Nếu metadata sai format thì templateName = null.
API không lỗi toàn bộ chi tiết vì metadata template sai.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P2
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận template hiển thị đúng theo snapshot của lần tạo thiệp.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-043
- **Section**: AC-003
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-024
- **Section**: Mô hình dữ liệu
- **Ghi chú**:
