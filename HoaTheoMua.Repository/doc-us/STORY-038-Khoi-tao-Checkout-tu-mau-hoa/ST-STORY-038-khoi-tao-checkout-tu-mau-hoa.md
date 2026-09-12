# ST — STORY-038 — Khách hàng khởi tạo Checkout từ mẫu hoa — System Tests

---

## ST-038-01-02 — Smoke: Mở Checkout từ mẫu hoa hợp lệ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-038-01-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/c201c979-0eba-425c-8a49-94a4f8c267f2) |
| **Story** | STORY-038 |
| **Loại** | 1 |
| **Suite** | SMOKE |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Mẫu hoa hợp lệ.
- Combo nguồn còn khả dụng.
- Core còn đủ hàng.

**Steps:**
1. Mở mẫu hoa A.
2. Chọn **"Đặt hàng ngay"**.
3. Quan sát Checkout.
4. Quan sát mẫu hoa nguồn và trạng thái đơn hàng.

**Test Data:**
- Khách hàng: khách hàng A.
- Mẫu hoa: mẫu hoa A.
- Combo: Combo A.

**Expected Result:**
- Khách hàng được đưa vào màn hình Checkout.
- Checkout tham chiếu đúng 1 mẫu hoa A.
- Step 1 được hiển thị.
- Chưa tạo đơn hàng.
- Chưa có trạng thái chờ thanh toán.

**Trace to:**
- [STORY-038/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4) · [STORY-038/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)
- [BR-081](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4) · [BR-082](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)

**Rationale:**
> Xác minh trải nghiệm khách hàng: khách hàng mở được Checkout tạm từ một mẫu hoa hợp lệ và Checkout tham chiếu đúng một mẫu hoa nguồn.

---

## ST-038-02-01 — Validation thông tin giao hàng bắt buộc

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-038-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/1b47682d-c9ff-4565-a372-6d079af25309) |
| **Story** | STORY-038 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đang ở bước nhập thông tin giao hàng trong Checkout.

**Steps:**
1. Để trống một trường bắt buộc.
2. Chọn tiếp tục hoặc đặt hàng.
3. Quan sát thông báo trên form.
4. Nhập lại dữ liệu hợp lệ cho trường đó.
5. Lặp lại với các trường bắt buộc còn lại.

**Test Data — Kiểm tra lần lượt thiếu:**

| # | Trường bắt buộc thiếu |
|---|---|
| 1 | Số điện thoại người nhận |
| 2 | Tỉnh/Thành phố |
| 3 | Phường/Xã |
| 4 | Địa chỉ chi tiết |

**Expected Result:**
- Khách hàng không thể chuyển sang bước tiếp theo khi còn thiếu thông tin bắt buộc.
- Thông báo lỗi hiển thị ngay tại trường cần bổ sung.
- Các thông tin hợp lệ đã nhập trước đó vẫn được giữ lại.
- Sau khi khách hàng bổ sung dữ liệu hợp lệ, hệ thống cho phép tiếp tục Checkout.

**Trace to:**
- [STORY-038/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4) · [STORY-038/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)

**Rationale:**
> Xác minh khách hàng được hướng dẫn rõ ràng khi thiếu thông tin giao hàng bắt buộc và có thể sửa để tiếp tục Checkout.

---

## ST-038-03-01 — Hoàn tất Checkout không có thiệp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-038-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/4b1bc228-7ade-44b6-a16a-2f879a39c4a4) |
| **Story** | STORY-038 |
| **Loại** | ALT |
| **Suite** | REGRESSION |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Checkout hợp lệ. Thông tin giao hàng hợp lệ. Không có thiệp được chọn.

**Steps:**
1. Hoàn thiện thông tin Checkout.
2. Không chọn thiệp.
3. Thực hiện Hoàn tất.

**Test Data:**
- —

**Expected Result:**
- Hệ thống cho phép tiếp tục dù không có thiệp. Checkout không chứa thiệp. Bill không có giá thiệp. Việc không chọn thiệp không tạo lỗi validation.

**Trace to:**
- [STORY-038/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4) · [BR-085](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)

**Rationale:**
> Xác minh trải nghiệm khách hàng: thiệp là tùy chọn và không có thiệp không làm chặn Checkout hợp lệ.

---

## ST-038-04-01 — Tồn kho Core và Support: phân biệt xử lý khi hết hàng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-038-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/a4c726cc-9933-4460-9b6e-1a7ddbffe7ec) |
| **Story** | STORY-038 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Mẫu hoa hợp lệ và thuộc phạm vi được phép sử dụng.
- Combo nguồn tồn tại.
- Có khả năng thay đổi trạng thái tồn kho Core và Support trong môi trường test.
- Hệ thống kiểm tra được tồn kho hiện tại của Combo.

**Steps — Tình huống A (Core hết khi mở Checkout):**
1. Thiết lập Core của Combo A ở trạng thái không đủ hàng.
2. Mở mẫu hoa A.
3. Chọn **"Đặt hàng ngay"**.
4. Quan sát phản hồi của hệ thống.
5. Quan sát xem Checkout có được khởi tạo hay không.

**Steps — Tình huống B (Support hết, Core còn):**
1. Thiết lập Core của Combo A còn đủ hàng.
2. Thiết lập ít nhất 1 Support của Combo A không đủ hàng.
3. Mở mẫu hoa A.
4. Chọn **"Đặt hàng ngay"**.
5. Quan sát Checkout.
6. Quan sát thông tin liên quan đến Support cần thay thế.

**Steps — Tình huống C (Core hết khi Hoàn tất):**
1. Thiết lập Core của Combo A còn đủ hàng.
2. Mở Checkout thành công từ mẫu hoa A.
3. Nhập đầy đủ thông tin Checkout hợp lệ.
4. Trước khi chọn "Hoàn tất", thay đổi Core của Combo A sang trạng thái không đủ hàng.
5. Chọn **"Hoàn tất"**.
6. Quan sát phản hồi của hệ thống.
7. Quan sát trạng thái Checkout và đơn hàng.

**Test Data:**

| Scenario | Core | Support |
|---|---|---|
| A — Core hết khi mở | Không đủ hàng | Có thể còn hoặc hết |
| B — Support hết, Core còn | Đủ hàng | Ít nhất 1 không đủ hàng |
| C — Core hết khi Hoàn tất | Đủ hàng khi mở → Không đủ hàng khi Hoàn tất | — |

**Expected Result — Tình huống A:**
- Hệ thống phát hiện Core không đủ hàng.
- Hệ thống không khởi tạo Checkout.
- Không tạo đơn hàng.
- Hiển thị: **"Combo hiện không còn đủ thành phần chính để đặt hàng. Vui lòng chọn mẫu hoa khác."**

**Expected Result — Tình huống B:**
- Hệ thống xác định Core vẫn đủ hàng.
- Checkout vẫn được khởi tạo và khách hàng được tiếp tục thao tác.
- Hệ thống ghi nhận các Support cần thay thế.
- Hệ thống không tự động thay thế Support.
- Việc Support hết hàng không làm chặn Checkout.

**Expected Result — Tình huống C:**
- Hệ thống phát hiện Core không còn đủ hàng.
- Checkout hiện tại vẫn được giữ.
- Hệ thống không tạo đơn hàng.
- Hệ thống không hoàn tất Checkout.
- Hiển thị: **"Combo hiện không còn đủ thành phần chính để đặt hàng. Vui lòng chọn mẫu hoa khác."**

**Trace to:**
- [STORY-038/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4) · [STORY-038/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4) · [STORY-038/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)
- [BR-084](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)

**Rationale:**
> Xác minh trải nghiệm khách hàng: Core hết hàng phải chặn đặt, còn Support hết hàng vẫn cho phép tiếp tục.

---

## ST-038-05-01 — Refresh không hủy Checkout; rời phiên thì hủy

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-038-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/06863cbf-310f-4a31-b5a5-d2d613a475d5) |
| **Story** | STORY-038 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Checkout hợp lệ đã được khởi tạo.
- Checkout chưa được hoàn tất.
- Khách hàng đã nhập một số dữ liệu trong Checkout.
- Chưa tạo đơn hàng hoặc trạng thái chờ thanh toán.
- Nếu Checkout đã phát sinh kết quả AI trước đó thì các kết quả này đã được lưu vào lịch sử.

**Steps — Tình huống A (Refresh Checkout):**
1. Mở Checkout A.
2. Nhập dữ liệu Checkout hợp lệ nhưng chưa chọn "Hoàn tất".
3. Refresh trang.
4. Chờ trang tải lại.
5. Quan sát Checkout và các dữ liệu đã nhập.

**Steps — Tình huống B (Rời khỏi Checkout):**
1. Mở Checkout B.
2. Nhập một số dữ liệu nhưng chưa chọn "Hoàn tất".
3. Rời khỏi Checkout bằng một trong các cách được quy định: đóng tab, điều hướng sang trang khác, đăng xuất hoặc đóng trình duyệt.
4. Mở lại hệ thống và truy cập lại luồng đặt hàng.
5. Quan sát Checkout cũ.
6. Quan sát đơn hàng/chờ thanh toán.
7. Nếu trước đó có kết quả AI, quan sát lịch sử và lượt sử dụng AI.

**Test Data:**

| Scenario | Checkout | Dữ liệu đã nhập |
|---|---|---|
| A — Refresh | Checkout A | SĐT: `0901234567`, Tỉnh/TP: TP.HCM, Phường/Xã: hợp lệ, Địa chỉ: `123 Nguyễn Văn A` |
| B — Rời phiên | Checkout B | Có dữ liệu đã nhập, có thể có kết quả AI đã lưu lịch sử |

**Expected Result — Tình huống A:**
- Refresh không làm hủy Checkout A.
- Checkout hiện tại vẫn tồn tại sau khi trang tải lại.
- Các dữ liệu Checkout đã nhập trước đó vẫn được giữ.
- Không tạo đơn hàng.
- Không tạo trạng thái chờ thanh toán.

**Expected Result — Tình huống B:**
- Checkout B bị hủy khi khách hàng thực sự rời khỏi phiên thao tác.
- Checkout cũ không thể được khôi phục để tiếp tục.
- Dữ liệu đã nhập trong Checkout cũ không còn được giữ.
- Không tạo đơn hàng.
- Không tạo trạng thái chờ thanh toán.
- Các kết quả AI đã tạo ảnh thành công trước đó vẫn được giữ trong lịch sử.
- Lượt sử dụng AI đã sử dụng không được hoàn lại chỉ vì khách hàng rời Checkout.

**Trace to:**
- [STORY-038/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4) · [STORY-038/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)
- [BR-086](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)

**Rationale:**
> Xác minh trải nghiệm khách hàng: Checkout chỉ bị hủy khi khách hàng thực sự rời phiên thao tác, còn refresh không làm mất Checkout.

---

## ST-038-06-01 — Regression: Mẫu Custom AI đã tạo đơn vẫn mở được Checkout mới

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-038-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/87f1aaaa-de8a-4aae-b298-3a7b9c825341) |
| **Story** | STORY-038 |
| **Loại** | 1 |
| **Suite** | REGRESSION |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Mẫu AI-mẫu hoa A thuộc khách hàng.
- Mẫu đã từng được dùng để tạo đơn hàng.
- Combo nguồn vẫn khả dụng.
- Core còn đủ hàng.

**Steps:**
1. Mở lịch sử mẫu hoa.
2. Chọn AI-mẫu hoa A.
3. Chọn **"Đặt hàng ngay"**.
4. Quan sát Checkout mới và lịch sử cũ.

**Test Data:**
- Mẫu AI-mẫu hoa A đã từng tạo đơn hàng.
- Combo nguồn còn khả dụng.

**Expected Result:**
- Checkout mới được khởi tạo.
- Đơn hàng cũ không bị thay đổi.
- Các liên kết lịch sử cũ vẫn được giữ.

**Trace to:**
- [STORY-038/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4) · [BR-083](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)

**Rationale:**
> Xác minh trải nghiệm khách hàng: mẫu Custom AI đã từng tạo đơn hàng vẫn có thể được dùng để mở Checkout mới nếu Combo nguồn vẫn hợp lệ.

---

## ST-038-07-01 — Combo nguồn không còn khả dụng: không mở được Checkout

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-038-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8149847a-0ee3-4eea-869a-e9151899f02f) |
| **Story** | STORY-038 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Mẫu Custom AI thuộc khách hàng.
- Combo nguồn không còn khả dụng.

**Steps:**
1. Mở mẫu Custom AI.
2. Chọn **"Đặt hàng ngay"**.
3. Quan sát phản hồi.

**Test Data:**
- Mẫu Custom AI có Combo nguồn không còn khả dụng.

**Expected Result:**
- Khách hàng không được đưa vào màn hình Checkout.
- Không tạo đơn hàng.
- Không tạo liên kết Mẫu hoa–đơn hàng mới.
- Lịch sử cũ không bị thay đổi.
- Hiển thị thông báo Combo không còn khả dụng.

**Trace to:**
- [STORY-038/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4) · [STORY-038/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)

**Rationale:**
> Xác minh trải nghiệm khách hàng: không thể mở Checkout từ mẫu Custom AI nếu Combo nguồn đã bị xóa, disable hoặc không còn cho đặt hàng.

---

## ST-038-08-01 — Bảo vệ ownership: không thể truy cập Checkout của khách hàng khác

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-038-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/8596c3a7-d16d-4f30-9693-40991c1618eb) |
| **Story** | STORY-038 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có khách hàng A và khách hàng B.
- Có mẫu hoa/Checkout thuộc khách hàng B.
- Khách hàng A đang đăng nhập.

**Steps:**
1. Dùng khách hàng A thực hiện thao tác mở hoặc truy cập Checkout của khách hàng B.
2. Quan sát phản hồi.
3. Thử hoàn tất Checkout không thuộc mình.

**Test Data:**
- Khách hàng A, Khách hàng B, Checkout/Mẫu hoa thuộc Khách hàng B.

**Expected Result:**
- Khách hàng thấy thao tác bị từ chối.
- Khách hàng không thấy Checkout được mở hoặc thay đổi.
- Không tạo đơn hàng.
- Khách hàng không nhìn thấy ảnh, dữ liệu Checkout hoặc thông tin của khách hàng khác.
- Hiển thị: **"Bạn không có quyền thực hiện thao tác này."**

**Trace to:**
- [STORY-038/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5cc87b78-e6bf-4996-8d67-787e0bc34bb4)

**Rationale:**
> Xác minh trải nghiệm khách hàng: Hệ thống bảo vệ ownership của mẫu hoa và Checkout.
