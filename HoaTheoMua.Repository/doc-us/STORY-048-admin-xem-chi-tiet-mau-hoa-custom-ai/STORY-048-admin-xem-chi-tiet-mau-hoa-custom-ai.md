# STORY-048 — Admin xem chi tiết lịch sử mẫu hoa Custom AI

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý AI Custom, tôi muốn xem thông tin chi tiết các mẫu hoa Custom AI được khách hàng tạo, để kiểm tra ảnh kết quả, Combo nguồn, thành phần hoa, yêu cầu tùy chỉnh, khách hàng sở hữu và các Order liên quan. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hồ Hoàng Nam |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Chức năng được truy cập tại **AI Custom → Mẫu hoa** trên sidebar của Website quản trị.
- Thành phần Core, Support, Mockup nguồn, yêu cầu tùy chỉnh và Order liên kết được xem đầy đủ tại Chi tiết mẫu hoa.
- Chi tiết mẫu hoa không hiển thị giá hoặc bill. Giá được xem tại Chi tiết Order tương ứng.

---

## Conditions

### Preconditions
- Người dùng đã đăng nhập Website quản trị.
- Người dùng có quyền xem dữ liệu AI Custom → Mẫu hoa.

### Trigger
> Admin mở trực tiếp Chi tiết mẫu hoa bằng mã mẫu hợp lệ.
> Hoặc Admin chọn mã mẫu hoa hoặc “Xem chi tiết” trong menu thao tác từ danh sách mẫu hoa.

---

## Flow

### Main Flow: MF – Xem Chi tiết mẫu hoa

1. Backend kiểm tra quyền truy cập và kết quả mẫu hoa.
2. Hệ thống hiển thị:
   - Ảnh kết quả mẫu hoa.
   - Mã mẫu hoa.
   - Thông tin khách hàng: họ tên, số điện thoại và email nếu có.
   - Mã, tên và thông tin Combo nguồn.
   - Danh sách thành phần Core và số lượng từng loại tại thời điểm tạo.
   - Danh sách thành phần Support và số lượng từng loại tại thời điểm tạo.
   - Mockup nguồn được sử dụng.
   - Yêu cầu hoặc ghi chú tùy chỉnh.
   - Ngày tạo.
   - Trạng thái file.
   - Danh sách đầy đủ các Order liên kết.
3. Admin có thể chọn ảnh kết quả hoặc Mockup để mở Image Lightbox.
4. Nếu có Order liên kết, Admin có thể chọn mã Order để mở Chi tiết Order.

---

### Alternative Flows

#### ALT-01 — Mẫu hoa liên kết nhiều Order
1. Trang Chi tiết mẫu hoa hiển thị toàn bộ Order liên kết.
2. Mỗi Order giữ thông tin bill riêng tại Chi tiết Order.

#### ALT-02 — Xem nhanh ảnh
1. Admin chọn ảnh tại trang chi tiết.
2. Hệ thống mở Image Lightbox, hiển thị ảnh theo đúng tỷ lệ và cho phép đóng để quay lại vị trí trước đó.

---

### Exception Flows

#### EXC-01 — Không tải được dữ liệu
1. Khi hệ thống không tải được dữ liệu chi tiết mẫu hoa do lỗi hệ thống, lỗi mạng hoặc API không phản hồi, hệ thống hiển thị error state.
2. Hệ thống không hiển thị dữ liệu thiếu, dữ liệu cũ hoặc dữ liệu không đầy đủ như kết quả hoàn chỉnh.
3. Hệ thống cho phép Admin tải lại dữ liệu.
4. Nếu Admin tải lại thành công, hệ thống hiển thị Chi tiết mẫu hoa theo dữ liệu mới nhất được backend trả về.
5. Nếu vẫn không tải được dữ liệu, hệ thống tiếp tục giữ error state.

#### EXC-02 — Không có quyền truy cập
1. Khi người dùng không có quyền truy cập AI Custom hoặc không có quyền xem Chi tiết mẫu hoa, backend từ chối request.
2. Backend không trả ảnh, dữ liệu khách hàng, Combo, thành phần hoa, Order liên kết hoặc metadata nhạy cảm.
3. Hệ thống hiển thị trạng thái không có quyền truy cập.
4. Hệ thống không hiển thị dữ liệu chi tiết đã tải trước đó nếu quyền truy cập không hợp lệ.
5. Admin không thể mở ảnh, xem thông tin khách hàng hoặc truy cập Order liên kết từ mẫu hoa này.

#### EXC-03 — Không tìm thấy mẫu hoa
1. Khi mã mẫu hoa không tồn tại hoặc Admin không thể truy cập mẫu hoa đó, backend không trả dữ liệu chi tiết.
2. Hệ thống hiển thị trạng thái tài nguyên không tồn tại hoặc không thể truy cập.
3. Hệ thống không hiển thị ảnh, thông tin khách hàng, Combo, thành phần hoa, yêu cầu tùy chỉnh hoặc Order liên kết.
4. Hệ thống cho phép Admin quay lại danh sách mẫu hoa.
5. Hệ thống không tự động chuyển Admin sang một mẫu hoa khác.

#### EXC-04 — File ảnh mẫu hoa không tải được
1. Hệ thống giữ History record và metadata của kết quả.
2. Admin vẫn được mở Chi tiết mẫu hoa.
3. Trang chi tiết hiển thị trạng thái lỗi tại vùng ảnh.
4. Hệ thống cho phép Admin tải lại trang.
5. Hệ thống không hiển thị ảnh lỗi như ảnh hợp lệ.
6. Hệ thống không tự động gọi AI để tạo lại ảnh.
7. Hệ thống không thay đổi dữ liệu mẫu hoa, History record hoặc Order liên kết.

---

## Acceptance Criteria

### AC-001 - Mẫu hoa liên kết nhiều Order
- **Given:** một mẫu hoa liên kết với nhiều Order
- **When:** Admin xem Chi tiết mẫu hoa
- **Then:** Chi tiết mẫu hoa hiển thị đầy đủ các Order liên kết.

### AC-002 - Xem Lightbox
- **Given:** file ảnh còn khả dụng
- **When:** Admin chọn ảnh tại trang chi tiết
- **Then:** hệ thống mở Image Lightbox
- **And:** hiển thị ảnh đúng tỷ lệ.

### AC-003 - Xem Chi tiết mẫu hoa
- **Given:** Admin có quyền và mẫu hoa tồn tại
- **When:** Admin chọn mã mẫu hoặc “Xem chi tiết”
- **Then:** hệ thống hiển thị dữ liệu khách hàng, Combo nguồn, Core, Support, Mockup, yêu cầu tùy chỉnh và Order liên kết.

### AC-004 - File ảnh không tải được
- **Given:** History record có ảnh output hợp lệ nhưng hệ thống không tải được file ảnh tại thời điểm hiển thị
- **When:** Admin xem chi tiết
- **Then:** trang chi tiết vẫn hiển thị metadata
- **And:** hiển thị trạng thái lỗi tại vùng ảnh
- **And:** cho phép Admin tải lại trang
- **And:** không tự động generate ảnh thay thế.

### AC-005 - Không hiển thị giá
- **Given:** một mẫu hoa có thể liên kết với nhiều Order có giá khác nhau
- **When:** Admin xem Chi tiết mẫu hoa
- **Then:** hệ thống không hiển thị giá hoặc bill snapshot
- **And:** Admin xem giá tại Chi tiết Order tương ứng.

### AC-006 - Phân quyền
- **Given:** người dùng không có quyền xem AI Custom
- **When:** người dùng gọi API chi tiết
- **Then:** backend từ chối
- **And:** không trả ảnh, dữ liệu mẫu hoa hoặc thông tin khách hàng.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-147**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d7ba22e1-39f3-4c7a-8033-fd13c480478a) | Dữ liệu chi tiết | AI Custom - Mẫu hoa | Thành phần Core, Support, Mockup nguồn, yêu cầu tùy chỉnh, trạng thái file và danh sách Order liên kết chỉ hiển thị đầy đủ tại Chi tiết mẫu hoa. | Admin mở Chi tiết mẫu hoa. | Hệ thống hiển thị đầy đủ thành phần Core, Support, Mockup nguồn, yêu cầu tùy chỉnh, trạng thái file và danh sách Order liên kết. | N/A | N/A | STORY-048 | Draft | v0 | N/A |
| [**BR-148**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/71698060-d5a9-4f17-8009-8dfe848db5b9) | Dữ liệu nguồn tại thời điểm tạo | AI Custom - Mẫu hoa | Trang chi tiết hiển thị dữ liệu Combo, Core, Support và Mockup đã được ghi nhận cho lần generate tương ứng; không dùng trạng thái tồn kho hiện tại để thay đổi dữ liệu lịch sử. | Admin xem Chi tiết mẫu hoa. | Trang chi tiết hiển thị dữ liệu Combo, Core, Support và Mockup đã được ghi nhận cho lần generate tương ứng. | Không dùng trạng thái tồn kho hiện tại để thay đổi dữ liệu lịch sử. | N/A | STORY-048 | Draft | v0 | N/A |
| [**BR-149**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/96d99b5e-1045-4ec3-a8a8-4f91cd38e2ff) | Không hiển thị trạng thái Combo | AI Custom - Mẫu hoa | Danh sách và chi tiết chỉ hiển thị mã/tên và thông tin Combo nguồn; không hiển thị hoặc đánh giá trạng thái khả dụng hiện tại của Combo trong US này. | Admin xem danh sách hoặc Chi tiết mẫu hoa. | Hệ thống chỉ hiển thị mã/tên và thông tin Combo nguồn. | Không hiển thị hoặc đánh giá trạng thái khả dụng hiện tại của Combo trong US này. | N/A | STORY-048 | Draft | v0 | N/A |
| [**BR-150**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0ff0a844-39dd-4502-b928-f0b458cb3312) | Giá và bill | AI Custom - Mẫu hoa | Danh sách và Chi tiết mẫu hoa không hiển thị giá. Giá và bill snapshot thuộc từng Order và được xem tại Chi tiết Order. | Admin xem danh sách hoặc Chi tiết mẫu hoa. | Hệ thống không hiển thị giá hoặc bill snapshot. | Giá và bill snapshot được xem tại Chi tiết Order tương ứng. | N/A | STORY-047, STORY-048 | Draft | v0 | N/A |
| [**BR-151**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35519137-c90d-42ae-a6b5-32460cefb38f) | File ảnh không tải được | AI Custom - Mẫu hoa | Lỗi tải file ảnh không xóa History record, không đổi lần generate trước thành thất bại và không kích hoạt AI. | Hệ thống không tải được file ảnh mẫu hoa tại thời điểm hiển thị. | Hệ thống giữ History record, hiển thị trạng thái lỗi tại vùng ảnh và cho phép Admin tải lại trang. | Không kích hoạt AI để tạo lại ảnh và không thay đổi dữ liệu mẫu hoa, History record hoặc Order liên kết. | N/A | STORY-047, STORY-048 | Draft | v0 | N/A |
| [**BR-132**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/556f0aa7-fe97-49c4-82fb-d64b637f4577) | Quyền truy cập | AI Custom - Phân quyền | Backend chỉ cung cấp danh sách, chi tiết, ảnh và thông tin khách hàng cho Admin có quyền quản lý AI Custom. | Backend nhận request danh sách, chi tiết, ảnh hoặc thông tin khách hàng. | Backend kiểm tra quyền quản lý AI Custom trước khi cung cấp dữ liệu, bao gồm họ tên, số điện thoại và email của khách hàng nếu có. | Không cung cấp ảnh, thông tin khách hàng hoặc metadata nhạy cảm cho người dùng không có quyền quản lý AI Custom. | Đức Bình | STORY-048 | Draft | v0 | 2026-08-14 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-147 | [Dữ liệu chi tiết](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d7ba22e1-39f3-4c7a-8033-fd13c480478a) |
| BR-148 | [Dữ liệu nguồn tại thời điểm tạo](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/71698060-d5a9-4f17-8009-8dfe848db5b9) |
| BR-149 | [Không hiển thị trạng thái Combo](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/96d99b5e-1045-4ec3-a8a8-4f91cd38e2ff) |
| BR-150 | [Giá và bill](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0ff0a844-39dd-4502-b928-f0b458cb3312) |
| BR-151 | [File ảnh không tải được](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35519137-c90d-42ae-a6b5-32460cefb38f) |
| BR-132 | [Quyền truy cập](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/556f0aa7-fe97-49c4-82fb-d64b637f4577) |

### Dependencies
- [**STORY-030**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [**STORY-033**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9)
- [**STORY-039**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [**STORY-040**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eed51ca8-012b-420e-a576-1eb053081d3e)

---

## Non-Functional Requirements

- Chi tiết mẫu hoa **p95 ≤ 2 giây** trong điều kiện bình thường, không tính thời gian tải ảnh.
- API metadata không nhúng binary/Base64 và không expose đường dẫn storage nội bộ.
- Backend kiểm tra phân quyền trong mọi request, không chỉ dựa trên sidebar hoặc UI.
- UI có loading, empty và error state.

---

## Out of Scope

- Generate, tạo lại, chỉnh sửa hoặc xóa mẫu hoa.
- Kiểm tra tồn kho hoặc trạng thái khả dụng hiện tại của Combo.
- Tải xuống mẫu hoa Custom AI.
- Thay đổi dữ liệu khách hàng.
- Thay đổi Order hoặc bill.
- Export Excel.
