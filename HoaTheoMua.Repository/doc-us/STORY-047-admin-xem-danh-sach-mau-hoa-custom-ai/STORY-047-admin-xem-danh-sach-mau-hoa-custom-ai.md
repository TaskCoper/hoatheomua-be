# STORY-047 — Admin xem danh sách mẫu hoa Custom AI

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một Admin có quyền quản lý AI Custom, tôi muốn xem danh sách các mẫu hoa Custom AI được khách hàng tạo, để kiểm tra ảnh kết quả, Combo nguồn, khách hàng sở hữu và các Order liên quan. |
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
- Mỗi kết quả generate có ảnh output hợp lệ được hiển thị thành **đúng 01 item**.
- Một mẫu hoa vẫn chỉ xuất hiện một lần dù được liên kết với nhiều Order.
- Danh sách dùng để tra cứu nhanh. Thành phần Core, Support, Mockup nguồn, yêu cầu tùy chỉnh và Order liên kết được xem đầy đủ tại Chi tiết mẫu hoa.
- Danh sách mẫu hoa không hiển thị giá hoặc bill. Giá được xem tại Chi tiết Order tương ứng.
- **Tìm kiếm:** Tên khách hàng / Mã Order / Mã mẫu hoa.
- **Lọc:** Combo nguồn.

---

## Conditions

### Preconditions
- Người dùng đã đăng nhập Website quản trị.
- Người dùng có quyền xem dữ liệu AI Custom → Mẫu hoa.

### Trigger
> Admin chọn **AI Custom → Mẫu hoa** trên sidebar.

---

## Flow

### Main Flow: MF – Xem danh sách mẫu hoa

1. Admin chọn AI Custom → Mẫu hoa.
2. Backend kiểm tra quyền truy cập của Admin.
3. Hệ thống lấy danh sách các kết quả Custom AI có ảnh output hợp lệ.
4. Hệ thống sắp xếp danh sách theo ngày tạo mới nhất trước.
5. Mỗi item trong bảng hiển thị:
   - Preview ảnh mẫu hoa.
   - Mã mẫu hoa.
   - Khách hàng.
   - Combo nguồn: mã hoặc tên Combo.
   - Ngày tạo.
   - Thao tác.
6. Admin có thể tìm kiếm, lọc, thay đổi số item trên trang hoặc chuyển trang.
7. Admin chọn Preview để mở Image Lightbox.
8. Admin chọn mã mẫu hoa hoặc "Xem chi tiết" trong menu thao tác để mở Chi tiết mẫu hoa.

---

### Alternative Flows

#### ALT-01 — Tìm kiếm mẫu hoa
1. Admin nhập tên khách hàng, mã Order hoặc mã mẫu hoa.
2. Hệ thống trả về các mẫu hoa phù hợp mà Admin có quyền xem.
3. Khi tìm kiếm bằng mã Order, hệ thống trả về các mẫu hoa có liên kết với Order tương ứng; mã Order không bắt buộc hiển thị trên danh sách.
4. Admin có thể mở Chi tiết mẫu hoa để xem đầy đủ các Order liên kết.

#### ALT-02 — Lọc theo Combo
1. Admin mở bộ lọc Combo.
2. Admin chọn một hoặc nhiều Combo.
3. Hệ thống chỉ hiển thị kết quả được tạo từ các Combo đã chọn.
4. Admin có thể xóa từng điều kiện hoặc đặt lại bộ lọc.

#### ALT-03 — Thay đổi số item trên trang
1. Danh sách mặc định hiển thị 10 item trên một trang.
2. Admin chọn một trong các giá trị: 5, 10, 20, 30, 40 hoặc 50 item/trang.
3. Hệ thống tải lại danh sách theo số lượng đã chọn và đưa Admin về trang đầu tiên.

#### ALT-04 — Mẫu hoa liên kết nhiều Order
1. Mẫu hoa vẫn chỉ xuất hiện đúng 01 item trong danh sách.

#### ALT-05 — Xem nhanh ảnh
1. Admin chọn Preview tại danh sách.
2. Hệ thống mở Image Lightbox, hiển thị ảnh theo đúng tỷ lệ và cho phép đóng để quay lại vị trí trước đó.

---

### Exception Flows

#### EXC-01 — Không có dữ liệu
1. Chưa có mẫu hoa hoặc không có kết quả phù hợp với điều kiện tìm kiếm và bộ lọc.
2. Hệ thống hiển thị empty state.
3. Hệ thống không hiển thị dữ liệu mẫu, dữ liệu thiếu hoặc dữ liệu của điều kiện trước đó như kết quả hiện tại.
4. Admin có thể thay đổi điều kiện tìm kiếm, bộ lọc hoặc đặt lại bộ lọc để tải lại danh sách.

#### EXC-02 — Không tải được dữ liệu
1. Hệ thống không tải được danh sách do lỗi hệ thống, lỗi mạng hoặc API không phản hồi.
2. Hệ thống hiển thị error state.
3. Hệ thống không hiển thị dữ liệu thiếu, dữ liệu cũ hoặc dữ liệu không đầy đủ như kết quả hoàn chỉnh.
4. Hệ thống cho phép Admin tải lại danh sách.
5. Nếu Admin tải lại thành công, hệ thống hiển thị danh sách theo dữ liệu mới nhất được backend trả về.
6. Nếu vẫn không tải được dữ liệu, hệ thống tiếp tục giữ error state.

#### EXC-03 — Không có quyền truy cập
1. Người dùng không có quyền truy cập AI Custom hoặc Mẫu hoa.
2. Backend từ chối request danh sách.
3. Backend không trả ảnh, dữ liệu khách hàng, Combo, thành phần hoa, Order liên kết hoặc metadata nhạy cảm.
4. Hệ thống hiển thị trạng thái không có quyền truy cập.
5. Hệ thống không hiển thị dữ liệu danh sách đã tải trước đó nếu quyền truy cập không hợp lệ.

#### EXC-04 — File ảnh mẫu hoa không tải được
1. Một file ảnh mẫu hoa trong danh sách không tải được.
2. Hệ thống vẫn giữ History record và metadata của kết quả.
3. Hệ thống chỉ hiển thị trạng thái lỗi tại vùng Preview của item tương ứng.
4. Các item khác vẫn hiển thị bình thường nếu dữ liệu và ảnh của các item đó tải được.
5. Hệ thống không hiển thị file lỗi như một ảnh hợp lệ.
6. Hệ thống cho phép Admin tải lại trang để thử tải lại ảnh.
7. Hệ thống không tự động gọi AI để tạo lại ảnh.
8. Hệ thống không thay đổi dữ liệu mẫu hoa, History record hoặc Order liên kết.

---

## Acceptance Criteria

### AC-001 – Hiển thị danh sách mẫu hoa
- **Given:** Admin có quyền xem AI Custom
- **When:** Admin mở AI Custom → Mẫu hoa
- **Then:** hệ thống hiển thị mỗi kết quả Custom AI có ảnh hợp lệ thành đúng 01 item
- **And:** sắp xếp ngày tạo mới nhất trước.

### AC-002 – Mẫu hoa liên kết nhiều Order
- **Given:** một mẫu hoa liên kết với nhiều Order
- **When:** danh sách mẫu hoa hiển thị
- **Then:** mẫu hoa chỉ xuất hiện đúng 01 item.

### AC-003 – Tìm kiếm
- **Given:** danh sách có dữ liệu phù hợp
- **When:** Admin tìm theo tên khách hàng, mã Order hoặc mã mẫu hoa
- **Then:** hệ thống chỉ hiển thị các mẫu hoa thỏa điều kiện tìm kiếm.
- **And:** nếu tìm bằng mã Order, kết quả phải là mẫu hoa có liên kết với Order đó.

### AC-004 – Lọc theo Combo
- **Given:** Admin chọn một hoặc nhiều Combo
- **When:** áp dụng bộ lọc
- **Then:** hệ thống chỉ hiển thị các mẫu hoa được tạo từ Combo đã chọn.

### AC-005 – Phân trang
- **Given:** danh sách có nhiều hơn số item trên một trang
- **When:** Admin chọn 5, 10, 20, 30, 40 hoặc 50 item/trang
- **Then:** hệ thống phân trang theo số lượng đã chọn
- **And:** mặc định là 10 item/trang.

### AC-006 – Xem Lightbox
- **Given:** file ảnh còn khả dụng
- **When:** Admin chọn Preview
- **Then:** hệ thống mở Image Lightbox
- **And:** hiển thị ảnh đúng tỷ lệ.

### AC-007 – File ảnh không tải được
- **Given:** History record có ảnh output hợp lệ nhưng hệ thống không tải được file ảnh tại thời điểm hiển thị
- **When:** Admin xem danh sách
- **Then:** danh sách hiển thị trạng thái lỗi tại vùng ảnh
- **And:** cho phép Admin tải lại trang
- **And:** không tự động generate ảnh thay thế.
- **And:** các item khác trong danh sách vẫn hiển thị bình thường nếu dữ liệu và ảnh của chúng tải được.

### AC-008 – Không hiển thị giá
- **Given:** một mẫu hoa có thể liên kết với nhiều Order có giá khác nhau
- **When:** Admin xem danh sách
- **Then:** hệ thống không hiển thị giá hoặc bill snapshot
- **And:** Admin xem giá tại Chi tiết Order tương ứng.

### AC-009 – Phân quyền
- **Given:** người dùng không có quyền xem AI Custom
- **When:** người dùng gọi API danh sách
- **Then:** backend từ chối
- **And:** không trả ảnh, dữ liệu mẫu hoa hoặc thông tin khách hàng.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-144**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d40c4ad7-0723-43ab-ba2f-c52bf73b3cf1) | Một item theo kết quả AI | AI Custom - Mẫu hoa | Mỗi kết quả generate có ảnh output hợp lệ được hiển thị thành đúng 01 item, không phụ thuộc số Order liên kết. | Hệ thống hiển thị danh sách mẫu hoa Custom AI. | Hệ thống hiển thị mỗi kết quả generate có ảnh output hợp lệ thành đúng 01 item. | Không tạo nhiều item cho cùng một mẫu hoa dù mẫu hoa được liên kết với nhiều Order. | Đức Bình | STORY-047 | Draft | v0 | 2026-08-14 |
| [**BR-145**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/96489393-fd4e-4d3c-895a-5a0fd8d27bcd) | Thứ tự mặc định | AI Custom - Mẫu hoa | Danh sách mẫu hoa sắp xếp theo ngày tạo mới nhất trước. | Hệ thống hiển thị danh sách mẫu hoa. | Hệ thống sắp xếp danh sách mẫu hoa theo ngày tạo mới nhất trước. | N/A | Đức Bình | STORY-047 | Draft | v0 | 2026-08-14 |
| [**BR-146**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c57c1f24-ded5-4593-9b67-b049dedb4a4b) | Dữ liệu danh sách | AI Custom - Mẫu hoa | Danh sách chỉ hiển thị Preview, mã mẫu hoa, khách hàng, Combo nguồn, ngày tạo và thao tác. | Hệ thống hiển thị danh sách mẫu hoa. | Hệ thống chỉ hiển thị Preview, mã mẫu hoa, khách hàng, Combo nguồn, ngày tạo và thao tác. | N/A | Đức Bình | STORY-047 | Draft | v0 | 2026-08-14 |
| [**BR-150**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0ff0a844-39dd-4502-b928-f0b458cb3312) | Giá và bill | AI Custom - Mẫu hoa | Danh sách và Chi tiết mẫu hoa không hiển thị giá. Giá và bill snapshot thuộc từng Order và được xem tại Chi tiết Order. | Admin xem danh sách hoặc Chi tiết mẫu hoa. | Hệ thống không hiển thị giá hoặc bill snapshot. | Giá và bill snapshot được xem tại Chi tiết Order tương ứng. | Đức Bình | STORY-047, STORY-048 | Draft | v0 | 2026-08-14 |
| [**BR-151**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35519137-c90d-42ae-a6b5-32460cefb38f) | File ảnh không tải được | AI Custom - Mẫu hoa | Lỗi tải file ảnh không xóa History record, không đổi lần generate trước thành thất bại và không kích hoạt AI. | Hệ thống không tải được file ảnh mẫu hoa tại thời điểm hiển thị. | Hệ thống giữ History record, hiển thị trạng thái lỗi tại vùng ảnh và cho phép Admin tải lại trang. | Không kích hoạt AI để tạo lại ảnh và không thay đổi dữ liệu mẫu hoa, History record hoặc Order liên kết. | Đức Bình | STORY-047, STORY-048 | Draft | v0 | 2026-08-14 |
| [**BR-152**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e98ce2c8-f7f8-4ff9-aae4-4cf51ab5296b) | Điều hướng AI Custom | AI Custom - Mẫu hoa | Mục Mẫu hoa được hiển thị bên dưới nhóm AI Custom trên sidebar và chỉ hiển thị cho tài khoản có quyền tương ứng. | Người dùng truy cập Website quản trị. | Hệ thống hiển thị mục Mẫu hoa bên dưới nhóm AI Custom cho tài khoản có quyền tương ứng. | Không hiển thị mục Mẫu hoa cho tài khoản không có quyền tương ứng. | Đức Bình | STORY-047 | Draft | v0 | 2026-08-14 |
| [**BR-132**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/556f0aa7-fe97-49c4-82fb-d64b637f4577) | Quyền truy cập | AI Custom - Phân quyền | Backend chỉ cung cấp danh sách, chi tiết, ảnh và thông tin khách hàng cho Admin có quyền quản lý AI Custom. | Backend nhận request danh sách, chi tiết, ảnh hoặc thông tin khách hàng. | Backend kiểm tra quyền quản lý AI Custom trước khi cung cấp dữ liệu, bao gồm họ tên, số điện thoại và email của khách hàng nếu có. | Không cung cấp ảnh, thông tin khách hàng hoặc metadata nhạy cảm cho người dùng không có quyền quản lý AI Custom. | Đức Bình | STORY-047 | Draft | v0 | 2026-08-14 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-144 | [Một item theo kết quả AI](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d40c4ad7-0723-43ab-ba2f-c52bf73b3cf1) |
| BR-145 | [Thứ tự mặc định](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/96489393-fd4e-4d3c-895a-5a0fd8d27bcd) |
| BR-146 | [Dữ liệu danh sách](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c57c1f24-ded5-4593-9b67-b049dedb4a4b) |
| BR-150 | [Giá và bill](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0ff0a844-39dd-4502-b928-f0b458cb3312) |
| BR-151 | [File ảnh không tải được](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/35519137-c90d-42ae-a6b5-32460cefb38f) |
| BR-152 | [Điều hướng AI Custom](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e98ce2c8-f7f8-4ff9-aae4-4cf51ab5296b) |
| BR-132 | [Quyền truy cập](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/556f0aa7-fe97-49c4-82fb-d64b637f4577) |

### Dependencies
- [**STORY-039**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [**STORY-040**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/eed51ca8-012b-420e-a576-1eb053081d3e)

---

## Non-Functional Requirements

- API danh sách phải phân trang phía server.
- Danh sách **p95 ≤ 2 giây** với tối đa 50 item/trang trong điều kiện bình thường, không tính thời gian tải ảnh từ storage.
- API metadata không nhúng binary/Base64 và không expose đường dẫn storage nội bộ.
- Backend kiểm tra phân quyền trong mọi request, không chỉ dựa trên sidebar hoặc UI.
- UI có loading, empty và error state.
- Điều kiện tìm kiếm, bộ lọc và số item/trang được giữ khi Admin chuyển trang và quay lại từ trang chi tiết trong cùng phiên làm việc.
- Một file lỗi không làm hỏng toàn bộ danh sách.

---

## Out of Scope

- Generate, tạo lại, chỉnh sửa hoặc xóa mẫu hoa.
- Kiểm tra tồn kho hoặc trạng thái khả dụng hiện tại của Combo.
- Tải xuống mẫu hoa Custom AI.
- Thay đổi dữ liệu khách hàng.
- Thay đổi Order hoặc bill.
- Export Excel.
