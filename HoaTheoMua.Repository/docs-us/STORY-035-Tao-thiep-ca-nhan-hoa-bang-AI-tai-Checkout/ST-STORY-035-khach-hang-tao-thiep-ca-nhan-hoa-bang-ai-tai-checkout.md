# ST — STORY-035 — Khách hàng tạo thiệp cá nhân hóa bằng AI tại Checkout — System Tests

---

## ST-035-01-01 — Smoke: Tạo thiệp AI thành công tại Checkout và xác nhận sử dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-035-01-01 |
| **Story** | STORY-035 |
| **Loại** | 1 |
| **Suite** | SMOKE |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Nháp |

**Precondition:**
- Khách hàng đã đăng nhập.
- Checkout tồn tại, thuộc khách hàng hiện tại và chưa hoàn tất.
- Checkout có mẫu hoa hiện tại đủ điều kiện tạo thiệp.
- Có ít nhất 01 Template và 01 Size đang khả dụng từ Core DB.
- Nếu Checkout/mẫu hoa hiện tại có Combo nguồn, Combo nguồn còn khả dụng tại thời điểm backend validate.
- Khách hàng còn quota tạo thiệp trong ngày.
- Mẫu hoa nguồn chưa sử dụng đủ 03 lượt generate thiệp AI.
- AI service đang hoạt động bình thường.

**Steps:**
1. Khách hàng mở chức năng tạo thiệp tại Checkout.
2. Khách hàng nhập Người gửi, Người nhận và Lời chúc hợp lệ.
3. Khách hàng chọn Template, Size và Hình thức hợp lệ.
4. Khách hàng chọn **"Tạo thiệp"**.
5. Chờ AI tạo ảnh thiệp thành công.
6. Khách hàng xem kết quả thiệp vừa tạo.
7. Khách hàng chọn **"Xác nhận"** đối với kết quả muốn sử dụng.
8. Khách hàng kiểm tra thiệp hiện tại trong Checkout và Order Summary.

**Test Data:**
- Người gửi: `Hào`.
- Người nhận: `Mẹ`.
- Lời chúc: `Chúc mẹ luôn vui vẻ và hạnh phúc.`
- Template: `Template A` đang khả dụng.
- Size: `Size A` đang khả dụng.
- Hình thức: `Gõ máy`.
- Ảnh đính kèm: không có.
- Quota tạo thiệp trong ngày trước thao tác: còn ít nhất 1 lượt.
- Số lượt generate thiệp của mẫu hoa nguồn trước thao tác: nhỏ hơn 3.

**Expected Result:**
- Backend validate thành công Checkout, input, Template, Size, Combo nguồn nếu có, quota ngày và giới hạn 03 lượt của mẫu hoa nguồn.
- Hệ thống tạo đúng 01 AI Job cho request hợp lệ.
- Hệ thống ghi nhận đúng 01 lượt quota tạo thiệp trong ngày.
- Hệ thống ghi nhận đúng 01 lượt generate thiệp của mẫu hoa nguồn.
- AI tạo được ảnh output hợp lệ.
- Hệ thống lưu ảnh kết quả chính thức dưới định dạng PNG và không gắn logo.
- Hệ thống tạo đúng 01 History record mới.
- Ảnh thiệp vừa generate chưa tự động trở thành thiệp hiện tại của Checkout khi khách hàng chưa xác nhận.
- Sau khi khách hàng chọn **"Xác nhận"**, kết quả đó trở thành thiệp hiện tại của Checkout.
- Order Summary hiển thị thiệp đã xác nhận và giá tạm tính tương ứng.

**Trace to:**
- STORY-035/AC-001
- STORY-035/AC-014
- STORY-035/BR-048
- STORY-035/BR-049
- STORY-035/BR-050
- STORY-035/BR-053
- STORY-035/BR-054

**Rationale:**
> Xác minh luồng chính: khách hàng tạo được thiệp AI hợp lệ tại Checkout, hệ thống lưu ảnh và History đúng quy tắc, trừ đúng quota, và chỉ gắn thiệp vào Checkout sau khi khách hàng xác nhận.

---

## ST-035-02-01 — Validation dữ liệu bắt buộc và giới hạn số từ

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-035-02-01 |
| **Story** | STORY-035 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Nháp |

**Precondition:**
- Khách hàng đã đăng nhập.
- Checkout hợp lệ, thuộc khách hàng hiện tại và chưa hoàn tất.
- Template, Size và Hình thức hợp lệ đang khả dụng.
- Khách hàng còn quota tạo thiệp trong ngày.
- Mẫu hoa nguồn chưa sử dụng đủ 03 lượt generate thiệp AI.

**Steps:**
1. Khách hàng mở form tạo thiệp tại Checkout.
2. Khách hàng lần lượt nhập các bộ dữ liệu trong phần Test Data.
3. Với mỗi bộ dữ liệu, khách hàng chọn **"Tạo thiệp"**.
4. Kiểm tra lỗi hiển thị trên UI.
5. Kiểm tra backend không tạo AI Job, không tạo History record và không trừ quota với dữ liệu không hợp lệ.

**Test Data:**

| # | Người gửi | Người nhận | Lời chúc | Kỳ vọng |
|---|---|---|---|---|
| 1 | _(rỗng)_ | `Mẹ` | `Chúc mẹ vui vẻ.` | Lỗi Người gửi |
| 2 | `Hào` | _(rỗng)_ | `Chúc mẹ vui vẻ.` | Lỗi Người nhận |
| 3 | `Hào` | `Mẹ` | _(rỗng)_ | Lỗi Lời chúc |
| 4 | 21 từ | `Mẹ` | `Chúc mẹ vui vẻ.` | Lỗi Người gửi vượt 20 từ |
| 5 | `Hào` | 21 từ | `Chúc mẹ vui vẻ.` | Lỗi Người nhận vượt 20 từ |
| 6 | `Hào` | `Mẹ` | 101 từ | Lỗi Lời chúc vượt 100 từ |
| 7 | `  Hào  ` | `  Mẹ  ` | `  Chúc mẹ vui vẻ.  ` | Hợp lệ sau khi trim |
| 8 | `Hào` | `Mẹ` | Nội dung có nhiều khoảng trắng, tab, xuống dòng liên tiếp nhưng không vượt giới hạn từ | Hợp lệ |

**Expected Result:**
- Hệ thống áp dụng đúng quy tắc đếm từ: trim khoảng trắng đầu/cuối, nhiều khoảng trắng/tab/xuống dòng liên tiếp được tính là một dấu phân tách.
- Dấu câu đi liền với một từ không được tính thành từ riêng.
- Nội dung chỉ gồm khoảng trắng được tính là 0 từ.
- Với dữ liệu thiếu hoặc vượt giới hạn, backend từ chối request.
- Với dữ liệu không hợp lệ, hệ thống không bắt đầu AI.
- Với dữ liệu không hợp lệ, hệ thống không tạo History record.
- Với dữ liệu không hợp lệ, hệ thống không trừ quota.
- Hệ thống hiển thị lỗi tại trường tương ứng.
- Với dữ liệu hợp lệ sau trim và không vượt giới hạn, hệ thống cho phép tiếp tục tạo thiệp.

**Trace to:**
- STORY-035/AC-004
- STORY-035/EXC-02
- STORY-035/BR-051

**Rationale:**
> Xác minh dữ liệu bắt buộc và giới hạn số từ được kiểm tra đúng ở cả Frontend và Backend trước khi cho phép tạo AI Job.

---

## ST-035-03-01 — Hình thức Gõ máy render nguyên văn nội dung lên ảnh thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-035-03-01 |
| **Story** | STORY-035 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Nháp |

**Precondition:**
- Checkout hợp lệ và thuộc khách hàng hiện tại.
- Khách hàng còn quota tạo thiệp trong ngày.
- Template và Size đang khả dụng.
- Khách hàng chọn Hình thức **Gõ máy**.
- Người gửi, Người nhận và Lời chúc hợp lệ.

**Steps:**
1. Khách hàng mở chức năng tạo thiệp tại Checkout.
2. Khách hàng nhập nội dung Người gửi, Người nhận và Lời chúc hợp lệ.
3. Khách hàng chọn Hình thức **Gõ máy**.
4. Khách hàng chọn **"Tạo thiệp"**.
5. Chờ AI tạo thiệp thành công.
6. Kiểm tra nội dung được render trên ảnh output.
7. Kiểm tra dữ liệu được lưu trong History record.

**Test Data:**
- Người gửi: `Thiên Hào`.
- Người nhận: `Khánh Linh`.
- Lời chúc: `Chúc bạn một ngày thật vui và nhiều năng lượng.`
- Hình thức: `Gõ máy`.

**Expected Result:**
- AI tạo thiệp thành công.
- Ảnh output hiển thị nguyên văn Người gửi.
- Ảnh output hiển thị nguyên văn Người nhận.
- Ảnh output hiển thị nguyên văn Lời chúc.
- Hệ thống không tự động sửa, rút gọn, dịch hoặc thay đổi Người gửi, Người nhận và Lời chúc.
- History record lưu đúng nội dung khách hàng đã nhập.

**Trace to:**
- STORY-035/AC-003
- STORY-035/BR-154
- STORY-035/BR-057

**Rationale:**
> Xác minh khi khách hàng chọn Gõ máy, nội dung bắt buộc phải được render nguyên văn lên ảnh kết quả và không bị AI tự ý thay đổi.

---

## ST-035-04-01 — Hình thức Calligraphy không render nội dung lên ảnh nhưng vẫn lưu History

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-035-04-01 |
| **Story** | STORY-035 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Nháp |

**Precondition:**
- Checkout hợp lệ và thuộc khách hàng hiện tại.
- Khách hàng còn quota tạo thiệp trong ngày.
- Template và Size đang khả dụng.
- Khách hàng chọn Hình thức **Calligraphy**.
- Người gửi, Người nhận và Lời chúc hợp lệ.

**Steps:**
1. Khách hàng mở chức năng tạo thiệp tại Checkout.
2. Khách hàng nhập Người gửi, Người nhận và Lời chúc hợp lệ.
3. Khách hàng chọn Hình thức **Calligraphy**.
4. Khách hàng chọn **"Tạo thiệp"**.
5. Chờ AI tạo thiệp thành công.
6. Kiểm tra ảnh output.
7. Kiểm tra History record của thiệp.

**Test Data:**
- Người gửi: `Thiên Hào`.
- Người nhận: `Khánh Linh`.
- Lời chúc: `Chúc bạn luôn hạnh phúc.`
- Hình thức: `Calligraphy`.

**Expected Result:**
- AI tạo thiệp thành công.
- Ảnh output không hiển thị Người gửi.
- Ảnh output không hiển thị Người nhận.
- Ảnh output không hiển thị Lời chúc.
- Người gửi, Người nhận và Lời chúc vẫn được lưu cùng History record của thiệp.
- Các nội dung này không bị mất để Staff/Admin có thể sử dụng khi viết tay lên thiệp.

**Trace to:**
- STORY-035/AC-013
- STORY-035/BR-154

**Rationale:**
> Xác minh Hình thức Calligraphy không render nội dung bắt buộc lên ảnh output nhưng vẫn lưu đầy đủ dữ liệu để phục vụ thao tác xử lý thiệp sau đó.

---

## ST-035-05-01 — Ảnh đính kèm hợp lệ được giữ nguyên nội dung khi tạo thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-035-05-01 |
| **Story** | STORY-035 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Nháp |

**Precondition:**
- Checkout hợp lệ và thuộc khách hàng hiện tại.
- Khách hàng còn quota tạo thiệp trong ngày.
- Template và Size đang khả dụng.
- Khách hàng có 01 ảnh đính kèm hợp lệ.
- AI service có thể tạo ảnh output hợp lệ.

**Steps:**
1. Khách hàng mở chức năng tạo thiệp tại Checkout.
2. Khách hàng nhập đầy đủ thông tin bắt buộc hợp lệ.
3. Khách hàng chọn Template, Size và Hình thức hợp lệ.
4. Khách hàng upload 01 ảnh đính kèm hợp lệ.
5. Khách hàng chọn **"Tạo thiệp"**.
6. Chờ AI tạo thiệp thành công.
7. Kiểm tra ảnh đính kèm trong output.

**Test Data:**
- Ảnh đính kèm: `attachment-a.png`.
- Định dạng: `PNG`.
- Dung lượng: nhỏ hơn hoặc bằng `10 MB`.
- Số lượng ảnh: `1`.

**Expected Result:**
- Hệ thống chấp nhận ảnh đính kèm hợp lệ.
- AI giữ nguyên nội dung gốc của ảnh.
- AI không cắt mất chủ thể.
- AI không xoay ảnh.
- AI không đổi màu ảnh.
- AI không thêm/xóa hoặc làm biến dạng nội dung ảnh.
- AI được phép scale đồng dạng hoặc thêm khoảng đệm để phù hợp Template.
- Ảnh output được lưu thành công cùng History record.

**Trace to:**
- STORY-035/AC-005
- STORY-035/BR-052
- STORY-035/BR-057

**Rationale:**
> Xác minh ảnh đính kèm hợp lệ được AI sử dụng trong phạm vi Template nhưng không bị thay đổi nội dung gốc.

---

## ST-035-06-01 — File upload không hợp lệ bị từ chối và vẫn cho tiếp tục không có ảnh

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-035-06-01 |
| **Story** | STORY-035 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Nháp |

**Precondition:**
- Khách hàng đã đăng nhập.
- Checkout hợp lệ và thuộc khách hàng hiện tại.
- Khách hàng đang ở màn tạo thiệp tại Checkout.
- Các dữ liệu bắt buộc còn lại hợp lệ.

**Steps:**
1. Khách hàng chọn ảnh đính kèm không hợp lệ.
2. Kiểm tra thông báo lỗi.
3. Khách hàng chọn lại ảnh khác hợp lệ.
4. Khách hàng xóa ảnh đính kèm.
5. Khách hàng tiếp tục tạo thiệp không có ảnh đính kèm.

**Test Data:**

| # | File upload | Kỳ vọng |
|---|---|---|
| 1 | File định dạng không phải PNG/JPG | Bị từ chối |
| 2 | File PNG lớn hơn 10 MB | Bị từ chối |
| 3 | File JPG lớn hơn 10 MB | Bị từ chối |
| 4 | Upload nhiều hơn 01 ảnh | Bị từ chối |
| 5 | Không chọn ảnh đính kèm | Vẫn được tạo thiệp nếu dữ liệu bắt buộc hợp lệ |

**Expected Result:**
- Hệ thống từ chối file không đúng định dạng.
- Hệ thống từ chối file vượt quá 10 MB.
- Hệ thống từ chối khi số lượng ảnh vượt quá 01 ảnh.
- Hệ thống hiển thị lý do file không hợp lệ.
- Hệ thống cho phép khách hàng chọn file khác.
- Hệ thống cho phép khách hàng tiếp tục tạo thiệp không có ảnh đính kèm nếu các dữ liệu bắt buộc còn lại hợp lệ.
- Với file không hợp lệ, hệ thống không gửi yêu cầu sang AI.
- Với file không hợp lệ, hệ thống không tạo History record và không trừ quota.

**Trace to:**
- STORY-035/AC-006
- STORY-035/AC-012
- STORY-035/EXC-03
- STORY-035/BR-052

**Rationale:**
> Xác minh quy tắc upload ảnh đính kèm: ảnh không bắt buộc, nhưng nếu upload thì phải đúng định dạng, dung lượng và số lượng cho phép.

---

## ST-035-07-01 — Kiểm tra quota 10 lượt/ngày và giới hạn 03 lượt theo mẫu hoa nguồn

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-035-07-01 |
| **Story** | STORY-035 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Nháp |

**Precondition:**
- Khách hàng đã đăng nhập.
- Checkout hợp lệ và thuộc khách hàng hiện tại.
- Dữ liệu tạo thiệp hợp lệ.
- Template và Size đang khả dụng.

**Steps:**
1. Thực hiện tạo thiệp khi khách hàng còn quota ngày và mẫu hoa nguồn còn lượt.
2. Kiểm tra quota ngày và số lượt của mẫu hoa nguồn sau khi request được chấp nhận.
3. Thiết lập khách hàng đã sử dụng đủ 10 lượt tạo thiệp trong ngày.
4. Khách hàng thử tạo thiệp mới.
5. Thiết lập mẫu hoa nguồn đã sử dụng đủ 03 lượt generate thiệp AI.
6. Khách hàng thử tạo thiệp mới cho cùng mẫu hoa nguồn.

**Test Data:**
- Quota ngày tối đa: `10` lượt/khách hàng/ngày.
- Giới hạn theo mẫu hoa nguồn: `03` lượt/mẫu hoa.
- Scenario A: quota ngày còn lượt, mẫu hoa nguồn còn lượt.
- Scenario B: quota ngày đã đủ `10/10`.
- Scenario C: mẫu hoa nguồn đã đủ `3/3`.

**Expected Result:**
- Scenario A: hệ thống chấp nhận request hợp lệ và tạo AI Job.
- Scenario A: hệ thống ghi nhận đúng 01 lượt vào quota ngày.
- Scenario A: hệ thống ghi nhận đúng 01 lượt vào giới hạn của mẫu hoa nguồn.
- Scenario B: hệ thống không tạo AI Job mới.
- Scenario B: hệ thống không gửi yêu cầu sang AI.
- Scenario B: hệ thống không tạo History record.
- Scenario B: hệ thống không ghi nhận thêm lượt quota.
- Scenario B: hệ thống thông báo khách hàng đã sử dụng hết quota tạo thiệp trong ngày.
- Scenario C: hệ thống không tạo AI Job mới.
- Scenario C: hệ thống không trừ quota ngày.
- Scenario C: hệ thống không tạo History record.
- Scenario C: hệ thống thông báo khách hàng đã sử dụng hết số lượt tạo thiệp cho mẫu hoa này.

**Trace to:**
- STORY-035/EXC-01
- STORY-035/EXC-09
- STORY-035/BR-048
- STORY-035/BR-049

**Rationale:**
> Xác minh hệ thống áp dụng đồng thời quota 10 lượt/ngày của khách hàng và giới hạn 03 lượt generate thiệp AI theo mẫu hoa nguồn.

---

## ST-035-08-01 — Template, Size, mẫu thiệp hoặc giá không còn khả dụng thì chặn tạo thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-035-08-01 |
| **Story** | STORY-035 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Nháp |

**Precondition:**
- Khách hàng đã đăng nhập.
- Checkout hợp lệ và thuộc khách hàng hiện tại.
- Khách hàng đã nhập dữ liệu bắt buộc hợp lệ.
- Khách hàng còn quota tạo thiệp trong ngày.
- Khách hàng đã chọn Template, Size và Hình thức.

**Steps:**
1. Khách hàng mở form tạo thiệp và chọn Template/Size khi dữ liệu còn khả dụng.
2. Trước khi khách hàng gửi request, chuyển Template sang không còn khả dụng.
3. Khách hàng chọn **"Tạo thiệp"**.
4. Khôi phục dữ liệu để chạy scenario độc lập.
5. Chuyển Size sang không còn khả dụng hoặc không tìm thấy giá hiện hành.
6. Khách hàng chọn **"Tạo thiệp"**.
7. Khôi phục dữ liệu để chạy scenario độc lập.
8. Chuyển mẫu thiệp đã chọn sang Inactive hoặc xóa mềm.
9. Khách hàng chọn **"Tạo thiệp"**.

**Test Data:**
- Template: `Template A`.
- Size: `Size A`.
- Mẫu thiệp: `Mẫu thiệp A`.
- Scenario A: Template không còn khả dụng.
- Scenario B: Size không còn khả dụng hoặc không có giá hiện hành.
- Scenario C: Mẫu thiệp không còn Active hoặc đã bị xóa mềm.

**Expected Result:**
- Backend kiểm tra lại trạng thái khả dụng của Template và Size từ Core system.
- Backend kiểm tra lại mẫu thiệp vẫn đang Active và chưa bị xóa mềm.
- Nếu Template không còn khả dụng, hệ thống không bắt đầu AI.
- Nếu Size không còn khả dụng hoặc không có giá hiện hành, hệ thống không cho tiếp tục tạo thiệp.
- Nếu mẫu thiệp không còn khả dụng, hệ thống không tạo thiệp bằng mẫu đó.
- Hệ thống không gửi yêu cầu sang AI.
- Hệ thống không tạo History record.
- Hệ thống không trừ quota.
- Hệ thống yêu cầu khách hàng chọn Template, Size hoặc mẫu thiệp hiện còn khả dụng.
- Hệ thống hiển thị thông báo phù hợp với nguyên nhân bị chặn.

**Trace to:**
- STORY-035/AC-007
- STORY-035/AC-015
- STORY-035/AC-016
- STORY-035/EXC-04
- STORY-035/EXC-010
- STORY-035/EXC-011
- STORY-035/BR-054

**Rationale:**
> Xác minh các dữ liệu cấu hình từ Core DB phải còn khả dụng tại thời điểm backend validate; nếu không, hệ thống không được tạo thiệp hoặc trừ quota.

---

## ST-035-09-01 — Reload hoặc rời màn hình không hủy AI Job đang xử lý

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-035-09-01 |
| **Story** | STORY-035 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Nháp |

**Precondition:**
- Checkout hợp lệ và thuộc khách hàng hiện tại.
- Khách hàng đã gửi request tạo thiệp hợp lệ.
- Backend đã tạo AI Job.
- AI Job đang ở trạng thái **"Đang tạo"**.
- Job chưa có ảnh output hợp lệ.

**Steps:**
1. Khách hàng bắt đầu tạo thiệp AI.
2. Hệ thống hiển thị trạng thái **"Đang tạo"**.
3. Khách hàng reload hoặc rời khỏi màn hình.
4. Khách hàng quay lại Checkout hoặc màn lịch sử liên quan.
5. Kiểm tra trạng thái của AI Job.
6. Chờ AI Job hoàn tất thành công.
7. Kiểm tra History record và quota.

**Test Data:**
- AI Job: `CARD-AI-JOB-A`.
- Trạng thái ban đầu: **"Đang tạo"**.

**Expected Result:**
- Reload hoặc rời màn hình không làm hủy AI Job.
- Hệ thống không khởi tạo lại AI Job mới.
- AI Job tiếp tục xử lý ngầm.
- Khi khách hàng quay lại, hệ thống hiển thị trạng thái **"Đang tạo"** tại màn lịch sử liên quan.
- Khi chưa có ảnh output hợp lệ, hệ thống chưa tạo History item.
- Lượt quota đã được trừ khi request được chấp nhận.
- Nếu AI Job tạo ảnh thành công, hệ thống tạo đúng 01 History record và giữ nguyên lượt đã ghi nhận.

**Trace to:**
- STORY-035/AC-011
- STORY-035/ALT-01

**Rationale:**
> Xác minh AI Job tạo thiệp đã được chấp nhận tiếp tục chạy độc lập với việc khách hàng còn mở màn hình hay không.

---

## ST-035-10-01 — AI không tạo được ảnh sau retry thì hoàn quota đúng một lần

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-035-10-01 |
| **Story** | STORY-035 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Nháp |

**Precondition:**
- Khách hàng đã gửi yêu cầu tạo thiệp hợp lệ.
- Backend đã tạo AI Job và gửi yêu cầu sang AI.
- Hệ thống đã ghi nhận quota ngày và lượt generate của mẫu hoa nguồn cho AI Job đó.
- Môi trường test có khả năng mô phỏng AI không tạo được ảnh output hợp lệ.

**Steps:**
1. Khách hàng gửi yêu cầu tạo thiệp hợp lệ.
2. Hệ thống chấp nhận request và tạo AI Job.
3. AI không tạo được ảnh output hợp lệ.
4. Hệ thống tự động thử lại theo chính sách kỹ thuật.
5. AI tiếp tục không tạo được ảnh sau toàn bộ số lần thử lại cho phép.
6. Kiểm tra trạng thái AI Job, History record và quota.
7. Kiểm tra việc hoàn quota khi cùng AI Job phát sinh nhiều lỗi kết thúc.

**Test Data:**
- Quota tạo thiệp trong ngày trước request: còn ít nhất 1 lượt.
- Lượt generate thiệp của mẫu hoa nguồn trước request: nhỏ hơn 3.
- AI Job thất bại sau toàn bộ retry.
- Không có ảnh output hợp lệ.

**Expected Result:**
- Hệ thống kết thúc xử lý thất bại sau toàn bộ retry.
- Hệ thống không tạo History record.
- Hệ thống hoàn lại đúng 01 lượt quota tạo thiệp trong ngày.
- Hệ thống hoàn lại đúng 01 lượt generate thiệp của mẫu hoa nguồn đã ghi nhận cho AI Job đó.
- Mỗi AI Job chỉ được hoàn mỗi loại lượt tối đa 01 lần.
- Hệ thống thông báo khách hàng thử lại sau.
- Không có ảnh output được lưu làm kết quả chính thức.

**Trace to:**
- STORY-035/AC-002
- STORY-035/EXC-05
- STORY-035/BR-048
- STORY-035/BR-049

**Rationale:**
> Xác minh lỗi AI không tạo được ảnh được xử lý bằng retry; nếu vẫn thất bại, hệ thống hoàn đúng quota đã ghi nhận và không tạo History record rỗng.

---

## ST-035-11-01 — Request tạo thiệp bị gửi trùng chỉ xử lý một lần

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-035-11-01 |
| **Story** | STORY-035 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Nháp |

**Precondition:**
- Khách hàng đã đăng nhập.
- Checkout hợp lệ và thuộc khách hàng hiện tại.
- Dữ liệu tạo thiệp hợp lệ.
- Khách hàng còn quota tạo thiệp trong ngày.
- Mẫu hoa nguồn chưa sử dụng đủ 03 lượt generate thiệp AI.
- Có cơ chế nhận diện request trùng theo idempotency.

**Steps:**
1. Khách hàng nhập đầy đủ dữ liệu tạo thiệp hợp lệ.
2. Khách hàng nhấn **"Tạo thiệp"** nhiều lần liên tiếp hoặc cùng một request được gửi lại do lỗi mạng/trình duyệt.
3. Backend nhận request tạo thiệp bị trùng.
4. Kiểm tra số AI Job được tạo.
5. Kiểm tra số lần gọi AI.
6. Kiểm tra số ảnh, History record và quota được ghi nhận.

**Test Data:**
- Cùng một request hợp lệ được gửi lại nhiều lần.
- Idempotency key/hash của các request trùng nhau.

**Expected Result:**
- Backend nhận diện request trùng theo cơ chế idempotent.
- Hệ thống không gọi AI nhiều lần cho cùng một request hợp lệ.
- Hệ thống không tạo nhiều AI Job cho cùng request.
- Hệ thống không tạo nhiều ảnh.
- Hệ thống không tạo nhiều History record.
- Hệ thống không ghi nhận quota nhiều lần.
- Hệ thống chỉ trả về kết quả tương ứng với request hợp lệ đã được xử lý.

**Trace to:**
- STORY-035/AC-009
- STORY-035/EXC-07
- STORY-035/BR-055

**Rationale:**
> Xác minh backend chống request trùng để không phát sinh nhiều job AI, ảnh, History record hoặc lượt quota cho cùng một thao tác tạo thiệp.

---

## ST-035-12-01 — Checkout không hợp lệ hoặc không thuộc khách hàng thì từ chối tạo thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-035-12-01 |
| **Story** | STORY-035 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Nháp |

**Precondition:**
- Có khách hàng A và khách hàng B.
- Có Checkout hợp lệ thuộc khách hàng B.
- Khách hàng A đã đăng nhập.
- Có thể mô phỏng Checkout không tồn tại hoặc đã hoàn tất.

**Steps:**
1. Khách hàng A thực hiện tạo thiệp với Checkout thuộc khách hàng B.
2. Quan sát phản hồi.
3. Khách hàng A thực hiện tạo thiệp với Checkout không tồn tại.
4. Quan sát phản hồi.
5. Khách hàng A thực hiện tạo thiệp với Checkout đã hoàn tất.
6. Quan sát phản hồi.
7. Kiểm tra AI Job, History record và quota sau mỗi scenario.

**Test Data:**
- Người dùng hiện tại: `Khách hàng A`.
- Checkout thuộc tài khoản khác: `CHECKOUT-B`.
- Checkout không tồn tại: `CHECKOUT-NOT-FOUND`.
- Checkout đã hoàn tất: `CHECKOUT-COMPLETED`.

**Expected Result:**
- Backend từ chối Checkout không tồn tại.
- Backend từ chối Checkout không thuộc khách hàng hiện tại.
- Backend từ chối Checkout đã hoàn tất.
- Hệ thống không gửi yêu cầu sang AI.
- Hệ thống không tạo AI Job.
- Hệ thống không tạo History record.
- Hệ thống không trừ quota.
- Hệ thống hiển thị lỗi tương ứng với nguyên nhân Checkout không hợp lệ.
- Hệ thống không để lộ dữ liệu của Checkout hoặc khách hàng khác.

**Trace to:**
- STORY-035/AC-010
- STORY-035/EXC-08
- STORY-035/BR-056

**Rationale:**
> Xác minh backend bắt buộc kiểm tra ownership và trạng thái Checkout trước khi cho phép tạo thiệp trong Checkout.

---

## ST-035-13-01 — Combo nguồn chỉ được tham chiếu đúng phạm vi và xử lý đúng khi không còn khả dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | ST-035-13-01 |
| **Story** | STORY-035 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Nháp |

**Precondition:**
- Khách hàng đã đăng nhập.
- Checkout hợp lệ và thuộc khách hàng hiện tại.
- Mẫu hoa trong Checkout có Combo nguồn.
- Template, Size, Hình thức và nội dung bắt buộc hợp lệ.
- Khách hàng còn quota tạo thiệp trong ngày.
- Mẫu hoa nguồn chưa sử dụng đủ 03 lượt generate thiệp AI.

**Steps:**
1. Khách hàng tạo thiệp khi Combo nguồn còn khả dụng.
2. Kiểm tra input gửi sang AI và ảnh output.
3. Chạy scenario Combo nguồn bị xóa mềm, hết hàng hoặc ngừng khả dụng trước khi backend validate.
4. Khách hàng gửi yêu cầu tạo thiệp.
5. Quan sát phản hồi.
6. Chạy scenario backend đã validate thành công và đã tạo AI Job.
7. Trong lúc AI đang xử lý, Admin xóa mềm hoặc ngừng khả dụng Combo nguồn.
8. Chờ AI Job hoàn tất.
9. Kiểm tra ảnh kết quả, History record và quota.

**Test Data:**
- Combo nguồn: `Combo A`.
- Template: `Template A`.
- Size: `Size A`.
- Hình thức: `Gõ máy`.
- Scenario A: Combo nguồn còn khả dụng tại thời điểm validate.
- Scenario B: Combo nguồn không còn khả dụng trước khi backend validate.
- Scenario C: Combo nguồn bị xóa mềm/ngừng khả dụng sau khi backend đã validate và đã tạo AI Job.

**Expected Result:**
- Scenario A: AI được phép sử dụng thông tin Combo nguồn làm ngữ cảnh tham chiếu để tạo ảnh thiệp phù hợp với sản phẩm hoa.
- Scenario A: Combo nguồn không được làm thay đổi Template.
- Scenario A: Combo nguồn không được làm thay đổi Size.
- Scenario A: Combo nguồn không được làm thay đổi Hình thức.
- Scenario A: Combo nguồn không được làm thay đổi nội dung bắt buộc.
- Scenario A: Combo nguồn không được làm thay đổi ảnh đính kèm của khách hàng.
- Scenario B: hệ thống không tạo AI Job.
- Scenario B: hệ thống không gửi yêu cầu sang AI.
- Scenario B: hệ thống không tạo History record.
- Scenario B: hệ thống không trừ quota.
- Scenario B: hệ thống thông báo Combo nguồn không còn khả dụng và yêu cầu khách hàng chọn lại sản phẩm/mẫu hoa phù hợp.
- Scenario C: hệ thống tiếp tục xử lý AI Job theo dữ liệu snapshot tại thời điểm validate.
- Scenario C: nếu AI tạo được ảnh output hợp lệ, hệ thống lưu ảnh kết quả, tạo đúng 01 History record và giữ nguyên lượt generate đã ghi nhận.
- Từ các request tạo thiệp mới sau đó, Combo nguồn đã xóa mềm/ngừng khả dụng không được validate là nguồn hợp lệ.

**Trace to:**
- STORY-035/AC-017
- STORY-035/AC-018
- STORY-035/AC-019
- STORY-035/ALT-03
- STORY-035/EXC-012
- STORY-035/BR-057

**Rationale:**
> Xác minh Combo nguồn chỉ là ngữ cảnh tham chiếu cho AI, không được làm thay đổi dữ liệu khách hàng đã chọn; đồng thời hệ thống xử lý đúng khác biệt giữa Combo không còn khả dụng trước validate và sau khi AI Job đã bắt đầu.
