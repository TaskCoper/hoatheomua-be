# STORY-036 — Khách hàng tạo lại thiệp cá nhân hóa bằng AI

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã đăng nhập, tôi muốn tạo lại từ một thiệp đã có, để có một mẫu thiệp cá nhân hóa phù hợp với nhu cầu của mình. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Khách hàng có thể chọn **"Tạo lại"** tại màn Chi tiết thiệp đã tạo.

- Mỗi request Tạo lại hợp lệ phải đồng thời tuân theo **giới hạn 03 lượt** generate thiệp theo mẫu hoa và **10 lượt** generate thiệp AI mỗi khách hàng mỗi ngày.
- Khi request được chấp nhận, hệ thống ghi nhận **01 lượt** vào cả hai giới hạn.
- Nếu AI tạo ảnh thành công: **giữ nguyên** các lượt đã ghi nhận.
- Nếu AI Job thất bại sau toàn bộ retry và không có ảnh output hợp lệ: **hoàn lại** các lượt đã ghi nhận.
- Nếu thao tác được thực hiện từ History chung, thiệp mới chỉ được lưu vào History và **không tự động gắn** với Checkout.

### Ngữ cảnh sử dụng

| Ngữ cảnh | Hành vi kết quả |
|---|---|
| **Từ History chung** | Thiệp mới chỉ được lưu vào History, không tự động gắn với Checkout |
| **Trong Checkout** | Kết quả mới được lưu vào History, hiển thị để xem. Chỉ khi chọn **"Xác nhận"** mới trở thành thiệp hiện tại của Checkout |

> Nếu Checkout đã có thiệp đang chọn, thiệp trước đó bị **gỡ khỏi Checkout** nhưng vẫn được **giữ trong History**.

### Input Data

**Thông tin bắt buộc:**

| Trường | Giới hạn |
|---|---|
| Người gửi | Tối đa 20 từ |
| Người nhận | Tối đa 20 từ |
| Lời chúc | Tối đa 100 từ |
| Template | Bắt buộc chọn |
| Size | Bắt buộc chọn |
| Hình thức | Calligraphy hoặc Gõ máy |

**Ảnh đính kèm (không bắt buộc):**

| Thuộc tính | Quy định |
|---|---|
| Số lượng | Tối đa 01 ảnh |
| Định dạng | PNG hoặc JPG |
| Dung lượng | Tối đa 10 MB |

### Quy tắc đếm từ

- Một từ là một chuỗi ký tự liên tục được phân tách bởi khoảng trắng, tab hoặc ký tự xuống dòng.
- Khoảng trắng ở đầu và cuối nội dung **không được tính**.
- Nhiều khoảng trắng, tab hoặc ký tự xuống dòng liên tiếp được tính là **một dấu phân tách**.
- Dấu câu đi liền với một từ **không được tính** thành từ riêng.
- Nội dung chỉ gồm khoảng trắng được tính là **0 từ**.

### Dữ liệu sử dụng khi Tạo lại

> Hệ thống sử dụng **nguyên** Người gửi, Người nhận, Lời chúc, Template, Size, Hình thức và ảnh đính kèm (nếu có) của lần generate hiện tại. Khách hàng **không chỉnh sửa** các dữ liệu này trong thao tác Tạo lại.

> Frontend chỉ cho phép gửi yêu cầu khi dữ liệu hợp lệ. **Backend phải kiểm tra lại** toàn bộ dữ liệu, quyền sở hữu và điều kiện xử lý.

---

## Conditions

### Preconditions
- Khách hàng đã đăng nhập.
- Khi tạo lại từ History, thiệp nguồn phải tồn tại và thuộc khách hàng hiện tại.
- Có ít nhất 01 **Template** và 01 **Size** đang ở trạng thái khả dụng từ Core system.

### Trigger
> Khách hàng chọn **"Tạo lại"** khi xem một thiệp thuộc History của mình.

---

## Flow

### Main Flow — Tạo lại thiệp cá nhân hóa bằng AI

1. Khách hàng đang xem một kết quả thiệp AI đã tạo.
2. Khách hàng chọn **"Tạo lại"**.
3. Backend kiểm tra quyền sở hữu, giới hạn **03 lượt** theo mẫu hoa nguồn và quota **10 lượt/ngày**.
4. Hệ thống sử dụng **nguyên** dữ liệu đầu vào của lần generate hiện tại.
5. Hệ thống tạo AI Job mới, ghi nhận **01 lượt** vào quota tạo thiệp trong ngày và **01 lượt** vào giới hạn generate thiệp của mẫu hoa nguồn.
6. Hệ thống gửi yêu cầu AI mới mà **không mở biểu mẫu chỉnh sửa**.
7. Nếu có ảnh output hợp lệ, hệ thống lưu ảnh chính thức dưới định dạng **PNG**, **không gắn logo**.
8. Hệ thống tạo đúng **01 History record** mới và **giữ nguyên thiệp nguồn**.
9. Kết quả mới được hiển thị để khách hàng tiếp tục **Tạo lại** hoặc **Xác nhận**.

---

### Alternative Flows

#### ALT-01 — Reload khi AI đang xử lý
1. Yêu cầu AI đang được xử lý ngầm.
2. Khách hàng reload hoặc rời khỏi màn hình.
3. Job AI **tiếp tục chạy** và không bị khởi tạo lại.
4. Yêu cầu hiển thị trạng thái **"Đang tạo"** tại màn lịch sử liên quan.
5. Job chưa tạo History item khi chưa có ảnh output hợp lệ. Lượt quota đã được trừ khi request tạo lại được chấp nhận và chỉ được hoàn nếu AI Job thất bại sau toàn bộ retry.

#### ALT-02 — Không có ảnh đính kèm
1. Khách hàng vẫn được tạo thiệp nếu các dữ liệu bắt buộc còn lại hợp lệ.

#### ALT-04 — Combo nguồn bị xóa mềm sau khi AI Job Tạo lại đã bắt đầu
1. Khách hàng gửi yêu cầu Tạo lại thiệp hợp lệ.
2. Backend đã validate thành công thiệp nguồn, Template, Size, Combo nguồn nếu có, quota và giới hạn generate.
3. Hệ thống đã tạo AI Job và gửi yêu cầu sang quá trình AI.
4. Admin xóa mềm hoặc ngừng khả dụng Combo nguồn trong lúc AI Job đang xử lý.
5. Hệ thống tiếp tục xử lý AI Job theo dữ liệu snapshot tại thời điểm validate.
6. Nếu AI tạo được ảnh output hợp lệ, hệ thống lưu ảnh kết quả, tạo đúng **01 History record** mới và giữ nguyên lượt generate đã ghi nhận.

---

### Exception Flows

#### EXC-01 — Hết quota
1. Khách hàng đang xem một kết quả thiệp AI đã tạo.
2. Khách hàng chọn **"Tạo lại"**.
3. Backend kiểm tra quota tạo thiệp AI của khách hàng trong ngày.
4. Backend phát hiện khách hàng đã sử dụng đủ **10 lượt** generate thiệp AI trong ngày.
5. Hệ thống không tạo AI Job mới.
6. Hệ thống không tạo History record mới.
7. Hệ thống không ghi nhận thêm lượt quota ngày hoặc lượt generate theo mẫu hoa.
8. Hệ thống thông báo: **"Bạn đã sử dụng hết 10 lượt tạo thiệp AI trong ngày."**

#### EXC-02 — Template hoặc Size không còn khả dụng
1. Khách hàng chọn **"Tạo lại"**.
2. Backend kiểm tra Template và Size của thiệp nguồn.
3. Backend phát hiện Template hoặc Size đã ngừng khả dụng.
4. Hệ thống không tạo AI Job mới.
5. Hệ thống không tạo History record mới.
6. Hệ thống không ghi nhận quota ngày hoặc lượt generate theo mẫu hoa.
7. Hệ thống thông báo Template hoặc Size của thiệp nguồn **không còn khả dụng** để Tạo lại.

#### EXC-03 — AI không tạo được ảnh output
1. Khách hàng đã gửi yêu cầu tạo lại thiệp hợp lệ.
2. Hệ thống đã gửi yêu cầu sang quá trình AI.
3. AI không tạo được ảnh output hợp lệ.
4. Hệ thống tự động thử lại theo chính sách kỹ thuật.
5. Nếu vẫn không tạo được ảnh sau số lần thử lại cho phép, hệ thống kết thúc xử lý thất bại.
6. Hệ thống không tạo History record mới.
7. Hệ thống hoàn lại đúng **01 lượt quota** tạo thiệp trong ngày và **01 lượt generate** thiệp theo mẫu hoa đã ghi nhận cho AI Job đó.
8. Mỗi AI Job chỉ được hoàn quota tối đa **01 lần**.
9. Hệ thống thông báo khách hàng thử lại sau.

#### EXC-04 — Request bị gửi trùng
1. Khách hàng chọn **"Tạo lại"** nhiều lần liên tiếp hoặc cùng một request được gửi lại do lỗi mạng/trình duyệt.
2. Backend nhận request tạo lại thiệp bị trùng.
3. Backend nhận diện request trùng theo **cơ chế idempotent**.
4. Hệ thống không gọi AI nhiều lần cho cùng một request hợp lệ.
5. Hệ thống không tạo nhiều ảnh.
6. Hệ thống không tạo nhiều History record.
7. Hệ thống không ghi nhận quota nhiều lần.
8. Hệ thống chỉ trả về kết quả tương ứng với request hợp lệ đã được xử lý.

#### EXC-05 — Không có quyền với thiệp nguồn
1. Khách hàng mở hoặc gửi yêu cầu tạo lại từ một thiệp nguồn.
2. Backend kiểm tra quyền sở hữu của thiệp nguồn.
3. Backend phát hiện thiệp nguồn **không thuộc** khách hàng hiện tại.
4. Hệ thống từ chối request tạo lại.
5. Hệ thống không trả ảnh của thiệp nguồn.
6. Hệ thống không trả metadata nhạy cảm của thiệp nguồn.
7. Hệ thống không bắt đầu AI.
8. Hệ thống không tạo History record mới và không trừ quota.

#### EXC-06 — Đã đủ 03 lượt generate cho mẫu hoa
1. Khách hàng đang xem một kết quả thiệp AI thuộc History của mình.
2. Khách hàng chọn **"Tạo lại"**.
3. Backend kiểm tra số lượt generate thiệp đã sử dụng của mẫu hoa nguồn.
4. Backend phát hiện mẫu hoa đã sử dụng đủ **03 lượt** generate thiệp AI.
5. Hệ thống không tạo AI Job mới.
6. Hệ thống không ghi nhận thêm quota ngày.
7. Hệ thống không tạo History record mới.
8. Hệ thống thông báo: **"Bạn đã sử dụng hết số lượt tạo thiệp cho mẫu hoa này."**

#### EXC-07 — Combo nguồn không còn khả dụng trước khi chạy AI
1. Khách hàng đang thực hiện Tạo lại thiệp từ một thiệp nguồn.
2. Mẫu hoa nguồn của thiệp có Combo nguồn.
3. Trước khi hệ thống tạo AI Job, Combo nguồn bị hết hàng, chuyển sang Inactive, bị xóa mềm hoặc ngừng khả dụng.
4. Khách hàng chọn **"Tạo lại"**.
5. Backend kiểm tra lại trạng thái khả dụng của Combo nguồn.
6. Backend phát hiện Combo nguồn không còn khả dụng.
7. Hệ thống không gửi yêu cầu sang AI.
8. Hệ thống không tạo History record.
9. Hệ thống không trừ quota.
10. Hệ thống thông báo: **"Combo hoa nguồn hiện không còn khả dụng nên hệ thống chưa thể tạo thiệp phù hợp nhất với mẫu hoa đã chọn. Vui lòng chọn lại mẫu hoa hoặc sản phẩm hoa khác để tiếp tục."**

---

## Acceptance Criteria

### AC-001 — Tạo lại thành công
- **Given:** Thiệp nguồn thuộc khách hàng hiện tại.
- **When:** Khách hàng tạo lại và AI tạo được ảnh hợp lệ.
- **Then:** Hệ thống lưu ảnh kết quả chính thức dưới định dạng **PNG** và **không gắn logo**.
- **And:** Hệ thống tạo đúng **01 History record** mới.
- **And:** Không ghi đè hoặc xóa thiệp nguồn.

### AC-002 — AI không tạo được ảnh hợp lệ
- **Given:** Yêu cầu đã được gửi sang AI.
- **When:** Quá trình xử lý kết thúc nhưng không có ảnh output hợp lệ.
- **Then:** Hệ thống không tạo History record.
- **And:** Hệ thống hoàn lại đúng **01 lượt quota** tạo thiệp trong ngày và **01 lượt generate** thiệp theo mẫu hoa đã ghi nhận cho AI Job đó.

### AC-003 — Giữ nguyên nội dung thiệp
- **Given:** Khách hàng đã nhập Người gửi, Người nhận và Lời chúc hợp lệ.
- **When:** AI tạo thiệp.
- **Then:** Hệ thống phải giữ nguyên chính xác dữ liệu Người gửi, Người nhận và Lời chúc; việc render các nội dung này lên ảnh kết quả tuân theo **BR-154**.

### AC-004 — Template hoặc Size không khả dụng
- **Given:** Template hoặc Size đã ngừng khả dụng.
- **When:** Khách hàng gửi yêu cầu.
- **Then:** Hệ thống không bắt đầu AI.
- **And:** Hệ thống thông báo Template hoặc Size của thiệp nguồn **không còn khả dụng** để Tạo lại.

### AC-005 — Chống request trùng
- **Given:** Cùng một request được gửi lại.
- **When:** Backend xử lý request trùng.
- **Then:** Chỉ có tối đa **01 lần** gọi AI, **01 ảnh**, **01 History record**, **01 lượt quota ngày** và **01 lượt generate** theo mẫu hoa được ghi nhận.

### AC-006 — Giữ AI Job khi reload
- **Given:** Job AI đang chạy ngầm.
- **When:** Khách hàng reload hoặc mở lại màn hình.
- **Then:** Job tiếp tục xử lý.
- **And:** Hệ thống hiển thị trạng thái **"Đang tạo"**.
- **And:** Chưa tạo History item khi chưa có ảnh output hợp lệ; lượt quota đã được trừ khi request được chấp nhận.

### AC-007 — Không có quyền với thiệp nguồn
- **Given:** Thiệp nguồn không thuộc khách hàng hiện tại.
- **When:** Khách hàng yêu cầu tạo lại.
- **Then:** Backend từ chối request.
- **And:** Không trả ảnh hoặc metadata nhạy cảm.

### AC-008 — Tạo lại với dữ liệu hiện tại
- **Given:** Thiệp nguồn tồn tại và thuộc khách hàng hiện tại.
- **When:** Khách hàng chọn **"Tạo lại"**.
- **Then:** Hệ thống sử dụng lại **nguyên dữ liệu đầu vào** của lần generate hiện tại để tạo một AI Job mới.
- **And:** Hệ thống **không mở biểu mẫu chỉnh sửa** trước khi gửi request mới.
- **And:** Thao tác Tạo lại tiêu hao **01 lượt** nếu request được chấp nhận và tuân theo cả giới hạn **03 lượt theo mẫu hoa** và **10 lượt/ngày**.

### AC-009 — Giữ nguyên ảnh đính kèm
- **Given:** Khách hàng đã chọn Template và thiệp nguồn có thể có ảnh đính kèm.
- **When:** AI thực hiện Tạo lại thiệp.
- **Then:** Giữ nguyên **100% ảnh đính kèm** nếu có.
- **And:** Giữ nguyên nội dung gốc; được phép scale đồng dạng hoặc thêm khoảng đệm, không được cắt mất chủ thể, xoay, đổi màu hoặc làm biến dạng.

### AC-010 — Không tạo AI Job nếu Combo nguồn không còn khả dụng trước validate
- **Given:** Mẫu hoa nguồn của thiệp có Combo nguồn.
- **When:** Khách hàng gửi yêu cầu tạo lại thiệp nhưng Combo nguồn đã hết hàng, bị chuyển sang Inactive, bị xóa mềm hoặc ngừng khả dụng trước khi Backend validate.
- **Then:** Hệ thống không tạo AI Job.
- **And:** Không tạo History record.
- **And:** Không trừ quota.
- **And:** Hệ thống thông báo Combo nguồn không còn khả dụng và yêu cầu khách hàng chọn lại sản phẩm/mẫu hoa phù hợp.

### AC-011 — Vẫn hoàn tất AI Job Tạo lại nếu Combo nguồn bị xóa sau validate
- **Given:** AI Job Tạo lại đang được xử lý cho một thiệp nguồn có Combo nguồn.
- **When:** Admin xóa mềm hoặc ngừng khả dụng Combo nguồn trong lúc AI Job đang xử lý.
- **Then:** Hệ thống vẫn tiếp tục AI Job theo dữ liệu đã được ghi nhận tại thời điểm request Tạo lại được chấp nhận.
- **And:** Nếu AI tạo được ảnh output hợp lệ, hệ thống lưu ảnh kết quả.
- **And:** Hệ thống tạo đúng **01 History record** mới.
- **And:** Hệ thống giữ nguyên lượt generate đã ghi nhận.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|---|---|
| [**BR-044**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b4df6090-28ff-4fe4-a528-9123a7f5ad8e) | Tạo lại | Tạo ảnh | Tạo lại luôn khởi tạo một yêu cầu AI mới. | Khách hàng tạo lại từ một thiệp đã có. | Nếu có ảnh output hợp lệ, hệ thống tạo History record mới. | Thiệp nguồn được giữ nguyên. | Đức Bình | STORY-036 | Draft | v0 | 2026-08-14 |
| [**BR-046**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d1ba26c7-d034-4be5-849a-4d427ab9f4eb) | Quyền sở hữu | Thiệp | Khách hàng chỉ được tạo lại từ thiệp thuộc tài khoản của mình và chỉ được gắn thiệp vào Checkout thuộc tài khoản của mình. | Khách hàng tạo lại thiệp hoặc gắn thiệp vào Checkout. | Backend kiểm tra quyền sở hữu của thiệp và Checkout. | Không cho phép tạo lại từ thiệp hoặc gắn vào Checkout không thuộc tài khoản khách hàng hiện tại. | Đức Bình | STORY-036 | Draft | v0 | 2026-08-14 |
| [**BR-154**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5573cc50-4ff5-405c-a44f-08a102297e92) | Nội dung ảnh thiệp theo hình thức | Tạo thiệp | Ảnh thiệp AI phải xử lý nội dung theo Hình thức được chọn. | Khách hàng tạo thiệp với Hình thức Gõ máy hoặc Calligraphy. | **Gõ máy:** AI render nguyên văn Người gửi, Người nhận và Lời chúc lên ảnh output. **Calligraphy:** AI không render Người gửi, Người nhận và Lời chúc lên ảnh output. Các nội dung này vẫn được lưu cùng History record để Staff/Admin sử dụng khi viết tay lên thiệp. | — | Đức Bình | STORY-036 | Draft | v0 | 2026-08-18 |
| [**BR-048**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b04b77dd-11d1-42f5-8976-b360ba3e5014) | Quota tạo thiệp | AI | Mỗi khách hàng được sử dụng tối đa **10 lượt** generate thiệp AI mỗi ngày. Đối với mỗi mẫu hoa nguồn, khách hàng được thực hiện tối đa **03 lượt** generate thiệp AI. Hai giới hạn này được áp dụng đồng thời. | Khách hàng thực hiện thao tác Tạo thiệp hoặc Tạo lại làm phát sinh một AI Job tạo thiệp mới. | Backend kiểm tra đồng thời: Số lượt generate thiệp AI khách hàng đã sử dụng trong ngày chưa đạt 10 lượt. Số lượt generate thiệp AI của mẫu hoa nguồn chưa đạt 03 lượt. Nếu cả hai giới hạn đều còn lượt, hệ thống chấp nhận request, tạo AI Job và ghi nhận 01 lượt vào quota ngày và 01 lượt vào giới hạn của mẫu hoa. Nếu AI tạo được ảnh output hợp lệ, các lượt đã ghi nhận được giữ nguyên. Nếu AI Job thất bại sau toàn bộ retry và không tạo được ảnh output hợp lệ, hệ thống hoàn lại 01 lượt quota ngày và 01 lượt của mẫu hoa đã ghi nhận cho AI Job đó. | Nếu khách hàng đã sử dụng đủ 10 lượt trong ngày hoặc mẫu hoa nguồn đã sử dụng đủ 03 lượt generate thiệp, hệ thống không cho phép tạo AI Job mới. Các thao tác Xác nhận thiệp, chọn thiệp đã tạo từ History hoặc tải xuống thiệp không làm phát sinh AI Job và không được tính vào các giới hạn trên. Quota 10 lượt/ngày được đặt lại lúc 00:00 theo múi giờ Asia/Ho_Chi_Minh và độc lập với quota tạo mẫu hoa AI. | Đức Bình | STORY-036 | Draft | v0 | 2026-08-14 |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-044 | [Tạo lại](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b4df6090-28ff-4fe4-a528-9123a7f5ad8e) |
| BR-046 | [Quyền sở hữu](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d1ba26c7-d034-4be5-849a-4d427ab9f4eb) |
| BR-154 | [Nội dung ảnh thiệp theo hình thức](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5573cc50-4ff5-405c-a44f-08a102297e92) |
| BR-048 | [Quota tạo thiệp](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b04b77dd-11d1-42f5-8976-b360ba3e5014) |

### Dependencies
- [**STORY-045**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)
- [**STORY-039**](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)

---

## Non-Functional Requirements

- Mở biểu mẫu tạo thiệp **p95 ≤ 1 giây** trong điều kiện bình thường.
- Tải Template và Size **p95 ≤ 1,5 giây**, không tính thời gian Core system bị chậm.
- Backend kiểm tra **ownership, quota và idempotency**.
- Trạng thái job AI phải có thể được **truy xuất lại** sau khi khách hàng reload.
- API metadata không trả binary ảnh hoặc expose đường dẫn storage nội bộ.
- Thông báo lỗi không được chứa **stack trace** hoặc thông tin kỹ thuật nhạy cảm.

---

## Out of Scope

- Chọn thiệp đã có từ History cho Checkout.
- Hoàn tất Checkout, tạo Order và thanh toán.
- Tạo liên kết Thiệp-Order và tính bill cuối.
- Tải xuống, chia sẻ, chỉnh sửa hoặc xóa thiệp.