# STORY-035 — Tạo thiệp cá nhân hóa bằng AI tại Checkout

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã đăng nhập, tôi muốn tạo thiệp mới tại Checkout, để có một mẫu thiệp cá nhân hóa phù hợp với đơn hàng của mình. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Hoàng Thị Khánh Linh |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |

---

## Context

Khách hàng có thể tạo thiệp mới tại **Checkout**.

- Mỗi lần AI tạo được ảnh output hợp lệ, hệ thống tạo đúng **01 History record** mới và ghi nhận **01 lượt generate** đã sử dụng.
- Mỗi mẫu hoa trong quá trình đặt hàng được generate tối đa **03 lượt** thiệp AI và mỗi khách hàng được sử dụng tối đa **10 lượt** generate thiệp AI mỗi ngày.
- Ảnh thiệp vừa generate **không tự động** trở thành thiệp hiện tại của Checkout. Khách hàng phải chọn **"Xác nhận"** đối với kết quả muốn sử dụng.
- Nếu Checkout đã có thiệp, thiệp cũ chỉ bị **gỡ khỏi Checkout** và vẫn được **giữ trong History**.
- Việc hoàn tất Checkout, tạo Order, tính bill cuối và tạo liên kết Thiệp-Order thuộc phạm vi khác.

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
| Định dạng | PNG, JPG |
| Dung lượng | Tối đa 10 MB |

### Quy tắc đếm từ

- Một từ là một chuỗi ký tự liên tục được phân tách bởi khoảng trắng, tab hoặc ký tự xuống dòng.
- Khoảng trắng ở đầu và cuối nội dung không được tính.
- Nhiều khoảng trắng, tab hoặc ký tự xuống dòng liên tiếp được tính là **một dấu phân tách**.
- Dấu câu đi liền với một từ **không được tính** thành từ riêng.
- Nội dung chỉ gồm khoảng trắng được tính là **0 từ**.

> Frontend chỉ cho phép gửi yêu cầu khi dữ liệu hợp lệ. **Backend phải kiểm tra lại** toàn bộ dữ liệu, quyền sở hữu và điều kiện xử lý.

---

## Conditions

### Preconditions
- Khách hàng đã đăng nhập.
- Checkout phải tồn tại, thuộc khách hàng hiện tại và chưa hoàn tất.
- Có ít nhất 01 **Template** và 01 **Size** đang ở trạng thái khả dụng từ Core DB.

### Trigger
> Khách hàng chọn **"Tạo thiệp"** tại Checkout.

---

## Flow

### Main Flow — Tạo thiệp cá nhân hóa bằng AI tại Checkout

1. Khách hàng mở chức năng tạo thiệp tại Checkout.
2. Backend kiểm tra Checkout, số lượt generate thiệp đã sử dụng cho mẫu hoa nguồn và quota tạo thiệp trong ngày của khách hàng.
3. Hệ thống tải danh sách **Template** và **Size** hiện hành từ Core system.
4. Khách hàng nhập Người gửi, Người nhận, Lời chúc; chọn Template, Size, Hình thức và có thể tải lên 01 ảnh.
5. Hệ thống lấy giá hiện hành của Size từ DB hệ thống và hiển thị **giá tạm tính** theo Size và Hình thức theo BR-054.
6. Khách hàng chọn **"Tạo thiệp"**.
7. Backend kiểm tra lại Checkout, dữ liệu đầu vào, Template, Size, **giới hạn 03 lượt** generate thiệp của mẫu hoa nguồn và **quota 10 lượt** generate thiệp trong ngày của khách hàng.
8. Hệ thống tạo AI Job cho request hợp lệ, ghi nhận 01 lượt vào quota tạo thiệp trong ngày và 01 lượt vào giới hạn generate thiệp của mẫu hoa nguồn.
9. Hệ thống gửi yêu cầu sang quá trình AI.
10. AI tạo ảnh theo Template và Hình thức đã chọn (tuân BR-154).
11. Nếu có ảnh đính kèm, AI phải giữ nguyên nội dung gốc của ảnh; không cắt mất chủ thể, xoay, đổi màu, thêm/xóa hoặc làm biến dạng. Được phép scale đồng dạng hoặc thêm khoảng đệm để phù hợp Template.
12. Nếu có ảnh output hợp lệ, hệ thống lưu ảnh kết quả chính thức dưới định dạng **PNG** và **không gắn logo**.
13. Hệ thống giữ nguyên lượt đã ghi nhận và tạo đúng **01 History record** mới.
14. Hệ thống hiển thị kết quả thiệp cùng các thao tác **"Tạo lại"** và **"Xác nhận"**.
15. Khách hàng xem kết quả thiệp.
16. Nếu khách hàng chọn **"Xác nhận"**, hệ thống chọn kết quả đó làm **thiệp hiện tại** của Checkout.
17. Nếu Checkout đã có thiệp khác, thiệp trước đó bị gỡ khỏi Checkout nhưng vẫn được **giữ trong History**.
18. Order Summary hiển thị thiệp đã được xác nhận và giá tạm tính tương ứng.

---

### Alternative Flows

#### ALT-01 — Reload khi AI đang xử lý
1. Yêu cầu AI đang được xử lý ngầm.
2. Khách hàng reload hoặc rời khỏi màn hình.
3. Job AI **tiếp tục chạy** và không bị khởi tạo lại.
4. Yêu cầu hiển thị trạng thái **"Đang tạo"** tại màn lịch sử liên quan.
5. Job chưa tạo History item khi chưa có ảnh output hợp lệ. Lượt quota đã được trừ khi request được chấp nhận và chỉ được hoàn nếu AI Job kết thúc thất bại sau toàn bộ retry.

#### ALT-02 — Không có ảnh đính kèm
1. Khách hàng vẫn được tạo thiệp nếu các dữ liệu bắt buộc còn lại hợp lệ.

---

### Exception Flows

#### EXC-01 — Hết quota
1. Khách hàng đang ở màn tạo thiệp tại Checkout.
2. Khách hàng đã nhập đầy đủ dữ liệu hợp lệ.
3. Khách hàng ấn chọn **"Tạo thiệp"**.
4. Backend kiểm tra quota tạo thiệp của khách hàng trong ngày.
5. Backend phát hiện khách hàng đã sử dụng hết quota tạo thiệp trong ngày.
6. Hệ thống không gửi yêu cầu sang AI.
7. Hệ thống không tạo History record.
8. Hệ thống không ghi nhận thêm lượt quota.
9. Hệ thống thông báo khách hàng đã sử dụng hết quota tạo thiệp trong ngày.

#### EXC-02 — Dữ liệu bắt buộc không hợp lệ
1. Khách hàng nhập thiếu hoặc nhập không hợp lệ một trong các trường bắt buộc: Người gửi, Người nhận, Lời chúc, Template, Size hoặc Hình thức.
2. Khách hàng ấn chọn **"Tạo thiệp"**.
3. Backend kiểm tra lại toàn bộ dữ liệu đầu vào.
4. Backend phát hiện dữ liệu không hợp lệ.
5. Hệ thống không gửi yêu cầu sang AI.
6. Hệ thống không tạo History record.
7. Hệ thống không trừ quota.
8. Hệ thống hiển thị lỗi tại trường tương ứng.

#### EXC-03 — File upload không hợp lệ
1. Khách hàng chọn ảnh đính kèm.
2. File ảnh không thuộc định dạng PNG, JPG, hoặc có dung lượng vượt quá 10 MB.
3. Hệ thống từ chối file ảnh.
4. Hệ thống hiển thị lý do file không hợp lệ.
5. Hệ thống cho phép khách hàng chọn file khác.
6. Khách hàng vẫn có thể tiếp tục tạo thiệp không có ảnh đính kèm nếu các dữ liệu bắt buộc còn lại hợp lệ.

#### EXC-04 — Template hoặc Size không còn khả dụng
1. Khách hàng đã chọn Template hoặc Size.
2. Khách hàng ấn chọn **"Tạo thiệp"**.
3. Backend kiểm tra lại trạng thái khả dụng của Template và Size từ Core system.
4. Backend phát hiện Template hoặc Size đã ngừng khả dụng.
5. Hệ thống không gửi yêu cầu sang AI.
6. Hệ thống không tạo History record.
7. Hệ thống không trừ quota.
8. Hệ thống yêu cầu khách hàng chọn Template hoặc Size hiện còn khả dụng.

#### EXC-05 — AI không tạo được ảnh output
1. Khách hàng đã gửi yêu cầu tạo thiệp hợp lệ.
2. Hệ thống đã gửi yêu cầu sang quá trình AI.
3. AI không tạo được ảnh output hợp lệ.
4. Hệ thống tự động thử lại theo chính sách kỹ thuật.
5. Nếu vẫn không tạo được ảnh sau số lần thử lại cho phép, hệ thống kết thúc xử lý thất bại.
6. Hệ thống không tạo History record.
7. Hệ thống hoàn lại đúng **01 lượt quota** tạo thiệp trong ngày và **01 lượt generate** thiệp của mẫu hoa nguồn đã ghi nhận cho AI Job đó.
8. Mỗi AI Job chỉ được hoàn quota tối đa **01 lần**.
9. Hệ thống thông báo khách hàng thử lại sau.

#### EXC-06 — Có ảnh nhưng gắn với Checkout thất bại
1. Khách hàng đang xem một kết quả thiệp AI đã generate thành công.
2. Khách hàng chọn **"Xác nhận"**.
3. Hệ thống thực hiện chọn thiệp đó làm thiệp hiện tại của Checkout.
4. Hệ thống gặp lỗi khi cập nhật Checkout.
5. History record của thiệp **vẫn được giữ lại**.
6. Các lượt generate đã sử dụng **vẫn được ghi nhận**.
7. Checkout giữ thiệp trước đó nếu có.
8. Hệ thống thông báo thao tác Xác nhận chưa thành công và cho phép khách hàng thử lại.

#### EXC-07 — Request bị gửi trùng
1. Khách hàng ấn **"Tạo thiệp"** nhiều lần liên tiếp hoặc cùng một request được gửi lại do lỗi mạng/trình duyệt.
2. Backend nhận request tạo thiệp bị trùng.
3. Backend nhận diện request trùng theo **cơ chế idempotent**.
4. Hệ thống không gọi AI nhiều lần cho cùng một request hợp lệ.
5. Hệ thống không tạo nhiều ảnh, nhiều History record hoặc ghi nhận quota nhiều lần.
6. Hệ thống chỉ trả về kết quả tương ứng với request hợp lệ đã được xử lý.

#### EXC-08 — Checkout không hợp lệ
1. Khách hàng ấn chọn **"Tạo thiệp"**.
2. Backend kiểm tra Checkout hiện tại.
3. Backend phát hiện Checkout không tồn tại, không thuộc khách hàng hiện tại hoặc đã hoàn tất.
4. Hệ thống từ chối thao tác tạo thiệp.
5. Hệ thống không gửi yêu cầu sang AI.
6. Hệ thống không tạo History record.
7. Hệ thống không trừ quota.
8. Hệ thống hiển thị lỗi tương ứng với nguyên nhân Checkout không hợp lệ và **không để lộ dữ liệu** của Checkout/khách hàng khác.

#### EXC-09 — Đã đủ 03 lượt cho mẫu hoa
1. Khách hàng đã sử dụng đủ **03 lượt** generate thiệp AI cho mẫu hoa nguồn của lần đặt hàng hiện tại.
2. Khách hàng thực hiện thao tác làm phát sinh AI Job mới.
3. Backend phát hiện giới hạn theo mẫu hoa đã hết.
4. Hệ thống không tạo AI Job.
5. Hệ thống không trừ quota ngày.
6. Hệ thống không tạo History record.
7. Hệ thống thông báo khách hàng đã sử dụng hết số lượt tạo thiệp cho mẫu hoa này.

---

## Acceptance Criteria

### AC-001 — Tạo thiệp thành công
- **Given:** Checkout hợp lệ và khách hàng còn quota.
- **When:** AI tạo được ảnh output hợp lệ.
- **Then:** Hệ thống lưu ảnh kết quả chính thức dưới định dạng **PNG** và **không gắn logo**.
- **And:** Tạo đúng **01 History record**.
- **And:** Ghi nhận đúng **01 lượt generate** đã sử dụng.
- **And:** Chưa tự động chọn thiệp mới làm thiệp hiện tại của Checkout khi khách hàng chưa xác nhận.

### AC-002 — AI không tạo được ảnh hợp lệ
- **Given:** Yêu cầu đã được gửi sang AI.
- **When:** Quá trình xử lý kết thúc nhưng không có ảnh output hợp lệ.
- **Then:** Hệ thống hoàn lại đúng **01 lượt quota** tạo thiệp trong ngày và **01 lượt generate** thiệp của mẫu hoa nguồn đã ghi nhận.
- **And:** Mỗi AI Job chỉ được hoàn mỗi loại lượt tối đa **01 lần**.

### AC-003 — Nội dung thiệp Gõ máy
- **Given:** Khách hàng chọn Hình thức Gõ máy và đã nhập Người gửi, Người nhận, Lời chúc hợp lệ.
- **When:** AI tạo thiệp.
- **Then:** Hệ thống phải render **nguyên văn** Người gửi, Người nhận và Lời chúc lên ảnh kết quả.
- **And:** Không được tự động sửa, rút gọn, dịch hoặc thay đổi Người gửi, Người nhận và Lời chúc.

### AC-004 — Giới hạn số từ
- **Given:** Một trường vượt quá giới hạn số từ.
- **When:** Khách hàng gửi yêu cầu.
- **Then:** Backend từ chối dữ liệu.
- **And:** Không bắt đầu AI.
- **And:** Không trừ quota.

### AC-005 — Template và ảnh đính kèm
- **Given:** Khách hàng đã chọn Template và có thể có ảnh đính kèm.
- **When:** AI tạo thiệp.
- **Then:** AI giữ nguyên nội dung gốc của ảnh; không cắt mất chủ thể, xoay, đổi màu, thêm/xóa hoặc làm biến dạng. Được phép scale đồng dạng hoặc thêm khoảng đệm để phù hợp Template.
- **And:** Giữ nguyên 100% ảnh đính kèm nếu có.
- **And:** Không sửa, cắt, xoay, đổi màu, thêm, xóa hoặc làm biến dạng nội dung ảnh.

### AC-006 — File ảnh không hợp lệ
- **Given:** File không đúng định dạng hoặc vượt quá 10 MB.
- **When:** Khách hàng tải file lên.
- **Then:** Hệ thống từ chối file.
- **And:** Cho phép chọn file khác hoặc tiếp tục không có ảnh.

### AC-007 — Template hoặc Size không khả dụng
- **Given:** Template hoặc Size đã ngừng khả dụng.
- **When:** Khách hàng gửi yêu cầu.
- **Then:** Hệ thống không bắt đầu AI.
- **And:** Yêu cầu chọn dữ liệu hiện còn khả dụng.

### AC-008 — Không gắn được thiệp sau khi Xác nhận
- **Given:** Khách hàng đang xem một kết quả thiệp AI đã generate thành công.
- **When:** Khách hàng chọn **"Xác nhận"** nhưng hệ thống không thể chọn thiệp đó làm thiệp hiện tại của Checkout.
- **Then:** History record vẫn được giữ.
- **And:** Các lượt generate đã sử dụng vẫn được ghi nhận.
- **And:** Checkout giữ thiệp trước đó nếu có.

### AC-009 — Chống request trùng
- **Given:** Cùng một request được gửi lại.
- **When:** Backend xử lý request trùng.
- **Then:** Chỉ có tối đa **01 lần** gọi AI, **01 ảnh**, **01 History record** và **01 lượt quota** được ghi nhận.

### AC-010 — Checkout không hợp lệ
- **Given:** Checkout không tồn tại, không thuộc khách hàng hoặc đã hoàn tất.
- **When:** Khách hàng yêu cầu tạo thiệp trong Checkout.
- **Then:** Backend từ chối.
- **And:** Không bắt đầu AI.

### AC-011 — Giữ AI Job khi reload
- **Given:** Job AI đang chạy ngầm.
- **When:** Khách hàng reload hoặc mở lại màn hình.
- **Then:** Job tiếp tục xử lý.
- **And:** Hệ thống hiển thị trạng thái **"Đang tạo"**.
- **And:** Chưa tạo History item khi chưa có ảnh output hợp lệ; lượt quota đã được trừ khi request được chấp nhận.

### AC-012 — Tạo thiệp không có ảnh đính kèm
- **Given:** Checkout hợp lệ, khách hàng còn quota và các trường bắt buộc khác hợp lệ.
- **When:** Khách hàng không chọn ảnh đính kèm.
- **Then:** Hệ thống vẫn phải cho phép tạo thiệp bằng AI.
- **And:** Nếu tạo thành công, hệ thống tạo ảnh kết quả và xử lý History/quota/Checkout như Main Flow.

### AC-013 — Nội dung thiệp Calligraphy
- **Given:** Khách hàng chọn Hình thức Calligraphy và đã nhập Người gửi, Người nhận, Lời chúc hợp lệ.
- **When:** AI tạo thiệp thành công.
- **Then:** Ảnh output **không hiển thị** Người gửi, Người nhận và Lời chúc.
- **And:** Các nội dung này vẫn được lưu cùng History record của thiệp.

### AC-014 — Xác nhận kết quả thiệp
- **Given:** Khách hàng đang xem một kết quả thiệp AI đã generate thành công trong Checkout.
- **When:** Khách hàng chọn **"Xác nhận"**.
- **Then:** Nếu Checkout đã có thiệp đang chọn, thiệp trước đó bị **gỡ khỏi Checkout** nhưng vẫn được **giữ trong History**.
- **And:** Thao tác Xác nhận không gọi AI, không tạo History record mới và không ghi nhận thêm lượt generate.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Trạng thái | Version | Ngày hiệu lực |
|---|---|---|---|---|---|---|---|---|---|
| **BR-048** | Quota tạo thiệp | AI | Mỗi khách hàng được sử dụng tối đa **10 lượt** generate thiệp AI mỗi ngày. Mỗi mẫu hoa nguồn được tối đa **03 lượt** generate thiệp AI. Hai giới hạn áp dụng đồng thời. | Khách hàng thực hiện thao tác Tạo thiệp hoặc Tạo lại làm phát sinh một AI Job tạo thiệp mới. | Backend kiểm tra đồng thời cả hai giới hạn. Nếu còn lượt: chấp nhận request, tạo AI Job, ghi nhận 01 lượt quota ngày và 01 lượt giới hạn mẫu hoa. Nếu AI thành công: giữ nguyên lượt. Nếu AI thất bại sau toàn bộ retry: hoàn lại 01 lượt quota ngày và 01 lượt của mẫu hoa. | Nếu đã hết 10 lượt ngày hoặc mẫu hoa đã dùng đủ 03 lượt: không cho phép tạo AI Job mới. Thao tác Xác nhận, chọn thiệp từ History, tải xuống không làm phát sinh AI Job. Quota 10 lượt/ngày reset lúc 00:00 Asia/Ho_Chi_Minh, độc lập với quota tạo mẫu hoa AI. | Draft | v0 | 2026-08-14 |
| **BR-049** | Quy tắc trừ và hoàn quota tạo thiệp | AI | Mỗi request tạo thiệp hợp lệ được hệ thống chấp nhận sử dụng 01 lượt quota tạo thiệp. | Backend chấp nhận request và tạo AI Job. | Hệ thống trừ đúng 01 lượt quota. Nếu AI Job tạo thành công ảnh output hợp lệ, lượt đã trừ được giữ nguyên. | Nếu AI Job kết thúc thất bại sau toàn bộ retry và không có ảnh output hợp lệ, hệ thống hoàn lại đúng 01 lượt quota đã trừ. Mỗi AI Job chỉ được trừ tối đa 01 lượt và hoàn tối đa 01 lượt. | Draft | v0 | 2026-08-14 |
| **BR-050** | History record | Tạo thiệp | Mỗi lần generate có ảnh output hợp lệ tạo đúng 01 History record mới. | AI tạo được ảnh output hợp lệ. | Hệ thống tạo đúng 01 History record mới. | Kết quả mới không ghi đè kết quả cũ. | Draft | v0 | 2026-08-14 |
| **BR-051** | Nội dung bắt buộc | Tạo thiệp | Người gửi tối đa 20 từ, Người nhận tối đa 20 từ và Lời chúc tối đa 100 từ theo quy tắc đếm từ. | Khách hàng gửi yêu cầu tạo thiệp. | Hệ thống kiểm tra giới hạn từ của Người gửi, Người nhận và Lời chúc. | Dữ liệu vượt quá giới hạn số từ không được chấp nhận. | Draft | v0 | 2026-08-14 |
| **BR-052** | Ảnh đính kèm | Tạo thiệp | Mỗi yêu cầu có tối đa 01 ảnh đính kèm định dạng PNG, JPG hoặc JPEG và dung lượng tối đa 10 MB. | Khách hàng tải ảnh đính kèm trong yêu cầu tạo thiệp. | Hệ thống kiểm tra số lượng, định dạng và dung lượng ảnh đính kèm. | File không đúng định dạng, vượt quá dung lượng hoặc vượt quá số lượng không được chấp nhận. | Draft | v0 | 2026-08-14 |
| **BR-053** | Một thiệp trong Checkout | Tạo thiệp | Mỗi Checkout có tối đa 01 thiệp đang chọn. | Thiệp mới được chọn trong Checkout. | Thiệp được chọn sau thay thế thiệp đang chọn trước đó. | Thiệp đang chọn trước đó không bị xóa khỏi History. | Draft | v0 | 2026-08-14 |
| **BR-054** | Giá tạm tính | Tạo thiệp | Giá tạm tính của thiệp được xác định từ giá hiện hành của Size và phụ phí theo Hình thức. | Hệ thống hiển thị giá tạm tính cho thiệp. | **Gõ máy:** Giá hiện hành của Size. **Calligraphy:** Giá hiện hành của Size + phụ phí theo số từ Lời chúc: 0–35 từ: +0đ; 36–70 từ: +39.000đ; 71–100 từ: +69.000đ. | Nếu không tìm thấy giá hiện hành tương ứng với Size đã chọn, hệ thống không cho tiếp tục tạo thiệp và thông báo dữ liệu giá hiện không khả dụng. | Draft | v0 | 2026-08-14 |
| **BR-055** | Xử lý request trùng | Tạo thiệp | Backend phải bảo đảm cùng một request hợp lệ không tạo nhiều job AI, ảnh, History record hoặc lượt quota. | Backend nhận request tạo thiệp bị gửi trùng. | Hệ thống xử lý request theo cơ chế idempotent. | Không tạo nhiều job AI, ảnh, History record hoặc lượt quota cho cùng một request hợp lệ. | Draft | v0 | 2026-08-14 |
| **BR-056** | Quyền sở hữu | Tạo thiệp | Khách hàng chỉ được gắn thiệp vào Checkout thuộc tài khoản của mình. | Khách hàng tạo thiệp trong Checkout. | Backend kiểm tra quyền sở hữu của Checkout. | Không cho phép gắn thiệp vào Checkout không thuộc tài khoản khách hàng hiện tại. | Draft | v0 | 2026-08-14 |
| **BR-057** | Phạm vi sáng tạo của AI | Tạo thiệp | AI chỉ được bổ sung yếu tố trang trí trong phạm vi Template cho phép; không được thay đổi nội dung bắt buộc, layout, phong cách Template hoặc ảnh đính kèm. | AI tạo ảnh thiệp. | Giữ nguyên chính xác dữ liệu Người gửi, Người nhận và Lời chúc (việc render theo BR-154). Giữ nguyên layout và phong cách của Template đã chọn. Giữ nguyên nội dung gốc của ảnh đính kèm; được phép scale đồng dạng hoặc thêm khoảng đệm. | AI chỉ được thêm thành phần trang trí tại vùng Template cho phép và không được che hoặc làm thay đổi nội dung bắt buộc, layout, phong cách hay ảnh đính kèm. | Draft | v0 | 2026-08-14 |
| **BR-154** | Nội dung ảnh thiệp theo hình thức | Tạo thiệp | Ảnh thiệp AI phải xử lý nội dung theo Hình thức được chọn. | Khách hàng tạo thiệp với Hình thức Gõ máy hoặc Calligraphy. | **Gõ máy:** AI render nguyên văn Người gửi, Người nhận và Lời chúc lên ảnh output. **Calligraphy:** AI không render Người gửi, Người nhận và Lời chúc lên ảnh output. Các nội dung này vẫn được lưu cùng History record để Staff/Admin sử dụng khi viết tay lên thiệp. | — | Draft | v0 | — |

---

## References

### Business Rules
`BR-048` · `BR-049` · `BR-050` · `BR-051` · `BR-052` · `BR-053` · `BR-054` · `BR-055` · `BR-056` · `BR-057` · `BR-154`

### Dependencies
- **STORY-038**
- **STORY-042**

---

## Non-Functional Requirements

- Mở biểu mẫu tạo thiệp **p95 ≤ 1 giây** trong điều kiện bình thường.
- Tải Template và Size **p95 ≤ 1.5 giây** (không tính thời gian Core system bị chậm).
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