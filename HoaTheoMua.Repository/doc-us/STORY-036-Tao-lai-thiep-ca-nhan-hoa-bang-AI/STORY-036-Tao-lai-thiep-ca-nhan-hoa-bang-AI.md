# STORY-036 — Khách hàng tạo lại thiệp bằng AI trong bước tạo thiệp sau thanh toán

## Metadata

| Trường | Nội dung |
|---|---|
| **Loại** | Story |
| **User Story** | Là một khách hàng đã đăng nhập và có Order Thiệp Custom đã thanh toán thành công, tôi muốn tạo lại thiệp bằng AI trong bước tạo thiệp sau thanh toán, để có thêm lựa chọn ảnh thiệp phù hợp trước khi xác nhận. |
| **Sprint** | S1 |
| **Priority** | Must |
| **Assignee (FE)** | Danh Nguyen |
| **Creator** | Hoàng Thị Khánh Linh |
| **Status** | Cần làm |
| **Phiên bản** | v0.1 |
| **Phê duyệt** | Nháp |

---

## Context

Chức năng Tạo lại thiệp AI chỉ được phép sử dụng trong bước tạo Thiệp Custom sau khi Order đã thanh toán thành công.
---

## Conditions

### Preconditions

- Khách hàng đã đăng nhập.
- Order tồn tại và thuộc khách hàng hiện tại.
- Order có cấu hình Thiệp Custom mới.
- Payment của Order đã được xác minh thành công.
- Order đang ở bước tạo Thiệp Custom sau thanh toán.
- Template và Size trong snapshot Order còn hợp lệ để tạo thiệp AI.
- Khách hàng còn quota tạo thiệp AI trong ngày.
- Mẫu hoa nguồn của Order chưa vượt giới hạn generate thiệp AI.

### Trigger

> - Khách hàng chọn "Tạo lại" trong bước tạo Thiệp Custom sau khi Order đã thanh toán thành công.


---

## Flow

### Main Flow

1. Khách hàng đang ở bước tạo Thiệp Custom sau khi Order thanh toán thành công.
2. Hệ thống hiển thị kết quả thiệp AI hiện tại và thao tác "Tạo lại".
3. Khách hàng chọn "Tạo lại".
4. Backend kiểm tra Order tồn tại, thuộc khách hàng hiện tại và đã Payment SUCCESS.
5. Backend kiểm tra Order có cấu hình Thiệp Custom mới và đang ở bước cho phép tạo thiệp AI.
6. Backend kiểm tra không phải request Tạo lại từ History.
7. Backend kiểm tra Template và Size trong snapshot Order còn hợp lệ.
8. Backend kiểm tra khách hàng còn quota tạo thiệp AI trong ngày và mẫu hoa nguồn chưa vượt giới hạn generate.
9. Nếu khách hàng đã chỉnh sửa Nội dung thiệp trước khi tạo lại, hệ thống kiểm tra số lượng từ sau chỉnh sửa không vượt quá số lượng từ đã thanh toán.
10. Hệ thống tạo AI Job mới cho lần Tạo lại.
11. Hệ thống ghi nhận 01 lượt vào quota tạo thiệp trong ngày và 01 lượt vào giới hạn generate thiệp của mẫu hoa nguồn.
12. Hệ thống gửi yêu cầu AI theo Template, Size, Hình thức, Nội dung thiệp và ảnh đính kèm nếu có từ snapshot hoặc dữ liệu hợp lệ sau thanh toán.
13. Nếu có ảnh output hợp lệ, hệ thống lưu ảnh chính thức dưới định dạng PNG, không gắn logo.
14. Hệ thống tạo đúng 01 History record mới cho lần Tạo lại.
15. Hệ thống hiển thị kết quả mới để khách hàng tiếp tục Tạo lại nếu còn lượt hoặc Xác nhận thiệp.

### Alternative Flows

#### ALT-01 — Không chỉnh sửa Nội dung thiệp trước khi Tạo lại
- Khách hàng chọn "Tạo lại" mà không chỉnh sửa Nội dung thiệp.
- Hệ thống sử dụng Nội dung thiệp đã thanh toán trong snapshot Order.
- Hệ thống tiếp tục xử lý theo Main Flow.

#### ALT-02 — Chỉnh sửa Nội dung thiệp trong phạm vi đã thanh toán
- Khách hàng chỉnh sửa Nội dung thiệp trước khi chọn "Tạo lại".
- Hệ thống đếm số lượng từ của Nội dung thiệp sau chỉnh sửa.
- Số lượng từ sau chỉnh sửa nhỏ hơn hoặc bằng số lượng từ đã thanh toán.
- Hệ thống cho phép tiếp tục Tạo lại theo Main Flow.

#### ALT-03 — Reload khi AI đang xử lý
- AI Job của lần Tạo lại đang được xử lý ngầm.
- Khách hàng reload hoặc rời khỏi màn hình.
- Job AI tiếp tục chạy và không bị khởi tạo lại.
- Khi khách hàng quay lại bước tạo Thiệp Custom sau thanh toán, hệ thống hiển thị trạng thái hiện tại hoặc kết quả cuối cùng nếu job đã hoàn thành.

### Exception Flows

#### EXC-01 — Order chưa thanh toán thành công
- Khách hàng hoặc client gửi request Tạo lại.
- Backend kiểm tra trạng thái Payment của Order.
- Backend phát hiện Order chưa Payment SUCCESS.
- Hệ thống không tạo AI Job.
- Hệ thống không gọi AI service.
- Hệ thống không tạo ảnh mới, không tạo History record và không trừ quota.

#### EXC-02 — Request Tạo lại từ History
- Khách hàng xem danh sách History hoặc Chi tiết thiệp đã tạo.
- Client gửi request Tạo lại thiệp từ History.
- Backend xác định request không thuộc bước tạo Thiệp Custom sau thanh toán.
- Backend từ chối request.
- Hệ thống không tạo AI Job, không gọi AI service, không tạo ảnh mới, không tạo History record và không trừ quota.

#### EXC-03 — Template hoặc Size không còn khả dụng
- Khách hàng chọn "Tạo lại" trong bước sau thanh toán.
- Backend kiểm tra Template và Size trong snapshot Order.
- Backend phát hiện Template hoặc Size đã ngừng khả dụng.
- Hệ thống không tạo AI Job mới.
- Hệ thống không tạo History record mới.
- Hệ thống không ghi nhận quota ngày hoặc lượt generate theo mẫu hoa.
- Hệ thống thông báo Template hoặc Size không còn khả dụng để tạo thiệp.

#### EXC-04 — Nội dung thiệp vượt số lượng từ đã thanh toán
- Khách hàng chỉnh sửa Nội dung thiệp trước khi chọn "Tạo lại".
- Hệ thống đếm số lượng từ sau chỉnh sửa.
- Hệ thống phát hiện số lượng từ sau chỉnh sửa vượt quá số lượng từ đã thanh toán.
- Hệ thống không tạo AI Job.
- Hệ thống không gọi AI service.
- Hệ thống không trừ quota.
- Hệ thống yêu cầu khách hàng rút gọn nội dung hoặc xử lý cập nhật thanh toán theo nghiệp vụ được quy định riêng.

#### EXC-05 — Hết quota hoặc vượt giới hạn generate
- Khách hàng chọn "Tạo lại" trong bước sau thanh toán.
- Backend kiểm tra quota tạo thiệp AI trong ngày và giới hạn generate theo mẫu hoa nguồn.
- Backend phát hiện khách hàng hết quota ngày hoặc mẫu hoa nguồn đã vượt giới hạn generate.
- Hệ thống không tạo AI Job mới.
- Hệ thống không tạo History record mới.
- Hệ thống không trừ quota.
- Hệ thống thông báo lý do không thể tiếp tục tạo thiệp AI.

#### EXC-06 — AI không tạo được ảnh output
- Khách hàng đã gửi yêu cầu Tạo lại hợp lệ.
- Hệ thống đã gửi yêu cầu sang AI service.
- AI không tạo được ảnh output hợp lệ.
- Hệ thống tự động thử lại theo chính sách kỹ thuật.
- Nếu vẫn không tạo được ảnh sau số lần thử lại cho phép, hệ thống kết thúc xử lý thất bại.
- Hệ thống không tạo History record mới.
- Hệ thống hoàn lại đúng 01 lượt quota tạo thiệp trong ngày và 01 lượt generate thiệp theo mẫu hoa đã ghi nhận cho AI Job đó.

---

## Acceptance Criteria

### AC-001
- **Given**: Order có Thiệp Custom mới, thuộc khách hàng hiện tại và đã Payment SUCCESS.
- **When**: Khách hàng ở bước tạo Thiệp Custom sau thanh toán.
- **Then**: Hệ thống được hiển thị thao tác "Tạo lại" nếu Order còn đủ điều kiện tạo thiệp AI.

### AC-002
- **Given**: Order chưa Payment SUCCESS.
- **When**: Khách hàng hoặc client gửi request Tạo lại.
- **Then**:
  - Backend từ chối request.
  - Không tạo AI Job, không gọi AI service, không tạo History record và không trừ quota.

### AC-003
- **Given**: Khách hàng đang xem danh sách History hoặc Chi tiết thiệp đã tạo.
- **When**: Màn hình được hiển thị.
- **Then**: Hệ thống không hiển thị thao tác "Tạo lại".

### AC-004
- **Given**: Backend nhận request Tạo lại thiệp từ History.
- **When**: Request được xử lý.
- **Then**:
  - Backend phải từ chối request.
  - Không tạo AI Job, không gọi AI service, không tạo ảnh mới, không tạo History record và không trừ quota.

### AC-005
- **Given**: Khách hàng chọn "Tạo lại" trong bước tạo Thiệp Custom sau thanh toán.
- **When**: Request được backend chấp nhận.
- **Then**:
  - Hệ thống tạo AI Job mới.
  - Hệ thống ghi nhận đúng 01 lượt vào quota ngày và 01 lượt vào giới hạn generate của mẫu hoa nguồn.

### AC-006
- **Given**: Khách hàng chỉnh sửa Nội dung thiệp trước khi Tạo lại.
- **When**: Nội dung sau chỉnh sửa có số lượng từ nhỏ hơn hoặc bằng số lượng từ đã thanh toán.
- **Then**: Hệ thống cho phép tiếp tục Tạo lại nếu các điều kiện khác hợp lệ.

### AC-007
- **Given**: Khách hàng chỉnh sửa Nội dung thiệp trước khi Tạo lại.
- **When**: Nội dung sau chỉnh sửa có số lượng từ vượt quá số lượng từ đã thanh toán.
- **Then**:
  - Hệ thống không tạo AI Job.
  - Hệ thống không gọi AI service và không trừ quota.

### AC-008
- **Given**: Khách hàng đang Tạo lại trong bước sau thanh toán.
- **When**: Màn hình cho phép thao tác với cấu hình thiệp.
- **Then**: Hệ thống không cho phép chỉnh sửa Template hoặc Size đã thanh toán.

### AC-009
- **Given**: Request Tạo lại đã được chấp nhận và đã trừ quota.
- **When**: AI tạo được ảnh output hợp lệ.
- **Then**:
  - Hệ thống lưu ảnh kết quả chính thức dưới định dạng PNG và không gắn logo.
  - Hệ thống tạo đúng 01 History record mới.

### AC-010
- **Given**: Request Tạo lại đã được chấp nhận và đã trừ quota.
- **When**: AI Job thất bại sau toàn bộ retry và không tạo được ảnh output hợp lệ.
- **Then**:
  - Hệ thống không tạo History record mới.
  - Hệ thống hoàn lại đúng 01 lượt quota tạo thiệp trong ngày và 01 lượt generate thiệp theo mẫu hoa đã ghi nhận cho AI Job đó.

---

## Business Rules

| Rule ID | Tên rule | Danh mục | Phát biểu (Statement) | Điều kiện (When) | Hành vi (Then) | Ngoại lệ (Except) | Nguồn | Người sở hữu | Story liên quan | Trạng thái | Version | Ngày hiệu lực | Ghi chú / Link logic |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| [BR-044](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b4df6090-28ff-4fe4-a528-9123a7f5ad8e) | Phạm vi Tạo lại thiệp AI | Tạo ảnh | Tạo lại thiệp AI chỉ được phép thực hiện trong bước tạo Thiệp Custom sau khi Order đã thanh toán thành công; không được Tạo lại thiệp từ History. | Khách hàng chọn Tạo lại thiệp hoặc backend nhận request Tạo lại thiệp. | Backend chỉ chấp nhận request Tạo lại khi request thuộc một Order Thiệp Custom của khách hàng hiện tại, Order đã Payment SUCCESS và đang ở bước tạo Thiệp Custom sau thanh toán. Mỗi request Tạo lại hợp lệ khởi tạo một AI Job mới và được tính vào quota tạo thiệp AI. Hệ thống không hiển thị thao tác Tạo lại trên danh sách History hoặc Chi tiết thiệp đã tạo. Backend phải từ chối mọi request Tạo lại thiệp từ History. Khi request bị từ chối, hệ thống không tạo AI Job, không gọi AI service, không tạo ảnh mới, không tạo History record và không trừ quota. | Không áp dụng cho thao tác chọn thiệp đã có từ History cho Checkout, vì thao tác đó không gọi AI và không tạo History record mới. | Product discussion 2026-09-11; STORY-036; STORY-039; STORY-045 | Đức Bình | STORY-036 | Draft | v0 | 2026-09-11 | Rule này phân biệt Tạo lại hợp lệ sau thanh toán thành công với Tạo lại không hợp lệ từ History. |
| [BR-046](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d1ba26c7-d034-4be5-849a-4d427ab9f4eb) | Quyền sở hữu | Thiệp | Khách hàng chỉ được xem thiệp, tải thiệp hoặc gắn thiệp vào Checkout khi thiệp và Checkout thuộc tài khoản của mình. | Khách hàng xem thiệp trong History, tải thiệp hoặc gắn thiệp vào Checkout. | Backend kiểm tra quyền sở hữu của thiệp và Checkout trước khi cung cấp dữ liệu hoặc cập nhật lựa chọn thiệp. | Không cho phép xem, tải hoặc gắn thiệp không thuộc tài khoản khách hàng hiện tại vào Checkout. | Product discussion 2026-09-11; STORY-036; STORY-042 | Đức Bình | STORY-036 | Draft | v0 | 2026-09-11 | Chức năng Tạo lại thiệp từ History không còn được hỗ trợ nên không còn nằm trong phạm vi quyền sở hữu của rule này. |
| [BR-048](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b04b77dd-11d1-42f5-8976-b360ba3e5014) | Quota tạo thiệp | AI | Mỗi khách hàng được sử dụng tối đa 10 lượt generate thiệp AI mỗi ngày. Đối với mỗi mẫu hoa nguồn, khách hàng được thực hiện tối đa 03 lượt generate thiệp AI. Hai giới hạn này được áp dụng đồng thời. | Khách hàng thực hiện thao tác tạo thiệp hoặc Tạo lại thiệp hợp lệ làm phát sinh một AI Job tạo thiệp mới trong bước tạo Thiệp Custom sau khi Order đã thanh toán thành công. | Backend kiểm tra đồng thời số lượt generate thiệp AI khách hàng đã sử dụng trong ngày chưa đạt 10 lượt và số lượt generate thiệp AI của mẫu hoa nguồn chưa đạt 03 lượt. Nếu cả hai giới hạn đều còn lượt, hệ thống chấp nhận request, tạo AI Job và ghi nhận 01 lượt vào quota ngày và 01 lượt vào giới hạn của mẫu hoa. Nếu AI tạo được ảnh output hợp lệ, các lượt đã ghi nhận được giữ nguyên. Nếu AI Job thất bại sau toàn bộ retry và không tạo được ảnh output hợp lệ, hệ thống hoàn lại 01 lượt quota ngày và 01 lượt của mẫu hoa đã ghi nhận cho AI Job đó. | Nếu khách hàng đã sử dụng đủ 10 lượt trong ngày hoặc mẫu hoa nguồn đã sử dụng đủ 03 lượt generate thiệp, hệ thống không cho phép tạo AI Job mới. Thao tác Tạo lại thiệp từ History không còn được hỗ trợ, không được tạo AI Job và không được tính vào quota. Các thao tác Xác nhận thiệp, chọn thiệp đã tạo từ History hoặc tải xuống thiệp không làm phát sinh AI Job và không được tính vào các giới hạn trên. Quota 10 lượt/ngày được đặt lại lúc 00:00 theo múi giờ Asia/Ho_Chi_Minh và độc lập với quota tạo mẫu hoa AI. | Product discussion 2026-09-11; STORY-035; STORY-036; STORY-039 | Đức Bình | STORY-035; STORY-036; STORY-039 | Draft | v0 | 2026-09-11 | Tạo lại hợp lệ sau thanh toán thành công được tính như một lần generate thiệp AI mới. |
| [BR-154](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5573cc50-4ff5-405c-a44f-08a102297e92) | Nội dung ảnh thiệp theo hình thức | Tạo thiệp | Ảnh thiệp AI phải xử lý Nội dung thiệp/Lời chúc theo Hình thức được chọn. | Khách hàng tạo thiệp với Hình thức Gõ máy hoặc Calligraphy. | Với Hình thức Gõ máy, AI render nguyên văn Nội dung thiệp/Lời chúc lên ảnh output. Với Hình thức Calligraphy, AI không bắt buộc render Nội dung thiệp/Lời chúc lên ảnh output. Nội dung này vẫn được lưu cùng History record để Staff/Admin sử dụng khi viết tay lên thiệp. | AI không được tự ý thêm Người gửi hoặc Người nhận vào ảnh output nếu luồng đầu vào không có hai thông tin này. | Product discussion 2026-09-11; STORY-036 | Đức Bình | STORY-036 | Draft | v0 | 2026-09-11 | Rule này thay thế cách xử lý cũ render Người gửi, Người nhận và Lời chúc. |
| [BR-156](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3bb381ff-ff91-4892-9309-ceed135f603c) | Giới hạn 03 lần tạo thiệp theo mẫu hoa | AI | Trong một lần đặt hàng từ một mẫu hoa nguồn, khách hàng được thực hiện tối đa 03 lần generate thiệp AI cho mẫu hoa đó. | Khách hàng thực hiện thao tác tạo thiệp hoặc Tạo lại thiệp hợp lệ làm phát sinh một AI Job tạo thiệp mới trong bước tạo Thiệp Custom sau khi Order đã thanh toán thành công. | Backend phải đồng thời kiểm tra mẫu hoa hiện tại chưa sử dụng hết 03 lượt generate thiệp và khách hàng chưa sử dụng hết 10 lượt generate thiệp trong ngày. Nếu cả hai điều kiện đều thỏa, hệ thống cho phép tạo AI Job và ghi nhận 01 lượt vào cả hai giới hạn. | Nếu AI Job thất bại sau toàn bộ retry và không tạo được ảnh output hợp lệ, hệ thống hoàn lại lượt đã ghi nhận cho cả giới hạn theo mẫu hoa và quota ngày. Thao tác Xác nhận thiệp, chọn thiệp đã có từ History, tải xuống thiệp và request Tạo lại thiệp từ History không được tính là lượt generate. Request Tạo lại thiệp từ History phải bị từ chối theo BR-044. | Product discussion 2026-09-11; STORY-036; STORY-038; STORY-039 | Đức Bình | STORY-036; STORY-038; STORY-039 | Draft | v0 | 2026-09-11 | Tạo lại hợp lệ sau thanh toán thành công vẫn nằm trong giới hạn 03 lần generate thiệp theo mẫu hoa. |
| [BR-272](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/767acec8-626b-48e7-9352-622cf0a55a61) | Giới hạn chỉnh sửa nội dung thiệp theo số lượng từ đã thanh toán | Tạo thiệp | Sau khi Order đã thanh toán thành công, nếu khách hàng chỉnh sửa Nội dung thiệp trước khi tạo ảnh AI, nội dung sau chỉnh sửa không được vượt quá số lượng từ đã được thanh toán trong Order. | Khách hàng chỉnh sửa Nội dung thiệp của Thiệp Custom sau khi Order đã thanh toán thành công và trước khi gửi yêu cầu tạo ảnh AI, bao gồm cả thao tác Tạo lại hợp lệ trong bước sau thanh toán. | Hệ thống đếm số lượng từ của Nội dung thiệp sau chỉnh sửa theo quy tắc đếm từ hiện hành. Hệ thống so sánh số lượng từ sau chỉnh sửa với số lượng từ đã được dùng để tính tiền và lưu trong snapshot Order. Nếu số lượng từ sau chỉnh sửa nhỏ hơn hoặc bằng số lượng từ đã thanh toán, hệ thống cho phép tiếp tục tạo thiệp AI. Nếu số lượng từ sau chỉnh sửa vượt quá số lượng từ đã thanh toán, hệ thống không cho phép tiếp tục tạo thiệp AI và yêu cầu khách hàng rút gọn nội dung hoặc cập nhật thanh toán theo nghiệp vụ được quy định riêng. | Không áp dụng rule này cho thao tác chọn thiệp đã tạo từ History cho Checkout, vì thao tác đó không mở biểu mẫu chỉnh sửa nội dung và không gọi AI. | Product discussion 2026-09-11; STORY-035; STORY-039 | Đức Bình | STORY-035; STORY-039 | Draft | v0 | 2026-09-11 | Rule này không tự định nghĩa quy trình thu thêm tiền khi khách hàng muốn tăng số lượng từ sau thanh toán; quy trình đó nằm ngoài phạm vi STORY-035 và STORY-039 nếu chưa được chốt riêng. Chức năng Tạo lại thiệp từ History không còn được hỗ trợ; Tạo lại hợp lệ chỉ diễn ra trong bước tạo Thiệp Custom sau khi Order đã thanh toán thành công. |

---

## References

### Business Rules

| Rule ID | Link |
|---|---|
| BR-044 | [BR-044](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b4df6090-28ff-4fe4-a528-9123a7f5ad8e) |
| BR-046 | [BR-046](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d1ba26c7-d034-4be5-849a-4d427ab9f4eb) |
| BR-048 | [BR-048](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b04b77dd-11d1-42f5-8976-b360ba3e5014) |
| BR-154 | [BR-154](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5573cc50-4ff5-405c-a44f-08a102297e92) |
| BR-156 | [BR-156](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3bb381ff-ff91-4892-9309-ceed135f603c) |
| BR-272 | [BR-272](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/767acec8-626b-48e7-9352-622cf0a55a61) |

### Dependencies

- [STORY-039](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fc14ad2e-0f75-40f9-806d-f935e31d34f6)
- [STORY-045](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/3f89b027-0641-4888-909e-bbb18795398d)

---

## Non-Functional Requirements

- Backend kiểm tra ownership, trạng thái thanh toán, quota và idempotency trong mọi request Tạo lại.
- Trạng thái job AI phải có thể được truy xuất lại sau khi khách hàng reload.
- API metadata không trả binary ảnh hoặc expose đường dẫn storage nội bộ.
- Thông báo lỗi không được chứa stack trace hoặc thông tin kỹ thuật nhạy cảm.

---

## Out of Scope

- Tạo lại thiệp từ History.
- Chọn thiệp đã có từ History cho Checkout.
- Hoàn tất Checkout, tạo Order và thanh toán.
- Tải xuống, chia sẻ, chỉnh sửa hoặc xóa thiệp trong History.

---
