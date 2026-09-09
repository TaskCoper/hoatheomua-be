# UT-029: Unit Tests - Customer lấy URL ảnh mẫu hoa AI

## Thông tin tài liệu

- **Tiêu đề**: Unit test cho API customer lấy URL ảnh hoa AI.
- **Ghi chú**: TDD-029 chỉ kiểm tra Backend trả URL; việc tải file trực tiếp do FE và Storage thực hiện.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-029
- **Phiên bản**: v0.1
- **Author (bắt buộc)**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner (bắt buộc)**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-07

---

## UT-029-01: Trả URL ảnh hợp lệ

## Thông tin tài liệu

- **Tiêu đề**: Customer nhận đúng URL của ảnh hoa AI thuộc mình.
- **Ghi chú**: Luồng chính của API.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Tải ảnh hoa AI
- **Unit under test (bắt buộc)**: `GeneratedFlower.Service.GetImageUrl`
- **Loại**: Happy
- **Precondition / Mock setup**:
  - JWT có `userId = customer-001`.
  - Generated Flower `flower-001` có cùng `user_id`, `is_deleted = false`.
  - `image_url = https://res.cloudinary.com/demo/flower-001.png`.
- **Input**:

```http
GET /api/v1/generated-flowers/flower-001/image-url
Authorization: Bearer <customer_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 200, response.isSuccess = true.
response.value.generatedFlowerId = flower-001.
response.value.imageUrl đúng bằng URL đã lưu.
Service không tải binary, không gọi AI và không trừ quota.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận customer nhận được URL để FE tải trực tiếp ảnh đã tạo.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-034
- **Section**: AC-001, AC-005
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-029
- **Section**: Endpoint #1 - Response
- **Ghi chú**:

---

## UT-029-02: Tải được kết quả cũ và mới bằng cùng API

## Thông tin tài liệu

- **Tiêu đề**: Không giới hạn API chỉ cho kết quả mới nhất.
- **Ghi chú**: Cùng một contract áp dụng cho mọi Generated Flower đã lưu.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Tải ảnh hoa AI
- **Unit under test (bắt buộc)**: `GeneratedFlower.Service.GetImageUrl`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - `flower-old` và `flower-new` cùng thuộc customer-001, đều chưa xóa và có URL khác nhau.
- **Input**:

```text
Gọi lần 1: GET /api/v1/generated-flowers/flower-old/image-url
Gọi lần 2: GET /api/v1/generated-flowers/flower-new/image-url
```

- **Expected output (bắt buộc)**:

```text
Cả hai lần đều HTTP 200.
Mỗi lần trả đúng URL theo ID được gửi.
Kết quả cũ không bị chặn vì không còn là kết quả mới nhất.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận customer tải được mọi kết quả hợp lệ từ lịch sử.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-034
- **Section**: AC-002, AC-004
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-029
- **Section**: Business Rules - URL ảnh
- **Ghi chú**:

---

## UT-029-03: Gọi lại nhiều lần không trừ quota

## Thông tin tài liệu

- **Tiêu đề**: Lấy URL lặp lại không tạo AI hoặc trừ lượt sử dụng.
- **Ghi chú**: API đọc dữ liệu thuần túy.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Tải ảnh hoa AI
- **Unit under test (bắt buộc)**: `GeneratedFlower.Service.GetImageUrl`
- **Loại**: Quirk
- **Precondition / Mock setup**:
  - `flower-001` hợp lệ, có URL.
  - Quota hiện tại được mock ở một giá trị cố định; không có dependency AI trong Service.
- **Input**:

```text
Gọi GET /api/v1/generated-flowers/flower-001/image-url ba lần liên tiếp.
```

- **Expected output (bắt buộc)**:

```text
Cả ba lần trả HTTP 200 và cùng imageUrl.
Không tạo Generated Flower mới.
Không thay đổi quota hoặc Client History.
Không gọi AI/Storage từ Backend.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P2
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận thao tác tải lặp lại không bị tính là lượt generate.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-034
- **Section**: AC-010, AC-011
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-029
- **Section**: Business Rules - không trừ quota
- **Ghi chú**:

---

## UT-029-04: Combo hoặc Mockup đã đổi trạng thái

## Thông tin tài liệu

- **Tiêu đề**: Nguồn hiện tại inactive hoặc bị xóa không chặn URL đã lưu.
- **Ghi chú**: API chỉ đọc Generated Flower, không revalidate nguồn.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Tải ảnh hoa AI
- **Unit under test (bắt buộc)**: `GeneratedFlower.Service.GetImageUrl`
- **Loại**: Quirk
- **Precondition / Mock setup**:
  - Generated Flower `flower-001` hợp lệ và có URL.
  - Combo hoặc Mockup nguồn đã inactive/soft-delete trong dữ liệu live.
- **Input**:

```http
GET /api/v1/generated-flowers/flower-001/image-url
```

- **Expected output (bắt buộc)**:

```text
HTTP 200 và trả imageUrl đã lưu.
Không truy vấn hoặc chặn theo trạng thái Combo/Mockup.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận lịch sử đã tạo vẫn dùng được khi nguồn live thay đổi.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-034
- **Section**: AC-012, AC-013
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-029
- **Section**: Business Rules - không kiểm tra nguồn live
- **Ghi chú**:

---

## UT-029-05: Không cho customer xem URL của người khác

## Thông tin tài liệu

- **Tiêu đề**: Ownership được kiểm tra trong cùng truy vấn.
- **Ghi chú**: Không làm lộ URL hoặc metadata của customer khác.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Tải ảnh hoa AI
- **Unit under test (bắt buộc)**: `GeneratedFlower.Service.GetImageUrl`
- **Loại**: Error
- **Precondition / Mock setup**:
  - `flower-002` thuộc customer-002, chưa xóa và có URL.
  - JWT hiện tại là customer-001.
- **Input**:

```http
GET /api/v1/generated-flowers/flower-002/image-url
Authorization: Bearer <customer_001_token>
```

- **Expected output (bắt buộc)**:

```text
HTTP 404 với mã FLOWER_IMAGE_NOT_FOUND.
Không trả imageUrl.
Không trả inputSnapshot hoặc thông tin storage.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận customer không thể tải ảnh bằng ID của tài khoản khác.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-034
- **Section**: AC-003
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-029
- **Section**: Mã lỗi - FLOWER_IMAGE_NOT_FOUND
- **Ghi chú**:

---

## UT-029-06: ID hoặc ảnh không hợp lệ

## Thông tin tài liệu

- **Tiêu đề**: Chặn ID rỗng, record không tồn tại và URL rỗng.
- **Ghi chú**: Các lỗi đều không trả URL.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Tải ảnh hoa AI
- **Unit under test (bắt buộc)**: `GeneratedFlower.Service.GetImageUrl`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Case A: ID là `Guid.Empty`.
  - Case B: ID không tồn tại hoặc record đã `is_deleted = true`.
  - Case C: record thuộc customer hiện tại nhưng `image_url` null/rỗng.
- **Input**:

```text
Case A: GET /api/v1/generated-flowers/00000000-0000-0000-0000-000000000000/image-url
Case B: GET /api/v1/generated-flowers/flower-missing/image-url
Case C: GET /api/v1/generated-flowers/flower-no-image/image-url
```

- **Expected output (bắt buộc)**:

```text
Case A: HTTP 400, mã INVALID_GENERATED_FLOWER_ID; không query DB.
Case B: HTTP 404, mã FLOWER_IMAGE_NOT_FOUND; không trả URL.
Case C: HTTP 404, mã FLOWER_IMAGE_NOT_FOUND; không trả URL.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale (bắt buộc)**: Xác nhận API không trả URL thiếu hoặc không hợp lệ.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-034
- **Section**: AC-014, AC-015
- **Ghi chú**: Với contract mới, Backend xử lý ID/URL rỗng; lỗi tải thực tế từ Storage do FE xử lý.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-029
- **Section**: Mã lỗi - INVALID_GENERATED_FLOWER_ID, FLOWER_IMAGE_NOT_FOUND
- **Ghi chú**:

---

## UT-029-07: Backend không stream hoặc chuẩn hóa file

## Thông tin tài liệu

- **Tiêu đề**: Service chỉ đọc URL, không gọi HttpClient/Storage để tải file.
- **Ghi chú**: Khóa quyết định kiến trúc tải trực tiếp từ Storage.

## Đơn vị kiểm thử

- **Module (bắt buộc)**: Customer - Tải ảnh hoa AI
- **Unit under test (bắt buộc)**: `GeneratedFlower.Service.GetImageUrl`
- **Loại**: Quirk
- **Precondition / Mock setup**:
  - Generated Flower hợp lệ có URL.
  - Mock `HttpClient`, Cloudinary hoặc storage client; không cấu hình dependency tải file cho Service.
- **Input**:

```http
GET /api/v1/generated-flowers/flower-001/image-url
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
- **Rationale (bắt buộc)**: Xác nhận Backend nhẹ tải và FE chịu trách nhiệm tải file trực tiếp.

## TEST_LINKS

**Link 1**
- **Loại**: TDD
- **Mã**: TDD-029
- **Section**: Ngoài phạm vi, Sequence Diagram
- **Ghi chú**:

