# STORY-040 — Khách hàng xem lịch sử các mẫu hoa Custom AI đã tạo

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã sử dụng chức năng Custom AI, tôi muốn xem lại các yêu cầu tạo mẫu hoa và các kết quả AI thuộc từng yêu cầu, để tiếp tục sử dụng yêu cầu và xem lại những mẫu hoa đã tạo. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Chức năng được truy cập từ mục **“Yêu cầu tạo mẫu hoa”** trên sidebar; không có trang History tách riêng. 
- Mỗi yêu cầu tạo mẫu hoa là **một nhóm**. 
- Một yêu cầu có thể có **0, 1 hoặc nhiều** kết quả AI có ảnh. 
- Mỗi lần generate có ảnh hợp lệ tạo **một item lịch sử**. 
- Generate không có ảnh **không tạo item**. 
- Danh sách yêu cầu và danh sách kết quả trong yêu cầu đều sắp xếp theo hoạt động/generate **gần nhất, mới nhất trước**.

---

## Conditions

### Preconditions
- Khách hàng đã đăng nhập.

### Trigger
> Khách hàng mở **“Yêu cầu tạo mẫu hoa”**.

---

## Flow

### Main Flow: MF — Xem danh sách yêu cầu

1. Backend lấy tất cả yêu cầu tạo mẫu hoa thuộc khách hàng hiện tại.
2. Hệ thống sắp xếp yêu cầu theo thời điểm generate gần nhất, mới nhất trước. Nếu yêu cầu chưa từng generate, hệ thống sử dụng thời điểm tạo yêu cầu để sắp xếp.
3. Mỗi card hiển thị thông tin yêu cầu, tổng số kết quả có ảnh hợp lệ và trạng thái của AI Job mới nhất nếu có. Trạng thái AI Job gồm: “Đang tạo”, “Đã tạo”, “Lỗi”. Nếu yêu cầu chưa từng có AI Job, hệ thống hiển thị “Chưa tạo”. Job đang chạy chưa được tính vào tổng số mẫu và chưa tạo History item.
4. Khách hàng chọn một yêu cầu.
5. Hệ thống hiển thị danh sách các kết quả có ảnh thuộc đúng yêu cầu, mới nhất trước.
6. Khách hàng chọn một kết quả để xem chi tiết.

---

### Alternative Flows

#### ALT-01 — Yêu cầu có nhiều kết quả
1. Yêu cầu có nhiều kết quả AI có ảnh.
2. Hệ thống hiển thị mọi kết quả có ảnh.
3. Hệ thống không ghi đè kết quả cũ.
4. Hệ thống sắp xếp kết quả mới nhất trước.

#### ALT-02 — Yêu cầu có job AI đang chạy ngầm
1. Job AI của yêu cầu đang được xử lý.
2. Khách hàng reload hoặc mở lại màn hình “Yêu cầu tạo mẫu hoa”.
3. Hệ thống hiển thị card yêu cầu với trạng thái “Đang tạo”.
4. Tổng số mẫu chỉ tính các kết quả đã có ảnh hợp lệ.
5. Job đang chạy chưa được hiển thị như một History item.
6. Các kết quả thành công trước đó, nếu có, vẫn được hiển thị bình thường.

---

### Exception Flows

#### EXC-01 — Không có yêu cầu
1. Khách hàng mở “Yêu cầu tạo mẫu hoa”.
2. Backend kiểm tra danh sách yêu cầu tạo mẫu hoa thuộc khách hàng hiện tại.
3. Hệ thống không tìm thấy yêu cầu nào.
4. Hệ thống hiển thị empty state.

#### EXC-02 — Không tải được dữ liệu
1. Khách hàng mở “Yêu cầu tạo mẫu hoa”.
2. Hệ thống thực hiện tải dữ liệu danh sách yêu cầu hoặc danh sách kết quả.
3. Hệ thống không tải được dữ liệu.
4. Hệ thống hiển thị error state.
5. Hệ thống cho phép khách hàng tải lại.

#### EXC-03 — Không có quyền
1. Khách hàng truy cập yêu cầu hoặc kết quả bằng ID/URL.
2. Backend kiểm tra quyền sở hữu của yêu cầu hoặc kết quả.
3. Backend phát hiện yêu cầu hoặc kết quả không thuộc khách hàng hiện tại.
4. Backend từ chối truy cập.
5. Backend không trả ảnh hoặc metadata.

#### EXC-04 — File ảnh không còn khả dụng
1. Khách hàng xem một item lịch sử từng có ảnh.
2. Hệ thống kiểm tra file ảnh tương ứng.
3. Hệ thống phát hiện file ảnh không còn khả dụng.
4. Hệ thống giữ History record và metadata.
5. Hệ thống hiển thị “Ảnh không còn khả dụng.”
6. Hệ thống không làm lỗi toàn bộ màn hình.

---

## Acceptance Criteria

### AC-001 – Hiển thị mọi yêu cầu
- **Given:** khách hàng có yêu cầu tạo mẫu hoa thuộc tài khoản mình
- **When:** mở “Yêu cầu tạo mẫu hoa”
- **Then:** hệ thống hiển thị yêu cầu dù có hoặc chưa có kết quả AI thành công.

### AC-003 – Lưu item có ảnh
- **Given:** một lần generate tạo được ảnh hợp lệ
- **When:** quá trình hoàn tất
- **Then:** hệ thống tạo đúng 01 item lịch sử thuộc đúng yêu cầu.

### AC-004 – Không có ảnh
- **Given:** một lần generate không tạo được ảnh output
- **When:** quá trình kết thúc
- **Then:** không tạo item lịch sử
- **And:** không tính vào tổng số mẫu.

### AC-005 – Sắp xếp yêu cầu
- **Given:** khách hàng có nhiều yêu cầu
- **When:** danh sách hiển thị
- **Then:** yêu cầu có hoạt động/generate gần nhất được xếp trước.

### AC-006 – Sắp xếp kết quả
- **Given:** một yêu cầu có nhiều kết quả
- **When:** xem chi tiết
- **Then:** kết quả generate gần nhất được xếp trước.

### AC-007 – File mất
- **Given:** item từng có ảnh nhưng file hiện không truy cập được
- **When:** khách hàng xem
- **Then:** record vẫn tồn tại
- **And:** hiển thị “Ảnh không còn khả dụng.”
- **And:** lỗi của file đó không làm lỗi toàn bộ danh sách hoặc màn hình.

### AC-008 – Ownership
- **Given:** yêu cầu/kết quả thuộc khách hàng khác
- **When:** user truy cập trực tiếp bằng ID/URL
- **Then:** backend từ chối
- **And:** không trả ảnh hoặc metadata.

### AC-009 – Hiển thị yêu cầu đang tạo sau khi reload
- **Given:** yêu cầu có job AI đang chạy ngầm
- **When:** khách hàng reload hoặc mở lại màn hình “Yêu cầu tạo mẫu hoa”
- **Then:** card yêu cầu hiển thị trạng thái “Đang tạo”
- **And:** tổng số mẫu chỉ tính các kết quả đã có ảnh hợp lệ
- **And:** job đang chạy chưa tạo History item
- **And:** các kết quả thành công trước đó vẫn được hiển thị.

### AC-010 – Không có yêu cầu
- **Given:** khách hàng không có yêu cầu tạo mẫu hoa nào
- **When:** mở “Yêu cầu tạo mẫu hoa”
- **Then:** Hệ thống hiển thị empty state cho biết khách hàng chưa có yêu cầu tạo mẫu hoa.

### AC-011 – Không tải được dữ liệu
- **Given:** hệ thống không tải được danh sách yêu cầu hoặc danh sách kết quả
- **When:** quá trình tải thất bại
- **Then:** Hệ thống hiển thị error state và cho phép khách hàng tải lại dữ liệu.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-095**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9d83c7a2-8d9f-41b8-833d-f4c6e550a1e9) | Nhóm theo yêu cầu | Xem lịch sử các mẫu hoa Custom AI đã tạo | Mỗi yêu cầu tạo mẫu hoa là một nhóm độc lập và có thể chứa nhiều kết quả AI. | Hệ thống hiển thị lịch sử mẫu hoa Custom AI. | Hệ thống nhóm các kết quả AI theo từng yêu cầu tạo mẫu hoa. | Không gộp kết quả của nhiều yêu cầu tạo mẫu hoa vào cùng một nhóm. | Đức Bình | STORY-040 | Draft | v0 | 2026-08-14 |
| [**BR-096**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5d2f87a0-b9f8-4a88-97a3-ca6c941f22e5) | Hiển thị yêu cầu | Xem lịch sử các mẫu hoa Custom AI đã tạo | Mọi yêu cầu thuộc khách hàng đều xuất hiện tại màn “Yêu cầu tạo mẫu hoa” | Khách hàng mở màn “Yêu cầu tạo mẫu hoa”. | Hệ thống hiển thị mọi yêu cầu thuộc khách hàng hiện tại. | N/A | Đức Bình | STORY-040 | Draft | v0 | 2026-08-14 |
| [**BR-097**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a616dc95-b28a-4370-94f5-dff3cddab2ea) | Điều kiện tạo item | Xem lịch sử các mẫu hoa Custom AI đã tạo | Chỉ lần generate có ảnh output hợp lệ mới tạo item lịch sử và được tính vào tổng số mẫu. | Một lần generate kết thúc. | Hệ thống tạo item lịch sử và tính vào tổng số mẫu nếu có ảnh output hợp lệ. | Generate không có ảnh output hợp lệ không tạo item lịch sử và không được tính vào tổng số mẫu. | Đức Bình | STORY-040 | Draft | v0 | 2026-08-14 |
| [**BR-098**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b8dabb0d-98cf-4142-a86e-7e74681705ec) | Thứ tự | Xem lịch sử các mẫu hoa Custom AI đã tạo | Danh sách yêu cầu được sắp xếp theo thời điểm generate gần nhất, mới nhất trước; yêu cầu chưa từng generate dùng thời điểm tạo. Danh sách kết quả được sắp xếp theo thời điểm generate, mới nhất trước. | Hệ thống hiển thị danh sách yêu cầu hoặc danh sách kết quả. | Yêu cầu có generate dùng thời điểm generate gần nhất để sắp xếp; yêu cầu chưa từng generate dùng thời điểm tạo. Kết quả được sắp xếp theo thời điểm generate giảm dần. | Không sắp xếp kết quả cũ trước kết quả mới hơn. | Đức Bình | STORY-040 | Draft | v0 | 2026-08-14 |
| [**BR-099**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c34f1cbc-902b-49e9-bfeb-b0cf83e585dd) | Bảo toàn record | Xem lịch sử các mẫu hoa Custom AI đã tạo | Mất/hỏng file sau khi từng generate thành công không xóa record và không đổi lần generate trước thành thất bại. | File ảnh của một item lịch sử không còn khả dụng. | Hệ thống giữ History record và metadata. | Không xóa record và không đổi lần generate trước thành thất bại. | Đức Bình | STORY-040 | Draft | v0 | 2026-08-14 |
| [**BR-100**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0fc0831c-0ef8-421f-8732-543580b05027) | Quyền sở hữu | Xem lịch sử các mẫu hoa Custom AI đã tạo | Khách hàng chỉ được xem yêu cầu và kết quả thuộc tài khoản của mình. | Khách hàng truy cập yêu cầu hoặc kết quả. | Backend kiểm tra quyền sở hữu trước khi trả dữ liệu. | Không trả ảnh hoặc metadata của yêu cầu/kết quả không thuộc khách hàng hiện tại. | Đức Bình | STORY-040 | Draft | v0 | 2026-08-14 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-095 | [Nhóm theo yêu cầu](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9d83c7a2-8d9f-41b8-833d-f4c6e550a1e9) |
| BR-096 | [Hiển thị yêu cầu](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5d2f87a0-b9f8-4a88-97a3-ca6c941f22e5) |
| BR-097 | [Điều kiện tạo item](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a616dc95-b28a-4370-94f5-dff3cddab2ea) |
| BR-098 | [Thứ tự](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b8dabb0d-98cf-4142-a86e-7e74681705ec) |
| BR-099 | [Bảo toàn record](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c34f1cbc-902b-49e9-bfeb-b0cf83e585dd) |
| BR-100 | [Quyền sở hữu](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/0fc0831c-0ef8-421f-8732-543580b05027) |

### Dependencies
- [**STORY-030**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/225510e4-396c-403b-9ae4-59f00859a2ce)
- [**STORY-033**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e1cdce1d-5295-4945-930a-95f9e8bee6b9)

---

## Non-Functional Requirements

- API danh sách phân trang phía server và không trả binary ảnh trong payload metadata.
- Mục tiêu **p95 ≤ 2 giây** với page size cấu hình, không tính tải ảnh storage.
- Ownership kiểm tra tại backend.
- UI có loading, empty và error state.
- Một file lỗi không được làm hỏng toàn bộ danh sách.

---

## Out of Scope

- Generate/tạo lại mẫu hoa AI.
- Tải xuống mẫu hoa.
- Xóa History.
- Checkout và Order.
