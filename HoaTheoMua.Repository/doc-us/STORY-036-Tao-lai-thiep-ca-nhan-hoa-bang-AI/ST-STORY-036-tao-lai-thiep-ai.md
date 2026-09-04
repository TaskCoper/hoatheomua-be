# ST — STORY-036 — Khách hàng tạo lại thiệp cá nhân hóa bằng AI — System Tests

---

## ST-036-01-01 — Smoke: Tạo lại thiệp thành công từ lịch sử

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-036-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/afc523ca-fbf3-4e8a-bad7-91c5facaa9bc) |
| **Story** | STORY-036 |
| **Loại** | 1 |
| **Suite** | SMOKE |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Thiệp nguồn tồn tại và thuộc khách hàng hiện tại.
- Thiệp nguồn đã được tạo ảnh thành công.
- Template và Size của thiệp nguồn vẫn còn khả dụng.
- Khách hàng chưa sử dụng hết 10 lượt tạo ảnh thiệp trong ngày.
- Mẫu hoa nguồn chưa sử dụng đủ 3 lượt tạo ảnh thiệp.
- Không có quá trình tạo ảnh AI liên quan đang xử lý.
- AI service hoạt động bình thường.

**Steps:**
1. Mở Chi tiết thiệp A từ lịch sử.
2. Chọn **"Tạo lại"**.
3. Quan sát hành vi của hệ thống trước khi gửi AI.
4. Chờ AI tạo ảnh thành công.
5. Quan sát kết quả mới.
6. Quan sát lịch sử.
7. Quan sát thiệp nguồn.
8. Quan sát lượt sử dụng.

**Test Data:**
- Khách hàng: khách hàng A.
- Thiệp nguồn: thiệp A.
- Template: TEMPLATE-A · Size: SIZE-A.
- Người gửi: `Nguyễn An` · Người nhận: `Trần Bình`.
- Lời chúc: `Chúc bạn sinh nhật vui vẻ`.
- Hình thức: Gõ máy · Ảnh đính kèm: Không.
- Lượt sử dụng trong ngày trước thao tác: 2/10.
- Lượt theo mẫu hoa: 1/3.

**Expected Result:**
- Hệ thống không mở biểu mẫu cho khách hàng chỉnh sửa dữ liệu.
- Hệ thống sử dụng nguyên dữ liệu của thiệp A.
- Khách hàng thấy quá trình tạo ảnh AI mới bắt đầu.
- Ghi nhận đúng 1 lượt sử dụng trong ngày.
- Ghi nhận đúng 1 lượt tạo ảnh của mẫu hoa nguồn.
- Ảnh kết quả mới được lưu dưới định dạng PNG và không gắn logo.
- Hệ thống tạo đúng 1 mục lịch sử mới.
- Thiệp A vẫn được giữ nguyên trong lịch sử. Không ghi đè hoặc xóa thiệp nguồn.
- Kết quả mới được hiển thị cho khách hàng.
- Vì thao tác xuất phát từ lịch sử chung, kết quả mới không tự động được gắn vào Checkout.

**Trace to:**
- [STORY-036/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338) · [STORY-036/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)
- [STORY-036/BR-044](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338) · [STORY-036/BR-048](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)

**Rationale:**
> Xác minh trải nghiệm khách hàng: khách hàng có thể Tạo lại thành công từ một thiệp thuộc lịch sử của mình; hệ thống tạo một quá trình tạo ảnh AI mới bằng nguyên dữ liệu của thiệp nguồn, tạo kết quả và mục lịch sử mới nhưng không ghi đè hoặc thay đổi thiệp nguồn.

---

## ST-036-02-01 — Dữ liệu đầu vào được giữ nguyên khi Tạo lại (Gõ máy + Calligraphy)

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-036-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a8af8ce9-4eb2-4bcd-a5ef-3814beea9bd7) |
| **Story** | STORY-036 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Có thiệp nguồn thuộc khách hàng.
- Thiệp nguồn có dữ liệu hợp lệ.
- Template và Size vẫn còn khả dụng.
- Khách hàng còn lượt sử dụng.

**Steps — Tình huống A (Gõ máy có ảnh):**
1. Mở thiệp nguồn.
2. Chọn **"Tạo lại"**.
3. Chờ tạo ảnh thành công.
4. Quan sát ảnh output và dữ liệu lịch sử mới.

**Steps — Tình huống B (Calligraphy không có ảnh):**
1. Mở thiệp nguồn Calligraphy không có ảnh.
2. Chọn **"Tạo lại"**.
3. Chờ tạo ảnh thành công.
4. Quan sát output và lịch sử.

**Test Data:**

| Scenario | Người gửi | Người nhận | Lời chúc | Hình thức | Ảnh đính kèm |
|---|---|---|---|---|---|
| A — Gõ máy có ảnh | Nguyễn An | Trần Bình | Chúc bạn luôn vui vẻ | Gõ máy | IMAGE-A |
| B — Calligraphy không ảnh | _(hợp lệ)_ | _(hợp lệ)_ | _(hợp lệ)_ | Calligraphy | Không |

**Expected Result — Tình huống A:**
- Người gửi, Người nhận, Lời chúc được giữ nguyên chính xác.
- Với Gõ máy, các nội dung này được render nguyên văn lên ảnh.
- IMAGE-A được giữ nguyên nội dung gốc. Không cắt mất chủ thể, xoay, đổi màu, thêm/xóa hoặc làm biến dạng.
- Chỉ được scale đồng dạng hoặc thêm khoảng đệm khi cần phù hợp Template.

**Expected Result — Tình huống B:**
- Tạo lại vẫn thành công dù không có ảnh đính kèm.
- Với Calligraphy, Người gửi, Người nhận và Lời chúc không xuất hiện trên ảnh output.
- Các nội dung này vẫn được giữ trong mục lịch sử mới.

**Trace to:**
- [STORY-036/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338) · [STORY-036/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)
- [STORY-036/BR-154](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)

**Rationale:**
> Xác minh trải nghiệm khách hàng: thao tác Tạo lại không làm thay đổi dữ liệu đầu vào của thiệp nguồn, bao gồm Người gửi, Người nhận, Lời chúc và ảnh đính kèm nếu có.

---

## ST-036-03-01 — Tạo lại trong Checkout: xác nhận thiệp mới thay thế thiệp hiện tại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-036-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/01c03493-9e0c-47c0-8da2-a00b12f6457e) |
| **Story** | STORY-036 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout tồn tại, thuộc khách hàng hiện tại và chưa hoàn tất.
- Checkout đang có thiệp A là thiệp hiện tại.
- Thiệp A thuộc khách hàng.
- Template và Size còn khả dụng.
- Khách hàng còn lượt sử dụng.

**Steps:**
1. Trong Checkout, mở thiệp A.
2. Chọn **"Tạo lại"**.
3. Chờ AI tạo ảnh thành công → thiệp B.
4. Quan sát Checkout trước khi Xác nhận.
5. Quan sát lịch sử.
6. Chọn **"Xác nhận"** tại thiệp B.
7. Quan sát lại Checkout và lịch sử.

**Expected Result:**
- Hệ thống tạo quá trình tạo ảnh AI mới.
- Thiệp B được lưu thành một mục lịch sử mới.
- **Trước khi Xác nhận:** thiệp A vẫn là thiệp hiện tại của Checkout; thiệp B chưa tự động được gắn vào Checkout.
- **Khi khách hàng chọn "Xác nhận":** thiệp B trở thành thiệp hiện tại; thiệp A bị gỡ khỏi Checkout.
- Thiệp A vẫn được giữ trong lịch sử.
- Checkout chỉ có một thiệp hiện tại.
- Thao tác Xác nhận không làm phát sinh thêm quá trình tạo ảnh AI hay lượt sử dụng.

**Trace to:**
- [STORY-036/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338) · [STORY-036/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)

**Rationale:**
> Xác minh trải nghiệm khách hàng: kết quả Tạo lại trong Checkout được lưu vào lịch sử nhưng không tự động thay thế thiệp hiện tại; chỉ khi khách hàng chọn "Xác nhận" thì thiệp mới mới được sử dụng.

---

## ST-036-04-01 — Quá trình tạo ảnh AI tiếp tục khi reload/rời trang

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-036-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/920014c5-6d38-4d15-8a3a-b9794f0fffd9) |
| **Story** | STORY-036 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Thao tác Tạo lại hợp lệ đã được chấp nhận.
- Quá trình tạo ảnh AI đang ở trạng thái xử lý.

**Steps:**
1. Chọn **"Tạo lại"**.
2. Quan sát quá trình tạo ảnh AI đang chạy.
3. Reload hoặc rời khỏi màn hình.
4. Quay lại màn hình liên quan.
5. Quan sát trạng thái.
6. Chờ quá trình tạo ảnh AI hoàn tất.

**Expected Result:**
- Quá trình tạo ảnh AI tiếp tục chạy sau reload/rời trang.
- Khách hàng không thấy quá trình tạo ảnh AI mới bắt đầu do reload.
- Hệ thống hiển thị trạng thái **"Đang tạo"**.
- Lượt sử dụng đã được ghi nhận từ lúc thao tác được chấp nhận.
- Chưa tạo mục lịch sử khi chưa có ảnh output hợp lệ.
- Khi tạo ảnh thành công, hệ thống tạo mục lịch sử theo luồng chính.

**Trace to:**
- [STORY-036/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338) · [STORY-036/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)

**Rationale:**
> Xác minh trải nghiệm khách hàng: quá trình tạo ảnh AI Tạo lại tiếp tục được hệ thống xử lý khi khách hàng reload hoặc rời khỏi màn hình và không bị khởi tạo thêm quá trình xử lý.

---

## ST-036-05-01 — Hết lượt sử dụng trong ngày hoặc hết lượt theo mẫu hoa

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-036-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aae6935a-e090-41a4-9a29-708872e598b1) |
| **Story** | STORY-036 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Thiệp nguồn hợp lệ và thuộc khách hàng.
- Template và Size còn khả dụng.

**Steps:**
1. Thực hiện **"Tạo lại"** với Tình huống A.
2. Quan sát phản hồi.
3. Chuyển sang Tình huống B.
4. Thực hiện **"Tạo lại"**.
5. Quan sát phản hồi.

**Test Data:**

| Scenario | Lượt trong ngày | Lượt mẫu hoa |
|---|---|---|
| A — Hết lượt ngày | 10/10 | 1/3 |
| B — Hết lượt mẫu hoa | 5/10 | 3/3 |

**Expected Result — Tình huống A:**
- Khách hàng không thấy quá trình tạo ảnh AI mới bắt đầu.
- Không tạo mục lịch sử.
- Số lượt sử dụng hiển thị không tăng thêm.
- Hiển thị: **"Bạn đã sử dụng hết 10 lượt tạo thiệp AI trong ngày."**

**Expected Result — Tình huống B:**
- Khách hàng không thấy quá trình tạo ảnh AI mới bắt đầu.
- Số lượt sử dụng hiển thị không tăng thêm.
- Không tạo mục lịch sử.
- Hiển thị: **"Bạn đã sử dụng hết số lượt tạo thiệp cho mẫu hoa này."**

**Trace to:**
- [STORY-036/BR-048](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)
- [STORY-036/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338) · [STORY-036/EXC-09](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)

**Rationale:**
> Xác minh trải nghiệm khách hàng: thao tác Tạo lại phải đồng thời thỏa lượt sử dụng tối đa 10 lượt/ngày và tối đa 3 lượt tạo ảnh theo mẫu hoa nguồn.

---

## ST-036-06-01 — Template hoặc Size không còn khả dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-036-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/44f1c65b-9f24-4363-9375-8b5e6bce78e3) |
| **Story** | STORY-036 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Thiệp nguồn thuộc khách hàng.
- Template hoặc Size khả dụng tại thời điểm tạo thiệp nguồn.
- Có khả năng làm Template hoặc Size ngừng khả dụng trước khi Tạo lại.
- Khách hàng còn lượt sử dụng.

**Steps:**
1. Mở thiệp nguồn.
2. Làm Template hoặc Size của thiệp nguồn ngừng khả dụng.
3. Chọn **"Tạo lại"**.
4. Quan sát phản hồi.
5. Quan sát quá trình tạo ảnh AI, lịch sử và lượt sử dụng.

**Expected Result:**
- Phát hiện dữ liệu không còn khả dụng.
- Khách hàng không thấy quá trình tạo ảnh AI mới bắt đầu.
- Không có ảnh AI mới được tạo.
- Không tạo mục lịch sử.
- Không ghi nhận lượt sử dụng trong ngày.
- Không ghi nhận lượt theo mẫu hoa.
- Hệ thống thông báo Template hoặc Size của thiệp nguồn không còn khả dụng để Tạo lại.

**Trace to:**
- [STORY-036/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338) · [STORY-036/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)

**Rationale:**
> Xác minh trải nghiệm khách hàng: Hệ thống kiểm tra lại trạng thái Template và Size của thiệp nguồn trước khi tạo quá trình tạo ảnh AI mới.

---

## ST-036-07-01 — Tạo ảnh AI thất bại: hoàn lượt sử dụng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-036-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cadd24b2-ccd0-4d12-9887-7348e1c95a49) |
| **Story** | STORY-036 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Thao tác Tạo lại hợp lệ.
- Khách hàng còn cả hai loại lượt.
- Môi trường test có dữ liệu để quan sát AI hoặc hệ thống không tạo được ảnh output hợp lệ sau toàn bộ thử lại.

**Steps:**
1. Chọn **"Tạo lại"**.
2. Quan sát thao tác được chấp nhận.
3. Quan sát hai lượt đã được ghi nhận.
4. Thực hiện tình huống quá trình tạo ảnh thất bại.
5. Chờ hệ thống thử lại cho đến khi có kết quả cuối cùng.
6. Chờ quá trình xử lý kết thúc thất bại.
7. Quan sát lịch sử và lượt sử dụng.

**Test Data:**
- Lượt sử dụng trong ngày trước: 4/10.
- Lượt mẫu hoa trước: 1/3.

**Expected Result:**
- Hệ thống thực hiện thử lại theo chính sách kỹ thuật.
- Số lượt sử dụng hiển thị không tăng thêm cho thử lại.
- Không tạo mục lịch sử nếu không có ảnh hợp lệ.
- Hoàn đúng **1 lượt** sử dụng trong ngày.
- Hoàn đúng **1 lượt** tạo ảnh của mẫu hoa.
- Mỗi loại lượt chỉ được hoàn tối đa một lần cho quá trình tạo ảnh AI.
- Thiệp nguồn vẫn được giữ nguyên.
- Hệ thống thông báo khách hàng thử lại sau.

**Trace to:**
- [STORY-036/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338) · [STORY-036/EXC-05](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)

**Rationale:**
> Xác minh trải nghiệm khách hàng: thao tác Tạo lại thất bại sau toàn bộ thử lại không tạo lịch sử và hoàn đúng các lượt đã ghi nhận cho quá trình tạo ảnh AI đó.

---

## ST-036-08-01 — Chống thao tác trùng (double submit)

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-036-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/05d85d43-3095-4ee0-bf8b-42260fae1347) |
| **Story** | STORY-036 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Thiệp nguồn hợp lệ.
- Khách hàng còn lượt sử dụng.
- Có khả năng gửi lại cùng thao tác nhiều lần.

**Steps:**
1. Thực hiện thao tác **"Tạo lại"**.
2. Thực hiện lại cùng thao tác nhiều lần liên tiếp.
3. Chờ tạo ảnh hoàn tất.
4. Quan sát số quá trình tạo ảnh AI.
5. Quan sát số ảnh mới.
6. Quan sát lịch sử.
7. Quan sát lượt sử dụng trong ngày và lượt mẫu hoa.

**Expected Result:**
- Hệ thống nhận diện thao tác bị trùng.
- Chỉ có tối đa **1 lần** gọi AI.
- Chỉ tạo **1 ảnh** kết quả.
- Lịch sử chỉ hiển thị **một mục mới** cho thao tác này.
- Chỉ ghi nhận **1 lượt** sử dụng trong ngày.
- Chỉ ghi nhận **1 lượt** theo mẫu hoa.
- Hệ thống trả kết quả của thao tác hợp lệ đã được xử lý.

**Trace to:**
- [STORY-036/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338) · [STORY-036/EXC-06](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)

**Rationale:**
> Xác minh trải nghiệm khách hàng: cơ chế chống thao tác trùng bảo đảm cùng một thao tác Tạo lại không phát sinh nhiều quá trình tạo ảnh AI, ảnh, mục lịch sử hoặc lượt sử dụng.

---

## ST-036-09-01 — Từ chối nguồn không hợp lệ hoặc không có quyền

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-036-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/fdc1f3d7-5002-4df1-99dd-a40e7625ef75) |
| **Story** | STORY-036 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có khách hàng A và khách hàng B.
- Có thiệp B thuộc khách hàng B.
- Có Checkout của khách hàng B.
- Có một Checkout đã hoàn tất.
- Khách hàng A đang đăng nhập.

**Steps — Tình huống A (Thiệp nguồn không thuộc khách hàng):**
1. Với khách hàng A, thực hiện thao tác Tạo lại từ thiệp B.
2. Quan sát phản hồi.

**Steps — Tình huống B (Checkout không hợp lệ):**
1. Thực hiện Tạo lại trong Checkout của khách hàng B, Checkout không tồn tại hoặc Checkout đã hoàn tất.
2. Quan sát phản hồi.

**Expected Result — Tình huống A:**
- Khách hàng thấy thao tác bị từ chối.
- Không trả ảnh thiệp nguồn.
- Không trả metadata nhạy cảm.
- Khách hàng không thấy quá trình tạo ảnh AI bắt đầu.
- Không xuất hiện mục mới trong lịch sử.
- Số lượt sử dụng còn lại không bị giảm.

**Expected Result — Tình huống B:**
- Hệ thống từ chối thao tác Tạo lại.
- Không bắt đầu AI.
- Không xuất hiện mục mới trong lịch sử.
- Số lượt sử dụng còn lại không bị giảm.
- Không làm lộ dữ liệu Checkout hoặc khách hàng khác.
- Hiển thị lỗi phù hợp với nguyên nhân Checkout không hợp lệ.

**Trace to:**
- [STORY-036/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)
- [STORY-036/EXC-07](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338) · [STORY-036/EXC-08](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a9328fef-ec31-4e66-beb6-569d5383a338)
- [STORY-035/EXC-08](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/54a960a5-99fa-4037-b1d8-7e931f2b7e43)

**Rationale:**
> Xác minh trải nghiệm khách hàng: Hệ thống bảo vệ quyền sở hữu của thiệp nguồn và Checkout, đồng thời không làm lộ ảnh hoặc metadata của khách hàng khác.
