# UT-031: Unit Tests - Customer lấy URL ảnh thiệp đã tạo

## Thông tin tài liệu

- **Tiêu đề**: Unit test cho API trả URL ảnh thiệp.
- **Ghi chú**: TDD-031 dùng chung cho thiệp mở từ History và thiệp mở từ Order; Backend không truyền file binary.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-031
- **Phiên bản**: v0.1
- **Author (bắt buộc)**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-07

---

## UT-031-01: Trả URL thiệp hợp lệ

## Thông tin tài liệu

- **Tiêu đề**: Customer nhận đúng URL của thiệp thuộc mình.
- **Ghi chú**: Luồng chính của API.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Tải ảnh thiệp
- **Unit under test (bắt buộc)**: `GeneratedCard.Service.GetImageUrl`
- **Loại**: Happy
- **Precondition / Mock setup**:
  - JWT có `userId = customer-001`.
  - Generated Card C01 cùng user, `is_deleted = false`.
  - `image_url = https://res.cloudinary.com/demo/card-01.png`.
- **Input**:

```http
GET /api/v1/generated-cards/card-001/image-url
Authorization: Bearer <customer_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200, response.isSuccess = true.
response.value.generatedCardId = card-001.
response.value.imageUrl đúng bằng URL đã lưu.
Không trả binary hoặc stream ảnh.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận FE nhận được URL để tải đúng ảnh thiệp.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-041
- **Section**: AC-001
- **Ghi chú**: Contract mới trả URL thay vì Backend stream file.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-031
- **Section**: Endpoint #1 - Response
- **Ghi chú**:

---

## UT-031-02: Cùng API dùng cho History và Order

## Thông tin tài liệu

- **Tiêu đề**: Không cần `orderId` khi tải thiệp từ hai màn hình.
- **Ghi chú**: FE lấy cùng `generatedCardId` từ History hoặc OrderDetails.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Tải ảnh thiệp
- **Unit under test (bắt buộc)**: `GeneratedCard.Service.GetImageUrl`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - C01 thuộc customer-001 và có URL.
  - History và một OrderDetails đều chứa/tham chiếu C01.
  - Không cần mock truy vấn Order trong GeneratedCard Service.
- **Input**:

```text
Case A: FE gửi card-001 sau khi mở từ History.
Case B: FE gửi card-001 sau khi mở từ Order.
Cả hai đều gọi:
GET /api/v1/generated-cards/card-001/image-url
```

- **Expected output (bắt buộc)**:

```text
Cả hai case đều HTTP 200 và trả cùng imageUrl.
Request không có orderId.
Service không truy vấn Order hoặc kiểm tra liên kết Card–Order.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận một API tối ưu phục vụ cả hai điểm vào của FE.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-041
- **Section**: AC-001, AC-002, AC-003
- **Ghi chú**: Quyết định mới bỏ kiểm tra Order trong API URL.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-031
- **Section**: Business Rules - không cần orderId
- **Ghi chú**:

---

## UT-031-03: Tải lại nhiều lần không ảnh hưởng dữ liệu

## Thông tin tài liệu

- **Tiêu đề**: Gọi API lặp lại không tạo Card/History và không trừ quota.
- **Ghi chú**: API chỉ đọc URL đã lưu.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Tải ảnh thiệp
- **Unit under test (bắt buộc)**: `GeneratedCard.Service.GetImageUrl`
- **Loại**: Quirk
- **Precondition / Mock setup**:
  - C01 hợp lệ, có URL.
  - Không cấu hình dependency gọi AI/quota/storage trong Service.
- **Input**:

```text
Gọi GET /api/v1/generated-cards/card-001/image-url ba lần liên tiếp.
```

- **Expected output (bắt buộc)**:

```text
Cả ba lần HTTP 200 và cùng imageUrl.
Không tạo Generated Card hoặc Client History mới.
Không thay đổi quota, Card hoặc Order.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận tải lại thiệp không phải thao tác generate.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-041
- **Section**: AC-004, AC-005, AC-006
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-031
- **Section**: Business Rules - không gọi AI và không trừ quota
- **Ghi chú**:

---

## UT-031-04: Không trả URL của customer khác

## Thông tin tài liệu

- **Tiêu đề**: Ownership được kiểm tra trong truy vấn Card.
- **Ghi chú**: Không làm lộ URL hoặc metadata nhạy cảm.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Tải ảnh thiệp
- **Unit under test (bắt buộc)**: `GeneratedCard.Service.GetImageUrl`
- **Loại**: Error
- **Precondition / Mock setup**:
  - C02 thuộc customer-002, chưa xóa và có URL.
  - JWT hiện tại là customer-001.
- **Input**:

```http
GET /api/v1/generated-cards/card-002/image-url
Authorization: Bearer <customer_001_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 404 với mã CARD_IMAGE_NOT_FOUND.
Không trả imageUrl, rawImage hoặc metadata.
Không tiết lộ Card có tồn tại hay không.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận customer không thể tải ảnh bằng ID của tài khoản khác.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-041
- **Section**: AC-009
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-031
- **Section**: Mã lỗi - CARD_IMAGE_NOT_FOUND
- **Ghi chú**:

---

## UT-031-05: Card không tồn tại hoặc ảnh rỗng

## Thông tin tài liệu

- **Tiêu đề**: Không cung cấp URL khi Card không hợp lệ.
- **Ghi chú**: Bao gồm ID rỗng, Card bị xóa và `image_url` rỗng.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Tải ảnh thiệp
- **Unit under test (bắt buộc)**: `GeneratedCard.Service.GetImageUrl`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Case A: `Guid.Empty`.
  - Case B: ID không tồn tại hoặc Card `is_deleted = true`.
  - Case C: Card đúng user nhưng `image_url` null/rỗng.
- **Input**:

```text
Case A: GET /api/v1/generated-cards/00000000-0000-0000-0000-000000000000/image-url
Case B: GET /api/v1/generated-cards/card-missing/image-url
Case C: GET /api/v1/generated-cards/card-no-image/image-url
```

- **Expected output (bắt buộc)**:

```text
Case A: HTTP 400, mã INVALID_GENERATED_CARD_ID; không query DB.
Case B: HTTP 404, mã CARD_IMAGE_NOT_FOUND; không trả URL.
Case C: HTTP 404, mã CARD_IMAGE_NOT_FOUND; không trả URL.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận API không trả URL thiếu hoặc của tài nguyên không hợp lệ.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-041
- **Section**: AC-007, AC-008
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-031
- **Section**: Mã lỗi - INVALID_GENERATED_CARD_ID, CARD_IMAGE_NOT_FOUND
- **Ghi chú**:

---

## UT-031-06: Backend không stream hoặc xử lý lại ảnh

## Thông tin tài liệu

- **Tiêu đề**: Service chỉ trả URL và không gọi Storage.
- **Ghi chú**: Khóa quyết định kiến trúc tải trực tiếp từ Cloudinary/Core Storage.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Tải ảnh thiệp
- **Unit under test (bắt buộc)**: `GeneratedCard.Service.GetImageUrl`
- **Loại**: Quirk
- **Precondition / Mock setup**:
  - C01 hợp lệ có URL.
  - Mock HttpClient, Cloudinary hoặc storage client; Service không được inject dependency tải file.
- **Input**:

```http
GET /api/v1/generated-cards/card-001/image-url
```

- **Expected output (bắt buộc)**:

```text
HTTP 200 với response.value.imageUrl.
Không có request outbound từ Backend đến Storage.
Không có byte[] hoặc stream trong response.
Không đổi tên file, không ép PNG và không gắn lại logo.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận API nhẹ tải và FE chịu trách nhiệm tải ảnh trực tiếp.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-041
- **Section**: AC-001, AC-011, AC-012
- **Ghi chú**: Các quy tắc stream/tên file cũ được thay bằng contract trả URL.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-031
- **Section**: Ngoài phạm vi và Sequence Diagram
- **Ghi chú**:

