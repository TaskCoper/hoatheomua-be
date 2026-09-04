# ST — STORY-046 — Admin tải xuống thiệp AI — System Tests

---

## ST-046-01-01 — Tải xuống ảnh thiệp AI thành công

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-046-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9380cda5-d822-4768-984f-ae285c9e7e27) |
| **Story** | STORY-046 |
| **Loại** | 1 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập Website quản trị.
- Admin có quyền tải thiệp trong module AI Custom.
- Có một thiệp đã generate thành công và file ảnh còn khả dụng.

**Steps:**
1. Admin mở danh sách hoặc Chi tiết thiệp.
2. Admin chọn **"Tải ảnh thiệp xuống"**.
3. Admin quan sát quá trình tải.
4. Admin mở file vừa tải.
5. Admin quan sát tên file.

**Test Data:**
- Mã thiệp: `CARD-00125`.
- File ảnh: còn khả dụng.
- Định dạng ảnh: `PNG`.

**Expected Result:**
- Hệ thống tải xuống đúng ảnh của `CARD-00125`.
- File tải xuống mở được và hiển thị đúng ảnh thiệp.
- File tải xuống có định dạng `PNG`.
- Tên file sử dụng mã thiệp `CARD-00125`.
- Tên file có dạng `thiep_[ma-thiep]_[yyyyMMdd_HHmmss].png`.
- Thao tác tải xuống không làm thay đổi nội dung thiệp hiện có.

**Trace to:**
- [STORY-046/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cbdafb14-da58-4686-b230-21254a606883)
- [BR-140](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e6b1d971-1140-4a78-9173-e415327dce18)
- [BR-141](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/51379482-5a1a-44c9-bc4f-64970e316a00)

**Rationale:**
> Xác minh Admin có quyền có thể tải xuống đúng file ảnh của thiệp AI đã được tạo thành công.

---

## ST-046-02-01 — Tải ảnh thiệp trực tiếp từ danh sách

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-046-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/570f36e2-2ab2-488d-a9b0-033b4d5d160e) |
| **Story** | STORY-046 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập Website quản trị.
- Admin có quyền tải thiệp trong module AI Custom.
- Danh sách thiệp có ít nhất một thiệp có file ảnh còn khả dụng.

**Steps:**
1. Admin mở AI Custom → Thiệp.
2. Admin tìm thiệp cần tải.
3. Admin mở menu thao tác của thiệp.
4. Admin chọn **"Tải ảnh thiệp xuống"**.
5. Admin quan sát file được tải.

**Test Data:**
- Mã thiệp: `CARD-00130`.

**Expected Result:**
- Thao tác **"Tải ảnh thiệp xuống"** được hiển thị tại item tương ứng.
- Khi Admin chọn tải, hệ thống tải đúng ảnh của `CARD-00130`.
- File tải xuống có định dạng `PNG`.
- Tên file sử dụng mã thiệp `CARD-00130`.
- Admin không cần mở Chi tiết thiệp để thực hiện tải xuống.

**Trace to:**
- [STORY-046/ALT-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cbdafb14-da58-4686-b230-21254a606883)
- [STORY-046/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cbdafb14-da58-4686-b230-21254a606883)
- [BR-140](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e6b1d971-1140-4a78-9173-e415327dce18)
- [BR-141](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/51379482-5a1a-44c9-bc4f-64970e316a00)

**Rationale:**
> Xác minh Admin có thể tải trực tiếp ảnh thiệp từ danh sách.

---

## ST-046-03-01 — Tải ảnh từ Chi tiết thiệp không phụ thuộc Order liên kết

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-046-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/78f28434-f8b9-46ee-8c95-0fdf7ef0f1ae) |
| **Story** | STORY-046 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập Website quản trị.
- Admin có quyền tải thiệp trong module AI Custom.
- Có một thiệp có file ảnh còn khả dụng.
- Thiệp có thể đang liên kết với một hoặc nhiều Order.

**Steps:**
1. Admin mở Chi tiết thiệp.
2. Admin chọn **"Tải ảnh thiệp xuống"**.
3. Admin quan sát file được tải.
4. Admin quan sát tên file.

**Test Data:**
- Mã thiệp: `CARD-00135`.
- Order liên kết: `ORD-00110`.

**Expected Result:**
- Hệ thống tải đúng ảnh của `CARD-00135`.
- File tải xuống có định dạng `PNG`.
- Tên file sử dụng mã thiệp `CARD-00135`.
- Tên file không sử dụng mã Order `ORD-00110`.
- Việc thiệp có liên kết Order không làm thay đổi file được tải.

**Trace to:**
- [STORY-046/ALT-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cbdafb14-da58-4686-b230-21254a606883)
- [STORY-046/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cbdafb14-da58-4686-b230-21254a606883)
- [BR-141](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/51379482-5a1a-44c9-bc4f-64970e316a00)

**Rationale:**
> Xác minh tải ảnh từ Chi tiết thiệp sử dụng chính mã thiệp và không phụ thuộc Order liên kết.

---

## ST-046-04-01 — Tải lại cùng một thiệp nhiều lần không đổi nội dung ảnh

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-046-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/11d4b59b-35af-480f-9e67-785fd841b7d0) |
| **Story** | STORY-046 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập Website quản trị.
- Admin có quyền tải thiệp trong module AI Custom.
- File ảnh thiệp còn khả dụng.

**Steps:**
1. Admin tải ảnh thiệp lần thứ nhất.
2. Admin mở file vừa tải và quan sát nội dung.
3. Admin thực hiện tải lại thiệp lần thứ hai.
4. Admin thực hiện thêm một lần tải.
5. Admin mở các file vừa tải và đối chiếu nội dung.

**Test Data:**
- Mã thiệp: `CARD-00140`.

**Expected Result:**
- Admin có thể tải `CARD-00140` nhiều lần.
- Mỗi lần tải đều cung cấp đúng ảnh của cùng thiệp.
- Nội dung ảnh không bị thay đổi giữa các lần tải.
- Hệ thống không giới hạn số lần tải khi Admin vẫn còn quyền và file còn khả dụng.
- Các lần tải không tạo thêm thiệp mới trong History.

**Trace to:**
- [STORY-046/ALT-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cbdafb14-da58-4686-b230-21254a606883)
- [STORY-046/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cbdafb14-da58-4686-b230-21254a606883)
- [STORY-046/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cbdafb14-da58-4686-b230-21254a606883)
- [BR-140](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e6b1d971-1140-4a78-9173-e415327dce18)
- [BR-143](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ecef43eb-67f5-4c95-9e5c-4867d94cf326)

**Rationale:**
> Xác minh Admin có thể tải lại cùng một thiệp nhiều lần mà không làm thay đổi History hoặc nội dung ảnh.

---

## ST-046-05-01 — Tải xuống không tạo History mới và không đổi quota khách hàng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-046-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9fb013fa-04a3-42db-be5f-125a57885fa5) |
| **Story** | STORY-046 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền tải thiệp AI.
- Có một thiệp có file ảnh còn khả dụng.
- Có thể quan sát số History item của thiệp và số lượt AI còn lại của khách hàng trước và sau khi tải.

**Steps:**
1. Admin ghi nhận số History item hiện tại của thiệp.
2. Admin ghi nhận số lượt AI còn lại của khách hàng.
3. Admin tải ảnh thiệp.
4. Admin tải lại ảnh thêm một lần.
5. Admin kiểm tra lại History của thiệp.
6. Admin kiểm tra lại số lượt AI còn lại của khách hàng.

**Test Data:**
- Mã thiệp: `CARD-00145`.
- Số History item trước khi tải: `1`.
- Quota AI còn lại trước khi tải: `2` lượt.

**Expected Result:**
- File ảnh được tải xuống thành công.
- Sau các lần tải, `CARD-00145` vẫn chỉ có 01 History item.
- Không xuất hiện thiệp hoặc ảnh History mới do thao tác tải xuống.
- Quota AI của khách hàng vẫn còn `2` lượt.
- Tải lại không làm giảm quota AI.

**Trace to:**
- [STORY-046/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cbdafb14-da58-4686-b230-21254a606883)
- [BR-140](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e6b1d971-1140-4a78-9173-e415327dce18)
- [BR-143](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/ecef43eb-67f5-4c95-9e5c-4867d94cf326)

**Rationale:**
> Xác minh tải xuống chỉ sử dụng ảnh đã có và không ảnh hưởng đến History hoặc quota AI của khách hàng.

---

## ST-046-06-01 — Chặn tài khoản không có quyền tải thiệp AI

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-046-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/380e6cd2-3e29-414f-adc7-db8f6669c0c9) |
| **Story** | STORY-046 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có một tài khoản đã đăng nhập Website quản trị nhưng không có quyền tải thiệp AI.
- Có một thiệp AI tồn tại và file ảnh còn khả dụng.

**Steps:**
1. Đăng nhập bằng tài khoản không có quyền tải thiệp.
2. Mở danh sách hoặc Chi tiết thiệp.
3. Thử thực hiện thao tác tải ảnh thiệp nếu thao tác có thể truy cập.
4. Quan sát kết quả.

**Test Data:**
- Tài khoản: `staff_no_download_permission`.
- Mã thiệp: `CARD-00150`.

**Expected Result:**
- Người dùng không thể tải file ảnh của `CARD-00150`.
- Hệ thống không cung cấp file tải xuống.
- Hệ thống hiển thị trạng thái không có quyền tải hoặc không hiển thị thao tác tải phù hợp với thiết kế.
- Không có file thiệp được tải xuống thiết bị.

**Trace to:**
- [STORY-046/EXC-02](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cbdafb14-da58-4686-b230-21254a606883)
- [STORY-046/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cbdafb14-da58-4686-b230-21254a606883)
- [BR-142](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/37e14851-8725-4cc1-b578-e095dec06a4a)

**Rationale:**
> Xác minh chỉ Admin có quyền tải thiệp AI mới có thể thực hiện chức năng tải xuống.

---

## ST-046-07-01 — Thiệp không tồn tại hoặc không thể truy cập thì không tải file

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-046-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2448d787-1619-4a86-ba57-9c785006ee6d) |
| **Story** | STORY-046 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền tải thiệp AI.
- Có thể truy cập một thiệp bằng mã hoặc đường dẫn trực tiếp.

**Steps:**
1. Admin truy cập một mã thiệp không tồn tại hoặc không còn có thể truy cập.
2. Admin thực hiện thao tác tải xuống nếu thao tác xuất hiện.
3. Admin quan sát kết quả.

**Test Data:**
- Mã thiệp không tồn tại: `CARD-99999`.

**Expected Result:**
- Hệ thống không tải xuống file cho `CARD-99999`.
- Hệ thống hiển thị trạng thái tài nguyên không tồn tại hoặc không thể truy cập.
- Không có file ảnh được tải xuống.
- Hệ thống không hiển thị một file của thiệp khác thay thế.

**Trace to:**
- [STORY-046/EXC-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cbdafb14-da58-4686-b230-21254a606883)

**Rationale:**
> Xác minh hệ thống không cung cấp file khi thiệp được yêu cầu không tồn tại hoặc không thể truy cập.

---

## ST-046-08-01 — Lỗi tạm thời khi tải file thì cho thử lại

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-046-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b8d48c31-6d48-497b-b24a-13f6c7349bae) |
| **Story** | STORY-046 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền tải thiệp AI.
- Thiệp `CARD-00125` có History record và file PNG còn khả dụng.
- Lần tải đầu tiên được cấu hình gặp lỗi tạm thời khi cung cấp file.
- Sau đó lỗi được khắc phục.

**Steps:**
1. Admin mở Chi tiết thiệp `CARD-00125`.
2. Admin chọn **"Tải ảnh thiệp xuống"**.
3. Admin quan sát kết quả khi tải thất bại.
4. Khắc phục lỗi tạm thời.
5. Admin chọn **"Thử lại"** hoặc tải lại.
6. Admin kiểm tra file được tải về.

**Test Data:**
- Mã thiệp: `CARD-00125`.
- File PNG chính thức: còn khả dụng.
- Quota trước test: ghi nhận để đối chiếu.

**Expected Result:**
- Lần tải đầu tiên không trả file thành công.
- Hệ thống hiển thị: **"Tải thiệp thất bại. Vui lòng thử lại."**
- Hệ thống không trả file thiếu hoặc hỏng như một kết quả thành công.
- Hệ thống không gọi AI.
- Hệ thống không tạo file ảnh mới.
- Hệ thống không tạo History record mới.
- Hệ thống không thay đổi quota.
- Admin có thể thử lại.
- Khi thử lại, hệ thống kiểm tra lại quyền, History record và trạng thái file.
- Sau khi lỗi được khắc phục, hệ thống tải thành công đúng file PNG chính thức của thiệp.
- Tên file có dạng `thiep_CARD-00125_[yyyyMMdd_HHmmss].png`.

**Trace to:**
- [STORY-046/EXC-03](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cbdafb14-da58-4686-b230-21254a606883)

**Rationale:**
> Xác minh Admin nhận được phản hồi phù hợp khi tải file gặp lỗi và có thể thử lại sau khi tình trạng được khắc phục.

---

## ST-046-09-01 — File ảnh không còn khả dụng thì không tải và vẫn giữ History

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-046-09-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/2326797b-290a-432a-8795-094a7ea4645c) |
| **Story** | STORY-046 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền tải thiệp AI.
- History record của thiệp vẫn tồn tại nhưng file ảnh không còn khả dụng.

**Steps:**
1. Admin mở Chi tiết thiệp.
2. Admin quan sát trạng thái file.
3. Admin thực hiện hoặc quan sát thao tác **"Tải ảnh thiệp xuống"**.
4. Admin quan sát thông tin của thiệp sau khi file không còn khả dụng.

**Test Data:**
- Mã thiệp: `CARD-00160`.
- Trạng thái file: `Không khả dụng`.

**Expected Result:**
- History record `CARD-00160` vẫn tồn tại.
- Các metadata còn lại của thiệp vẫn được hiển thị.
- Hệ thống hiển thị **"Ảnh không còn khả dụng."**
- Hệ thống không cung cấp file tải xuống.
- Hệ thống không xuất hiện ảnh mới thay thế file đã mất.
- History record không bị xóa do file không còn khả dụng.

**Trace to:**
- [STORY-046/EXC-04](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cbdafb14-da58-4686-b230-21254a606883)
- [BR-140](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/e6b1d971-1140-4a78-9173-e415327dce18)

**Rationale:**
> Xác minh việc file ảnh mất hoặc hỏng không làm mất History record và Admin không thể tải một file không còn khả dụng.

---

## ST-046-10-01 — Tên file tải xuống xử lý ký tự không phù hợp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-046-10-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9d31a858-c425-4ab7-bb2c-85d761cc4ae1) |
| **Story** | STORY-046 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Admin đã đăng nhập và có quyền tải thiệp AI.
- Có thiệp có mã chứa ký tự cần được xử lý khi sử dụng trong tên file tải xuống.

**Steps:**
1. Admin mở thiệp cần kiểm tra.
2. Admin chọn **"Tải ảnh thiệp xuống"**.
3. Admin quan sát tên file được tải.
4. Admin mở file vừa tải.

**Test Data:**
- Mã thiệp dùng để kiểm tra: `CARD/001:TEST`.
- File ảnh: còn khả dụng.

**Expected Result:**
- File được tải xuống thành công.
- Các ký tự không phù hợp trong mã thiệp được loại bỏ hoặc thay thế trong tên file.
- Tên file không chứa ký tự làm cho file không thể lưu hoặc mở bình thường.
- Tên file kết thúc bằng `.png`.
- File PNG mở được bình thường sau khi tải.

**Trace to:**
- [STORY-046/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/cbdafb14-da58-4686-b230-21254a606883)
- [BR-141](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/51379482-5a1a-44c9-bc4f-64970e316a00)

**Rationale:**
> Xác minh tên file tải xuống được tạo an toàn ngay cả khi mã thiệp chứa ký tự không phù hợp với tên file.
