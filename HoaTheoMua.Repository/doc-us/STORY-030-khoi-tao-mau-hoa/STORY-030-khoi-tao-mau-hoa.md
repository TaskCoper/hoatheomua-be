# STORY-030 — Tạo yêu cầu tạo mẫu hoa

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã đăng nhập, tôi muốn tạo yêu cầu tạo mẫu hoa để lưu lại ý tưởng và các lựa chọn của mình, làm cơ sở cho AI tạo ra mẫu hoa sau này. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Khách hàng bắt đầu quy trình khởi tạo mẫu hoa bằng cách ấn chọn **"Tạo mẫu hoa"** trên một Combo Card.

- Hệ thống ghi nhận Combo đó là **Combo nguồn** của yêu cầu tạo mẫu hoa.

### Quy trình gồm 2 Step:

| Step | Tên | Nội dung |
|---|---|---|
| **Step 1** | Thông tin & Combo | Khách hàng nhập Tên yêu cầu tạo mẫu hoa. Hệ thống hiển thị thông tin Combo nguồn đã chọn. |
| **Step 2** | Mockup | Hệ thống hiển thị danh sách Mockup để khách hàng chọn. |

### Thông tin yêu cầu tạo mẫu hoa:

**Step 1 — Thông tin & Combo:**
- **Tên** — bắt buộc, từ 1 đến 100 ký tự (sau khi loại bỏ khoảng trắng đầu/cuối)
- Hình ảnh nguyên liệu của Combo đã chọn
- Tên combo, mô tả, thành phần nguyên liệu combo, giá combo

**Step 2 — Mockup:**
- Hình ảnh Mockup, phân trang

### Data Source

| Dữ liệu | Nguồn |
|---|---|
| **Mockup** | Core Database — nguồn chính để truy xuất và xác thực Mockup |
| **Combo** | Core Database — Backend phải lấy thông tin Combo từ Core Database |
| **Tồn kho Combo** | Nhanh.vn — kiểm tra tình trạng tồn kho hiện tại |
| **Yêu cầu tạo mẫu hoa** | Core Database |

---

## Conditions

### Preconditions
- Khách hàng đã đăng nhập.
- Tài khoản khách hàng đang hoạt động.
- Khách hàng có quyền tạo yêu cầu tạo mẫu hoa.
- Hệ thống đang hoạt động bình thường.

### Trigger
> Khách hàng ấn chọn **"Tạo mẫu hoa"** trên một Combo Card.

---

## Flow

### Main Flow

1. Khách hàng ấn chọn **"Tạo mẫu hoa"** trên một Combo Card.
2. Hệ thống ghi nhận Combo được chọn là **Combo nguồn** của quy trình.
3. Hệ thống lấy thông tin Combo nguồn từ Core Database.
4. Hệ thống hiển thị **Step 1 – Thông tin & Combo**, bao gồm trường Tên và thông tin Combo nguồn.
5. Khách hàng nhập Tên của yêu cầu tạo mẫu hoa.
6. Khách hàng ấn chọn **"Tiếp theo"**.
7. Hệ thống kiểm tra tính hợp lệ của dữ liệu Step 1.
8. Nếu dữ liệu hợp lệ, hệ thống chuyển sang **Step 2 – Mockup**.
9. Hệ thống truy xuất danh sách Mockup còn khả dụng từ Core Database.
10. Hệ thống hiển thị các Mockup dưới dạng **Mockup Card**.
11. Khách hàng xem và chọn duy nhất một Mockup.
12. Khách hàng ấn chọn **"Hoàn thành"**.
13. Hệ thống kiểm tra lại toàn bộ dữ liệu của yêu cầu.
14. Hệ thống kiểm tra lại trạng thái khả dụng của Combo từ Core Database.
15. Hệ thống kiểm tra lại trạng thái khả dụng của Mockup từ Core Database.
16. Nếu toàn bộ dữ liệu hợp lệ, hệ thống tạo một yêu cầu tạo mẫu hoa.
17. Hệ thống liên kết yêu cầu tạo mẫu hoa với: Tài khoản khách hàng hiện tại; Combo nguồn; Mockup đã chọn.
18. Hệ thống gán trạng thái ban đầu của yêu cầu là **Bản nháp**.
19. Hệ thống thông báo tạo yêu cầu tạo mẫu hoa thành công.
20. Hệ thống chuyển khách hàng đến màn hình **Chi tiết yêu cầu tạo mẫu hoa**.
21. Hệ thống hiển thị đầy đủ thông tin của yêu cầu vừa tạo.
22. Hệ thống hiển thị chức năng **"Tạo mẫu hoa bằng AI"**.
23. Hệ thống chưa gửi yêu cầu tạo mẫu hoa bằng AI cho đến khi khách hàng chủ động sử dụng chức năng này.

---

### Alternative Flows

#### ALT-01 — Quay lại bước trước
1. Khách hàng đang ở Step 2.
2. Khách hàng ấn chọn **"Quay lại"**.
3. Hệ thống chuyển khách hàng về Step trước đó.
4. Hệ thống giữ nguyên các thông tin và lựa chọn khách hàng đã thực hiện.
5. Khách hàng có thể chỉnh sửa dữ liệu và tiếp tục quy trình.

#### ALT-02 — Xem Preview Mockup
1. Khách hàng đang ở Step 2.
2. Khách hàng ấn chọn **"Xem Mockup"**.
3. Hệ thống hiển thị **Image Preview Modal** với hình ảnh Mockup kích thước lớn hơn.
4. Khách hàng có thể đóng Image Preview Modal và tiếp tục lựa chọn Mockup.

---

### Exception Flows

#### EXC-01 — Tên không hợp lệ
1. Khách hàng đang ở Step 1 – Thông tin & Combo.
2. Khách hàng để trống hoặc nhập Tên không hợp lệ.
3. Khách hàng ấn chọn **"Tiếp theo"**.
4. Hệ thống loại bỏ khoảng trắng ở đầu và cuối của Tên trước khi kiểm tra.
5. Hệ thống hiển thị lỗi tại trường Tên.
6. Hệ thống yêu cầu khách hàng bổ sung hoặc điều chỉnh thông tin.

#### EXC-02 — Lỗi khi lưu dữ liệu
1. Khách hàng đã cung cấp đầy đủ dữ liệu hợp lệ.
2. Khách hàng ấn chọn **"Hoàn thành"**.
3. Hệ thống gặp lỗi trong quá trình lưu.
4. Hệ thống không tạo yêu cầu tạo mẫu hoa.
5. Hệ thống thông báo: **"Tạo yêu cầu mẫu hoa thất bại. Vui lòng thử lại."**
6. Dữ liệu khách hàng đã nhập được giữ lại nếu có thể.
7. Khách hàng có thể thử lại.

#### EXC-03 — Combo không còn khả dụng
1. Khách hàng đang thực hiện quy trình tạo yêu cầu tạo mẫu hoa từ Combo Card.
2. Khách hàng ấn chọn **"Hoàn thành"**.
3. Hệ thống kiểm tra lại Combo nguồn từ Core Database.
4. Hệ thống phát hiện Combo không còn khả dụng.
5. Hệ thống không tạo yêu cầu tạo mẫu hoa.
6. Hệ thống thông báo: **"Combo đã chọn không còn khả dụng. Vui lòng chọn combo khác."**
7. Khách hàng được chuyển về danh sách Combo để chọn Combo khác.

#### EXC-04 — Mockup không còn khả dụng
1. Khách hàng đang ở Step 2 và đã chọn một Mockup.
2. Khách hàng ấn chọn **"Hoàn thành"**.
3. Hệ thống kiểm tra lại Mockup đã chọn từ Core Database.
4. Hệ thống phát hiện Mockup không còn khả dụng.
5. Hệ thống không tạo yêu cầu tạo mẫu hoa.
6. Hệ thống thông báo: **"Mockup đã chọn không còn khả dụng. Vui lòng chọn Mockup khác."**
7. Hệ thống tải lại danh sách Mockup còn khả dụng từ Core Database.
8. Khách hàng chọn Mockup khác và tiếp tục quy trình.

#### EXC-05 — Chưa chọn Mockup
1. Khách hàng đang ở Step 2.
2. Khách hàng chưa chọn Mockup.
3. Khách hàng ấn chọn **"Hoàn thành"**.
4. Hệ thống không tạo yêu cầu tạo mẫu hoa.
5. Hệ thống yêu cầu khách hàng chọn một Mockup trước khi tiếp tục.

---

## Acceptance Criteria

### AC-001 — Hiển thị form tạo yêu cầu tạo mẫu hoa
- **Given:** Khách hàng đã đăng nhập và đang xem một Combo Card.
- **When:** Khách hàng ấn chọn **"Tạo mẫu hoa"**.
- **Then:** Hệ thống phải hiển thị form tạo yêu cầu tạo mẫu hoa.

### AC-002 — Hiển thị trường bắt buộc
- **Given:** Khách hàng đang ở Step 1 – Thông tin.
- **When:** Form tạo yêu cầu tạo mẫu hoa được hiển thị.
- **Then:** Trường Tên phải được đánh dấu bắt buộc.

### AC-003 — Validate trường bắt buộc
- **Given:** Khách hàng đang ở Step 1 và một hoặc nhiều trường bắt buộc chưa có giá trị hợp lệ.
- **When:** Khách hàng ấn chọn **"Tiếp theo"**.
- **Then:** Hệ thống không được chuyển sang Step 2.
- **And:** Hệ thống phải hiển thị lỗi tại các trường tương ứng.

### AC-004 — Validate giới hạn ký tự
- **Given:** Khách hàng đang nhập Tên tại Step 1.
- **When:** Tên vượt quá 100 ký tự sau khi loại bỏ khoảng trắng ở đầu và cuối.
- **Then:** Hệ thống phải hiển thị lỗi tại trường Tên.
- **And:** Hệ thống không cho phép tiếp tục cho đến khi Tên hợp lệ.

### AC-005 — Giữ dữ liệu khi quay lại
- **Given:** Khách hàng đã hoàn thành ít nhất một Step trong quy trình.
- **When:** Khách hàng ấn chọn **"Quay lại"**.
- **Then:** Hệ thống phải hiển thị lại các thông tin và lựa chọn đã thực hiện trước đó.
- **And:** Khách hàng có thể thay đổi dữ liệu trước đó.

### AC-006 — Hiển thị Combo nguồn
- **Given:** Khách hàng đang ở Step 1.
- **When:** Hệ thống hiển thị danh sách hình ảnh nguyên liệu và thông tin của Combo.
- **Then:** Hệ thống phải lấy và hiển thị đúng thông tin Combo mà khách hàng đã chọn từ Core Database.
- **And:** Combo này phải được xác định là Combo nguồn của yêu cầu tạo mẫu hoa.

### AC-007 — Hiển thị danh sách Mockup
- **Given:** Khách hàng đã hoàn thành Step 1.
- **When:** Hệ thống chuyển sang Step 2.
- **Then:** Hệ thống phải truy xuất danh sách Mockup còn khả dụng từ Core Database.
- **And:** Hệ thống phải hiển thị các Mockup còn khả dụng dưới dạng Mockup Card.

### AC-008 — Chọn duy nhất một Mockup
- **Given:** Khách hàng đang ở Step 2.
- **When:** Khách hàng chọn một Mockup.
- **Then:** Hệ thống phải ghi nhận Mockup đó là Mockup được chọn.
- **And:** Tại một thời điểm chỉ được có duy nhất một Mockup được chọn.
- **And:** Khi khách hàng chọn Mockup khác, Mockup trước đó phải được bỏ chọn.

### AC-009 — Bắt buộc chọn Mockup
- **Given:** Khách hàng đang ở Step 2 và chưa chọn Mockup.
- **When:** Khách hàng ấn chọn **"Hoàn thành"**.
- **Then:** Hệ thống không được tạo yêu cầu tạo mẫu hoa.
- **And:** Hệ thống phải yêu cầu khách hàng chọn một Mockup.

### AC-010 — Kiểm tra Combo và Mockup trước khi tạo
- **Given:** Khách hàng đã nhập đầy đủ dữ liệu hợp lệ và đã chọn một Mockup.
- **When:** Khách hàng ấn chọn **"Hoàn thành"**.
- **Then:** Hệ thống phải kiểm tra lại trạng thái khả dụng của Combo từ nguồn dữ liệu bên thứ ba.
- **And:** Hệ thống phải kiểm tra lại trạng thái khả dụng của Mockup từ Core Database.
- **And:** Nếu Combo hoặc Mockup không còn khả dụng, hệ thống không được tạo yêu cầu tạo mẫu hoa.

### AC-011 — Tạo yêu cầu tạo mẫu hoa thành công
- **Given:** Các thông tin bắt buộc hợp lệ, Combo nguồn còn khả dụng và khách hàng đã chọn một Mockup còn khả dụng.
- **When:** Khách hàng ấn chọn **"Hoàn thành"**.
- **Then:** Hệ thống phải tạo một yêu cầu tạo mẫu hoa mới.
- **And:** Yêu cầu phải được liên kết với tài khoản khách hàng hiện tại.
- **And:** Yêu cầu phải được liên kết với Combo nguồn.
- **And:** Yêu cầu phải được liên kết với Mockup đã chọn.
- **And:** Hệ thống phải thông báo tạo thành công.
- **And:** Hệ thống phải chuyển khách hàng đến màn hình Chi tiết yêu cầu tạo mẫu hoa.

### AC-012 — Trạng thái ban đầu của yêu cầu tạo mẫu hoa
- **Given:** Yêu cầu tạo mẫu hoa được tạo thành công.
- **When:** Hệ thống lưu yêu cầu tạo mẫu hoa.
- **Then:** Yêu cầu tạo mẫu hoa phải có trạng thái ban đầu là **Bản nháp**.

### AC-013 — Hiển thị chi tiết yêu cầu tạo mẫu hoa
- **Given:** Yêu cầu tạo mẫu hoa đã được tạo thành công.
- **When:** Khách hàng được chuyển đến màn hình Chi tiết yêu cầu tạo mẫu hoa.
- **Then:** Hệ thống phải hiển thị đầy đủ thông tin của yêu cầu vừa được tạo.
- **And:** Hệ thống phải hiển thị đúng các thông tin đã lưu, gồm: Tên, ngày tạo, trạng thái, dịp sử dụng, phong cách, ngân sách dự kiến, kích thước, ghi chú, Combo và Mockup.
- **And:** Hệ thống phải hiển thị nút **"Tạo mẫu hoa bằng AI"**.
- **And:** Hệ thống chưa tự động gửi yêu cầu tạo mẫu hoa bằng AI cho đến khi khách hàng chủ động ấn nút này.

### AC-014 — Phân trang danh sách Mockup
- **Given:** Mockup khả dụng.
- **When:** Khách hàng truy cập Step 2.
- **Then:** Hệ thống hiển thị tối đa **4 Mockup trên mỗi trang**.
- **And:** Hiển thị chức năng chuyển trang.
- **And:** Giữ nguyên Mockup đã chọn khi khách hàng chuyển trang.

### AC-015 — Validate Tên
- **Given:** Khách hàng đang ở Step 1 – Thông tin & Combo.
- **When:** Khách hàng nhập Tên và ấn chọn **"Tiếp theo"**.
- **Then:** Hệ thống phải loại bỏ khoảng trắng ở đầu và cuối của Tên trước khi kiểm tra.
- **And:** Tên phải là chuỗi có độ dài từ 1 đến 100 ký tự.
- **And:** Hệ thống không được chấp nhận null, chuỗi rỗng, chuỗi chỉ chứa khoảng trắng hoặc chuỗi vượt quá 100 ký tự.
- **And:** Nếu Tên không hợp lệ, hệ thống không được chuyển sang Step 2 và phải hiển thị lỗi tại trường Tên.

### AC-016 — Validate giới hạn ký tự
- **Given:** Khách hàng đang nhập thông tin tại Step 1 – Thông tin.
- **When:** Tên vượt quá 100 ký tự sau khi loại bỏ khoảng trắng ở đầu và cuối.
- **Then:** Hệ thống phải hiển thị lỗi tại trường tương ứng.
- **And:** Hệ thống không cho phép khách hàng chuyển sang Step 2 cho đến khi dữ liệu hợp lệ.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-068**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b1017f3c-b500-4d66-83a3-46b3a488d37f) | Giới hạn ký tự | Khởi tạo mẫu hoa | Tên yêu cầu tạo mẫu hoa phải tuân thủ giới hạn ký tự được quy định. | Khách hàng nhập thông tin tại Step 1. | Hệ thống loại bỏ khoảng trắng ở đầu và cuối. Tên phải có độ dài từ 1 đến 100 ký tự. | Không chấp nhận null, chuỗi rỗng, chuỗi chỉ chứa khoảng trắng hoặc chuỗi vượt quá 100 ký tự. | Đức Bình | STORY-030 | Draft | v0 | 2026-08-14 |
| [**BR-069**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a925fb05-3c5e-4592-8e0c-22a516df58a2) | Validation thông tin bắt buộc | Khởi tạo mẫu hoa | Tên là thông tin bắt buộc của yêu cầu tạo mẫu hoa. | Khách hàng ấn chọn "Tiếp theo" tại Step 1. | Hệ thống loại bỏ khoảng trắng ở đầu và cuối của Tên, sau đó kiểm tra Tên có độ dài từ 1 đến 100 ký tự. | Không chấp nhận null, chuỗi rỗng, chuỗi chỉ chứa khoảng trắng hoặc chuỗi vượt quá 100 ký tự. Nếu Tên không hợp lệ, hệ thống không chuyển sang Step 2. | Đức Bình | STORY-030 | Draft | v0 | 2026-08-14 |
| [**BR-070**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e85f0e09-9b6a-49a8-b62d-ccc467c2bb94) | Combo nguồn của yêu cầu tạo mẫu hoa | Khởi tạo mẫu hoa | Mỗi yêu cầu tạo mẫu hoa được tạo từ chức năng "Tạo mẫu hoa" phải được liên kết với một Combo nguồn. | Khách hàng chọn "Tạo mẫu hoa" trên một Combo Card. | Hệ thống ghi nhận Combo được chọn là Combo nguồn của yêu cầu tạo mẫu hoa. | Không cho phép hoàn thành yêu cầu tạo mẫu hoa nếu không xác định được Combo nguồn hoặc Combo nguồn không còn khả dụng. | Đức Bình | STORY-030 | Draft | v0 | 2026-08-14 |
| [**BR-071**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/30761eaa-4df8-4e44-8d1e-efaba44782bf) | Một yêu cầu tạo mẫu hoa chỉ có một Combo | Khởi tạo mẫu hoa | Một yêu cầu tạo mẫu hoa chỉ được liên kết với duy nhất một Combo tại một thời điểm. | Combo nguồn được ghi nhận cho yêu cầu tạo mẫu hoa. | Hệ thống chỉ lưu một Combo nguồn cho yêu cầu. | Không cho phép một yêu cầu tạo mẫu hoa đồng thời liên kết với nhiều Combo. | Đức Bình | STORY-030 | Draft | v0 | 2026-08-14 |
| [**BR-072**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2371ded1-c7f4-42a2-98a4-1c4f71d8095c) | Quyền sở hữu yêu cầu tạo mẫu hoa | Khởi tạo mẫu hoa | Mỗi yêu cầu tạo mẫu hoa phải thuộc về một khách hàng cụ thể. | Yêu cầu tạo mẫu hoa được tạo. | Hệ thống phải liên kết yêu cầu với tài khoản khách hàng đang đăng nhập. | Không cho phép tồn tại yêu cầu tạo mẫu hoa không có chủ sở hữu. | Đức Bình | STORY-030 | Draft | v0 | 2026-08-14 |
| [**BR-076**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c5e7d260-7968-449d-b0a6-8de5602056d0) | Nguồn và điều kiện hiển thị Mockup | Khởi tạo mẫu hoa | Danh sách Mockup tại Step 2 được lấy từ Core Database. | Khách hàng chuyển đến Step 2. | Danh sách Mockup được lấy từ Core Database. Mỗi trang hiển thị 4 card. Hệ thống duy trì Mockup đã chọn khi khách hàng chuyển trang. | Mockup không còn khả dụng không được hiển thị. | Đức Bình | STORY-030 | Draft | v0 | 2026-08-14 |
| [**BR-077**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c67cdf93-5604-438a-a247-e5195bf0e46d) | Một yêu cầu tạo mẫu hoa chỉ có một Mockup | Khởi tạo mẫu hoa | Một yêu cầu tạo mẫu hoa chỉ được liên kết với duy nhất một Mockup tại một thời điểm. | Khách hàng chọn Mockup tại Step 2. | Hệ thống ghi nhận Mockup được chọn. | Khi khách hàng chọn Mockup khác, Mockup trước đó phải được bỏ chọn. | Đức Bình | STORY-030 | Draft | v0 | 2026-08-14 |
| [**BR-078**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4e2249f4-842f-4159-a04c-295904f50ec3) | Mockup bắt buộc khi hoàn thành | Khởi tạo mẫu hoa | Yêu cầu tạo mẫu hoa phải có một Mockup được chọn trước khi được tạo. | Khách hàng ấn chọn "Hoàn thành". | Hệ thống kiểm tra Mockup đã chọn. | Nếu chưa có Mockup, hệ thống không tạo yêu cầu và yêu cầu khách hàng chọn một Mockup. | Đức Bình | STORY-030 | Draft | v0 | 2026-08-14 |
| [**BR-079**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/71fd8ff3-1fbd-4ab8-969b-1256d2e70618) | Nguồn dữ liệu Combo | Khởi tạo mẫu hoa | Combo sử dụng trong quy trình khởi tạo mẫu hoa phải được lấy và xác thực từ Core Database. | Hệ thống cần hiển thị hoặc kiểm tra Combo nguồn. | Backend phải sử dụng Core Database làm nguồn xác thực thông tin Combo. | Không sử dụng dữ liệu Combo lưu tạm ở frontend làm nguồn xác thực cuối cùng. | Đức Bình | STORY-030 | Draft | v0 | 2026-08-14 |
| [**BR-080**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/25e0fc0c-5927-4bab-8a46-86ff656fe108) | Trạng thái ban đầu | Khởi tạo mẫu hoa | Yêu cầu tạo mẫu hoa mới được tạo phải có trạng thái ban đầu là Bản nháp. | Yêu cầu tạo mẫu hoa được tạo thành công. | Hệ thống gán trạng thái Bản nháp. | STORY-030 không tự động chuyển trạng thái sang trạng thái tạo AI. | Đức Bình | STORY-030 | Draft | v0 | 2026-08-14 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-068 | [Giới hạn ký tự](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b1017f3c-b500-4d66-83a3-46b3a488d37f) |
| BR-069 | [Validation thông tin bắt buộc](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a925fb05-3c5e-4592-8e0c-22a516df58a2) |
| BR-070 | [Combo nguồn của yêu cầu tạo mẫu hoa](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e85f0e09-9b6a-49a8-b62d-ccc467c2bb94) |
| BR-071 | [Một yêu cầu tạo mẫu hoa chỉ có một Combo](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/30761eaa-4df8-4e44-8d1e-efaba44782bf) |
| BR-072 | [Quyền sở hữu yêu cầu tạo mẫu hoa](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2371ded1-c7f4-42a2-98a4-1c4f71d8095c) |
| BR-076 | [Nguồn và điều kiện hiển thị Mockup](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c5e7d260-7968-449d-b0a6-8de5602056d0) |
| BR-077 | [Một yêu cầu tạo mẫu hoa chỉ có một Mockup](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c67cdf93-5604-438a-a247-e5195bf0e46d) |
| BR-078 | [Mockup bắt buộc khi hoàn thành](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4e2249f4-842f-4159-a04c-295904f50ec3) |
| BR-079 | [Nguồn dữ liệu Combo](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/71fd8ff3-1fbd-4ab8-969b-1256d2e70618) |
| BR-080 | [Trạng thái ban đầu](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/25e0fc0c-5927-4bab-8a46-86ff656fe108) |

### Dependencies
- [**STORY-003**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5dee32c7-a632-40c1-ac0f-df2c78ce0ded) — Xem danh sách Combo thành phần

---

## Non-Functional Requirements

- API tạo yêu cầu tạo mẫu hoa có thời gian phản hồi **p95 ≤ 1.5 giây** (không tính thời gian bất khả kháng từ hệ thống bên thứ ba nếu team có cách đo tách riêng dependency).
- Validation trên giao diện phải phản hồi trong vòng **≤ 300 ms**.
- Một thao tác **"Hoàn thành"** chỉ được tạo tối đa **một** yêu cầu tạo mẫu hoa.
- Việc khách hàng ấn **"Hoàn thành"** nhiều lần liên tiếp không được tạo duplicate ngoài ý muốn.
- Không được tồn tại yêu cầu tạo mẫu hoa ở trạng thái dữ liệu lưu dở dang.
- Backend phải kiểm tra lại Combo từ nguồn dữ liệu bên thứ ba thay vì tin trực tiếp `comboId` hoặc thông tin Combo từ frontend.
- Backend phải xác thực khách hàng đã đăng nhập trước khi tạo yêu cầu.

---

## Out of Scope

- Xem danh sách combo
- Tạo mới hoặc chỉnh sửa Combo
- Tạo mới hoặc chỉnh sửa Mockup
- Quản lý Hoa
- Thực hiện sinh mẫu hoa bằng AI
- Chỉnh sửa kết quả mẫu hoa AI
- Tải xuống mẫu hoa AI
