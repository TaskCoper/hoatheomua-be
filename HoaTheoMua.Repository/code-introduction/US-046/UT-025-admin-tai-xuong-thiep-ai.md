# UT-025: Unit Tests - Admin tải xuống thiệp AI

## Thông tin tài liệu

- **Tiêu đề**: Unit test cho API Admin lấy thông tin tải xuống ảnh thiệp AI.
- **Ghi chú**: Mỗi mục UT bên dưới kiểm tra một nhánh xử lý của TDD-025.

## Metadata quản trị tài liệu

- **Mã tài liệu**: UT-025
- **Phiên bản**: v0.1
- **Author**: Codex
- **Reviewer**:
- **Approver**: Chưa chỉ định
- **Owner**: Hoàng Thị Khánh Linh
- **Cập nhật gần nhất**: 2026-09-07

---

## UT-025-01: Lấy thông tin tải thiệp AI thành công

## Thông tin tài liệu

- **Tiêu đề**: Admin nhận đúng link và tên file của thiệp AI.
- **Ghi chú**: Kiểm tra luồng chính khi History, Generated Card và link ảnh đều hợp lệ.

## Đơn vị kiểm thử

- **Module**: Admin - Tải ảnh thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetDownloadUrl`
- **Loại**: Happy
- **Precondition / Mock setup**:
  - Có History `id = history-001`, `type = card`, `is_deleted = false`.
  - History có `output_id = ca000000-0000-0000-0000-000000000001`.
  - Có Generated Card cùng ID, `is_deleted = false`.
  - `history.user_id = generated_card.user_id`.
  - `generated_cards.image_url = "https://res.cloudinary.com/demo/card-01.png"`.
- **Input**:

```http
GET /api/v1/admin/generated-cards/ca000000-0000-0000-0000-000000000001/download
```

- **Expected output**:

```text
HTTP 200
generatedCardId = ca000000-0000-0000-0000-000000000001.
imageUrl = "https://res.cloudinary.com/demo/card-01.png".
fileName khớp mẫu thiep_ca00000000000000000000000000000001_[yyyyMMdd_HHmmss].png.
contentType = "image/png".
Không tạo hoặc cập nhật dữ liệu.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận endpoint trả đúng thông tin cần thiết để FE tải ảnh thiệp AI.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-046
- **Section**: AC-001
- **Ghi chú**: Xác nhận tải đúng file và dùng mã thiệp trong tên file.

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: BƯỚC 3 - Ví dụ 1
- **Ghi chú**:

---

## UT-025-02: Tạo tên file theo giờ Việt Nam

## Thông tin tài liệu

- **Tiêu đề**: Tên file có đúng mã thiệp, thời gian Việt Nam và đuôi PNG.
- **Ghi chú**: Mã thiệp là Guid nên không phát sinh ký tự không an toàn trong tên file.

## Đơn vị kiểm thử

- **Module**: Admin - Tải ảnh thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetDownloadUrl`
- **Loại**: Determinism
- **Precondition / Mock setup**:
  - Có thiệp và History hợp lệ như UT-025-01.
  - Thời điểm UTC dùng trong test tương ứng `2026-09-07 14:30:00` tại múi giờ Việt Nam.
- **Input**:

```http
GET /api/v1/admin/generated-cards/ca000000-0000-0000-0000-000000000001/download
```

- **Expected output**:

```text
HTTP 200
fileName = "thiep_ca00000000000000000000000000000001_20260907_143000.png".
Tên file kết thúc bằng .png.
Tên file không chứa ký tự không an toàn.
Tên file không sử dụng mã Order.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận BR-141 và AC-005 về tên file an toàn theo múi giờ Việt Nam.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-046
- **Section**: AC-005, BR-141
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: Business Rules - Tên file
- **Ghi chú**:

---

## UT-025-03: Tải lại không tạo dữ liệu và không trừ quota

## Thông tin tài liệu

- **Tiêu đề**: Gọi API nhiều lần chỉ trả lại ảnh đã có.
- **Ghi chú**: Kiểm tra thao tác tải không bị hiểu nhầm thành một lần generate.

## Đơn vị kiểm thử

- **Module**: Admin - Tải ảnh thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetDownloadUrl`
- **Loại**: Quirk
- **Precondition / Mock setup**:
  - Có thiệp và History hợp lệ như UT-025-01.
  - Trước test có 1 Generated Card, 1 History và quota khách hàng còn 2 lượt.
  - Mock AI service và quota service để theo dõi số lần được gọi.
- **Input**:

```text
Gọi GetDownloadUrl cùng generatedCardId ba lần liên tiếp.
```

- **Expected output**:

```text
Cả ba lần đều trả cùng generatedCardId và imageUrl.
Vẫn chỉ có 1 Generated Card và 1 History.
Quota vẫn còn 2 lượt.
AI service không được gọi.
Không thêm, sửa hoặc xóa Order.
```

## Phân loại và trách nhiệm

- **Suite**: REGRESSION
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận tải lại không tạo History, không gọi AI và không tiêu hao quota.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-046
- **Section**: AC-002, AC-003, BR-140, BR-143
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: Business Rules - Không thay đổi dữ liệu
- **Ghi chú**:

---

## UT-025-04: Thiệp liên kết Order vẫn tải theo mã thiệp

## Thông tin tài liệu

- **Tiêu đề**: Order liên quan không ảnh hưởng link và tên file tải.
- **Ghi chú**: Có thể tải từ danh sách hoặc trang chi tiết mà không cần đọc Order.

## Đơn vị kiểm thử

- **Module**: Admin - Tải ảnh thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetDownloadUrl`
- **Loại**: Branch
- **Precondition / Mock setup**:
  - Có thiệp và History hợp lệ như UT-025-01.
  - Thiệp được tham chiếu trong hai Order khác nhau.
- **Input**:

```http
GET /api/v1/admin/generated-cards/ca000000-0000-0000-0000-000000000001/download
```

- **Expected output**:

```text
HTTP 200
Trả đúng imageUrl của Generated Card.
fileName chứa generatedCardId và không chứa bất kỳ orderId nào.
Service không cần Order để xác định file tải.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P2
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận tải ảnh dựa trên thiệp, không phụ thuộc Order liên quan.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-046
- **Section**: ALT-02, ALT-03
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: Business Rules - Không phụ thuộc Order
- **Ghi chú**:

---

## UT-025-05: generatedCardId rỗng

## Thông tin tài liệu

- **Tiêu đề**: API chặn mã thiệp bằng Guid.Empty.
- **Ghi chú**: Service phải dừng trước khi truy vấn dữ liệu.

## Đơn vị kiểm thử

- **Module**: Admin - Tải ảnh thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetDownloadUrl`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Không cần dữ liệu thiệp.
- **Input**:

```http
GET /api/v1/admin/generated-cards/00000000-0000-0000-0000-000000000000/download
```

- **Expected output**:

```text
HTTP 400
code = INVALID_GENERATED_CARD_ID.
Không query History hoặc Generated Card.
Không trả imageUrl.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận mã thiệp không hợp lệ bị chặn trước khi xử lý.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-046
- **Section**: EXC-01
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: Mã lỗi - INVALID_GENERATED_CARD_ID
- **Ghi chú**:

---

## UT-025-06: Không tìm thấy thiệp AI hợp lệ

## Thông tin tài liệu

- **Tiêu đề**: API không trả link khi thiệp hoặc History không hợp lệ.
- **Ghi chú**: Bao gồm không tồn tại, bị xóa logic, sai type hoặc ownership không khớp.

## Đơn vị kiểm thử

- **Module**: Admin - Tải ảnh thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetDownloadUrl`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Chạy riêng từng bộ dữ liệu: không có Generated Card; Card bị xóa; History bị xóa; History không có `type = card`; hoặc `history.user_id != generated_card.user_id`.
- **Input**:

```http
GET /api/v1/admin/generated-cards/ffffffff-ffff-ffff-ffff-ffffffffffff/download
```

- **Expected output**:

```text
HTTP 404
code = GENERATED_CARD_NOT_FOUND.
Không trả imageUrl, fileName hoặc metadata của thiệp khác.
Không thay đổi History hiện có.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận chỉ cặp History và Generated Card hợp lệ mới được cung cấp link tải.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-046
- **Section**: EXC-01
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: Mã lỗi - GENERATED_CARD_NOT_FOUND
- **Ghi chú**:

---

## UT-025-07: Ảnh thiệp không còn khả dụng

## Thông tin tài liệu

- **Tiêu đề**: API không trả link khi imageUrl không có giá trị.
- **Ghi chú**: History vẫn được giữ nguyên và hệ thống không tạo ảnh thay thế.

## Đơn vị kiểm thử

- **Module**: Admin - Tải ảnh thiệp AI
- **Unit under test**: `AdminGeneratedCard.Service.GetDownloadUrl`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Có History type `card` và Generated Card hợp lệ.
  - Chạy riêng ba giá trị `image_url = null`, `image_url = ""` và `image_url = "   "`.
  - Ghi nhận số History và quota trước khi gọi.
- **Input**:

```http
GET /api/v1/admin/generated-cards/ca000000-0000-0000-0000-000000000002/download
```

- **Expected output**:

```text
HTTP 400
code = CARD_IMAGE_UNAVAILABLE.
Không trả imageUrl hoặc fileName.
History vẫn tồn tại và không bị chỉnh sửa.
Không gọi AI tạo ảnh thay thế.
Quota không thay đổi.
```

## Phân loại và trách nhiệm

- **Suite**: FULL
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận ảnh không khả dụng không làm mất History và không kích hoạt tạo ảnh mới.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-046
- **Section**: EXC-04, BR-140, BR-143
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: Mã lỗi - CARD_IMAGE_UNAVAILABLE
- **Ghi chú**:

---

## UT-025-08: Chặn người dùng không có quyền tải

## Thông tin tài liệu

- **Tiêu đề**: API không cung cấp link ảnh cho người dùng không có quyền.
- **Ghi chú**: Kiểm tra ở tầng Controller và authorization policy.

## Đơn vị kiểm thử

- **Module**: Admin - Tải ảnh thiệp AI
- **Unit under test**: `AdminGeneratedCardController.GetDownloadUrl`
- **Loại**: Error
- **Precondition / Mock setup**:
  - Trường hợp 1: request không có token.
  - Trường hợp 2: request có token nhưng không có quyền Admin/tải thiệp AI.
  - Có thiệp AI hợp lệ để chắc chắn kết quả bị chặn bởi quyền.
- **Input**:

```http
GET /api/v1/admin/generated-cards/ca000000-0000-0000-0000-000000000001/download
```

- **Expected output**:

```text
Chưa đăng nhập: HTTP 401.
Không có quyền: HTTP 403.
Service không được gọi.
Không trả imageUrl, fileName, đường dẫn storage hoặc metadata thiệp.
```

## Phân loại và trách nhiệm

- **Suite**: SMOKE
- **Priority**: P1
- **Owner**: Hoàng Thị Khánh Linh
- **Rationale**: Xác nhận chỉ Admin có quyền mới nhận được thông tin tải ảnh.

## TEST_LINKS

**Link 1**
- **Loại**: User Story
- **Mã**: STORY-046
- **Section**: AC-004, EXC-02, BR-142
- **Ghi chú**:

**Link 2**
- **Loại**: TDD
- **Mã**: TDD-025
- **Section**: Mã lỗi - UNAUTHORIZED, FORBIDDEN
- **Ghi chú**:
