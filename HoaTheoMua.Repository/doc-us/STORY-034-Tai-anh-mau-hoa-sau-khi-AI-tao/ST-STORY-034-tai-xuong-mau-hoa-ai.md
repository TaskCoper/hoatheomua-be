# ST — STORY-034 — Tải xuống mẫu hoa AI — System Tests

---

## ST-034-01-01 — Smoke: Tải xuống kết quả mẫu hoa AI thành công

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-034-01-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/978520a4-ab2f-4bf6-ba06-00f0aaa8d5d4) |
| **Story** | STORY-034 |
| **Loại** | 1 |
| **Suite** | SMOKE |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Yêu cầu tạo mẫu hoa tồn tại và thuộc khách hàng hiện tại.
- Yêu cầu đã có ít nhất một kết quả AI được tạo thành công.
- File ảnh kết quả còn tồn tại trong Core Storage.
- Kết quả AI đang ở trạng thái sẵn sàng để tải.
- Lượt sử dụng AI hiện tại của khách hàng có thể kiểm tra được.

**Steps:**
1. Đăng nhập bằng khách hàng A.
2. Mở màn hình Kết quả bó hoa AI của `RESULT-A`.
3. Quan sát chức năng tải xuống.
4. Ghi nhận lượt sử dụng AI hiện tại.
5. Chọn **"Tải xuống"**.
6. Chờ quá trình tải hoàn tất.
7. Quan sát file nhận được trên thiết bị.
8. Quan sát định dạng, tên file, chất lượng và logo của ảnh.
9. Quan sát lại lượt sử dụng AI.

**Test Data:**
- Khách hàng: `Khách hàng A`.
- Yêu cầu: `REQ-A`.
- Kết quả AI: `RESULT-A`.
- File ảnh chính thức: đã được gắn logo.
- Lượt sử dụng trước tải: `2`.

**Expected Result:**
- Hệ thống hiển thị nút **"Tải xuống"**.
- Hệ thống chỉ tải đúng 1 ảnh tương ứng với `RESULT-A`.
- File tải xuống có định dạng `.png`.
- Ảnh giữ nguyên resolution và chất lượng của ảnh kết quả đã lưu.
- Ảnh tải xuống là file chính thức đã được gắn logo.
- Chức năng tải xuống không gắn lại hoặc thay đổi logo.
- Tên file tuân theo định dạng `mauhoa_[ma-ket-qua]_[yyyyMMdd_HHmmss].png`.
- Tên file không chứa ký tự không an toàn.
- Lượt sử dụng AI của khách hàng không thay đổi sau thao tác tải xuống.

**Trace to:**
- [STORY-034/AC-001](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)
- [STORY-034/AC-005](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)
- [STORY-034/AC-006](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)
- [STORY-034/AC-007](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)
- [STORY-034/AC-008](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)
- [STORY-034/AC-009](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)
- [STORY-034/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)

**Rationale:**
> Xác minh trải nghiệm khách hàng: khách hàng có thể tải xuống thành công đúng một ảnh AI đã tạo, với đúng định dạng PNG, giữ nguyên chất lượng, sử dụng file chính thức đã gắn logo, đúng quy tắc tên file và không làm giảm lượt sử dụng AI.

---

## ST-034-02-01 — Tải xuống được cả kết quả mới nhất và kết quả cũ

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-034-02-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/bb8c5cde-aa98-4009-a026-55bd84f42847) |
| **Story** | STORY-034 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Yêu cầu tạo mẫu hoa thuộc khách hàng hiện tại.
- Yêu cầu có ít nhất 2 kết quả AI thành công:
  - `RESULT-A` là kết quả cũ.
  - `RESULT-B` là kết quả mới nhất.
- File ảnh của cả hai kết quả vẫn còn khả dụng.

**Steps:**
1. Mở Lịch sử tạo mẫu hoa.
2. Chọn `RESULT-A`.
3. Quan sát màn hình Kết quả bó hoa AI tương ứng.
4. Chọn **"Tải xuống"**.
5. Quan sát file tải xuống.
6. Quay lại lịch sử.
7. Mở `RESULT-B`.
8. Chọn **"Tải xuống"**.
9. Quan sát file tải xuống.

**Test Data:**
- `RESULT-A`: kết quả cũ.
- `RESULT-B`: kết quả mới nhất.

**Expected Result:**
- Khi mở `RESULT-A`, hệ thống hiển thị nút **"Tải xuống"**.
- Khách hàng tải được đúng ảnh của `RESULT-A`.
- Khi mở `RESULT-B`, hệ thống cũng hiển thị nút **"Tải xuống"**.
- Khách hàng tải được đúng ảnh của `RESULT-B`.
- Việc một kết quả không còn là kết quả mới nhất không làm mất quyền tải xuống.

**Trace to:**
- [STORY-034/AC-002](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)
- [STORY-034/AC-004](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)

**Rationale:**
> Xác minh trải nghiệm khách hàng: khách hàng có thể tải xuống cả kết quả AI mới nhất và các kết quả AI cũ đã được tạo thành công thông qua Lịch sử tạo.

---

## ST-034-03-01 — Tải lại cùng một kết quả nhiều lần không trừ lượt AI

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-034-03-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/dfe9add0-9a8e-4053-9498-cf424a1d3439) |
| **Story** | STORY-034 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Có một kết quả AI thành công thuộc khách hàng.
- File ảnh còn khả dụng.
- Khách hàng đã tải thành công kết quả này ít nhất một lần.

**Steps:**
1. Mở `RESULT-A`.
2. Ghi nhận lượt sử dụng AI hiện tại.
3. Chọn **"Tải xuống"**.
4. Chờ tải hoàn tất.
5. Chọn **"Tải xuống"** lần thứ hai.
6. Chờ tải hoàn tất.
7. Thực hiện thêm một lần tải nếu cần.
8. Quan sát lượt sử dụng AI sau các lần tải.

**Test Data:**
- Kết quả: `RESULT-A`.
- Lượt sử dụng trước thao tác: `2`.

**Expected Result:**
- Mỗi thao tác tải xuống được xử lý độc lập.
- Hệ thống cho phép tải lại cùng `RESULT-A` nhiều lần.
- Không áp dụng giới hạn số lần tải.
- Mỗi lần thao tác chỉ tải đúng một ảnh.
- Lượt sử dụng AI không bị trừ sau bất kỳ lần tải nào.

**Trace to:**
- [STORY-034/AC-010](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)
- [STORY-034/AC-011](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)

**Rationale:**
> Xác minh trải nghiệm khách hàng: khách hàng có thể tải cùng một kết quả AI nhiều lần mà không bị giới hạn và mỗi lần tải không ảnh hưởng đến lượt sử dụng AI.

---

## ST-034-04-01 — Trạng thái Combo hoặc Mockup hiện tại không chặn tải kết quả đã lưu

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-034-04-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/9ada2cb0-5581-4fe2-a379-b645778b64af) |
| **Story** | STORY-034 |
| **Loại** | 2 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đang duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Có một kết quả AI cũ đã được tạo thành công.
- File ảnh của kết quả vẫn còn trong Core Storage.
- Có khả năng thay đổi trạng thái Combo và Mockup liên quan.

**Steps:**
1. **Tình huống A — Combo không còn khả dụng:** Chuyển Combo A sang trạng thái không còn khả dụng.
2. Mở `RESULT-A`.
3. Chọn **"Tải xuống"**.
4. Quan sát kết quả.
5. **Tình huống B — Mockup không còn khả dụng:** Khôi phục trạng thái phù hợp để chạy scenario độc lập.
6. Disable hoặc xóa Mockup A.
7. Mở `RESULT-A`.
8. Chọn **"Tải xuống"**.
9. Quan sát kết quả.

**Test Data:**
- Kết quả: `RESULT-A`.
- Combo nguồn: `Combo A`.
- Mockup: `Mockup A`.

**Expected Result:**
- **Tình huống A:** Việc Combo A không còn khả dụng không chặn tải xuống.
- **Tình huống A:** Hệ thống vẫn trả file ảnh đã lưu của `RESULT-A`.
- **Tình huống B:** Việc Mockup A không còn khả dụng không chặn tải xuống.
- **Tình huống B:** Hệ thống vẫn trả file ảnh đã lưu của `RESULT-A`.

**Trace to:**
- [STORY-034/AC-012](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)
- [STORY-034/AC-013](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)

**Rationale:**
> Xác minh trải nghiệm khách hàng: khả năng tải xuống một kết quả AI đã lưu không phụ thuộc vào trạng thái hiện tại của Combo hoặc Mockup nguồn.

---

## ST-034-05-01 — Không cho tải khi ảnh AI chưa sẵn sàng

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-034-05-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/05b92280-403f-48b1-9679-17f37f1ac403) |
| **Story** | STORY-034 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đang duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Có một quá trình tạo ảnh AI thuộc khách hàng đang ở trạng thái **"Đang tạo"** hoặc ảnh chưa sẵn sàng.

**Steps:**
1. Mở màn hình kết quả trong khi quá trình xử lý-A đang xử lý.
2. Quan sát nút **"Tải xuống"**.
3. Quan sát thông báo trên màn hình.
4. Thử thao tác tải nếu UI cho phép.

**Test Data:**
- Quá trình tạo ảnh AI: `quá trình xử lý-A`.
- Trạng thái: **"Đang tạo"**.

**Expected Result:**
- Nút **"Tải xuống"** ở trạng thái disabled.
- Hệ thống hiển thị thông báo: **"Ảnh đang được tạo. Vui lòng đợi hoàn tất để tải xuống."**
- Hệ thống không bắt đầu quá trình tải file.
- Không trả file ảnh khi ảnh chưa sẵn sàng.

**Trace to:**
- [STORY-034/AC-014](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)

**Rationale:**
> Xác minh trải nghiệm khách hàng: khách hàng không thể tải ảnh khi quá trình tạo ảnh AI vẫn đang tạo hoặc file kết quả chưa được lưu hoàn tất.

---

## ST-034-06-01 — Không cho tải kết quả thuộc khách hàng khác

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-034-06-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/5fc839f6-57db-497e-94eb-8295c107d5c0) |
| **Story** | STORY-034 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Có khách hàng A và khách hàng B.
- Có `RESULT-B` thuộc yêu cầu của khách hàng B.
- Khách hàng A đã đăng nhập.
- `RESULT-B` đã tạo thành công và file tồn tại.

**Steps:**
1. Đăng nhập bằng khách hàng A.
2. Thực hiện yêu cầu tải xuống `RESULT-B`.
3. Quan sát phản hồi.
4. Quan sát xem file hoặc thông tin lưu trữ có được trả về hay không.

**Test Data:**
- Người dùng hiện tại: `Khách hàng A`.
- Kết quả mục tiêu: `RESULT-B`.
- Chủ sở hữu thực tế: `Khách hàng B`.

**Expected Result:**
- Hệ thống xác định `RESULT-B` không thuộc khách hàng A.
- Hệ thống từ chối thao tác tải xuống.
- Không trả file ảnh.
- Hệ thống hiển thị thông báo: **"Bạn không có quyền tải xuống mẫu hoa này."**
- Không trả đường dẫn lưu trữ hoặc metadata nhạy cảm của file.

**Trace to:**
- [STORY-034/AC-003](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)

**Rationale:**
> Xác minh trải nghiệm khách hàng: chỉ chủ sở hữu của kết quả AI được phép tải ảnh và người dùng khác không thể lấy file dù biết mã kết quả.

---

## ST-034-07-01 — Lỗi file hoặc lỗi chuẩn bị file hiển thị thông báo phù hợp

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-034-07-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/d5f0e0d8-adb0-469f-a9e8-b24704e3a8f9) |
| **Story** | STORY-034 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P1 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đã duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Kết quả AI hợp lệ và thuộc khách hàng.
- Môi trường test có dữ liệu để quan sát:
  - File không tồn tại trong Core Storage.
  - Lỗi trong quá trình chuẩn bị file PNG.

**Steps:**
1. **Tình huống A — Không tìm thấy file:** Mở `RESULT-A`.
2. Chọn **"Tải xuống"**.
3. Quan sát phản hồi.
4. **Tình huống B — Lỗi chuẩn bị file:** Mở `RESULT-B`.
5. Bật tình trạng lỗi chuẩn bị file.
6. Chọn **"Tải xuống"**.
7. Quan sát phản hồi.

**Test Data:**
- Scenario A: `RESULT-A` có bản ghi kết quả nhưng không lấy được file từ Core Storage.
- Scenario B: `RESULT-B` có file tồn tại nhưng xảy ra lỗi khi chuẩn bị file tải xuống.

**Expected Result:**
- **Tình huống A:** Hệ thống không bắt đầu tải xuống.
- **Tình huống A:** Hệ thống hiển thị: **"Không thể tải mẫu hoa. Vui lòng thử lại."**
- **Tình huống B:** Hệ thống không bắt đầu tải xuống khi việc chuẩn bị file thất bại.
- **Tình huống B:** Hệ thống hiển thị cùng thông báo: **"Không thể tải mẫu hoa. Vui lòng thử lại."**
- Hệ thống không hiển thị lỗi kỹ thuật nội bộ cho khách hàng.

**Trace to:**
- [STORY-034/AC-015](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)

**Rationale:**
> Xác minh trải nghiệm khách hàng: hệ thống xử lý thống nhất và thân thiện khi không thể lấy ảnh từ Core Storage hoặc gặp lỗi trong quá trình chuẩn bị file tải xuống.

---

## ST-034-08-01 — Disable nút tải xuống trong lúc đang xử lý tải file

| Trường | Nội dung |
|---|---|
| **Test ID** | [ST-034-08-01](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/b9c53a7f-693c-4950-b052-63f593b6f292) |
| **Story** | STORY-034 |
| **Loại** | 4 |
| **Suite** | FULL |
| **Priority** | P2 |
| **Owner** | Hoàng Thị Khánh Linh |
| **Trạng thái** | Đang duyệt |

**Precondition:**
- Khách hàng đã đăng nhập.
- Có một kết quả AI hợp lệ và sẵn sàng tải.

**Steps:**
1. Mở `RESULT-A`.
2. Chọn **"Tải xuống"**.
3. Ngay sau khi chọn **"Tải xuống"**, quan sát trạng thái nút.
4. Trong khi quá trình tải chưa hoàn tất, thử nhấn lại nút **"Tải xuống"**.
5. Chờ tải xuống thành công.
6. Quan sát lại trạng thái nút **"Tải xuống"**.

**Test Data:**
- Kết quả: `RESULT-A`.

**Expected Result:**
- Ngay sau khi khách hàng chọn **"Tải xuống"**, hệ thống disable nút **"Tải xuống"**.
- Hệ thống hiển thị loading hoặc trạng thái đang xử lý.
- Nút **"Tải xuống"** duy trì trạng thái disabled trong suốt quá trình chuẩn bị và tải file.
- Khách hàng không thể kích hoạt thêm thao tác tải xuống trong khi quá trình hiện tại chưa hoàn tất.
- Chỉ một lượt tải xuống được thực hiện.
- Sau khi tải file thành công, nút **"Tải xuống"** được enable trở lại để khách hàng có thể tiếp tục thao tác nếu cần.

**Trace to:**
- [STORY-034/AC-016](https://document-first.vnzdna.com/projects/117393d8-1afc-4c89-baa9-9aeea430cdcd/documents/aa22c827-d7cf-4ce3-a575-03576eb9d27f)

**Rationale:**
> Xác minh giao diện phản hồi rõ ràng trong lúc chuẩn bị file và ngăn khách hàng vô tình kích hoạt nhiều lượt tải cùng lúc.
