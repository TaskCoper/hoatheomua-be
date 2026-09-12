# STORY-003 — Xem danh sách combo

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng, tôi muốn xem danh sách các combo thành phần hiện có trên hệ thống để tôi có thể tra cứu và tham khảo các combo phù hợp với nhu cầu của mình. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hồ Hoàng Nam |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Khách hàng có thể xem và tra cứu các combo thành phần ngay tại trang Sản phẩm để tham khảo các combo phù hợp với nhu cầu. Hệ thống hiển thị danh sách combo dưới dạng Combo Card và hỗ trợ tìm kiếm, lọc và phân trang. Danh sách chỉ hiển thị các Combo đang hoạt động (`is_active = true`). Đối với Combo đang hoạt động nhưng có Core hết hàng, Combo vẫn được hiển thị trong danh sách và được thể hiện với trạng thái “Hết hàng” để khách hàng nhận biết. Khách hàng không bắt buộc phải đăng nhập để xem danh sách Combo. Bộ lọc gồm: Danh mục (Hoa Lan Chúc Mừng, Sinh nhật, Khai trương,...) và Khoảng giá.

---

## Conditions

### Preconditions
- Hệ thống đang hoạt động bình thường.
- Hệ thống đã có dữ liệu combo.
- Các combo đáp ứng điều kiện hiển thị đã được cấu hình trên hệ thống.
- Khách hàng có thể truy cập màn hình danh sách combo.
- Khách hàng không bắt buộc phải đăng nhập để xem danh sách Combo.

### Trigger
> Khách hàng truy cập trang Sản phẩm và xem khu vực danh sách Combo.

---

## Flow

### Main Flow

1. Khách hàng truy cập trang Sản phẩm.
2. Hệ thống tải danh sách combo.
3. Hệ thống hiển thị các combo dưới dạng **Combo Card**.
4. Mỗi Combo Card hiển thị các thông tin cơ bản của combo theo quy định.
5. Khách hàng xem danh sách combo.
6. Khách hàng có thể chuyển sang các trang khác để tiếp tục xem danh sách.

---

### Alternative Flows

#### ALT-01 — Tìm kiếm combo
1. Khách hàng nhập từ khóa vào ô tìm kiếm.
2. Hệ thống tìm kiếm combo dựa trên từ khóa.
3. Hệ thống hiển thị danh sách kết quả phù hợp.

#### ALT-02 — Lọc combo
1. Khách hàng chọn một hoặc nhiều điều kiện lọc.
2. Hệ thống lọc danh sách theo các điều kiện đã chọn.
3. Hệ thống hiển thị các combo phù hợp.
4. Khách hàng có thể thay đổi từng điều kiện lọc.
5. Khách hàng có thể đặt lại toàn bộ bộ lọc để quay về danh sách mặc định.

#### ALT-03 — Kết hợp tìm kiếm và bộ lọc
1. Khách hàng nhập từ khóa tìm kiếm.
2. Khách hàng áp dụng một hoặc nhiều điều kiện lọc.
3. Hệ thống tìm các combo thỏa mãn từ khóa và các điều kiện lọc.
4. Hệ thống hiển thị kết quả phù hợp.

#### ALT-04 — Phân trang
1. Số lượng combo lớn hơn giới hạn hiển thị trên một trang.
2. Hệ thống hiển thị chức năng phân trang.
3. Khách hàng chọn trang tiếp theo, trang trước hoặc một trang cụ thể.
4. Hệ thống tải và hiển thị dữ liệu của trang tương ứng.
5. Điều kiện tìm kiếm và bộ lọc hiện tại được giữ nguyên khi chuyển trang.

---

### Exception Flows

#### EXC-01 — Không có combo
- Hệ thống không có combo đáp ứng điều kiện hiển thị.
- Hệ thống hiển thị trạng thái rỗng và thông báo phù hợp: *“Hiện chưa có combo.”*

#### EXC-02 — Không có kết quả tìm kiếm hoặc lọc
- Khách hàng thực hiện tìm kiếm hoặc áp dụng bộ lọc.
- Không có combo phù hợp với điều kiện.
- Hệ thống hiển thị thông báo: *“Không tìm thấy combo phù hợp.”*
- Khách hàng có thể thay đổi từ khóa hoặc điều kiện lọc.

#### EXC-03 — Không thể tải danh sách
- Hệ thống không thể lấy dữ liệu combo.
- Hệ thống hiển thị trạng thái lỗi tải danh sách Combo và cung cấp thao tác "Thử lại".
- Không hiển thị thông tin kỹ thuật nội bộ cho khách hàng.

#### EXC-04 — Hình ảnh Combo không tải được
- Hình ảnh của một combo không thể tải.
- Hệ thống hiển thị hình ảnh mặc định hoặc placeholder.
- Các thông tin còn lại của Combo Card vẫn được hiển thị bình thường.

---

## Acceptance Criteria

### AC-001 — Hiển thị danh sách combo
- **Given:** Hệ thống có các combo đáp ứng điều kiện hiển thị.
- **When:** Khách hàng truy cập màn hình combo.
- **Then:** Hệ thống phải hiển thị danh sách combo.
- **And:** Danh sách phải được trình bày dưới dạng Combo Card theo thiết kế.

### AC-002 — Hiển thị thông tin Combo Card
- **Given:** Danh sách combo được tải thành công.
- **When:** Hệ thống hiển thị Combo Card.
- **Then:** Mỗi Combo Card phải hiển thị các thông tin tối thiểu: Hình ảnh, Tên combo, Mô tả ngắn nếu có.

### AC-003 — Tìm kiếm combo
- **Given:** Khách hàng đang xem danh sách combo.
- **When:** Khách hàng nhập từ khóa tìm kiếm.
- **Then:** Hệ thống phải hiển thị các combo phù hợp với từ khóa.
- **And:** Khi xóa từ khóa tìm kiếm, hệ thống phải hiển thị lại danh sách phù hợp với các điều kiện lọc hiện tại.

### AC-004 — Lọc combo
- **Given:** Khách hàng đang ở màn hình danh sách.
- **When:** Khách hàng áp dụng một hoặc nhiều điều kiện lọc.
- **Then:** Hệ thống phải hiển thị các combo đáp ứng các điều kiện đã chọn.
- **And:** Khách hàng phải có thể thay đổi hoặc đặt lại bộ lọc.

### AC-005 — Kết hợp tìm kiếm và lọc
- **Given:** Khách hàng đã nhập từ khóa và áp dụng ít nhất một điều kiện lọc.
- **When:** Hệ thống cập nhật danh sách.
- **Then:** Danh sách phải chỉ hiển thị những combo đáp ứng cả từ khóa tìm kiếm và điều kiện lọc.

### AC-006 — Phân trang
- **Given:** Số lượng combo lớn hơn số lượng được phép hiển thị trên một trang.
- **When:** Khách hàng chuyển sang trang khác.
- **Then:** Hệ thống phải hiển thị đúng dữ liệu của trang được chọn.
- **And:** Các điều kiện tìm kiếm và lọc hiện tại phải được giữ nguyên.

### AC-007 — Không có kết quả
- **Given:** Không có combo phù hợp với từ khóa hoặc bộ lọc.
- **When:** Hệ thống xử lý yêu cầu.
- **Then:** Hệ thống phải hiển thị trạng thái không có kết quả.
- **And:** Không được hiển thị một khu vực danh sách trống mà không có thông báo.

### AC-008 — Xử lý lỗi dữ liệu
- **Given:** Hệ thống không thể lấy danh sách combo.
- **When:** Khách hàng truy cập hoặc tải lại màn hình.
- **Then:** Hệ thống phải hiển thị trạng thái lỗi phù hợp.
- **And:** Khách hàng phải có thể chọn Thử lại.
- **And:** Không hiển thị thông tin kỹ thuật nội bộ.

### AC-009 — Xử lý lỗi hình ảnh
- **Given:** Hình ảnh của combo không thể tải thành công.
- **When:** Combo Card được hiển thị.
- **Then:** Hệ thống phải sử dụng hình ảnh mặc định hoặc placeholder.
- **And:** Không được làm ảnh hưởng tới bố cục của Combo Card.

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-006 | [Điều kiện hiển thị Combo](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1a8d331-5e0c-4ed0-ab5b-bde958797d06) |
| BR-007 | [Thông tin hiển thị Combo Card](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/970f5370-3829-4a22-b14a-d28e9131831d) |
| BR-008 | [Tìm kiếm combo](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/29a75d56-8f54-46fa-b195-76290abdf116) |
| BR-009 | [Lọc combo theo danh mục và giá](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ec306904-4195-4119-bf82-b0bbee0b1dde) |
| BR-010 | [Phân trang danh sách combo](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/148e9a8a-76ba-4187-8360-119c0599e11b) |

---

## Non-Functional Requirements

- API lấy danh sách combo phải có thời gian phản hồi ≤ 2 giây ở p95 trong điều kiện tải bình thường.
- API search/filter/pagination phải có thời gian phản hồi ≤ 2.5 giây ở p95.
- Hình ảnh Combo Card phải sử dụng lazy loading.
- Tỷ lệ request thành công của API danh sách phải đạt ≥ 99% trong điều kiện hoạt động bình thường.
- Một ảnh tải lỗi không được làm lỗi toàn bộ danh sách.
- Nếu một ảnh không tải được trong vòng 5 giây, hệ thống phải hiển thị placeholder.
- API public chỉ trả về các trường dữ liệu được phép hiển thị cho khách hàng.

---

## Out of Scope

- Chọn combo.
- Đăng nhập / Đăng ký.
- Tạo bản nháp.
